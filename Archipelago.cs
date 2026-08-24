using BepInEx.Logging;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;

namespace PolytopiaArchipelagoMW;
public static class Archipelago
{
    private static ManualLogSource logger = Main.logger;
    public static bool isConnectedToArchipelago = false;
    public static LoginSuccessful? slotData;
    private static readonly string game_name = "The Battle of Polytopia";

    // public static void Load(ManualLogSource logger)
    // {
    //     Archipelago.logger = logger;
    //     logger.LogInfo("Archipelago.MultiClient.Net Loaded");
    // }

    public async static Task<bool> ConnectToRoom(string URL, int port, string slot_name, string password)
    {
        LoginResult result;
        var session = ArchipelagoSessionFactory.CreateSession(URL, port);

        try {
            result = session.TryConnectAndLogin(game: game_name, name: slot_name, 
                             itemsHandlingFlags: ItemsHandlingFlags.AllItems, password: password);
        } catch (Exception e) {
            result = new LoginFailure(e.GetBaseException().Message);
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
        isConnectedToArchipelago = true;
        logger.LogInfo($"Connected to Archipelago Room: {URL}:{port} as {slot_name}");
        return true;
    }
}