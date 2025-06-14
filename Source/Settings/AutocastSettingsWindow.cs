using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.UItils;

namespace BetterAutocastVPE.Settings;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VanillaPsycastsExpanded;
using Windows;

public static class AutocastSettingsWindow
{
    private static Vector2 settingsScrollPosition = new();

    private static float settingsHeight;
    private static string searchString = string.Empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesSearch(string value)
    {
        return value.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool confirmUninstall;
    public static bool confirmReset;

    private static AutocastSettings Settings => BetterAutocastVPE.Settings;

    private static readonly HashSet<string> expandedDefs = [];
    private static readonly HashSet<string> expandedPaths = [];

#if DEBUG
    private static PsycasterPathDef? currentPsycasterPath;
#endif

    static bool PsycasterPathHeader(Listing_Standard listing, string psycasterPathDefName)
    {
        PsycasterPathDef psycasterPathDef = DefDatabase<PsycasterPathDef>.GetNamed(
            psycasterPathDefName,
            false
        );
        if (psycasterPathDef is null)
        {
            BetterAutocastVPE.Warn(
                $"Tried to configure path {psycasterPathDefName}, but it is not installed",
                (psycasterPathDefName + "notInstalled").GetHashCode()
            );
            return false;
        }

#if DEBUG
        currentPsycasterPath = psycasterPathDef;
#endif

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            return true;
        }

        listing.GapLine();

        bool expanded = expandedPaths.Contains(psycasterPathDefName);

        Rect psycasterPathRow = listing.GetRect(
            Text.CalcHeight(psycasterPathDef.LabelCap, listing.ColumnWidth)
        );
        Widgets.DrawHighlightIfMouseover(psycasterPathRow);
        if (Widgets.ButtonInvisible(psycasterPathRow))
        {
            if (expanded)
            {
                expandedPaths.Remove(psycasterPathDefName);
            }
            else
            {
                expandedPaths.Add(psycasterPathDefName);
            }
            expanded = !expanded;
        }
        Widgets.DrawTextureFitted(
            psycasterPathRow.TakeLeftPart(psycasterPathRow.height),
            expanded ? TexButton.Collapse : TexButton.Reveal,
            1.0f
        );
        Widgets.Label(psycasterPathRow, psycasterPathDef.LabelCap);

        return expanded;
    }

