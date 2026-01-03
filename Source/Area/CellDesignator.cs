using RimWorld;
using Verse;

namespace BetterAutocastVPE;

public class CellDesignator<TArea> : Designator_Cells
    where TArea : Area
{
    private readonly DesignateMode mode;

#if v1_5
    public override int DraggableDimensions => 2;
#endif

#if v1_6
    public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Areas;
#endif

    public override bool DragDrawMeasurements => true;

    protected CellDesignator(DesignateMode mode)
    {
        this.mode = mode;
        soundDragSustain = mode switch
        {
            DesignateMode.Add => SoundDefOf.Designate_DragAreaAdd,
            DesignateMode.Remove => SoundDefOf.Designate_DragAreaDelete,
            _ => throw new System.NotImplementedException(),
        };
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        soundSucceeded = mode switch
        {
            DesignateMode.Add => SoundDefOf.Designate_ZoneAdd,
            DesignateMode.Remove => SoundDefOf.Designate_ZoneDelete,
            _ => throw new System.NotImplementedException(),
        };
        useMouseIcon = true;
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(Map))
        {
            return false;
        }
        bool cellContained = Map.areaManager.Get<TArea>()[c];
        return mode switch
        {
            DesignateMode.Add => !cellContained,
            DesignateMode.Remove => cellContained,
            _ => throw new System.NotImplementedException(),
        };
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        Map.areaManager.Get<TArea>()[c] = mode == DesignateMode.Add;
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        Map.areaManager.Get<TArea>().MarkForDraw();
    }
}
