using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BetterAutocastVPE.Helpers;

using static ThingHelper;

internal static class PawnHelper
{
    internal static IEnumerable<Pawn> WithoutHediff(
        this IEnumerable<Pawn> pawns,
        string hediffDefName
    )
    {
        return pawns.Where(pawn => !pawn.HasHediff(hediffDefName));
    }

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

    internal static IEnumerable<Pawn> Immunizable(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.health.hediffSet.HasImmunizableNotImmuneHediff());
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

    internal static IEnumerable<Pawn> WithMentalBreak(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.MentalState != null);
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
            ) ?? Enumerable.Empty<Pawn>();
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

    internal static IEnumerable<Pawn> LowJoy(this IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn =>
            pawn.needs.TryGetNeed<Need_Joy>()?.CurLevel
            <= BetterAutocastVPE.Settings.WordOfJoyMoodThreshold
        );
    }
}
