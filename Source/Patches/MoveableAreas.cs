#if v1_6

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BetterAutocastVPE.Patches;

internal record CustomMoveableAreas(MoveableArea iceCrystal, MoveableArea solarPinhole, MoveableArea craftTimeskip, MoveableArea runecircle, MoveableArea greaterRunecircle) : IExposable
{
    internal static ConditionalWeakTable<MoveableAreas, CustomMoveableAreas> AllMoveableAreas = [];

    public MoveableArea iceCrystal = iceCrystal;
    public MoveableArea solarPinhole = solarPinhole;
    public MoveableArea craftTimeskip = craftTimeskip;
    public MoveableArea runecircle = runecircle;
    public MoveableArea greaterRunecircle = greaterRunecircle;

    public void ExposeData()
    {
        Scribe_Deep.Look(ref iceCrystal, nameof(iceCrystal));
        Scribe_Deep.Look(ref solarPinhole, nameof(solarPinhole));
        Scribe_Deep.Look(ref craftTimeskip, nameof(craftTimeskip));
        Scribe_Deep.Look(ref runecircle, nameof(runecircle));
        Scribe_Deep.Look(ref greaterRunecircle, nameof(greaterRunecircle));
    }
}

[HarmonyPatch(typeof(MoveableAreas), nameof(MoveableAreas.ExposeData))]
internal static class MoveableAreasExpose
{
    internal static void Postfix(MoveableAreas __instance)
    {
        if (CustomMoveableAreas.AllMoveableAreas.TryGetValue(__instance, out var areas))
        {
            Scribe_Deep.Look(ref areas, "vpe-better-autocast.areas");

            CustomMoveableAreas.AllMoveableAreas.AddOrUpdate(__instance, areas);
        }
    }
}

[HarmonyPatch(typeof(MoveableAreas), nameof(MoveableAreas), MethodType.Constructor)]
internal static class MoveableAreasConstructor
{
    internal static void Postfix(MoveableAreas __instance)
    {
        CustomMoveableAreas.AllMoveableAreas.AddOrUpdate(__instance, new CustomMoveableAreas(
            new MoveableArea(),
            new MoveableArea(),
            new MoveableArea(),
            new MoveableArea(),
            new MoveableArea()
        ));
    }
}

[HarmonyPatch(typeof(Gravship), "CopyAreas")]
internal static class GravshipCopyAreas
{
    internal static void Postfix(Gravship __instance, Map oldMap, IntVec3 origin, HashSet<IntVec3> engineFloors)
    {
        if (!CustomMoveableAreas.AllMoveableAreas.TryGetValue(__instance.areas, out var areas))
        {
            BetterAutocastVPE.DebugError("Gravship is missing custom moveable areas");
            return;
        }

        foreach (var area in oldMap.areaManager.AllAreas)
        {
            var moveableArea = new MoveableArea(__instance, area.Label, area.RenamableLabel, area.Color, area.ID);
            foreach (var cell in engineFloors.Intersect(area.ActiveCells))
                moveableArea.Add(cell - origin);

            if (area is Area_IceCrystal)
                areas.iceCrystal = moveableArea;
            else if (area is Area_SolarPinhole)
                areas.solarPinhole = moveableArea;
            else if (area is Area_CraftTimeskip)
                areas.craftTimeskip = moveableArea;
            else if (area is Area_Runecircle)
                areas.runecircle = moveableArea;
            else if (area is Area_GreaterRunecircle)
                areas.greaterRunecircle = moveableArea;
        }
    }
}

[HarmonyPatch(typeof(GravshipPlacementUtility), "CopyAreasIntoMap")]
internal static class GravshipCopyAreasIntoMap
{
    private static void CopyArea<T>(MoveableArea area, Map map, IntVec3 root)
    where T : Area
    {
        string areaName = typeof(T).Name;
        BetterAutocastVPE.DebugLog("Copying " + areaName);
        var newMapArea = map.areaManager.Get<T>();
        if (newMapArea is not null)
        {
            foreach (IntVec3 relativeCell in area.RelativeCells)
            {
                newMapArea[root + relativeCell] = true;
            }
        }
        else
        {
            BetterAutocastVPE.DebugError("New map's " + areaName + " is null");
        }
    }
    internal static void Postfix(Gravship gravship, Map map, IntVec3 root)
    {
        if (!CustomMoveableAreas.AllMoveableAreas.TryGetValue(gravship.areas, out var areas)) return;

        if (areas.iceCrystal is not null)
        {
            CopyArea<Area_IceCrystal>(areas.iceCrystal, map, root);
        }
        if (areas.solarPinhole is not null)
        {
            CopyArea<Area_SolarPinhole>(areas.solarPinhole, map, root);
        }
        if (areas.craftTimeskip is not null)
        {
            CopyArea<Area_CraftTimeskip>(areas.craftTimeskip, map, root);
        }
        if (areas.runecircle is not null)
        {
            CopyArea<Area_Runecircle>(areas.runecircle, map, root);
        }
        if (areas.greaterRunecircle is not null)
        {
            CopyArea<Area_GreaterRunecircle>(areas.greaterRunecircle, map, root);
        }
    }
}

#endif