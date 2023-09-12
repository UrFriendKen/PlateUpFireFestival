using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using System.Collections.Generic;

namespace FireEvent.Customs
{
    public class DummyPrivateDiningAppliance : CustomAppliance
    {
        public override string UniqueNameID => "dummyPrivateDiningAppliance";
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>()
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo("Private Dining", "", new List<Appliance.Section>(), new List<string>()))
        };
        public override bool IsPurchasable => false;
        public override bool IsPurchasableAsUpgrade => false;
    }
}
