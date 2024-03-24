using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BetterAutocastVPE.Helpers;

internal static class PawnHelper
{
    internal static IEnumerable<Pawn> GetPawnsWithoutHediff(
        IEnumerable<Pawn> pawns,
        string hediffDefName
    )
    {
        return pawns.Where(pawn => !PawnHasHediff(pawn, hediffDefName));
    }

    internal static IEnumerable<Pawn> GetColonists(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.IsColonist);
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
        return pawns.Where(pawn => !PawnIsDown(pawn));
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

    internal static bool PawnHasHediff(Pawn pawn, string hediffDefName)
    {
        return pawn?.health?.hediffSet?.hediffs.Any(hediff => hediff.def.defName == hediffDefName)
            ?? false;
    }

    internal static bool PawnIsDown(Pawn pawn)
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
                && (pawn.Position - referencePawn.Position).LengthHorizontal <= range
            ) ?? Enumerable.Empty<Pawn>();
    }

    internal static bool PawnCanCast(Pawn pawn)
    {
        return pawn != null
            && pawn.CurJobDef != JobDefOf.LayDown
            && !pawn.Downed
            && pawn.HasPsylink
            && pawn.jobs?.curDriver?.asleep == false
            && pawn.CurJob?.def.defName != "VFEA_GotoTargetAndUseAbility"
            && pawn.CurJob?.def.defName != "VFEA_UseAbility";
    }

    internal static IEnumerable<Pawn> GetLowJoyPawns(IEnumerable<Pawn> pawns)
    {
        return pawns.Where(pawn => pawn.needs.TryGetNeed<Need_Joy>()?.CurLevel < 0.20f);
    }
}
