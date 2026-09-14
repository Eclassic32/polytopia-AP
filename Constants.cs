using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using PolytopiaBackendBase.Common;

public class Constants
{
    public static readonly string game_name = "The Battle of Polytopia";
    public static readonly TribeType[] APTribeOrder = new TribeType[] {
        TribeType.Xinxi,
        TribeType.Imperius,
        TribeType.Bardur,
        TribeType.Oumaji,
        TribeType.Kickoo,
        TribeType.Hoodrick,
        TribeType.Luxidoor,
        TribeType.Vengir,
        TribeType.Zebasi,
        TribeType.Aimo,
        TribeType.Quetzali,
        TribeType.Yadakk,
        TribeType.Aquarion,
        TribeType.Elyrion,
        TribeType.Polaris,
        TribeType.Cymanti,
    }; 
    
    public static long TribeSpecificLocationID(TribeType tribe, int locationID)
    {
        int APTribeIdx = Array.IndexOf(APTribeOrder, tribe) + 1;
        return (APTribeIdx * 1000) + locationID;
    }

    public class SlotDataClass 
    {
        public string[] PlayableTribes = Array.Empty<string>();
        public int UniqueTribesWins { get; set; } = -1;
        public int ScoreToVictory { get; set; } = -1;
        public bool SendScoreChecksImmediately {get; set;} = true;
        public TechnologyOption TechnologyLocations { get; set; } = TechnologyOption.off;
        public TechnologyOption TechnologyItems { get; set; } = TechnologyOption.off;

        public TribeType[] GetPlayableTribes()
        {
            return PlayableTribes.Select(t => (TribeType)Enum.Parse(typeof(TribeType), t)).ToArray();
        }

        public void SetSlotData(Dictionary<string, object> slotData, ManualLogSource logger)
        {
            PlayableTribes = ((JArray)slotData["playable_tribes"]).Select(t => t.ToString()).ToArray();
            logger.LogInfo($"Slot Data - Playable Tribes: {string.Join(", ", PlayableTribes)}");

            UniqueTribesWins = (int)(long)slotData["unique_tribes_wins"];
            logger.LogInfo($"Slot Data - Unique Tribes Wins: {UniqueTribesWins}");

            ScoreToVictory = (int)(long)slotData["score_to_victory"];
            logger.LogInfo($"Slot Data - Score to Victory: {ScoreToVictory}");

            SendScoreChecksImmediately = (long)slotData["send_score_checks_immediately"] == 1;
            logger.LogInfo($"Slot Data - Send Score Checks Immediately: {SendScoreChecksImmediately}");

            TechnologyLocations = (TechnologyOption)(long)slotData["technology_locations"];
            logger.LogInfo($"Slot Data - Technology Locations: {TechnologyLocations}");

            TechnologyItems = (TechnologyOption)(long)slotData["technology_items"];
            logger.LogInfo($"Slot Data - Technology Items: {TechnologyItems}");
        }

        public override string ToString()
        {
            return $"Playable Tribes: {string.Join(", ", PlayableTribes)}\n" +
                $"Unique Tribes Wins: {UniqueTribesWins}\n" +
                $"Score to Victory: {ScoreToVictory}\n" +
                $"Send Score Checks Immediately: {SendScoreChecksImmediately}\n" +
                $"Technology Locations: {TechnologyLocations}\n" +
                $"Technology Items: {TechnologyItems}";
        }
    }

    public static readonly int TECH_OFFSET = 500;

    public enum TechnologyOption
    {
        off = 0,
        on = 1,
        by_tribe = 2,
        by_action = 3,
        by_tribe_and_action = 4
    }
}