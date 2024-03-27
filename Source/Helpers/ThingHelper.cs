using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BetterAutocastVPE.Helpers;

internal static class ThingHelper
{
    internal static IEnumerable<Thing> GetThingsInNamedStockpile(this Map map, string stockpileName)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (stockpileName is null)
            throw new ArgumentNullException(nameof(stockpileName));

        return map.listerThings.AllThings.Where(thing => thing.IsInNamedStockpile(stockpileName));
    }

    private static bool IsInNamedStockpile(this Thing thing, string stockpileName)
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

    internal static IEnumerable<Thing> GetThingsInStorage(this Map map)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));

        return map
            .listerBuildings.AllBuildingsColonistOfClass<Building_Storage>()
            .SelectMany(buildingStorage => buildingStorage.slotGroup.HeldThings);
    }

    internal static bool PawnIsDraftedOrThingIsInAllowedArea(Pawn pawn, Thing thing)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (thing is null)
            throw new ArgumentNullException(nameof(thing));

        if (pawn.Drafted)
            return true;

        if (thing.MapHeld is not Map thingMap || thingMap != pawn.MapHeld)
            return false;

        return thing.PositionHeld.InAllowedArea(pawn);
    }

    internal static T? GetRandomElement<T>(
        this IEnumerable<T> things,
        Func<T, float>? weightSelector
    )
    {
        T? result;
        if (weightSelector is null)
            things.TryRandomElement(out result);
        else
            things.TryRandomElementByWeight(weightSelector, out result);
        return result;
    }

    internal static QualityCategory? GetQuality(this Thing thing)
    {
        if (thing.TryGetQuality(out QualityCategory quality))
            return quality;
        else
            return null;
    }
}
