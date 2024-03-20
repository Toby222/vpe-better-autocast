using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VPEAutoCastBuffs
{
    public static class PawnHelper
    {
        public static IEnumerable<Pawn> GetPawnsWithHediff(IEnumerable<Pawn> pawns, String hediffDefName)
        {
            return pawns.Where(pawn => PawnHasHediff(pawn, hediffDefName));
        }

        public static IEnumerable<Pawn> GetPawnsWithoutHediff(IEnumerable<Pawn> pawns, String hediffDefName)
        {
            return pawns.Where(pawn => !PawnHasHediff(pawn, hediffDefName));
        }

        public static IEnumerable<Pawn> GetColonists(IEnumerable<Pawn> pawns)
        {
            return pawns.Where(pawn => pawn.IsColonist);
        }

        public static IEnumerable<Pawn> GetSlaves(IEnumerable<Pawn> pawns)
        {
            return pawns.Where(pawn => pawn.IsSlaveOfColony);
        }

        public static IEnumerable<Pawn> GetPrisoners(IEnumerable<Pawn> pawns)
        {
            return pawns.Where(pawn => pawn.IsPrisoner);
        }

        public static IEnumerable<Pawn> GetPawnsNotDown(IEnumerable<Pawn> pawns)
        {
            return pawns.Where(pawn => !PawnIsDown(pawn));
        }

        public static IEnumerable<Pawn> GetPawnsDown(IEnumerable<Pawn> pawns)
        {
            return pawns.Where(pawn => PawnIsDown(pawn));
        }

        public static IEnumerable<Pawn> GetPawnsWithMentalBreak(IEnumerable<Pawn> pawns)
        {
            return pawns.Where(pawn => pawn.MentalState != null);
        }

        public static Pawn GetHighestSensitivity(IEnumerable<Pawn> pawns)
        {
            return pawns.OrderByDescending(pawn => pawn.psychicEntropy.PsychicSensitivity).FirstOrDefault();
        }

        public static Pawn GetLowestSensitivity(IEnumerable<Pawn> pawns)
        {
            return pawns.OrderBy(pawn => pawn.psychicEntropy.PsychicSensitivity).FirstOrDefault();
        }

        public static bool PawnHasHediff(Pawn pawn, string hediffDefName)
        {
            return pawn?.health?.hediffSet?.hediffs.Any(hediff => hediff.def.defName == hediffDefName) ?? false;
        }

        public static bool PawnIsDown(Pawn pawn)
        {
            return pawn == null ||
                   pawn.CurJobDef == JobDefOf.LayDown ||
                   (pawn.jobs?.curDriver?.asleep == true) ||
                   pawn.Downed;
        }

        public static IEnumerable<Pawn> GetPawnsInRange(Pawn referencePawn, float range)
        {
            return referencePawn.Map?.mapPawns.AllPawns
                .Where(pawn => pawn != referencePawn && !pawn.Dead && pawn.Spawned &&
                               (pawn.Position - referencePawn.Position).LengthHorizontal <= range);
        }

        public static bool PawnCanCast(Pawn pawn, float psyFocusLimit)
        {
            return pawn != null &&
                   pawn.CurJobDef != JobDefOf.LayDown &&
                   !pawn.Downed &&
                   pawn.HasPsylink &&
                   pawn.psychicEntropy.CurrentPsyfocus >= psyFocusLimit &&
                   pawn.jobs?.curDriver?.asleep == false &&
                   pawn.CurJob?.def.defName != "VFEA_GotoTargetAndUseAbility" &&
                   pawn.CurJob?.def.defName != "VFEA_UseAbility";
        }

        public static IEnumerable<Pawn> GetPawnsWithDamagedEquipment(IEnumerable<Pawn> pawns)
        {
            if (pawns == null) return new List<Pawn>(); // Return empty list if colonists list is null

            return pawns
                .Where(colonist =>
                    (colonist.equipment?.Primary != null && ThingHelper.ThingIsDamaged(colonist.equipment.Primary)) ||
                    (colonist.apparel != null && colonist.apparel.WornApparel
                        .Any(apparel => ThingHelper.ThingIsDamaged(apparel) && !apparel.def.label.ToLower().Contains("warcasket")))
                );
        }

        public static IEnumerable<Pawn> GetLowJoyPawns(IEnumerable<Pawn> pawns)
        {
            return pawns
                .Where(pawn => pawn.needs.TryGetNeed<Need_Joy>()?.CurLevel < 0.20f);
        }
    }
}