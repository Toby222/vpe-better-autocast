using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using VFECore.UItils;

namespace BetterAutocastVPE.Settings.Windows;

public class BlockedJobsListWindow : Window
{
    public BlockedJobsListWindow()
    {
        doCloseX = true;
        doCloseButton = true;
        closeOnClickedOutside = true;
        absorbInputAroundWindow = true;
    }

    private static float listingHeight;
    private static string searchString = string.Empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesSearch(string defName, string label)
    {
        return (defName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
            || (label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private Vector2 scrollPosition = new();
    public override Vector2 InitialSize => new(900f, 700f);

    public override RimWorld.QuickSearchWidget CommonSearchWidget => base.CommonSearchWidget;

    private bool enableAllConfirm;
    private bool disableAllConfirm;

    public override void DoWindowContents(Rect windowInRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(
            new Rect(0f, 0f, windowInRect.width - 150f - 17f, 35f),
            "BetterAutocastVPE.BlockedJobs".TranslateSafe()
        );
        Text.Font = GameFont.Small;
        Rect inRect = new(
            0f,
            40f,
            windowInRect.width,
            windowInRect.height - 40f - CloseButSize.y
        );

        searchString = Widgets.TextArea(inRect.TakeTopPart(30f), searchString);

        Rect viewRect = new(inRect.x, inRect.y, inRect.width - 16f, listingHeight);
        Widgets.BeginScrollView(
            inRect.TopPartPixels(inRect.height - CloseButSize.y),
            ref scrollPosition,
            viewRect
        );
        Listing_Standard listing = new();
        listing.Begin(new Rect(viewRect.x, viewRect.y, viewRect.width, float.PositiveInfinity));

        listing.Label("BetterAutocastVPE.BlockedJobs.Manager.Explanation".TranslateSafe());
        listing.GapLine();
        Rect rect = listing.GetRect(30f);
        if (enableAllConfirm)
        {
            Color prevColor = GUI.color;
            GUI.color = Color.red;
            if (
                Widgets.ButtonText(
                    rect.LeftHalf(),
                    "BetterAutocastVPE.BlockedJobs.EnableAll.Confirmation".TranslateSafe()
                )
            )
            {
                BetterAutocastVPE.Settings.BlockedJobDefs.UnionWith(
                    DefDatabase<JobDef>.AllDefs.Select(jobDef => jobDef.defName)
                );
                enableAllConfirm = false;
            }
            GUI.color = prevColor;
        }
        else if (
            Widgets.ButtonText(
                rect.LeftHalf(),
                "BetterAutocastVPE.BlockedJobs.EnableAll".TranslateSafe()
            )
        )
        {
            enableAllConfirm = true;
        }

        if (disableAllConfirm)
        {
            Color prevColor = GUI.color;
            GUI.color = Color.red;
            if (
                Widgets.ButtonText(
                    rect.RightHalf(),
                    "BetterAutocastVPE.BlockedJobs.DisableAll.Confirmation".TranslateSafe()
                )
            )
            {
                BetterAutocastVPE.Settings.BlockedJobDefs.Clear();
                disableAllConfirm = false;
            }
            GUI.color = prevColor;
        }
        else if (
            Widgets.ButtonText(
                rect.RightHalf(),
                "BetterAutocastVPE.BlockedJobs.DisableAll".TranslateSafe()
            )
        )
        {
            disableAllConfirm = true;
        }

        const float thirdColumnWidth = 24f;
        float firstColumnWidth = (listing.ColumnWidth - thirdColumnWidth) / 2f;
        float secondColumnWidth = firstColumnWidth;
        float headerHeight = Math.Max(
            Text.CalcHeight(
                "BetterAutocastVPE.BlockedJobs.defNameExplanation".TranslateSafe(),
                firstColumnWidth
            ),
            Text.CalcHeight(
                "BetterAutocastVPE.BlockedJobs.reportStringExplanation".TranslateSafe(),
                secondColumnWidth
            )
        );
        Rect headerRow = listing.GetRect(headerHeight);
        Widgets.Label(
            headerRow.TakeLeftPart(firstColumnWidth),
            "BetterAutocastVPE.BlockedJobs.defNameExplanation".TranslateSafe()
        );
        Widgets.Label(
            headerRow.TakeLeftPart(secondColumnWidth),
            "BetterAutocastVPE.BlockedJobs.reportStringExplanation".TranslateSafe()
        );

        listing.Gap();

        bool highlight = false;
        foreach (JobDef jobDef in DefDatabase<JobDef>.AllDefs.OrderBy(def => def.defName[0]))
        {
            if (!MatchesSearch(jobDef.defName, jobDef.reportString))
                continue;

            float rowHeight = Math.Max(
                Text.CalcHeight(jobDef.defName, firstColumnWidth),
                Text.CalcHeight(jobDef.reportString.TrimEnd('.'), secondColumnWidth)
            );
            Rect jobDefRow = listing.GetRect(rowHeight);
            if (highlight)
            {
                Color oldColor = GUI.color;
                GUI.color = new Color(0.75f, 0.75f, 0.85f, 1f);
                GUI.DrawTexture(jobDefRow, TexUI.HighlightTex);
                GUI.color = oldColor;
            }
            highlight = !highlight;
            Rect toggleButtonRect = jobDefRow.TakeRightPart(thirdColumnWidth);
            Widgets.Label(jobDefRow.LeftHalf(), jobDef.defName);
            Widgets.Label(jobDefRow.RightHalf(), jobDef.reportString.TrimEnd('.'));
            bool isEnabled = BetterAutocastVPE.Settings.BlockedJobDefs.Contains(jobDef.defName);
            bool isEnabledOriginal = isEnabled;
            Widgets.Checkbox(toggleButtonRect.xMin, toggleButtonRect.yMin, ref isEnabled);
            if (isEnabled != isEnabledOriginal)
            {
                if (isEnabled)
                    BetterAutocastVPE.Settings.BlockedJobDefs.Add(jobDef.defName);
                else
                    BetterAutocastVPE.Settings.BlockedJobDefs.Remove(jobDef.defName);
            }
        }

        listing.GapLine();

        listing.Label("BetterAutocastVPE.BlockedJobs.Nonexistent.Explanation".TranslateSafe());

        HashSet<string> allDefNames = DefDatabase<JobDef>
            .AllDefs.Select(def => def.defName)
            .ToHashSet();
        HashSet<string> nonexistentDefNames = BetterAutocastVPE
            .Settings.BlockedJobDefs.Where(defName => !allDefNames.Contains(defName))
            .ToHashSet();

        highlight = false;
        foreach (string nonexistentDefName in nonexistentDefNames)
        {
            float rowHeight = Math.Max(
                Text.CalcHeight(nonexistentDefName, listing.ColumnWidth - 20f),
                Text.CalcHeight("X", 20f)
            );
            Rect row = listing.GetRect(rowHeight);
            if (highlight)
            {
                Color oldColor = GUI.color;
                GUI.color = new Color(0.75f, 0.75f, 0.85f, 1f);
                GUI.DrawTexture(row, TexUI.HighlightTex);
                GUI.color = oldColor;
            }
            highlight = !highlight;
            Rect buttonRect = row.TakeRightPart(rowHeight);
            Widgets.Label(row, nonexistentDefName);
            if (Widgets.ButtonText(buttonRect, "X"))
                BetterAutocastVPE.Settings.BlockedJobDefs.Remove(nonexistentDefName);
        }
        if (nonexistentDefNames.Count == 0)
            listing.Label("BetterAutocastVPE.NoNonexistentDefs".TranslateSafe());

        listingHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();
    }
}
