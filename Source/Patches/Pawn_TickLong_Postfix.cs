using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace BetterAutocastVPE.Patches;

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

        if (
            !__instance.Drafted
            && ticksGame % BetterAutocastVPE.Settings.AutocastIntervalUndrafted == 0
        )
        {
            ProcessAbilities(__instance, PsycastingHandler.HandleAbilityUndrafted);
        }
        else if (
            __instance.Drafted
            && ticksGame % BetterAutocastVPE.Settings.AutocastIntervalDrafted == 0
        )
        {
            ProcessAbilities(__instance, PsycastingHandler.HandleAbilityDrafted);
        }
    }

    private static void ProcessAbilities(Pawn pawn, Func<Pawn, Ability, bool> handleAbility)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));
        if (handleAbility is null)
            throw new ArgumentNullException(nameof(handleAbility));

        if (!pawn.IsColonistPlayerControlled)
            return;

        if (!PawnCanCast(pawn))
            return;

        if (pawn.GetComp<CompAbilities>()?.LearnedAbilities is not List<Ability> abilities)
            return;


        foreach (var ability in abilities)
        {
            if (ability is null)
                continue;
            if (ability.IsEnabledForPawn(out _) && ability.autoCast && handleAbility(pawn, ability))
                break;
        }
    }
}
