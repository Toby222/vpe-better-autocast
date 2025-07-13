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

public class Designator_Area_CraftTimeskip : Designator_Cells
{
    private readonly DesignateMode mode;

    public override bool Visible => BetterAutocastVPE.Settings.ShowCraftTimeskipArea;

#if v1_5
    public override int DraggableDimensions => 2;
#endif

    public override bool DragDrawMeasurements => true;

    protected Designator_Area_CraftTimeskip(DesignateMode mode)
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
        bool cellContained = Map.areaManager.Get<Area_CraftTimeskip>()[c];
        return mode switch
        {
            DesignateMode.Add => !cellContained,
            DesignateMode.Remove => cellContained,
            _ => throw new System.NotImplementedException(),
        };
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        Map.areaManager.Get<Area_CraftTimeskip>()[c] = mode == DesignateMode.Add;
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        Map.areaManager.Get<Area_CraftTimeskip>().MarkForDraw();
    }
}

public class Designator_Area_CraftTimeskip_Expand : Designator_Area_CraftTimeskip
{
    public Designator_Area_CraftTimeskip_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.CraftTimeskipArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.CraftTimeskipArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/CraftTimeskipArea");
        soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
        soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
        soundSucceeded = SoundDefOf.Designate_ZoneAdd_Stockpile;
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
        soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
        soundDragChanged = null;
        soundSucceeded = SoundDefOf.Designate_ZoneDelete;
    }
}
