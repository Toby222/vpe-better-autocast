#nullable disable
using Verse;

namespace BetterAutocastVPE.DefOf;

[RimWorld.DefOf]
public static class JobDefs
{
    public static JobDef BetterAutocastVPE_GotoLocationAndCastAbilityOnce;

    static JobDefs()
    {
        RimWorld.DefOfHelper.EnsureInitializedInCtor(typeof(JobDefs));
    }
}
#nullable enable
