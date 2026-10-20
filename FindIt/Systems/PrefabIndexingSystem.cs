using Colossal.Entities;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Common;
using Colossal.Serialization.Entities;

using FindIt.Domain;
using FindIt.Domain.Enums;
using FindIt.Domain.Interfaces;
using FindIt.Utilities;

using Game;
using Game.Common;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;
using Game.UI.InGame;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;

namespace FindIt.Systems
{
	public partial class PrefabIndexingSystem : GameSystemBase
	{
		private const long MinFrameBudgetMilliseconds = 10;
		// The loading screen waits for texture streaming, which advances once per frame.
		// A larger budget would make it longer.
		private const long LoadingFrameBudgetMilliseconds = 150;

		private PrefabSystem _prefabSystem;
		private ImageSystem _imageSystem;
		private PrefabUISystem _prefabUISystem;
		private FindItUISystem _finditUISystem;
		private PickerUISystem _pickerUISystem;
		private HashSet<string> _blackList;
		private ComponentType? roadBuilderDiscarded;
		private static Dictionary<Entity, ZoneTypeFilter> _zoneTypeCache;
		private readonly Dictionary<PrefabBase, bool> _hasParkingCache = new();
		private readonly List<IPrefabCategoryProcessor> _prefabCategoryProcessors = new();
		private EntityQuery _changedPrefabsQuery;
		private IEnumerator _fullIndex;
		private List<PrefabIndex> _pendingSortKeys = new();
		private Task _sortKeysTask = Task.CompletedTask;

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_imageSystem = World.GetOrCreateSystemManaged<ImageSystem>();
			_prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			_finditUISystem = World.GetOrCreateSystemManaged<FindItUISystem>();
			_pickerUISystem = World.GetOrCreateSystemManaged<PickerUISystem>();

			using var stream = typeof(Mod).Assembly.GetManifestResourceStream("FindIt.Resources.Blacklist.txt");
			using var reader = new StreamReader(stream);

			_blackList = new HashSet<string>(reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

			foreach (var type in typeof(PrefabIndexingSystem).Assembly.GetTypes())
			{
				if (typeof(IPrefabCategoryProcessor).IsAssignableFrom(type) && !type.IsAbstract)
				{
					var constructor = type.GetConstructors()[0];
					var parameters = constructor.GetParameters();
					var objectParams = new object[parameters.Length];

					for (var i = 0; i < parameters.Length; i++)
					{
						if (parameters[i].ParameterType == typeof(EntityManager))
						{
							objectParams[i] = EntityManager;
						}
						else if (parameters[i].ParameterType == typeof(PrefabSystem))
						{
							objectParams[i] = _prefabSystem;
						}
						else if (parameters[i].ParameterType == typeof(ImageSystem))
						{
							objectParams[i] = _imageSystem;
						}
					}

					_prefabCategoryProcessors.Add((IPrefabCategoryProcessor)Activator.CreateInstance(type, objectParams));
				}
			}

			_changedPrefabsQuery = SystemAPI.QueryBuilder()
				.WithAll<PrefabData>()
				.WithAny<Created, Updated, Deleted>()
				.Build();

			Enabled = false;
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			Enabled = false;

			_fullIndex = null;
		}

		protected override void OnGameLoaded(Context serializationContext)
		{
			base.OnGameLoaded(serializationContext);

			if (Mod.IsRoadBuilderEnabled)
			{
				roadBuilderDiscarded ??= new ComponentType(Assembly.Load("RoadBuilder").GetType("RoadBuilder.Domain.Components.DiscardedRoadBuilderPrefab"), ComponentType.AccessMode.ReadOnly);
			}

			if (GameManager.instance.gameMode is GameMode.Game or GameMode.Editor)
			{
				FindItUtil.IsReady = false;

				_fullIndex = RunIndex(true);

				Enabled = true;
			}
		}

		protected override void OnUpdate()
		{
			if (_fullIndex is not null && !_fullIndex.MoveNext())
			{
				_fullIndex = null;
			}

			// Created, Updated, and Deleted last one frame, so this can't wait for the full index.
			if (!_changedPrefabsQuery.IsEmptyIgnoreFilter)
			{
				RunIndex(false).MoveNext();
			}
		}

