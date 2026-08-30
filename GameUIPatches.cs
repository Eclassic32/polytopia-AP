using BepInEx.Logging;
using HarmonyLib;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
using UnityEngine.Rendering;

namespace PolytopiaArchipelagoMW;
public static class GameUIPatches
{
    private static ManualLogSource logger = new("apmw: GameUIPatches");

    public static void Load(ManualLogSource logger)
    {
        Harmony.CreateAndPatchAll(typeof(GameUIPatches));

        GameUIPatches.logger = logger;
        logger.LogInfo("GameUIPatches loaded");
    }

    // Jump from New Game to Creative Mode Screen
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(StartScreen_UI2), nameof(StartScreen_UI2.ClickNewGame))]
    // private static bool StartScreen_UI2_JumpToCreative_Postfix(StartScreen_UI2 __instance)
    // {
    //     if (!Archipelago.isConnected) { return true; }
    //     logger.LogInfo("GameModeScreen_UI2.OnShow called. Jumping to Creative Mode screen.");
    //     __instance.
    //     return false;
    // }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameModeScreen_UI2), nameof(GameModeScreen_UI2.OnShow))]
    private static bool GameModeScreen_UI2_JumpToCreative_Prefix(GameModeScreen_UI2 __instance)
    {
        if (!Archipelago.isConnected)
        {
            logger.LogInfo("GameModeScreen_UI2.OnShow called. Not connected to Archipelago, skipping custom logic.");
            return true;
        }

        logger.LogInfo("GameModeScreen_UI2.OnShow called. Jumping to Creative Mode screen.");
        __instance.OnCustom();
        return false;
    }


    // --- EXPLORATION ---

    // In game > Game Stats
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameModeUtils), nameof(GameModeUtils.GetTitle))]
    private static void GameModeUtils_GetTitle_Prefix(GameMode gameMode, ref string __result)
    {
        logger.LogInfo($"GameModeUtils.GetTitle called for game mode: {gameMode}. Result: {__result}");
    }


    // private static UIRoundButton_UI2? backButton = null;

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(TribePickerScreen_UI2), nameof(TribePickerScreen_UI2.Init))]
    // private static void TribePickerScreen_UI2_Init_Postfix(TribePickerScreen_UI2 __instance, RectTransform transform)
    // {
    //     logger.LogInfo($"TribePickerScreen_UI2.Init called");
    //     var backButton = __instance.backButton;
    //     backButton.OnClickedSignal.Add(DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(JumpToMain));
    // }

    // internal static void JumpToMain(TribePickerScreen_UI2 __instance)
    // {
    //     __instance.
    // }
}
