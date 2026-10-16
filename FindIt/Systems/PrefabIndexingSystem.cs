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
		// In game, the index always gets at least this many milliseconds per frame, so it still
		// finishes at high frame rates.
		// This plus a game frame shorter than itself keeps the frame rate above 50.
		private const long MinFrameBudgetMilliseconds = 10;
		// Milliseconds per frame the index gets while the loading screen is up, since nobody sees
		// the frame rate then.
		// Giving it less per frame wouldn't shorten the loading screen, which stays up about as
		// long as the total work behind it.
		// Giving it more would slow the screen down: the texture streaming it waits on advances
		// once a frame.
		// It's also the most the index ever gets in game.
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
		// Matches the prefabs the game created, updated, or deleted this frame.
		// A partial index visits them, and OnUpdate checks the query instead of RequireForUpdate,
		// because the system has to run every frame while a full index is in progress.
		private EntityQuery _changedPrefabsQuery;
		// The full index in progress, which OnUpdate advances a bit every frame.
		// Null when none is running.
		private IEnumerator _fullIndex;
		// Prefabs the index added or renamed since it last paused.
		// A worker thread computes their name sort keys next.
		private List<PrefabIndex> _pendingSortKeys = new();

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

			// Throws away an index the previous load didn't finish.
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
				// OnUpdate runs the full index a bit at a time, and it keeps running behind the
				// loading screen.
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

			// Also runs during a full index.
			// Created, Updated, and Deleted only last one frame, and the full index may already
			// have gone past these prefabs' processor.
			if (!_changedPrefabsQuery.IsEmptyIgnoreFilter)
			{
				// A partial index never pauses, so a single MoveNext runs it to the end.
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

			// Each index starts with an empty cache, as prefabs may have changed since the last.
			// A partial index between two frames of a full one keeps the full index's cache.
			if (full || _fullIndex is null)
			{
				_hasParkingCache.Clear();
			}

			if (full)
			{
				FindItUtil.CategorizedPrefabs.Clear();
				// The panel would otherwise show the last index's search results, and let the
				// player pick from them, until the search that ends this index replaces them.
				// A search still running from before the load would write them back.
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

			// Each step yields after one item, where a full index can stop until the next frame.
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
				// Quick enough to run all at once.
				FillPdxModsData();
			}

			// A partial index during a full one leaves IsReady false until the full one ends.
			if (full || _fullIndex is null)
			{
				FindItUtil.IsReady = true;

				if (full)
				{
					// The picker's options, like the panel, stay empty if the player opened them
					// before the index was ready.
					// The picker refreshes first, since its options don't read the index.
					// The catch logs a failure, so the search below and the rest of the frame's
					// update still run.
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
			}

			stopWatch.Stop();

			workTime += frameStopWatch.Elapsed;

			// A partial index runs in one frame, so only a full one logs its frames and work time.
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

			// A new list, since the worker thread still reads the old one.
			_pendingSortKeys = new();

			// Task.Run carries the caller's culture over, so the keys sort as they would on the
			// main thread.
			Task.Run(() =>
			{
				foreach (var prefab in prefabs)
				{
					// Reading the key computes and keeps it.
					_ = prefab.NameSortKey;
				}
			});
		}

		private long GetFrameBudgetMilliseconds(TimeSpan lastSlice)
		{
			if (GameManager.instance.isGameLoading)
			{
				return LoadingFrameBudgetMilliseconds;
			}

			// Gives the index as much time as the game itself took last frame, which roughly halves
			// the frame rate.
			// The last frame's time includes the index's own share, so this subtracts it.
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

				// GetEntityQuery caches the query, so full and partial indexes share it.
				// Matches gives each query a mask, and nothing checks a world's limit of 1024.
				// The cache keeps that to two masks per processor, one per HideRandomAssets value.
				var query = GetEntityQuery(queries);

				if (full)
				{
					// ToArray copies the entities out of the Temp allocation, which only lasts the
					// frame, since a full index walks them over several frames.
					entities = query.ToEntityArray(Allocator.Temp).ToArray();
				}
				else
				{
					// This keeps a partial's cost to the changes, whatever the playset's size.
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
				// When UpdatePrefab gives a prefab a new entity, or RemovePrefab drops it, the game
				// marks the old entity Deleted, and it stays until the end of the frame.
				// This removes its entry, which would otherwise stay under an id the game reuses.
				// It comes before TryGetPrefab, which fails for a removed prefab.
				// When RemovePrefab runs later in the frame than this system's update, the game
				// destroys the entity before the next update, so its entry stays.
				// Road Builder tags the new entity of a road it deletes, and that entity still
				// matches the roads processor's query, so the check keeps deleted roads out too.
				if (EntityManager.HasComponent<Deleted>(entity) || (roadBuilderDiscarded.HasValue && EntityManager.HasComponent(entity, roadBuilderDiscarded.Value)))
				{
					FindItUtil.RemoveItem(entity);

					return;
				}

				// Unity's name getter is a native call that makes a new string each time, so this
				// reads the name once and passes it on.
				// It throws on a destroyed prefab, so the catch below doesn't read it again.
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
					// Only a processor that takes the prefab replaces its old entry.
					// Several processors' queries can match one prefab, and one that rejects the
					// prefab would otherwise find the entry another just added and remove it.
					// Find may also return this entity's own entry, which the full index added.
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
			prefabIndex.Tags ??= new();
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

			_pendingSortKeys.Add(prefabIndex);

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

			// Copies the list, since a partial index can change it between two steps.
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

			// Copies the keys, since between two steps a partial index can add or remove a
			// favorites subcategory, and resetting the favorites replaces their category.
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
			// Buildings share a lot of sub-objects, so this checks each prefab only once per index.
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