		private IEnumerator RunIndex(bool full)
		{
			var stopWatch = Stopwatch.StartNew();
			var frameStopWatch = Stopwatch.StartNew();
			var workTime = TimeSpan.Zero;
			var frames = 1;
			var frameBudget = GetFrameBudgetMilliseconds(TimeSpan.Zero);

			if (full || _fullIndex is null)
			{
				_hasParkingCache.Clear();
			}

			if (full)
			{
				FindItUtil.CategorizedPrefabs.Clear();
				_finditUISystem.CancelSearch();
				FindItUtil.ClearCachedSearch();
				_pendingSortKeys.Clear();

				AddAllCategories();

				IndexZones();
			}

			var steps = IndexEntities(full);

			if (full)
			{
				steps = steps
					.Concat(AddNumberToDuplicatePrefabNames())
					.Concat(CleanupBrandPrefabs());
			}

			foreach (var _ in steps)
			{
				if (full && frameStopWatch.ElapsedMilliseconds >= frameBudget)
				{
					var slice = frameStopWatch.Elapsed;

					workTime += slice;

					ComputeSortKeysInBackground();

					yield return null;

					frames++;
					frameBudget = GetFrameBudgetMilliseconds(slice);
					frameStopWatch.Restart();
				}
			}

			ComputeSortKeysInBackground();

			if (full)
			{
				FillPdxModsData();

				while (!_sortKeysTask.IsCompleted)
				{
					workTime += frameStopWatch.Elapsed;

					yield return null;

					frames++;
					frameStopWatch.Restart();
				}
			}

			if (full || _fullIndex is null)
			{
				FindItUtil.IsReady = true;

				if (full)
				{
					try
					{
						_pickerUISystem.RefreshOptions();
						_finditUISystem.RefreshPanel();
					}
					catch (Exception ex)
					{
						Mod.Log.Error(ex, "Failed to refresh the UI after the prefab index");
					}
				}

				_finditUISystem.TriggerSearch();

				SortListsInBackground();
			}

			stopWatch.Stop();

			workTime += frameStopWatch.Elapsed;

			var frameInfo = full
				? $" over {frames} frames ({workTime.TotalSeconds:0.000}s of work)"
				: string.Empty;

			Mod.Log.Info($"{(full ? "Full" : "Partial")} Prefab Indexing completed in {stopWatch.Elapsed.TotalSeconds:0.000}s{frameInfo}");
			Mod.Log.Info($"Indexed Prefabs Count: {FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].Count}");
		}

		private void ComputeSortKeysInBackground()
		{
			if (_pendingSortKeys.Count is 0)
			{
				return;
			}

			var prefabs = _pendingSortKeys;
			var compareInfo = CultureInfo.CurrentCulture.CompareInfo;

			_pendingSortKeys = new();

			// A renamed prefab's new key must land after its old one, so the batches run in order.
			_sortKeysTask = _sortKeysTask.ContinueWith(_ =>
			{
				foreach (var prefab in prefabs)
				{
					SetNameKeys(prefab, compareInfo);
				}
			}, TaskScheduler.Default);
		}

		// A list sorts on its first view, which drops frames on a large playset.
		// Favoriting a prefab can add a list, so the lists are copied on the main thread.
		private static void SortListsInBackground()
		{
			// Opening the panel resets the lists under these sorts.
			if (IndexedPrefabList.IsSortedByUsage)
			{
				return;
			}

			var lists = FindItUtil.CategorizedPrefabs.Values.SelectMany(x => x.Values).ToList();

			Task.Run(() =>
			{
				foreach (var list in lists)
				{
					_ = list.OrderedList;
				}
			});
		}

