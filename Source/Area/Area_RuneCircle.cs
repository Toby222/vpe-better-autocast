using RimWorld;
using UnityEngine;
using Verse;

namespace BetterAutocastVPE;

public class Area_Runecircle : Area
{
    public override string Label => "BetterAutocastVPE.RunecircleArea".TranslateSafe();

    public override Color Color => new(0.31f, 0.33f, 0.36f);

    public override int ListPriority => 1000;

    public Area_Runecircle() { }

    public Area_Runecircle(AreaManager areaManager)
        : base(areaManager) { }

    public override string GetUniqueLoadID()
    {
        return "Area_" + ID + "_Runecircle";
    }
}

public class Designator_Area_Runecircle(DesignateMode mode) : CellDesignator<Area_Runecircle>(mode)
{
    public static bool ModActive;

    public override bool Visible => ModActive && BetterAutocastVPE.Settings.ShowRunecircleArea;
}

public class Designator_Area_Runecircle_Expand : Designator_Area_Runecircle
{
    public Designator_Area_Runecircle_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.RunecircleArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.RunecircleArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/RunecircleArea");
    }
}

public class Designator_Area_Runecircle_Clear : Designator_Area_Runecircle
{
    public Designator_Area_Runecircle_Clear()
        : base(DesignateMode.Remove)
    {
        defaultLabel = "BetterAutocastVPE.RunecircleArea.Remove".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.RunecircleArea.Remove.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/RunecircleAreaOff");
    }
}
