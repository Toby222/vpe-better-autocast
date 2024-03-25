using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.UItils;

namespace BetterAutocastVPE;

public class BetterAutocastVPE : Mod
{
    public BetterAutocastVPE(ModContentPack content)
        : base(content)
    {
        Harmony harmony = new("dev.tobot.vpe-better-autocast");
        if (
            UnregisterPatch(
                harmony,
                typeof(Pawn),
                "TryGetAttackVerb",
                HarmonyPatchType.Postfix,
                "OskarPotocki.VFECore"
            )
        )
        {
            Log.Message("UnregisterPatch succeeded");
        }
        else
        {
            Log.Error("UnregisterPatch failed");
        }
        harmony.PatchAll();
        Settings = GetSettings<AutocastModSettings>();
    }

#nullable disable
    public static AutocastModSettings Settings { get; private set; }

#nullable enable

    private static Vector2 settingsScrollPosition = new();

    private static float settingsHeight;

    public override void DoSettingsWindowContents(Rect inRect)
    {
        static void AbilityHeader(Listing_Standard listing, string abilityDefName)
        {
            AbilityDef ability = DefDatabase<AbilityDef>.GetNamed(abilityDefName);
            Rect abilityLabelRow = listing.GetRect(
                Text.CalcHeight(ability.LabelCap, listing.ColumnWidth)
            );

            Widgets.DrawHighlightIfMouseover(abilityLabelRow);
            Widgets.DrawTextureFitted(
                abilityLabelRow.TakeLeftPart(abilityLabelRow.height),
                ability.icon,
                1.0f
            );
            Widgets.Label(abilityLabelRow, ability.LabelCap);

            if (
                LanguageDatabase.activeLanguage.HaveTextForKey(
                    $"BetterAutocastVPE.{abilityDefName}.Explanation"
                )
            )
            {
                string explanation = $"BetterAutocastVPE.{abilityDefName}.Explanation".Translate();
                Widgets.Label(
                    listing.GetRect(Text.CalcHeight(explanation, listing.ColumnWidth)),
                    explanation
                );
            }

            bool castWhileDrafted = Settings.DraftedAutocastDefs.Contains(abilityDefName);
            bool castWhileDraftedOriginal = castWhileDrafted;
            listing.CheckboxLabeled(
                "BetterAutocastVPE.CastDrafted".Translate(),
                ref castWhileDrafted
            );

            if (castWhileDrafted != castWhileDraftedOriginal)
            {
                if (castWhileDrafted)
                {
                    Settings.DraftedAutocastDefs.Add(abilityDefName);
                }
                else
                {
                    Settings.DraftedAutocastDefs.Remove(abilityDefName);
                }
            }

            if (ability.showUndrafted)
            {
                bool castWhileUndrafted = Settings.UndraftedAutocastDefs.Contains(abilityDefName);
                bool castWhileUndraftedOriginal = castWhileUndrafted;
                listing.CheckboxLabeled(
                    "BetterAutocastVPE.CastUndrafted".Translate(),
                    ref castWhileUndrafted
                );

                if (castWhileUndrafted != castWhileUndraftedOriginal)
                {
                    if (castWhileUndrafted)
                    {
                        Settings.UndraftedAutocastDefs.Add(abilityDefName);
                    }
                    else
                    {
                        Settings.UndraftedAutocastDefs.Remove(abilityDefName);
                    }
                }
            }
        }

        Listing_Standard listing = new();

        Rect viewRect = new(inRect.x, inRect.y, inRect.width - 16f, settingsHeight);
        Widgets.BeginScrollView(inRect, ref settingsScrollPosition, viewRect);
        listing.Begin(new Rect(viewRect.x, viewRect.y, viewRect.width, 10000f));

        Settings.AutocastIntervalDrafted = (int)
            listing.SliderLabeled(
                "BetterAutocastVPE.AutocastIntervalDrafted".Translate(
                    Settings.AutocastIntervalDrafted
                ),
                Settings.AutocastIntervalDrafted,
                1f,
                10000f,
                tooltip: "BetterAutocastVPE.AutocastIntervalDrafted.Description".Translate(
                    Settings.AutocastIntervalDrafted
                )
            );
        Settings.AutocastIntervalUndrafted = (int)
            listing.SliderLabeled(
                "BetterAutocastVPE.AutocastIntervalUndrafted".Translate(
                    Settings.AutocastIntervalUndrafted
                ),
                Settings.AutocastIntervalUndrafted,
                1f,
                10000f,
                tooltip: "BetterAutocastVPE.AutocastIntervalUndrafted.Description".Translate(
                    Settings.AutocastIntervalUndrafted
                )
            );

        listing.GapLine();

        AbilityHeader(listing, "VPE_Mend");

        Settings.MendHealthThreshold = listing.SliderLabeled(
            "BetterAutocastVPE.MendHealthThreshold".Translate(
                Settings.MendHealthThreshold.ToString("P")
            ),
            Settings.MendHealthThreshold,
            0.0f,
            1.0f,
            tooltip: "BetterAutocastVPE.MendHealthThreshold.Description".Translate()
        );
        listing.CheckboxLabeled("BetterAutocastVPE.MendPawns".Translate(), ref Settings.MendPawns);
        listing.CheckboxLabeled(
            "BetterAutocastVPE.MendInStockpile".Translate(),
            ref Settings.MendInStockpile
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.MendInStorage".Translate(),
            ref Settings.MendInStorage
        );

        listing.GapLine();

        AbilityHeader(listing, "VPE_EnchantQuality");
        listing.CheckboxLabeled(
            "BetterAutocastVPE.EnchantInStockpile".Translate(),
            ref Settings.EnchantInStockpile
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.EnchantInStorage".Translate(),
            ref Settings.EnchantInStorage
        );

        listing.GapLine();

        AbilityHeader(listing, "VPE_StealVitality");

        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetPrisoners".Translate(),
            ref Settings.StealVitalityFromPrisoners
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetSlaves".Translate(),
            ref Settings.StealVitalityFromSlaves
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetColonists".Translate(),
            ref Settings.StealVitalityFromColonists
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetVisitors".Translate(),
            ref Settings.StealVitalityFromVisitors
        );

