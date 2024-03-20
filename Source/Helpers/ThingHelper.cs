using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VPEAutoCastBuffs.Helpers;

internal static class ThingHelper
{
    internal static IEnumerable<Thing> GetThingsInNamedStockpile(Map map, string stockpileName)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (stockpileName is null)
            throw new ArgumentNullException(nameof(stockpileName));

        return map.listerThings.AllThings.Where(thing =>
            ThingIsInNamedStockpile(thing, stockpileName)
        );
    }

    private static bool ThingIsInNamedStockpile(Thing thing, string stockpileName)
    {
        if (thing is null)
            throw new ArgumentNullException(nameof(thing));
        if (stockpileName is null)
            throw new ArgumentNullException(nameof(stockpileName));

        // Check if the thing is in a stockpile zone with the specified name
        Zone zone = thing.Position.GetZone(thing.Map);
        return zone is Zone_Stockpile
            && zone.label.IndexOf(stockpileName, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    internal static IEnumerable<Thing> GetThingsInStorage(Map map)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));

        return map
            .listerBuildings.AllBuildingsColonistOfClass<Building_Storage>()
            .SelectMany(buildingStorage => buildingStorage.slotGroup.HeldThings);
    }
}
