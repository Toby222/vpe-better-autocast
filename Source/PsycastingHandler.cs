using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Ability = VFECore.Abilities.Ability;
using static VPEAutoCastBuffs.PawnHelper;
using static VPEAutoCastBuffs.WeatherHelper;
using static VPEAutoCastBuffs.ThingHelper;
using HarmonyLib;
using VanillaPsycastsExpanded.Technomancer;
using Verse.AI;

namespace VPEAutoCastBuffs
{
    public static class PsycastingHandler
    {
        public static Dictionary<string, Func<Pawn, Ability, bool>> undraftedAbilityHandlers
            = new Dictionary<string, Func<Pawn, Ability, bool>>
                {
                    { "VPE_SpeedBoost", HandleSelfBuff },
                    { "VPE_StealVitality", HandleStealVitality },
                    { "VPEP_BrainLeech", HandleBrainLeech },
                    { "VPE_PsychicGuidance", HandlePsychicGuidance },
                    { "VPE_EnchantQuality", HandleEnchant },
                    { "VPE_Mend", HandleMend },
                    { "VPE_WordofJoy", HandleWoJ },
                    { "VPE_WordofSerenity", HandleWoS },
                    { "VPE_WordofProductivity", HandleWoP },
                    { "VPE_Eclipse", HandleEclipse },
                    { "VPE_Darkvision", HandleDarkVision }
                };

        public static Dictionary<string, Func<Pawn, Ability, bool>> draftedAbilityHandlers
            = new Dictionary<string, Func<Pawn, Ability, bool>>
        {
                    { "VPE_SpeedBoost", HandleSelfBuff },
                    { "VPE_BladeFocus", HandleSelfBuff },
                    { "VPE_FiringFocus", HandleSelfBuff },
                    { "VPE_AdrenalineRush", HandleSelfBuff },
                    { "VPE_ControlledFrenzy", HandleSelfBuff },
                    { "VPE_GuidedShot", HandleSelfBuff }
        };

        public static bool HandleAbilityUndrafted(Pawn __instance, Ability ability)
        {
            if (undraftedAbilityHandlers.TryGetValue(ability.def.defName, out var handler))
            {
                return handler(__instance, ability);
            }

            return false;
        }

        public static bool HandleAbilityDrafted(Pawn __instance, Ability ability)
        {
            if (draftedAbilityHandlers.TryGetValue(ability.def.defName, out var handler))
            {
                return handler(__instance, ability);
            }

            return false;
        }

        public static bool HandleSelfBuff(Pawn __instance, Ability ability)
        {
            // note, this method only works right if the buff hediff defName and the ability hediff defName are the same
            if (!PawnHasHediff(__instance, ability.def.defName))
            {
                return CastAbilityOnTarget(ability, __instance);
            }
            else return false;
        }

        public static bool HandleStealVitality(Pawn __instance, Ability ability)
        {
            if (PawnHasHediff(__instance, "VPE_GainedVitality")) return false;

            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, ability.GetRangeForPawn());

            return
                CastAbilityOnTarget(ability, GetHighestSensitivity(GetPrisoners(pawnsInRange))) ||
                CastAbilityOnTarget(ability, GetHighestSensitivity(GetSlaves(pawnsInRange))) ||
                CastAbilityOnTarget(ability, GetHighestSensitivity(GetColonists(pawnsInRange)));
        }

        public static bool HandlePsychicGuidance(Pawn __instance, Ability ability)
        {
            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> eligiblePawns = GetColonists(GetPawnsNotDown(pawnsInRange))
                                       .Where(pawn => !PawnHasHediff(pawn, "VPE_PsychicGuidance"));

            return eligiblePawns.FirstOrDefault() is Pawn target && CastAbilityOnTarget(ability, target);
        }

        public static bool HandleDarkVision(Pawn __instance, Ability ability)
        {
            if (!PawnHasHediff(__instance, "VPE_Darkvision"))
            {
                return CastAbilityOnTarget(ability, __instance);
            }

            Pawn target = GetColonists(GetPawnsNotDown(GetPawnsInRange(__instance, ability.GetRangeForPawn())))
                            .FirstOrDefault(pawn => !PawnHasHediff(pawn, "VPE_Darkvision"));

            return target != null && CastAbilityOnTarget(ability, target);
        }

