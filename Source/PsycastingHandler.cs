using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace BetterAutocastVPE;

using static Helpers.EnchantHelper;
using static Helpers.MendHelper;
using static Helpers.PawnHelper;
using static Helpers.ThingHelper;
using static Helpers.WeatherHelper;

internal static class PsycastingHandler
{
    #region private members
    internal static readonly ReadOnlyDictionary<string, Func<Pawn, Ability, bool>> abilityHandlers =
        new(
            // TODO: Probably sort these more sensibly than alphabetically
            // Or allow configuring priorities
            new Dictionary<string, Func<Pawn, Ability, bool>>
            {
                { "VPE_AdrenalineRush", HandleSelfBuff },
                { "VPE_BladeFocus", HandleSelfBuff },
                { "VPE_ControlledFrenzy", HandleSelfBuff },
                { "VPE_Darkvision", HandleDarkvision },
                { "VPE_Deathshield", HandleDeathshield },
                { "VPE_Eclipse", HandleEclipse },
                { "VPE_EnchantQuality", HandleEnchant },
                { "VPE_FiringFocus", HandleSelfBuff },
                { "VPE_GuidedShot", HandleSelfBuff },
                { "VPE_Invisibility", HandleInvisibility },
                { "VPE_Mend", HandleMend },
                { "VPE_PsychicGuidance", HandlePsychicGuidance },
                { "VPE_SpeedBoost", HandleSelfBuff },
                { "VPE_StealVitality", HandleStealVitality },
                { "VPE_WordofImmunity", HandleWordOfImmunity },
                { "VPE_WordofJoy", HandleWordOfJoy },
                { "VPE_WordofProductivity", HandleWordOfProductivity },
                { "VPE_WordofSerenity", HandleWordOfSerenity },
                { "VPEP_BrainLeech", HandleBrainLeech },
            }
        );
    #endregion private members

    #region helper functions
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

    /// <summary>
    /// Tries to create a job to cast the given ability on the given target
    /// </summary>
    /// <returns>If a job was successfully created</returns>
    private static bool CastAbilityOnTarget(Ability ability, Thing target)
    {
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        ability.CreateCastJob(new GlobalTargetInfo(target));
        return true;
    }
    #endregion helper functions

    #region worker functions
    /// <summary>
    /// Tries to auto-cast the ability
    /// </summary>
    /// <returns>If the ability was successfullly autocast</returns>
    internal static bool HandleAbility(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        string? jobBeforeCast = pawn.CurJobDef?.defName ?? "<none>";

        if (
            (!pawn.Drafted && GetsCastWhileUndrafted(ability.def.defName))
            || (pawn.Drafted && GetsCastWhileDrafted(ability.def.defName))
        )
        {
            bool wasAutocast = abilityHandlers[ability.def.defName](pawn, ability);
            if (wasAutocast)
            {
                BetterAutocastVPE.DebugLog(
                    $"{pawn.Name} autocast {ability.def.defName} - previous job: {jobBeforeCast}"
                );
            }
            return wasAutocast;
        }

        return false;
    }
    #endregion worker functions

    #region handlers
    #region generic
    private static bool HandleSelfBuff(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        // note, this method only works right if the buff hediff defName and the ability hediff defName are the same
        if (pawn.HasHediff(ability.def.defName))
            return false;
        else
            return CastAbilityOnTarget(ability, pawn);
    }
    #endregion generic

