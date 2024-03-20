using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace VPEAutoCastBuffs
{
    [StaticConstructorOnStartup]
    internal static class VPEAutoCastBuffs
    {
        static VPEAutoCastBuffs()
        {
            Harmony harmony = new("NetzachSloth.VanillaExpandedFrameworkRealAutoAbilities");
            if (
                UnregisterPatch(
                    harmony,
                    typeof(Pawn),
                    "TryGetAttackVerb",
                    HarmonyPatchType.Postfix,
                    "OskarPotocki.VFECore"
                )
            )
            {
                Log.Message("UnregisterPatch succeeded");
            }
            else
            {
                Log.Message("UnregisterPatch failed");
            }
            harmony.PatchAll();
        }

        internal static bool UnregisterPatch(
            Harmony harmony,
            Type targetType,
            string methodName,
            HarmonyPatchType harmonyPatchType,
            string harmonyID
        )
        {
            try
            {
                // Get the MethodInfo for the private method using reflection
                MethodInfo targetMethod = targetType.GetMethod(
                    methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
                );

                if (targetMethod != null)
                {
                    // Attempt to remove the patch by specifying the MethodInfo
                    harmony.Unpatch(targetMethod, harmonyPatchType, harmonyID);
                    return true; // Patch removed successfully
                }
                else
                {
                    Log.Error($"Method '{methodName}' not found in type '{targetType.FullName}'.");
                    return false; // Method not found
                }
            }
            catch (Exception e)
            {
                // Handle any exceptions that may occur during unregistration
                Log.Error(text: "Error unregistering Harmony patch: " + e);
                return false;
            }
        }
    }
}
