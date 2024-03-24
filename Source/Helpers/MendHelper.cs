using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BetterAutocastVPE.Helpers;

using static ThingHelper;

internal static class MendHelper
{
    // TODO: This should be configurable
    // Possibly should have an absolute value of damage
    // Also if possible make it use psychic power where appropriate
    private static float DamageThreshold => BetterAutocastVPE.Settings.MendHealthThreshold;

    private static bool ThingIsSufficientlyDamaged(Thing thing)
    {
        if (thing is null)
            throw new ArgumentNullException(nameof(thing));

        return thing.def.useHitPoints && thing.HitPoints < (thing.MaxHitPoints * DamageThreshold);
    }

    internal static Thing? GetRandomDamagedThingInStorage(Map map)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));

        GetThingsInStorage(map)
            .Where(ThingIsSufficientlyDamaged)
            .TryRandomElementByWeight(
                thing => (float)thing.HitPoints / thing.MaxHitPoints,
                out var thing
            );
        return thing;
    }

    internal static Thing GetRandomDamagedThingInStockpile(Map map)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));

        GetThingsInNamedStockpile(map, "mend")
            .Where(ThingIsSufficientlyDamaged)
            .TryRandomElementByWeight(
                thing => (float)thing.HitPoints / thing.MaxHitPoints,
                out var thing
            );
        return thing;
    }

    internal static Pawn? GetRandomPawnWithDamagedEquipment(IEnumerable<Pawn> pawns)
    {
        if (pawns is null)
            throw new ArgumentNullException(nameof(pawns));

        return pawns
            .Where(colonist =>
                (
                    colonist.equipment?.Primary != null
                    && ThingIsSufficientlyDamaged(colonist.equipment.Primary)
                )
                || (
                    colonist.apparel?.WornApparel.Any(apparel =>
                        ThingIsSufficientlyDamaged(apparel)
                        && apparel.def.label.IndexOf(
                            "warcasket",
                            StringComparison.OrdinalIgnoreCase
                        ) < 0
                    ) == true
                )
            )
            .RandomElementWithFallback();
    }
}
