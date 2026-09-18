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
    private static ManualLogSource logger = Main.logger;
    private static UIManager? uiManager;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartScreen_UI2), nameof(StartScreen_UI2.OnShow))]
    private static void StartScreen_UI2_OnShow_Postfix(StartScreen_UI2 __instance)
    {
        if (uiManager != null) { return; }
        uiManager = UnityEngine.Object.FindObjectOfType<UIManager>();
        logger.LogInfo("UIManager found: " + (uiManager != null ? "Yes" : "No"));
    }

    // Jump from New Game to Creative Mode Screen
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameModeScreen_UI2), nameof(GameModeScreen_UI2.OnShow))]
    private static bool GameModeScreen_UI2_JumpToCreative_Prefix(GameModeScreen_UI2 __instance)
    {
        if (!Archipelago.IsConnected)
        {
            logger.LogInfo("GameModeScreen_UI2.OnShow called. Not connected to Archipelago, skipping custom logic.");
            return true;
        }

        logger.LogInfo("GameModeScreen_UI2.OnShow called. Jumping to Creative Mode screen.");
        __instance.OnCustom();
        return false;
    }

    // Repurpose back button in Creative Mode Screen to Start Screen
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TribePickerScreen_UI2), nameof(TribePickerScreen_UI2.Init))]
    private static void TribePickerScreen_UI2_Init_Postfix(TribePickerScreen_UI2 __instance, RectTransform transform)
    {
        if (!Archipelago.IsConnected) { return; }
        logger.LogInfo($"TribePickerScreen_UI2.Init called");
        var backButton = __instance.backButton;
        backButton.ClearCallbacks();
        backButton.OnClickedSignal.Add(DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(JumpToMain));
    }

    internal static void JumpToMain()
    {
        var mainScreen = UIConstants.Screens.StartScreen;
        uiManager?.ShowScreen(mainScreen);
        
    }
}
