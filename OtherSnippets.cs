using BepInEx.Logging;
using HarmonyLib;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes;

namespace OtherSnippets;
public static class OtherSnippets
{
    private static ManualLogSource logger = new("apmw: OtherSnippets");

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

        // Fires on any score set durning the game, (fires a ton)
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ScoreContainer), nameof(ScoreContainer.SetScore))]
        private static void ScoreContainer_SetScore_Prefix(ScoreContainer __instance, float score, int bestScore)
        {
            logger.LogInfo($"ScoreContainer.SetScore called. Score: {score}; Best Score: {bestScore}");
        }

        // Called after ending the turn?
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIWorldScoreBase), nameof(UIWorldScoreBase.Update))]
        private static void UIWorldScoreBase_Update_Prefix(UIWorldScoreBase __instance)
        {
            logger.LogInfo($"UIWorldScoreBase.Update called.");
        }

        // Main Menu > High Score (online scores per tribe)
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HighScoreScreen), nameof(HighScoreScreen.Show))]
        private static void HighScoreScreen_Show_Prefix(HighScoreScreen __instance, bool instant)
        {
            logger.LogInfo($"HighScoreScreen.Show called.");
        }
        
        // Main Menu > Throne Room (stats screen)
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ThroneRoomScreen), nameof(ThroneRoomScreen.Show))]
        private static void ThroneRoomScreen_Show_Prefix(ThroneRoomScreen __instance, bool instant)
        {
            logger.LogInfo($"ThroneRoomScreen.Show called.");
        }

        // Called on both Player and AI units
        // Called once when new unit is created/destroyed, __result is the score of that unit type
        // Called twice when water unit upgraded/landed, once land unit score, once water unit score
        // Called once per PLAYER unit on Game Over Scores screen
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ScoreSheet), nameof(ScoreSheet.GetUnitScore))]
        private static void ScoreSheet_GetUnitScore_Postfix(ScoreSheet __instance, UnitState unitState, GameState gameState, ref int __result)
        {
            logger.LogInfo($"ScoreSheet.GetUnitScore called. UnitState: {unitState}; GameState: {gameState}; Result: {__result}");
        }

        // Called on actuall UI Score update 
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ScoreContainer), nameof(ScoreContainer.UpdateText))]
        private static void ScoreContainer_UpdateText_Prefix(ScoreContainer __instance)
        {
            // logger.LogInfo($"ScoreContainer.UpdateText called. {__instance.score}");
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