		private static void SetNameKeys(PrefabIndex prefabIndex, CompareInfo compareInfo)
		{
			prefabIndex.NameSortKey = compareInfo.GetSortKey(prefabIndex.Name);
			prefabIndex.NameLower = prefabIndex.Name.ToLowerInvariant();
			prefabIndex.NameAbbreviation = prefabIndex.Name.GetSearchAbbreviation();
			prefabIndex.PrefabNameLower = prefabIndex.PrefabName.ToLowerInvariant();
			prefabIndex.PrefabNameAbbreviation = prefabIndex.PrefabName.GetSearchAbbreviation();
		}

		private long GetFrameBudgetMilliseconds(TimeSpan lastSlice)
		{
			if (GameManager.instance.isGameLoading)
			{
				return LoadingFrameBudgetMilliseconds;
			}

			var lastFrame = UnityEngine.Time.unscaledDeltaTime * 1000;
			var gameFrame = (long)(lastFrame - lastSlice.TotalMilliseconds);
			var budget = Math.Max(gameFrame, MinFrameBudgetMilliseconds);

			return Math.Min(budget, LoadingFrameBudgetMilliseconds);
		}

		private IEnumerable<object> IndexEntities(bool full)
		{
			var existingMeshes = new HashSet<string>();

			foreach (var processor in _prefabCategoryProcessors)
			{
				if (full)
				{
					Mod.Log.Info($"Indexing prefabs with {processor.GetType().Name}");
				}

				if (!TryGetEntities(processor, full, out var entities))
				{
					continue;
				}

				if (full)
				{
					Mod.Log.Info($"\tTotal Entities Count: {entities.Length}");
				}

				for (var i = 0; i < entities.Length; i++)
				{
					IndexEntity(processor, entities[i], full, existingMeshes);

					yield return null;
				}
			}
		}

		private bool TryGetEntities(
			IPrefabCategoryProcessor processor,
			bool full,
			out Entity[] entities)
		{
			try
			{
				var queries = processor.GetEntityQuery();

				if (Mod.Settings.HideRandomAssets)
				{
					for (var i = 0; i < queries.Length; i++)
					{
						queries[i].None = queries[i].None.Concat(new[] { ComponentType.ReadOnly<PlaceholderObjectData>() }).ToArray();
					}
				}

				var query = GetEntityQuery(queries);

				if (full)
				{
					// Temp allocations only last one frame.
					entities = query.ToEntityArray(Allocator.Temp).ToArray();
				}
				else
				{
					entities = _changedPrefabsQuery.ToEntityArray(Allocator.Temp)
						.Where(query.Matches)
						.ToArray();
				}

				return true;
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Prefab indexing failed for processor {processor.GetType().Name}");

				entities = null;
				return false;
			}
		}

