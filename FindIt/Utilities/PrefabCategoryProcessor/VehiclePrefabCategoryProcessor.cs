using Colossal.Entities;
using FindIt.Domain;
using FindIt.Domain.Interfaces;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class VehiclePrefabCategoryProcessor //: IPrefabCategoryProcessor
    {
        private readonly EntityManager _entityManager;

        public VehiclePrefabCategoryProcessor(EntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public EntityQueryDesc[] GetEntityQuery()
        {
            return new[]
            {
                new EntityQueryDesc
                {
                    All = new[]
                    {
                        ComponentType.ReadOnly<VehicleData>(),
                    },
                },
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            if (prefab is not MovingObjectPrefab)
            {
                prefabIndex = null;
                return false;
            }

            prefabIndex = new PrefabIndex(prefab)
            {
                Category = Domain.Enums.PrefabCategory.Vehicles,
            };

            if (_entityManager.TryGetComponent<PersonalCarData>(entity, out var personalCarData))
            {
                prefabIndex.SubCategory = personalCarData.m_PassengerCapacity == 1 ? Domain.Enums.PrefabSubCategory.Vehicles_Bikes : Domain.Enums.PrefabSubCategory.Vehicles_Residential;
                return true;
            }

            if (_entityManager.TryGetComponent<PublicTransportVehicleData>(entity, out var publicTransportVehicleData))
            {
                if (publicTransportVehicleData.m_PurposeMask.HasFlag(PublicTransportPurpose.TransportLine))
                {
                    prefabIndex.SubCategory = publicTransportVehicleData.m_TransportType switch
                    {
                        TransportType.Bus => Domain.Enums.PrefabSubCategory.Vehicles_Bus,
                        TransportType.Train or TransportType.Tram or TransportType.Subway => Domain.Enums.PrefabSubCategory.Vehicles_Train,
                        TransportType.Ship => Domain.Enums.PrefabSubCategory.Vehicles_Ship,
                        TransportType.Post => Domain.Enums.PrefabSubCategory.Vehicles_Services,
                        TransportType.Airplane or TransportType.Helicopter => Domain.Enums.PrefabSubCategory.Vehicles_Plane,
                        _ => Domain.Enums.PrefabSubCategory.Vehicles_Misc,
                    };
                    return true;
                }

                if (publicTransportVehicleData.m_PurposeMask.HasFlag(PublicTransportPurpose.PrisonerTransport))
                {
                    prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Vehicles_Services;
                    return true;
                }
            }

            if (_entityManager.HasComponent<AmbulanceData>(entity)
                || _entityManager.HasComponent<PoliceCarData>(entity)
                || _entityManager.HasComponent<PublicTransportVehicleData>(entity)
                || _entityManager.HasComponent<HearseData>(entity)
                || _entityManager.HasComponent<FireEngineData>(entity)
                || _entityManager.HasComponent<GarbageTruckData>(entity)
                || _entityManager.HasComponent<MaintenanceVehicleData>(entity))
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Vehicles_Services;
                return true;
            }

            if (_entityManager.HasComponent<CargoTransportVehicleData>(entity)
                || _entityManager.HasComponent<DeliveryTruckData>(entity)
                || _entityManager.HasComponent<WorkVehicleData>(entity))
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Vehicles_Industrial;
                return true;
            }

            prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Vehicles_Misc;

            return true;
        }
    }
}
