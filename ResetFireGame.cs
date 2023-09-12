using Kitchen;
using KitchenMods;
using System;
using Unity.Entities;

namespace FireEvent
{
    public class ResetFireGame : GenericSystemBase, IModSystem
    {
        private static ResetFireGame _instance;

        EntityQuery Resets;
        EntityQuery Active;
        EntityQuery FireScore;

        protected override void Initialise()
        {
            base.Initialise();
            _instance = this;
            Resets = GetEntityQuery(typeof(SReset));
            Active = GetEntityQuery(typeof(SFireGameActive));
            FireScore = GetEntityQuery(typeof(SFireScore));

        }

        protected override void OnUpdate()
        {
            if (Resets.IsEmpty)
                return;

            Clear<SInsuranceSpending>();

            if (!Enum.TryParse(Main.PrefManager.Get<string>(Main.GAME_MODE_ID), out FireGameMode gameMode))
            {
                gameMode = FireGameMode.None;
            }

            if (gameMode == FireGameMode.None)
            {
                EntityManager.DestroyEntity(Active);
                EntityManager.DestroyEntity(FireScore);
                return;
            }

            int target;
            float startingScore;
            float proximityMultiplier;
            float baseDecaySpeed;
            float itemDeliveredRecovery;
            float groupLeavingRecovery;
            float randomFireInterval;
            UpdateFireScoreCountUp.ScoringMode countUpScoringMode = default;

            bool loseWhenScoreEmpty;
            switch (gameMode)
            {
                case FireGameMode.CountDown:
                    startingScore = Main.PrefManager.Get<int>(Main.COUNT_DOWN_STARTING_SCORE_ID) * 60f;
                    baseDecaySpeed = Main.PrefManager.Get<int>(Main.COUNT_DOWN_BASE_DECAY_SPEED_ID);
                    itemDeliveredRecovery = Main.PrefManager.Get<int>(Main.COUNT_DOWN_ITEM_DELIVERED_RECOVERY_ID);
                    groupLeavingRecovery = Main.PrefManager.Get<int>(Main.COUNT_DOWN_GROUP_LEAVING_RECOVERY_ID);
                    proximityMultiplier = Main.PrefManager.Get<int>(Main.COUNT_DOWN_PROXIMITY_MULTIPLIER_ID);
                    target = Main.PrefManager.Get<int>(Main.COUNT_DOWN_LAST_DAY_ID);
                    randomFireInterval = Main.PrefManager.Get<int>(Main.COUNT_DOWN_RANDOM_FIRE_INTERVAL_ID);
                    loseWhenScoreEmpty = true;
                    break;
                case FireGameMode.CountUp:
                default:
                    startingScore = 0f;
                    baseDecaySpeed = 0f;
                    itemDeliveredRecovery = 0f;
                    groupLeavingRecovery = 0f;
                    proximityMultiplier = 0f;
                    target = Main.PrefManager.Get<int>(Main.COUNT_UP_TARGET_ID);
                    randomFireInterval = Main.PrefManager.Get<int>(Main.COUNT_UP_RANDOM_FIRE_INTERVAL_ID);
                    loseWhenScoreEmpty = false;
                    countUpScoringMode = Enum.TryParse(Main.PrefManager.Get<string>(Main.COUNT_UP_SCORING_MODE), out UpdateFireScoreCountUp.ScoringMode scoringMode) ? scoringMode : default;
                    break;
            }
            proximityMultiplier /= 100f;
            bool randomFireActive = randomFireInterval != -1;
            bool isEndless = gameMode != FireGameMode.CountDown || target < 1;

            Set<SFireGameActive>();
            Set(new SFireScore()
            {
                Score = startingScore,
                BaseDecaySpeed = baseDecaySpeed,
                ItemDeliveredRecovery = itemDeliveredRecovery,
                GroupLeavingRecovery = groupLeavingRecovery,
                CustomerProximityMultiplier = proximityMultiplier,
                LoseWhenScoreEmpty = loseWhenScoreEmpty,
                GameMode = gameMode,
                FrameMultiplier = 0f,
                Endless = isEndless,

                RandomFireActive = randomFireActive,
                RandomFireInterval = randomFireInterval,

                CountUpScoringMode = countUpScoringMode,

                Target = target
            });

            EntityManager.DestroyEntity(Resets);
        }

        public static void Reset()
        {
            if (Session.CurrentGameNetworkMode != GameNetworkMode.Host || GameInfo.CurrentScene != SceneType.Kitchen || !GameInfo.IsPreparationTime || _instance == null)
            {
                return;
            }
            Main.LogWarning("FIRE GAME RESET");
            _instance.Set<SReset>();
        }
    }
}
