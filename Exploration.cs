using BepInEx.Logging;
using HarmonyLib;
using Polytopia.Data;

public class ExplorationPatches
{
    private readonly static ManualLogSource logger = new("apmw: ExplorationPatches");

    // Called when player learns a tech, including basic and starting tech
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(ActionUtils), nameof(ActionUtils.LearnTech))]
    // private static void ActionUtils_LearnTech_Postfix(GameState gameState, PlayerState playerState, TechData.Type type, int cost, bool shouldUseActions)
    // {
    //     logger.LogInfo($"ActionUtils.LearnTech called for player: {player.UserName} ({player.tribe}), type: {type}, cost: {cost}, shouldUseActions: {shouldUseActions}.");
    // }


    // Gets Called for every Tech check
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(PlayerExtensions), nameof(PlayerExtensions.HasTech))]
    // private static void PlayerExtensions_HasTech_Postfix(PlayerState player, TechData.Type tech, ref bool __result)
    // {
    //     if (!__result) { return; }
    //     logger.LogInfo($"PlayerExtensions.HasTech called for player: {player.UserName} ({player.tribe}), type: {tech}, Result: {__result}");
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.GetTechPrice))]
    // private static void GameLogicData_GetTechPrice_Postfix(GameLogicData __instance, TechData techData, PlayerState playerState, GameState state, ref int __result)
    // {
    //     logger.LogInfo($"GameLogicData.GetTechPrice called. Tech: {Localization.Get(techData.displayName)}, Player: {playerState.UserName} ({playerState.tribe}), Result: {__result}");
    // }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.GetImprovementData))]
    private static void GameLogicData_GetImprovementData_Postfix(GameLogicData __instance, ImprovementData.Type id, ref ImprovementData __result)
    {
        logger.LogInfo($"GameLogicData.GetImprovementData called. ID: {id}, Result: {__result.displayName}");
    }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.GetOverride))]
    // private static void GameLogicData_GetOverride_Postfix(GameLogicData __instance, TechData techData, PlayerState playerState, TribeData tribeData, ref TechData __result)
    // {
    //     logger.LogInfo($"GameLogicData.GetOverride called. Tech: {techData.ToString()}, Player: {playerState.ToString()}, Tribe: {tribeData.ToString()}. Result: {__result.ToString()}");
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.TryGetData), new Type[] { typeof(TribeType), typeof(TribeData)})]
    // private static void GameLogicData_TryGetData_TribeData_Postfix(GameLogicData __instance, TribeData tribeData, TechData.Type techType, ref TechData __result)
    // {
    //     logger.LogInfo($"GameLogicData.TryGetData(TribeData) called. Tribe: {tribeData.ToString()}, type: {techType}. Result: {__result.ToString()}");
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.TryGetData), new Type[] { typeof(TechData.Type) })]
    // private static void GameLogicData_TryGetData_TechData_Postfix(GameLogicData __instance, TribeData tribeData, TechData.Type techType, ref TechData __result)
    // {
    //     logger.LogInfo($"GameLogicData.TryGetData(TechData) called. Tribe: {tribeData.ToString()}, type: {techType}. Result: {__result.ToString()}");
    // }
    
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.TryGetData), new Type[] { typeof(ImprovementData.Type) })]
    // private static void GameLogicData_TryGetData_ImprovementData_Postfix(GameLogicData __instance, TribeData tribeData, ImprovementData.Type improvementType, ref TechData __result)
    // {
    //     logger.LogInfo($"GameLogicData.TryGetData(ImprovementData) called. Tribe: {tribeData.ToString()}, type: {improvementType}. Result: {__result.ToString()}");
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.TryGetData), new Type[] { typeof(UnitData.Type) })]
    // private static void GameLogicData_TryGetData_UnitData_Postfix(GameLogicData __instance, TribeData tribeData, UnitData.Type unitType, ref TechData __result)
    // {
    //     logger.LogInfo($"GameLogicData.TryGetData(UnitData) called. Tribe: {tribeData.ToString()}, type: {unitType}. Result: {__result.ToString()}");
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.TryGetData), new Type[] { typeof(PlayerAbility.Type) })]
    // private static void GameLogicData_TryGetData_PlayerAbility_Postfix(GameLogicData __instance, TribeData tribeData, PlayerAbility.Type abilityType, ref TechData __result)
    // {
    //     logger.LogInfo($"GameLogicData.TryGetData(PlayerAbility) called. Tribe: {tribeData.ToString()}, type: {abilityType}. Result: {__result.ToString()}");
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.TryGetData), new Type[] { typeof(TaskData.Type) })]
    // private static void GameLogicData_TryGetData_TaskData_Postfix(GameLogicData __instance, TribeData tribeData, TaskData.Type taskType, ref TechData __result)
    // {
    //     logger.LogInfo($"GameLogicData.TryGetData(TaskData) called. Tribe: {tribeData.ToString()}, type: {taskType}. Result: {__result.ToString()}");
    // }

}