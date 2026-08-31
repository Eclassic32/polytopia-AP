using BepInEx.Logging;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;
using PolytopiaBackendBase.Common;

namespace PolytopiaArchipelagoMW;
public static class Archipelago
{
    private static readonly ManualLogSource logger = Main.logger;
    private static readonly string game_name = "The Battle of Polytopia";
    public static bool IsConnected {get; private set;} = false;
    public static ArchipelagoSession? Session {get; private set;}
    public static LoginSuccessful? SlotData {get; private set;}
    private static string[] PlayableTribes = Array.Empty<string>();
    public static int UniqueTribesWins { get; private set; } = -1;


    public async static Task<bool> ConnectToRoom(string URL, int port, string slot_name, string? password)
    {
        LoginResult? result = null;
        Session = ArchipelagoSessionFactory.CreateSession(URL, port);
        Session.Socket.ErrorReceived += OnErrorReceived;
        Session.MessageLog.OnMessageReceived += OnMessageReceived;
        Session.Items.ItemReceived += OnItemReceived;

        try {
            result = Session.TryConnectAndLogin(game: game_name, name: slot_name, 
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

        SlotData = (LoginSuccessful)result;
        IsConnected = true;
        logger.LogInfo($"Connected to Archipelago Room: {URL}:{port} as {slot_name}");

        PlayableTribes = ((JArray)SlotData.SlotData["playable_tribes"]).Select(t => t.ToString()).ToArray();
        logger.LogInfo($"Slot Data - Playable Tribes: {string.Join(", ", PlayableTribes)}");

        UniqueTribesWins = (int)(long)SlotData.SlotData["unique_tribes_wins"];
        logger.LogInfo($"Slot Data - Unique Tribes Wins: {UniqueTribesWins}");
        return true;
    }

    public static TribeType[] GetPlayableTribes()
    {
        return PlayableTribes.Select(t => (TribeType)Enum.Parse(typeof(TribeType), t)).ToArray();
    }

    public static TribeType[] GetEnabledTribes()
    {
        TribeType[] result = Array.Empty<TribeType>();
        if (!IsConnected || Session is null) { return result; }
        var allItems = Session.Items.AllItemsReceived;
        foreach (var item in allItems)
        {
            if (item.ItemName.StartsWith("Tribe Unlock - "))
            {
                string tribeName = item.ItemName["Tribe Unlock - ".Length..];
                if (Enum.TryParse(tribeName, out TribeType tribeType))
                {
                    result = result.Append(tribeType).ToArray();
                }
            }
        }
        return result;
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