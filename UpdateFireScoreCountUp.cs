using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace FireEvent
{
    [UpdateInGroup(typeof(EndOfFrameGroup))]
    public class UpdateFireScoreCountUp : RestaurantSystem, IModSystem
    {
        public enum ScoringMode
        {
            GrossEarnings,
            Savings,
            Donations
        }

        protected override void Initialise()
        {
            base.Initialise();

            RequireSingletonForUpdate<SFireScore>();
            RequireSingletonForUpdate<SFireGameActive>();
        }

        protected override void OnUpdate()
        {
            SFireScore fireScore = GetSingleton<SFireScore>();
            if (fireScore.GameMode != FireGameMode.CountUp || !Require(out SMoney money))
            {
                return;
            }

            if (Has<SIsDayFirstUpdate>())
            {
                switch (fireScore.CountUpScoringMode)
                {
                    case ScoringMode.Savings:
                        fireScore.Score = money;
                        break;
                    case ScoringMode.Donations:
                        fireScore.Score = GetOrDefault<SInsuranceSpending>().Amount;
                        break;
                }
                fireScore.StartOfDayCoins = money;
            }
            else if (Has<SIsNightFirstUpdate>())
            {
                switch (fireScore.CountUpScoringMode)
                {
                    case ScoringMode.GrossEarnings:
                    case ScoringMode.Savings:
                        fireScore.Score += money - fireScore.StartOfDayCoins;
                        break;
                }
            }
            Set(fireScore);
        }
    }
}
