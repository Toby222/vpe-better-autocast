using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BetterAutocastVPE.Helpers;

using static ThingHelper;

internal static class MendHelper
{
    // TODO:
    // Possibly should have an absolute value of damage
    // If possible make it use psychic power where appropriate
    private static float DamageThreshold => BetterAutocastVPE.Settings.MendHealthThreshold;

    private static bool ThingIsSufficientlyDamaged(Thing thing)
    {
        if (thing is null)
            throw new ArgumentNullException(nameof(thing));

        return thing.def.useHitPoints && thing.HitPoints < (thing.MaxHitPoints * DamageThreshold);
    }

    internal static Thing? GetRandomAllowedDamagedThingInStorage(Map map, Pawn pawn)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));

        if (pawn.MapHeld != map)
            return null;

        IEnumerable<Thing> things = BetterAutocastVPE.Settings.MendOnlyNamedStorageGroups
            ? map.GetThingsInNamedStorageGroup("mend")
            : map.GetThingsInStorage();

#if DEBUG
        things = things.ToList();
        BetterAutocastVPE.DebugLog(
            "DamagedThings" + string.Join(",", things.Select(thing => thing.ToStringSafe()))
        );
#endif

        return things
            .Where(ThingIsSufficientlyDamaged)
            .Where(thing => PawnIsDraftedOrThingIsAllowedAndReservable(pawn, thing))
            .GetRandomClass(thing => (float)thing.HitPoints / thing.MaxHitPoints);
    }

    internal static Thing? GetRandomAllowedDamagedThingInStockpile(Map map, Pawn pawn)
    {
        if (map is null)
            throw new ArgumentNullException(nameof(map));
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));

        if (pawn.MapHeld != map)
            return null;

        IEnumerable<Thing> things = BetterAutocastVPE.Settings.MendOnlyNamedStockpiles
            ? map.GetThingsInNamedStockpile("mend")
            : map.GetThingsInAllStockpiles();

        return things
            .Where(thing => PawnIsDraftedOrThingIsAllowedAndReservable(pawn, thing))
            .Where(ThingIsSufficientlyDamaged)
            .GetRandomClass(thing => (float)thing.HitPoints / thing.MaxHitPoints);
    }

    [Obsolete]
    internal static IEnumerable<Pawn> GetPawnsWithDamagedEquipment(IEnumerable<Pawn> pawns)
    {
        return pawns.WithDamagedEquipment();
    }

    internal static IEnumerable<Pawn> WithDamagedEquipment(this IEnumerable<Pawn> pawns)
    {
        if (pawns is null)
            throw new ArgumentNullException(nameof(pawns));

        return pawns.Where(colonist =>
            (
                colonist.equipment?.Primary != null
                && ThingIsSufficientlyDamaged(colonist.equipment.Primary)
            )
            || (
                colonist.apparel?.WornApparel.Any(apparel =>
                    ThingIsSufficientlyDamaged(apparel)
                    && apparel.def.label.IndexOf("warcasket", StringComparison.OrdinalIgnoreCase)
                        < 0
                ) == true
            )
        );
    }
}
