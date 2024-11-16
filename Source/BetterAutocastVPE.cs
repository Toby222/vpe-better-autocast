using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace BetterAutocastVPE;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Settings;
using UnityEngine;
using VanillaPsycastsExpanded;
using VFECore.Abilities;

#if DEBUG
#warning Building in Debug mode
#endif

public class BetterAutocastVPE : Mod
{
    public BetterAutocastVPE(ModContentPack content)
        : base(content)
    {
#if v1_5
        const string GAME_VERSION = "v1.5";
#else
#error No version defined
        const string GAME_VERSION = "UNDEFINED";
#endif

#if DEBUG
        const string build = "Debug";
#else
        const string build = "Release";
#endif
        Log(
            $"Running Version {Assembly.GetAssembly(typeof(BetterAutocastVPE)).GetName().Version} {build} compiled for RimWorld version {GAME_VERSION}"
        );

        Designator_Area_Runecircle.ModActive = ModsConfig.IsActive("Chairheir.VPERunesmith");

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

        DebugLog("PatchAll ran successfully");
        // Casting necessary for compat with Harmony 2.2.2.0 (game versions under 1.5)
        HarmonyMethod postfix =
            new(((Delegate)Patches.CanAutoCast_EnableHandledPsycasts.Postfix).Method);
        DebugLog("postfix created successfully");

        MethodInfo ability = typeof(Ability).GetMethod(
            "get_" + nameof(Ability.CanAutoCast),
            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public
        );
        MethodInfo ability_spawn = typeof(Ability_Spawn).GetMethod(
            "get_" + nameof(Ability_Spawn.CanAutoCast),
            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public
        );
        MethodInfo ability_spawnbuilding = typeof(Ability_SpawnBuilding).GetMethod(
            "get_" + nameof(Ability_SpawnBuilding.CanAutoCast),
            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public
        );

        harmony.Patch(ability, postfix: postfix);
        harmony.Patch(ability_spawn, postfix: postfix);
        harmony.Patch(ability_spawnbuilding, postfix: postfix);
        DebugLog("Patched CanAutoCast");

        Settings = GetSettings<AutocastSettings>();
        // In case some of the values were null (e.g. added between versions), write with default values.
        // Also prevents repeat errors for namespace change of Settings class
        WriteSettings();
    }

#if DEBUG
    [LudeonTK.DebugAction(
        category = "Better Autocasting",
        name = "Give all handled psycasts",
        allowedGameStates = LudeonTK.AllowedGameStates.PlayingOnMap,
        requiresIdeology = true,
        actionType = LudeonTK.DebugActionType.ToolMapForPawns
    )]
    public static void GiveAllPsycasts(Pawn pawn)
    {
        Traverse
            .Create(typeof(DebugToolsPawns))
            .Method("GivePsylink")
            .GetValue<List<LudeonTK.DebugActionNode>>()
            .Last()
            .pawnAction(pawn);
        PsycastUtility.ResetPsycasts(pawn);
        pawn.Psycasts().ImproveStats(100);
        foreach (PsycasterPathDef pathDef in DefDatabase<PsycasterPathDef>.AllDefsListForReading)
        {
            pawn.Psycasts().UnlockPath(pathDef);
        }
        foreach (
            AbilityDef ability in PsycastingHandler.abilityHandlers.Keys.Select(defName =>
                DefDatabase<AbilityDef>.GetNamed(defName)
            )
        )
        {
            pawn.GetComp<CompAbilities>().GiveAbility(ability);
        }
    }

    [LudeonTK.DebugAction(
        category = "Better Autocasting",
        name = "List handled psycasts with too many targets",
        allowedGameStates = LudeonTK.AllowedGameStates.Invalid
    )]
    public static void CheckAllPsycastsValid()
    {
        DebugError(
            "Invalid psycasts:\n"
                + PsycastingHandler
                    .abilityHandlers.Keys.Where(defName =>
                        DefDatabase<AbilityDef>.GetNamed(defName).targetCount > 1
                    )
                    .ToCommaList()
        );
    }

