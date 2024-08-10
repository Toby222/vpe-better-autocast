using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.UItils;

namespace BetterAutocastVPE.Settings;

using System.Collections.Generic;
using System.Linq;
using Windows;

public static class AutocastSettingsWindow
{
    private static Vector2 settingsScrollPosition = new();

    private static float settingsHeight;

    private static AutocastSettings Settings => BetterAutocastVPE.Settings;

    private static readonly HashSet<string> expandedDefs = [];

    static bool AbilityHeader(Listing_Standard listing, string abilityDefName)
    {
        listing.GapLine();

        bool expanded = expandedDefs.Contains(abilityDefName);

        AbilityDef abilityDef = DefDatabase<AbilityDef>.GetNamed(abilityDefName);
        Rect abilityLabelRow = listing.GetRect(
            Text.CalcHeight(abilityDef.LabelCap, listing.ColumnWidth)
        );

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

        return expanded;
    }

    static void Checkbox(Listing_Standard listing, string labelKey, ref bool value)
    {
        listing.CheckboxLabeled(("BetterAutocastVPE." + labelKey).Translate(), ref value);
    }

#if DEBUG
    private readonly static HashSet<string> configuredDefs = [];
#endif

    public static void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listing = new();

        void Checkbox(string labelKey, ref bool value)
        {
            AutocastSettingsWindow.Checkbox(listing, labelKey, ref value);
        }

#if DEBUG
        configuredDefs.Clear();
#endif

        bool AbilityHeader(string abilityDefName)
        {
#if DEBUG
            configuredDefs.Add(abilityDefName);
#endif
            return AutocastSettingsWindow.AbilityHeader(listing, abilityDefName);
        }

        Rect viewRect = new(inRect.x, inRect.y, inRect.width - 16f, settingsHeight);
        Widgets.BeginScrollView(inRect, ref settingsScrollPosition, viewRect);
        listing.Begin(new Rect(viewRect.x, viewRect.y, viewRect.width, float.PositiveInfinity));

        listing.Label("BetterAutocastVPE.Clarification".Translate());

        listing.GapLine();

        #region General

        bool inGame = Current.Game is not null;
        string uninstallLabel = inGame
            ? "BetterAutocastVPE.Uninstall"
            : "BetterAutocastVPE.Uninstall.Disabled";

        listing.Label("BetterAutocastVPE.Uninstall.Explanation".Translate());

        if (listing.ButtonText(uninstallLabel.Translate()) && inGame)
            LoadedModManager.GetMod<BetterAutocastVPE>().Uninstall();

        listing.GapLine();

        if (listing.ButtonText("BetterAutocastVPE.ResetSettings".Translate()))
            LoadedModManager.GetMod<BetterAutocastVPE>().ResetSettings();

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
            Find.WindowStack.Add(new BlockedJobsListWindow());
        #endregion General

        #region Mend
        if (AbilityHeader("VPE_Mend"))
        {
            Settings.MendHealthThreshold = listing.SliderLabeled(
                "BetterAutocastVPE.MendHealthThreshold".Translate(
                    Settings.MendHealthThreshold.ToString("P")
                ),
                Settings.MendHealthThreshold,
                0.0f,
                1.0f,
                tooltip: "BetterAutocastVPE.MendHealthThreshold.Description".Translate()
            );
            Checkbox("MendPawns", ref Settings.MendPawns);
            Checkbox("MendInStockpile", ref Settings.MendInStockpile);
            Checkbox("MendOnlyNamedStockpiles", ref Settings.MendOnlyNamedStockpiles);
            Checkbox("MendInStorage", value: ref Settings.MendInStorage);
#if v1_4
#elif v1_5
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
#if v1_4
#elif v1_5
            Checkbox("EnchantOnlyNamedStorageGroups", ref Settings.EnchantOnlyNamedStorageGroups);
#else
            throw new NotImplementedException();
#endif
        }
        #endregion Enchant quality

        #region Steal vitality
        if (AbilityHeader("VPE_StealVitality"))
        {
            Checkbox("TargetPrisoners", ref Settings.StealVitalityFromPrisoners);
            Checkbox("TargetSlaves", ref Settings.StealVitalityFromSlaves);
            Checkbox("TargetColonists", ref Settings.StealVitalityFromColonists);
            Checkbox("TargetVisitors", ref Settings.StealVitalityFromVisitors);
        }
        #endregion Steal vitality

