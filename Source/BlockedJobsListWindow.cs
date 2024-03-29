using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using VFECore.UItils;

namespace BetterAutocastVPE;

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

    private Vector2 scrollPosition = new();
    public override Vector2 InitialSize => new Vector2(900f, 700f);

    public override RimWorld.QuickSearchWidget CommonSearchWidget => base.CommonSearchWidget;

    public override void DoWindowContents(Rect inRect2)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(
            new Rect(0f, 0f, inRect2.width - 150f - 17f, 35f),
            "BetterAutocastVPE.BlockedJobs".Translate()
        );
        Text.Font = GameFont.Small;
        Rect inRect = new Rect(
            0f,
            40f,
            inRect2.width,
            inRect2.height - 40f - Window.CloseButSize.y
        );

        BetterAutocastVPE.DebugLog(
            $"{nameof(BlockedJobsListWindow)}.{nameof(listingHeight)} = {listingHeight}"
        );
        Rect viewRect = new(inRect.x, inRect.y, inRect.width - 16f, listingHeight);
        Widgets.BeginScrollView(
            inRect.TopPartPixels(inRect.height - Window.CloseButSize.y),
            ref scrollPosition,
            viewRect
        );
        Listing_Standard listing = new();
        listing.Begin(new Rect(viewRect.x, viewRect.y, viewRect.width, float.PositiveInfinity));

        listing.Label("BetterAutocastVPE.BlockedJobs.Manager.Explanation".Translate());
        listing.GapLine();

        float thirdColumnWidth = 24f;
        float firstColumnWidth = (listing.ColumnWidth - thirdColumnWidth) / 2f;
        float secondColumnWidth = firstColumnWidth;
        float headerHeight = Math.Max(
            Text.CalcHeight(
                "BetterAutocastVPE.BlockedJobs.Nonexistent.defNameExplanation".Translate(),
                firstColumnWidth
            ),
            Text.CalcHeight(
                "BetterAutocastVPE.BlockedJobs.Nonexistent.reportStringExplanation".Translate(),
                secondColumnWidth
            )
        );
        Rect headerRow = listing.GetRect(headerHeight);
        Widgets.Label(
            headerRow.TakeLeftPart(firstColumnWidth),
            "BetterAutocastVPE.BlockedJobs.Nonexistent.defNameExplanation".Translate()
        );
        Widgets.Label(
            headerRow.TakeLeftPart(secondColumnWidth),
            "BetterAutocastVPE.BlockedJobs.Nonexistent.reportStringExplanation".Translate()
        );

        List<JobDef> allDefsAlphabetic = DefDatabase<JobDef>
            .AllDefs.OrderBy(def => def.defName[0])
            .ToList();
        foreach (JobDef jobDef in allDefsAlphabetic)
        {
            float rowHeight = Math.Max(
                Text.CalcHeight(jobDef.defName, firstColumnWidth),
                Text.CalcHeight(jobDef.reportString.TrimEnd('.'), secondColumnWidth)
            );
            Rect jobDefRow = listing.GetRect(rowHeight);
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

        listing.Label("BetterAutocastVPE.BlockedJobs.Nonexistent.Explanation".Translate());

        HashSet<string> allDefNames = DefDatabase<JobDef>
            .AllDefs.Select(def => def.defName)
            .ToHashSet();
        IEnumerable<string> nonexistentDefNames = BetterAutocastVPE.Settings.BlockedJobDefs.Where(
            defName => !allDefNames.Contains(defName)
        );
        foreach (string nonexistentDefName in nonexistentDefNames)
        {
            float rowHeight = Text.CalcHeight("X", listing.ColumnWidth - 20f);
            Rect row = listing.GetRect(rowHeight);
            Rect buttonRect = row.TakeRightPart(rowHeight);
            Widgets.Label(row, nonexistentDefName);
            if (Widgets.ButtonText(buttonRect, "X"))
                BetterAutocastVPE.Settings.BlockedJobDefs.Remove(nonexistentDefName);
        }

        listingHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();
    }
}
