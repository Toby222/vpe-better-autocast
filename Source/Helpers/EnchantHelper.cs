using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace BetterAutocastVPE.Helpers;

internal static class EnchantHelper
{
    private static bool ThingIsEnchantable(Thing thing, Ability ability)
    {
        if (thing is null)
            throw new ArgumentNullException(nameof(thing));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        if (thing.GetQuality() is not QualityCategory quality)
            return false;

        QualityCategory maxQuality = Traverse
            .Create(ability)
            .Property<QualityCategory>("MaxQuality")
            .Value;

        return quality < maxQuality;
    }

    private static Thing? GetRandomEnchantableThing(IEnumerable<Thing> things, Ability ability)
    {
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return things
            .Where(thing => ThingIsEnchantable(thing, ability))
            .GetRandomClass(thing => (float)thing.GetQuality()! + 1.0f);
    }

    internal static Thing? GetRandomEnchantableThingInStockpile(Map map, Ability ability)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        IEnumerable<Thing> things = BetterAutocastVPE.Settings.EnchantOnlyNamedStockpiles
            ? map.GetThingsInNamedStockpile("enchant")
            : map.GetThingsInAllStockpiles();

        return GetRandomEnchantableThing(things, ability);
    }

    internal static Thing? GetRandomEnchantableThingInStorage(Map map, Ability ability)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        IEnumerable<Thing> things = BetterAutocastVPE.Settings.EnchantOnlyNamedStorageGroups
            ? map.GetThingsInNamedStorageGroup("enchant")
            : map.GetThingsInStorage();

        return GetRandomEnchantableThing(things, ability);
    }
}