		private void IndexEntity(
			IPrefabCategoryProcessor processor,
			Entity entity,
			bool full,
			HashSet<string> existingMeshes)
		{
			PrefabBase prefab = null;
			string prefabName = null;
			PrefabIndex prefabIndex = null;

			try
			{
				// TryGetPrefab fails for a removed prefab, so this check comes first.
				if (EntityManager.HasComponent<Deleted>(entity) || (roadBuilderDiscarded.HasValue && EntityManager.HasComponent(entity, roadBuilderDiscarded.Value)))
				{
					FindItUtil.RemoveItem(entity);

					return;
				}

				if (!_prefabSystem.TryGetPrefab<PrefabBase>(entity, out prefab)
					|| (prefabName = prefab?.name) is null)
				{
					return;
				}

				if (_blackList.Contains(prefabName))
				{
					return;
				}

				if (full && Mod.Log.isLevelEnabled(Level.Debug))
				{
					Mod.Log.Debug($"\tProcessing: {prefabName}");
#if DEBUG
					Mod.Log.Debug($"\t\t> {prefab.GetType().Name} - {string.Join(", ", EntityManager.GetComponentTypes(entity).Select(x => x.GetManagedType()?.Name ?? string.Empty))}");
#endif
				}

				if (processor.TryCreatePrefabIndex(prefab, entity, out prefabIndex))
				{
					if (!full && EntityManager.HasComponent<Created>(entity) && FindItUtil.Find(prefab, false, out var oldId) && oldId != entity.Index)
					{
						FindItUtil.RemoveItem(oldId);
					}

					if (full
						&& prefab is ObjectGeometryPrefab geometry
						&& geometry.m_Meshes?.FirstOrDefault()?.m_Mesh?.name is string meshName
						&& existingMeshes.Add(meshName))
					{
						prefabIndex.IsUniqueMesh = true;
					}

					if (prefab.TryGet<EditorAssetCategoryOverride>(out var overrides) && (overrides?.m_IncludeCategories?.Any() ?? false))
					{
						if (overrides?.m_ExcludeCategories?.Any(x => x.StartsWith("FindIt")) ?? false)
						{
							return;
						}

						if (overrides?.m_IncludeCategories?.Any() ?? false)
						{
							for (var ind = 0; ind < overrides.m_IncludeCategories.Length; ind++)
							{
								if (overrides.m_IncludeCategories[ind].StartsWith("FindIt/"))
								{
									var split = overrides.m_IncludeCategories[ind].Split('/');

									if (split.Length >= 3 && int.TryParse(split[1], out var categeory) && int.TryParse(split[2], out var subCategeory))
									{
										prefabIndex.Category = (PrefabCategory)categeory;
										prefabIndex.SubCategory = (PrefabSubCategory)subCategeory;
									}

									if (split.Length >= 4 && int.TryParse(split[3], out var pdxModsId))
									{
										prefabIndex.PdxModsId = pdxModsId.ToString();
									}
								}
							}
						}
					}

					if (!full)
					{
						FindItUtil.RemoveFromCategories(entity.Index);
					}

					if (prefab.TryGet<FindItProperties>(out var properties))
					{
						if (properties.m_HideFromFindIt)
						{
							return;
						}

						if (FindItUtil.TryParseSubCategory(properties.m_SubCategory, out var subCategory))
						{
							prefabIndex.SubCategory = subCategory;
							prefabIndex.Category = FindItUtil.GetCategory(subCategory);
						}
					}

					AddPrefab(prefab, prefabName, entity, prefabIndex);
				}
				else
				{
					Mod.Log.Debug($"\t\tSkipped: {prefabName}");
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Prefab indexing failed for prefab '{prefabName ?? entity.ToString()}'" + (string.IsNullOrEmpty(prefabIndex?.PdxModsId) ? "" : $" (Pdx Mods ID: {prefabIndex.PdxModsId})"));
			}
		}

		private void AddPrefab(
			PrefabBase prefab,
			string prefabName,
			Entity entity,
			PrefabIndex prefabIndex)
		{
			prefabIndex.Id = entity.Index;
			prefabIndex.TypeHash = prefab.GetType().GetHashCode();
			prefabIndex.PrefabName = prefabName;
			prefabIndex.Name = GetAssetName(entity, prefabName);
			prefabIndex.IsFavorited = FindItUtil.IsFavorited(prefabName);
			prefabIndex.FallbackThumbnail ??= CategoryIconAttribute.GetAttribute(prefabIndex.SubCategory).Icon;
			prefabIndex.CategoryThumbnail ??= CategoryIconAttribute.GetAttribute(prefabIndex.SubCategory).Icon;
			prefabIndex.Theme ??= prefab.GetComponent<ThemeObject>()?.m_Theme;
			prefabIndex.AssetPacks ??= prefab.GetComponent<AssetPackItem>()?.m_Packs?.Where(x => x is not null).ToArray() ?? new AssetPackPrefab[0];
			prefabIndex.ThemeThumbnail ??= prefabIndex.Theme is null ? null : ImageSystem.GetThumbnail(prefabIndex.Theme);
			prefabIndex.PackThumbnails ??= prefabIndex.AssetPacks.Select(ImageSystem.GetThumbnail).ToArray();
			prefabIndex.Tags = FindItUtil.GetTags(prefabName);
			prefabIndex.CreatorTags = FindItUtil.GetCreatorTags(prefab);
			prefabIndex.UIOrder = prefab.TryGet<UIObject>(out var uIObject) ? uIObject.m_Priority : int.MaxValue;
			prefabIndex.IsVanilla = prefab.isBuiltin || prefab.Has<FindItGenerated>();
			prefabIndex.HasParking = prefabIndex.Category is PrefabCategory.Buildings or PrefabCategory.ServiceBuildings && HasParking(prefab);
			prefabIndex.IsRandom = prefabIndex.SubCategory is not PrefabSubCategory.Networks_Pillars && EntityManager.HasComponent<PlaceholderObjectData>(entity);
			prefabIndex.IsResourceIntensive = CheckIfResourceIntensive(prefab);

			if (prefab.asset?.database == AssetDatabase<ParadoxMods>.instance)
			{
				prefabIndex.PdxModsId = prefab.asset.GetMeta().platformID;
			}

#if DEBUG
			if (prefabIndex.SubCategory != PrefabSubCategory.Props_Branding && !prefabIndex.IsRandom && ImageSystem.GetIcon(prefab) is null or "" && !prefab.Has<ServiceUpgrade>())
			{
				if (uIObject is null
					|| uIObject.m_Group is null
					|| !prefab.isBuiltin
					|| !uIObject.m_Group.isBuiltin)
				{
					Mod.Log.Info("MISSINGICON: " + prefab.name);
				}
			}
#endif

			if (prefabIndex.IsRandom && EntityManager.TryGetBuffer<PlaceholderObjectElement>(entity, true, out var placeholderObjectElements))
			{
				prefabIndex.RandomPrefabs = new int[placeholderObjectElements.Length];
				prefabIndex.RandomPrefabBases = new PrefabBase[placeholderObjectElements.Length];

				for (var i = 0; i < placeholderObjectElements.Length; i++)
				{
					prefabIndex.RandomPrefabs[i] = placeholderObjectElements[i].m_Object.Index;

					if (_prefabSystem.TryGetPrefab<PrefabBase>(placeholderObjectElements[i].m_Object, out var randomPrefab))
					{
						prefabIndex.RandomPrefabBases[i] = randomPrefab;
					}
				}
			}

			if (prefab.TryGet<ContentPrerequisite>(out var contentPrerequisites)
				&& contentPrerequisites.m_ContentPrerequisite.TryGet<DlcRequirement>(out var dlcRequirements))
			{
				prefabIndex.AssetPacks = new AssetPackPrefab[0];
				prefabIndex.PackThumbnails = new string[0];
				prefabIndex.DlcId = dlcRequirements.m_Dlc;
				prefabIndex.DlcThumbnail = $"Media/DLC/{PlatformManager.instance.GetDlcName(dlcRequirements.m_Dlc)}.svg";
			}
			else if (prefabIndex.IsVanilla)
			{
				prefabIndex.DlcId = DlcId.BaseGame;
			}
			else
			{
				prefabIndex.DlcId = DlcId.Invalid;
			}

			if (EntityManager.TryGetComponent<BuildingData>(entity, out var buildingData))
			{
				prefabIndex.LotSize = buildingData.m_LotSize;
			}

			if (_fullIndex is null)
			{
				SetNameKeys(prefabIndex, CultureInfo.CurrentCulture.CompareInfo);
			}
			else
			{
				_pendingSortKeys.Add(prefabIndex);
			}

			if (!Mod.Settings.HideBrandsFromAny || prefabIndex.SubCategory is not PrefabSubCategory.Props_Branding)
			{
				FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any][prefabIndex.Id] = prefabIndex;
			}

			FindItUtil.CategorizedPrefabs[prefabIndex.Category][PrefabSubCategory.Any][prefabIndex.Id] = prefabIndex;

			FindItUtil.CategorizedPrefabs[prefabIndex.Category][prefabIndex.SubCategory][prefabIndex.Id] = prefabIndex;

			if (prefabIndex.IsFavorited)
			{
				FindItUtil.CategorizedPrefabs[PrefabCategory.Favorite][PrefabSubCategory.Any][prefabIndex.Id] = prefabIndex;

				if (!FindItUtil.CategorizedPrefabs[PrefabCategory.Favorite].ContainsKey(prefabIndex.SubCategory))
				{
					FindItUtil.CategorizedPrefabs[PrefabCategory.Favorite][prefabIndex.SubCategory] = new();
				}

				FindItUtil.CategorizedPrefabs[PrefabCategory.Favorite][prefabIndex.SubCategory][prefabIndex.Id] = prefabIndex;
			}

			//FindItUtil.UpdateFavoritesPack(prefabIndex);
		}

