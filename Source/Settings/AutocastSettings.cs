using System.Collections.Generic;
using Verse;

namespace BetterAutocastVPE.Settings;

public class AutocastSettings : ModSettings
{
    private readonly HashSet<string> defaultDraftedAutocastDefs =
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
    private readonly HashSet<string> defaultUndraftedAutocastDefs =
    [
        "VPE_Darkvision",
        "VPE_Deathshield",
        "VPE_Eclipse",
        "VPE_EnchantQuality",
        "VPE_Enthrall",
        "VPE_Ghostwalk",
        "VPE_IceCrystal",
        "VPE_Mend",
        "VPE_Power",
        "VPE_PsychicGuidance",
        "VPE_SolarPinhole",
        "VPE_SolarPinholeSunlamp",
        "VPE_SpeedBoost",
        "VPE_StealVitality",
        "VPE_WordofImmunity",
        "VPE_WordofJoy",
        "VPE_WordofProductivity",
        "VPE_WordofSerenity",
        "VPEP_BrainLeech",
    ];
    private readonly HashSet<string> defaultBlockedJobDefs =
    [
        "BetterAutocastVPE_GotoLocationAndCastAbilityOnce",
        "DeliverToBed",
        "Ingest",
        "LayDown",
        "SpectateCeremony",
        "Goto",
        "VFEA_GotoTargetAndUseAbility",
        "VFEA_UseAbility",
        "Wait_Asleep",
        "Wait_Downed",
    ];
    private readonly HashSet<string> defaultWordOfSerenityIgnoredMentalStateDefs =
    [
        "BerserkTrance",
        "BerserkWarcall",
        "Crying",
        "Giggling",
    ];

    public AutocastSettings()
    {
        BetterAutocastVPE.DebugLog("Initiating settings");
        BlockedJobDefs ??= defaultBlockedJobDefs;
        DraftedAutocastDefs ??= defaultDraftedAutocastDefs;
        UndraftedAutocastDefs ??= defaultUndraftedAutocastDefs;
        WordOfSerenityIgnoredMentalStateDefs ??= defaultWordOfSerenityIgnoredMentalStateDefs;
    }

    public int AutocastIntervalDrafted = 30;
    public int AutocastIntervalUndrafted = 600;
    public HashSet<string> DraftedAutocastDefs;
    public HashSet<string> UndraftedAutocastDefs;
    public HashSet<string> BlockedJobDefs;

    public float MendHealthThreshold = 0.5f;
    public bool MendPawns = true;
    public bool MendInStockpile = true;
    public bool MendOnlyNamedStockpiles = true;
    public bool MendInStorage = true;
    public bool MendOnlyNamedStorageGroups = false;

    public bool EnchantInStockpile = true;
    public bool EnchantOnlyNamedStockpiles = true;
    public bool EnchantInStorage = true;
    public bool EnchantOnlyNamedStorageGroups = false;

    public bool PowerBuildings = true;
    public bool PowerMechs = true;

    public bool StealVitalityFromPrisoners = true;
    public bool StealVitalityFromSlaves = true;
    public bool StealVitalityFromColonists = true;
    public bool StealVitalityFromVisitors = false;

    public bool DeathshieldColonists = true;
    public bool DeathshieldColonyAnimals = false;
    public bool DeathshieldSlaves = false;
    public bool DeathshieldPrisoners = false;
    public bool DeathshieldVisitors = false;

    public float WordOfJoyMoodThreshold = 0.2f;

    public bool WordOfSerenityTargetScaria = false;
    public bool WordOfSerenityTargetColonists = true;
    public bool WordOfSerenityTargetColonyAnimals = false;
    public bool WordOfSerenityTargetWildAnimals = false;
    public bool WordOfSerenityTargetSlaves = false;
    public bool WordOfSerenityTargetPrisoners = false;
    public bool WordOfSerenityTargetVisitors = false;
    public HashSet<string> WordOfSerenityIgnoredMentalStateDefs;

    public bool BrainLeechTargetPrisoners = true;
    public bool BrainLeechTargetSlaves = true;

    public bool DarkvisionTargetSelf = true;
    public bool DarkvisionTargetColonists = true;

    public bool InvisibilityTargetSelf = true;
    public bool InvisibilityTargetColonists = true;

    public bool OvershieldTargetSelf = true;
    public bool OvershieldTargetColonists = true;

