using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.UItils;

namespace BetterAutocastVPE;

public partial class BetterAutocastVPE : Mod
{
    public BetterAutocastVPE(ModContentPack content)
        : base(content)
    {
#if DEBUG
        const string build = "Debug";
#else
        const string build = "Release";
#endif
        Log(
            $"Running Version {Assembly.GetAssembly(typeof(BetterAutocastVPE)).GetName().Version} "
                + build
        );

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
            Log("UnregisterPatch succeeded");
        }
        else
        {
            Error("UnregisterPatch failed");
        }
        harmony.PatchAll();
        Settings = GetSettings<AutocastModSettings>();
        // In case some of the values were null (e.g. added between versions), write with default values.
        WriteSettings();
    }

#nullable disable // Set in constructor.

    public static AutocastModSettings Settings { get; private set; }

#nullable enable

    private static Vector2 settingsScrollPosition = new();

    private static float settingsHeight;

    public override void DoSettingsWindowContents(Rect inRect)
    {
        static void AbilityHeader(Listing_Standard listing, string abilityDefName)
        {
            AbilityDef abilityDef = DefDatabase<AbilityDef>.GetNamed(abilityDefName);
            Rect abilityLabelRow = listing.GetRect(
                Text.CalcHeight(abilityDef.LabelCap, listing.ColumnWidth)
            );

            Widgets.DrawHighlightIfMouseover(abilityLabelRow);
            Widgets.DrawTextureFitted(
                abilityLabelRow.TakeLeftPart(abilityLabelRow.height),
                abilityDef.icon,
                1.0f
            );
            Widgets.Label(abilityLabelRow, abilityDef.LabelCap);

            if (
                LanguageDatabase.activeLanguage.HaveTextForKey(
                    $"BetterAutocastVPE.{abilityDefName}.Explanation"
                )
            )
            {
                string explanation = $"BetterAutocastVPE.{abilityDefName}.Explanation".Translate();
                listing.Label(explanation);
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
                    Settings.DraftedAutocastDefs.Add(abilityDefName);
                else
                    Settings.DraftedAutocastDefs.Remove(abilityDefName);
            }

            if (abilityDef.showUndrafted)
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
                        Settings.UndraftedAutocastDefs.Add(abilityDefName);
                    else
                        Settings.UndraftedAutocastDefs.Remove(abilityDefName);
                }
            }
        }

        Listing_Standard listing = new();

        Rect viewRect = new(inRect.x, inRect.y, inRect.width - 16f, settingsHeight);
        Widgets.BeginScrollView(inRect, ref settingsScrollPosition, viewRect);
        listing.Begin(new Rect(viewRect.x, viewRect.y, viewRect.width, float.PositiveInfinity));

        listing.Label("BetterAutocastVPE.Clarification".Translate());

        listing.GapLine();

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

        listing.Label("BetterAutocastVPE.BlockedJobs.Explanation".Translate());
        if (listing.ButtonText("BetterAutocastVPE.BlockedJobs".Translate()))
        {
            Find.WindowStack.Add(new BlockedJobsListWindow());
        }

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
                Error($"Method '{methodName}' not found in type '{targetType.FullName}'.");
                return false; // Method not found
            }
        }
        catch (Exception e)
        {
            // Handle any exceptions that may occur during unregistration
            Error("Error unregistering Harmony patch: " + e);
            return false;
        }
    }

    const string LogPrefix = "Better Autocasting - ";

    public static void DebugError(string message)
    {
#if DEBUG
        Error(message);
#endif
    }

    public static void Error(string message)
    {
        Verse.Log.Error(LogPrefix + message);
    }

    public static void DebugWarn(string message)
    {
#if DEBUG
        Warn(message);
#endif
    }

    public static void Warn(string message)
    {
        Verse.Log.Warning(LogPrefix + message);
    }

    public static void DebugLog(string message)
    {
#if DEBUG
        Log(message);
#endif
    }

    public static void Log(string message)
    {
        Verse.Log.Message(LogPrefix + message);
    }
}