		private bool CheckIfResourceIntensive(PrefabBase prefab)
		{
			if (prefab is not ObjectGeometryPrefab geometryPrefab || geometryPrefab.m_Meshes is null || prefab.Has<TreeObject>() || prefab.isBuiltin)
			{
				return false;
			}

			return geometryPrefab.m_Meshes.Any(mesh =>
			{
				if (mesh.m_Mesh is not RenderPrefab meshPrefab)
				{
					return false;
				}

				var vertexCount = Math.Floor(meshPrefab.vertexCount / 3000D);
				var lodCount = meshPrefab.TryGet<LodProperties>(out var lodProperties) ? lodProperties.m_LodMeshes.Length : 0;

				if (vertexCount <= 4)
				{
					return false;
				}

				if (vertexCount <= 15)
				{
					return lodCount < 1;
				}

				return lodCount < 2;
			});
		}

		private string GetAssetName(Entity entity, string prefabName)
		{
			_prefabUISystem.GetTitleAndDescription(entity, out var titleId, out var _);

			return GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name)
				&& name is not null
				? name
				: prefabName.Replace('_', ' ').FormatWords();
		}

		private async void FillPdxModsData()
		{
			foreach (var grp in FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].Where(x => int.TryParse(x.PdxModsId, out var id) && id > 0).GroupBy(x => x.PdxModsId))
			{
				var details = await PdxModsUtil.GetLocalModDetails(grp.Key);

				if (details?.Success == true)
				{
					var folder = details.Mod.LocalData?.FolderAbsolutePath ?? string.Empty;
					var installDate = Directory.Exists(folder) ? Directory.GetCreationTime(folder) : (DateTime?)null;

					foreach (var item in grp)
					{
						item.InstalledDate = installDate;
						item.UpdatedDate = details.Mod.UpdatedDate;
					}
				}
			}
		}

		private IEnumerable<object> AddNumberToDuplicatePrefabNames()
		{
			var groups = new Dictionary<string, List<PrefabIndex>>();
			var anyList = FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any];

			foreach (var prefab in anyList.ToArray())
			{
				if (!groups.TryGetValue(prefab.Name, out var group))
				{
					groups[prefab.Name] = group = new();
				}

				group.Add(prefab);

				yield return null;
			}

			foreach (var group in groups.Values)
			{
				if (group.Count is 1)
				{
					continue;
				}

				var format = new string('0', group.Count.ToString().Length);

				for (var i = 0; i < group.Count; i++)
				{
					group[i].Name = $"{group[i].Name} {(i + 1).ToString(format)}";

					_pendingSortKeys.Add(group[i]);

					yield return null;
				}
			}
		}

		private IEnumerable<object> CleanupBrandPrefabs()
		{
			var brands = new HashSet<string>(FindItUtil.CategorizedPrefabs[PrefabCategory.Props][PrefabSubCategory.Props_Branding].Select(x => x.PrefabName));

			foreach (var category in FindItUtil.CategorizedPrefabs.Keys.ToArray())
			{
				if (category is PrefabCategory.Any)
				{
					continue;
				}

				var subCategories = FindItUtil.CategorizedPrefabs[category];

				foreach (var subCategory in subCategories.Keys.ToArray())
				{
					if (subCategory is PrefabSubCategory.Props_Branding || (category is PrefabCategory.Props && subCategory is PrefabSubCategory.Any))
					{
						continue;
					}

					if (!subCategories.TryGetValue(subCategory, out var prefabs))
					{
						continue;
					}

					foreach (var item in prefabs.ToList())
					{
						if (brands.Contains(item.PrefabName))
						{
							prefabs.Remove(item);

							Mod.Log.Debug($"Removed {item.PrefabName} from {subCategory}");
						}

						yield return null;
					}
				}
			}
		}

		private void AddAllCategories()
		{
			foreach (PrefabCategory category in Enum.GetValues(typeof(PrefabCategory)))
			{
				FindItUtil.CategorizedPrefabs[category] = new()
				{
					{ PrefabSubCategory.Any, new() }
				};

				if (category == PrefabCategory.Any)
				{
					continue;
				}

				foreach (PrefabSubCategory subCategory in Enum.GetValues(typeof(PrefabSubCategory)))
				{
					if ((int)subCategory > (int)category && (int)subCategory < (int)category + 100)
					{
						FindItUtil.CategorizedPrefabs[category][subCategory] = new();
					}
				}
			}
		}

		private void IndexZones()
		{
			var zonesQuery = SystemAPI.QueryBuilder().WithAll<ZoneData, ZonePropertiesData, PrefabData>().Build();
			var zones = zonesQuery.ToEntityArray(Allocator.Temp);
			var propertiesData = zonesQuery.ToComponentDataArray<ZonePropertiesData>(Allocator.Temp);

			var buildingsQuery = SystemAPI.QueryBuilder().WithAll<BuildingData, SpawnableBuildingData, PrefabData>().WithNone<SignatureBuildingData>().Build();
			var buildingsData = buildingsQuery.ToComponentDataArray<BuildingData>(Allocator.Temp);
			var spawnableBuildings = buildingsQuery.ToComponentDataArray<SpawnableBuildingData>(Allocator.Temp);

			var dictionary = new Dictionary<Entity, ZoneTypeFilter>();

			for (var i = 0; i < zones.Length; i++)
			{
				var zone = zones[i];
				var info = propertiesData[i];

				if (info.m_ResidentialProperties <= 0f)
				{
					dictionary[zone] = ZoneTypeFilter.Any;
					continue;
				}

				var ratio = info.m_ResidentialProperties / info.m_SpaceMultiplier;

				if (!info.m_ScaleResidentials)
				{
					dictionary[zone] = ZoneTypeFilter.Low;
				}
				else if (ratio < 1f)
				{
					var isRowHousing = true;

					for (var j = 0; j < spawnableBuildings.Length; j++)
					{
						if (spawnableBuildings[j].m_ZonePrefab == zone && buildingsData[j].m_LotSize.x > 2)
						{
							isRowHousing = false;
							break;
						}
					}

					dictionary[zone] = isRowHousing ? ZoneTypeFilter.Row : ZoneTypeFilter.Medium;
				}
				else
				{
					dictionary[zone] = ZoneTypeFilter.High;
				}
			}

			_zoneTypeCache = dictionary;
		}

		public static ZoneTypeFilter GetZoneType(Entity zonePrefab)
		{
			if (_zoneTypeCache != null && _zoneTypeCache.TryGetValue(zonePrefab, out var type))
			{
				return type;
			}

			return ZoneTypeFilter.Any;
		}

		private bool HasParking(PrefabBase prefab)
		{
			if (!_hasParkingCache.TryGetValue(prefab, out var hasParking))
			{
				_hasParkingCache[prefab] = hasParking = FindParking(prefab);
			}

			return hasParking;
		}

		private bool FindParking(PrefabBase prefab)
		{
			if (prefab.TryGet<SpawnLocation>(out var spawnLocation) && spawnLocation.m_ConnectionType == RouteConnectionType.Parking)
			{
				return true;
			}

			if (prefab.TryGet<ObjectSubLanes>(out var subLanes) && subLanes.m_SubLanes is not null)
			{
				foreach (var lane in subLanes.m_SubLanes)
				{
					if (lane.m_LanePrefab.Has<ParkingLane>())
					{
						return true;
					}
				}
			}

			if (prefab.TryGet<ObjectSubObjects>(out var subObjects) && subObjects.m_SubObjects is not null)
			{
				foreach (var obj in subObjects.m_SubObjects)
				{
					if (obj.m_Object is not null && HasParking(obj.m_Object))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
