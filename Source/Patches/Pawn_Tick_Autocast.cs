using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

#if v1_5
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
#else
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;
#endif

namespace BetterAutocastVPE.Patches;

using static Helpers.PawnHelper;

#if v1_5
[HarmonyPatch(typeof(Pawn), nameof(Pawn.Tick))]
#else
[HarmonyPatch(typeof(Pawn), "Tick")]
#endif
internal static class Pawn_Tick_Autocast
{
    [HarmonyPostfix]
    internal static void Postfix(Pawn __instance)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (BetterAutocastVPE.Settings is not { } settings)
            throw new Exception("Settings are not initialized yet for some reason?");

        int interval = __instance.Drafted
            ? BetterAutocastVPE.Settings.AutocastIntervalDrafted
            : BetterAutocastVPE.Settings.AutocastIntervalUndrafted;
        if (__instance.HashOffsetTicks() % interval == 0)
        {
            ProcessAbilities(__instance);
        }
    }

    private static void ProcessAbilities(Pawn pawn)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));

        if (!pawn.IsColonistPlayerControlled)
            return;

        if (!pawn.CanPsycast())
            return;

        if (pawn.GetComp<CompAbilities>()?.LearnedAbilities is not List<Ability> abilities)
            return;

        foreach (
            Ability ability in abilities.OrderByDescending(ability =>
                ability.def.defName is "VPE_SolarPinholeSunlamp"
            )
        )
        {
            if (!ability.autoCast)
                continue;
            if (!ability.IsEnabledForPawn(out _))
                continue;
            if (PsycastingHandler.HandleAbility(pawn, ability))
                break;
        }
    }
}