    static bool AbilityHeader_Soothe(Listing_Standard listing)
    {
        AbilityDef abilityDefFemale = DefDatabase<AbilityDef>.GetNamed("VPE_SootheFemale");
        AbilityDef abilityDefMale = DefDatabase<AbilityDef>.GetNamed("VPE_SootheMale");

        string labelCap = abilityDefFemale.LabelCap + " & " + abilityDefMale.LabelCap;
        if (!MatchesSearch(labelCap))
            return false;

        listing.GapLine();

#if DEBUG
        if (currentPsycasterPath is null)
        {
            BetterAutocastVPE.DebugError(
                "Config for ability Soothe is not in any path section",
                "Soothe_WRONG_PATH!!!!".GetHashCode()
            );
        }
        else if (!currentPsycasterPath.abilities.Contains(abilityDefMale))
        {
            BetterAutocastVPE.DebugError(
                $"Config for ability Soothe is in wrong path section {currentPsycasterPath.defName}",
                "Soothe_WRONG_PATH!!!!".GetHashCode()
            );
        }
#endif

        Rect abilityLabelRow = listing.GetRect(
            Text.CalcHeight(labelCap, listing.ColumnWidth - 12f)
        );

        bool expanded = expandedDefs.Contains("VPE_SootheFemale");
        Widgets.DrawHighlightIfMouseover(abilityLabelRow);
        if (Widgets.ButtonInvisible(abilityLabelRow))
        {
            if (expanded)
            {
                expandedDefs.Remove("VPE_SootheFemale");
            }
            else
            {
                expandedDefs.Add("VPE_SootheFemale");
            }
            expanded = !expanded;
        }
        Widgets.DrawTextureFitted(
            abilityLabelRow.TakeLeftPart(abilityLabelRow.height),
            expanded ? TexButton.Collapse : TexButton.Reveal,
            1.0f
        );
        Widgets.DrawTextureFitted(
            abilityLabelRow.TakeLeftPart(abilityLabelRow.height),
            abilityDefFemale.icon,
            1.0f
        );
        Widgets.DrawTextureFitted(
            abilityLabelRow.TakeLeftPart(abilityLabelRow.height),
            abilityDefMale.icon,
            1.0f
        );
        Widgets.Label(abilityLabelRow, labelCap);

        if (expanded)
        {
            if (
                LanguageDatabase.activeLanguage.HaveTextForKey(
                    "BetterAutocastVPE.Soothe.Explanation"
                )
            )
            {
                string explanation = "BetterAutocastVPE.Soothe.Explanation".TranslateSafe();
                listing.Label(explanation);
            }
            else
            {
                BetterAutocastVPE.DebugError(
                    "Missing text for key BetterAutocastVPE.Soothe.Explanation",
                    "Missing text for key BetterAutocastVPE.Soothe.Explanation".GetHashCode()
                );
            }

            bool castWhileDrafted = Settings.DraftedAutocastDefs.Contains("VPE_SootheFemale");
            bool castWhileDraftedOriginal = castWhileDrafted;

            listing.CheckboxLabeled(
                "BetterAutocastVPE.CastDrafted".TranslateSafe(),
                ref castWhileDrafted
            );

            if (castWhileDrafted != castWhileDraftedOriginal)
            {
                if (castWhileDrafted)
                {
                    Settings.DraftedAutocastDefs.Add("VPE_SootheFemale");
                    Settings.DraftedAutocastDefs.Add("VPE_SootheMale");
                }
                else
                {
                    Settings.DraftedAutocastDefs.Remove("VPE_SootheFemale");
                    Settings.DraftedAutocastDefs.Remove("VPE_SootheMale");
                }
            }

            if (abilityDefFemale.showUndrafted)
            {
                bool castWhileUndrafted = Settings.UndraftedAutocastDefs.Contains(
                    "VPE_SootheFemale"
                );
                bool castWhileUndraftedOriginal = castWhileUndrafted;
                listing.CheckboxLabeled(
                    "BetterAutocastVPE.CastUndrafted".TranslateSafe(),
                    ref castWhileUndrafted
                );

                if (castWhileUndrafted != castWhileUndraftedOriginal)
                {
                    if (castWhileUndrafted)
                    {
                        Settings.UndraftedAutocastDefs.Add("VPE_SootheFemale");
                        Settings.UndraftedAutocastDefs.Add("VPE_SootheMale");
                    }
                    else
                    {
                        Settings.UndraftedAutocastDefs.Remove("VPE_SootheFemale");
                        Settings.UndraftedAutocastDefs.Remove("VPE_SootheMale");
                    }
                }
            }
        }

        return expanded;
    }

