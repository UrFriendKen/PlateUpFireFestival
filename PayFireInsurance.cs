using Kitchen;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace FireEvent
{
    [UpdateInGroup(typeof(EndOfFrameGroup), OrderFirst = true)]
    public class PayFireInsurance : StartOfDaySystem, IModSystem
    {
        protected override void Initialise()
        {
            base.Initialise();
            RequireSingletonForUpdate<SFireScore>();
            RequireSingletonForUpdate<SFireGameActive>();
        }

        protected override void OnUpdate()
        {
            float percent = Main.PrefManager.Get<float>(Main.MONEY_MODIFIERS_CHARITY_DONATION);
            percent = Mathf.Clamp01(percent);
            if (!(percent > 0f) || !Require(out SMoney sMoney))
            {
                return;
            }

            int insuranceAmount = Mathf.FloorToInt(sMoney.Amount * percent);
            SInsuranceSpending insuranceSpending = GetOrDefault<SInsuranceSpending>();
            insuranceSpending.Amount += insuranceAmount;
            sMoney.Amount = sMoney.Amount - insuranceAmount;
            Set(sMoney);
            Set(insuranceSpending);
        }
    }
}