    #region Protector
    private static bool HandleInvisibility(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        Pawn? target = null;

        if (
            BetterAutocastVPE.Settings.InvisibilityTargetSelf
            && !pawn.HasHediff("PsychicInvisibility")
        )
        {
            target ??= pawn;
        }

        if (BetterAutocastVPE.Settings.InvisibilityTargetColonists && target is null)
        {
            float range = ability.GetRangeForPawn();
            target ??= pawn.GetPawnsInRange(range)
                .WithoutHediff("PsychicInvisibility")
                .PsychicallySensitive()
                .NotDown()
                .Colonists()
                .ClosestTo(pawn);
        }

        return target is not null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleWordOfImmunity(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        float range = ability.GetRangeForPawn();
        Pawn[] eligiblePawns = pawn.GetPawnsInRange(range)
            .WithoutHediff("VPE_Immunity")
            .Immunizable()
            .ToArray();

        Pawn? target = null;

        if (BetterAutocastVPE.Settings.WordOfImmunityTargetColonists)
            target ??= eligiblePawns.Colonists().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.WordOfImmunityTargetColonyAnimals)
            target ??= eligiblePawns.ColonyAnimals().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.WordOfImmunityTargetSlaves)
            target ??= eligiblePawns.Slaves().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.WordOfImmunityTargetPrisoners)
            target ??= eligiblePawns.Prisoners().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.WordOfImmunityTargetVisitors)
            target ??= eligiblePawns.Visitors().ClosestTo(pawn);

        return target is not null && CastAbilityOnTarget(ability, target);
    }
    #endregion Protector

    #region Necropath
    private static bool HandleStealVitality(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (pawn.HasHediff("VPE_GainedVitality"))
            return false;

        Pawn[] eligiblePawns = pawn.GetPawnsInRange(ability.GetRangeForPawn())
            .PsychicallySensitive()
            .ToArray();

        Pawn? target = null;

        if (BetterAutocastVPE.Settings.StealVitalityFromPrisoners)
            target ??= eligiblePawns.Prisoners().HighestSensitivity();
        if (BetterAutocastVPE.Settings.StealVitalityFromSlaves)
            target ??= eligiblePawns.Slaves().HighestSensitivity();
        if (BetterAutocastVPE.Settings.StealVitalityFromColonists)
            target ??= eligiblePawns.Colonists().HighestSensitivity();
        if (BetterAutocastVPE.Settings.StealVitalityFromVisitors)
            target ??= eligiblePawns.Visitors().HighestSensitivity();

        return target is not null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleDeathshield(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        Pawn? target = null;

        Pawn[] pawnsInRange = pawn.GetPawnsInRange(ability.GetRangeForPawn())
            .WithoutHediff("VPE_DeathShield")
            .ToArray();

        if (BetterAutocastVPE.Settings.DeathshieldColonists)
            target ??= pawnsInRange.Colonists().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.DeathshieldColonyAnimals)
            target ??= pawnsInRange.ColonyAnimals().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.DeathshieldSlaves)
            target ??= pawnsInRange.Slaves().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.DeathshieldPrisoners)
            target ??= pawnsInRange.Prisoners().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.DeathshieldVisitors)
            target ??= pawnsInRange.Visitors().ClosestTo(pawn);

        return target is not null && CastAbilityOnTarget(ability, target);
    }
    #endregion Necropath

    #region Harmonist
    private static bool HandlePsychicGuidance(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return pawn.GetPawnsInRange((float)ability.GetRangeForPawn())
                .NotDown()
                .PsychicallySensitive()
                .Colonists()
                .WithoutHediff("VPE_PsychicGuidance")
                .GetRandomElement(weightSelector: null)
                is Pawn target
            && CastAbilityOnTarget(ability, target);
    }
    #endregion Harmonist

    #region Nightstalker
    private static bool HandleDarkvision(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        Pawn? target = null;

        if (BetterAutocastVPE.Settings.DarkvisionTargetSelf && !pawn.HasHediff("VPE_Darkvision"))
            target ??= pawn;

        if (BetterAutocastVPE.Settings.DarkvisionTargetColonists && target is null)
        {
            target ??= pawn.GetPawnsInRange(ability.GetRangeForPawn())
                .NotDown()
                .PsychicallySensitive()
                .Colonists()
                .WithoutHediff("VPE_Darkvision")
                .ClosestTo(pawn);
        }

        return target is not null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleEclipse(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return !EclipseOnMap(pawn.Map) && CastAbilityOnTarget(ability, pawn);
    }
    #endregion Nightstalker

    #region Puppeteer
    private static bool HandleBrainLeech(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (pawn.HasHediff("VPEP_Leeching"))
            return false;

        Pawn[] eligiblePawns = pawn.GetPawnsInRange(ability.GetRangeForPawn())
            .PsychicallySensitive()
            .ToArray();
        Pawn? target = null;

        if (BetterAutocastVPE.Settings.BrainLeechTargetPrisoners && target is null)
            target = eligiblePawns.Prisoners().RandomElementWithFallback();
        if (BetterAutocastVPE.Settings.BrainLeechTargetSlaves && target is null)
            target = eligiblePawns.Slaves().RandomElementWithFallback();

        return target is not null && CastAbilityOnTarget(ability, target);
    }
    #endregion Puppeteer

    #region Empath
    private static bool HandleWordOfSerenity(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        float range = ability.GetRangeForPawn();
        IEnumerable<Pawn> eligiblePawns = pawn.GetPawnsInRange(range);

        if (!BetterAutocastVPE.Settings.WordOfSerenityTargetScaria)
            eligiblePawns = eligiblePawns.WithoutHediff(HediffDefOf.Scaria.defName);

        eligiblePawns = eligiblePawns
            .WithMentalBreak()
            .PsychicallySensitive()
            .Where(pawn =>
                !BetterAutocastVPE.Settings.WordOfSerenityIgnoredMentalStateDefs.Contains(
                    pawn.MentalStateDef.defName
                )
            );

        Pawn? target = null;
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetColonists && target is null)
            target = eligiblePawns.Colonists().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetColonyAnimals && target is null)
            target = eligiblePawns.ColonyAnimals().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetWildAnimals && target is null)
            target = eligiblePawns.WildAnimals().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetSlaves && target is null)
            target = eligiblePawns.Slaves().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetPrisoners && target is null)
            target = eligiblePawns.Prisoners().ClosestTo(pawn);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetVisitors && target is null)
            target = eligiblePawns.Visitors().ClosestTo(pawn);

        return target is not null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleWordOfJoy(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        float range = ability.GetRangeForPawn();

        Pawn? target = pawn.GetPawnsInRange(range)
            .WithoutHediff("Joyfuzz")
            .PsychicallySensitive()
            .Colonists()
            .NotDown()
            .LowJoy()
            .RandomElementWithFallback();

        return target is not null && CastAbilityOnTarget(ability, target);
    }
    #endregion Empath

    #region Archon
    private static bool HandleWordOfProductivity(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        float range = ability.GetRangeForPawn();

        Pawn? target = pawn.GetPawnsInRange(range)
            .WithoutHediff("VPE_Productivity")
            .NotDown()
            .Colonists()
            .RandomElementWithFallback();

        return target is not null && CastAbilityOnTarget(ability, target);
    }
    #endregion

    #region Technomancer
    private static bool HandleMend(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (BetterAutocastVPE.Settings.MendPawns && HandleMendByPawn(pawn, ability))
            return true;
        if (BetterAutocastVPE.Settings.MendInStockpile && HandleMendByZone(pawn, ability))
            return true;
        if (BetterAutocastVPE.Settings.MendInStorage && HandleMendByStorage(pawn, ability))
            return true;
        return false;
    }

    private static bool HandleEnchant(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (BetterAutocastVPE.Settings.EnchantInStockpile && HandleEnchantByZone(pawn, ability))
            return true;
        if (BetterAutocastVPE.Settings.EnchantInStorage && HandleEnchantByStorage(pawn, ability))
            return true;
        return false;
    }

    #region Technomancer helpers
    private static bool HandleMendByPawn(Pawn pawn, Ability ability)
    {
        return pawn.GetPawnsInRange(ability.GetRangeForPawn())
                .Colonists()
                .WithDamagedEquipment()
                .RandomElementWithFallback()
                is Pawn target
            && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleMendByZone(Pawn pawn, Ability ability)
    {
        return GetRandomAllowedDamagedThingInStockpile(pawn.Map, pawn) is Thing target
            && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleMendByStorage(Pawn pawn, Ability ability)
    {
        return GetRandomAllowedDamagedThingInStorage(pawn.Map, pawn) is Thing target
            && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleEnchantByZone(Pawn pawn, Ability ability)
    {
        return GetRandomEnchantableThingInStockpile(pawn.Map, ability) is Thing target
            && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleEnchantByStorage(Pawn pawn, Ability ability)
    {
        return GetRandomEnchantableThingInStorage(pawn.Map, ability) is Thing target
            && CastAbilityOnTarget(ability, target);
    }
    #endregion Technomancer helpers
    #endregion Technomancer
    #endregion handlers
}
