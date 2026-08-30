using BepInEx.Logging;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;

namespace PolytopiaArchipelagoMW;
public static class Archipelago
{
    private static readonly ManualLogSource logger = Main.logger;
    private static readonly string game_name = "The Battle of Polytopia";
    public static bool isConnected = false;
    public static ArchipelagoSession? session;
    public static LoginSuccessful? slotData;
    public static string[] PlayableTribes { get; set; } = Array.Empty<string>();
    public static int UniqueTribesWins { get; set; } = -1;


    public async static Task<bool> ConnectToRoom(string URL, int port, string slot_name, string? password)
    {
        LoginResult? result = null;
        session = ArchipelagoSessionFactory.CreateSession(URL, port);
        session.Socket.ErrorReceived += OnErrorReceived;
        session.MessageLog.OnMessageReceived += OnMessageReceived;
        session.Items.ItemReceived += OnItemReceived;

        try {
            result = session.TryConnectAndLogin(game: game_name, name: slot_name, 
                             itemsHandlingFlags: ItemsHandlingFlags.AllItems, password: password, requestSlotData: true);
        } catch (Exception e) {
            if (result is null) {
                result = new LoginFailure(e.GetBaseException().Message);
            } else {
                logger.LogDebug($"[IGNORE] Archipelago connection error: {e.Message}");
            }
        }

        if (!result.Successful) {
            LoginFailure failure = (LoginFailure)result;
            string errorMessage = $"Failed to Connect to {URL}:{port} as {slot_name}:";
            foreach (string error in failure.Errors)
            {
                errorMessage += $"\n    {error}";
            }
            foreach (ConnectionRefusedError error in failure.ErrorCodes)
            {
                errorMessage += $"\n    {error}";
            }

            logger.LogError(errorMessage);
            return false;
        }

        slotData = (LoginSuccessful)result;
        isConnected = true;
        logger.LogInfo($"Connected to Archipelago Room: {URL}:{port} as {slot_name}");

        PlayableTribes = ((JArray)slotData.SlotData["playable_tribes"]).Select(t => t.ToString()).ToArray();
        logger.LogInfo($"Slot Data - Playable Tribes: {string.Join(", ", PlayableTribes)}");

        UniqueTribesWins = (int)(long)slotData.SlotData["unique_tribes_wins"];
        logger.LogInfo($"Slot Data - Unique Tribes Wins: {UniqueTribesWins}");
        return true;
    }

    private static void OnItemReceived(ReceivedItemsHelper helper)
    {
        ItemInfo info = helper.PeekItem();
        string itemName = info.ItemDisplayName ?? info.ItemName ?? $"Item ID: {info.ItemId}";
        string locationName = info.LocationDisplayName ?? info.LocationName ?? $"Location ID: {info.LocationId}";
        logger.LogMessage($"AP Item: Received {itemName} from {info.Player} at {locationName}");

        helper.DequeueItem();
    }

    internal static void OnMessageReceived(LogMessage message)
    {
        logger.LogMessage($"AP Message: {message}");
    }

    internal static void OnErrorReceived(Exception e, string message)
    {
        logger.LogError($"AP Connection Error: {message}\n{e}");
    }

}