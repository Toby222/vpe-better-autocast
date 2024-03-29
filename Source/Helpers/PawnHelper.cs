using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BetterAutocastVPE.Helpers;

using static ThingHelper;

internal static class PawnHelper
{
    internal static IEnumerable<Pawn> GetPawnsWithoutHediff(
        IEnumerable<Pawn> pawns,
        string hediffDefName
    )
    {
        return pawns.Where(pawn => !pawn.HasHediff(hediffDefName));
    }

    internal static T? GetClosestTo<T>(IEnumerable<T> things, Thing origin)
        where T : Thing
    {
        if (origin is null)
            throw new ArgumentNullException(nameof(origin));

        return things.OrderBy(pawn => origin.Position.DistanceTo(pawn.Position)).FirstOrDefault();
    }

    internal static IEnumerable<Pawn> GetColonyAnimals(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.Faction?.IsPlayer == true && pawn.RaceProps.Animal);
    }

    internal static IEnumerable<Pawn> GetVisitors(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn =>
            !pawn.Faction.IsPlayer && !pawn.Faction.HostileTo(Faction.OfPlayer)
        );
    }

    internal static IEnumerable<Pawn> GetColonists(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.IsColonist);
    }

    internal static IEnumerable<Pawn> GetImmunizablePawns(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.health.hediffSet.HasImmunizableNotImmuneHediff());
    }

    internal static IEnumerable<Pawn> GetSlaves(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.IsSlaveOfColony);
    }

    internal static IEnumerable<Pawn> GetPrisoners(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.IsPrisoner);
    }

    internal static IEnumerable<Pawn> GetPawnsNotDown(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => !pawn.PawnIsDown());
    }

    internal static IEnumerable<Pawn> GetPawnsWithMentalBreak(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.MentalState != null);
    }

    internal static Pawn GetHighestSensitivity(IEnumerable<Pawn> pawns)
    {
        return pawns
            .OrderByDescending(pawn => pawn.psychicEntropy.PsychicSensitivity)
            .FirstOrDefault();
    }

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

    internal static IEnumerable<Pawn> GetPawnsInRange(Pawn referencePawn, float range)
    {
        return referencePawn.Map?.mapPawns.AllPawns.Where(pawn =>
                pawn != referencePawn
                && !pawn.Dead
                && pawn.Spawned
                && pawn.Position.InHorDistOf(referencePawn.Position, range)
                && PawnIsDraftedOrThingIsAllowedAndReservable(referencePawn, pawn)
            ) ?? Enumerable.Empty<Pawn>();
    }

    internal static bool CanPsycast(this Pawn pawn)
    {
        return pawn?.Downed == false
            && pawn.HasPsylink
            && pawn.Awake()
            && (
                pawn.CurJob is null
                || !BetterAutocastVPE.Settings.BlockedJobDefs.Contains(pawn.CurJobDef.defName)
            );
    }

    internal static IEnumerable<Pawn> GetLowJoyPawns(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn =>
            pawn.needs.TryGetNeed<Need_Joy>()?.CurLevel
            <= BetterAutocastVPE.Settings.WordOfJoyMoodThreshold
        );
    }
}
