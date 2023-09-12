using Kitchen;
using KitchenCardsManager;
using KitchenLib.References;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace FireEvent
{
    [UpdateBefore(typeof(CreateNewKitchen))]
    [UpdateInGroup(typeof(ChangeModeGroup))]
    public class ProvideStartingCards : RestaurantSystem, IModSystem
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct SProvided : IComponentData, IModComponent
        {
        }

        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            if (HasSingleton<SProvided>() || !Has<SLayout>())
            {
                return;
            }

            HashSet<int> requestedCards = new HashSet<int>();

            Main.LogInfo("Providing Starting Cards...");
            if (Main.PrefManager.Get<bool>(Main.AUTO_CARDS_TIPPING_CULTURE))
            {
                CardsManagerUtil.AddProgressionUnlockToRun(UnlockCardReferences.TippingCulture);
                requestedCards.Add(UnlockCardReferences.TippingCulture);
                Main.LogInfo($"Requested Tipping Culture ({UnlockCardReferences.TippingCulture})");
            }
            if (Main.PrefManager.Get<bool>(Main.AUTO_CARDS_INSTANT_SERVICE))
            {
                CardsManagerUtil.AddProgressionUnlockToRun(UnlockCardReferences.InstantOrders);
                requestedCards.Add(UnlockCardReferences.InstantOrders);
                Main.LogInfo($"Requested Instant Service ({UnlockCardReferences.InstantOrders})");
            }

            if (GameInfo.AllCurrentCards.Select(x => x.CardID).Intersect(requestedCards).Count() == requestedCards.Count)
            {
                Main.LogInfo($"All requested cards successfully provided.");
                Set<SProvided>();
            }


        }
    }
}
