using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

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

    internal static bool PawnIsDraftedOrThingIsAllowedAndReservable(Pawn pawn, Thing thing)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (thing is null)
            throw new ArgumentNullException(nameof(thing));

        return pawn.Drafted || (!thing.IsForbidden(pawn) && pawn.CanReserve(thing));
    }

    internal static T? GetRandomClass<T>(
        this IEnumerable<T> things,
        Func<T, float>? weightSelector = null
    )
        where T : class
    {
        T? result;
        if (weightSelector is null)
        {
            if (things.TryRandomElement(out result))
                return result;
        }
        else
        {
            if (things.TryRandomElementByWeight(weightSelector, out result))
                return result;
        }
        return null;
    }

    internal static T? GetRandomStruct<T>(
        this IEnumerable<T> things,
        Func<T, float>? weightSelector = null
    )
        where T : struct
    {
        T result;
        if (weightSelector is null)
        {
            if (things.TryRandomElement(out result))
                return result;
        }
        else
        {
            if (things.TryRandomElementByWeight(weightSelector, out result))
                return result;
        }
        return null;
    }

    internal static QualityCategory? GetQuality(this Thing thing)
    {
        if (thing.TryGetQuality(out QualityCategory quality))
            return quality;
        else
            return null;
    }
}
