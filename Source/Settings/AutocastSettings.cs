using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Verse;

namespace BetterAutocastVPE.Settings;

public class AutocastSettings : ModSettings
{
    private static HashSet<string> DefaultDraftedAutocastDefs() =>
        [
            "VPE_AdrenalineRush",
            "VPE_BladeFocus",
            "VPE_ControlledFrenzy",
            "VPE_Deathshield",
            "VPE_FireShield",
            "VPE_FiringFocus",
            "VPE_Ghostwalk",
            "VPE_GuidedShot",
            "VPE_IceShield",
            "VPE_Invisibility",
            "VPE_Overshield",
            "VPE_SpeedBoost",
            "VPE_StaticAura",
        ];

    private static HashSet<string> DefaultUndraftedAutocastDefs() =>
        [
            "VPE_CraftTimeskip",
            "VPE_Darkvision",
            "VPE_Deathshield",
            "VPE_Eclipse",
            "VPE_EnchantQuality",
            "VPE_Enthrall",
            "VPE_Focus",
            "VPE_Ghostwalk",
            "VPE_IceCrystal",
            "VPE_Mend",
            "VPE_Power",
            "VPE_PsychicGuidance",
            "VPE_SolarPinhole",
            "VPE_SolarPinholeSunlamp",
            "VPE_SootheFemale",
            "VPE_SootheMale",
            "VPE_SpeedBoost",
            "VPE_StealVitality",
            "VPE_WordofAlliance",
            "VPE_WordofImmunity",
            "VPE_WordofInspiration",
            "VPE_WordofJoy",
            "VPE_WordofProductivity",
            "VPE_WordofSerenity",
            "VPEP_BrainLeech",
            "VPER_Etch_Runecircle_Greater",
            "VPER_Etch_Runecircle",
        ];

    private static HashSet<string> DefaultBlockedJobDefs() =>
        [
            "AttackMelee",
            "BetterAutocastVPE_GotoLocationAndCastAbilityOnce",
            "DeliverToBed",
            "Goto",
            "Ingest",
            "LayDown",
            "SpectateCeremony",
            "VFEA_GotoTargetAndUseAbility",
            "VFEA_UseAbility",
            "Wait_Asleep",
            "Wait_Downed",
        ];

    private static HashSet<string> DefaultWordOfSerenityIgnoredMentalStateDefs() =>
        ["BerserkTrance", "BerserkWarcall", "Crying", "Giggling"];

    public AutocastSettings()
    {
        BlockedJobDefs = DefaultBlockedJobDefs();
        DraftedAutocastDefs = DefaultDraftedAutocastDefs();
        UndraftedAutocastDefs = DefaultUndraftedAutocastDefs();
        WordOfSerenityIgnoredMentalStateDefs = DefaultWordOfSerenityIgnoredMentalStateDefs();
        Reset();
    }

    public bool DebugLog;
    public bool ShowValidationMessages;
    public int AutocastIntervalDrafted;
    public int AutocastIntervalUndrafted;
    public float MinFocusThreshold;
    public HashSet<string> DraftedAutocastDefs;
    public HashSet<string> UndraftedAutocastDefs;
    public HashSet<string> BlockedJobDefs;

    public float MendHealthThreshold;
    public bool MendPawns;
    public bool MendMechs;
    public bool MendInStockpile;
    public bool MendOnlyNamedStockpiles;
    public bool MendInStorage;
    public bool MendOnlyNamedStorageGroups;

    public bool EnchantInStockpile;
    public bool EnchantOnlyNamedStockpiles;
    public bool EnchantInStorage;
    public bool EnchantOnlyNamedStorageGroups;

    public bool PowerBuildings;
    public bool PowerMechs;
    public bool PowerUseRange;
    public float PowerRange;

    public bool StealVitalityFromPrisoners;
    public bool StealVitalityFromSlaves;
    public bool StealVitalityFromColonists;
    public bool StealVitalityFromVisitors;

    public bool DeathshieldColonists;
    public bool DeathshieldColonyAnimals;
    public bool DeathshieldSlaves;
    public bool DeathshieldPrisoners;
    public bool DeathshieldVisitors;


    public bool
    WordOfInspirationTargetColonists;
    public bool WordOfInspirationTargetSlaves;

