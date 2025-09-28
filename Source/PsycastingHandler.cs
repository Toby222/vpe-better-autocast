using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BetterAutocastVPE.Helpers;
using RimWorld;
using RimWorld.Planet;
using VanillaPsycastsExpanded;
using VanillaPsycastsExpanded.Technomancer;
using Verse;
using VanillaPsycastsExpanded.Nightstalker;

#if v1_5
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
#else
using VEF.Abilities;
using Ability = VEF.Abilities.Ability;
#endif

namespace BetterAutocastVPE;

using static Helpers.AreaHelper;
using static Helpers.EnchantHelper;
using static Helpers.MendHelper;
using static Helpers.PawnHelper;
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
                { "VPE_CraftTimeskip", HandleCraftTimeskip },
                { "VPE_Darkvision", HandleDarkvision },
                { "VPE_Deathshield", HandleDeathshield },
                { "VPE_Eclipse", HandleEclipse },
                { "VPE_EnchantQuality", HandleEnchant },
                { "VPE_Enthrall", HandleEnthrall },
                { "VPE_FireShield", HandleFireShield },
                { "VPE_FiringFocus", HandleSelfBuff },
                { "VPE_Focus", HandleFocus },
                { "VPE_Ghostwalk", HandleSelfBuff },
                { "VPE_GuidedShot", HandleSelfBuff },
                { "VPE_IceCrystal", HandleIceCrystal },
                { "VPE_IceShield", HandleIceShield },
                { "VPE_Invisibility", HandleInvisibility },
                { "VPE_Mend", HandleMend },
                { "VPE_Overshield", HandleOvershield },
                { "VPE_Power", HandlePower },
                { "VPE_PsychicGuidance", HandleColonistBuff },
                { "VPE_SolarPinhole", HandleSolarPinhole },
                { "VPE_SolarPinholeSunlamp", HandleSolarPinhole },
                { "VPE_SootheFemale", HandleSoothe },
                { "VPE_SootheMale", HandleSoothe },
                { "VPE_SpeedBoost", HandleSelfBuff },
                { "VPE_StaticAura", HandleStaticAura },
                { "VPE_StealVitality", HandleStealVitality },
                { "VPE_WordofAlliance", HandleWordOfAlliance },
                { "VPE_WordofImmunity", HandleWordOfImmunity },
                { "VPE_WordofJoy", HandleWordOfJoy },
                { "VPE_WordofProductivity", HandleColonistBuff },
                { "VPE_WordofSerenity", HandleWordOfSerenity },
                { "VPEP_BrainLeech", HandleBrainLeech },
                { "VPER_Etch_Runecircle_Greater", HandleEtchRunecircle },
                { "VPER_Etch_Runecircle", HandleEtchRunecircle },
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
    private static bool CastAbilityOnTarget(Ability ability, GlobalTargetInfo target)
    {
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));
        if (ability.def.targetCount > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ability),
                "Can't autocast with multiple targets"
            );
        }

        AbilityTargetingMode targetMode = ability.def.targetModes[0];

#if DEBUG
        const bool showMessages = true;
#else
        bool showMessages = BetterAutocastVPE.Settings.ShowValidationMessages;
