using System;
using VFECore.Abilities;

namespace BetterAutocastVPE.Patches;

// No Harmony attributes, patched manually since the same patch needs to be applied to multiple classes' getters
internal static class CanAutoCast_EnableHandledPsycasts
{
    internal static void Postfix(Ability __instance, ref bool __result)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));

        if (!__result)
            __result = PsycastingHandler.HasHandler(__instance.def.defName);
    }
}
