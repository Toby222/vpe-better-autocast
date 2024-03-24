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

        if (
            __instance.def.defName == "VPE_PsychicGuidance"
            || __instance.def.defName == "VPE_EnchantQuality"
            || __instance.def.defName == "VPE_Mend"
            || __instance.def.defName == "VPE_WordofProductivity"
            || __instance.def.defName == "VPE_WordofSerenity"
            || __instance.def.defName == "VPE_WordofJoy"
        )
        {
            __result = true;
        }
    }
}
