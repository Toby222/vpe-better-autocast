using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace BetterAutocastVPE.Helpers;

using static ThingHelper;

internal static class PawnHelper
{
    internal static T? ClosestTo<T>(this IEnumerable<T> things, Thing origin)
        where T : Thing
    {
        if (origin is null)
            throw new ArgumentNullException(nameof(origin));

        return things.OrderBy(thing => origin.Position.DistanceTo(thing.Position)).FirstOrDefault();
    }

    internal static IEnumerable<Pawn> ColonyAnimals(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.Faction?.IsPlayer is true && pawn.RaceProps.Animal);
    }

    internal static IEnumerable<Pawn> WildAnimals(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.Faction is null && pawn.RaceProps.Animal);
    }

    internal static IEnumerable<Pawn> Visitors(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn =>
            pawn.Faction is not null
            && !(pawn.Faction.IsPlayer || pawn.Faction.HostileTo(Faction.OfPlayer))
        );
    }

    internal static IEnumerable<Pawn> Colonists(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.IsColonist);
    }

    internal static bool Immunizable(this Pawn pawn)
    {
        return pawn.health.hediffSet.HasImmunizableNotImmuneHediff();
    }

    internal static IEnumerable<Pawn> Slaves(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.IsSlaveOfColony);
    }

    internal static IEnumerable<Pawn> Prisoners(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.IsPrisoner);
    }

    internal static IEnumerable<Pawn> NotDown(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => !pawn.PawnIsDown());
    }

    internal static Pawn HighestSensitivity(this IEnumerable<Pawn> pawns)
    {
        return pawns
            .OrderByDescending(pawn => pawn.psychicEntropy.PsychicSensitivity)
            .FirstOrDefault();
    }

    internal static IEnumerable<Pawn> PsychicallySensitive(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.psychicEntropy?.IsPsychicallySensitive is true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool DoesNotHaveHediff(this Pawn pawn, string hediffDefName) =>
        !HasHediff(pawn, hediffDefName);

    internal static bool HasHediff(this Pawn pawn, string hediffDefName)
    {
        return pawn?.health?.hediffSet?.hediffs.Any(hediff => hediff.def.defName == hediffDefName)
            ?? false;
    }

    internal static bool PawnIsDown(this Pawn pawn)
    {
        return pawn?.CurJobDef == JobDefOf.LayDown
            || pawn?.jobs?.curDriver?.asleep == true
            || pawn?.Downed == true;
    }

    internal static IEnumerable<Pawn> GetPawnsInRange(this Pawn referencePawn, float range)
    {
        return referencePawn.Map?.mapPawns.AllPawns.Where(pawn =>
                pawn != referencePawn
                && !pawn.Dead
                && pawn.Spawned
                && pawn.Position.InHorDistOf(referencePawn.Position, range)
                && PawnIsDraftedOrThingIsAllowedAndReservable(referencePawn, pawn)
            ) ?? [];
    }

    internal static IEnumerable<Thing> GetThingsInRange(this Pawn referencePawn, float range)
    {
        return referencePawn.Map?.spawnedThings.Where(pawn =>
                pawn != referencePawn
                && pawn.Spawned
                && pawn.Position.InHorDistOf(referencePawn.Position, range)
                && PawnIsDraftedOrThingIsAllowedAndReservable(referencePawn, pawn)
            ) ?? [];
    }

    internal static bool CanPsycast(this Pawn pawn)
    {
        return pawn?.Downed == false
            && pawn.HasPsylink
            && pawn.Awake()
            && pawn.psychicEntropy.CurrentPsyfocus >= BetterAutocastVPE.Settings.MinFocusThreshold
            && (
                pawn.CurJob is null
                || !BetterAutocastVPE.Settings.BlockedJobDefs.Contains(pawn.CurJobDef.defName)
            );
    }

    internal static bool LowJoy(this Pawn pawn)
    {
        return pawn.needs.TryGetNeed<Need_Joy>()?.CurLevelPercentage
            <= BetterAutocastVPE.Settings.WordOfJoyMoodThreshold;
    }
}
