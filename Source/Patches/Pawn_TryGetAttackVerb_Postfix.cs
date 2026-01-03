using System;
using System.Collections.Generic;
using System.Linq;
using BetterAutocastVPE.Helpers;
using HarmonyLib;
using Verse;
#if v1_5
using VFECore.Abilities;
#else
using VEF.Abilities;
#endif

namespace BetterAutocastVPE.Patches;

// Modified from Vanilla Expanded Framework's code
[HarmonyBefore("legodude17.mvcf")]
[HarmonyPatch(typeof(Pawn), nameof(Pawn.TryGetAttackVerb))]
internal static class Pawn_TryGetAttackVerb_Postfix
{
    [HarmonyPostfix]
    internal static void Postfix(Pawn __instance, ref Verb __result, Thing target)
    {
        ref var _result = ref __result;
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));

        if (__instance.TryGetComp<CompAbilities>() is not CompAbilities compAbilities)
            return;

        IEnumerable<Verb_CastAbility> castableVerbs = compAbilities
            .LearnedAbilities.Where(ability =>
                ability.AutoCast
                && !AbilityIsBlacklisted(ability)
                && ability.IsEnabledForPawn(out string _)
                && (target == null || ability.CanHitTarget(target))
            )
            .Select(ability => ability.verb);

        BetterAutocastVPE.DebugLog("Possible verbs: " + string.Join(", ", castableVerbs.Select(verb => verb?.ReportLabel.ToStringSafe())) + " _result: " + _result.ReportLabel.ToStringSafe());

        if (target is not null)
        {
            (Verb verb, float chance)? result = castableVerbs
                .Where(verbAbility => verbAbility.ability.AICanUseOn(target))
                .Select(verbAbility =>
                    (verb: (Verb?)verbAbility, chance: verbAbility.ability.Chance)
                )
                .AddItem((verb: _result, chance: 1f))
                .Where(verbChance => verbChance.verb is not null)
                .Select(verbChance => (verb: verbChance.verb!, verbChance.chance))
                .GetRandomStruct(verbChance => verbChance.chance);

            if (result?.verb is Verb castVerb)
            {
                _result = castVerb;
                BetterAutocastVPE.DebugLog(
                    $"Pawn_TryGetAttackVerb_Postfix for {__instance.NameFullColored} set verb to {_result?.ReportLabel.ToStringSafe()} targetting {target.LabelCap}"
                );
            }
        }
        else
        {
            _result = castableVerbs
                .AddItem(_result)
                .Where(verb => verb is not null)
                .MaxBy(verb => verb.verbProps.range);
            BetterAutocastVPE.DebugLog(
                $"Pawn_TryGetAttackVerb_Postfix for {__instance.NameFullColored} set verb to {_result?.ReportLabel.ToStringSafe()}"
            );
        }
    }

    private static bool AbilityIsBlacklisted(Ability ability)
    {
        // Defs that apparently need special handling
        return PsycastingHandler.abilityHandlers.ContainsKey(ability.def.defName);
    }
}
