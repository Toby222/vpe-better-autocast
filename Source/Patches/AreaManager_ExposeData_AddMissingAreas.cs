using HarmonyLib;
using Verse;

namespace BetterAutocastVPE.Patches;

[HarmonyPatch(typeof(AreaManager), nameof(AreaManager.ExposeData))]
internal static class AddMissingAreas
{
    [HarmonyPostfix]
    internal static void Postfix(AreaManager __instance)
    {
        if (__instance.Get<Area_IceCrystal>() is null)
            __instance.AllAreas.Add(new Area_IceCrystal(__instance));
        if (__instance.Get<Area_SolarPinhole>() is null)
            __instance.AllAreas.Add(new Area_SolarPinhole(__instance));
        if (__instance.Get<Area_CraftTimeskip>() is null)
            __instance.AllAreas.Add(new Area_CraftTimeskip(__instance));
        if (__instance.Get<Area_Runecircle>() is null)
            __instance.AllAreas.Add(new Area_Runecircle(__instance));
        if (__instance.Get<Area_GreaterRunecircle>() is null)
            __instance.AllAreas.Add(new Area_GreaterRunecircle(__instance));
    }
}
