using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BetterAutocastVPE;

public class AutocastModSettings : ModSettings
{
    private readonly HashSet<string> defaultDraftedAutocastDefs =
        new()
        {
            "VPE_SpeedBoost",
            "VPE_BladeFocus",
            "VPE_FiringFocus",
            "VPE_AdrenalineRush",
            "VPE_ControlledFrenzy",
            "VPE_GuidedShot",
            "VPE_Invisibility",
        };
    private readonly HashSet<string> defaultUndraftedAutocastDefs =
        new()
        {
            "VPE_SpeedBoost",
            "VPE_StealVitality",
            "VPEP_BrainLeech",
            "VPE_PsychicGuidance",
            "VPE_EnchantQuality",
            "VPE_Mend",
            "VPE_WordofJoy",
            "VPE_WordofSerenity",
            "VPE_WordofProductivity",
            "VPE_Eclipse",
            "VPE_Darkvision",
            "VPE_WordofImmunity",
        };
    private readonly HashSet<string> defaultBlockedJobDefs =
        new()
        {
            "VFEA_GotoTargetAndUseAbility",
            "VFEA_UseAbility",
            "LayDown",
            "Wait_Asleep",
            "Wait_Downed",
            "SpectateCeremony",
            "DeliverToBed",
            "Ingest",
        };

    public AutocastModSettings()
    {
        DraftedAutocastDefs ??= defaultDraftedAutocastDefs;
        UndraftedAutocastDefs ??= defaultUndraftedAutocastDefs;
        BlockedJobDefs ??= defaultBlockedJobDefs;
    }

    public int AutocastIntervalDrafted = 30;
    public int AutocastIntervalUndrafted = 600;

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

    public float WordOfJoyMoodThreshold = 0.2f;

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

    public HashSet<string> DraftedAutocastDefs;
    public HashSet<string> UndraftedAutocastDefs;
    public HashSet<string> BlockedJobDefs;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(
            ref AutocastIntervalDrafted,
            nameof(AutocastIntervalDrafted),
            defaultValue: 30
        );
        Scribe_Values.Look(
            ref AutocastIntervalUndrafted,
            nameof(AutocastIntervalUndrafted),
            defaultValue: 600
        );

        Scribe_Values.Look(ref MendHealthThreshold, nameof(MendHealthThreshold));
        Scribe_Values.Look(ref MendPawns, nameof(MendPawns), defaultValue: true);
        Scribe_Values.Look(ref MendInStorage, nameof(MendInStorage), defaultValue: true);
        Scribe_Values.Look(ref MendInStockpile, nameof(MendInStockpile), defaultValue: true);

        Scribe_Values.Look(ref EnchantInStorage, nameof(EnchantInStorage), defaultValue: true);
        Scribe_Values.Look(ref EnchantInStockpile, nameof(EnchantInStockpile), defaultValue: true);

        Scribe_Values.Look(
            ref StealVitalityFromPrisoners,
            nameof(StealVitalityFromPrisoners),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref StealVitalityFromSlaves,
            nameof(StealVitalityFromSlaves),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref StealVitalityFromColonists,
            nameof(StealVitalityFromColonists),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref StealVitalityFromVisitors,
            nameof(StealVitalityFromVisitors),
            defaultValue: false
        );

        Scribe_Values.Look(
            ref WordOfJoyMoodThreshold,
            nameof(WordOfJoyMoodThreshold),
            defaultValue: 0.2f
        );

        Scribe_Values.Look(
            ref BrainLeechTargetPrisoners,
            nameof(BrainLeechTargetPrisoners),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref BrainLeechTargetSlaves,
            nameof(BrainLeechTargetSlaves),
            defaultValue: true
        );

        Scribe_Values.Look(
            ref DarkvisionTargetSelf,
            nameof(DarkvisionTargetSelf),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref DarkvisionTargetColonists,
            nameof(DarkvisionTargetColonists),
            defaultValue: true
        );

        Scribe_Values.Look(
            ref InvisibilityTargetSelf,
            nameof(InvisibilityTargetSelf),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref InvisibilityTargetColonists,
            nameof(InvisibilityTargetColonists),
            defaultValue: true
        );

        Scribe_Values.Look(
            ref WordOfImmunityTargetColonists,
            nameof(WordOfImmunityTargetColonists),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref WordOfImmunityTargetColonyAnimals,
            nameof(WordOfImmunityTargetColonyAnimals),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref WordOfImmunityTargetSlaves,
            nameof(WordOfImmunityTargetSlaves),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref WordOfImmunityTargetPrisoners,
            nameof(WordOfImmunityTargetPrisoners),
            defaultValue: true
        );
        Scribe_Values.Look(
            ref WordOfImmunityTargetVisitors,
            nameof(WordOfImmunityTargetVisitors),
            defaultValue: false
        );

        if (Scribe.mode == LoadSaveMode.Saving && DraftedAutocastDefs is null)
        {
            BetterAutocastVPE.Warn(
                nameof(DraftedAutocastDefs)
                    + " is null before saving. Reinitializing with default values."
            );
            DraftedAutocastDefs ??= defaultDraftedAutocastDefs;
        }
        Scribe_Collections.Look(
            ref DraftedAutocastDefs,
            nameof(DraftedAutocastDefs),
            LookMode.Value
        );
        if (Scribe.mode == LoadSaveMode.LoadingVars && DraftedAutocastDefs is null)
        {
            BetterAutocastVPE.Warn(
                nameof(DraftedAutocastDefs)
                    + " is null after loading. Reinitializing with default values."
            );
            DraftedAutocastDefs ??= defaultDraftedAutocastDefs;
        }

        if (Scribe.mode == LoadSaveMode.Saving && UndraftedAutocastDefs is null)
        {
            BetterAutocastVPE.Warn(
                nameof(UndraftedAutocastDefs)
                    + " is null before saving. Reinitializing with default values."
            );
            UndraftedAutocastDefs ??= defaultUndraftedAutocastDefs;
        }
        Scribe_Collections.Look(
            ref UndraftedAutocastDefs,
            false,
            nameof(UndraftedAutocastDefs),
            LookMode.Value
        );
        if (Scribe.mode == LoadSaveMode.LoadingVars && UndraftedAutocastDefs is null)
        {
            BetterAutocastVPE.Warn(
                nameof(UndraftedAutocastDefs)
                    + " is null after loading. Reinitializing with default values."
            );
            UndraftedAutocastDefs ??= defaultUndraftedAutocastDefs;
        }

        if (Scribe.mode == LoadSaveMode.Saving && BlockedJobDefs is null)
        {
            BetterAutocastVPE.Warn(
                nameof(BlockedJobDefs)
                    + " is null before saving. Reinitializing with default values."
            );
            BlockedJobDefs ??= defaultBlockedJobDefs;
        }
        Scribe_Collections.Look(ref BlockedJobDefs, false, nameof(BlockedJobDefs), LookMode.Value);
        if (Scribe.mode == LoadSaveMode.LoadingVars && BlockedJobDefs is null)
        {
            BetterAutocastVPE.Warn(
                nameof(BlockedJobDefs)
                    + " is null after loading. Reinitializing with default values."
            );
            BlockedJobDefs ??= defaultBlockedJobDefs;
        }
    }
}
