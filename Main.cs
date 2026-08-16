using BepInEx.Logging;

namespace PolyScriptTemplate;
public static class Main
{
    public static void Load(ManualLogSource logger)
    {
        logger.LogMessage("Here we go!");
    }
}
