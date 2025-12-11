using Colossal.IO.AssetDatabase;
using Colossal.Serialization.Entities;

using FindIt.Domain;
using FindIt.Domain.Enums;
using FindIt.Utilities;
using FindIt.Utilities.PrefabCategoryProcessor;

using Game;
using Game.Objects;
using Game.Prefabs;
using Game.SceneFlow;

using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace FindIt.Systems
{
	internal partial class AutoQuantityPropGeneratorSystem : GameSystemBase
	{
		private bool generatedProps;
		private PrefabSystem _prefabSystem;

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

			Enabled = false;
		}

		protected override void OnUpdate()
		{
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			if (!generatedProps)
			{
				generatedProps = true;

				GenerateProps();
			}
		}

		private void GenerateProps()
		{
			var quantityEntities = SystemAPI.QueryBuilder().WithAll<QuantityObjectData>().WithNone<PlaceholderObjectData>().Build().ToEntityArray(Allocator.Temp);
			var prefabs = new List<string>();
			var localeDictionary = new Dictionary<string, string>();

			for (var i = 0; i < quantityEntities.Length; i++)
			{
				if (!_prefabSystem.TryGetPrefab<ObjectGeometryPrefab>(quantityEntities[i], out var prefab))
				{
					continue;
				}

				if (prefab.TryGet<EditorAssetCategoryOverride>(out var overrides) && (overrides.m_ExcludeCategories?.Contains("FindIt") ?? false))
				{
					continue;
				}

				foreach (var item in prefab.m_Meshes.OrderBy(x => x.m_RequireState))
				{
					if (prefabs.Contains(item.m_Mesh.name))
					{
						continue;
					}

					var newPrefabName = $"Prop_{prefab.name}_{item.m_RequireState}";

					CreatePrefab(newPrefabName, $" {GetName(item.m_RequireState)} Prop", (int)GetCategory(prefab, quantityEntities[i]), prefab, item, localeDictionary);

					prefabs.Add(item.m_Mesh.name);

					FindItUtil.AssetMap[prefab.name] = newPrefabName;
				}
			}

			foreach (var item in new LocaleHelper(localeDictionary).GetAvailableLanguages())
			{
				GameManager.instance.localizationManager.AddSource(item.LocaleId, item);
			}

			Mod.Log.InfoFormat("Generated Quantity Prop Assets ({0})", prefabs.Count);
		}

		private void CreatePrefab(string name, string typeName, int subCategory, ObjectGeometryPrefab original, ObjectMeshInfo mesh, Dictionary<string, string> localeDictionary)
		{
			var newPrefab = ScriptableObject.CreateInstance<StaticObjectPrefab>();

			newPrefab.name = name;

			newPrefab.AddComponent<FindItGenerated>();

			var overrides = newPrefab.AddComponent<EditorAssetCategoryOverride>();
			overrides.m_ExcludeCategories = new string[0];
			overrides.m_IncludeCategories = new[] { $"FindIt/{subCategory - (subCategory % 100)}/{subCategory}" };

			if (original != null)
			{
				newPrefab.m_Meshes = new[]
				{
					new ObjectMeshInfo
					{
						m_Mesh = mesh.m_Mesh,
						m_Position = mesh.m_Position,
						m_Rotation = mesh.m_Rotation,
					}
				};

				if (original.TryGet<UIObject>(out var uIObject))
				{
					newPrefab.AddComponentFrom(uIObject).m_Group = null;
				}

				if (original.TryGet<ThemeObject>(out var themeObject))
				{
					newPrefab.AddComponentFrom(themeObject);
				}

				if (original.TryGet<AssetPackItem>(out var assetPackItem))
				{
					newPrefab.AddComponentFrom(assetPackItem);
				}

				if (original.TryGet<ContentPrerequisite>(out var contentPrerequisite))
				{
					newPrefab.AddComponentFrom(contentPrerequisite);
				}

				if (GameManager.instance.localizationManager.activeDictionary.TryGetValue("Assets.NAME[" + original.name + "]", out var localeName))
				{
					localeDictionary.Add("Assets.NAME[" + name + "]", localeName + typeName);
				}
				else
				{
					localeDictionary.Add("Assets.NAME[" + name + "]", original.name.Replace('_', ' ').FormatWords() + typeName);
				}

				if (original.asset?.database == AssetDatabase<ParadoxMods>.instance)
				{
					overrides.m_IncludeCategories = new[] { $"FindIt/{subCategory - (subCategory % 100)}/{subCategory}/{original.asset.GetMeta().platformID}" };
				}
			}

			_prefabSystem.AddPrefab(newPrefab);
		}

		private object GetName(ObjectState state)
		{
			return state switch
			{
				ObjectState.None => "Empty",
				ObjectState.Partial1 => "Low",
				ObjectState.Partial2 => "Medium",
				_ => state.ToString(),
			};
		}

		private PrefabSubCategory GetCategory(PrefabBase prefab, Entity entity)
		{
			if (PropPrefabCategoryProcessor.TryGetSubCategory(prefab, entity, EntityManager, out var subCategory))
			{
				return subCategory;
			}

			return PrefabSubCategory.Props_Misc;
		}
	}
}
