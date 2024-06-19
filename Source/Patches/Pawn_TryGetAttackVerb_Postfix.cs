using System;
using System.Collections.Generic;
using System.Linq;
using BetterAutocastVPE.Helpers;
using HarmonyLib;
using Verse;
using VFECore.Abilities;

namespace BetterAutocastVPE.Patches;

// Modified from Vanilla Expanded Framework's code
[HarmonyBefore("legodude17.mvcf")]
[HarmonyPatch(typeof(Pawn), nameof(Pawn.TryGetAttackVerb))]
internal static class Pawn_TryGetAttackVerb_Postfix
{
    [HarmonyPostfix]
    internal static void Postfix(Pawn __instance, ref Verb __result, Thing target)
    {
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

        if (target is not null)
        {
            (Verb, float)? result = castableVerbs
                .Where(x => x.ability.AICanUseOn(target))
                .Select(x => (x as Verb, x.ability.Chance))
                .AddItem((__result, 1f))
                .GetRandomStruct(t => t.Item2);

            if (result?.Item1 is Verb castVerb)
            {
                __result = castVerb;
            }
        }
        else
        {
            __result = castableVerbs.AddItem(__result).MaxBy(verb => verb.verbProps.range);
        }
    }

    private static bool AbilityIsBlacklisted(Ability ab)
    {
        return ab.def.defName switch
        {
            "VPE_StealVitality" or "VPEP_BrainLeech" => true,
            _ => false,
        };
    }
}
