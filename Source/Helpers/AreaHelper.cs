using System.Linq;
using Verse;

namespace BetterAutocastVPE.Helpers;

internal static class AreaHelper
{
    public static IntVec3? GetRandomUnoccupiedCellInArea<T>(Map map)
        where T : Area
    {
        System.Collections.Generic.HashSet<IntVec3> activeCells = map
            .areaManager.Get<T>()
            .ActiveCells.ToHashSet();
        BetterAutocastVPE.DebugLog(
            $"GetRandomUnoccupiedCellInArea(...) activeCells: {string.Join(", ", activeCells)}"
        );
        IntVec3? result = activeCells
            .Where(cell => cell.GetFirstBuilding(map) is null)
            .GetRandomStruct();
        BetterAutocastVPE.DebugLog(
            $"GetRandomUnoccupiedCellInArea(...) -> {result.ToStringSafe()}"
        );
        return result;
    }
}
