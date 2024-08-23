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

public abstract class Designator_Area_IceCrystal : Designator_Cells
{
    private readonly DesignateMode mode;

    public override int DraggableDimensions => 2;

    public override bool DragDrawMeasurements => true;

    protected Designator_Area_IceCrystal(DesignateMode mode)
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
        bool cellContained = Map.areaManager.Get<Area_IceCrystal>()[c];
        return mode switch
        {
            DesignateMode.Add => !cellContained,
            DesignateMode.Remove => cellContained,
            _ => throw new System.NotImplementedException(),
        };
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        Map.areaManager.Get<Area_IceCrystal>()[c] = mode == DesignateMode.Add;
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        Map.areaManager.Get<Area_IceCrystal>().MarkForDraw();
    }
}

public class Designator_Area_IceCrystal_Expand : Designator_Area_IceCrystal
{
    public Designator_Area_IceCrystal_Expand()
        : base(DesignateMode.Add)
    {
        defaultLabel = "BetterAutocastVPE.IceCrystalArea.Expand".TranslateSafe();
        defaultDesc = "BetterAutocastVPE.IceCrystalArea.Expand.Description".TranslateSafe();
        icon = ContentFinder<Texture2D>.Get("UI/Icons/BetterAutocastVPE/IceCrystalArea");
        soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
        soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
        soundSucceeded = SoundDefOf.Designate_ZoneAdd_Stockpile;
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
        soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
        soundDragChanged = null;
        soundSucceeded = SoundDefOf.Designate_ZoneDelete;
    }
}
