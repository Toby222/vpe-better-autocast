using HarmonyLib;
using Verse;

namespace BetterAutocastVPE.Patches;

[HarmonyPatch(typeof(AreaManager), nameof(AreaManager.AddStartingAreas))]
internal static class AddAutocastAreas
{
    [HarmonyPostfix]
    internal static void Postfix(AreaManager __instance)
    {
        BetterAutocastVPE.DebugLog(
            "Adding autocasting areas - currently " + __instance.AllAreas.Count + " areas"
        );
        __instance.AllAreas.Add(new Area_IceCrystal(__instance));
        __instance.AllAreas.Add(new Area_SolarPinhole(__instance));
        BetterAutocastVPE.DebugLog(
            "Added autocasting areas - currently " + __instance.AllAreas.Count + " areas"
        );
    }
}
