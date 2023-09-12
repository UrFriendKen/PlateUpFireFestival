using Kitchen;
using KitchenMods;
using System;

namespace FireEvent
{
    public class ResetWhenCreateKitchen : RestaurantInitialisationSystem, IModSystem
    {
        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            if (Has<SInitialized>())
                return;
            Set<SReset>();
            Set<SInitialized>();
        }
    }
}