    public float WordOfJoyMoodThreshold;

    public bool WordOfSerenityTargetScaria;
    public bool WordOfSerenityTargetColonists;
    public bool WordOfSerenityTargetColonyAnimals;
    public bool WordOfSerenityTargetWildAnimals;
    public bool WordOfSerenityTargetSlaves;
    public bool WordOfSerenityTargetPrisoners;
    public bool WordOfSerenityTargetVisitors;
    public HashSet<string> WordOfSerenityIgnoredMentalStateDefs;

    public bool SootheColonistsCheck;
    public float SootheColonistsMaximumMood;
    public bool SootheSlavesCheck;
    public float SootheSlavesMaximumMood;
    public bool SoothePrisonersCheck;
    public float SoothePrisonersMaximumMood;
    public bool SootheVisitorsCheck;
    public float SootheVisitorsMaximumMood;

    public bool BrainLeechTargetPrisoners;
    public bool BrainLeechTargetSlaves;

    public bool ShowRunecircleArea;
    public bool ShowGreaterRunecircleArea;

    public bool ShowCraftTimeskipArea;

    public bool ShowSolarPinholeArea;

    public bool ShowIceCrystalArea;

    public bool DarkvisionTargetSelf;
    public bool DarkvisionTargetColonists;

    public bool FocusTargetSelf;
    public bool FocusTargetColonists;
    public bool FocusTargetSlaves;

    public bool InvisibilityTargetSelf;
    public bool InvisibilityTargetColonists;

    public bool OvershieldTargetSelf;
    public bool OvershieldTargetColonists;

    public bool WordOfImmunityTargetColonists;
    public bool WordOfImmunityTargetColonyAnimals;
    public bool WordOfImmunityTargetSlaves;
    public bool WordOfImmunityTargetPrisoners;
    public bool WordOfImmunityTargetVisitors;

    public bool IceShieldTargetSelf;
    public bool IceShieldTargetColonists;
    public bool IceShieldTargetSlaves;
    public bool IceShieldTargetVisitors;

    public bool FireShieldTargetSelf;
    public bool FireShieldTargetColonists;
    public bool FireShieldTargetSlaves;
    public bool FireShieldTargetVisitors;

    public bool StaticAuraTargetSelf;
    public bool StaticAuraTargetColonists;
    public bool StaticAuraTargetSlaves;
    public bool StaticAuraTargetVisitors;

    public bool EnthrallInStockpile;
    public bool EnthrallOnlyNamedStockpiles;
    public bool EnthrallInStorage;
    public bool EnthrallOnlyNamedStorageGroups;

    public bool WordOfAllianceCheckAllowedArea;
    public IntRange WordOfAllianceGoodwill;

    #region Scribe Helpers

    private static AutocastSettings DefaultValues() => new();

    private void LookStruct<T>(Expression<Func<T>> expression)
        where T : struct
    {
        if (
            expression.Body
            is not MemberExpression
            {
                Member: MemberInfo { MemberType: MemberTypes.Field, Name: string memberName }
            }
        )
        {
            throw new ArgumentException(
                "Invalid expression passed to LookStruct",
                nameof(expression)
            );
        }

        FieldInfo fieldInfo = typeof(AutocastSettings).GetField(memberName);
        T? value = fieldInfo.GetValue(this).ChangeType<T>();
        T defaultValue = fieldInfo.GetValue(DefaultValues()).ChangeType<T>();
        Scribe_Values.Look(ref value, memberName, defaultValue);
        fieldInfo.SetValue(this, value);
    }

    private static void LookHashSet<T>(
        ref HashSet<T> valueHashSet,
        string label,
        HashSet<T> defaultValues
    )
        where T : notnull
    {
        if (Scribe.mode == LoadSaveMode.Saving && valueHashSet is null)
        {
            BetterAutocastVPE.Warn(
                label + " is null before saving. Reinitializing with default values."
            );
            valueHashSet = defaultValues;
        }
        Scribe_Collections.Look(ref valueHashSet, label, lookMode: LookMode.Value);
        if (Scribe.mode == LoadSaveMode.LoadingVars && valueHashSet is null)
        {
            BetterAutocastVPE.Warn(
                label + " is null after loading. Reinitializing with default values."
            );
            valueHashSet = defaultValues;
        }
    }
    #endregion

