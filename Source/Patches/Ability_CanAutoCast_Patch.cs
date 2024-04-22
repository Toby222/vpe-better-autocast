using System;
using HarmonyLib;
using VFECore.Abilities;

namespace BetterAutocastVPE.Patches;

[HarmonyPatch(typeof(Ability), nameof(Ability.CanAutoCast), MethodType.Getter)]
internal static class Ability_CanAutocast_Patch
{
    [HarmonyPostfix]
    internal static void Postfix(Ability __instance, ref bool __result)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));

        if (!__result)
            __result = PsycastingHandler.HasHandler(__instance.def.defName);
    }
}

[HarmonyPatch(
    typeof(Ability_SpawnBuilding),
    nameof(Ability_SpawnBuilding.CanAutoCast),
    MethodType.Getter
)]
internal static class Ability_SpawnBuilding_CanAutocast_Patch
{
    [HarmonyPostfix]
    internal static void Postfix(Ability_SpawnBuilding __instance, ref bool __result)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));

        if (!__result)
            __result = PsycastingHandler.HasHandler(__instance.def.defName);
    }
}
