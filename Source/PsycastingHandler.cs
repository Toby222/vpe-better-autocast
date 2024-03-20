using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using static VPEAutoCastBuffs.PawnHelper;
using static VPEAutoCastBuffs.ThingHelper;
using static VPEAutoCastBuffs.WeatherHelper;
using Ability = VFECore.Abilities.Ability;

namespace VPEAutoCastBuffs
{
    internal static class PsycastingHandler
    {
        internal static Dictionary<string, Func<Pawn, Ability, bool>> undraftedAbilityHandlers =
            new Dictionary<string, Func<Pawn, Ability, bool>>
            {
                { "VPE_SpeedBoost", HandleSelfBuff },
                { "VPE_StealVitality", HandleStealVitality },
                { "VPEP_BrainLeech", HandleBrainLeech },
                { "VPE_PsychicGuidance", HandlePsychicGuidance },
                { "VPE_EnchantQuality", HandleEnchant },
                { "VPE_Mend", HandleMend },
                { "VPE_WordofJoy", HandleWordOfJoy },
                { "VPE_WordofSerenity", HandleWoS },
                { "VPE_WordofProductivity", HandleWoP },
                { "VPE_Eclipse", HandleEclipse },
                { "VPE_Darkvision", HandleDarkVision }
            };

        internal static Dictionary<string, Func<Pawn, Ability, bool>> draftedAbilityHandlers =
            new Dictionary<string, Func<Pawn, Ability, bool>>
            {
                { "VPE_SpeedBoost", HandleSelfBuff },
                { "VPE_BladeFocus", HandleSelfBuff },
                { "VPE_FiringFocus", HandleSelfBuff },
                { "VPE_AdrenalineRush", HandleSelfBuff },
                { "VPE_ControlledFrenzy", HandleSelfBuff },
                { "VPE_GuidedShot", HandleSelfBuff }
            };

        internal static bool HandleAbilityUndrafted(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            if (undraftedAbilityHandlers.TryGetValue(ability.def.defName, out var handler))
            {
                return handler(__instance, ability);
            }

            return false;
        }

        internal static bool HandleAbilityDrafted(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            if (draftedAbilityHandlers.TryGetValue(ability.def.defName, out var handler))
            {
                return handler(__instance, ability);
            }

            return false;
        }

        internal static bool HandleSelfBuff(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            // note, this method only works right if the buff hediff defName and the ability hediff defName are the same
            if (PawnHasHediff(__instance, ability.def.defName))
                return false;
            else
                return CastAbilityOnTarget(ability, __instance);
        }

        internal static bool HandleStealVitality(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            if (PawnHasHediff(__instance, "VPE_GainedVitality"))
                return false;

            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, ability.GetRangeForPawn());

            return CastAbilityOnTarget(ability, GetHighestSensitivity(GetPrisoners(pawnsInRange)))
                || CastAbilityOnTarget(ability, GetHighestSensitivity(GetSlaves(pawnsInRange)))
                || CastAbilityOnTarget(ability, GetHighestSensitivity(GetColonists(pawnsInRange)));
        }

        internal static bool HandlePsychicGuidance(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> eligiblePawns = GetColonists(GetPawnsNotDown(pawnsInRange))
                .Where(pawn => !PawnHasHediff(pawn, "VPE_PsychicGuidance"));

            return eligiblePawns.FirstOrDefault() is Pawn target
                && CastAbilityOnTarget(ability, target);
        }

        internal static bool HandleDarkVision(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            if (!PawnHasHediff(__instance, "VPE_Darkvision"))
            {
                return CastAbilityOnTarget(ability, __instance);
            }

            Pawn target = GetColonists(
                    GetPawnsNotDown(GetPawnsInRange(__instance, ability.GetRangeForPawn()))
                )
                .FirstOrDefault(pawn => !PawnHasHediff(pawn, "VPE_Darkvision"));

            return target != null && CastAbilityOnTarget(ability, target);
        }

        internal static bool HandleEclipse(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            return !EclipseOnMap(__instance.Map) && CastAbilityOnTarget(ability, __instance);
        }

