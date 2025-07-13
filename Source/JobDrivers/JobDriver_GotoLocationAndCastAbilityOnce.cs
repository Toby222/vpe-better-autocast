using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
#if v1_5
using VFECore.Abilities;
#else
using VEF.Abilities;
#endif

namespace BetterAutocastVPE.JobDrivers;

// Derived from Vanilla Expanded Framework's JobDriver_GotoTargetAndCastAbilityOnce
// only difference is that this allows targets that aren't Things
public class JobDriver_GotoLocationAndCastAbilityOnce : JobDriver_CastAbilityOnce
{
    protected override IEnumerable<Toil> MakeNewToils()
    {
        if (pawn != TargetA.Thing)
        {
            foreach (var toil in GotoToils())
            {
                yield return toil;
            }
        }
        foreach (var toil in base.MakeNewToils())
        {
            yield return toil;
        }
        AddFinishAction(
            delegate
            {
                if (
                    job.targetA.Thing is Pawn victim
                    && victim.CurJobDef == VFE_DefOf_Abilities.VFEA_StandAndFaceTarget
                )
                {
                    victim.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            }
        );
    }

#if v1_5
    private IEnumerable<Toil> GotoToils()
    {
        yield return new Toil
        {
            debugName = "Stop and go to",
            initAction = pawn.pather.StopDead,
            tickAction = delegate
            {
                IntVec3 target = job.targetA.Cell;
                pawn.rotationTracker.FaceTarget(target);
                Map map = pawn.Map;
                if (
                    GenSight.LineOfSight(pawn.Position, target, map, skipFirstCell: true)
                    && pawn.Position.DistanceTo(target)
                        <= CompAbilities.currentlyCasting.def.distanceToTarget
                    && (!pawn.pather.Moving || pawn.pather.nextCell.GetDoor(map) == null)
                )
                {
                    pawn.pather.StopDead();
                    pawn.rotationTracker.FaceTarget(target);
                    if (job.targetA.Thing is Pawn victim)
                    {
                        victim.jobs.TryTakeOrderedJob(
                            JobMaker.MakeJob(VFE_DefOf_Abilities.VFEA_StandAndFaceTarget, pawn)
                        );
                    }

                    ReadyForNextToil();
                }
                else if (!pawn.pather.Moving)
                {
                    if (CompAbilities.currentlyCasting.def.distanceToTarget <= 1.5f)
                    {
                        pawn.pather.StartPath(TargetA, PathEndMode.Touch);
                    }
                    else
                    {
                        IntVec3 intVec = IntVec3.Invalid;
                        for (int i = 0; i < 9 && (i != 8 || !intVec.IsValid); i++)
                        {
                            IntVec3 intVec2 = target + GenAdj.AdjacentCellsAndInside[i];
                            if (
                                intVec2.InBounds(map)
                                && intVec2.Walkable(map)
                                && intVec2 != pawn.Position
                                && InteractionUtility.IsGoodPositionForInteraction(
                                    intVec2,
                                    target,
                                    map
                                )
                                && pawn.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly)
                                && (
                                    !intVec.IsValid
                                    || pawn.Position.DistanceToSquared(intVec2)
                                        < pawn.Position.DistanceToSquared(intVec)
                                )
                            )
                            {
                                intVec = intVec2;
                            }
                        }
                        if (intVec.IsValid)
                        {
                            pawn.pather.StartPath(intVec, PathEndMode.OnCell);
                        }
                        else
                        {
                            ReadyForNextToil();
                        }
                    }
                }
            },
            handlingFacing = true,
            socialMode = RandomSocialMode.Off,
            defaultCompleteMode = ToilCompleteMode.Never,
        };
    }
#else

    private IEnumerable<Toil> GotoToils()
    {
        Toil toil = ToilMaker.MakeToil("Go to and cast");

        toil.initAction = pawn.pather.StopDead;

        toil.tickAction = () =>
        {
            IntVec3 target = job.targetA.Cell;
            pawn.rotationTracker.FaceTarget(target);
            Map map = pawn.Map;
            if (GenSight.LineOfSight(pawn.Position, target, map, skipFirstCell: true)
                && pawn.Position.DistanceTo(target) <= CompAbilities.currentlyCasting.def.distanceToTarget
                && (!pawn.pather.Moving || pawn.pather.nextCell.GetDoor(map) == null))
            {
                pawn.pather.StopDead();
                pawn.rotationTracker.FaceTarget(target);
                if (job.targetA.Pawn is Pawn victim)
                {
                    victim.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VFE_DefOf_Abilities.VFEA_StandAndFaceTarget, pawn));
                }
                ReadyForNextToil();
            }
            else if (!pawn.pather.Moving)
            {
                if (CompAbilities.currentlyCasting.def.distanceToTarget <= 1.5f)
                {
                    pawn.pather.StartPath(TargetA, PathEndMode.Touch);
                }
                else
                {
                    IntVec3 intVec = IntVec3.Invalid;
                    for (int i = 0; i < 9 && (i != 8 || !intVec.IsValid); i++)
                    {
                        IntVec3 intVec2 = target + GenAdj.AdjacentCellsAndInside[i];
                        if (intVec2.InBounds(map) && intVec2.Walkable(map) && intVec2 != pawn.Position &&
                        SocialInteractionUtility.IsGoodPositionForInteraction(intVec2, target, map)
                        && pawn.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly)
                        && (!intVec.IsValid || pawn.Position.DistanceToSquared(intVec2) < pawn.Position.DistanceToSquared(intVec)))
                        {
                            intVec = intVec2;
                        }
                    }
                    if (intVec.IsValid)
                    {
                        pawn.pather.StartPath(intVec, PathEndMode.OnCell);
                    }
                    else
                    {
                        ReadyForNextToil();
                    }
                }
            }
        };

        toil.handlingFacing = true;
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;

        yield return toil;
    }
#endif
}
