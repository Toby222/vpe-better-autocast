using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;
using BetterAutocastVPE.Settings;

namespace BetterAutocastVPE.Patches;

using static Helpers.PawnHelper;

[HarmonyPatch(typeof(CompAbilities), nameof(CompAbilities.CompTick))]
internal static class Pawn_Tick_Autocast
{
    [HarmonyPostfix]
    internal static void Postfix(CompAbilities __instance)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (__instance.Pawn is null)
            throw new ArgumentNullException(nameof(__instance.Pawn));

        if (BetterAutocastVPE.Settings is not AutocastSettings settings)
            throw new Exception("Settings are not initialized yet for some reason?");

        int interval = __instance.Pawn.Drafted
            ? settings.AutocastIntervalDrafted
            : settings.AutocastIntervalUndrafted;
        if (__instance.Pawn.HashOffsetTicks() % interval == 0)
        {
            ProcessAbilities(__instance);
        }
    }

    private static void ProcessAbilities(CompAbilities comp)
    {
        Pawn pawn = comp.Pawn;
        if (!pawn.IsColonistPlayerControlled)
            return;

        if (!pawn.CanPsycast())
            return;

        if (pawn.GetComp<CompAbilities>()?.LearnedAbilities is not List<Ability> abilities)
            return;

        BetterAutocastVPE.DebugLog(
            $"Trying to autocast for {pawn.NameFullColored}{(pawn.CurJob is null ? string.Empty : " " + pawn.GetJobReport())}"
        );

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
