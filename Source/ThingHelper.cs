using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VPEAutoCastBuffs
{
    internal static class ThingHelper
    {
        internal static bool ThingIsDamaged(Thing thing)
        {
            if (thing is ThingWithComps thingWithComps)
            {
                return thingWithComps.HitPoints < (thingWithComps.MaxHitPoints * 0.5);
            }

            return false;
        }

        internal static bool ThingIsBiocoded(Thing thing)
        {
            if (thing == null)
            {
                return false; // Return false if the thing is null to avoid null reference exception
            }

            // Get the CompBiocodable component
            CompBiocodable biocodeComp = thing.TryGetComp<CompBiocodable>();

            // Check if the component exists and the item is biocoded
            return biocodeComp?.Biocoded == true;
        }

        internal static IEnumerable<Thing> GetThingsInNamedStockpile(Map map, string stockpileName)
        {
            if (map == null || string.IsNullOrEmpty(stockpileName))
            {
                return Enumerable.Empty<Thing>();
            }

            return map.listerThings.AllThings.Where(thing =>
                (thing.def.IsWeapon || thing.def.IsApparel)
                && ThingInNamedStockpile(thing, stockpileName)
            );
        }

        internal static bool ThingInNamedStockpile(Thing thing, string stockpileName)
        {
            // Check if the thing is in a stockpile zone with the specified name
            Zone zone = thing.Position.GetZone(thing.Map);
            return zone is Zone_Stockpile
                && zone.label.IndexOf(stockpileName, System.StringComparison.OrdinalIgnoreCase)
                    >= 0;
        }
    }
}
