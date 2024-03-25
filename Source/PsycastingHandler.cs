using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace BetterAutocastVPE;

using static Helpers.EnchantHelper;
using static Helpers.MendHelper;
using static Helpers.PawnHelper;
using static Helpers.WeatherHelper;

internal static class PsycastingHandler
{
    private static readonly ReadOnlyDictionary<string, Func<Pawn, Ability, bool>> abilityHandlers =
        new(
            // TODO: Probably sort these more sensibly than alphabetically
            // Or allow configuring priorities
            new Dictionary<string, Func<Pawn, Ability, bool>>
            {
                { "VPE_AdrenalineRush", HandleSelfBuff },
                { "VPE_BladeFocus", HandleSelfBuff },
                { "VPE_ControlledFrenzy", HandleSelfBuff },
                { "VPE_Darkvision", HandleDarkVision },
                { "VPE_Eclipse", HandleEclipse },
                { "VPE_EnchantQuality", HandleEnchant },
                { "VPE_FiringFocus", HandleSelfBuff },
                { "VPE_GuidedShot", HandleSelfBuff },
                { "VPE_Mend", HandleMend },
                { "VPE_PsychicGuidance", HandlePsychicGuidance },
                { "VPE_SpeedBoost", HandleSelfBuff },
                { "VPE_StealVitality", HandleStealVitality },
                { "VPE_WordofJoy", HandleWordOfJoy },
                { "VPE_WordofProductivity", HandleWordOfProductivity },
                { "VPE_WordofSerenity", HandleWordOfSerenity },
                { "VPEP_BrainLeech", HandleBrainLeech },
            }
        );

    internal static bool HasHandler(string abilityDefName)
    {
        return abilityHandlers.ContainsKey(abilityDefName);
    }

    internal static bool GetsCastWhileDrafted(string abilityDefName)
    {
        return BetterAutocastVPE.Settings.DraftedAutocastDefs.Contains(abilityDefName);
    }

    internal static bool GetsCastWhileUndrafted(string abilityDefName)
    {
        return BetterAutocastVPE.Settings.UndraftedAutocastDefs.Contains(abilityDefName);
    }
    internal static bool HandleAbility(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (
            (__instance.Drafted && GetsCastWhileUndrafted(ability.def.defName))
            || (!__instance.Drafted && GetsCastWhileDrafted(ability.def.defName))
        )
        {
            return abilityHandlers[ability.def.defName](__instance, ability);
        }

        return false;
    }

    private static bool HandleSelfBuff(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        // note, this method only works right if the buff hediff defName and the ability hediff defName are the same
        if (PawnHasHediff(__instance, ability.def.defName))
            return false;
        else
            return CastAbilityOnTarget(ability, __instance);
    }

    private static bool HandleStealVitality(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (PawnHasHediff(__instance, "VPE_GainedVitality"))
            return false;

        IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, ability.GetRangeForPawn());

