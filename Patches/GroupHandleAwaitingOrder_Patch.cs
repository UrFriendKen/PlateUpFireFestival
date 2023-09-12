using HarmonyLib;
using Kitchen;
using System;
using System.Reflection;
using Unity.Entities;

namespace FireEvent.Patches
{
    [HarmonyPatch]
    static class GroupHandleAwaitingOrder_Patch
    {
        static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(GroupHandleAwaitingOrder), t => t.Name.Contains("c__DisplayClass_OnUpdate_LambdaJob0"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        static void Prefix(ref int __state, ref CGroupReward reward)
        {
            __state = reward.Amount * 2;
        }

        static void Postfix(ref EntityContext ___ctx, ref int __state, ref float ___profit, Entity e, ref CGroupReward reward, ref CPosition pos)
        {
            if (reward == 0 &&
                PatchController.HasStatic<SFireGameActive>() &&
                PatchController.IsGroupSittingAtIsolatedTable(e) &&
                (!PatchController.RequireStatic(e, out CHalloweenOrder halloweenOrder) || halloweenOrder.State != TrickTreatStates.TrickNoPayment))
            {
                PatchController.CreateMoneyPopup(__state, pos);
                MoneyTracker.AddEvent(___ctx, Main.DummyPrivateDiningAppliance?.ID ?? 0, __state);
                ___profit += __state;
            }
        }
    }
}
