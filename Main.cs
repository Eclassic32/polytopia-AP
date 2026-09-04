using BepInEx.Logging;
using HarmonyLib;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSystem.Collections.Generic;

namespace PolytopiaArchipelagoMW;
public static class Main
{
    public static ManualLogSource logger = new("apmw: Main");

    public static void Load(ManualLogSource logger)
    {
        Harmony.CreateAndPatchAll(typeof(Main));

        Main.logger = logger;
        // Archipelago.isConnectedToArchipelago = true; // TODO: Implement actual connection check to Archipelago server
        logger.LogInfo("Archipelago Mod Loaded");
    }

    // Pick Your Tribe - Hiding not playable tribes in Archipelago
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TribePickerScreen_UI2), nameof(TribePickerScreen_UI2.RunLayout))]
    private static void TribePickerScreen_UI2_RunLayout_Postfix(TribePickerScreen_UI2 __instance, ScreenBase_UI2.ScreenSize screenSize)
    {
        if (!Archipelago.IsConnected) { return; }
        logger.LogInfo($"TribePickerScreen_UI2.RunLayout called");

        TribeType[] playableTribes = Archipelago.SlotData.GetPlayableTribes();
        
        foreach (TribeType tribe in Archipelago.APTribeOrder)
        {
            
            UIRoundButton_UI2 btn = __instance.tribeButtons[tribe];

            if (!playableTribes.Contains(tribe))
            {
                btn.enabled = false;
                btn.gameObject.SetActive(false);
                logger.LogInfo($"↪ Tribe {tribe} is not playable in Archipelago.");
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

        // logger.LogInfo($"PlayableTribes: {string.Join(", ", Archipelago.GetPlayableTribes())}");
        // logger.LogInfo($"EnabledTribes: {string.Join(", ", Archipelago.GetEnabledTribes())}");
        // logger.LogInfo($"GameSettings.IsTribeEnabled called for tribe: {tribeType}. Result: {__result}");
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


    // Game Over Screen - Logging Final Score and Winner
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EndMatchCommand), nameof(EndMatchCommand.Execute))]
    private static void EndMatchCommand_Execute_Postfix(EndMatchCommand __instance, GameState state)
    {
        PlayerState player = state.GetFirstHumanPlayer();
        // state.TryGetWinner(out PlayerState winner);

        uint finalScore = player.score;
        logger.LogInfo("EndMatchCommand.Execute called.");
        logger.LogInfo($"↪ Final Score: {finalScore}");

        int score_K = (int)finalScore/1000; 
        Archipelago.SendScoreLocation(player.tribe, score_K);
        Archipelago.CheckIfGoaled();
    }


    // --- EXPLORATION ---
    



}
