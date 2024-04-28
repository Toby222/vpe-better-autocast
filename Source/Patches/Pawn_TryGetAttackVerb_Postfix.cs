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
        if (__result is null)
            throw new ArgumentNullException(nameof(__result));

        if (__instance.TryGetComp<CompAbilities>() is not CompAbilities compAbilities)
            return;

        List<Verb_CastAbility> list = compAbilities
            .LearnedAbilities.Where(ability =>
                ability.autoCast
                && !AbilityIsBlacklisted(ability)
                && ability.IsEnabledForPawn(out string reason)
                && (target == null || ability.CanHitTarget(target))
            )
            .Select(ability => ability.verb)
            .ToList();
        // List<Verb_CastAbility> list = (
        //     from ab in compAbilities.LearnedAbilities
        //     where
        //         ab.Autocast
        //         && !AbilityIsBlacklisted(ab)
        //         && ab.IsEnabledForPawn(out reason)
        //         && (target == null || ab.CanHitTarget(target))
        //     select ab.verb
        // ).ToList();
        if (list.Count == 0)
            return;

        if (target is not null)
        {
            var result = list.Where(x => x.ability.AICanUseOn(target))
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
            __result = list.AddItem(__result).MaxBy((Verb ve) => ve.verbProps.range);
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
