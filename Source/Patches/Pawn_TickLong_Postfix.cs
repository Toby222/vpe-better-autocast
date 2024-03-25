using System;
using System.Collections.Generic;
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

        if (
            Find.TickManager.TicksGame
                % (
                    __instance.Drafted
                        ? BetterAutocastVPE.Settings.AutocastIntervalDrafted
                        : BetterAutocastVPE.Settings.AutocastIntervalUndrafted
                )
            == 0
        )
        {
            ProcessAbilities(__instance);
        }
    }

    private static void ProcessAbilities(Pawn pawn)
    {
        if (pawn is null)
            throw new ArgumentNullException(nameof(pawn));

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
            if (
                ability.autoCast
                && ability.IsEnabledForPawn(out _)
                && PsycastingHandler.HandleAbility(pawn, ability)
            )
            {
                break;
            }
        }
    }
}