    public void Reset()
    {
        #region General
        DebugLog = false;
        ShowValidationMessages = false;
        AutocastIntervalDrafted = 30;
        AutocastIntervalUndrafted = 600;
        MinFocusThreshold = 0.5f;
        DraftedAutocastDefs = DefaultDraftedAutocastDefs();
        UndraftedAutocastDefs = DefaultUndraftedAutocastDefs();
        BlockedJobDefs = DefaultBlockedJobDefs();
        #endregion General

        #region Mend
        MendHealthThreshold = 0.5f;
        MendPawns = true;
        MendMechs = true;
        MendInStockpile = true;
        MendOnlyNamedStockpiles = true;
        MendInStorage = true;
        MendOnlyNamedStorageGroups = false;
        #endregion Mend

        #region Enchant Quality
        EnchantInStockpile = true;
        EnchantOnlyNamedStockpiles = true;
        EnchantInStorage = true;
        EnchantOnlyNamedStorageGroups = false;
        #endregion Enchant Quality

        #region Power
        PowerBuildings = true;
        PowerMechs = true;
        PowerUseRange = false;
        PowerRange = 20f;
        #endregion Power

        #region Steal Vitality
        StealVitalityFromPrisoners = true;
        StealVitalityFromSlaves = true;
        StealVitalityFromColonists = true;
        StealVitalityFromVisitors = false;
        #endregion Steal Vitality

        #region Deathshield
        DeathshieldColonists = true;
        DeathshieldColonyAnimals = true;
        DeathshieldSlaves = false;
        DeathshieldPrisoners = false;
        DeathshieldVisitors = false;
        #endregion Deathshield

        #region Enthrall
        EnthrallInStockpile = true;
        EnthrallOnlyNamedStockpiles = true;
        EnthrallInStorage = true;
        EnthrallOnlyNamedStorageGroups = true;
        #endregion Enthrall

        #region Word of Inspiration
        WordOfInspirationTargetColonists = true;
        WordOfInspirationTargetSlaves = false;
        #endregion Word of Inspiration

        #region Word of Joy
        WordOfJoyMoodThreshold = 0.2f;
        #endregion Word of Joy

        #region Word of Serenity
        WordOfSerenityTargetScaria = false;
        WordOfSerenityTargetColonists = true;
        WordOfSerenityTargetColonyAnimals = false;
        WordOfSerenityTargetWildAnimals = false;
        WordOfSerenityTargetSlaves = false;
        WordOfSerenityTargetPrisoners = false;
        WordOfSerenityTargetVisitors = false;
        WordOfSerenityIgnoredMentalStateDefs = DefaultWordOfSerenityIgnoredMentalStateDefs();
        #endregion Word of Serenity

        #region Brain Leech
        BrainLeechTargetPrisoners = true;
        BrainLeechTargetSlaves = true;
        #endregion Brain Leech

        #region Craft Timeskip
        ShowCraftTimeskipArea = true;
        #endregion Craft Timeskip

        #region Solar Pinhole
        ShowSolarPinholeArea = true;
        #endregion Solar Pinhole

        #region Ice Crystal
        ShowIceCrystalArea = true;
        #endregion Ice Crystal

        #region Etch Greater Runecircle
        ShowGreaterRunecircleArea = true;
        #endregion Etch Greater Runecircle

        #region Etch Runecircle
        ShowRunecircleArea = true;
        #endregion Etch Runecircle

        #region Darkvision
        DarkvisionTargetSelf = true;
        DarkvisionTargetColonists = true;
        #endregion Darkvision

        #region Invisibility
        InvisibilityTargetSelf = true;
        InvisibilityTargetColonists = true;
        #endregion Invisibility

        #region Focus
        FocusTargetSelf = true;
        FocusTargetColonists = true;
        FocusTargetSlaves = false;
        #endregion Focus

        #region Overshield
        OvershieldTargetSelf = true;
        OvershieldTargetColonists = true;
        #endregion Overshield

        #region Word of Immunity
        WordOfImmunityTargetColonists = true;
        WordOfImmunityTargetColonyAnimals = true;
        WordOfImmunityTargetSlaves = true;
        WordOfImmunityTargetPrisoners = true;
        WordOfImmunityTargetVisitors = false;
        #endregion Word of Immunity

        #region Ice Shield
        IceShieldTargetSelf = true;
        IceShieldTargetColonists = true;
        IceShieldTargetSlaves = false;
        IceShieldTargetVisitors = false;
        #endregion Ice Shield

        #region Fire Shield
        FireShieldTargetSelf = true;
        FireShieldTargetColonists = true;
        FireShieldTargetSlaves = false;
        FireShieldTargetVisitors = false;
        #endregion Fire Shield

        #region Static Aura
        StaticAuraTargetSelf = true;
        StaticAuraTargetColonists = true;
        StaticAuraTargetSlaves = false;
        StaticAuraTargetVisitors = false;
        #endregion Static Aura

        #region Soothe (Female/Male)
        SootheColonistsCheck = true;
        SootheColonistsMaximumMood = 0.5f;
        SootheSlavesCheck = false;
        SootheSlavesMaximumMood = 0.5f;
        SoothePrisonersCheck = false;
        SoothePrisonersMaximumMood = 0.5f;
        SootheVisitorsCheck = false;
        SootheVisitorsMaximumMood = 0.5f;
        #endregion Soothe (Female/Male)

        #region Word of Alliance
        WordOfAllianceCheckAllowedArea = true;
        WordOfAllianceGoodwill = new IntRange(-100, 100);
        #endregion Word of Alliance
    }

