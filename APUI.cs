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
    private static UIRoundButton_UI2? archipelagoModButton = null;
    private static string APAddress = "localhost:38281";
    private static string APSlotName = "Player_Polytopia";
    private static string? APPassword = null;


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
        archipelagoModButton.bg.sprite = Registry.GetSprite("ap-logo");
        archipelagoModButton.OnClickedSignal.Add(DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(ShowArchipelagoHub));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartScreen_UI2), nameof(StartScreen_UI2.RunLayout))]
    private static void StartScreen_UI2_RunLayout(StartScreen_UI2 __instance, ScreenBase_UI2.ScreenSize screenSize)
    {
        if(archipelagoModButton == null)
        {
            logger.LogWarning("Archipelago Hub button is null when running layout!");
            return;
        }
        archipelagoModButton.iconContainer.gameObject.SetActive(false);
        archipelagoModButton.outline.gameObject.SetActive(false);
        archipelagoModButton.bg.color = Color.white;
        archipelagoModButton.Text = Localization.Get("apmw.hub.btn");
		float num = 50f;
		archipelagoModButton.SetPosition(screenSize.safeRect.Left + (num * 2.5f), screenSize.safeRect.Top - num);
    }

    internal static void ShowArchipelagoHub()
    {
        BasicPopupLegacy popup = GetBasicPopupLegacy();
        popup.Header = Localization.Get("apmw.hub");

        List<PopupButtonData> popupButtons = new() {
            new("buttons.back", closesPopup: true),
        };

        if (!Archipelago.IsConnected) {
            popup.Description = Localization.Get("apmw.disconnected");
            PopupButtonData btn = new(
                "apmw.connect.btn",
                callback: DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(ConnectToArchipelago)
            );

            popupButtons.Add(btn);

            AddInputToPopup(popup, baseValue: APAddress, placeholderText: bold(Localization.Get("apmw.fields.address")), 
                            onSubmit: input => { APAddress = input; }, 
                            onValueChanged: input => { APAddress = input; });
            AddInputToPopup(popup, baseValue: APSlotName, placeholderText: bold(Localization.Get("apmw.fields.slotname")), 
                            onSubmit: input => { APSlotName = input; }, 
                            onValueChanged: input => { APSlotName = input; });
            AddInputToPopup(popup, baseValue: "", placeholderText: bold(Localization.Get("apmw.fields.password")), 
                            onSubmit: input => { APPassword = input; }, 
                            onValueChanged: input => { APPassword = input; });

        } else {
            popup.Description = Localization.Get("apmw.connected", new Il2CppSystem.Object[] {APAddress, APSlotName});
            popupButtons.Add(new(
                "apmw.disconnect.btn",
                callback: DelegateSupport.ConvertDelegate<Il2CppSystem.Action>(DisconnectFromArchipelago),
                customColorStates: new UIButtonBase.ColorStates() {
                    defaultColor = UIConstants.COLOR_DELETE,
                }
            ));
        }
        
        
        async void ConnectToArchipelago()
        {
            logger.LogInfo("ConnectToArchipelago called.");
            bool res = await Archipelago.ConnectToRoom(APAddress, APSlotName, APPassword);
            if (res) {
                logger.LogInfo(Archipelago.Connection?.ToString());
            }
        }

        void DisconnectFromArchipelago()
        {
            logger.LogInfo("DisconnectFromArchipelago called.");
            Archipelago.Disconnect();
        }

        popup.buttonData = popupButtons.ToArray();
        popup.Show();
    }

    // copied from https://github.com/PolyModdingTeam/PolyMod/blob/main/src/Managers/Visual.cs
    // why is there so many internal methods T_T
    internal static BasicPopupLegacy GetBasicPopupLegacy()
	{
		WhatsNewPopup whatsNewPopup = PopupManager.GetWhatsNewPopup();
		BasicPopupLegacy original = PopupManager.instance.popupPrefabs[29].Cast<BasicPopupLegacy>();
		BasicPopupLegacy basicPopupLegacy = UnityEngine.Object.Instantiate(original, PopupManager.instance.transform);
		basicPopupLegacy.buttonContainer = GameObject.Instantiate(whatsNewPopup.buttonContainer, basicPopupLegacy.transform);
		basicPopupLegacy.popupId = "basicPopupLegacy";
		basicPopupLegacy.popupManager = PopupManager.instance;
		basicPopupLegacy.Init();
		basicPopupLegacy.identifier = null;
		basicPopupLegacy.rectTransform.SetAsLastSibling();
		return basicPopupLegacy;
	}

    // Thanks to Fa (pingvin) for letting me to copy his code
    // https://github.com/johnklipi/PolytopiaMapMaker/blob/main/src/UI/Popup/CustomInput.cs
    public static void AddInputToPopup(
        BasicPopupLegacy popup, 
        string baseValue = "", 
        string placeholderText = "Type here...", 
        Action<string>? onSubmit = null, 
        Action<string>? onValueChanged = null)
    {
        EventSystem.current.sendNavigationEvents = false;
        InputManager.DisableInput(InputManager.InputType.Camera | InputManager.InputType.Map | InputManager.InputType.Input);
        Transform parent = popup.content != null ? popup.content.transform : popup.transform;

        GameObject go = new GameObject("InputBox");
        go.transform.SetParent(parent, false);


        // Body
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 50);
        rt.anchoredPosition = new Vector2(0, 0);

        var layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.minWidth = 200;
        layoutElement.minHeight = 20;
        layoutElement.preferredWidth = 400;
        layoutElement.preferredHeight = 50;

        // Background
        var bg = go.AddComponent<Image>();
        bg.color = Color.white;

        // Text
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 40;
        text.color = Color.black;

        // Text layout
        RectTransform txtRT = textGo.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(10, 10);
        txtRT.offsetMax = new Vector2(-10, -10);

        // Textarea
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(go.transform, false);

        RectTransform textAreaRT = textArea.AddComponent<RectTransform>();
        textAreaRT.anchorMin = Vector2.zero;
        textAreaRT.anchorMax = Vector2.one;
        textAreaRT.offsetMin = new Vector2(10, 10);
        textAreaRT.offsetMax = new Vector2(-10, -10);

        textArea.AddComponent<RectMask2D>();

        // Placeholder

        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(textArea.transform, false);

        var placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholder.text = placeholderText;
        placeholder.fontSize = 36;
        placeholder.color = new Color(0.6f, 0.6f, 0.6f);

        RectTransform placeholderRT = placeholderGO.GetComponent<RectTransform>();
        placeholderRT.anchorMin = Vector2.zero;
        placeholderRT.anchorMax = Vector2.one;
        placeholderRT.offsetMin = Vector2.zero;
        placeholderRT.offsetMax = Vector2.zero;


        // TMP InputField
        var input = go.AddComponent<TMP_InputField>();
        input.textViewport = textAreaRT;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.interactable = true;
        input.text = baseValue;
        popup.IsUnskippable = true;
        if (onSubmit != null)
        {
            input.onSubmit.RemoveAllListeners();
            input.onSubmit.AddListener(onSubmit);
        }
        if (onValueChanged != null)
        {
            input.onValueChanged.RemoveAllListeners();
            input.onValueChanged.AddListener(onValueChanged);
        }

        // popup.Show();

        // UINavigationManager.Select(input);
        // popup.currentSelectable = input;
    }

    internal static string bold(string text)
    {
        return $"<b>{text}</b>";
    }
}