        listing.GapLine();

        AbilityHeader(listing, "VPE_WordofJoy");

        Settings.WordOfJoyMoodThreshold = listing.SliderLabeled(
            "BetterAutocastVPE.WordOfJoyMoodThreshold".Translate(
                Settings.WordOfJoyMoodThreshold.ToString("P")
            ),
            Settings.WordOfJoyMoodThreshold,
            0.0f,
            1.0f,
            tooltip: "BetterAutocastVPE.WordOfJoyMoodThreshold.Description".Translate()
        );

        listing.GapLine();

        if (ModsConfig.IsActive("VanillaExpanded.VPE.Puppeteer"))
        {
            AbilityHeader(listing, "VPEP_BrainLeech");

            listing.CheckboxLabeled(
                "BetterAutocastVPE.TargetPrisoners".Translate(),
                ref Settings.BrainLeechTargetPrisoners
            );
            listing.CheckboxLabeled(
                "BetterAutocastVPE.TargetSlaves".Translate(),
                ref Settings.BrainLeechTargetSlaves
            );

            listing.GapLine();
        }

        AbilityHeader(listing, "VPE_AdrenalineRush");
        listing.GapLine();
        AbilityHeader(listing, "VPE_BladeFocus");
        listing.GapLine();
        AbilityHeader(listing, "VPE_ControlledFrenzy");
        listing.GapLine();
        AbilityHeader(listing, "VPE_Darkvision");
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetSelf".Translate(),
            ref Settings.DarkvisionTargetSelf
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetColonists".Translate(),
            ref Settings.DarkvisionTargetColonists
        );
        listing.GapLine();
        AbilityHeader(listing, "VPE_Eclipse");
        listing.GapLine();
        AbilityHeader(listing, "VPE_FiringFocus");
        listing.GapLine();
        AbilityHeader(listing, "VPE_GuidedShot");
        listing.GapLine();
        AbilityHeader(listing, "VPE_PsychicGuidance");
        listing.GapLine();
        AbilityHeader(listing, "VPE_SpeedBoost");
        listing.GapLine();
        AbilityHeader(listing, "VPE_WordofProductivity");
        listing.GapLine();
        AbilityHeader(listing, "VPE_WordofSerenity");
        listing.GapLine();
        AbilityHeader(listing, "VPE_Invisibility");
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetSelf".Translate(),
            ref Settings.InvisibilityTargetSelf
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetColonists".Translate(),
            ref Settings.InvisibilityTargetColonists
        );
        listing.GapLine();
        AbilityHeader(listing, "VPE_WordofImmunity");
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetColonists".Translate(),
            ref Settings.WordOfImmunityTargetColonists
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetColonyAnimals".Translate(),
            ref Settings.WordOfImmunityTargetColonyAnimals
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetSlaves".Translate(),
            ref Settings.WordOfImmunityTargetSlaves
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetPrisoners".Translate(),
            ref Settings.WordOfImmunityTargetPrisoners
        );
        listing.CheckboxLabeled(
            "BetterAutocastVPE.TargetVisitors".Translate(),
            ref Settings.WordOfImmunityTargetVisitors
        );

        listing.End();
        settingsHeight = listing.CurHeight;
        Widgets.EndScrollView();
    }

    public override string SettingsCategory()
    {
        return "BetterAutocastVPE.SettingsCategory".Translate();
    }

    private static bool UnregisterPatch(
        Harmony harmony,
        Type targetType,
        string methodName,
        HarmonyPatchType harmonyPatchType,
        string harmonyID
    )
    {
        try
        {
            // Get the MethodInfo for the private method using reflection
            MethodInfo targetMethod = targetType.GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
            );

            if (targetMethod != null)
            {
                // Attempt to remove the patch by specifying the MethodInfo
                harmony.Unpatch(targetMethod, harmonyPatchType, harmonyID);
                return true; // Patch removed successfully
            }
            else
            {
                Log.Error($"Method '{methodName}' not found in type '{targetType.FullName}'.");
                return false; // Method not found
            }
        }
        catch (Exception e)
        {
            // Handle any exceptions that may occur during unregistration
            Log.Error(text: "Error unregistering Harmony patch: " + e);
            return false;
        }
    }
}

public class AutocastModSettings : ModSettings
{
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

    public HashSet<string> DraftedAutocastDefs =
        new()
        {
            "VPE_SpeedBoost",
            "VPE_BladeFocus",
            "VPE_FiringFocus",
            "VPE_AdrenalineRush",
            "VPE_ControlledFrenzy",
            "VPE_GuidedShot",
        };
    public HashSet<string> UndraftedAutocastDefs =
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
            "VPE_Invisibility",
            "VPE_WordofImmunity",
        };

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

        Scribe_Collections.Look(
            ref DraftedAutocastDefs,
            nameof(DraftedAutocastDefs),
            LookMode.Value
        );

        Scribe_Collections.Look(
            ref UndraftedAutocastDefs,
            false,
            nameof(UndraftedAutocastDefs),
            LookMode.Value
        );
    }
}
