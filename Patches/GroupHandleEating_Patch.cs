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
    static class GroupHandleEating_Patch
    {
        static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(GroupHandleEating), t => t.Name.Contains("c__DisplayClass_OnUpdate_LambdaJob0"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        static bool Prefix(ref DynamicBuffer<CGroupMember> members)
        {
            return !PatchController.IsAnyMemberOnFire(members);
        }
    }
}