    public bool WordOfImmunityTargetColonists = true;
    public bool WordOfImmunityTargetColonyAnimals = false;
    public bool WordOfImmunityTargetSlaves = true;
    public bool WordOfImmunityTargetPrisoners = true;
    public bool WordOfImmunityTargetVisitors = false;

    public bool IceShieldTargetSelf = true;
    public bool IceShieldTargetColonists = true;
    public bool IceShieldTargetSlaves = false;
    public bool IceShieldTargetVisitors = false;

    public bool FireShieldTargetSelf = true;
    public bool FireShieldTargetColonists = true;
    public bool FireShieldTargetSlaves = false;
    public bool FireShieldTargetVisitors = false;

    public bool StaticAuraTargetSelf = true;
    public bool StaticAuraTargetColonists = true;
    public bool StaticAuraTargetSlaves = false;
    public bool StaticAuraTargetVisitors = false;

    public bool EnthrallInStockpile = true;
    public bool EnthrallOnlyNamedStockpiles = true;
    public bool EnthrallInStorage = true;
    public bool EnthrallOnlyNamedStorageGroups = true;

    #region Scribe Helpers
    private static void LookField<T>(ref T value, string label, T defaultValue)
        where T : struct
    {
        Scribe_Values.Look(ref value, label, defaultValue);
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

    public override void ExposeData()
    {
        base.ExposeData();
        LookField(ref AutocastIntervalDrafted, nameof(AutocastIntervalDrafted), 30);
        LookField(ref AutocastIntervalUndrafted, nameof(AutocastIntervalUndrafted), 600);

        #region Mend
        LookField(ref MendHealthThreshold, nameof(MendHealthThreshold), 0.5f);
        LookField(ref MendPawns, nameof(MendPawns), true);
        LookField(ref MendInStockpile, nameof(MendInStockpile), true);
        LookField(ref MendOnlyNamedStockpiles, nameof(MendOnlyNamedStockpiles), true);
        LookField(ref MendInStorage, nameof(MendInStorage), true);
        LookField(ref MendOnlyNamedStorageGroups, nameof(MendOnlyNamedStorageGroups), false);
        #endregion Mend

        #region Enchant Quality
        LookField(ref EnchantInStockpile, nameof(EnchantInStockpile), true);
        LookField(ref EnchantOnlyNamedStockpiles, nameof(EnchantOnlyNamedStockpiles), true);
        LookField(ref EnchantInStorage, nameof(EnchantInStorage), true);
        LookField(ref EnchantOnlyNamedStorageGroups, nameof(EnchantOnlyNamedStorageGroups), false);
        #endregion Enchant Quality

        #region Power
        LookField(ref PowerBuildings, nameof(PowerBuildings), true);
        LookField(ref PowerMechs, nameof(PowerMechs), true);
        #endregion Power

        #region Steal Vitality
        LookField(ref StealVitalityFromPrisoners, nameof(StealVitalityFromPrisoners), true);
        LookField(ref StealVitalityFromSlaves, nameof(StealVitalityFromSlaves), true);
        LookField(ref StealVitalityFromColonists, nameof(StealVitalityFromColonists), true);
        LookField(ref StealVitalityFromVisitors, nameof(StealVitalityFromVisitors), false);
        #endregion Steal Vitality

        #region Deathshield
        LookField(ref DeathshieldColonists, nameof(DeathshieldColonists), true);
        LookField(ref DeathshieldColonyAnimals, nameof(DeathshieldColonyAnimals), true);
        LookField(ref DeathshieldSlaves, nameof(DeathshieldSlaves), false);
        LookField(ref DeathshieldPrisoners, nameof(DeathshieldPrisoners), false);
        LookField(ref DeathshieldVisitors, nameof(DeathshieldVisitors), false);
        #endregion Deathshield

        #region Enthrall
        LookField(ref EnthrallInStockpile, nameof(EnthrallInStockpile), true);
        LookField(ref EnthrallOnlyNamedStockpiles, nameof(EnthrallOnlyNamedStockpiles), true);
        LookField(ref EnthrallInStorage, nameof(EnthrallInStorage), true);
        LookField(ref EnthrallOnlyNamedStorageGroups, nameof(EnthrallOnlyNamedStorageGroups), true);
        #endregion Enthrall

        #region Word of Joy
        LookField(ref WordOfJoyMoodThreshold, nameof(WordOfJoyMoodThreshold), 0.2f);
        #endregion Word of Joy

        #region Word of Serenity
        LookField(ref WordOfSerenityTargetScaria, nameof(WordOfSerenityTargetScaria), false);
        LookField(ref WordOfSerenityTargetColonists, nameof(WordOfSerenityTargetColonists), false);
        LookField(
            ref WordOfSerenityTargetColonyAnimals,
            nameof(WordOfSerenityTargetColonyAnimals),
            false
        );
        LookField(
            ref WordOfSerenityTargetWildAnimals,
            nameof(WordOfSerenityTargetWildAnimals),
            false
        );
        LookField(ref WordOfSerenityTargetSlaves, nameof(WordOfSerenityTargetSlaves), false);
        LookField(ref WordOfSerenityTargetPrisoners, nameof(WordOfSerenityTargetPrisoners), false);
        LookField(ref WordOfSerenityTargetVisitors, nameof(WordOfSerenityTargetVisitors), false);
        LookHashSet(
            ref WordOfSerenityIgnoredMentalStateDefs,
            nameof(WordOfSerenityIgnoredMentalStateDefs),
            defaultWordOfSerenityIgnoredMentalStateDefs
        );
        #endregion Word of Serenity

        #region Brain Leech
        LookField(ref BrainLeechTargetPrisoners, nameof(BrainLeechTargetPrisoners), true);
        LookField(ref BrainLeechTargetSlaves, nameof(BrainLeechTargetSlaves), true);
        #endregion Brain Leech

        #region Darkvision
        LookField(ref DarkvisionTargetSelf, nameof(DarkvisionTargetSelf), true);
        LookField(ref DarkvisionTargetColonists, nameof(DarkvisionTargetColonists), true);
        #endregion Darkvision

        #region Invisibility
        LookField(ref InvisibilityTargetSelf, nameof(InvisibilityTargetSelf), true);
        LookField(ref InvisibilityTargetColonists, nameof(InvisibilityTargetColonists), true);
        #endregion Invisibility

        #region Overshield
        LookField(ref OvershieldTargetSelf, nameof(OvershieldTargetSelf), true);
        LookField(ref OvershieldTargetColonists, nameof(OvershieldTargetColonists), true);
        #endregion Overshield

        #region Word of Immunity
        LookField(ref WordOfImmunityTargetColonists, nameof(WordOfImmunityTargetColonists), true);
        LookField(
            ref WordOfImmunityTargetColonyAnimals,
            nameof(WordOfImmunityTargetColonyAnimals),
            true
        );
        LookField(ref WordOfImmunityTargetSlaves, nameof(WordOfImmunityTargetSlaves), true);
        LookField(ref WordOfImmunityTargetPrisoners, nameof(WordOfImmunityTargetPrisoners), true);
        LookField(ref WordOfImmunityTargetVisitors, nameof(WordOfImmunityTargetVisitors), false);
        #endregion Word of Immunity

        #region Ice Shield
        LookField(ref IceShieldTargetSelf, nameof(IceShieldTargetSelf), true);
        LookField(ref IceShieldTargetColonists, nameof(IceShieldTargetColonists), true);
        LookField(ref IceShieldTargetSlaves, nameof(IceShieldTargetSlaves), false);
        LookField(ref IceShieldTargetVisitors, nameof(IceShieldTargetVisitors), false);
        #endregion Ice Shield

        #region Fire Shield
        LookField(ref FireShieldTargetSelf, nameof(FireShieldTargetSelf), true);
        LookField(ref FireShieldTargetColonists, nameof(FireShieldTargetColonists), true);
        LookField(ref FireShieldTargetSlaves, nameof(FireShieldTargetSlaves), false);
        LookField(ref FireShieldTargetVisitors, nameof(FireShieldTargetVisitors), false);
        #endregion Fire Shield

        #region Static Aura
        LookField(ref StaticAuraTargetSelf, nameof(StaticAuraTargetSelf), true);
        LookField(ref StaticAuraTargetColonists, nameof(StaticAuraTargetColonists), true);
        LookField(ref StaticAuraTargetSlaves, nameof(StaticAuraTargetSlaves), false);
        LookField(ref StaticAuraTargetVisitors, nameof(StaticAuraTargetVisitors), false);
        #endregion Static Aura

        #region General
        LookHashSet(
            ref DraftedAutocastDefs,
            nameof(DraftedAutocastDefs),
            defaultDraftedAutocastDefs
        );
        LookHashSet(
            ref UndraftedAutocastDefs,
            nameof(UndraftedAutocastDefs),
            defaultUndraftedAutocastDefs
        );
        LookHashSet(ref BlockedJobDefs, nameof(BlockedJobDefs), defaultBlockedJobDefs);
        #endregion General
    }
}
