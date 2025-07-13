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

public class Designator_Area_SolarPinhole : Designator_Cells
{
    private readonly DesignateMode mode;

    public override bool Visible => BetterAutocastVPE.Settings.ShowSolarPinholeArea;

#if v1_5
    public override int DraggableDimensions => 2;
#endif

    public override bool DragDrawMeasurements => true;

    protected Designator_Area_SolarPinhole(DesignateMode mode)
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
        bool cellContained = Map.areaManager.Get<Area_SolarPinhole>()[c];
        return mode switch
        {
            DesignateMode.Add => !cellContained,
            DesignateMode.Remove => cellContained,
            _ => throw new System.NotImplementedException(),
        };
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        Map.areaManager.Get<Area_SolarPinhole>()[c] = mode == DesignateMode.Add;
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        Map.areaManager.Get<Area_SolarPinhole>().MarkForDraw();
    }
}

public class Designator_Area_SolarPinhole_Expand : Designator_Area_SolarPinhole
{
    public Designator_Area_SolarPinhole_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.SolarPinholeArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.SolarPinholeArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/SolarPinholeArea");
        soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
        soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
        soundSucceeded = SoundDefOf.Designate_ZoneAdd_Stockpile;
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
        soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
        soundDragChanged = null;
        soundSucceeded = SoundDefOf.Designate_ZoneDelete;
    }
}