    public override void ExposeData()
    {
        base.ExposeData();

        LookStruct(() => DebugLog);
        LookStruct(() => ShowValidationMessages);
        LookStruct(() => AutocastIntervalDrafted);
        LookStruct(() => AutocastIntervalUndrafted);
        LookStruct(() => MinFocusThreshold);

        #region Mend
        LookStruct(() => MendHealthThreshold);
        LookStruct(() => MendPawns);
        LookStruct(() => MendMechs);
        LookStruct(() => MendInStockpile);
        LookStruct(() => MendOnlyNamedStockpiles);
        LookStruct(() => MendInStorage);
        LookStruct(() => MendOnlyNamedStorageGroups);
        #endregion Mend

        #region Enchant Quality
        LookStruct(() => EnchantInStockpile);
        LookStruct(() => EnchantOnlyNamedStockpiles);
        LookStruct(() => EnchantInStorage);
        LookStruct(() => EnchantOnlyNamedStorageGroups);
        #endregion Enchant Quality

        #region Power
        LookStruct(() => PowerBuildings);
        LookStruct(() => PowerMechs);
        LookStruct(() => PowerUseRange);
        LookStruct(() => PowerRange);
        #endregion Power

        #region Steal Vitality
        LookStruct(() => StealVitalityFromPrisoners);
        LookStruct(() => StealVitalityFromSlaves);
        LookStruct(() => StealVitalityFromColonists);
        LookStruct(() => StealVitalityFromVisitors);
        #endregion Steal Vitality

        #region Deathshield
        LookStruct(() => DeathshieldColonists);
        LookStruct(() => DeathshieldColonyAnimals);
        LookStruct(() => DeathshieldSlaves);
        LookStruct(() => DeathshieldPrisoners);
        LookStruct(() => DeathshieldVisitors);
        #endregion Deathshield

        #region Enthrall
        LookStruct(() => EnthrallInStockpile);
        LookStruct(() => EnthrallOnlyNamedStockpiles);
        LookStruct(() => EnthrallInStorage);
        LookStruct(() => EnthrallOnlyNamedStorageGroups);
        #endregion Enthrall

        #region Word of Inspiration
        LookStruct(() => WordOfInspirationTargetColonists);
        LookStruct(() => WordOfInspirationTargetSlaves);
        #endregion Word of Inspiration

        #region Word of Joy
        LookStruct(() => WordOfJoyMoodThreshold);
        #endregion Word of Joy

        #region Word of Serenity
        LookStruct(() => WordOfSerenityTargetScaria);
        LookStruct(() => WordOfSerenityTargetColonists);
        LookStruct(() => WordOfSerenityTargetColonyAnimals);
        LookStruct(() => WordOfSerenityTargetWildAnimals);
        LookStruct(() => WordOfSerenityTargetSlaves);
        LookStruct(() => WordOfSerenityTargetPrisoners);
        LookStruct(() => WordOfSerenityTargetVisitors);
        LookHashSet(
            ref WordOfSerenityIgnoredMentalStateDefs,
            nameof(WordOfSerenityIgnoredMentalStateDefs),
            DefaultWordOfSerenityIgnoredMentalStateDefs()
        );
        #endregion Word of Serenity

        #region Brain Leech
        LookStruct(() => BrainLeechTargetPrisoners);
        LookStruct(() => BrainLeechTargetSlaves);
        #endregion Brain Leech

        #region Darkvision
        LookStruct(() => DarkvisionTargetSelf);
        LookStruct(() => DarkvisionTargetColonists);
        #endregion Darkvision

        #region Craft Timeskip
        LookStruct(() => ShowCraftTimeskipArea);
        #endregion Craft Timeskip

        #region Solar Pinhole
        LookStruct(() => ShowSolarPinholeArea);
        #endregion Solar Pinhole

        #region Ice Crystal
        LookStruct(() => ShowIceCrystalArea);
        #endregion Ice Crystal

        #region Etch Greater Runecircle
        LookStruct(() => ShowGreaterRunecircleArea);
        #endregion Etch Greater Runecircle

        #region Etch Runecircle
        LookStruct(() => ShowRunecircleArea);
        #endregion Etch Runecircle

        #region Invisibility
        LookStruct(() => InvisibilityTargetSelf);
        LookStruct(() => InvisibilityTargetColonists);
        #endregion Invisibility

        #region Overshield
        LookStruct(() => OvershieldTargetSelf);
        LookStruct(() => OvershieldTargetColonists);
        #endregion Overshield

        #region Word of Immunity
        LookStruct(() => WordOfImmunityTargetColonists);
        LookStruct(() => WordOfImmunityTargetColonyAnimals);
        LookStruct(() => WordOfImmunityTargetSlaves);
        LookStruct(() => WordOfImmunityTargetPrisoners);
        LookStruct(() => WordOfImmunityTargetVisitors);
        #endregion Word of Immunity

        #region Focus
        LookStruct(() => FocusTargetSelf);
        LookStruct(() => FocusTargetColonists);
        LookStruct(() => FocusTargetSlaves);
        #endregion Focus

        #region Ice Shield
        LookStruct(() => IceShieldTargetSelf);
        LookStruct(() => IceShieldTargetColonists);
        LookStruct(() => IceShieldTargetSlaves);
        LookStruct(() => IceShieldTargetVisitors);
        #endregion Ice Shield

        #region Fire Shield
        LookStruct(() => FireShieldTargetSelf);
        LookStruct(() => FireShieldTargetColonists);
        LookStruct(() => FireShieldTargetSlaves);
        LookStruct(() => FireShieldTargetVisitors);
        #endregion Fire Shield

        #region Static Aura
        LookStruct(() => StaticAuraTargetSelf);
        LookStruct(() => StaticAuraTargetColonists);
        LookStruct(() => StaticAuraTargetSlaves);
        LookStruct(() => StaticAuraTargetVisitors);
        #endregion Static Aura

        #region Soothe (Female/Male)
        LookStruct(() => SootheColonistsCheck);
        LookStruct(() => SootheColonistsMaximumMood);
        LookStruct(() => SootheSlavesCheck);
        LookStruct(() => SootheSlavesMaximumMood);
        LookStruct(() => SoothePrisonersCheck);
        LookStruct(() => SoothePrisonersMaximumMood);
        LookStruct(() => SootheVisitorsCheck);
        LookStruct(() => SootheVisitorsMaximumMood);
        #endregion Soothe (Female/Male)

        #region Word of Alliance
        LookStruct(() => WordOfAllianceCheckAllowedArea);
        LookStruct(() => WordOfAllianceGoodwill);
        #endregion Word of Alliance

        #region General
        LookHashSet(
            ref DraftedAutocastDefs,
            nameof(DraftedAutocastDefs),
            DefaultDraftedAutocastDefs()
        );
        LookHashSet(
            ref UndraftedAutocastDefs,
            nameof(UndraftedAutocastDefs),
            DefaultUndraftedAutocastDefs()
        );
        LookHashSet(ref BlockedJobDefs, nameof(BlockedJobDefs), DefaultBlockedJobDefs());
        #endregion General
    }
}
