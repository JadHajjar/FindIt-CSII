using Colossal.Entities;
using Colossal.Json;
using Colossal.Serialization.Entities;

using FindIt.Domain.Enums;
using FindIt.Utilities;

using Game;
using Game.Prefabs;
using Game.SceneFlow;

using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace FindIt.Systems
{
	internal partial class AutoVehiclePropGeneratorSystem : GameSystemBase
	{
		private bool generatedProps;
		private PrefabSystem _prefabSystem;

		private readonly Dictionary<string, List<string>> _obsoleteIdentifiers = JSON.MakeInto<Dictionary<string, List<string>>>(JSON.Load("{\"Prop_TramEngineCO01\":[\"TramEnginePropCO01\"],\"Prop_TramCarCO01\":[\"TramCarPropCO01\"],\"Prop_BusCO01\":[\"BusPropCO01\"],\"Prop_BusCO02\":[\"BusPropCO02\"],\"Prop_AirplanePassengerCO01\":[\"AirplanePassengerPropCO01\"],\"Prop_MuscleCar01\":[\"MuscleProp01\"],\"Prop_MuscleCar02\":[\"MuscleProp02\"],\"Prop_MuscleCar03\":[\"MuscleProp03\"],\"Prop_MuscleCar04\":[\"MuscleProp04\"],\"Prop_MuscleCar05\":[\"MuscleProp05\"],\"Prop_EU_PoliceVehicle02\":[\"EU_PoliceProp02\"],\"Prop_NA_PoliceVehicle02\":[\"NA_PoliceProp02\"],\"Prop_HelicopterPassenger01\":[\"HelicopterPassengerProp01\"],\"Prop_AirplanePassenger02\":[\"AirplanePassengerProp02\"],\"Prop_TruckTrailer02\":[\"TruckTrailerProp02\"],\"Prop_TruckTrailer03\":[\"TruckTrailerProp03\"],\"Prop_TruckTrailer01\":[\"TruckTrailerProp01\"],\"Prop_TruckTrailer04\":[\"TruckTrailerProp04\"],\"Prop_EU_TrainPassengerEngine01\":[\"TrainPassengerEngineProp01\"],\"Prop_NA_TrainPassengerEngine01\":[\"TrainPassengerEngineProp02\"],\"Prop_EU_TrainPassengerCar01\":[\"TrainPassengerCarProp01\"],\"Prop_NA_TrainPassengerCar01\":[\"TrainPassengerCarProp02\"],\"Prop_TrainForestryCar01\":[\"TrainForestryCarProp01\"],\"Prop_NA_TrainCargoEngine01\":[\"NA_TrainCargoEngineProp01\"],\"Prop_EU_TrainCargoEngine01\":[\"EU_TrainCargoEngineProp01\"],\"Prop_TrainCargoCar01\":[\"TrainCargoCarProp01\"],\"Prop_TrainOreCar01\":[\"TrainOreCarProp01\"],\"Prop_TrainAgricultureCar01\":[\"TrainAgricultureCarProp01\"],\"Prop_TrainOilCar01\":[\"TrainOilCarProp01\"],\"Prop_TractorTrailer02\":[\"TractorTrailerProp02\"],\"Prop_TractorTrailer01\":[\"TractorTrailerProp01\"],\"Prop_TractorTrailer03\":[\"TractorTrailerProp03\"],\"Prop_Taxi01\":[\"TaxiProp01\"],\"Prop_Taxi02\":[\"TaxiProp02\"],\"Prop_SubwayEngine01\":[\"SubwayEngineProp01\"],\"Prop_TramEngine01\":[\"TramEngineProp01\"],\"Prop_SubwayCar01\":[\"SubwayCarProp01\"],\"Prop_TramCar01\":[\"TramCarProp01\"],\"Prop_SpaceRocket01\":[\"SpaceRocketProp01\"],\"Prop_ShipPassenger01\":[\"ShipPassengerProp01\"],\"Prop_ShipCargo01\":[\"ShipCargoProp01\"],\"Prop_RoadMaintenanceVehicle01\":[\"RoadMaintenanceProp01\"],\"Prop_PrisonVan01\":[\"ParkPrisonVanProp01\"],\"Prop_ParkMaintenanceVehicle01\":[\"ParkMaintenanceProp01\"],\"Prop_OreMiningTruck01\":[\"OreMiningTruckProp01\"],\"Prop_OilTruck01\":[\"OilTruckProp01\"],\"Prop_MotorbikeDelivery01\":[\"MotorbikeDeliveryProp01\"],\"Prop_HelicopterPolice01\":[\"HelicopterPoliceProp01\"],\"Prop_HelicopterHealthcare01\":[\"HelicopterHealthcareProp01\"],\"Prop_HelicopterFire01\":[\"HelicopterFireProp01\"],\"Prop_Hearse01\":[\"HearseProp01\"],\"Prop_FrontendLoaderTrailer01\":[],\"Prop_OreMiningTractorTrailer01\":[],\"Prop_ForestHarvesterTrailer01\":[\"ForestHarvesterTrailerProp01\"],\"Prop_TractorFertilizerSpreader01\":[\"TractorFertilizerSpreaderProp01\"],\"Prop_TractorPlough01\":[\"TractorPloughProp01\"],\"Prop_TractorSowingMachine01\":[\"TractorSowingMachineProp01\"],\"Prop_TractorSprayer01\":[\"TractorSprayerProp01\"],\"Prop_ForestForwarderTrailer01\":[\"ForestForwarderTrailerProp01\"],\"Prop_ForestForwarder01\":[\"ForestForwarderProp01\"],\"Prop_ForestHarvester01\":[\"ForestHarvesterProp01\"],\"Prop_FrontendLoader01\":[\"FrontendLoaderProp01\"],\"Prop_OreMiningTractor01\":[\"OreMiningTractorProp01\"],\"Prop_Tractor01\":[\"TractorProp01\",\"NA_TractorProp01\"],\"Prop_CoalTruck01\":[\"CoalTruckProp01\"],\"Prop_CarTrailer01\":[\"CarTrailerProp01\"],\"Prop_Car02\":[\"CarProp02\"],\"Prop_Car03\":[\"CarProp03\"],\"Prop_Car04\":[\"CarProp04\"],\"Prop_Car05\":[\"CarProp05\"],\"Prop_Car06\":[\"CarProp06\"],\"Prop_Car07\":[\"CarProp07\"],\"Prop_Car08\":[\"CarProp08\"],\"Prop_Van01\":[\"VanProp01\"],\"Prop_Car09\":[\"CarProp09\"],\"Prop_Car01\":[\"CarProp01\"],\"Prop_Motorbike01\":[\"MotorbikeProp01\"],\"Prop_Scooter01\":[\"ScooterProp01\"],\"Prop_CamperTrailer01\":[\"CamperTrailerProp01\"],\"Prop_Bus01\":[\"BusProp01\"],\"Prop_Bus02\":[\"BusProp02\"],\"Prop_Bus03\":[\"BusProp03\"],\"Prop_BlastholeDrillingRig01\":[],\"Prop_CombineHarvester01\":[\"CombineHarvesterProp01\"],\"Prop_MiningExcavator01\":[\"MiningExcavatorProp01\"],\"Prop_AirplanePassenger01\":[\"AirplanePassengerProp01\"],\"Prop_AirplaneCargo01\":[\"AirplaneCargoProp01\"],\"Prop_AdministrationVehicle01\":[\"AdministrationProp01\"],\"Prop_EU_Snowplow01\":[\"SnowplowProp01\"],\"Prop_NA_Snowplow01\":[\"SnowplowProp02\"],\"Prop_EU_GarbageTruck01\":[\"GarbageTruckProp01\"],\"Prop_NA_GarbageTruck01\":[\"GarbageTruckProp02\"],\"Prop_EU_TruckTractor01\":[\"EU_TruckTractorProp01\"],\"Prop_NA_TruckTractor01\":[\"NA_TruckTractorProp01\"],\"Prop_EU_PostVan01\":[\"EU_PostProp01\"],\"Prop_NA_PostVan01\":[\"NA_PostProp01\"],\"Prop_EU_PoliceVehicle01\":[\"EU_PoliceProp01\",\"EU_PoliceVehicleProp01\"],\"Prop_NA_PoliceVehicle01\":[\"NA_PoliceProp01\",\"NA_PoliceVehicleProp01\"],\"Prop_EU_FireTruck01\":[\"EU_FireTruckProp01\"],\"Prop_NA_FireTruck01\":[\"NA_FireTruckProp01\"],\"Prop_EU_Ambulance01\":[\"EU_AmbulanceProp01\",\"Paramedic_AmbulanceProp01\"],\"Prop_NA_Ambulance01\":[\"NA_AmbulanceProp01\"],\"Prop_EU_DeliveryVan01\":[\"EU_DeliveryVanProp01\"],\"Prop_NA_DeliveryVan01\":[\"NA_DeliveryVanPropProp01\"],\"Prop_Bicycle01\":[\"BicycleProp01\"],\"Prop_SkateBoard01\":[\"SkateBoardProp01\"],\"Prop_CasketTrolley01\":[\"CasketTrolleyProp01\"],\"Prop_KickScooter01\":[\"KickScooterProp01\"]}"));

		public static readonly Dictionary<string, string> AssetReferenceMap = new();

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
			var vehicleEntities = SystemAPI.QueryBuilder().WithAll<VehicleData>().Build().ToEntityArray(Allocator.Temp);

			for (var i = 0; i < vehicleEntities.Length; i++)
			{
				if (!_prefabSystem.TryGetPrefab<MovingObjectPrefab>(vehicleEntities[i], out var prefab))
				{
					return;
				}

				CreatePrefab("Prop_" + prefab.name, GetCategory(vehicleEntities[i]), prefab);
			}

			CreatePrefab("Prop_KickScooter01", PrefabSubCategory.Vehicles_Bikes, mesh: "KickScooter01_Prop Mesh");
			CreatePrefab("Prop_CasketTrolley01", PrefabSubCategory.Vehicles_Services, mesh: "Service_Hearse_CasketTrolley01_Prop Mesh");
			CreatePrefab("Prop_Bicycle01", PrefabSubCategory.Vehicles_Bikes, mesh: "Bicycle01_Prop Mesh");
			CreatePrefab("Prop_SkateBoard01", PrefabSubCategory.Vehicles_Bikes, mesh: "SkateBoard01_Prop Mesh");

			Mod.Log.Info("Generated Vehicle Prop Assets");
		}

		private void CreatePrefab(string name, PrefabSubCategory subCategory, ObjectGeometryPrefab original = null, string mesh = null)
		{
			var newPrefab = ScriptableObject.CreateInstance<StaticObjectPrefab>();

			newPrefab.name = name;

			if (original != null)
			{
				AssetReferenceMap[$"{nameof(StaticObjectPrefab)}.{name}"] = $"{original.GetType().Name}.{original.name}";

				newPrefab.m_Meshes = original.m_Meshes;

				if (original.TryGet<UIObject>(out var uIObject))
				{
					newPrefab.AddComponentFrom(uIObject);
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
					GameManager.instance.localizationManager.activeDictionary.Add("Assets.NAME[" + name + "]", localeName + " Prop");
				}
				else
				{
					GameManager.instance.localizationManager.activeDictionary.Add("Assets.NAME[" + name + "]", name.Replace('_', ' ').FormatWords() + " Prop");
				}
			}
			else if (mesh != null)
			{
				if (!_prefabSystem.TryGetPrefab(new PrefabID(nameof(RenderPrefab), mesh), out var meshPrefab))
				{
					Mod.Log.Warn("Mesh not found: " + mesh);
					return;
				}

				newPrefab.m_Meshes = new[]
				{
					new ObjectMeshInfo
					{
						m_Mesh = meshPrefab as RenderPrefab,
					}
				};

				GameManager.instance.localizationManager.activeDictionary.Add("Assets.NAME[" + name + "]", name.Replace('_', ' ').FormatWords() + " Prop");
			}

			var overrides = newPrefab.AddComponent<EditorAssetCategoryOverride>();
			overrides.m_ExcludeCategories
				= overrides.m_IncludeCategories
				= new[] { $"FindIt/{(int)PrefabCategory.Vehicles}/{(int)subCategory}" };

			if (_obsoleteIdentifiers.TryGetValue(newPrefab.name, out var identifiers))
			{
				var obsoleteIdentifier = newPrefab.AddComponent<ObsoleteIdentifiers>();
				obsoleteIdentifier.m_PrefabIdentifiers = identifiers.Select(x => new PrefabIdentifierInfo { m_Type = nameof(StaticObjectPrefab), m_Name = x }).ToArray();
			}

			_prefabSystem.AddPrefab(newPrefab);
		}

		private PrefabSubCategory GetCategory(Entity entity)
		{
			if (EntityManager.TryGetComponent<PersonalCarData>(entity, out var personalCarData))
			{
				return personalCarData.m_PassengerCapacity == 1 ? PrefabSubCategory.Vehicles_Bikes : PrefabSubCategory.Vehicles_Residential;
			}

			if (EntityManager.HasComponent<WatercraftData>(entity))
			{
				return PrefabSubCategory.Vehicles_Ship;
			}

			if (EntityManager.HasComponent<AircraftData>(entity))
			{
				return PrefabSubCategory.Vehicles_Plane;
			}

			if (EntityManager.HasComponent<TrainData>(entity))
			{
				return PrefabSubCategory.Vehicles_Train;
			}

			if (EntityManager.TryGetComponent<PublicTransportVehicleData>(entity, out var publicTransportVehicleData))
			{
				if (publicTransportVehicleData.m_PurposeMask.HasFlag(PublicTransportPurpose.TransportLine))
				{
					return publicTransportVehicleData.m_TransportType switch
					{
						TransportType.Bus => PrefabSubCategory.Vehicles_Bus,
						TransportType.Train or TransportType.Tram or TransportType.Subway => PrefabSubCategory.Vehicles_Train,
						TransportType.Ship => PrefabSubCategory.Vehicles_Ship,
						TransportType.Post => PrefabSubCategory.Vehicles_Services,
						TransportType.Airplane or TransportType.Helicopter => PrefabSubCategory.Vehicles_Plane,
						_ => PrefabSubCategory.Vehicles_Misc,
					};
				}

				if (publicTransportVehicleData.m_PurposeMask.HasFlag(PublicTransportPurpose.PrisonerTransport))
				{
					return PrefabSubCategory.Vehicles_Services;
				}
			}

			if (EntityManager.HasComponent<AmbulanceData>(entity)
				|| EntityManager.HasComponent<PoliceCarData>(entity)
				|| EntityManager.HasComponent<PublicTransportVehicleData>(entity)
				|| EntityManager.HasComponent<HearseData>(entity)
				|| EntityManager.HasComponent<PostVanData>(entity)
				|| EntityManager.HasComponent<TaxiData>(entity)
				|| EntityManager.HasComponent<FireEngineData>(entity)
				|| EntityManager.HasComponent<GarbageTruckData>(entity)
				|| EntityManager.HasComponent<MaintenanceVehicleData>(entity))
			{
				return PrefabSubCategory.Vehicles_Services;
			}

			if (EntityManager.HasComponent<CargoTransportVehicleData>(entity)
				|| EntityManager.HasComponent<DeliveryTruckData>(entity)
				|| EntityManager.HasComponent<WorkVehicleData>(entity))
			{
				return PrefabSubCategory.Vehicles_Industrial;
			}

			return PrefabSubCategory.Vehicles_Misc;
		}
	}
}
