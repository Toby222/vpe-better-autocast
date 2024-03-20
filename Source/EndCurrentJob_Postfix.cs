using RimWorld.Planet;
using System;
using Verse;
using Verse.AI;
using VFECore.Abilities;

namespace VPEAutoCastBuffs
{
    //[HarmonyPatch(typeof(Pawn_JobTracker), "EndCurrentJob")]
    public static class EndCurrentJob_Postfix
    {
        private static Action action = null;

        //[HarmonyPostfix]
        public static void Postfix(JobCondition condition, Pawn_JobTracker __instance)
        {
            if (action != null)
            {
                action.Invoke();
                action = null;
            }
        }

        public static void QueueCastingJob(Ability ability, Thing target)
        {
            action = () => ability.CreateCastJob(new GlobalTargetInfo(target));
        }
    }
}