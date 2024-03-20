using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VPEAutoCastBuffs
{
    public static class ThingHelper
    {
        public static bool ThingIsDamaged(Thing thing)
        {
            if (thing is ThingWithComps thingWithComps)
            {
                return thingWithComps.HitPoints < (thingWithComps.MaxHitPoints * 0.5);
            }

            return false;
        }

        public static bool ThingIsBiocoded(Thing thing)
        {
            if (thing == null)
            {
                return false; // Return false if the thing is null to avoid null reference exception
            }

            // Get the CompBiocodable component
            CompBiocodable biocodeComp = thing.TryGetComp<CompBiocodable>();

            // Check if the component exists and the item is biocoded
            return biocodeComp != null && biocodeComp.Biocoded;
        }

        public static IEnumerable<Thing> GetThingsInNamedStockpile(Map map, string stockpileName)
        {
            if (map == null || string.IsNullOrEmpty(stockpileName))
            {
                return new List<Thing>(); // Return empty list if pawn is not on a map or stockpileName is null/empty
            }

            return map.listerThings.AllThings.Where(thing => (thing.def.IsWeapon || thing.def.IsApparel) && ThingInNamedStockpile(thing, stockpileName));
        }

        public static bool ThingInNamedStockpile(Thing thing, string stockpileName)
        {
            // Check if the thing is in a stockpile zone with the specified name
            Zone zone = thing.Position.GetZone(thing.Map);
            return zone is Zone_Stockpile && zone.label.ToLower().Contains(stockpileName.ToLower());
        }
    }
}