using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace FireEvent
{
    public class MarkGroupMembersOnFire : DaySystem, IModSystem
    {
        EntityQuery Chairs;

        protected override void Initialise()
        {
            base.Initialise();

            Chairs = GetEntityQuery(new QueryHelper()
                .All(typeof(CApplianceChair)));

            RequireSingletonForUpdate<SFireScore>();
            RequireSingletonForUpdate<SFireGameActive>();
        }

        protected override void OnUpdate()
        {
            using NativeArray<Entity> chairEntities = Chairs.ToEntityArray(Allocator.Temp);
            using NativeArray<CApplianceChair> chairs = Chairs.ToComponentDataArray<CApplianceChair>(Allocator.Temp);

            for (int i = 0; i < chairs.Length; i++)
            {
                Entity chairEntity = chairEntities[i];
                CApplianceChair chair = chairs[i];
                if (!chair.IsInUse)
                    continue;
                if (!Has<CIsOnFire>(chairEntity))
                {
                    if (Has<CGroupMemberOnFire>(chair.Occupant))
                        EntityManager.RemoveComponent<CGroupMemberOnFire>(chair.Occupant);
                    continue;
                }
                Set<CGroupMemberOnFire>(chair.Occupant);
            }
        }
    }
}
