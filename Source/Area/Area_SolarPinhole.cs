using RimWorld;
using UnityEngine;
using Verse;

namespace BetterAutocastVPE;

public class Area_SolarPinhole : Area
{
    public override string Label => "BetterAutocastVPE.SolarPinholeArea".TranslateSafe();

    public override Color Color => new(0.97f, 0.84f, 0.11f);

    public override int ListPriority => 1000;

    public Area_SolarPinhole() { }

    public Area_SolarPinhole(AreaManager areaManager)
        : base(areaManager) { }

    public override string GetUniqueLoadID()
    {
        return "Area_" + ID + "_SolarPinhole";
    }
}

public class Designator_Area_SolarPinhole(DesignateMode mode)
    : CellDesignator<Area_SolarPinhole>(mode)
{
    public override bool Visible => BetterAutocastVPE.Settings.ShowSolarPinholeArea;
}

public class Designator_Area_SolarPinhole_Expand : Designator_Area_SolarPinhole
{
    public Designator_Area_SolarPinhole_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.SolarPinholeArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.SolarPinholeArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/SolarPinholeArea");
    }
}

public class Designator_Area_SolarPinhole_Clear : Designator_Area_SolarPinhole
{
    public Designator_Area_SolarPinhole_Clear()
        : base(DesignateMode.Remove)
    {
        defaultLabel = "BetterAutocastVPE.SolarPinholeArea.Remove".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.SolarPinholeArea.Remove.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/SolarPinholeAreaOff");
    }
}