        #region Deathshield
        if (AbilityHeader("VPE_Deathshield"))
        {
            Checkbox("TargetColonists", ref Settings.DeathshieldColonists);
            Checkbox("TargetColonyAnimals", ref Settings.DeathshieldColonists);
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
#if v1_4
#elif v1_5
            Checkbox("EnthrallOnlyNamedStorageGroups", ref Settings.EnthrallOnlyNamedStorageGroups);
#else
            throw new NotImplementedException();
#endif
        }
        #endregion Enthrall

        #region Word of Joy
        if (AbilityHeader("VPE_WordofJoy"))
        {
            Settings.WordOfJoyMoodThreshold = listing.SliderLabeled(
                "BetterAutocastVPE.WordOfJoyMoodThreshold".Translate(
                    Settings.WordOfJoyMoodThreshold.ToString("P")
                ),
                Settings.WordOfJoyMoodThreshold,
                0.0f,
                1.0f,
                tooltip: "BetterAutocastVPE.WordOfJoyMoodThreshold.Description".Translate()
            );
        }
        #endregion Word of Joy

        #region Puppeteer
        if (
            ModsConfig.IsActive("VanillaExpanded.VPE.Puppeteer") && AbilityHeader("VPEP_BrainLeech")
        )
        {
            Checkbox("TargetPrisoners", ref Settings.BrainLeechTargetPrisoners);
            Checkbox("TargetSlaves", ref Settings.BrainLeechTargetSlaves);
        }
        #endregion Puppeteer

        #region Adrenaline Rush
        AbilityHeader("VPE_AdrenalineRush");
        #endregion Adrenaline Rush

        #region Blade Focus
        AbilityHeader("VPE_BladeFocus");
        #endregion Blade Focus

        #region Controlled Frenzy
        AbilityHeader("VPE_ControlledFrenzy");
        #endregion Controlled Frenzy

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

        #region Firing Focus
        AbilityHeader("VPE_FiringFocus");
        #endregion Firing Focus

        #region Guided Shot
        AbilityHeader("VPE_GuidedShot");
        #endregion Guided Shot

        #region Psychic Guidance
        AbilityHeader("VPE_PsychicGuidance");
        #endregion Psychic Guidance

        #region Speed boost
        AbilityHeader("VPE_SpeedBoost");
        #endregion Speed boost

        #region Word of Productivity
        AbilityHeader("VPE_WordofProductivity");
        #endregion Word of Productivity

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

            listing.Label("BetterAutocastVPE.IgnoredMentalStates.Explanation".Translate());
            if (listing.ButtonText("BetterAutocastVPE.IgnoredMentalStates".Translate()))
                Find.WindowStack.Add(new IgnoredMentalStateListWindow());
        }
        #endregion Word of Serenity

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

        #region Ice Crystal
        AbilityHeader("VPE_IceCrystal");
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

        #region Fire Shield
        if (AbilityHeader("VPE_FireShield"))
        {
            Checkbox("TargetSelf", ref Settings.FireShieldTargetSelf);
            Checkbox("TargetColonists", ref Settings.FireShieldTargetColonists);
            Checkbox("TargetSlaves", ref Settings.FireShieldTargetSlaves);
            Checkbox("TargetVisitors", ref Settings.FireShieldTargetVisitors);
        }
        #endregion Fire Shield

        #region Static Aura
        if (AbilityHeader("VPE_StaticAura"))
        {
            Checkbox("TargetSelf", ref Settings.StaticAuraTargetSelf);
            Checkbox("TargetColonists", ref Settings.StaticAuraTargetColonists);
            Checkbox("TargetSlaves", ref Settings.StaticAuraTargetSlaves);
            Checkbox("TargetVisitors", ref Settings.StaticAuraTargetVisitors);
        }
        #endregion Static Aura

        #region Solar Pinhole
        AbilityHeader("VPE_SolarPinhole");
        #endregion Solar Pinhole

        #region Large Solar Pinhole
        if (ModsConfig.IsActive("dgrb.solarpinholeadditions"))
        {
            AbilityHeader("VPE_SolarPinholeSunlamp");
        }
        #endregion Large Solar Pinhole

        listing.End();
        settingsHeight = listing.CurHeight;
        Widgets.EndScrollView();

#if DEBUG
        configuredDefs.SymmetricExceptWith(PsycastingHandler.abilityHandlers.Keys.ToHashSet());
        if (configuredDefs.Count > 0)
        {
            BetterAutocastVPE.DebugWarn(
                "Config doesn't properly config everything. Missing: "
                    + configuredDefs.ToCommaList(true)
            );
        }
#endif
    }

    public static string SettingsCategory() => "BetterAutocastVPE.SettingsCategory".Translate();
}
