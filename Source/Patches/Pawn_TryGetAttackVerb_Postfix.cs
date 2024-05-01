using System;
using System.Collections.Generic;
using System.Linq;
using BetterAutocastVPE.Helpers;
using HarmonyLib;
using Verse;
using VFECore.Abilities;

namespace BetterAutocastVPE.Patches;

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
                ability.autoCast
                && !AbilityIsBlacklisted(ability)
                && ability.IsEnabledForPawn(out string reason)
                && (target == null || ability.CanHitTarget(target))
            )
            .Select(ability => ability.verb);

        if (target is not null)
        {
            var result = castableVerbs.Where(x => x.ability.AICanUseOn(target))
                .Select(x => new Tuple<Verb, float>(x, x.ability.Chance))
                .AddItem(new Tuple<Verb, float>(__result, 1f))
                .GetRandomClass(t => t.Item2);

            if (result is not null)
            {
                __result = result.Item1;
            }
        }
        else
        {
            __result = castableVerbs.AddItem(__result).MaxBy((Verb ve) => ve.verbProps.range);
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
