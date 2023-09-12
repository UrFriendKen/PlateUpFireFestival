using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FireEvent
{
    [UpdateInGroup(typeof(EndOfFrameGroup))]
    public class UpdateFireScoreCountDown : RestaurantSystem, IModSystem
    {
        EntityQuery Players;
        EntityQuery ItemDeliveredRecoveries;
        EntityQuery GroupLeavingRecoveries;

        protected override void Initialise()
        {
            base.Initialise();

            Players = GetEntityQuery(typeof(CPlayer));
            ItemDeliveredRecoveries = GetEntityQuery(typeof(CItemDeliveredAddFireScore));
            GroupLeavingRecoveries = GetEntityQuery(typeof(CGroupLeavingAddFireScore));

            RequireSingletonForUpdate<SFireScore>();
            RequireSingletonForUpdate<SFireGameActive>();
        }

        private float RecoveryPlayerCountMultiplier(int players)
        {
            if (players < 1)
                return 1f;
            return 0.5f + 0.5f / players;
        }

        protected override void OnUpdate()
        {
            SFireScore fireScore = GetSingleton<SFireScore>();
            if (fireScore.GameMode == FireGameMode.CountDown)
            {
                int groupLeavingTotalMembers = 0;
                using NativeArray<CGroupLeavingAddFireScore> groupLeavings = GroupLeavingRecoveries.ToComponentDataArray<CGroupLeavingAddFireScore>(Allocator.Temp);
                for (int i = 0; i < groupLeavings.Length; i++)
                {
                    groupLeavingTotalMembers += groupLeavings[i].Count;
                }

                int itemDeliveredRecoveryCount = 0;
                int groupLeavingRecoveryCount = 0;
                itemDeliveredRecoveryCount = ItemDeliveredRecoveries.CalculateEntityCount();
                groupLeavingRecoveryCount = groupLeavingTotalMembers;
                if (!Has<SIsDayTime>())
                {
                    return;
                }

                float dt = Time.DeltaTime;
                float frameMultiplier = fireScore.FrameMultiplier;
                float recoveryPlayerCountMultiplier = RecoveryPlayerCountMultiplier(Players.CalculateEntityCount());
                fireScore.Score -= dt * fireScore.BaseDecaySpeed * 2f / (1f + Mathf.Exp(frameMultiplier / 18.832f));
                fireScore.Score += itemDeliveredRecoveryCount * fireScore.ItemDeliveredRecovery * recoveryPlayerCountMultiplier;
                fireScore.Score += groupLeavingRecoveryCount * fireScore.GroupLeavingRecovery * recoveryPlayerCountMultiplier;
                Set(fireScore);
            }
            EntityManager.DestroyEntity(ItemDeliveredRecoveries);
            EntityManager.DestroyEntity(GroupLeavingRecoveries);
        }
    }
}