        return (
                BetterAutocastVPE.Settings.StealVitalityFromPrisoners
                && CastAbilityOnTarget(ability, GetHighestSensitivity(GetPrisoners(pawnsInRange)))
            )
            || (
                BetterAutocastVPE.Settings.StealVitalityFromSlaves
                && CastAbilityOnTarget(ability, GetHighestSensitivity(GetSlaves(pawnsInRange)))
            )
            || (
                BetterAutocastVPE.Settings.StealVitalityFromColonists
                && CastAbilityOnTarget(ability, GetHighestSensitivity(GetColonists(pawnsInRange)))
            )
            || (
                BetterAutocastVPE.Settings.StealVitalityFromVisitors
                && CastAbilityOnTarget(ability, GetHighestSensitivity(GetVisitors(pawnsInRange)))
            );
    }

    private static bool HandlePsychicGuidance(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        float range = ability.GetRangeForPawn();
        IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
        IEnumerable<Pawn> eligiblePawns = GetColonists(GetPawnsNotDown(pawnsInRange))
            .Where(pawn => !PawnHasHediff(pawn, "VPE_PsychicGuidance"));

        return eligiblePawns.FirstOrDefault() is Pawn target
            && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleDarkVision(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (!PawnHasHediff(__instance, "VPE_Darkvision"))
        {
            return CastAbilityOnTarget(ability, __instance);
        }

        Pawn target = GetColonists(
                GetPawnsNotDown(GetPawnsInRange(__instance, ability.GetRangeForPawn()))
            )
            .FirstOrDefault(pawn => !PawnHasHediff(pawn, "VPE_Darkvision"));

        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleEclipse(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return !EclipseOnMap(__instance.Map) && CastAbilityOnTarget(ability, __instance);
    }

    private static bool HandleBrainLeech(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (PawnHasHediff(__instance, "VPEP_Leeching"))
        {
            return false;
        }

        List<Pawn> pawnsInRange = GetPawnsInRange(__instance, ability.GetRangeForPawn()).ToList();
        Pawn? target = null;

        if (BetterAutocastVPE.Settings.BrainLeechTargetPrisoners)
        {
            GetPrisoners(pawnsInRange).TryRandomElement(out target);
        }
        if (BetterAutocastVPE.Settings.BrainLeechTargetSlaves && target is null)
        {
            GetSlaves(pawnsInRange).TryRandomElement(out target);
        }

        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleWordOfSerenity(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        float range = ability.GetRangeForPawn();
        IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
        IEnumerable<Pawn> pawnsWithMentalBreak = GetPawnsWithMentalBreak(pawnsInRange);
        IEnumerable<Pawn> notDownColonists = GetColonists(GetPawnsNotDown(pawnsWithMentalBreak));

        Pawn target = notDownColonists.FirstOrDefault();
        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleWordOfProductivity(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        float range = ability.GetRangeForPawn();
        IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
        IEnumerable<Pawn> pawnsWithoutHediff = GetPawnsWithoutHediff(
            pawnsInRange,
            "VPE_Productivity"
        );
        IEnumerable<Pawn> eligibleColonists = GetColonists(GetPawnsNotDown(pawnsWithoutHediff));

        Pawn target = eligibleColonists.FirstOrDefault();
        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleWordOfJoy(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        float range = ability.GetRangeForPawn();
        IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
        IEnumerable<Pawn> pawnsWithoutHediff = GetPawnsWithoutHediff(pawnsInRange, "Joyfuzz");
        IEnumerable<Pawn> notDownColonists = GetColonists(GetPawnsNotDown(pawnsWithoutHediff));
        IEnumerable<Pawn> lowJoyPawns = GetLowJoyPawns(notDownColonists);

        Pawn target = lowJoyPawns.FirstOrDefault();
        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleMend(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return (BetterAutocastVPE.Settings.MendPawns && HandleMendByPawn(__instance, ability))
            || (BetterAutocastVPE.Settings.MendInStockpile && HandleMendByZone(__instance, ability))
            || (
                BetterAutocastVPE.Settings.MendInStorage && HandleMendByStorage(__instance, ability)
            );
    }

    private static bool HandleMendByPawn(Pawn __instance, Ability ability)
    {
        float range = ability.GetRangeForPawn();
        IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
        IEnumerable<Pawn> colonistPawns = GetColonists(pawnsInRange);

        Pawn? target = GetRandomPawnWithDamagedEquipment(colonistPawns);
        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleMendByZone(Pawn __instance, Ability ability)
    {
        Thing? target = GetRandomDamagedThingInStockpile(__instance.Map);

        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleMendByStorage(Pawn __instance, Ability ability)
    {
        Thing? target = GetRandomDamagedThingInStorage(__instance.Map);

        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleEnchant(Pawn __instance, Ability ability)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return (
                BetterAutocastVPE.Settings.EnchantInStockpile
                && HandleEnchantByZone(__instance, ability)
            )
            || (
                BetterAutocastVPE.Settings.EnchantInStorage
                && HandleEnchantByStorage(__instance, ability)
            );
    }

    private static bool HandleEnchantByZone(Pawn __instance, Ability ability)
    {
        Thing? target = GetRandomEnchantableThingInStockpile(__instance.Map, ability);

        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleEnchantByStorage(Pawn __instance, Ability ability)
    {
        Thing? target = GetRandomEnchantableThingInStorage(__instance.Map, ability);

        return target != null && CastAbilityOnTarget(ability, target);
    }

    private static bool CastAbilityOnTarget(Ability ability, Thing target)
    {
        if (target == null || ability == null)
            return false;

        ability.CreateCastJob(new GlobalTargetInfo(target));
        return true;
    }
}
