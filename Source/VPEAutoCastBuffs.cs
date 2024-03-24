using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using VFECore.UItils;

namespace VPEAutoCastBuffs;

public class VPEAutoCastBuffs : Mod
{
    public VPEAutoCastBuffs(ModContentPack content)
        : base(content)
    {
        Harmony harmony = new("dev.tobot.vpeautocastbuffs");
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
        Settings = GetSettings<AutoCastModSettings>();
    }

#nullable disable
    public static AutoCastModSettings Settings { get; private set; }

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
                    $"VPEAutoCastBuffs.{abilityDefName}.Explanation"
                )
            )
            {
                string explanation = $"VPEAutoCastBuffs.{abilityDefName}.Explanation".Translate();
                Widgets.Label(
                    listing.GetRect(Text.CalcHeight(explanation, listing.ColumnWidth)),
                    explanation
                );
            }

            bool castWhileDrafted = Settings.DraftedAutoCastDefs.Contains(abilityDefName);
            bool castWhileDraftedOriginal = castWhileDrafted;
            listing.CheckboxLabeled(
                "VPEAutoCastBuffs.CastDrafted".Translate(),
                ref castWhileDrafted
            );

            if (castWhileDrafted != castWhileDraftedOriginal)
            {
                if (castWhileDrafted)
                {
                    Settings.DraftedAutoCastDefs.Add(abilityDefName);
                }
                else
                {
                    Settings.DraftedAutoCastDefs.Remove(abilityDefName);
                }
            }

            bool castWhileUndrafted = Settings.UndraftedAutoCastDefs.Contains(abilityDefName);
            bool castWhileUndraftedOriginal = castWhileUndrafted;
            listing.CheckboxLabeled(
                "VPEAutoCastBuffs.CastUndrafted".Translate(),
                ref castWhileUndrafted
            );

            if (castWhileUndrafted != castWhileUndraftedOriginal)
            {
                if (castWhileUndrafted)
                {
                    Settings.UndraftedAutoCastDefs.Add(abilityDefName);
                }
                else
                {
                    Settings.UndraftedAutoCastDefs.Remove(abilityDefName);
                }
            }
        }

        AutoCastModSettings settings = GetSettings<AutoCastModSettings>();

        Listing_Standard listing = new();

        Rect viewRect = new(inRect.x, inRect.y, inRect.width - 16f, settingsHeight);
        Widgets.BeginScrollView(inRect, ref settingsScrollPosition, viewRect);
        listing.Begin(new Rect(viewRect.x, viewRect.y, viewRect.width, 10000f));

        settings.AutoCastIntervalDrafted = (int)
            listing.SliderLabeled(
                "VPEAutoCastBuffs.AutoCastIntervalDrafted".Translate(
                    settings.AutoCastIntervalDrafted
                ),
                settings.AutoCastIntervalDrafted,
                1f,
                10000f,
                tooltip: "VPEAutoCastBuffs.AutoCastIntervalDrafted.Description".Translate(
                    settings.AutoCastIntervalDrafted
                )
            );
        settings.AutoCastIntervalUndrafted = (int)
            listing.SliderLabeled(
                "VPEAutoCastBuffs.AutoCastIntervalUndrafted".Translate(
                    settings.AutoCastIntervalUndrafted
                ),
                settings.AutoCastIntervalUndrafted,
                1f,
                10000f,
                tooltip: "VPEAutoCastBuffs.AutoCastIntervalUndrafted.Description".Translate(
                    settings.AutoCastIntervalUndrafted
                )
            );

        listing.GapLine();

        AbilityHeader(listing, "VPE_Mend");

        settings.MendHealthThreshold = listing.SliderLabeled(
            "VPEAutoCastBuffs.MendHealthThreshold".Translate(
                settings.MendHealthThreshold.ToString("P")
            ),
            settings.MendHealthThreshold,
            0.0f,
            1.0f,
            tooltip: "VPEAutoCastBuffs.MendHealthThreshold.Description".Translate()
        );
        listing.CheckboxLabeled("VPEAutoCastBuffs.MendPawns".Translate(), ref settings.MendPawns);
        listing.CheckboxLabeled(
            "VPEAutoCastBuffs.MendInStockpile".Translate(),
            ref settings.MendInStockpile
        );
        listing.CheckboxLabeled(
            "VPEAutoCastBuffs.MendInStorage".Translate(),
            ref settings.MendInStorage
        );

        listing.GapLine();

        AbilityHeader(listing, "VPE_EnchantQuality");
        listing.CheckboxLabeled(
            "VPEAutoCastBuffs.EnchantInStockpile".Translate(),
            ref settings.EnchantInStockpile
        );
        listing.CheckboxLabeled(
            "VPEAutoCastBuffs.EnchantInStorage".Translate(),
            ref settings.EnchantInStorage
        );

        listing.GapLine();

        AbilityHeader(listing, "VPE_AdrenalineRush");
        listing.GapLine();
        AbilityHeader(listing, "VPE_BladeFocus");
        listing.GapLine();
        AbilityHeader(listing, "VPE_ControlledFrenzy");
        listing.GapLine();
        AbilityHeader(listing, "VPE_Darkvision");
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
        AbilityHeader(listing, "VPE_StealVitality");
        listing.GapLine();
        AbilityHeader(listing, "VPE_WordofJoy");
        listing.GapLine();
        AbilityHeader(listing, "VPE_WordofProductivity");
        listing.GapLine();
        AbilityHeader(listing, "VPE_WordofSerenity");
        listing.GapLine();
        if (
            ModsConfig.ActiveModsInLoadOrder.Any(x =>
                x.SamePackageId("VanillaExpanded.VPE.Puppeteer", ignorePostfix: true)
            )
        )
        {
            AbilityHeader(listing, "VPEP_BrainLeech");
            listing.GapLine();
        }

        listing.End();
        settingsHeight = listing.CurHeight;
        Widgets.EndScrollView();
    }

    public override string SettingsCategory()
    {
        return "VPEAutoCastBuffs.SettingsCategory".Translate();
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

public class AutoCastModSettings : ModSettings
{
    public int AutoCastIntervalDrafted = 30;
    public int AutoCastIntervalUndrafted = 600;

    public float MendHealthThreshold = 0.5f;
    public bool MendPawns = true;
    public bool MendInStorage = true;
    public bool MendInStockpile = true;

    public bool EnchantInStorage = true;
    public bool EnchantInStockpile = true;

    public HashSet<string> DraftedAutoCastDefs =
        new()
        {
            "VPE_SpeedBoost",
            "VPE_BladeFocus",
            "VPE_FiringFocus",
            "VPE_AdrenalineRush",
            "VPE_ControlledFrenzy",
            "VPE_GuidedShot",
        };
    public HashSet<string> UndraftedAutoCastDefs =
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
        };

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(
            ref AutoCastIntervalDrafted,
            nameof(AutoCastIntervalDrafted),
            defaultValue: 30
        );
        Scribe_Values.Look(
            ref AutoCastIntervalUndrafted,
            nameof(AutoCastIntervalUndrafted),
            defaultValue: 600
        );

        Scribe_Values.Look(ref MendHealthThreshold, nameof(MendHealthThreshold));
        Scribe_Values.Look(ref MendPawns, nameof(MendPawns), defaultValue: true);
        Scribe_Values.Look(ref MendInStorage, nameof(MendInStorage), defaultValue: true);
        Scribe_Values.Look(ref MendInStockpile, nameof(MendInStockpile), defaultValue: true);

        Scribe_Values.Look(ref EnchantInStorage, nameof(EnchantInStorage), defaultValue: true);
        Scribe_Values.Look(ref EnchantInStockpile, nameof(EnchantInStockpile), defaultValue: true);

        Scribe_Collections.Look(
            ref DraftedAutoCastDefs,
            nameof(DraftedAutoCastDefs),
            LookMode.Value
        );

        Scribe_Collections.Look(
            ref UndraftedAutoCastDefs,
            false,
            nameof(UndraftedAutoCastDefs),
            LookMode.Value
        );
    }
}
