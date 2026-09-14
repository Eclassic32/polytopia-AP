using BepInEx.Logging;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json.Linq;
using PolytopiaBackendBase.Common;
using Polytopia.Data;
using static Constants;


namespace PolytopiaArchipelagoMW;
public static class Archipelago
{
    private static readonly ManualLogSource logger = Main.logger;
    
    public static bool IsConnected {get; private set;} = false;
    public static ArchipelagoSession? Session {get; private set;}
    public static LoginSuccessful? Connection {get; private set;}
    public static SlotDataClass SlotData {get; private set;} = new();    
    private static Dictionary<TribeType, int> TribeMaxScores = new();
    
    public async static Task<bool> ConnectToRoom(string address, string slot_name, string? password)
    {
        LoginResult? result = null;
        Session = ArchipelagoSessionFactory.CreateSession(address);
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
            string errorMessage = $"Failed to Connect to {address} as {slot_name}:";
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

        Connection = (LoginSuccessful)result;
        Session.SetClientState(ArchipelagoClientState.ClientConnected);
        IsConnected = true;
        logger.LogInfo($"Connected to Archipelago Room: {address} as {slot_name}");

        Dictionary<string, object> slotData = Connection.SlotData;
        SlotData.SetSlotData(slotData, logger);
        SendLocations.CheckIfGoaled();

        return true;
    }

    public async static void Disconnect()
    {
        if (!IsConnected || Session is null) { return; }

        await Session.Socket.DisconnectAsync();
        Connection = null;
        SlotData = new SlotDataClass();
        TribeMaxScores = new();
        Session = null;
        IsConnected = false;
        logger.LogInfo("Disconnected from Archipelago");
    }

    public static TribeType[] GetReceivedTribes()
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

    public static TribeType[] GetVictoriousTribes()
    {
        TribeType[] result = Array.Empty<TribeType>();
        if (!IsConnected || Session is null) { return result; }

        var checkedLocations = Session.Locations.AllLocationsChecked;
        var playableTribes = SlotData.GetPlayableTribes();
        foreach (TribeType tribe in playableTribes)
        {
            int tribeIndex = Array.IndexOf(APTribeOrder, tribe) + 1;
            long victoryLocationID = TribeSpecificLocationID(tribe, 0);
            if (checkedLocations.Contains(victoryLocationID))
            {
                result = result.Append(tribe).ToArray();
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

    public static void SetClientState(ArchipelagoClientState state)
    {
        if (!IsConnected || Session is null) { return; }
        Session.SetClientState(state);
    }

    
    public class SendLocations
    {
        public static void Technology(TribeType tribe, TechData.Type techType)
        {
            if (!IsConnected || Session is null) { return; }
            long[] locationIDs = new long[2]; 
            locationIDs[0] = TribeSpecificLocationID(0, (int)techType + TECH_OFFSET);
            locationIDs[1] = TribeSpecificLocationID(tribe, (int)techType + TECH_OFFSET);
            logger.LogInfo($"Sending Technology Location Checks for {tribe} [{Array.IndexOf(APTribeOrder, tribe)}]" +
                            $" - Technology - {techType} ({(int)techType})\nLocation: {string.Join(", ", locationIDs)}");
            Session.Locations.CompleteLocationChecks(locationIDs);
        }

        public static void Score(TribeType tribe, int score_K, bool sendVictory = false)
        {
            if (!IsConnected || Session is null) { return; }

            if (TribeMaxScores.ContainsKey(tribe) && !sendVictory)
            {
                int maxScore = TribeMaxScores[tribe];
                if (score_K <= maxScore) { return; } 
            }

            long[] locationIDs = new long[score_K];
            for (int i = 0; i < locationIDs.Length; i++)
            {
                locationIDs[i] = TribeSpecificLocationID(tribe, i+1);
            }

            logger.LogInfo($"Sending Score Location Checks for {tribe} [{Array.IndexOf(APTribeOrder, tribe)}]" +
                            $" - Score: {score_K}K\nLocations: {string.Join(", ", locationIDs)}");
            Session.Locations.CompleteLocationChecks(locationIDs);
            TribeMaxScores[tribe] = score_K;

            if (sendVictory && SlotData.ScoreToVictory < score_K)
            {
                long victoryLocationID = TribeSpecificLocationID(tribe, 0);
                logger.LogInfo($"Sending Victory Location Check ({victoryLocationID}) " + 
                                $"for {tribe} [{Array.IndexOf(APTribeOrder, tribe)}]");
                Session.Locations.CompleteLocationChecks(new long[] { victoryLocationID });
                CheckIfGoaled();
            }
        }

        public static void CheckIfGoaled()
        {
            if (!IsConnected || Session is null) { return; }
            if (GetVictoriousTribes().Length < SlotData.UniqueTribesWins) { return; }

            Session.SetClientState(ArchipelagoClientState.ClientGoal);
            Session.SetGoalAchieved();
        }
    }
}

