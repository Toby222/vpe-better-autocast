using HarmonyLib;
using VFECore.Abilities;

namespace VPEAutoCastBuffs
{
    [HarmonyPatch(typeof(Ability), "CanAutoCast", MethodType.Getter)]
    public static class Ability_CanAutoCast_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Ability __instance, ref bool __result)
        {
            if (__instance.def.defName == "VPE_PsychicGuidance"
                || __instance.def.defName == "VPE_EnchantQuality"
                || __instance.def.defName == "VPE_Mend"
                || __instance.def.defName == "VPE_WordofProductivity"
                || __instance.def.defName == "VPE_WordofSerenity"
                || __instance.def.defName == "VPE_WordofJoy")
            {
                __result = true;
            }
        }
    }
}