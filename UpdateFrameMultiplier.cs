using Kitchen;
using Kitchen.Layouts;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FireEvent
{
    public class UpdateFrameMultiplier : RestaurantSystem, IModSystem
    {
        EntityQuery Fires;
        EntityQuery Chairs;

        protected override void Initialise()
        {
            base.Initialise();
            Fires = GetEntityQuery(new QueryHelper()
                .All(typeof(CIsOnFire), typeof(CPosition))
                .None(typeof(CFireImmune)));

            Chairs = GetEntityQuery(new QueryHelper()
                .All(typeof(CApplianceChair), typeof(CPosition)));

            RequireSingletonForUpdate<SFireScore>();
            RequireSingletonForUpdate<SFireGameActive>();
        }

        protected override void OnUpdate()
        {
            if (!Require(out SFireScore fireScore))
                return;

            if (!Has<SIsDayTime>())
            {
                fireScore.FrameMultiplier = 0f;
                Set(fireScore);
                return;
            }

            using NativeArray<CPosition> firePositions = Fires.ToComponentDataArray<CPosition>(Allocator.Temp);

            using NativeArray<CApplianceChair> chairAppliances = Chairs.ToComponentDataArray<CApplianceChair>(Allocator.Temp);
            using NativeArray<CPosition> chairPositions = Chairs.ToComponentDataArray<CPosition>(Allocator.Temp);

            Dictionary<Vector3, int> occupiedChairPositionsRooms = new Dictionary<Vector3, int>();

            for (int i = 0; i < chairAppliances.Length; i++)
            {
                CApplianceChair chair = chairAppliances[i];
                CPosition chairPos = chairPositions[i];
                if (!chair.IsInUse)
                    continue;

                if (!occupiedChairPositionsRooms.ContainsKey(chairPos))
                {
                    occupiedChairPositionsRooms.Add(chairPos.Position, GetRoom(chairPos));
                }
            }

            float frameMultiplier = 0f;
            foreach (CPosition firePosition in firePositions)
            {
                int fireRoom = GetRoom(firePosition);

                bool isNearbyCustomer = false;
                if (occupiedChairPositionsRooms.ContainsKey(firePosition))
                    isNearbyCustomer = true;

                if (!isNearbyCustomer)
                {
                    foreach (Vector3 chairPosIntersect in LayoutHelpers.AllNearbyRange2.Select(layoutPos => new Vector3(layoutPos.x, 0f, layoutPos.y) + firePosition).Intersect(occupiedChairPositionsRooms.Keys))
                    {
                        if (!occupiedChairPositionsRooms.TryGetValue(chairPosIntersect, out int chairRoom) || fireRoom != chairRoom)
                            continue;
                        isNearbyCustomer = true;
                        break;
                    }
                }

                frameMultiplier += isNearbyCustomer ? fireScore.CustomerProximityMultiplier : 1f;
            }

            fireScore.FrameMultiplier = frameMultiplier;
            Set(fireScore);
        }
    }
}
