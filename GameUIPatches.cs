using BepInEx.Logging;
using HarmonyLib;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes;

namespace PolytopiaArchipelagoMW;
public static class GameUIPatches
{
    private static ManualLogSource logger = new("apmw: GameUIPatches");

    private static bool isConnectedToArchipelago = false;
    private static bool jumpIntoCustomGameMode = true;

    public static void Load(ManualLogSource logger)
    {
        Harmony.CreateAndPatchAll(typeof(GameUIPatches));

        GameUIPatches.logger = logger;
        isConnectedToArchipelago = true; // TODO: Implement actual connection check to Archipelago server
        logger.LogInfo("GameUIPatches loaded");
    }

    // Jump from New Game to Creative Mode Screen
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameModeScreen_UI2), nameof(GameModeScreen_UI2.OnShow))]
    private static bool GameModeScreen_UI2_JumpToCreative_Prefix(GameModeScreen_UI2 __instance)
    {
        if (!isConnectedToArchipelago)
        {
            logger.LogInfo("GameModeScreen_UI2.OnShow called. Not connected to Archipelago, skipping custom logic.");
            return true;
        }

        logger.LogInfo("GameModeScreen_UI2.OnShow called. Jumping to Creative Mode screen.");
        __instance.OnCustom();
        return false;
    }


    // --- EXPLORATION ---

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StartScreen_UI2), nameof(StartScreen_UI2.RunLayout))]
    private static void StartScreen_UI2_RunLayout_Prefix(StartScreen_UI2 __instance)
    {
        logger.LogInfo("StartScreen_UI2.RunLayout called.");
        var weeklyButton =  __instance.weeklyChallengeButton.button;
        // weeklyButton.OnDown = new UIButtonBase.ButtonAction(() =>
        // {
        //     logger.LogInfo("StartScreen_UI2.RunLayout: Weekly Challenge button clicked.");
        //     if (isConnectedToArchipelago)
        //     {
        //         logger.LogInfo("↪ Connected to Archipelago, skipping Weekly Challenge.");
        //         return;
        //     }
        //     weeklyButton.OnDown.Invoke();
        // });
    }

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(StartScreen_UI2), nameof(StartScreen_UI2.OnWeeklyChallengeClicked))]
    // private static bool StartScreen_UI2_OnWeeklyChallengeClicked_Prefix(StartScreen_UI2 __instance)
    // {
    //     logger.LogInfo("StartScreen_UI2.OnWeeklyChallengeClicked called.");
    //     return true;
    // }

   



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
    //     backButton = __instance.backButton;
    // }

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(UIRoundButton_UI2), nameof(UIRoundButton_UI2.OnPointerClick))]
    // private static bool UIRoundButton_UI2_OnPointerClick_Prefix(UIRoundButton_UI2 __instance)
    // {
    //     logger.LogInfo($"UIRoundButton_UI2.OnPointerClick called for button: {__instance.name}");
    //     if (__instance == backButton)
    //     {
    //         logger.LogInfo($"↪ Back button clicked. Returning to previous screen.");
    //     }
    //     return true;
    // }
}
