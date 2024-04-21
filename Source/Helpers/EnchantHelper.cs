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

        QualityCategory maxQuality = Traverse.Create(ability).Property<QualityCategory>("MaxQuality").Value;
        BetterAutocastVPE.DebugLog($"{ability.pawn} max quality {maxQuality} - {ability.GetType().FullName}");
        return thing.GetQuality() is QualityCategory quality
            && quality < maxQuality;
    }

    private static Thing? GetRandomEnchantableThing(IEnumerable<Thing> things, Ability ability)
    {
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return things
            .Where(thing => ThingIsEnchantable(thing, ability))
            .GetRandomElement(thing => (float)thing.GetQuality()! + 1.0f);
    }

    internal static Thing? GetRandomEnchantableThingInStockpile(Map map, Ability ability)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return GetRandomEnchantableThing(map.GetThingsInNamedStockpile("enchant"), ability);
    }

    internal static Thing? GetRandomEnchantableThingInStorage(Map map, Ability ability)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return GetRandomEnchantableThing(map.GetThingsInStorage(), ability);
    }
}
