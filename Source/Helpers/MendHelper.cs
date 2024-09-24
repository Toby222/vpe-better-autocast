using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using VFECore.Abilities;

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

        return things
            .Where(ThingIsSufficientlyDamaged)
            .Where(thing => PawnIsDraftedOrThingIsAllowedAndReservable(pawn, thing))
            .GetRandomClass(thing => (float)thing.HitPoints / thing.MaxHitPoints);
    }

    internal static Thing? GetRandomAllowedDamagedThingInStockpile(
        Map map,
        Pawn pawn,
        Ability mendAbility
    )
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
            .Where(thing => mendAbility.ValidateTarget(thing, false))
            .GetRandomClass(thing => (float)thing.HitPoints / thing.MaxHitPoints);
    }

    internal static bool HasDamagedEquipment(this Pawn pawn)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));

        return pawn.equipment.AllEquipmentListForReading.Any(ThingIsSufficientlyDamaged)
            || pawn.apparel.WornApparel.Any(ThingIsSufficientlyDamaged);
    }
}