        internal static bool HandleBrainLeech(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            if (PawnHasHediff(__instance, "VPEP_Leeching"))
            {
                return false;
            }

            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, ability.GetRangeForPawn());
            Pawn target =
                GetPrisoners(pawnsInRange).FirstOrDefault()
                ?? GetSlaves(pawnsInRange).FirstOrDefault();

            return target != null && CastAbilityOnTarget(ability, target);
        }

        internal static bool HandleWoS(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> pawnsWithMentalBreak = GetPawnsWithMentalBreak(pawnsInRange);
            IEnumerable<Pawn> notDownColonists = GetColonists(
                GetPawnsNotDown(pawnsWithMentalBreak)
            );

            Pawn target = notDownColonists.FirstOrDefault();
            return target != null && CastAbilityOnTarget(ability, target);
        }

        internal static bool HandleWoP(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> pawnsWithoutHediff = GetPawnsWithoutHediff(
                pawnsInRange,
                "VPE_Productivity"
            );
            IEnumerable<Pawn> eligibleColonists = GetColonists(GetPawnsNotDown(pawnsWithoutHediff));

            Pawn target = eligibleColonists.FirstOrDefault();
            return target != null && CastAbilityOnTarget(ability, target);
        }

        internal static bool HandleWordOfJoy(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> pawnsWithoutHediff = GetPawnsWithoutHediff(pawnsInRange, "Joyfuzz");
            IEnumerable<Pawn> notDownColonists = GetColonists(GetPawnsNotDown(pawnsWithoutHediff));
            IEnumerable<Pawn> lowJoyPawns = GetLowJoyPawns(notDownColonists);

            Pawn target = lowJoyPawns.FirstOrDefault();
            return target != null && CastAbilityOnTarget(ability, target);
        }

        internal static bool HandleMend(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            return HandleMendByPawn(__instance, ability) || HandleMendByZone(__instance, ability);
        }

        private static bool HandleMendByPawn(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> colonistPawns = GetColonists(pawnsInRange);
            IEnumerable<Pawn> pawnsWithDamagedEquipment = GetPawnsWithDamagedEquipment(
                colonistPawns
            );

            Pawn target = pawnsWithDamagedEquipment.FirstOrDefault();
            return target != null && CastAbilityOnTarget(ability, target);
        }

        private static bool HandleMendByZone(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            IEnumerable<Thing> thingsInStockpile = GetThingsInNamedStockpile(
                __instance.Map,
                "mend"
            );
            thingsInStockpile
                .Where(thing => thing.HitPoints < thing.MaxHitPoints)
                // Avoid several psychic pawns targetting the same item (and wasting their power)
                // pick a random valid element, generally preferring the less damaged (and thus closest-to-being-done) ones
                .TryRandomElementByWeight(
                    thing => (float)thing.HitPoints / thing.MaxHitPoints,
                    out Thing target
                );

            return target != null && CastAbilityOnTarget(ability, target);
        }

        internal static bool HandleEnchant(Pawn __instance, Ability ability)
        {
            if (__instance is null)
                throw new ArgumentNullException(nameof(__instance));
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));

            QualityCategory maxQuality = (QualityCategory)(int)ability.GetPowerForPawn();

            GetThingsInNamedStockpile(__instance.Map, "enchant")
                .Where(thing => thing.TryGetQuality(out var quality) && quality < maxQuality)
                // Avoid several psychic pawns targetting the same item (and wasting their power)
                // pick a random valid element, generally preferring the higher-quality (and thus closest-to-being-done) ones
                .TryRandomElementByWeight(
                    thing =>
                    {
                        thing.TryGetQuality(out var quality);
                        return 1f + (float)quality;
                    },
                    out Thing target
                );

            return target != null && CastAbilityOnTarget(ability, target);
        }

        internal static bool CastAbilityOnTarget(Ability ability, Thing target)
        {
            if (target == null || ability == null)
                return false;

            ability.CreateCastJob(new GlobalTargetInfo(target));
            return true;
        }
    }
}
