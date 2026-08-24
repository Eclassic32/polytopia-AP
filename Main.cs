using BepInEx.Logging;
using HarmonyLib;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes;

namespace PolytopiaArchipelagoMW;
public static class Main
{
    public static ManualLogSource logger = new("apmw: Main");

    private static TribeType[] enabledTribes = new TribeType[]
    {
        TribeType.Xinxi,
        TribeType.Kickoo,
        TribeType.Zebasi,
        TribeType.Aimo,
        TribeType.Elyrion
    };

    public static void Load(ManualLogSource logger)
    {
        Harmony.CreateAndPatchAll(typeof(Main));

        Main.logger = logger;
        // Archipelago.isConnectedToArchipelago = true; // TODO: Implement actual connection check to Archipelago server
        logger.LogInfo("Archipelago Mod Loaded");
    }

    // Pick Your Tribe - Disabling Tribes Not Enabled in Archipelago
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.IsTribeEnabled))]
    private static void GameSettings_IsTribeEnabled_Postfix(GameSettings __instance, TribeType tribeType, ref bool __result)
    {
        __result = enabledTribes.Contains(tribeType) ? __result : false;

        // logger.LogInfo($"GameSettings.IsTribeEnabled called for tribe: {tribeType}. Result: {__result}");
    }

    // Triggers on Enabling/Disabling tribe
    private static void SelectTribePopup_HideDisableBtn(SelectTribePopup __instance, Vector2? origin)
    {
        int idx = __instance.tribeData.idx;
        logger.LogInfo($"SelectTribePopup.Show called for tribe: {idx} ({(TribeType)idx}).");

        if (Archipelago.isConnectedToArchipelago && !enabledTribes.Contains((TribeType)idx))
        {
            logger.LogInfo($"↪ Tribe {(TribeType)idx} is missing in Archipelago.");
            UIToggleButton disableButton = __instance.disableButton;
            disableButton.enabled = false;
            disableButton.gameObject.SetActive(false);
        }
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
        state.TryGetWinner(out PlayerState winner);

        uint finalScore = player.score;
        logger.LogInfo("EndMatchCommand.Execute called.");
        logger.LogInfo($"↪ Final Score: {finalScore}");
        logger.LogInfo($"↪ Player {(player.Id == winner.Id ? "won" : "lost")} the game. Winner: {winner.GetNameInternal()} ({winner.tribe})");
    }


    // --- EXPLORATION ---
    



}