    [LudeonTK.DebugAction(
        category = "Better Autocasting",
        name = "Generate mod description",
        allowedGameStates = LudeonTK.AllowedGameStates.Invalid
    )]
    public static void PrintDescription()
    {
        IEnumerable<string> handledAbilityDefNames = PsycastingHandler.abilityHandlers.Keys;
        IEnumerable<(string label, string modName)> handledAbilies = handledAbilityDefNames.Select(
            abilityDefName =>
            {
                AbilityDef? abilityDef = DefDatabase<AbilityDef>.GetNamed(abilityDefName, false);
                if (abilityDef is null)
                    Warn(abilityDefName + " def not found");
                return (
                    (abilityDef?.LabelCap ?? abilityDefName).ToStringSafe()!,
                    (
                        abilityDef?.modContentPack.ModMetaData.GetWorkshopName() ?? "unknown mod"
                    ).ToStringSafe()
                );
            }
        );
        IEnumerable<IGrouping<string, string>>? abilitiesByMod = handledAbilies
            .GroupBy(ability => ability.modName, ability => ability.label)
            .OrderBy(group => group.Key);
        IEnumerable<string> abilitiesListByMod = abilitiesByMod.Select(group =>
            group.Key + ":\n" + string.Join("\n", group)
        );
        string summary = string.Join("\n\n", abilitiesListByMod);

        Log("Handled psycasts:\n" + summary);
    }
#endif

#nullable disable // Set in constructor.

    public static AutocastSettings Settings { get; private set; }

#nullable enable

    public void Uninstall()
    {
        string fileNamePrefix = DateTime.Now.ToString("s", CultureInfo.InvariantCulture);
        GameDataSaveLoader.SaveGame(
            fileNamePrefix + " - " + "BetterAutocastVPE.BeforeUninstall".TranslateSafe()
        );
        Find.TickManager.Pause();
        foreach (Map map in Find.Maps)
        {
            map.areaManager.AllAreas.RemoveAll(area =>
                area is Area_IceCrystal or Area_SolarPinhole or Area_CraftTimeskip
            );

            foreach (Pawn pawn in map.mapPawns.AllPawns)
            {
                if (pawn.TryGetComp<CompAbilities>() is not CompAbilities compAbilities)
                    continue;

                foreach (Ability ability in compAbilities.LearnedAbilities)
                {
                    if (PsycastingHandler.HasHandler(ability.def.defName))
                        ability.autoCast = false;
                }
            }
        }
        List<ModContentPack> original = LoadedModManager.RunningMods.ToList();
        Traverse<List<ModContentPack>> traverse = Traverse
            .Create(typeof(LoadedModManager))
            .Field<List<ModContentPack>>("runningMods");
        traverse.Value = original
            .Where(x => !x.ModMetaData.SamePackageId(Content.PackageId))
            .ToList();
        GameDataSaveLoader.SaveGame(
            fileNamePrefix + " - " + "BetterAutocastVPE.AfterUninstall".TranslateSafe()
        );
        traverse.Value = original;
        GenScene.GoToMainMenu();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        AutocastSettingsWindow.confirmReset = false;
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

    public static void DebugError(string message, int? key = null)
    {
#if DEBUG
        Error(message, key);
#endif
    }

    public static void Error(string message, int? key = null)
    {
        if (key is int keyNotNull)
            Verse.Log.ErrorOnce(LogPrefix + message, keyNotNull);
        else
            Verse.Log.Error(LogPrefix + message);
    }

    public static void DebugWarn(string message, int? key = null)
    {
#if DEBUG
        Warn(message, key);
#endif
    }

    public static void Warn(string message, int? key = null)
    {
        if (key is int keyNotNull)
            Verse.Log.WarningOnce(LogPrefix + message, keyNotNull);
        else
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
