using BepInEx.Logging;
using HarmonyLib;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes;

namespace OtherSnippets;
public static class OtherSnippets
{
#pragma warning disable CS8618
    private static ManualLogSource logger;
#pragma warning restore CS8618

    private static bool isConnectedToArchipelago = false;
    private static bool shouldHidePerfectionAndDomination = true;
    private static TribeType[] enabledTribes = new TribeType[]
    {
        TribeType.Xinxi,
        TribeType.Kickoo,
        TribeType.Zebasi,
        TribeType.Aimo,
        TribeType.Elyrion
    };

    // public static void Load(ManualLogSource logger)
    // {
    //     Harmony.CreateAndPatchAll(typeof(OtherSnippets));

    //     OtherSnippets.logger = logger;
    //     isConnectedToArchipelago = true; // TODO: Implement actual connection check to Archipelago server
    //     logger.LogInfo("Polytopia Archipelago Mod Loaded");
    // }


    private static class Hooks
    {
        // Main Menu Screen Init
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameModeScreen), nameof(GameModeScreen.Init))]
        private static void GameModeScreen_Init_Prefix(GameModeScreen __instance)
        {
            logger.LogInfo("GameModeScreen.Init called.");
        }
        
        // New Game > Game Mode Screen Init
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameModeScreen_UI2), nameof(GameModeScreen_UI2.Init))]
        private static void GameModeScreen_UI2_Init_Prefix(GameModeScreen_UI2 __instance, RectTransform transform)
        {
            logger.LogInfo("GameModeScreen_UI2.Init called.");
        }

        // New Game > Game Mode > "Creative" Mode Load
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameModeScreen_UI2), nameof(GameModeScreen_UI2.OnCustom))]
        private static void GameModeScreen_UI2_OnCustom_Prefix(GameModeScreen_UI2 __instance)
        {
            logger.LogInfo("GameModeScreen_UI2.OnCustom called.");
        }

        // New Game > Game Mode > Perfection or Domination Preset Load
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameRules), nameof(GameRules.LoadPreset))]
        private static void GameRules_LoadPreset_Prefix(GameRules __instance, GameMode gameMode)
        {
            logger.LogInfo("GameRules.LoadPreset called.");
            logger.LogInfo($"Loaded preset: {gameMode.ToString()}");
        }

        // Game Over Action Execute
        // !!! TRIGGERS ON LAST TURN, NOT ON GAME OVER SCREEN !!!
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameOverAction), nameof(GameOverAction.Execute))]
        private static void GameOverAction_Execute_Postfix(GameOverAction __instance, GameState state)
        {
            logger.LogInfo("GameOverAction.Execute called. Game over.");
            logger.LogInfo($"Game state: {state.ToString()}");
            // logger.LogInfo($"Winner: {state.Winner}");
        }

        // Triggers on Enabling/Disabling tribe
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TribePickerScreen_UI2), nameof(TribePickerScreen_UI2.OnTribeEnabledChanged))]
        private static void TribePickerScreen_UI2_OnTribeEnabledChanged_Prefix(TribePickerScreen_UI2 __instance, int idx)
        {
            logger.LogInfo($"TribePickerScreen_UI2.OnTribeEnabledChanged called for tribe: {idx} ({(TribeType)idx}).");
        }
    }

    private static class Scripts
    {
        // Pick Your Tribe - Disabling Tribes Not Enabled in Archipelago
        // TODO: Still possible to select disabled tribe, then enable and pick on same popup.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.IsTribeEnabled))]
        private static void GameSettings_IsTribeEnabled_Postfix(GameSettings __instance, TribeType tribeType, ref bool __result)
        {
            __result = enabledTribes.Contains(tribeType);

            // logger.LogInfo($"GameSettings.IsTribeEnabled called for tribe: {tribeType}. Result: {__result}");
        }

        // New Game > Game Mode Screen Init
        // Hides the Perfection and Domination buttons in the Game Mode Screen UI2

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameModeScreen_UI2), nameof(GameModeScreen_UI2.Init))]
        private static void GameModeScreen_UI2_HideGameRulePresets(GameModeScreen_UI2 __instance, RectTransform transform)
        {
            var perfection = __instance.perfectionButton;
            var domination = __instance.dominationButton;
            var custom = __instance.creativeButton;
            var tutorial = __instance.tutorialButton;

            perfection?.gameObject.SetActive(false);
            domination?.gameObject.SetActive(false);
            tutorial?.gameObject.SetActive(false);

            var table = __instance.buttonContainer;
            if (table != null)
            {   
                table.Clear();
                table.AddCell(custom.Cast<IUILayoutable>());
                table.RunLayout();
            }
        }
        
    }    
}
