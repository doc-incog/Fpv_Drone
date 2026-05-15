using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    private Canvas canvas;
    private GameObject panel;
    private float displayTime = 10f;
    private float timer;

    void Start()
    {
        if (PlayerPrefs.GetInt("tutorial_seen", 0) == 1)
        {
            enabled = false;
            return;
        }

        CreateTutorialUI();
        timer = displayTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            canvas.enabled = false;
            PlayerPrefs.SetInt("tutorial_seen", 1);
            PlayerPrefs.Save();
            enabled = false;
        }

        if (Input.anyKeyDown)
        {
            timer = 0;
        }
    }

    private void CreateTutorialUI()
    {
        GameObject canvasGO = new GameObject("TutorialCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        panel = new GameObject("TutorialPanel", typeof(RectTransform));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, -150);
        rt.sizeDelta = new Vector2(700, 300);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        AddLine("CONTROLS", 0, 120, 36, FontStyles.Bold);
        AddLine("Left Stick: Throttle (up/down) / Yaw (left/right)", 0, 80, 22, FontStyles.Normal);
        AddLine("Right Stick: Pitch (up/down) / Roll (left/right)", 0, 50, 22, FontStyles.Normal);
        AddLine("A Button: Respawn    |    Tab: Switch Drone", 0, 15, 20, FontStyles.Normal);
        AddLine("LB / Y: Toggle FPV Camera    |    RB / ESC: Pause Menu", 0, -15, 20, FontStyles.Normal);
        AddLine("L: Leaderboard    |    Fly through rings to race!", 0, -45, 20, FontStyles.Normal);
        AddLine("Press any key to dismiss", 0, -90, 18, FontStyles.Italic);
    }

    private void AddLine(string text, float x, float y, float size, FontStyles style)
    {
        GameObject go = new GameObject("Line", typeof(RectTransform));
        go.transform.SetParent(panel.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(650, 30);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }
}
