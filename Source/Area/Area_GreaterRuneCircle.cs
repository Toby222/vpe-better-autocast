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

public class Designator_Area_GreaterRunecircle : Designator_Cells
{
    private readonly DesignateMode mode;

    public override bool Visible =>
        Designator_Area_Runecircle.ModActive
        && BetterAutocastVPE.Settings.ShowGreaterRunecircleArea;

    public override int DraggableDimensions => 2;

    public override bool DragDrawMeasurements => true;

    protected Designator_Area_GreaterRunecircle(DesignateMode mode)
    {
        this.mode = mode;
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        useMouseIcon = true;
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(Map))
        {
            return false;
        }
        bool cellContained = Map.areaManager.Get<Area_GreaterRunecircle>()[c];
        return mode switch
        {
            DesignateMode.Add => !cellContained,
            DesignateMode.Remove => cellContained,
            _ => throw new System.NotImplementedException(),
        };
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        Map.areaManager.Get<Area_GreaterRunecircle>()[c] = mode == DesignateMode.Add;
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        Map.areaManager.Get<Area_GreaterRunecircle>().MarkForDraw();
    }
}

public class Designator_Area_GreaterRunecircle_Expand : Designator_Area_GreaterRunecircle
{
    public Designator_Area_GreaterRunecircle_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.GreaterRunecircleArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.GreaterRunecircleArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/GreaterRunecircleArea");
        soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
        soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
        soundSucceeded = SoundDefOf.Designate_ZoneAdd_Stockpile;
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
        soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
        soundDragChanged = null;
        soundSucceeded = SoundDefOf.Designate_ZoneDelete;
    }
}
