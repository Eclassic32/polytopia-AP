using BepInEx.Logging;
using HarmonyLib;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Polytopia.Data;

namespace PolytopiaArchipelagoMW;
public static class Main
{
    public static ManualLogSource logger = new("apmw: Main");
    public static GameState? gameState;
    public static PlayerState playerState = new();

    public static void Load(ManualLogSource logger)
    {
        Harmony.CreateAndPatchAll(typeof(Main));
        Harmony.CreateAndPatchAll(typeof(ScorePatches));
        Harmony.CreateAndPatchAll(typeof(TribePatches));
        Harmony.CreateAndPatchAll(typeof(TechPatches));
        Harmony.CreateAndPatchAll(typeof(ExplorationPatches));

        Main.logger = logger;
        logger.LogInfo("Archipelago Mod Loaded");
    }

    // Get GameState 
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.OnLevelLoaded))]
    private static void GameManager_OnLevelLoaded_Postfix(GameManager __instance)
    {
        logger.LogInfo("GameManager.OnLevelLoaded called.");
        Archipelago.SetClientState(ArchipelagoClientState.ClientPlaying);
        
        gameState = __instance.client.GameState;
        playerState = gameState.GetFirstHumanPlayer();
    }
}

public class ScorePatches
{
    private readonly static ManualLogSource logger = Main.logger;

    // Game Over Screen - Logging Final Score and Winner
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EndMatchCommand), nameof(EndMatchCommand.Execute))]
    private static void EndMatchCommand_Execute_Postfix(EndMatchCommand __instance, GameState state)
    {
        PlayerState player = Main.playerState;
        // state.TryGetWinner(out PlayerState winner);

        uint finalScore = player.score;
        logger.LogInfo("EndMatchCommand.Execute called.");
        logger.LogInfo($"↪ Final Score: {finalScore}");

        int score_K = (int)finalScore/1000; 
        Archipelago.SendLocations.Score(player.tribe, score_K, true);
    }

    // Called on actuall UI Score update 
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScoreContainer), nameof(ScoreContainer.UpdateText))]
    private static void ScoreContainer_UpdateText_Prefix(ScoreContainer __instance)
    {
        if (!Archipelago.IsConnected || !Archipelago.SlotData.SendScoreChecksImmediately) { return; }

        GameState? gameState = Main.gameState;
        if (gameState == null) {
            logger.LogWarning("GameState is not found");
            return; 
        }
        
        PlayerState player = Main.playerState;
        TribeType tribe = player.tribe;
        float score = __instance.score;
        int score_K = (int)score/1000;

        Archipelago.SendLocations.Score(tribe, score_K, false);
    }
}

public class TribePatches
{
    private readonly static ManualLogSource logger = Main.logger;

    // Pick Your Tribe - Hiding not playable tribes in Archipelago
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TribePickerScreen_UI2), nameof(TribePickerScreen_UI2.RunLayout))]
    private static void TribePickerScreen_UI2_RunLayout_Postfix(TribePickerScreen_UI2 __instance, ScreenBase_UI2.ScreenSize screenSize)
    {
        if (!Archipelago.IsConnected) { return; }
        logger.LogInfo($"TribePickerScreen_UI2.RunLayout called");

        Archipelago.SetClientState(ArchipelagoClientState.ClientReady);
        TribeType[] playableTribes = Archipelago.SlotData.GetPlayableTribes();
        TribeType[] receivedTribes = Archipelago.GetReceivedTribes();
        
        foreach (TribeType tribe in Constants.APTribeOrder)
        {
            
            UIRoundButton_UI2 btn = __instance.tribeButtons[tribe];

            if (!playableTribes.Contains(tribe))
            {
                btn.enabled = false;
                btn.gameObject.SetActive(false);
                logger.LogInfo($"↪ Tribe {tribe} is not playable in Archipelago.");
            }

            if (!receivedTribes.Contains(tribe))
            {
                btn.bg.color = UIConstants.COLOR_DELETE;
                logger.LogInfo($"↪ Tribe {tribe} is not received in Archipelago.");
            }
        }
    }
        

    // Disabling Tribes Not Received in Archipelago
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.IsTribeEnabled))]
    private static void GameSettings_IsTribeEnabled_Postfix(GameSettings __instance, TribeType tribeType, ref bool __result)
    {
        if (!Archipelago.IsConnected) { return; }
        __result = Archipelago.GetReceivedTribes().Contains(tribeType) && __result;
    }

    // Triggers on Enabling/Disabling tribe
    private static void SelectTribePopup_HideDisableBtn(SelectTribePopup __instance, Vector2? origin)
    {
        int idx = __instance.tribeData.idx;
        logger.LogInfo($"SelectTribePopup.Show called for tribe: {idx} ({(TribeType)idx}).");

        if (!Archipelago.IsConnected || Archipelago.GetReceivedTribes().Contains((TribeType)idx)) { return; }

        logger.LogInfo($"↪ Tribe {(TribeType)idx} is missing in Archipelago.");
        UIToggleButton disableButton = __instance.disableButton;
        disableButton.enabled = false;
        disableButton.gameObject.SetActive(false);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SelectTribePopup), nameof(SelectTribePopup.Show), new Type[] {})]
    private static void SelectTribePopup_Show_Postfix(SelectTribePopup __instance)
        => SelectTribePopup_HideDisableBtn(__instance, null);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SelectTribePopup), nameof(SelectTribePopup.Show), new Type[] { typeof(Vector2) })]
    private static void SelectTribePopup_ShowV2_Postfix(SelectTribePopup __instance, Vector2 origin)
        => SelectTribePopup_HideDisableBtn(__instance, origin);

}

public class TechPatches
{
    private readonly static ManualLogSource logger = Main.logger;

    // Called when player learns a tech, including basic and starting tech
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ActionUtils), nameof(ActionUtils.LearnTech))]
    private static void ActionUtils_LearnTech_Postfix(GameState gameState, PlayerState playerState, TechData.Type type, int cost, bool shouldUseActions)
    {
        if (!Archipelago.IsConnected) { return; }
        PlayerState player = Main.playerState;
        if (playerState.Id != player.Id || cost == 0) { return; }
        logger.LogInfo($"ActionUtils.LearnTech called. Player: {playerState.UserName} ({playerState.tribe}), type: {type}, cost: {cost}, shouldUseActions: {shouldUseActions}.");
        Archipelago.SendLocations.Technology(playerState.tribe, type);
    }
}

