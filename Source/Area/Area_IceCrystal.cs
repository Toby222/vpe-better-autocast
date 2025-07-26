using RimWorld;
using UnityEngine;
using Verse;

namespace BetterAutocastVPE;

public class Area_IceCrystal : Area
{
    public override string Label => "BetterAutocastVPE.IceCrystalArea".TranslateSafe();

    public override Color Color => new(0.1f, 0.1f, 0.9f);

    public override int ListPriority => 1000;

    public Area_IceCrystal() { }

    public Area_IceCrystal(AreaManager areaManager)
        : base(areaManager) { }

    public override string GetUniqueLoadID()
    {
        return "Area_" + ID + "_IceCrystal";
    }
}

public class Designator_Area_IceCrystal(DesignateMode mode) : CellDesignator<Area_IceCrystal>(mode)
{
    public override bool Visible => BetterAutocastVPE.Settings.ShowIceCrystalArea;
}

public class Designator_Area_IceCrystal_Expand : Designator_Area_IceCrystal
{
    public Designator_Area_IceCrystal_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.IceCrystalArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.IceCrystalArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/IceCrystalArea");
    }
}

public class Designator_Area_IceCrystal_Clear : Designator_Area_IceCrystal
{
    public Designator_Area_IceCrystal_Clear()
        : base(DesignateMode.Remove)
    {
        defaultLabel = "BetterAutocastVPE.IceCrystalArea.Remove".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.IceCrystalArea.Remove.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/IceCrystalAreaOff");
    }
}