        public static bool HandleEclipse(Pawn __instance, Ability ability)
        {
            return !EclipseOnMap(__instance.Map) && CastAbilityOnTarget(ability, __instance);
        }

        public static bool HandleBrainLeech(Pawn __instance, Ability ability)
        {
            if (PawnHasHediff(__instance, "VPEP_Leeching"))
            {
                return false;
            }

            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, ability.GetRangeForPawn());
            Pawn target = GetPrisoners(pawnsInRange).FirstOrDefault() ?? GetSlaves(pawnsInRange).FirstOrDefault();

            return target != null && CastAbilityOnTarget(ability, target);
        }

        public static bool HandleWoS(Pawn __instance, Ability ability)
        {
            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> pawnsWithMentalBreak = GetPawnsWithMentalBreak(pawnsInRange);
            IEnumerable<Pawn> notDownColonists = GetColonists(GetPawnsNotDown(pawnsWithMentalBreak));

            Pawn target = notDownColonists.FirstOrDefault();
            return target != null && CastAbilityOnTarget(ability, target);
        }

        public static bool HandleWoP(Pawn __instance, Ability ability)
        {
            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> pawnsWithoutHediff = GetPawnsWithoutHediff(pawnsInRange, "VPE_Productivity");
            IEnumerable<Pawn> eligibleColonists = GetColonists(GetPawnsNotDown(pawnsWithoutHediff));

            Pawn target = eligibleColonists.FirstOrDefault();
            return target != null && CastAbilityOnTarget(ability, target);
        }

        public static bool HandleWoJ(Pawn __instance, Ability ability)
        {
            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> pawnsWithoutHediff = GetPawnsWithoutHediff(pawnsInRange, "Joyfuzz");
            IEnumerable<Pawn> notDownColonists = GetColonists(GetPawnsNotDown(pawnsWithoutHediff));
            IEnumerable<Pawn> lowJoyPawns = GetLowJoyPawns(notDownColonists);

            Pawn target = lowJoyPawns.FirstOrDefault();
            return target != null && CastAbilityOnTarget(ability, target);
        }

        public static bool HandleMend(Pawn __instance, Ability ability)
        {
            return HandleMendByPawn(__instance, ability) || HandleMendByZone(__instance, ability);
        }

        private static bool HandleMendByPawn(Pawn __instance, Ability ability)
        {
            float range = ability.GetRangeForPawn();
            IEnumerable<Pawn> pawnsInRange = GetPawnsInRange(__instance, range);
            IEnumerable<Pawn> colonistPawns = GetColonists(pawnsInRange);
            IEnumerable<Pawn> pawnsWithDamagedEquipment = GetPawnsWithDamagedEquipment(colonistPawns);

            Pawn target = pawnsWithDamagedEquipment.FirstOrDefault();
            return target != null && CastAbilityOnTarget(ability, target);
        }

        private static bool HandleMendByZone(Pawn __instance, Ability ability)
        {
            IEnumerable<Thing> thingsInStockpile = GetThingsInNamedStockpile(__instance.Map, "mend");
            Thing target = thingsInStockpile
                            .Where(thing => thing.HitPoints < thing.MaxHitPoints)
                            .OrderBy(thing => thing.HitPoints / (float)thing.MaxHitPoints)
                            .FirstOrDefault();

            return target != null && CastAbilityOnTarget(ability, target);
        }

        public static bool HandleEnchant(Pawn __instance, Ability ability)
        {
            QualityCategory maxQuality = (QualityCategory)(int)ability.GetPowerForPawn();

            var target = GetThingsInNamedStockpile(__instance.Map, "enchant")
                         .Where(thing => thing.TryGetQuality(out var quality) && quality < maxQuality)
                         // Avoid several psychic pawns targetting the same item (and wasting their power)
                         // pick a random valid element, preferring the higher-quality (and thus closest-to-being-done) ones
                         .RandomElementByWeight(thing =>
                         {
                             thing.TryGetQuality(out var quality);
                             return (int)quality;
                         });

            return target != null && CastAbilityOnTarget(ability, target);
        }

        public static bool CastAbilityOnTarget(Ability ability, Thing target)
        {
            if (target == null || ability == null)
                return false;

            ability.CreateCastJob(new GlobalTargetInfo(target));
            return true;
        }
    }
}