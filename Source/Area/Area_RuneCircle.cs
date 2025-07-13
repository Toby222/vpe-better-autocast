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

public class Designator_Area_Runecircle : Designator_Cells
{
    public static bool ModActive;

    private readonly DesignateMode mode;

    public override bool Visible => ModActive && BetterAutocastVPE.Settings.ShowRunecircleArea;

#if v1_5
    public override int DraggableDimensions => 2;
#endif

    public override bool DragDrawMeasurements => true;

    protected Designator_Area_Runecircle(DesignateMode mode)
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
        bool cellContained = Map.areaManager.Get<Area_Runecircle>()[c];
        return mode switch
        {
            DesignateMode.Add => !cellContained,
            DesignateMode.Remove => cellContained,
            _ => throw new System.NotImplementedException(),
        };
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        Map.areaManager.Get<Area_Runecircle>()[c] = mode == DesignateMode.Add;
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        Map.areaManager.Get<Area_Runecircle>().MarkForDraw();
    }
}

public class Designator_Area_Runecircle_Expand : Designator_Area_Runecircle
{
    public Designator_Area_Runecircle_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.RunecircleArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.RunecircleArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/RunecircleArea");
        soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
        soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
        soundSucceeded = SoundDefOf.Designate_ZoneAdd_Stockpile;
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
        soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
        soundDragChanged = null;
        soundSucceeded = SoundDefOf.Designate_ZoneDelete;
    }
}
