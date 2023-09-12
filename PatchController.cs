using Kitchen;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

namespace FireEvent
{
    public class PatchController : GenericSystemBase, IModSystem
    {
        private static PatchController _instance;

        private EntityQuery Tables;

        private static HashSet<int> _isolatedTables = new HashSet<int>();

        private static Queue<(int, Vector3)> _moneyPopupRequests = new Queue<(int, Vector3)>();

        protected override void Initialise()
        {
            base.Initialise();
            _instance = this;
            Tables = GetEntityQuery(typeof(CTableSet), typeof(CPosition));
        }

        protected override void OnUpdate()
        {
            int frontDoorRoomID = GetTile(GetFrontDoor(false)).RoomID;

            Dictionary<int, int> isolatedTablesByRoom = new Dictionary<int, int>();
            HashSet<int> processedRooms = new HashSet<int>();
            using NativeArray<Entity> tableEntities = Tables.ToEntityArray(Allocator.Temp);
            using NativeArray<CTableSet> tableSets = Tables.ToComponentDataArray<CTableSet>(Allocator.Temp);
            using NativeArray<CPosition> tablePositions = Tables.ToComponentDataArray<CPosition>(Allocator.Temp);
            for (int i = 0; i < tableEntities.Length; i++)
            {
                Entity e = tableEntities[i];
                CTableSet tableSet = tableSets[i];
                CPosition position = tablePositions[i];

                if (tableSets[i].ChairCount > 0)
                {
                    CLayoutRoomTile tile = GetTile(position);
                    if (tile.RoomID != 0 && tile.RoomID != frontDoorRoomID)
                    {
                        if (!processedRooms.Contains(tile.RoomID))
                        {
                            processedRooms.Add(tile.RoomID);
                            isolatedTablesByRoom.Add(tile.RoomID, e.Index);
                        }
                        else
                        {
                            isolatedTablesByRoom.Remove(tile.RoomID);
                        }
                    }
                }
            }
            _isolatedTables = isolatedTablesByRoom.Values.Distinct().ToHashSet();

            if (_moneyPopupRequests.Count > 0)
            {
                (int amount, Vector3 pos) moneyPopupRequest = _moneyPopupRequests.Dequeue();

                Entity popupEntity = EntityManager.CreateEntity();
                Set(popupEntity, new CMoneyPopup
                {
                    Change = moneyPopupRequest.amount
                });
                Set(popupEntity, new CPosition(moneyPopupRequest.pos));
                Set(popupEntity, new CLifetime(1f));
                Set(popupEntity, new CRequiresView()
                {
                    Type = ViewType.MoneyPopup
                });
            }
        }

        internal static bool RequireStatic<T>(Entity e, out T comp) where T : struct, IComponentData
        {
            comp = default;
            if (_instance == null)
                return false;
            return _instance.Require(e, out comp);
        }

        internal static bool HasStatic<T>(Entity e) where T : struct, IComponentData
        {
            if (_instance == null)
                return false;
            return _instance.Has<T>(e);
        }

        internal static bool HasStatic<T>() where T : struct, IComponentData
        {
            if (_instance == null)
                return false;
            return _instance.Has<T>();
        }

        internal static void GroupReceiveItemRestoreFireScore(Entity e, in DynamicBuffer<CWaitingForItem> orders)
        {
            if (_instance == null)
            {
                return;
            }

            EntityContext ctx = new EntityContext(_instance.EntityManager);

            if (!ctx.RequireBuffer(e, out DynamicBuffer<CFireScoreRestored> restores))
            {
                restores = ctx.AddBuffer<CFireScoreRestored>(e);
            }

            List<Entity> restoredEntities = new List<Entity>();
            for (int i = 0; i < restores.Length; i++)
            {
                restoredEntities.Add(restores[i].Item);
            }

            for (int i = 0; i < orders.Length; i++)
            {
                CWaitingForItem order = orders[i];

                if (order.Satisfied && !restoredEntities.Contains(order.Item))
                {
                    restores.Add(new CFireScoreRestored()
                    {
                        Item = order.Item
                    });
                    ctx.Set(ctx.CreateEntity(), new CItemDeliveredAddFireScore());
                }
            }
        }

        internal static void GroupStartLeavingRestoreFireScore(DynamicBuffer<CGroupMember> members)
        {
            if (_instance == null)
            {
                return;
            }

            EntityContext ctx = new EntityContext(_instance.EntityManager);
            ctx.Set(ctx.CreateEntity(), new CGroupLeavingAddFireScore()
            {
                Count = members.Length
            });
        }

        internal static bool IsAnyMemberOnFire(DynamicBuffer<CGroupMember> members)
        {
            if (_instance == null)
                return false;

            for (int i = 0; i < members.Length; i++)
            {
                Entity entity = members[i].Customer;
                if (_instance.Has<CGroupMemberOnFire>(entity))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsGroupSittingAtIsolatedTable(Entity group)
        {
            Main.LogInfo("Group sitting isolated?");
            if (_instance == null ||
                !_instance.Require(group, out CAssignedTable assignedTable) ||
                !_isolatedTables.Contains(assignedTable.Table.Index))
                return false;
            Main.LogInfo("Yes");
            return true;
        }

        internal static void CreateMoneyPopup(int amount, CPosition pos)
        {
            _moneyPopupRequests.Enqueue((amount, pos + Vector3.up * 0.5f));
        }
    }
}
