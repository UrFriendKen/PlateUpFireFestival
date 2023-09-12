using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FireEvent
{
    public class ConstantFires : DaySystem, IModSystem
    {
        private struct STimeTracker : IComponentData
        {
            public float LastTime;

            public float Delay;
        }

        private EntityQuery FlammableAppliances;

        protected override void Initialise()
        {
            base.Initialise();
            FlammableAppliances = GetEntityQuery(new QueryHelper().All(typeof(CAppliance), typeof(CIsInteractive)).None(typeof(CFireImmune), typeof(CApplianceTable), typeof(CApplianceChair), typeof(CIsOnFire)));

            RequireSingletonForUpdate<SFireScore>();
            RequireSingletonForUpdate<SFireGameActive>();
        }

        protected override void OnUpdate()
        {
            if (!TryGetSingleton(out SFireScore fireScore) || !fireScore.RandomFireActive)
            {
                return;
            }
            STimeTracker orCreate = GetOrCreate<STimeTracker>();
            float totalTime = base.Time.TotalTime;
            if (totalTime - orCreate.LastTime < orCreate.Delay)
            {
                return;
            }
            orCreate.LastTime = totalTime;
            orCreate.Delay = Random.Range(0.75f, 1.5f) * fireScore.RandomFireInterval;
            using NativeArray<Entity> list = FlammableAppliances.ToEntityArray(Allocator.Temp);
            list.ShuffleInPlace();
            foreach (Entity item in list)
            {
                if (Require(item, out CAppliance comp) && comp.Layer == OccupancyLayer.Default)
                {
                    base.EntityManager.AddComponent<CIsOnFire>(item);
                    Set(orCreate);
                    break;
                }
            }
        }
    }
}
