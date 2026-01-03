using RimWorld;
using UnityEngine;
using Verse;

namespace BetterAutocastVPE;

public class Area_GreaterRunecircle : Area
{
    public override string Label => "BetterAutocastVPE.GreaterRunecircleArea".TranslateSafe();

    public override Color Color => new(0.42f, 0.39f, 0.19f);

    public override int ListPriority => 1000;

    public Area_GreaterRunecircle() { }

    public Area_GreaterRunecircle(AreaManager areaManager)
        : base(areaManager) { }

    public override string GetUniqueLoadID()
    {
        return "Area_" + ID + "_GreaterRunecircle";
    }
}

public class Designator_Area_GreaterRunecircle(DesignateMode mode)
    : CellDesignator<Area_GreaterRunecircle>(mode)
{
    public override bool Visible =>
        Designator_Area_Runecircle.ModActive
        && BetterAutocastVPE.Settings.ShowGreaterRunecircleArea;
}

public class Designator_Area_GreaterRunecircle_Expand : Designator_Area_GreaterRunecircle
{
    public Designator_Area_GreaterRunecircle_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.GreaterRunecircleArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.GreaterRunecircleArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/GreaterRunecircleArea");
    }
}

public class Designator_Area_GreaterRunecircle_Clear : Designator_Area_GreaterRunecircle
{
    public Designator_Area_GreaterRunecircle_Clear()
        : base(DesignateMode.Remove)
    {
        defaultLabel = "BetterAutocastVPE.GreaterRunecircleArea.Remove".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.GreaterRunecircleArea.Remove.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/GreaterRunecircleAreaOff");
    }
}