#endif

        bool validated;

        if (targetMode == AbilityTargetingMode.Self)
        {
            BetterAutocastVPE.DebugLog(ability.def.defName + " validating self");
            // Self is not checked for validity, it's always fair game.
            validated = true;
        }
        else if (targetMode == AbilityTargetingMode.Random)
        {
            if (ability.targetParams.canTargetLocations)
            {
                BetterAutocastVPE.DebugLog(ability.def.defName + " validating target tile");
                validated = ability.ValidateTargetTile(target, showMessages);
            }
            else
            {
                BetterAutocastVPE.DebugLog(ability.def.defName + " validating target");
                validated = ability.ValidateTarget((LocalTargetInfo)target, showMessages);
            }
        }
        else if (ability.def.worldTargeting)
        {
            BetterAutocastVPE.DebugLog(ability.def.defName + " validating target tile");
            validated = ability.ValidateTargetTile(target, showMessages);
        }
        else
        {
            BetterAutocastVPE.DebugLog(ability.def.defName + " validating target");
            // Is this right?
            validated = ability.ValidateTarget((LocalTargetInfo)target, showMessages);
        }

        if (!validated)
        {
#if DEBUG
            BetterAutocastVPE.Error(
#else
            BetterAutocastVPE.Warn(
#endif
                $"<b><i>Please report this</i></b> - Failed to validate {ability.def.defName} for {ability.pawn.NameFullColored} on {target.Label} ({target}) - this is most likely harmless but means that I implemented something wrong."
            );
            return false;
        }
        else
        {
#if DEBUG
            BetterAutocastVPE.DebugLog(
                $"Validated cast of {ability.def.defName} for {ability.pawn.NameFullColored} on {target.Label} ({target})"
            );
#endif
        }

        ability.CreateCastJob(target);
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

        BetterAutocastVPE.DebugLog(
            $"Checking autocast of {ability.def.defName} for {pawn.NameFullColored}"
        );
        if (
            (!pawn.Drafted && GetsCastWhileUndrafted(ability.def.defName))
            || (pawn.Drafted && GetsCastWhileDrafted(ability.def.defName))
        )
        {
            BetterAutocastVPE.DebugLog(
                $"Autocasting {ability.def.defName} for {pawn.NameFullColored}"
            );
            bool wasAutocast = false;
            try
            {
                wasAutocast = abilityHandlers[ability.def.defName](pawn, ability);
            }
            catch (Exception ex)
            {
                BetterAutocastVPE.Error(
                    $"Exception while trying to autocast {ability.def.defName} of {pawn.NameFullColored}:\n{ex}\n{ex.StackTrace}"
                );
            }

            if (wasAutocast)
            {
                BetterAutocastVPE.DebugLog(
                    $"{pawn.NameFullColored} autocast {ability.def.defName} - previous job: {jobBeforeCast}"
                );
            }
            return wasAutocast;
        }

        return false;
    }
    #endregion worker functions

    #region handlers
    #region generic
    private static bool HandleSelfBuff(Pawn pawn, Ability ability) =>
        HandleHediffPsycast(pawn, ability, [TargetType.Self], FinalTargetType.Random, false);

    private static bool HandleColonistBuff(Pawn pawn, Ability ability) =>
        HandleHediffPsycast(pawn, ability, [TargetType.Colonists], FinalTargetType.Random, false);

    private enum TargetType
    {
        Self,
        Colonists,
        ColonyAnimals,
        Prisoners,
        Slaves,
        Visitors,
        WildAnimals,
    }

    private enum FinalTargetType
    {
        Random,
        Closest,
        MostPsychicallySensitive,
    }

    private static bool HandleTargetedPsycast(
        Pawn pawn,
        Ability ability,
        TargetType[] targetPriority,
        FinalTargetType finalTarget,
        Func<Pawn, bool> targetValidator,
        bool allowDowned
    )
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        Thing? target = null;

        float range = ability.GetRangeForPawn();
        IEnumerable<Pawn> pawnsInRange = pawn.GetPawnsInRange(range)
            .Where(targetValidator)
            .PsychicallySensitive();

        if (!allowDowned)
            pawnsInRange = pawnsInRange.NotDown();

        Pawn[] validPawnsInRange = pawnsInRange.ToArray();

        foreach (TargetType targetType in targetPriority)
        {
            IEnumerable<Pawn>? targets = targetType switch
            {
                TargetType.Self => targetValidator(pawn) ? [pawn] : [],
                TargetType.Colonists => validPawnsInRange.Colonists(),
                TargetType.ColonyAnimals => validPawnsInRange.ColonyAnimals(),
                TargetType.Prisoners => validPawnsInRange.Prisoners(),
                TargetType.Slaves => validPawnsInRange.Slaves(),
                TargetType.Visitors => validPawnsInRange.Visitors(),
                TargetType.WildAnimals => validPawnsInRange.WildAnimals(),
#if DEBUG
                _ => throw new NotImplementedException(),
#else
                _ => [],
#endif
            };

            target = finalTarget switch
            {
                FinalTargetType.Random => targets.GetRandomClass(),
                FinalTargetType.Closest => targets.ClosestTo(pawn),
                FinalTargetType.MostPsychicallySensitive => targets.HighestSensitivity(),
#if DEBUG
                _ => throw new NotImplementedException(),
#else
                _ => null,
#endif
            };

            if (target is not null)
                break;
        }

        return target is not null && CastAbilityOnTarget(ability, target);
    }

    private static readonly Dictionary<string, string> HediffPsycastCache = [];

    private static bool HandleHediffPsycast(
        Pawn pawn,
        Ability ability,
        TargetType[] targetPriority,
        FinalTargetType finalTarget,
        bool allowDowned,
        string? hediffDefName = null,
        Func<Pawn, bool>? extraValidator = null
    )
    {
        if (
            hediffDefName is null
            && !HediffPsycastCache.TryGetValue(ability.def.defName, out hediffDefName)
        )
        {
            AbilityExtension_Hediff? hediffExtension =
                ability.def.GetModExtensionCached<AbilityExtension_Hediff>();

            if (hediffExtension is null)
            {
                BetterAutocastVPE.Warn(
                    $"Ability {ability.def.defName} does not have an {nameof(AbilityExtension_Hediff)} for handling HediffPsycast; falling back to ability defName. Please report this issue!"
                );
                hediffDefName ??= ability.def.defName;
            }
            else
            {
                hediffDefName = hediffExtension.hediff.defName;
            }
            HediffPsycastCache[ability.def.defName] = hediffDefName;
        }

        return HandleTargetedPsycast(
            pawn,
            ability,
            targetPriority,
            finalTarget,
            extraValidator is null
                ? pawn => pawn.DoesNotHaveHediff(hediffDefName)
                : pawn => pawn.DoesNotHaveHediff(hediffDefName) && extraValidator(pawn),
            allowDowned
        );
    }

    #endregion generic

    #region Protector
    private static bool HandleInvisibility(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(2);

        if (BetterAutocastVPE.Settings.InvisibilityTargetSelf)
            targets.Add(TargetType.Self);
        if (BetterAutocastVPE.Settings.InvisibilityTargetColonists)
            targets.Add(TargetType.Colonists);

        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            false
        );
    }

    private static bool HandleOvershield(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(2);

        if (BetterAutocastVPE.Settings.InvisibilityTargetSelf)
            targets.Add(TargetType.Self);
        if (BetterAutocastVPE.Settings.InvisibilityTargetColonists)
            targets.Add(TargetType.Colonists);

        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            false
        );
    }

    private static bool HandleWordOfImmunity(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(5);
        if (BetterAutocastVPE.Settings.WordOfImmunityTargetColonists)
            targets.Add(TargetType.Colonists);
        if (BetterAutocastVPE.Settings.WordOfImmunityTargetColonyAnimals)
            targets.Add(TargetType.ColonyAnimals);
        if (BetterAutocastVPE.Settings.WordOfImmunityTargetSlaves)
            targets.Add(TargetType.Slaves);
        if (BetterAutocastVPE.Settings.WordOfImmunityTargetPrisoners)
            targets.Add(TargetType.Prisoners);
        if (BetterAutocastVPE.Settings.WordOfImmunityTargetVisitors)
            targets.Add(TargetType.Visitors);

        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            true,
            extraValidator: PawnHelper.Immunizable
        );
    }

    private static bool HandleFocus(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(3);
        if (BetterAutocastVPE.Settings.FocusTargetSelf)
            targets.Add(TargetType.Self);
        if (BetterAutocastVPE.Settings.FocusTargetColonists)
            targets.Add(TargetType.Colonists);
        if (BetterAutocastVPE.Settings.FocusTargetSlaves)
            targets.Add(TargetType.Slaves);

        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            false
        );
    }
    #endregion Protector

    #region Necropath
    private static bool HandleStealVitality(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (pawn.HasHediff("VPE_GainedVitality"))
            return false;

        List<TargetType> targets = new(4);
        if (BetterAutocastVPE.Settings.StealVitalityFromPrisoners)
            targets.Add(TargetType.Prisoners);
        if (BetterAutocastVPE.Settings.StealVitalityFromSlaves)
            targets.Add(TargetType.Slaves);
        if (BetterAutocastVPE.Settings.StealVitalityFromColonists)
            targets.Add(TargetType.Colonists);
        if (BetterAutocastVPE.Settings.StealVitalityFromVisitors)
            targets.Add(TargetType.Visitors);
        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.MostPsychicallySensitive,
            true,
            "VPE_LostVitality"
        );
    }

    private static bool HandleDeathshield(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(5);
        if (BetterAutocastVPE.Settings.DeathshieldColonists)
            targets.Add(TargetType.Colonists);
        if (BetterAutocastVPE.Settings.DeathshieldColonyAnimals)
            targets.Add(TargetType.ColonyAnimals);
        if (BetterAutocastVPE.Settings.DeathshieldSlaves)
            targets.Add(TargetType.Slaves);
        if (BetterAutocastVPE.Settings.DeathshieldPrisoners)
            targets.Add(TargetType.Prisoners);
        if (BetterAutocastVPE.Settings.DeathshieldVisitors)
            targets.Add(TargetType.Visitors);

        return HandleTargetedPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            pawn =>
                pawn.health.hediffSet.GetFirstHediff<Hediff_Deathshield>()
                    is not Hediff_Deathshield shield
                || shield.TryGetComp<HediffComp_Disappears>()
                    is not HediffComp_Disappears disappears
                ||
                // TODO: Probably make this timeout configurable
                disappears.ticksToDisappear <= (GenDate.TicksPerHour * 4),
            true
        );
    }

    private static bool HandleEnthrall(Pawn pawn, Ability ability)
    {
        if (BetterAutocastVPE.Settings.EnthrallInStockpile && HandleEnthrallByZone(pawn, ability))
            return true;
        if (BetterAutocastVPE.Settings.EnthrallInStorage && HandleEnthrallByStorage(pawn, ability))
            return true;

        return false;
    }

    #region Necropath helpers
    private static bool HandleEnthrallByZone(Pawn pawn, Ability ability)
    {
        Map map = pawn.MapHeld;
        Corpse? target = (
            BetterAutocastVPE.Settings.EnthrallOnlyNamedStockpiles
                ? map.GetThingsInNamedStockpile("thrall")
                : map.GetThingsInAllStockpiles()
        )
            .OfType<Corpse>()
            .ClosestTo(pawn);

        return target is not null && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleEnthrallByStorage(Pawn pawn, Ability ability)
    {
        Map map = pawn.MapHeld;
        Corpse? target = (
            BetterAutocastVPE.Settings.EnthrallOnlyNamedStockpiles
                ? map.GetThingsInNamedStorageGroup("thrall")
                : map.GetThingsInStorage()
        )
            .OfType<Corpse>()
            .ClosestTo(pawn);

        return target is not null && CastAbilityOnTarget(ability, target);
    }
    #endregion Necropath helpers
    #endregion Necropath

    #region Nightstalker
    private static bool HandleDarkvision(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(2);
        if (BetterAutocastVPE.Settings.DarkvisionTargetSelf)
            targets.Add(TargetType.Self);
        if (BetterAutocastVPE.Settings.DarkvisionTargetColonists)
            targets.Add(TargetType.Colonists);

        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            false
        );
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

        if (pawn.HasHediff("VPEP_Leeching"))
            return false;

        List<TargetType> targets = new(2);
        if (BetterAutocastVPE.Settings.BrainLeechTargetPrisoners)
            targets.Add(TargetType.Prisoners);
        if (BetterAutocastVPE.Settings.BrainLeechTargetSlaves)
            targets.Add(TargetType.Slaves);

        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.MostPsychicallySensitive,
            true,
            "VPEP_BrainLeech"
        );
    }
    #endregion Puppeteer

    #region Empath
    private static bool HandleWordOfJoy(Pawn pawn, Ability ability) =>
        HandleHediffPsycast(
            pawn,
            ability,
            [TargetType.Colonists],
            FinalTargetType.Closest,
            false,
            extraValidator: PawnHelper.LowJoy
        );

    private static bool HandleWordOfSerenity(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(6);

        if (BetterAutocastVPE.Settings.WordOfSerenityTargetColonists)
            targets.Add(TargetType.Colonists);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetColonyAnimals)
            targets.Add(TargetType.ColonyAnimals);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetWildAnimals)
            targets.Add(TargetType.WildAnimals);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetSlaves)
            targets.Add(TargetType.Slaves);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetPrisoners)
            targets.Add(TargetType.Prisoners);
        if (BetterAutocastVPE.Settings.WordOfSerenityTargetVisitors)
            targets.Add(TargetType.Visitors);

        return HandleTargetedPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            pawn =>
                pawn.MentalState is not null
                && !BetterAutocastVPE.Settings.WordOfSerenityIgnoredMentalStateDefs.Contains(
                    pawn.MentalStateDef.defName
                ),
            false
        );
    }

    private static bool HandleSoothe(Pawn pawn, Ability ability)
    {
        Gender gender = ability
            .def.GetModExtensionCached<AbilityExtension_CastPsychicSoothe>()!
            .gender;
        BetterAutocastVPE.DebugLog(ability.def.defName + " - Gender: " + gender);
        Pawn[] validTargets = pawn
            .MapHeld.mapPawns.AllPawnsSpawned.Where(mapPawn =>
                !mapPawn.Dead
                && mapPawn.gender == gender
                && mapPawn.DoesNotHaveHediff("VPE_PsychicSoothe")
                && mapPawn.needs.mood is Need_Mood mood
                && (
                    (
                        BetterAutocastVPE.Settings.SootheColonistsCheck
                        && mapPawn.IsColonist
                        && mood.CurInstantLevelPercentage
                            <= BetterAutocastVPE.Settings.SootheColonistsMaximumMood
                    )
                    || (
                        BetterAutocastVPE.Settings.SootheSlavesCheck
                        && mapPawn.IsSlaveOfColony
                        && mood.CurInstantLevelPercentage
                            <= BetterAutocastVPE.Settings.SootheSlavesMaximumMood
                    )
                    || (
                        BetterAutocastVPE.Settings.SoothePrisonersCheck
                        && mapPawn.IsPrisonerOfColony
                        && mood.CurInstantLevelPercentage
                            <= BetterAutocastVPE.Settings.SoothePrisonersMaximumMood
                    )
                    || (
                        BetterAutocastVPE.Settings.SootheVisitorsCheck
                        && !mapPawn.HostileTo(pawn)
                        && mood.CurInstantLevelPercentage
                            <= BetterAutocastVPE.Settings.SootheVisitorsMaximumMood
                    )
                )
            )
            .ToArray();

        BetterAutocastVPE.DebugLog("validTargets - " + validTargets.ToStringSafeEnumerable());

        return validTargets.Length > 0 && CastAbilityOnTarget(ability, pawn);
    }
    #endregion Empath

    #region Technomancer
    private static bool HandleMend(Pawn pawn, Ability ability)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if ((BetterAutocastVPE.Settings.MendPawns || BetterAutocastVPE.Settings.MendMechs) && HandleMendByPawn(pawn, ability))
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

    private static bool HandlePower(Pawn pawn, Ability ability)
    {
        BetterAutocastVPE.DebugLog("HandlePower");
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (!(BetterAutocastVPE.Settings.PowerMechs || BetterAutocastVPE.Settings.PowerBuildings))
            return false;

        IEnumerable<Thing> poweredThings = pawn.MapHeld.spawnedThings.Where(thing =>
            (
                BetterAutocastVPE.Settings.PowerBuildings
                && thing.TryGetComp<CompPowerTrader>() is { PowerOutput: < 0f }
            )
            || (
                BetterAutocastVPE.Settings.PowerMechs
                && thing is Pawn { needs.energy: not null } mech
                && mech.IsMechAlly(pawn)
            )
        );
        // No need to sort if just using the randomly-targeted mechs
        if (BetterAutocastVPE.Settings.PowerBuildings)
        {
            poweredThings = poweredThings.OrderByDescending(thing =>
                thing.TryGetComp<CompPowerTrader>()?.PowerOutput ?? float.NegativeInfinity
            );
        }

        return poweredThings.FirstOrDefault() is Thing target
            && CastAbilityOnTarget(ability, target);
    }

    #region Technomancer helpers
    private static bool HandleMendByPawn(Pawn pawn, Ability ability)
    {
        return pawn.GetPawnsInRange(ability.GetRangeForPawn())
                .Where(targetPawn =>
                    (
                        BetterAutocastVPE.Settings.MendPawns
                        && targetPawn.RaceProps.Humanlike
                        && targetPawn.Faction == pawn.Faction
                        && targetPawn.HasDamagedEquipment()
                    )
                    || (
                        BetterAutocastVPE.Settings.MendMechs
                        && targetPawn.RaceProps.IsMechanoid
                        && targetPawn.IsMechAlly(pawn)
                        && MechRepairUtility.CanRepair(targetPawn)
                    )
                )
                .GetRandomClass()
                is Pawn target
            && CastAbilityOnTarget(ability, target);
    }

    private static bool HandleMendByZone(Pawn pawn, Ability ability)
    {
        return GetRandomAllowedDamagedThingInStockpile(pawn.Map, pawn, ability) is Thing target
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
        return GetRandomEnchantableThingInStorage(pawn.MapHeld, ability) is Thing target
            && CastAbilityOnTarget(ability, target);
    }
    #endregion Technomancer helpers
    #endregion Technomancer

    #region Conflagrator

    private static bool HandleFireShield(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(4);
        if (BetterAutocastVPE.Settings.FireShieldTargetSelf)
            targets.Add(TargetType.Self);
        if (BetterAutocastVPE.Settings.FireShieldTargetColonists)
            targets.Add(TargetType.Colonists);
        if (BetterAutocastVPE.Settings.FireShieldTargetSlaves)
            targets.Add(TargetType.Slaves);
        if (BetterAutocastVPE.Settings.FireShieldTargetVisitors)
            targets.Add(TargetType.Visitors);

        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            false
        );
    }

    #endregion Conflagrator

    #region Staticlord

    private static bool HandleStaticAura(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(4);
        if (BetterAutocastVPE.Settings.StaticAuraTargetSelf)
            targets.Add(TargetType.Self);
        if (BetterAutocastVPE.Settings.StaticAuraTargetColonists)
            targets.Add(TargetType.Colonists);
        if (BetterAutocastVPE.Settings.StaticAuraTargetSlaves)
            targets.Add(TargetType.Slaves);
        if (BetterAutocastVPE.Settings.StaticAuraTargetVisitors)
            targets.Add(TargetType.Visitors);

        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            false
        );
    }

    #endregion

    #region Frostshaper
    private static bool HandleIceCrystal(Pawn pawn, Ability ability)
    {
        return GetRandomValidCellInArea<Area_IceCrystal>(
                pawn.MapHeld,
                cell => cell.GetFirstBuilding(pawn.MapHeld) is null
            )
                is IntVec3 target
            && CastAbilityOnTarget(ability, new GlobalTargetInfo(target, ability.pawn.MapHeld));
    }

    private static bool HandleIceShield(Pawn pawn, Ability ability)
    {
        List<TargetType> targets = new(4);
        if (BetterAutocastVPE.Settings.IceShieldTargetSelf)
            targets.Add(TargetType.Self);
        if (BetterAutocastVPE.Settings.IceShieldTargetColonists)
            targets.Add(TargetType.Colonists);
        if (BetterAutocastVPE.Settings.IceShieldTargetSlaves)
            targets.Add(TargetType.Slaves);
        if (BetterAutocastVPE.Settings.IceShieldTargetVisitors)
            targets.Add(TargetType.Visitors);

        return HandleHediffPsycast(
            pawn,
            ability,
            targets.ToArray(),
            FinalTargetType.Closest,
            false
        );
    }
    #endregion Frostshaper

    #region Skipmaster
    private static bool HandleSolarPinhole(Pawn pawn, Ability ability)
    {
        bool allowedOnBuildings =
            ability.def.GetModExtensionCached<AbilityExtension_Spawn>()?.allowOnBuildings != false;
        IntVec3? maybeTarget = GetRandomValidCellInArea<Area_SolarPinhole>(
            pawn.MapHeld,
            cell =>
            {
                return !cell.Filled(pawn.Map)
                    && (allowedOnBuildings || cell.GetFirstBuilding(pawn.Map) is null)
                    && !pawn
                        .MapHeld.thingGrid.ThingsListAtFast(cell)
                        .Any(thing => thing.def.defName is "SolarPinhole" or "SolarPinholeSunlamp");
            }
        );
        BetterAutocastVPE.DebugLog(
            $"HandleSolarPinhole({pawn.NameFullColored}, {ability.def.defName}) -> ({maybeTarget.ToStringSafe()})"
        );
        return maybeTarget is IntVec3 target
            && CastAbilityOnTarget(ability, new GlobalTargetInfo(target, ability.pawn.MapHeld));
    }
    #endregion Skipmaster

    #region Chronopath
    private static bool HandleCraftTimeskip(Pawn pawn, Ability ability)
    {
        return ThingsInArea<Area_CraftTimeskip>(pawn.MapHeld)
                .OfType<UnfinishedThing>()
                .Where(thing => !thing.IsForbidden(pawn))
                .ClosestTo(pawn)
                is Thing target
            && CastAbilityOnTarget(ability, target);
    }
    #endregion Chronopath

    #region Archon
    private static bool HandleWordOfAlliance(Pawn pawn, Ability ability)
    {
        Pawn? target = pawn
            .MapHeld.mapPawns.AllPawnsSpawned.Where(targetPawn =>
                targetPawn.Faction is Faction targetFaction
                && targetFaction != pawn.Faction
                && targetFaction.CanChangeGoodwillFor(pawn.Faction, 1)
                && (
                    !BetterAutocastVPE.Settings.WordOfAllianceCheckAllowedArea
                    || pawn.Drafted
                    || (
                        pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap
                            is Area allowedArea
                        && allowedArea[targetPawn.Position]
                    )
                )
                && targetFaction.PlayerGoodwill
                    < BetterAutocastVPE.Settings.WordOfAllianceGoodwill.max
                && targetFaction.PlayerGoodwill
                    >= BetterAutocastVPE.Settings.WordOfAllianceGoodwill.min
            )
            .OrderBy(mapPawn => mapPawn.Faction.PlayerGoodwill)
            .ThenByDescending(mapPawn => mapPawn.Position.DistanceTo(mapPawn.Position))
            .FirstOrFallback();
        return target is not null && CastAbilityOnTarget(ability, target);
    }
    #endregion Archon

    #region Runesmith
    private static bool HandleEtchRunecircle(Pawn pawn, Ability ability)
    {
        BetterAutocastVPE.DebugLog(
            $"HandleEtchRunecircle({pawn.NameFullColored}, {ability.def.defName})"
        );
        bool validator(IntVec3 cell)
        {
            return !cell.Filled(pawn.Map)
                && cell.GetFirstBuilding(pawn.Map) is null
                && !pawn
                    .MapHeld.thingGrid.ThingsListAtFast(cell)
                    .Any(thing =>
                        thing.def.defName
                            is "VPER_Runecircle_Constructible"
                                or "VPER_Runecircle_Standard"
                                or "VPER_Runecircle_Greater"
                    );
        }

        IntVec3? maybeTarget;
        switch (ability.def.defName)
        {
            case "VPER_Etch_Runecircle":
                maybeTarget = GetRandomValidCellInArea<Area_Runecircle>(pawn.MapHeld, validator);
                break;
            case "VPER_Etch_Runecircle_Greater":
                maybeTarget = GetRandomValidCellInArea<Area_GreaterRunecircle>(
                    pawn.MapHeld,
                    validator
                );
                break;
            default:
                BetterAutocastVPE.Error(
                    $"Unknown psycast {ability.def.defName} in HandleEtchRunecircle"
                );
                maybeTarget = null;
                break;
        }

        string thingsattarget = maybeTarget is IntVec3 target_
            ? string.Join(", ", pawn.MapHeld.thingGrid.ThingsListAtFast(target_))
            : "no target";
        BetterAutocastVPE.DebugLog(
            $"{maybeTarget?.GetFirstBuilding(pawn.Map).ToStringSafe()}, {thingsattarget}"
        );

        BetterAutocastVPE.DebugLog(
            $"HandleEtchRunecircle({pawn.NameFullColored}, {ability.def.defName}) -> ({maybeTarget.ToStringSafe()})"
        );
        return maybeTarget is IntVec3 target
            && CastAbilityOnTarget(ability, new GlobalTargetInfo(target, ability.pawn.MapHeld));
    }
    #endregion Runesmith
    #endregion handlers
}
