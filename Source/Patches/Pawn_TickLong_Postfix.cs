using System;
using HarmonyLib;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace VPEAutoCastBuffs.Patches;

using static Helpers.PawnHelper;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Tick))]
internal static class Pawn_Tick_Postfix
{
    [HarmonyPostfix]
    internal static void Postfix(Pawn __instance)
    {
        if (__instance is null)
            throw new ArgumentNullException(nameof(__instance));

        var ticksGame = Find.TickManager.TicksGame;

        if (ticksGame % 600 == 0 && !__instance.Drafted)
            ProcessAbilities(__instance, 0.5f, PsycastingHandler.HandleAbilityUndrafted);
        else if (ticksGame % 30 == 0 && __instance.Drafted)
            ProcessAbilities(__instance, 0.0f, PsycastingHandler.HandleAbilityDrafted);
    }

    private static void ProcessAbilities(
        Pawn pawn,
        float castThreshold,
        Func<Pawn, Ability, bool> handleAbility
    )
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (handleAbility is null)
            throw new ArgumentNullException(nameof(handleAbility));

        if (!pawn.IsColonistPlayerControlled)
            return;

        if (!PawnCanCast(pawn, castThreshold))
            return;

        if (
            pawn.GetComp<CompAbilities>()
                ?.LearnedAbilities?.Find(ability =>
                    ability.IsEnabledForPawn(out _) && ability.autoCast
                )
            is Ability ability
        )
        {
            handleAbility(pawn, ability);
        }
    }
}
