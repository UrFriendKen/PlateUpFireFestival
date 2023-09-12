using HarmonyLib;
using Kitchen;

namespace FireEvent.Patches
{
    [HarmonyPatch]
    static class CustomersOnFireLosePatience_Patch
    {
        [HarmonyPatch(typeof(CustomersOnFireLosePatience), "OnUpdate")]
        [HarmonyPrefix]
        static bool OnUpdate_Prefix()
        {
            return false;
        }
    }
}
