using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    private Canvas canvas;
    private GameObject mainGroup;
    private GameObject mapPanel;
    private GameObject dronePanel;

    private readonly Color gradientTop    = new Color(0.04f, 0.04f, 0.10f, 1f);
    private readonly Color gradientMid    = new Color(0.07f, 0.03f, 0.23f, 1f);
    private readonly Color gradientBot    = new Color(0.04f, 0.04f, 0.10f, 1f);
    private readonly Color gridColor     = new Color(0.31f, 0.16f, 0.78f, 0.18f);
    private readonly Color btnFace       = new Color(1f, 1f, 1f, 0.92f);
    private readonly Color btnHover      = new Color(1f, 1f, 1f, 1f);
    private readonly Color btnPress      = new Color(0.85f, 0.85f, 0.95f, 1f);
    private readonly Color btnTextMain   = new Color(0.18f, 0.10f, 0.43f, 1f);
    private readonly Color btnTextSub    = new Color(0.42f, 0.32f, 0.69f, 1f);

    private const float ButtonW = 320f;
    private const float ButtonH = 58f;
    private const float ButtonGap = 16f;

    void Awake()
    {
        CreateCanvas();
        CreateBackground();
        CreateGridOverlay();
        CreateMainGroup();
        CreateSubPanels();
    }

    void Start()
    {
        ShowMainMenu();
    }

    // ── Canvas ─────────────────────────────────────────────────────────
    private void CreateCanvas()
    {
        GameObject go = new GameObject("MainMenuCanvas", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        go.AddComponent<GraphicRaycaster>();
    }

    // ── Background gradient ────────────────────────────────────────────
    private void CreateBackground()
    {
        GameObject go = new GameObject("Background", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        RawImage img = go.AddComponent<RawImage>();
        img.texture = MakeGradientTexture();
        img.color = Color.white;
    }

    private Texture2D MakeGradientTexture()
    {
        int w = 4, h = 256;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < h; y++)
        {
            float t = y / (float)(h - 1);
            Color c;
            if (t < 0.5f)
                c = Color.Lerp(gradientTop, gradientMid, t * 2f);
            else
                c = Color.Lerp(gradientMid, gradientBot, (t - 0.5f) * 2f);
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, c);
        }
        tex.Apply();
        return tex;
    }

    // ── Grid overlay ───────────────────────────────────────────────────
    private void CreateGridOverlay()
    {
        GameObject go = new GameObject("GridOverlay", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        RawImage img = go.AddComponent<RawImage>();
        img.texture = MakeGridTexture();
        img.color = Color.white;
    }

    private Texture2D MakeGridTexture()
    {
        int w = 512, h = 512;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;

        Color clear = Color.clear;
        for (int y = 0; y < h; y++)
        {
            bool hLine = y % 60 == 0;
            for (int x = 0; x < w; x++)
            {
                bool vLine = x % 60 == 0;
                tex.SetPixel(x, y, (hLine || vLine) ? gridColor : clear);
            }
        }
        tex.Apply();
        return tex;
    }

    // ── Main group (title + 3 buttons) ────────────────────────────────
    private void CreateMainGroup()
    {
        mainGroup = new GameObject("MainGroup", typeof(RectTransform));
        mainGroup.transform.SetParent(canvas.transform, false);
        RectTransform rt = mainGroup.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 400);

        CreateTitle();
        CreateMainButton("Play", ShowMapSelection, 0);
        CreateMainButton("Options", ShowOptions, 1);
        CreateMainButton("Exit", ExitGame, 2);
    }

    private void CreateTitle()
    {
        GameObject go = new GameObject("Title", typeof(RectTransform));
        go.transform.SetParent(mainGroup.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -10);
        rt.sizeDelta = new Vector2(600, 60);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = "FPV Drone Training Simulator";
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.63f, 0.39f, 1f, 0.5f);
        shadow.effectDistance = new Vector2(0, 0);

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0.63f, 0.39f, 1f, 0.3f);
        outline.effectDistance = new Vector2(4, -4);
    }

    private void CreateMainButton(string label, UnityEngine.Events.UnityAction action, int index)
    {
        float totalH = 3 * ButtonH + 2 * ButtonGap;
        float startY = totalH * 0.5f - ButtonH * 0.5f;
        float yPos = startY - index * (ButtonH + ButtonGap);

        GameObject go = new GameObject(label, typeof(RectTransform));
        go.transform.SetParent(mainGroup.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(ButtonW, ButtonH);

        Image img = go.AddComponent<Image>();
        img.color = btnFace;
        ApplyRoundedSprite(img);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        ApplyColors(btn, btnFace);
        btn.onClick.AddListener(action);

        AddButtonScaleAnim(go);

        GameObject txt = new GameObject("Label", typeof(RectTransform));
        txt.transform.SetParent(go.transform, false);
        RectTransform txtRT = txt.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = btnTextMain;
    }

    // ── Button scale animation via EventTrigger ────────────────────────
    private void AddButtonScaleAnim(GameObject btn)
    {
        EventTrigger trigger = btn.AddComponent<EventTrigger>();

        AddEvent(trigger, EventTriggerType.PointerEnter, () =>
            LeanTweenScale(btn, 1.02f, 0.1f));
        AddEvent(trigger, EventTriggerType.PointerExit, () =>
            LeanTweenScale(btn, 1f, 0.1f));
        AddEvent(trigger, EventTriggerType.PointerDown, () =>
            LeanTweenScale(btn, 0.98f, 0.05f));
        AddEvent(trigger, EventTriggerType.PointerUp, () =>
            LeanTweenScale(btn, 1.02f, 0.05f));
    }

    private void AddEvent(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((_) => action());
        trigger.triggers.Add(entry);
    }

    private void LeanTweenScale(GameObject go, float target, float time)
    {
        StartCoroutine(ScaleOver(go.transform, target, time));
    }

    private IEnumerator ScaleOver(Transform t, float target, float time)
    {
        Vector3 from = t.localScale;
        Vector3 to = Vector3.one * target;
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(from, to, elapsed / time);
            yield return null;
        }
        t.localScale = to;
    }

    // ── Panel visibility ──────────────────────────────────────────────
    public void ShowMainMenu()
    {
        mainGroup.SetActive(true);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (dronePanel != null) dronePanel.SetActive(false);
    }

    public void ShowMapSelection()
    {
        mainGroup.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(true);
        if (dronePanel != null) dronePanel.SetActive(false);
    }

    public void ShowOptions()
    {
        mainGroup.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (dronePanel != null) dronePanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exiting game...");
    }

    // ── Sub-panel creation ────────────────────────────────────────────
    private void CreateSubPanels()
    {
        CreateMapPanel();
        CreateDronePanel();
    }

    private void CreateMapPanel()
    {
        mapPanel = MakeFullPanel("MapPanel");

        GameObject overlay = new GameObject("Overlay", typeof(RectTransform));
        overlay.transform.SetParent(mapPanel.transform, false);
        RectTransform oRT = overlay.GetComponent<RectTransform>();
        oRT.anchorMin = Vector2.zero;
        oRT.anchorMax = Vector2.one;
        oRT.sizeDelta = Vector2.zero;
        Image oImg = overlay.AddComponent<Image>();
        oImg.color = new Color(0, 0, 0, 0.6f);

        GameObject group = new GameObject("Group", typeof(RectTransform));
        group.transform.SetParent(mapPanel.transform, false);
        RectTransform gRT = group.GetComponent<RectTransform>();
        gRT.anchorMin = new Vector2(0.5f, 0.5f);
        gRT.anchorMax = new Vector2(0.5f, 0.5f);
        gRT.pivot = new Vector2(0.5f, 0.5f);
        gRT.anchoredPosition = Vector2.zero;
        gRT.sizeDelta = new Vector2(400, 400);

        float y = 200f;
        AddSubTitle(group, "SELECT MAP", ref y, 44);
        y -= 30f;
        AddSubButton(group, "CITY", "Easy \u00b7 Tutorial", ref y, () => LoadMap("city"));
        AddSubButton(group, "WAREHOUSE", "Freestyle", ref y, () => LoadMap("Warehouse"));
        AddSubButton(group, "FEST", "Expert", ref y, () => LoadMap("fest"));
        y -= 10f;
        AddBackButton(group, ref y, ShowMainMenu);
    }

    private void CreateDronePanel()
    {
        dronePanel = MakeFullPanel("DronePanel");

        GameObject overlay = new GameObject("Overlay", typeof(RectTransform));
        overlay.transform.SetParent(dronePanel.transform, false);
        RectTransform oRT = overlay.GetComponent<RectTransform>();
        oRT.anchorMin = Vector2.zero;
        oRT.anchorMax = Vector2.one;
        oRT.sizeDelta = Vector2.zero;
        Image oImg = overlay.AddComponent<Image>();
        oImg.color = new Color(0, 0, 0, 0.6f);

        GameObject group = new GameObject("Group", typeof(RectTransform));
        group.transform.SetParent(dronePanel.transform, false);
        RectTransform gRT = group.GetComponent<RectTransform>();
        gRT.anchorMin = new Vector2(0.5f, 0.5f);
        gRT.anchorMax = new Vector2(0.5f, 0.5f);
        gRT.pivot = new Vector2(0.5f, 0.5f);
        gRT.anchoredPosition = Vector2.zero;
        gRT.sizeDelta = new Vector2(400, 400);

        float y = 240f;
        AddSubTitle(group, "SELECT DRONE", ref y, 44);
        y -= 30f;

        string[] drones = { "PBR Racing Drone", "Quad", "UFO", "Toad", "BumbleBee" };
        for (int i = 0; i < drones.Length; i++)
        {
            int idx = i;
            AddSubButton(group, drones[i], "", ref y, () => SelectDrone(idx));
        }
        y -= 10f;
        AddBackButton(group, ref y, ShowMainMenu);
    }

    private GameObject MakeFullPanel(string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        return go;
    }

    private void LoadMap(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private void SelectDrone(int index)
    {
        PlayerPrefs.SetInt("selected_drone", index);
        PlayerPrefs.Save();
        Debug.Log("Selected drone: " + index);
    }

    // ── Sub-panel UI builders ─────────────────────────────────────────
    private void AddSubTitle(GameObject parent, string text, ref float yPos, int fontSize)
    {
        GameObject go = new GameObject("Title", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(600, 65);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        yPos -= 85f;
    }

    private void AddSubButton(GameObject parent, string title, string subtitle,
                               ref float yPos, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(title + "Btn", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(420, 65);

        Image img = go.AddComponent<Image>();
        img.color = btnFace;
        ApplyRoundedSprite(img);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        ApplyColors(btn, btnFace);
        btn.onClick.AddListener(action);

        AddButtonScaleAnim(go);

        AddTextChild(go, "Label", title, 26, btnTextMain,
                     new Vector2(0f, string.IsNullOrEmpty(subtitle) ? 0f : 10f));

        if (!string.IsNullOrEmpty(subtitle))
            AddTextChild(go, "Sub", subtitle, 15, btnTextSub, new Vector2(0f, -14f));

        yPos -= 80f;
    }

    private void AddBackButton(GameObject parent, ref float yPos,
                                UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject("BackBtn", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(220, 50);

        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.82f);
        ApplyRoundedSprite(img);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        ApplyColors(btn, new Color(1f, 1f, 1f, 0.82f));
        btn.onClick.AddListener(action);

        AddTextChild(go, "Label", "\u2190 BACK", 20, btnTextMain, Vector2.zero);

        yPos -= 65f;
    }

    private void AddTextChild(GameObject parent, string objName, string text,
                               int fontSize, Color color, Vector2 offset)
    {
        GameObject go = new GameObject(objName, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = offset;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
    }

    // ── Shared helpers ────────────────────────────────────────────────
    private void ApplyColors(Button btn, Color normal)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = normal;
        cb.highlightedColor = btnHover;
        cb.pressedColor = btnPress;
        cb.selectedColor = normal;
        btn.colors = cb;
    }

    private void ApplyRoundedSprite(Image img)
    {
        const int W = 128, H = 64, R = 20;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[W * H];

        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                pixels[y * W + x] = InsideRoundedRect(x, y, W, H, R)
                                     ? Color.white : Color.clear;

        tex.SetPixels(pixels);
        tex.Apply();

        img.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, W, H),
            new Vector2(0.5f, 0.5f),
            100f, 0,
            SpriteMeshType.FullRect,
            new Vector4(R, R, R, R)
        );
        img.type = Image.Type.Sliced;
    }

    private bool InsideRoundedRect(int px, int py, int w, int h, int r)
    {
        int cx = Mathf.Clamp(px, r, w - r - 1);
        int cy = Mathf.Clamp(py, r, h - r - 1);
        int dx = px - cx, dy = py - cy;
        return dx * dx + dy * dy <= r * r;
    }
}
