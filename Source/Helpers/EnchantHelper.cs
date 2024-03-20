using System;
using System.Linq;
using RimWorld;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace VPEAutoCastBuffs.Helpers;

using static ThingHelper;

internal static class EnchantHelper
{
    private static bool ThingIsEnchantable(Thing thing, Ability ability)
    {
        if (thing is null)
            throw new ArgumentNullException(nameof(thing));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        return thing.TryGetQuality(out QualityCategory quality)
            && quality < (QualityCategory)(int)ability.GetPowerForPawn();
    }

    internal static Thing GetRandomEnchantableThingInStockpile(Map map, Ability ability)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        GetThingsInNamedStockpile(map, "enchant")
            .Where(thing => ThingIsEnchantable(thing, ability))
            .TryRandomElementByWeight(
                thing =>
                {
                    thing.TryGetQuality(out QualityCategory quality);
                    return (float)quality + 1.0f;
                },
                out Thing target
            );
        return target;
    }

    internal static Thing GetRandomEnchantableThingInStorage(Map map, Ability ability)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (ability is null)
            throw new ArgumentNullException(nameof(ability));

        GetThingsInStorage(map)
            .Where(thing => ThingIsEnchantable(thing, ability))
            .TryRandomElementByWeight(
                thing =>
                {
                    thing.TryGetQuality(out QualityCategory quality);
                    return (float)quality + 1.0f;
                },
                out Thing target
            );
        return target;
    }
}
