using HarmonyLib;
using Kitchen;
using System;
using System.Reflection;
using Unity.Entities;

namespace FireEvent.Patches
{
    [HarmonyPatch]
    static class GroupHandleStartLeaving_Patch
    {
        static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(GroupHandleStartLeaving), t => t.Name.Contains("c__DisplayClass_OnUpdate_LambdaJob0"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        static void Postfix(ref DynamicBuffer<CGroupMember> members)
        {
            PatchController.GroupStartLeavingRestoreFireScore(members);
        }
    }
}
