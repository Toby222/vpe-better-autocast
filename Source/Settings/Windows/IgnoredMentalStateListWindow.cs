using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using VFECore.UItils;

namespace BetterAutocastVPE.Settings.Windows;

public class IgnoredMentalStateListWindow : Window
{
    public IgnoredMentalStateListWindow()
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
            "BetterAutocastVPE.IgnoredMentalStates".Translate()
        );
        Text.Font = GameFont.Small;
        Rect inRect = new Rect(
            0f,
            40f,
            inRect2.width,
            inRect2.height - 40f - Window.CloseButSize.y
        );

        BetterAutocastVPE.DebugLog(
            $"{nameof(IgnoredMentalStateListWindow)}.{nameof(listingHeight)} = {listingHeight}"
        );
        Rect viewRect = new(inRect.x, inRect.y, inRect.width - 16f, listingHeight);
        Widgets.BeginScrollView(
            inRect.TopPartPixels(inRect.height - Window.CloseButSize.y),
            ref scrollPosition,
            viewRect
        );
        Listing_Standard listing = new();
        listing.Begin(new Rect(viewRect.x, viewRect.y, viewRect.width, float.PositiveInfinity));

        listing.Label("BetterAutocastVPE.IgnoredMentalStates.Manager.Explanation".Translate());
        listing.GapLine();

        const float thirdColumnWidth = 24f;
        float firstColumnWidth = (listing.ColumnWidth - thirdColumnWidth) / 2f;
        float secondColumnWidth = firstColumnWidth;
        float headerHeight = Math.Max(
            Text.CalcHeight(
                "BetterAutocastVPE.IgnoredMentalStates.defNameExplanation".Translate(),
                firstColumnWidth
            ),
            Text.CalcHeight(
                "BetterAutocastVPE.IgnoredMentalStates.labelExplanation".Translate(),
                secondColumnWidth
            )
        );
        Rect headerRow = listing.GetRect(headerHeight);
        Widgets.Label(
            headerRow.TakeLeftPart(firstColumnWidth),
            "BetterAutocastVPE.IgnoredMentalStates.defNameExplanation".Translate()
        );
        Widgets.Label(
            headerRow.TakeLeftPart(secondColumnWidth),
            "BetterAutocastVPE.IgnoredMentalStates.labelExplanation".Translate()
        );

        listing.Gap();

        List<MentalStateDef> allDefsAlphabetic = DefDatabase<MentalStateDef>
            .AllDefs.OrderBy(def => def.defName[0])
            .ToList();
        bool highlight = false;
        foreach (MentalStateDef MentalStateDef in allDefsAlphabetic)
        {
            float rowHeight = Math.Max(
                Text.CalcHeight(MentalStateDef.defName, firstColumnWidth),
                Text.CalcHeight(MentalStateDef.LabelCap, secondColumnWidth)
            );
            Rect mentalStateDefRow = listing.GetRect(rowHeight);
            if (highlight)
            {
                Color oldColor = GUI.color;
                GUI.color = new Color(0.75f, 0.75f, 0.85f, 1f);
                GUI.DrawTexture(mentalStateDefRow, TexUI.HighlightTex);
                GUI.color = oldColor;
            }
            highlight = !highlight;
            Rect toggleButtonRect = mentalStateDefRow.TakeRightPart(thirdColumnWidth);
            Widgets.Label(mentalStateDefRow.LeftHalf(), MentalStateDef.defName);
            Widgets.Label(mentalStateDefRow.RightHalf(), MentalStateDef.LabelCap);
            bool isEnabled =
                BetterAutocastVPE.Settings.WordOfSerenityIgnoredMentalStateDefs.Contains(
                    MentalStateDef.defName
                );
            bool isEnabledOriginal = isEnabled;
            Widgets.Checkbox(toggleButtonRect.xMin, toggleButtonRect.yMin, ref isEnabled);
            if (isEnabled != isEnabledOriginal)
            {
                if (isEnabled)
                {
                    BetterAutocastVPE.Settings.WordOfSerenityIgnoredMentalStateDefs.Add(
                        MentalStateDef.defName
                    );
                }
                else
                {
                    BetterAutocastVPE.Settings.WordOfSerenityIgnoredMentalStateDefs.Remove(
                        MentalStateDef.defName
                    );
                }
            }
        }

        listing.GapLine();

        listing.Label("BetterAutocastVPE.IgnoredMentalStates.Nonexistent.Explanation".Translate());

        HashSet<string> allDefNames = DefDatabase<MentalStateDef>
            .AllDefs.Select(def => def.defName)
            .ToHashSet();
        HashSet<string> nonexistentDefNames = BetterAutocastVPE
            .Settings.WordOfSerenityIgnoredMentalStateDefs.Where(defName =>
                !allDefNames.Contains(defName)
            )
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
            Rect buttonRect = row.TakeRightPart(20f);
            Widgets.Label(row, nonexistentDefName);
            if (Widgets.ButtonText(buttonRect, "X"))
            {
                BetterAutocastVPE.Settings.WordOfSerenityIgnoredMentalStateDefs.Remove(
                    nonexistentDefName
                );
            }
        }
        if (nonexistentDefNames.Count == 0)
            listing.Label("BetterAutocastVPE.NoNonexistentDefs".Translate());

        listingHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();
    }
}
