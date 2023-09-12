using Kitchen;
using KitchenLib.References;
using KitchenMods;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Entities;
using UnityEngine;

namespace FireEvent
{
    [UpdateBefore(typeof(CreateNewKitchen))]
    [UpdateInGroup(typeof(ChangeModeGroup))]
    public class ProvideStartingEnvelopes : RestaurantSystem, IModSystem
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct SProvided : IComponentData, IModComponent
        {
        }

        protected override void Initialise()
        {
            base.Initialise();
            RequireSingletonForUpdate<SFireScore>();
        }

        protected override void OnUpdate()
        {
            if (HasSingleton<SProvided>() || !Has<SLayout>() || !TryGetSingleton(out SFireScore fireScore) || !Main.PrefManager.Get<bool>(Main.GIVE_APPLIANCES_FIRE_EXTINGUISHER))
            {
                return;
            }
            base.World.Add<SProvided>();

            List<Vector3> postTiles = GetPostTiles();
            int placed_tile = 0;
            if (!FindTile(ref placed_tile, postTiles, out var candidate))
            {
                candidate = GetFallbackTile();
            }
            if (Preferences.Get<bool>(Pref.ProvideStartingEnvelopesAsParcels))
            {
                PostHelpers.CreateApplianceParcel(base.EntityManager, candidate, ApplianceReferences.FireExtinguisherHolder);
            }
            else
            {
                PostHelpers.CreateBlueprintLetter(base.EntityManager, candidate, ApplianceReferences.FireExtinguisherHolder, 0f);
            }
        }

        public bool FindTile(ref int placed_tile, List<Vector3> floor_tiles, out Vector3 candidate)
        {
            candidate = Vector3.zero;
            bool flag = false;
            while (!flag && placed_tile < floor_tiles.Count)
            {
                candidate = floor_tiles[placed_tile++];
                if (GetOccupant(candidate) == default(Entity))
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                return false;
            }
            return true;
        }
    }
}
