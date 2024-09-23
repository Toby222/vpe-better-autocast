using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace BetterAutocastVPE.Helpers;

internal static class ThingHelper
{
    internal static IEnumerable<Thing> GetThingsInNamedStorageGroup(
        this Map map,
        string storageGroupName
    )
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (storageGroupName is null)
            throw new ArgumentNullException(nameof(storageGroupName));

#if v1_5
        return HarmonyLib
            .Traverse.Create(map.storageGroups)
            .Field<List<StorageGroup>>("groups")
            .Value.Where(group =>
                group.RenamableLabel.IndexOf(storageGroupName, StringComparison.OrdinalIgnoreCase)
                >= 0
            )
            .SelectMany(group => group.HeldThings);
#else
        throw new NotImplementedException();
#endif
    }

    internal static IEnumerable<Thing> GetThingsInNamedStockpile(this Map map, string stockpileName)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (stockpileName is null)
            throw new ArgumentNullException(nameof(stockpileName));

        return map
            .zoneManager.AllZones.OfType<Zone_Stockpile>()
            .Where(zone =>
                zone.label.IndexOf(stockpileName, StringComparison.OrdinalIgnoreCase) >= 0
            )
            .SelectMany(zone => zone.AllContainedThings);
    }

    internal static IEnumerable<Thing> GetThingsInAllStockpiles(this Map map)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));

        return map
            .zoneManager.AllZones.OfType<Zone_Stockpile>()
            .SelectMany(zone => zone.AllContainedThings);
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

        return pawn.Drafted
            || (
                !thing.IsForbidden(pawn)
                && pawn.CanReserve(thing)
                && (new GlobalTargetInfo(thing).IsValid)
            );
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
