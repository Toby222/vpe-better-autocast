using RimWorld;
using UnityEngine;
using Verse;

namespace BetterAutocastVPE;

public class Area_ColdZone : Area
{
    public override string Label => "BetterAutocastVPE.ColdZoneArea".TranslateSafe();

    public override Color Color => new(0.1f, 0.1f, 0.9f);

    public override int ListPriority => 1000;

    public Area_ColdZone() { }

    public Area_ColdZone(AreaManager areaManager)
        : base(areaManager) { }

    public override string GetUniqueLoadID()
    {
        return "Area_" + ID + "_ColdZone";
    }
}

public class Designator_Area_ColdZone(DesignateMode mode) : CellDesignator<Area_ColdZone>(mode)
{
    public override bool Visible => BetterAutocastVPE.Settings.ShowColdZoneArea;
}

public class Designator_Area_ColdZone_Expand : Designator_Area_ColdZone
{
    public Designator_Area_ColdZone_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.ColdZoneArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.ColdZoneArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/ColdZoneArea");
    }
}

public class Designator_Area_ColdZone_Clear : Designator_Area_ColdZone
{
    public Designator_Area_ColdZone_Clear()
        : base(DesignateMode.Remove)
    {
        defaultLabel = "BetterAutocastVPE.ColdZoneArea.Remove".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.ColdZoneArea.Remove.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/ColdZoneAreaOff");
    }
}
