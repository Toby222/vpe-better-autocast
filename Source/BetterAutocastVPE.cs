using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace BetterAutocastVPE;

using RimWorld;
using Settings;
using UnityEngine;

public class BetterAutocastVPE : Mod
{
    public BetterAutocastVPE(ModContentPack content)
        : base(content)
    {
#if DEBUG
        const string build = "Debug";
#else
        const string build = "Release";
#endif
        Log(
            $"Running Version {Assembly.GetAssembly(typeof(BetterAutocastVPE)).GetName().Version} "
                + build
        );
        if (VersionControl.CurrentVersion >= new Version(1, 5))
        {
            Warn(
                "This version of the mod is built for 1.4, support for 1.5 and up is not guaranteed, it will not receive any more updates"
            );
        }

        Harmony harmony = new("dev.tobot.vpe-better-autocast");
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
            Log("UnregisterPatch succeeded");
        }
        else
        {
            Error("UnregisterPatch failed");
        }
        harmony.PatchAll();
        Settings = GetSettings<AutocastSettings>();
        // In case some of the values were null (e.g. added between versions), write with default values.
        // Also prevents repeat errors for namespace change of Settings class
        WriteSettings();
    }

#nullable disable // Set in constructor.

    public static AutocastSettings Settings { get; private set; }

#nullable enable

    public void ResetSettings()
    {
        Settings = new();
    }

    public override void DoSettingsWindowContents(Rect inRect) =>
        AutocastSettingsWindow.DoSettingsWindowContents(inRect);

    public override string SettingsCategory() => AutocastSettingsWindow.SettingsCategory();

    private static bool UnregisterPatch(
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
                Error($"Method '{methodName}' not found in type '{targetType.FullName}'.");
                return false; // Method not found
            }
        }
        catch (Exception e)
        {
            // Handle any exceptions that may occur during unregistration
            Error("Error unregistering Harmony patch: " + e);
            return false;
        }
    }

    const string LogPrefix = "Better Autocasting - ";

    public static void DebugError(string message)
    {
#if DEBUG
        Error(message);
#endif
    }

    public static void Error(string message)
    {
        Verse.Log.Error(LogPrefix + message);
    }

    public static void DebugWarn(string message)
    {
#if DEBUG
        Warn(message);
#endif
    }

    public static void Warn(string message)
    {
        Verse.Log.Warning(LogPrefix + message);
    }

    public static void DebugLog(string message)
    {
#if DEBUG
        Log(message);
#endif
    }

    public static void Log(string message)
    {
        Verse.Log.Message(LogPrefix + message);
    }
}
