using BepInEx.Logging;
using HarmonyLib;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes;

namespace PolytopiaArchipelagoMW;
public static class Main
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

    public static void Load(ManualLogSource logger)
    {
        Harmony.CreateAndPatchAll(typeof(Main));

        Main.logger = logger;
        isConnectedToArchipelago = true; // TODO: Implement actual connection check to Archipelago server
        logger.LogInfo("Polytopia Archipelago Mod Loaded");
    }

    

    // Pick Your Tribe - Disabling Tribes Not Enabled in Archipelago
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSettings), nameof(GameSettings.IsTribeEnabled))]
    private static void GameSettings_IsTribeEnabled_Postfix(GameSettings __instance, TribeType tribeType, ref bool __result)
    {
        __result = enabledTribes.Contains(tribeType) ? __result : false;

        // logger.LogInfo($"GameSettings.IsTribeEnabled called for tribe: {tribeType}. Result: {__result}");
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

        __instance.OnCustom();
        logger.LogInfo("GameModeScreen_UI2.OnShow called. Jumped to Creative Mode screen.");
        return false;
    }

    // Triggers on Enabling/Disabling tribe
    private static void SelectTribePopup_HideDisableBtn(SelectTribePopup __instance, Vector2? origin)
    {
        int idx = __instance.tribeData.idx;
        logger.LogInfo($"SelectTribePopup.Show called for tribe: {idx} ({(TribeType)idx}).");

        if (isConnectedToArchipelago && !enabledTribes.Contains((TribeType)idx))
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


    // Later Usage?
    private static void TribeHintLogic(TribeType tribe)
    {
        logger.LogInfo($"↪ Hint button clicked for tribe {tribe}. Sending hint to Archipelago.");
                apchat($"!hint Tribe - {tribe}");
    }

    private static void apchat(string message) // TODO: Implement actual chat functionality with Archipelago server
    {
        if (isConnectedToArchipelago)
        {
            logger.LogInfo($"[APCHAT] {message}");
        }
    }
    

    // --- EXPLORATION ---

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameModeUtils), nameof(GameModeUtils.GetTitle))]
    private static void GameModeUtils_GetTitle_Prefix(GameMode gameMode, ref string __result)
    {
        logger.LogInfo($"GameModeUtils.GetTitle called for game mode: {gameMode}. Result: {__result}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameModeScreen_UI2), nameof(GameModeScreen_UI2.OnCustom))]
    private static void GameModeScreen_UI2_OnCustom_Prefix(GameModeScreen_UI2 __instance)
    {
        logger.LogInfo("GameModeScreen_UI2.OnCustom called.");
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
}
