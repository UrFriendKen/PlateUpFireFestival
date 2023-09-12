using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace FireEvent
{
    public class EndFireGameAfterLastDay : RestaurantSystem, IModSystem
    {
        EntityQuery Active;
        protected override void Initialise()
        {
            base.Initialise();

            Active = GetEntityQuery(typeof(SFireGameActive));

            RequireSingletonForUpdate<SFireScore>();
            RequireSingletonForUpdate<SFireGameActive>();
            RequireSingletonForUpdate<SDay>();
        }

        protected override void OnUpdate()
        {
            if (!TryGetSingleton(out SFireScore fireScore) || !TryGetSingleton(out SDay sDay))
                return;

            int day = sDay.Day;
            if (Has<SIsNightTime>())
            {
                day += 1;
            }

            if (!fireScore.Endless && day > fireScore.Target)
                EntityManager.DestroyEntity(Active);
        }
    }
}
