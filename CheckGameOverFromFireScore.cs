using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace FireEvent
{
    public class CheckGameOverFromFireScore : RestaurantSystem, IModSystem
    {
        protected override void Initialise()
        {
            base.Initialise();

            RequireSingletonForUpdate<SFireScore>();
            RequireSingletonForUpdate<SFireGameActive>();
        }

        protected override void OnUpdate()
        {
            if (!Require(out SFireScore fireScore) || !fireScore.LoseWhenScoreEmpty)
                return;

            if (!HasSingleton<SGameOver>() && fireScore.Score <= 0f && !Has<SPracticeMode>())
            {
                Entity entity = base.EntityManager.CreateEntity(typeof(SGameOver), typeof(CGamePauseBlock));
                base.EntityManager.SetComponentData(entity, new SGameOver
                {
                    Reason = LossReason.Patience
                });
            }
        }
    }
}
