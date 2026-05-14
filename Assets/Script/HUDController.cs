using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("HUD Text References")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI altText;
    public TextMeshProUGUI batteryText;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI throttleText;
    public TextMeshProUGUI gateText;
    public TextMeshProUGUI windText;

    [Header("Settings")]
    public Color lowBatteryColor = Color.red;
    public Color normalColor = Color.white;
    public bool flashLowBattery = true;
    public float flashRate = 0.5f;

    private Canvas canvas;
    private float lastFlashTime;
    private bool batteryVisible = true;
    private DroneController drone;
    private RaceManager race;
    private BatteryController battery;
    private WindController wind;

    void Awake()
    {
        Instance = this;
        CreateCanvas();
    }

    void Start()
    {
        race = FindFirstObjectByType<RaceManager>();
        wind = FindFirstObjectByType<WindController>();
    }

    void Update()
    {
        drone = FindFirstObjectByType<DroneController>();
        battery = FindFirstObjectByType<BatteryController>();
        if (drone == null) return;

        Rigidbody rb = drone.GetComponent<Rigidbody>();
        if (rb == null) return;

        float speed = rb.linearVelocity.magnitude * 3.6f;
        float altitude = drone.transform.position.y;

        if (speedText != null)
            speedText.text = $"SPD: {speed:F0} km/h";

        if (altText != null)
            altText.text = $"ALT: {altitude:F1} m";

        if (battery != null && batteryText != null)
        {
            float pct = battery.GetPercentage();
            batteryText.text = $"BATT: {pct:F0}%";
            bool low = pct <= 20f;

            if (flashLowBattery && low)
            {
                if (Time.time - lastFlashTime > flashRate)
                {
                    batteryVisible = !batteryVisible;
                    lastFlashTime = Time.time;
                }
                batteryText.color = batteryVisible ? lowBatteryColor : normalColor;
            }
            else
            {
                batteryText.color = low ? lowBatteryColor : normalColor;
                batteryVisible = true;
            }
        }

        if (race != null)
        {
            if (lapText != null)
                lapText.text = $"LAP {race.currentLap}/{race.totalLaps}";

            if (timeText != null)
                timeText.text = $"TIME: {race.currentLapTime:F1}s";

            if (gateText != null)
                gateText.text = $"GATE: {race.currentGateIndex}/{race.totalGates}";
        }

        if (throttleText != null)
        {
            float throttlePct = (drone.throttleInput + 1f) * 50f;
            throttleText.text = $"THR: {throttlePct:F0}%";
        }

        if (wind != null && windText != null)
        {
            float windStr = wind.currentWindStrength;
            Vector3 windDir = wind.currentWindDirection;
            string dirArrow = GetWindArrow(windDir);
            windText.text = windStr > 0.1f ? $"WIND {dirArrow} {windStr:F1} m/s" : "WIND calm";
        }
    }

    private string GetWindArrow(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        if (angle < 22.5f) return "\u2191";
        if (angle < 67.5f) return "\u2197";
        if (angle < 112.5f) return "\u2192";
        if (angle < 157.5f) return "\u2198";
        if (angle < 202.5f) return "\u2193";
        if (angle < 247.5f) return "\u2199";
        if (angle < 292.5f) return "\u2190";
        if (angle < 337.5f) return "\u2196";
        return "\u2191";
    }

    private void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("HUD_Canvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        FontStyles bold = FontStyles.Bold;

        speedText = CreateText("SpeedText", "SPD: 0 km/h", new Vector2(20, -20), new Vector2(300, 40), TextAlignmentOptions.TopLeft, 28, bold);
        altText = CreateText("AltText", "ALT: 0.0 m", new Vector2(20, -55), new Vector2(300, 30), TextAlignmentOptions.TopLeft, 22, bold);
        batteryText = CreateText("BatteryText", "BATT: 100%", new Vector2(20, -85), new Vector2(300, 30), TextAlignmentOptions.TopLeft, 22, bold);

        lapText = CreateText("LapText", "LAP 1/3", new Vector2(-20, -20), new Vector2(300, 40), TextAlignmentOptions.TopRight, 28, bold);
        timeText = CreateText("TimeText", "TIME: 0.0s", new Vector2(-20, -55), new Vector2(300, 30), TextAlignmentOptions.TopRight, 22, bold);
        gateText = CreateText("GateText", "GATE: 0/0", new Vector2(-20, -85), new Vector2(300, 30), TextAlignmentOptions.TopRight, 22, bold);

        throttleText = CreateText("ThrottleText", "THR: 0%", new Vector2(0, -20), new Vector2(300, 30), TextAlignmentOptions.Bottom, 24, bold);

        windText = CreateText("WindText", "WIND calm", new Vector2(-20, -20), new Vector2(250, 25), TextAlignmentOptions.BottomRight, 18, FontStyles.Normal);
    }

    private TextMeshProUGUI CreateText(string name, string defaultText, Vector2 anchoredPos, Vector2 sizeDelta, TextAlignmentOptions align, int fontSize, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        if (align == TextAlignmentOptions.TopLeft)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
        }
        else if (align == TextAlignmentOptions.TopRight)
        {
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
        }
        else if (align == TextAlignmentOptions.BottomRight)
        {
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
        }
        else if (align == TextAlignmentOptions.Bottom)
        {
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
        }

        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color = normalColor;
        tmp.outlineWidth = 0.15f;
        tmp.outlineColor = new Color32(0, 0, 0, 180);

        return tmp;
    }
}
