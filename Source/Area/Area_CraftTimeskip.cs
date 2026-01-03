using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BetterAutocastVPE;

public class Area_CraftTimeskip : Area
{
    public override string Label => "BetterAutocastVPE.CraftTimeskipArea".TranslateSafe();

    public override Color Color => new(0.92f, 0.2f, 0.2f);

    public override int ListPriority => 1000;

    public Area_CraftTimeskip() { }

    public Area_CraftTimeskip(AreaManager areaManager)
        : base(areaManager) { }

    public override string GetUniqueLoadID()
    {
        return "Area_" + ID + "_CraftTimeskip";
    }
}

public class Designator_Area_CraftTimeskip(DesignateMode mode)
    : CellDesignator<Area_CraftTimeskip>(mode)
{
    public override bool Visible => BetterAutocastVPE.Settings.ShowCraftTimeskipArea;
}

public class Designator_Area_CraftTimeskip_Expand : Designator_Area_CraftTimeskip
{
    public Designator_Area_CraftTimeskip_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.CraftTimeskipArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.CraftTimeskipArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/CraftTimeskipArea");
    }
}

public class Designator_Area_CraftTimeskip_Clear : Designator_Area_CraftTimeskip
{
    public Designator_Area_CraftTimeskip_Clear()
        : base(DesignateMode.Remove)
    {
        defaultLabel = "BetterAutocastVPE.CraftTimeskipArea.Remove".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.CraftTimeskipArea.Remove.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/CraftTimeskipAreaOff");
    }
}
