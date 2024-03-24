using System;
using System.Collections.Generic;
using System.Linq;
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

        CompAbilities compAbilities = __instance.TryGetComp<CompAbilities>();
        if (compAbilities == null)
        {
            return;
        }

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
        {
            return;
        }

        if (target != null)
        {
            if (
                (
                    from x in list
                    where x.ability.AICanUseOn(target)
                    select x into ve
                    select new Tuple<Verb, float>(ve, ve.ability.Chance)
                )
                    .AddItem(new Tuple<Verb, float>(__result, 1f))
                    .TryRandomElementByWeight((Tuple<Verb, float> t) => t.Item2, out var result)
            )
            {
                __result = result.Item1;
            }
        }
        else
        {
            Verb verb = list.AddItem(__result).MaxBy((Verb ve) => ve.verbProps.range);
            __result = verb;
        }
    }

    private static bool AbilityIsBlacklisted(Ability ab)
    {
        switch (ab.def.defName)
        {
            case "VPE_StealVitality":
            case "VPEP_BrainLeech":
                return true;
            default:
                return false;
        }
    }
}
