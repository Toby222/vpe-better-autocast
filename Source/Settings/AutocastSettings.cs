using System.Collections.Generic;
using Verse;

namespace BetterAutocastVPE.Settings;

public class AutocastSettings : ModSettings
{
    private readonly HashSet<string> defaultDraftedAutocastDefs =
        new()
        {
            "VPE_AdrenalineRush",
            "VPE_BladeFocus",
            "VPE_ControlledFrenzy",
            "VPE_Deathshield",
            "VPE_FiringFocus",
            "VPE_GuidedShot",
            "VPE_Invisibility",
            "VPE_SpeedBoost",
        };
    private readonly HashSet<string> defaultUndraftedAutocastDefs =
        new()
        {
            "VPE_Darkvision",
            "VPE_Deathshield",
            "VPE_Eclipse",
            "VPE_EnchantQuality",
            "VPE_Mend",
            "VPE_PsychicGuidance",
            "VPE_SpeedBoost",
            "VPE_StealVitality",
            "VPE_WordofImmunity",
            "VPE_WordofJoy",
            "VPE_WordofProductivity",
            "VPE_WordofSerenity",
            "VPEP_BrainLeech",
        };
    private readonly HashSet<string> defaultBlockedJobDefs =
        new()
        {
            "DeliverToBed",
            "Ingest",
            "LayDown",
            "SpectateCeremony",
            "VFEA_GotoTargetAndUseAbility",
            "VFEA_UseAbility",
            "Wait_Asleep",
            "Wait_Downed",
        };
    private readonly HashSet<string> defaultWordOfSerenityIgnoredMentalStateDefs =
        new() { "Crying", "Giggling", "BerserkTrance", "BerserkWarcall" };

    public AutocastSettings()
    {
        DraftedAutocastDefs ??= defaultDraftedAutocastDefs;
        UndraftedAutocastDefs ??= defaultUndraftedAutocastDefs;
        BlockedJobDefs ??= defaultBlockedJobDefs;
        WordOfSerenityIgnoredMentalStateDefs ??= defaultWordOfSerenityIgnoredMentalStateDefs;
    }

    public int AutocastIntervalDrafted = 30;
    public int AutocastIntervalUndrafted = 600;
    public HashSet<string> DraftedAutocastDefs;
    public HashSet<string> UndraftedAutocastDefs;
    public HashSet<string> BlockedJobDefs;

    public float MendHealthThreshold = 0.5f;
    public bool MendPawns = true;
    public bool MendInStorage = true;
    public bool MendInStockpile = true;

    public bool EnchantInStorage = true;
    public bool EnchantInStockpile = true;

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

    public bool WordOfImmunityTargetColonists = true;
    public bool WordOfImmunityTargetColonyAnimals = false;
    public bool WordOfImmunityTargetSlaves = true;
    public bool WordOfImmunityTargetPrisoners = true;
    public bool WordOfImmunityTargetVisitors = false;

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
        LookField(ref MendInStorage, nameof(MendInStorage), true);
        LookField(ref MendInStockpile, nameof(MendInStockpile), true);
        #endregion Mend

        #region Enchant Quality
        LookField(ref EnchantInStorage, nameof(EnchantInStorage), true);
        LookField(ref EnchantInStockpile, nameof(EnchantInStockpile), true);
        #endregion Enchant Quality

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
