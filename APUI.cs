using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Text.Json;
using Cpp2IL.Core.Extensions;
using I2.Loc;
using Il2CppInterop.Runtime;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static PopupBase;
using PolyMod.Managers;
using PolyMod;
using System.Reflection;

namespace PolytopiaArchipelagoMW;

public static class APUI
{
    private static ManualLogSource logger = new("apmw: APUI");
    private static bool isConnectedToArchipelago = false;

    private const string HEADER_PREFIX = "<align=\"center\"><size=150%><b>";
    private const string HEADER_POSTFIX = "</b></size><align=\"left\">";
    private const int POPUP_WIDTH = 1400;
    private static UIRoundButton_UI2? archipelagoModButton = null;

    public static void Load(ManualLogSource logger)
    {
        Harmony.CreateAndPatchAll(typeof(APUI));

        APUI.logger = logger;
        logger.LogInfo("APUI loaded");
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(StartScreen_UI2), nameof(StartScreen_UI2.Init))]
    private static void StartScreen_UI2_Init_Prefix(StartScreen_UI2 __instance, RectTransform transform)
    {
        logger.LogInfo("StartScreen_UI2.Init called.");

        archipelagoModButton = UILibrary.NewRoundButton(transform).SetStyle(UIButtonBase_UI2.ButtonStyle.Suggested);
        archipelagoModButton.bg.sprite = Registry.GetSprite("ap-logo__.png");
        archipelagoModButton.OnClickedSignal.Add(DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(ShowPolyModHub));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartScreen_UI2), nameof(StartScreen_UI2.RunLayout))]
    private static void StartScreen_UI2_RunLayout(StartScreen_UI2 __instance, ScreenBase_UI2.ScreenSize screenSize)
    {
        if(archipelagoModButton == null)
        {
            logger.LogWarning("PolyMod Hub button is null when running layout!");
            return;
        }
        archipelagoModButton.iconContainer.gameObject.SetActive(false);
        archipelagoModButton.outline.gameObject.SetActive(false);
        archipelagoModButton.bg.color = Color.white;
        archipelagoModButton.Text = Localization.Get("apmw.hub");
		float num = 50f;
		archipelagoModButton.SetPosition(screenSize.safeRect.Left + (num * 2.5f), screenSize.safeRect.Top - num);
    }

    private static void ShowPolyModHub()
    {
        logger.LogInfo("ShowPolyModHub called.");
    }
}