using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance { get; private set; }

    private Canvas canvas;
    private GameObject panel;
    public bool isOpen = false;

    private Slider sfxSlider;
    private Slider throttleExpoSlider;
    private Slider camSmoothSlider;
    private Slider sensitivitySlider;
    private Slider fovSlider;

    void Awake()
    {
        Instance = this;
        CreateSettingsUI();
        panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel"))
        {
            isOpen = !isOpen;
            panel.SetActive(isOpen);
            Time.timeScale = isOpen ? 0f : 1f;
        }
    }

    public void ApplySettings()
    {
        if (sfxSlider != null)
            PlayerPrefs.SetFloat("sfx_volume", sfxSlider.value);

        if (throttleExpoSlider != null)
            PlayerPrefs.SetFloat("throttle_expo", throttleExpoSlider.value);

        if (camSmoothSlider != null)
            PlayerPrefs.SetFloat("cam_smooth", camSmoothSlider.value);

        if (sensitivitySlider != null)
            PlayerPrefs.SetFloat("sensitivity", sensitivitySlider.value);

        if (fovSlider != null)
            PlayerPrefs.SetFloat("fpv_fov", fovSlider.value);

        PlayerPrefs.Save();

        AudioController audio = FindFirstObjectByType<AudioController>();
        if (audio != null) audio.LoadVolumeSettings();

        DroneController drone = FindFirstObjectByType<DroneController>();
        if (drone != null)
        {
            drone.expo = PlayerPrefs.GetFloat("throttle_expo", 0.3f);
            drone.cameraSmoothTime = PlayerPrefs.GetFloat("cam_smooth", 0.04f);
            drone.rcRate = PlayerPrefs.GetFloat("sensitivity", 1f);
        }

        FPVController fpv = FindFirstObjectByType<FPVController>();
        if (fpv != null) fpv.SetFOV(PlayerPrefs.GetFloat("fpv_fov", 80f));
    }

    private void CreateSettingsUI()
    {
        GameObject canvasGO = new GameObject("SettingsCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        panel = new GameObject("SettingsPanel", typeof(RectTransform));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(400, 550);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        float yPos = -40f;

        AddTitle(panelRT, "SETTINGS", ref yPos);

        AddLabel(panelRT, "SFX Volume", ref yPos);
        sfxSlider = AddSlider(panelRT, ref yPos, PlayerPrefs.GetFloat("sfx_volume", 1f));

        AddLabel(panelRT, "Throttle Expo", ref yPos);
        throttleExpoSlider = AddSlider(panelRT, ref yPos, PlayerPrefs.GetFloat("throttle_expo", 0.3f));

        AddLabel(panelRT, "Camera Smooth", ref yPos);
        camSmoothSlider = AddSlider(panelRT, ref yPos, PlayerPrefs.GetFloat("cam_smooth", 0.04f));

        AddLabel(panelRT, "Sensitivity", ref yPos);
        sensitivitySlider = AddSlider(panelRT, ref yPos, PlayerPrefs.GetFloat("sensitivity", 1f));

        AddLabel(panelRT, "FPV FOV", ref yPos);
        fovSlider = AddSlider(panelRT, ref yPos, PlayerPrefs.GetFloat("fpv_fov", 80f));

        AddButton(panelRT, "APPLY", ref yPos, ApplySettings);
        AddButton(panelRT, "CLOSE", ref yPos, () => { isOpen = false; panel.SetActive(false); Time.timeScale = 1f; });
    }

    private void AddTitle(RectTransform parent, string text, ref float yPos)
    {
        TextMeshProUGUI tmp = CreateLabel(parent, "Title", text, 36, FontStyles.Bold, Color.white, ref yPos);
        tmp.rectTransform.sizeDelta = new Vector2(300, 50);
        yPos -= 60f;
    }

    private void AddLabel(RectTransform parent, string text, ref float yPos)
    {
        CreateLabel(parent, "Label_" + text, text, 22, FontStyles.Normal, new Color(0.8f, 0.8f, 0.8f), ref yPos);
        yPos -= 30f;
    }

    private TextMeshProUGUI CreateLabel(RectTransform parent, string name, string text, float size, FontStyles style, Color color, ref float yPos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(300, 30);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        return tmp;
    }

    private Slider AddSlider(RectTransform parent, ref float yPos, float defaultValue)
    {
        GameObject go = new GameObject("Slider", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(300, 25);

        Slider slider = go.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = defaultValue;
        slider.wholeNumbers = false;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(rt, false);
        RectTransform fillRT = fillArea.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0, 0);
        fillRT.anchorMax = new Vector2(1, 1);
        fillRT.sizeDelta = new Vector2(-10, 0);
        fillRT.anchoredPosition = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillRT, false);
        RectTransform fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0, 0);
        fRT.anchorMax = new Vector2(1, 1);
        fRT.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.6f, 0.2f);

        slider.fillRect = fRT;

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(rt, false);
        RectTransform haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = new Vector2(0, 0);
        haRT.anchorMax = new Vector2(1, 1);
        haRT.sizeDelta = new Vector2(-10, 0);

        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(haRT, false);
        RectTransform hRT = handle.GetComponent<RectTransform>();
        hRT.sizeDelta = new Vector2(20, 20);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.handleRect = hRT;
        slider.targetGraphic = handleImg;

        yPos -= 50f;
        return slider;
    }

    private void AddButton(RectTransform parent, string text, ref float yPos, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(text + "Button", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(200, 45);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.5f, 0.25f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(action);

        GameObject txt = new GameObject("Text", typeof(RectTransform));
        txt.transform.SetParent(rt, false);
        RectTransform txtRT = txt.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 26;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        yPos -= 60f;
    }
}
