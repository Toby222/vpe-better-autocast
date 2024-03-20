using RimWorld;
using Verse;

namespace VPEAutoCastBuffs
{
    public static class WeatherHelper
    {
        public static bool EclipseOnMap(Map map)
        {
            if (map == null || map.GameConditionManager == null)
            {
                return false; // Return false if pawn, map, or game condition manager is null
            }

            // Check if there is an eclipse condition on the map
            return map.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse);
        }
    }
}