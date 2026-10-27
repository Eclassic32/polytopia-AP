using BepInEx.Logging;
using HarmonyLib;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using UnityEngine;
using Il2CppSystem.Linq;

namespace PolytopiaArchipelagoMW;
public class ExplorationPatches
{
    private readonly static ManualLogSource logger = Main.logger;

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

    private static Il2CppSystem.Collections.Generic.List<TechItem> techItems = new Il2CppSystem.Collections.Generic.List<TechItem>();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TechView), nameof(TechView.Show))]
    private static void TechView_Show_Postfix(TechView __instance, bool instant)
    {
        if (!Archipelago.IsConnected) { return; }
        if (Main.PlayerState == null) { return; }
        logger.LogInfo($"TechView.Show called.");

        var techItems = __instance.items;
        foreach (var item in techItems)
        {
            TechData.Type techType = item.TechData.type;
            bool isSent = Archipelago.IsTechLocationSent(Main.PlayerState.tribe, techType);
            bool isReceived = Archipelago.IsTechItemReceived(Main.PlayerState.tribe, techType);
            // bool isComplete = item.Complete; // can be and is learned
            // logger.LogInfo($"↪ TechView Item: {techType}, color: {item.bg.color}, complete: {isComplete},");
            if (isSent)
            {
                logger.LogInfo($"↪ TechView Item: {techType} location is sent.");
                item.bg.color = UIConstants.COLOR_DEFAULT;
            }
            if (isReceived)
            {
                logger.LogInfo($"↪ TechView Item: {techType} item is received.");
                item.bg.color = UIConstants.COLOR_COMPLETE;
            }
            if (isSent && isReceived)
            {
                // logger.LogInfo($"↪ TechView Item: {techType} location is sent and item is received.");
                item.bg.color = Color.white;
            }
        }
    }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(TechItem), nameof(TechItem.Complete), MethodType.Getter)]
    // private static void TechItem_Complete_Getter_Postfix(TechItem __instance, ref bool __result)
    // {
    //     if (!Archipelago.IsConnected) { return; }
    //     if (Main.PlayerState == null) { return; }

    //     TechData.Type techType = __instance.TechData.type;
    //     bool isSent = Archipelago.IsTechLocationSent(Main.PlayerState.tribe, techType);
    //     __result = isSent;
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(TechItem), nameof(TechItem.RefreshState))]
    // private static void TechItem_RefreshState_Postfix(TechItem __instance, bool forceUnavaliable)
    // {
    //     logger.LogInfo($"TechItem.RefreshState called for tech: {__instance.TechData.type}, forceUnavaliable: {forceUnavaliable}");
    // }

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.IsUnlocked), new Type[] { typeof(TechData.Type), typeof(PlayerState) })]
    // private static bool GameLogicData_IsUnlocked_Prefix(GameLogicData __instance, TechData.Type type, PlayerState player, ref bool __result)
    // {
    //     if (!Archipelago.IsConnected) { return true; }
    //     if (Main.PlayerState == null || Main.PlayerState.Id != player.Id) { return true; }

    //     bool isSent = Archipelago.IsTechLocationSent(player.tribe, type);
    //     logger.LogWarning($"GameLogicData.IsUnlocked called for player: {player.UserName} ({player.tribe}), tech: {type}. Location is sent: {isSent}. Original Result: {__result}");
    //     __result = isSent;
    //     return false;
    // }

    // Attempt 2
    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.IsUnlocked), new Type[] { typeof(TechData.Type), typeof(PlayerState) })]
    // private static void GameLogicData_IsUnlocked_Prefix(GameLogicData __instance, TechData.Type type, PlayerState player, ref bool __result)
    // {
    //     if (!Archipelago.IsConnected) { return; }
    //     if (Main.PlayerState == null || Main.PlayerState.Id != player.Id) { return; }

    //     bool isSent = Archipelago.IsTechLocationSent(player.tribe, type);
    //     if (isSent)
    //     {
    //         player.availableTech.Add(type);            
    //     } else
    //     {
    //         player.availableTech.Remove(type);
    //     }
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.IsUnlocked), new Type[] { typeof(ImprovementData.Type), typeof(PlayerState) })]
    // private static void GameLogicData_IsUnlocked_Improvement_Postfix(GameLogicData __instance, ImprovementData.Type type, PlayerState player, ref bool __result)
    // {
    //     if (Main.PlayerState == null || Main.PlayerState.Id != player.Id) { return; }
    //     logger.LogWarning($"GameLogicData.IsUnlocked (Improvement) called for player: {player.UserName} ({player.tribe}), Improvement: {type}. Original Result: {__result}");
    // }















    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(InteractionBar), nameof(InteractionBar.Show))]
    // private static void InteractionBar_Show_Postfix(InteractionBar __instance, bool instant, bool force)
    // {
    //     logger.LogInfo($"InteractionBar.Show called: instant: {instant}, force: {force}");
    // }
    
    // // [HarmonyPostfix]
    // // [HarmonyPatch(typeof(InteractionBar), nameof(InteractionBar.AddTrainUnitButtons))]
    // // private static void InteractionBar_AddTrainUnitButtons_Postfix(InteractionBar __instance, Tile tile)
    // // {
    // //     if (!Archipelago.IsConnected) { return; }
    // //     if (Main.PlayerState == null) { return; }

    // //     logger.LogInfo($"InteractionBar.AddTrainUnitButtons called: tile: {tile.Coordinates}, unit: {tile.Unit?.unitData?.displayName}, resource: {tile.resource?.data?.displayName}");
    // // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(InteractionBar), nameof(InteractionBar.OnBuildingSelected))]
    // private static void InteractionBar_OnBuildingSelected_Postfix(InteractionBar __instance, Building building)
    // {
    //     if (building == null) { 
    //         logger.LogWarning($"InteractionBar.OnBuildingSelected called: building is null");    
    //         return; 
    //     }
    //     var data = building.data;
    //     logger.LogWarning($"InteractionBar.OnBuildingSelected called: building: {data} ({data.displayName}), type: {data.type}, tile: {building.tile.Coordinates}");
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(InteractionBar), nameof(InteractionBar.OnTileSelected))]
    // private static void InteractionBar_OnTileSelected_Postfix(InteractionBar __instance, Tile tile)
    // {
    //     if (tile == null) { 
    //         logger.LogWarning($"InteractionBar.OnTileSelected called: tile is null");    
    //         return; 
    //     }

    //     var playerCities = Helpers.ToList(PlayerExtensions.GetCityTiles(Main.PlayerState, Main.GameState));
    //     bool isCityTile = playerCities.Any(cityTile => cityTile.coordinates == tile.Coordinates);

    //     logger.LogWarning($"InteractionBar.OnTileSelected called: tile: {tile.Coordinates}, unit: {tile.Unit?.unitData?.displayName}, resource: {tile.resource?.data?.displayName}, isCityTile: {isCityTile}");

    //     // if (isCityTile)
    //     // {
    //     //     __instance.ClearButtons();
    //     //     __instance.AddTrainUnitButtons(tile);
    //     // }
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(InteractionBar), nameof(InteractionBar.OnUnitSelected))]
    // private static void InteractionBar_OnUnitSelected_Postfix(InteractionBar __instance, Unit unit)
    // {
    //     logger.LogWarning($"InteractionBar.OnUnitSelected called: unit: {unit.unitData?.displayName}");
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(CommandUtils), nameof(CommandUtils.GetTrainableUnits))]
    // private static void CommandUtils_GetTrainableUnits_Postfix(GameState gameState, PlayerState player, TileData tile, bool includeUnavailable, ref Il2CppSystem.Collections.Generic.List<TrainCommand> __result)
    // {
    //     if (!Archipelago.IsConnected) { return; }
    //     if (Main.PlayerState == null || Main.PlayerState.Id != player.Id) { return; }
    //     logger.LogInfo($"CommandUtils.GetTrainableUnits called for player: {player.UserName} ({player.tribe}), tile: {tile?.coordinates}. Original Result: {string.Join(", ", __result)}");

    //     List<TrainCommand> result = new();
    //     TrainCommand a = new();
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(CommandValidation), nameof(CommandValidation.CanTrainUnit))]
    // private static void CommandValidation_CanTrainUnit_Postfix(PlayerState player, GameState state, UnitData.Type unitType, ref bool __result)
    // {
    //     if (!Archipelago.IsConnected) { return; }
    //     if (Main.PlayerState == null || Main.PlayerState.Id != player.Id) { return; }

    //     bool isSent = HasUnitItem(unitType);
    //     logger.LogWarning($"CommandValidation.CanTrainUnit called for player: {player.UserName} ({player.tribe}), Unit: {unitType}. Location is sent: {isSent}. Original Result: {__result}");
    //     __result = isSent;
    // }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.GetUnlockedUnits))]
    private static void GameLogicData_GetUnlockedUnits_Postfix(GameLogicData __instance, PlayerState player, bool shouldIncludeHidden, ref Il2CppSystem.Collections.Generic.List<UnitData> __result)
    {
        if (!Archipelago.IsConnected) { return; }
        if (Main.PlayerState == null || Main.PlayerState.Id != player.Id) { return; }

        List<UnitData> ogResult = Helpers.ToList(__result);
        TribeData tribeData = new((IntPtr)player.tribe);
        logger.LogInfo($"GameLogicData.GetUnlockedUnits called for player: {player.UserName} ({player.tribe}) [{tribeData}]. Original Result: {string.Join(", ", ogResult.Select(u => u.type))}");


        List<UnitData> result = new();
        var allUnits = Helpers.ToDictionary(__instance.AllUnitData);
        foreach (var unit in allUnits.Values)
        {
            // if (tribeData != null && tribeData.unitOverrides != null && tribeData.unitOverrides?.Count > 0) {
            //     if (tribeData.unitOverrides.TryGetValue(unit.type, out _))
            //     {
            //         logger.LogInfo($"↪ GameLogicData.GetUnlockedUnits: Unit {unit.displayName} ({unit.type}) is overridden for tribe {tribeData.type}");
            //         continue;
            //     }
            // }

            if (HasUnitItem(unit.type))
            {
                result.Add(unit);
                // logger.LogWarning($"↪ GameLogicData.GetUnlockedUnits: Unit {unit.displayName} ({unit.type}) is unlocked");
            }
            else
            {
                // logger.LogError($"↪ GameLogicData.GetUnlockedUnits: Unit {unit.displayName} ({unit.type}) is not unlocked");
            }
        }
        __result = Helpers.ToIl2CppList(result);

        if (tribeData != null && tribeData.unitOverrides != null && tribeData.unitOverrides?.Count > 0) {
            foreach (var kvp in tribeData.unitOverrides)
            {
                var unitType = kvp.Key;
                var overrideUnit = kvp.Value;
                // result.Add(overrideUnit);
                logger.LogInfo($"↪ GameLogicData.GetUnlockedUnits: Override Unit {overrideUnit.displayName} ({overrideUnit.type}) for tribe {tribeData.type}");
                
            }
        }
        logger.LogWarning($"GameLogicData.GetUnlockedUnits final result: {string.Join(", ", result.Select(u => $"{u.type} "))}");
    }

    public static readonly Dictionary<UnitData.Type, TechData.Type> UnitToTechMapping = new()
    {
        // { UnitData.Type.None, TechData.Type.Basic }, // Not used
        { UnitData.Type.Warrior, TechData.Type.Basic }, // Starting Unit, Not in Tech Tree
        { UnitData.Type.Rider, TechData.Type.Riding },
        { UnitData.Type.Knight, TechData.Type.Chivalry },
        { UnitData.Type.Defender, TechData.Type.Shields },
        { UnitData.Type.Cloak, TechData.Type.Diplomacy },
        { UnitData.Type.Cloak_Boat, TechData.Type.Diplomacy },
        { UnitData.Type.Dagger, TechData.Type.Diplomacy },
        { UnitData.Type.Catapult, TechData.Type.Mathematics },
        { UnitData.Type.Archer, TechData.Type.Archery },
        { UnitData.Type.MindBender, TechData.Type.Philosophy },
        { UnitData.Type.Swordsman, TechData.Type.Smithery },
        // { UnitData.Type.Giant, TechData.Type.Basic }, // Not in Tech Tree
        // { UnitData.Type.Bunny, TechData.Type.Basic }, // Not in Tech Tree
        // { UnitData.Type.Boat, TechData.Type.Fishing }, // Maybe Raft?
        // { UnitData.Type.Scout, TechData.Type.Sailing }, // Maybe Boat Upgrade?
        // { UnitData.Type.Bombership, TechData.Type.Navigation },
        // { UnitData.Type.Battleship, TechData.Type.Fishing }, // Not used?
        // { UnitData.Type.Rammership, TechData.Type.Ramming },
    };


    private static bool HasUnitItem(UnitData.Type unitType)
    {
        if (!Archipelago.IsConnected) { return true; }
        if (Main.PlayerState == null) { return false; }

        if (!UnitToTechMapping.ContainsKey(unitType)) { return false; }

        TechData.Type techType = UnitToTechMapping.GetValueOrDefault(unitType, TechData.Type.Basic);
        bool isReceived = Archipelago.IsTechItemReceived(Main.PlayerState.tribe, techType);
        return isReceived;
    }

}