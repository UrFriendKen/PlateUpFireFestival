using HarmonyLib;
using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace FireEvent.Patches
{
    [HarmonyPatch]
    static class GroupReceiveItem_Patch
    {
        static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(GroupReceiveItem), t => t.Name.Contains("c__DisplayClass_OnUpdate_LambdaJob0"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        static void Postfix(Entity e, ref DynamicBuffer<CWaitingForItem> orders)
        {
            PatchController.GroupReceiveItemRestoreFireScore(e, in orders);
        }
    }
}
