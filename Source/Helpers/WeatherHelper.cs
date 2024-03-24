using RimWorld;
using Verse;

namespace BetterAutocastVPE.Helpers;

internal static class WeatherHelper
{
    internal static bool EclipseOnMap(Map map)
    {
        // Check if there is an eclipse condition on the map, defaulting to false if there is no map.
        return map?.GameConditionManager?.ConditionIsActive(GameConditionDefOf.Eclipse) ?? false;
    }
}