    static bool AbilityHeader(Listing_Standard listing, string abilityDefName)
    {
        AbilityDef abilityDef = DefDatabase<AbilityDef>.GetNamed(abilityDefName);

        string labelCap = abilityDef.LabelCap.ToString();
        if (!MatchesSearch(labelCap))
            return false;

        listing.GapLine();

#if DEBUG
        if (currentPsycasterPath is null)
        {
            BetterAutocastVPE.DebugError(
                $"Config for ability {abilityDefName} is not in any path section",
                (abilityDefName + "_WRONG_PATH!!!!").GetHashCode()
            );
        }
        else if (!currentPsycasterPath.abilities.Contains(abilityDef))
        {
            BetterAutocastVPE.DebugError(
                $"Config for ability {abilityDefName} is in wrong path section {currentPsycasterPath.defName}",
                (abilityDefName + "_WRONG_PATH!!!!").GetHashCode()
            );
        }
#endif

        Rect abilityLabelRow = listing.GetRect(
            Text.CalcHeight(abilityDef.LabelCap, listing.ColumnWidth - 12f)
        );

        bool expanded = expandedDefs.Contains(abilityDefName);
        Widgets.DrawHighlightIfMouseover(abilityLabelRow);
        if (Widgets.ButtonInvisible(abilityLabelRow))
        {
            if (expanded)
            {
                expandedDefs.Remove(abilityDefName);
            }
            else
            {
                expandedDefs.Add(abilityDefName);
            }
            expanded = !expanded;
        }
        Widgets.DrawTextureFitted(
            abilityLabelRow.TakeLeftPart(abilityLabelRow.height),
            expanded ? TexButton.Collapse : TexButton.Reveal,
            1.0f
        );
        Widgets.DrawTextureFitted(
            abilityLabelRow.TakeLeftPart(abilityLabelRow.height),
            abilityDef.icon,
            1.0f
        );
        Widgets.Label(abilityLabelRow, abilityDef.LabelCap);

        if (expanded)
        {
            if (
                LanguageDatabase.activeLanguage.HaveTextForKey(
                    $"BetterAutocastVPE.{abilityDefName}.Explanation"
                )
            )
            {
                string explanation =
                    $"BetterAutocastVPE.{abilityDefName}.Explanation".TranslateSafe();
                listing.Label(explanation);
            }
            else
            {
                BetterAutocastVPE.DebugError(
                    "Missing text for key " + $"BetterAutocastVPE.{abilityDefName}.Explanation",
                    (
                        "Missing text for key " + $"BetterAutocastVPE.{abilityDefName}.Explanation"
                    ).GetHashCode()
                );
            }

            bool castWhileDrafted = Settings.DraftedAutocastDefs.Contains(abilityDefName);
            bool castWhileDraftedOriginal = castWhileDrafted;

            listing.CheckboxLabeled(
                "BetterAutocastVPE.CastDrafted".TranslateSafe(),
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
                    "BetterAutocastVPE.CastUndrafted".TranslateSafe(),
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

        return expanded;
    }

    static void Checkbox(Listing_Standard listing, string labelKey, ref bool value)
    {
        listing.CheckboxLabeled(("BetterAutocastVPE." + labelKey).TranslateSafe(), ref value);
    }

    public static void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listing = new();

        void Checkbox(string labelKey, ref bool value)
        {
            AutocastSettingsWindow.Checkbox(listing, labelKey, ref value);
        }

        bool AbilityHeader(string abilityDefName, string? modId = null)
        {
            return (modId is null || ModsConfig.IsActive(modId))
                && AutocastSettingsWindow.AbilityHeader(listing, abilityDefName);
        }

        bool PsycasterPathHeader(string psycasterPathDefName, string? modId = null)
        {
            return (modId is null || ModsConfig.IsActive(modId))
                && AutocastSettingsWindow.PsycasterPathHeader(listing, psycasterPathDefName);
        }

        searchString = Widgets.TextField(inRect.TakeTopPart(30f), searchString);
        Rect viewRect = new(inRect.x, inRect.y, inRect.width - 16f, settingsHeight);
        Widgets.BeginScrollView(inRect, ref settingsScrollPosition, viewRect);
        listing.Begin(new Rect(viewRect.x, viewRect.y, viewRect.width, float.PositiveInfinity));

        #region General

        if (string.IsNullOrWhiteSpace(searchString))
        {
            listing.Label("BetterAutocastVPE.Clarification".TranslateSafe());

            listing.GapLine();

            bool inGame = Current.Game is not null;
            string uninstallLabel = inGame
                ? confirmUninstall
                    ? "BetterAutocastVPE.Uninstall.Confirmation"
                    : "BetterAutocastVPE.Uninstall"
                : "BetterAutocastVPE.Uninstall.Disabled";

            listing.Label("BetterAutocastVPE.Uninstall.Explanation".TranslateSafe());

            Color prevColor = GUI.color;
            if (confirmUninstall && inGame)
            {
                GUI.color = Color.red;
            }

            if (listing.ButtonText(uninstallLabel.TranslateSafe()) && inGame)
            {
                if (confirmUninstall)
                    LoadedModManager.GetMod<BetterAutocastVPE>().Uninstall();
                else
                    confirmUninstall = true;
            }

            if (confirmUninstall)
            {
                GUI.color = prevColor;
            }

            listing.GapLine();

            if (confirmReset)
            {
                prevColor = GUI.color;
                GUI.color = Color.red;
                if (
                    listing.ButtonText(
                        "BetterAutocastVPE.ResetSettings.Confirmation".TranslateSafe()
                    )
                )
                {
                    Settings.Reset();
                    confirmReset = false;
                }
                GUI.color = prevColor;
            }
            else if (listing.ButtonText("BetterAutocastVPE.ResetSettings".TranslateSafe()))
            {
                confirmReset = true;
            }

            Checkbox("DebugLog", ref Settings.DebugLog);
            Checkbox("ShowValidationMessages", ref Settings.ShowValidationMessages);
            Settings.AutocastIntervalDrafted = (int)
                listing.SliderLabeled(
                    "BetterAutocastVPE.AutocastIntervalDrafted".TranslateSafe(
                        Settings.AutocastIntervalDrafted
                    ),
                    Settings.AutocastIntervalDrafted,
                    1f,
                    10000f,
                    tooltip: "BetterAutocastVPE.AutocastIntervalDrafted.Description".TranslateSafe(
                        Settings.AutocastIntervalDrafted
                    )
                );
            Settings.AutocastIntervalUndrafted = (int)
                listing.SliderLabeled(
                    "BetterAutocastVPE.AutocastIntervalUndrafted".TranslateSafe(
                        Settings.AutocastIntervalUndrafted
                    ),
                    Settings.AutocastIntervalUndrafted,
                    1f,
                    10000f,
                    tooltip: "BetterAutocastVPE.AutocastIntervalUndrafted.Description".TranslateSafe(
                        Settings.AutocastIntervalUndrafted
                    )
                );

            Settings.MinFocusThreshold = listing.SliderLabeled(
                "BetterAutocastVPE.MinFocusThreshold".TranslateSafe(
                    Settings.MinFocusThreshold.ToString("P")
                ),
                Settings.MinFocusThreshold,
                0f,
                1f,
                tooltip: "BetterAutocastVPE.MinFocusThreshold.Description".TranslateSafe()
            );

            listing.GapLine();
            listing.Label("BetterAutocastVPE.BlockedJobs.Explanation".TranslateSafe());
            if (listing.ButtonText("BetterAutocastVPE.BlockedJobs".TranslateSafe()))
                Find.WindowStack.Add(new BlockedJobsListWindow());
        }

        #endregion General

        float indent = string.IsNullOrWhiteSpace(searchString) ? 12f : 0f;

        if (PsycasterPathHeader("VPE_Archon"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Word of Alliance
            if (AbilityHeader("VPE_WordofAlliance"))
            {
                Checkbox("WordOfAllianceCheckArea", ref Settings.WordOfAllianceCheckAllowedArea);
                listing.Label("BetterAutocastVPE.WordOfAllianceGoodwillRange".TranslateSafe());

                listing.IntRange(ref Settings.WordOfAllianceGoodwill, -100, 100);
            }
            #endregion Word of Alliance

            #region Word of Productivity
            AbilityHeader("VPE_WordofProductivity");
            #endregion Word of Productivity

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        // if (PsycasterPathHeader("VPE_Archotechist")) { listing.Indent(indent); listing.ColumnWidth -= indent; listing.ColumnWidth += indent; listing.Outdent(indent); }
        if (PsycasterPathHeader("VPE_Chronopath"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Craft Timeskip
            if (AbilityHeader("VPE_CraftTimeskip"))
            {
                Checkbox("ShowCraftTimeskipArea", ref Settings.ShowCraftTimeskipArea);
            }
            #endregion Craft Timeskip

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Conflagrator"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Fire Shield
            if (AbilityHeader("VPE_FireShield"))
            {
                Checkbox("TargetSelf", ref Settings.FireShieldTargetSelf);
                Checkbox("TargetColonists", ref Settings.FireShieldTargetColonists);
                Checkbox("TargetSlaves", ref Settings.FireShieldTargetSlaves);
                Checkbox("TargetVisitors", ref Settings.FireShieldTargetVisitors);
            }
            #endregion Fire Shield

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Empath"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Soothe (Female/Male)
            if (AbilityHeader_Soothe(listing))
            {
                Checkbox("SootheColonists", ref Settings.SootheColonistsCheck);
                Settings.SootheColonistsMaximumMood = listing.SliderLabeled(
                    "BetterAutocastVPE.SootheTargetMinimumMood".TranslateSafe(
                        Settings.SootheColonistsMaximumMood.ToStringPercent()
                    ),
                    Settings.SootheColonistsMaximumMood,
                    0.0f,
                    1.0f
                );
                Checkbox("SootheSlaves", ref Settings.SootheSlavesCheck);
                Settings.SootheSlavesMaximumMood = listing.SliderLabeled(
                    "BetterAutocastVPE.SootheTargetMinimumMood".TranslateSafe(
                        Settings.SootheSlavesMaximumMood.ToStringPercent()
                    ),
                    Settings.SootheSlavesMaximumMood,
                    0.0f,
                    1.0f
                );
                Checkbox("SoothePrisoners", ref Settings.SoothePrisonersCheck);
                Settings.SoothePrisonersMaximumMood = listing.SliderLabeled(
                    "BetterAutocastVPE.SootheTargetMinimumMood".TranslateSafe(
                        Settings.SoothePrisonersMaximumMood.ToStringPercent()
                    ),
                    Settings.SoothePrisonersMaximumMood,
                    0.0f,
                    1.0f
                );
                Checkbox("SootheVisitors", ref Settings.SootheVisitorsCheck);
                Settings.SootheVisitorsMaximumMood = listing.SliderLabeled(
                    "BetterAutocastVPE.SootheTargetMinimumMood".TranslateSafe(
                        Settings.SootheVisitorsMaximumMood.ToStringPercent()
                    ),
                    Settings.SootheVisitorsMaximumMood,
                    0.0f,
                    1.0f
                );
            }
            #endregion Soothe (Female/Male)

            #region Word of Joy
            if (AbilityHeader("VPE_WordofJoy"))
            {
                Settings.WordOfJoyMoodThreshold = listing.SliderLabeled(
                    "BetterAutocastVPE.WordOfJoyMoodThreshold".TranslateSafe(
                        Settings.WordOfJoyMoodThreshold.ToString("P")
                    ),
                    Settings.WordOfJoyMoodThreshold,
                    0.0f,
                    1.0f,
                    tooltip: "BetterAutocastVPE.WordOfJoyMoodThreshold.Description".TranslateSafe()
                );
            }
            #endregion Word of Joy

            #region Word of Serenity
            if (AbilityHeader("VPE_WordofSerenity"))
            {
                Checkbox("WordOfSerenityTargetScaria", ref Settings.WordOfSerenityTargetScaria);
                listing.Gap();
                Checkbox("TargetColonists", ref Settings.WordOfSerenityTargetColonists);
                Checkbox("TargetColonyAnimals", ref Settings.WordOfSerenityTargetColonyAnimals);
                Checkbox("TargetWildAnimals", ref Settings.WordOfSerenityTargetWildAnimals);
                Checkbox("TargetSlaves", ref Settings.WordOfSerenityTargetSlaves);
                Checkbox("TargetPrisoners", ref Settings.WordOfSerenityTargetPrisoners);
                Checkbox("TargetVisitors", ref Settings.WordOfSerenityTargetVisitors);
                listing.Gap();

                listing.Label("BetterAutocastVPE.IgnoredMentalStates.Explanation".TranslateSafe());
                if (listing.ButtonText("BetterAutocastVPE.IgnoredMentalStates".TranslateSafe()))
                    Find.WindowStack.Add(new IgnoredMentalStateListWindow());
            }
            #endregion Word of Serenity

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Frostshaper"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Ice Crystal
            if (AbilityHeader("VPE_IceCrystal"))
            {
                Checkbox("ShowIceCrystalArea", ref Settings.ShowIceCrystalArea);
            }
            #endregion Ice Crystal

            #region Ice Shield
            if (AbilityHeader("VPE_IceShield"))
            {
                Checkbox("TargetSelf", ref Settings.IceShieldTargetSelf);
                Checkbox("TargetColonists", ref Settings.IceShieldTargetColonists);
                Checkbox("TargetSlaves", ref Settings.IceShieldTargetSlaves);
                Checkbox("TargetVisitors", ref Settings.IceShieldTargetVisitors);
            }
            #endregion Ice Shield

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Harmonist"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Psychic Guidance
            AbilityHeader("VPE_PsychicGuidance");
            #endregion Psychic Guidance

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Necropath"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Deathshield
            if (AbilityHeader("VPE_Deathshield"))
            {
                Checkbox("TargetColonists", ref Settings.DeathshieldColonists);
                Checkbox("TargetColonyAnimals", ref Settings.DeathshieldColonyAnimals);
                Checkbox("TargetSlaves", ref Settings.DeathshieldSlaves);
                Checkbox("TargetPrisoners", ref Settings.DeathshieldPrisoners);
                Checkbox("TargetVisitors", ref Settings.DeathshieldVisitors);
            }
            #endregion Deathshield

            #region Enthrall
            if (AbilityHeader("VPE_Enthrall"))
            {
                Checkbox("EnthrallInStockpile", ref Settings.EnthrallInStockpile);
                Checkbox("EnthrallOnlyNamedStockpiles", ref Settings.EnthrallOnlyNamedStockpiles);
                Checkbox("EnthrallInStorage", ref Settings.EnthrallInStorage);
#if v1_5
                Checkbox(
                    "EnthrallOnlyNamedStorageGroups",
                    ref Settings.EnthrallOnlyNamedStorageGroups
                );
#else
                throw new NotImplementedException();
#endif
            }
            #endregion Enthrall

            #region Ghostwalk
            AbilityHeader("VPE_Ghostwalk");
            #endregion Ghostwalk

            #region Steal vitality
            if (AbilityHeader("VPE_StealVitality"))
            {
                Checkbox("TargetPrisoners", ref Settings.StealVitalityFromPrisoners);
                Checkbox("TargetSlaves", ref Settings.StealVitalityFromSlaves);
                Checkbox("TargetColonists", ref Settings.StealVitalityFromColonists);
                Checkbox("TargetVisitors", ref Settings.StealVitalityFromVisitors);
            }
            #endregion Steal vitality

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Nightstalker"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Darkvision
            if (AbilityHeader("VPE_Darkvision"))
            {
                Checkbox("TargetSelf", ref Settings.DarkvisionTargetSelf);
                Checkbox("TargetColonists", ref Settings.DarkvisionTargetColonists);
            }
            #endregion Darkvision

            #region Eclipse
            AbilityHeader("VPE_Eclipse");
            #endregion Eclipse

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Protector"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Focus
            if (AbilityHeader("VPE_Focus"))
            {
                Checkbox("TargetSelf", ref Settings.FocusTargetSelf);
                Checkbox("TargetColonists", ref Settings.FocusTargetColonists);
                Checkbox("TargetSlaves", ref Settings.FocusTargetSlaves);
            }
            #endregion Focus

            #region Invisibility
            if (AbilityHeader("VPE_Invisibility"))
            {
                Checkbox("TargetSelf", ref Settings.InvisibilityTargetSelf);
                Checkbox("TargetColonists", ref Settings.InvisibilityTargetColonists);
            }
            #endregion Invisibility

            #region Overshield
            if (AbilityHeader("VPE_Overshield"))
            {
                Checkbox("TargetSelf", ref Settings.OvershieldTargetSelf);
                Checkbox("TargetColonists", ref Settings.OvershieldTargetColonists);
            }
            #endregion Overshield

            #region Word of Immunity
            if (AbilityHeader("VPE_WordofImmunity"))
            {
                Checkbox("TargetColonists", ref Settings.WordOfImmunityTargetColonists);
                Checkbox("TargetColonyAnimals", ref Settings.WordOfImmunityTargetColonyAnimals);
                Checkbox("TargetSlaves", ref Settings.WordOfImmunityTargetSlaves);
                Checkbox("TargetPrisoners", ref Settings.WordOfImmunityTargetPrisoners);
                Checkbox("TargetVisitors", ref Settings.WordOfImmunityTargetVisitors);
            }
            #endregion Word of Immunity

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPEP_Puppeteer", "VanillaExpanded.VPE.Puppeteer"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Brain Leech
            if (AbilityHeader("VPEP_BrainLeech"))
            {
                Checkbox("TargetPrisoners", ref Settings.BrainLeechTargetPrisoners);
                Checkbox("TargetSlaves", ref Settings.BrainLeechTargetSlaves);
            }
            #endregion Brain Leech

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPER_Runesmith", "Chairheir.VPERunesmith"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Etch Runecircle
            if (AbilityHeader("VPER_Etch_Runecircle"))
            {
                Checkbox("ShowRunecircleArea", ref Settings.ShowRunecircleArea);
            }
            #endregion Etch Runecircle
            #region Etch Greater Runecircle
            if (AbilityHeader("VPER_Etch_Runecircle_Greater"))
            {
                Checkbox("ShowGreaterRunecircleArea", ref Settings.ShowGreaterRunecircleArea);
            }
            #endregion Etch Greater Runecircle

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Skipmaster"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Solar Pinhole
            if (AbilityHeader("VPE_SolarPinhole"))
            {
                Checkbox("ShowSolarPinholeArea", ref Settings.ShowSolarPinholeArea);
            }
            #endregion Solar Pinhole

            #region Large Solar Pinhole
            if (AbilityHeader("VPE_SolarPinholeSunlamp", "dgrb.solarpinholeadditions"))
            {
                Checkbox("ShowSolarPinholeArea", ref Settings.ShowSolarPinholeArea);
            }
            #endregion Large Solar Pinhole

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Staticlord"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Static Aura
            if (AbilityHeader("VPE_StaticAura"))
            {
                Checkbox("TargetSelf", ref Settings.StaticAuraTargetSelf);
                Checkbox("TargetColonists", ref Settings.StaticAuraTargetColonists);
                Checkbox("TargetSlaves", ref Settings.StaticAuraTargetSlaves);
                Checkbox("TargetVisitors", ref Settings.StaticAuraTargetVisitors);
            }
            #endregion Static Aura

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Technomancer"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Mend
            if (AbilityHeader("VPE_Mend"))
            {
                Settings.MendHealthThreshold = listing.SliderLabeled(
                    "BetterAutocastVPE.MendHealthThreshold".TranslateSafe(
                        Settings.MendHealthThreshold.ToString("P")
                    ),
                    Settings.MendHealthThreshold,
                    0.0f,
                    1.0f,
                    tooltip: "BetterAutocastVPE.MendHealthThreshold.Description".TranslateSafe()
                );
                Checkbox("MendPawns", ref Settings.MendPawns);
                Checkbox("MendInStockpile", ref Settings.MendInStockpile);
                Checkbox("MendOnlyNamedStockpiles", ref Settings.MendOnlyNamedStockpiles);
                Checkbox("MendInStorage", value: ref Settings.MendInStorage);
#if v1_5
                Checkbox("MendOnlyNamedStorageGroups", ref Settings.MendOnlyNamedStorageGroups);
#else
                throw new NotImplementedException();
#endif
            }
            #endregion Mend

            #region Enchant quality
            if (AbilityHeader("VPE_EnchantQuality"))
            {
                Checkbox("EnchantInStockpile", ref Settings.EnchantInStockpile);
                Checkbox("EnchantOnlyNamedStockpiles", ref Settings.EnchantOnlyNamedStockpiles);
                Checkbox("EnchantInStorage", ref Settings.EnchantInStorage);
#if v1_5
                Checkbox(
                    "EnchantOnlyNamedStorageGroups",
                    ref Settings.EnchantOnlyNamedStorageGroups
                );
#else
                throw new NotImplementedException();
#endif
            }
            #endregion Enchant quality

            #region Power
            if (AbilityHeader("VPE_Power"))
            {
                Checkbox("PowerBuildings", ref Settings.PowerBuildings);
                Checkbox("PowerMechs", ref Settings.PowerMechs);
            }
            #endregion Power

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        if (PsycasterPathHeader("VPE_Warlord"))
        {
            listing.Indent(indent);
            listing.ColumnWidth -= indent;

            #region Adrenaline Rush
            AbilityHeader("VPE_AdrenalineRush");
            #endregion Adrenaline Rush

            #region Blade Focus
            AbilityHeader("VPE_BladeFocus");
            #endregion Blade Focus

            #region Controlled Frenzy
            AbilityHeader("VPE_ControlledFrenzy");
            #endregion Controlled Frenzy

            #region Firing Focus
            AbilityHeader("VPE_FiringFocus");
            #endregion Firing Focus

            #region Guided Shot
            AbilityHeader("VPE_GuidedShot");
            #endregion Guided Shot

            #region Speed boost
            AbilityHeader("VPE_SpeedBoost");
            #endregion Speed boost

            listing.ColumnWidth += indent;
            listing.Outdent(indent);
        }
        // if (PsycasterPathHeader("VPE_Wildspeaker")) { listing.Indent(indent); listing.ColumnWidth -= indent; listing.ColumnWidth += indent; listing.Outdent(indent); }

        listing.End();
        settingsHeight = listing.CurHeight;
        Widgets.EndScrollView();
    }

    public static string SettingsCategory() => "BetterAutocastVPE.SettingsCategory".TranslateSafe();
}
