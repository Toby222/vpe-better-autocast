global using static BetterAutocastVPE.Extensions.DefExtension;
using System;
using System.Collections.Generic;
using Verse;

namespace BetterAutocastVPE.Extensions;

static class DefExtension
{
    public static readonly Dictionary<(Def def, Type extension), DefModExtension?> Cache = [];
    public static TExtension? GetModExtensionCached<TExtension>(this Def def)
    where TExtension : DefModExtension
    {
        if (!Cache.ContainsKey((def, typeof(TExtension))))
        {
            Cache[(def, typeof(TExtension))] = def.GetModExtension<TExtension>();
        }
        return Cache[(def, typeof(TExtension))] as TExtension;
    }
}
