using HarmonyLib;
using Verse;
using VFECore.Abilities;
using System.Collections.Generic;
using Ability = VFECore.Abilities.Ability;
using System;

namespace VPEAutoCastBuffs
{
    [HarmonyPatch(typeof(Pawn), "Tick")]
    public static class Pawn_Tick_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance)
        {
            var ticksGame = Find.TickManager.TicksGame;

            if (ticksGame % 600 == 0 && !__instance.Drafted)
            {
                ProcessAbilities(__instance, 0.5f, PsycastingHandler.HandleAbilityUndrafted);
            }
            else if (ticksGame % 30 == 0 && __instance.Drafted)
            {
                ProcessAbilities(__instance, 0.0f, PsycastingHandler.HandleAbilityDrafted);
            }
        }

        private static void ProcessAbilities(Pawn pawn, float castThreshold, Func<Pawn, Ability, bool> handleAbility)
        {
            if (!pawn.IsColonistPlayerControlled) return;
            if (!PawnHelper.PawnCanCast(pawn, castThreshold)) return;

            List<Ability> abilities = pawn.GetComp<CompAbilities>()?.LearnedAbilities;
            if (abilities == null) return;

            foreach (Ability ability in abilities)
            {
                if (ability.IsEnabledForPawn(out _) && ability.autoCast && handleAbility(pawn, ability))
                {
                    return;
                }
            }
        }
    }
}