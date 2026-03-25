using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using VanillaPsycastsExpanded.Technomancer;
using Verse;

namespace BetterAutocastVPE.Helpers;

internal static class ConstructHelper
{
    private static readonly HashSet<ThingDef> chunkCache;
    static ConstructHelper()
    {
        chunkCache = Traverse.Create<Ability_Construct_Rock>()
            .Field<HashSet<ThingDef>>("chunkCache")
            .Value;
    }

    internal static Thing? GetRandomRockInStockpile(Map map)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));

        IEnumerable<Thing> things = BetterAutocastVPE.Settings.RockConstructOnlyNamedStockpiles
            ? map.GetThingsInNamedStockpile("construct")
            : map.GetThingsInAllStockpiles();

        return things
            .Where(thing => chunkCache.Contains(thing.def))
            .RandomElementWithFallback();
    }

    internal static Thing? GetRandomRockInStorage(Map map)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));

        IEnumerable<Thing> things = BetterAutocastVPE.Settings.RockConstructOnlyNamedStorageGroups
            ? map.GetThingsInNamedStorageGroup("construct")
            : map.GetThingsInStorage();

        return things
            .Where(thing => chunkCache.Contains(thing.def))
            // .Where(thing => Ability_Construct_Rock.IsNonResourceNaturalRock(thing.def) && thing.def.building.mineableThing is not null)
            .RandomElementWithFallback();
    }

    internal static Thing? GetRandomSlagInStockpile(Map map)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));

        IEnumerable<Thing> things = BetterAutocastVPE.Settings.SteelConstructOnlyNamedStockpiles
            ? map.GetThingsInNamedStockpile("construct")
            : map.GetThingsInAllStockpiles();

        return things
            .Where(thing => thing.def == ThingDefOf.ChunkSlagSteel)
            .RandomElement();
    }

    internal static Thing? GetRandomSlagInStorage(Map map)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));

        IEnumerable<Thing> things = BetterAutocastVPE.Settings.SteelConstructOnlyNamedStorageGroups
            ? map.GetThingsInNamedStorageGroup("construct")
            : map.GetThingsInStorage();

        return things
            .Where(thing => thing.def == ThingDefOf.ChunkSlagSteel)
            .RandomElement();
    }
}
