using HarmonyLib;
using Verse;

namespace BetterAutocastVPE.Patches;

[HarmonyPatch(typeof(AreaManager), nameof(AreaManager.ExposeData))]
internal static class AreaManager_ExposeData_Postfix
{
    [HarmonyPostfix]
    internal static void Postfix(AreaManager __instance)
    {
        if (__instance.Get<Area_IceCrystal>() is null)
            __instance.AllAreas.Add(new Area_IceCrystal(__instance));
    }
}
