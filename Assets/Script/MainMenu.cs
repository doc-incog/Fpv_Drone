using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private Canvas canvas;
    private GameObject mapPanel;
    private GameObject optionsPanel;

    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("MainMenu: No Canvas found in children");
            return;
        }

        WireExistingButtons();
        CreateSubPanels();
        ShowMainMenu();
    }

    private void WireExistingButtons()
    {
        WireButton("Play", ShowMapSelection);
        WireButton("Options", ShowOptions);
        WireButton("Exit", ExitGame);
    }

    private void WireButton(string name, UnityEngine.Events.UnityAction action)
    {
        Transform t = canvas.transform.Find(name);
        if (t == null) { Debug.LogWarning("MainMenu: Button \"" + name + "\" not found under Canvas"); return; }
        Button btn = t.GetComponent<Button>();
        if (btn == null) { Debug.LogWarning("MainMenu: \"" + name + "\" has no Button component"); return; }
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    public void ShowMainMenu()
    {
        SetMainButtonsActive(true);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void ShowMapSelection()
    {
        SetMainButtonsActive(false);
        if (mapPanel != null) mapPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        SetMainButtonsActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exiting game...");
    }

    private void SetMainButtonsActive(bool active)
    {
        SetActive("Play", active);
        SetActive("Options", active);
        SetActive("Exit", active);
    }

    private void SetActive(string name, bool active)
    {
        Transform t = canvas.transform.Find(name);
        if (t != null) t.gameObject.SetActive(active);
    }

    private void CreateSubPanels()
    {
        CreateMapPanel();
        CreateOptionsPanel();
    }

    private void CreateMapPanel()
    {
        mapPanel = new GameObject("MapPanel", typeof(RectTransform));
        mapPanel.transform.SetParent(canvas.transform, false);
        RectTransform rt = mapPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        float y = 200f;
        AddTitle(mapPanel, "SELECT MAP", ref y, 44);
        y -= 40f;
        AddMapButton(mapPanel, "CITY", "Easy - Tutorial", ref y, () => LoadMap("city"));
        AddMapButton(mapPanel, "WAREHOUSE", "Freestyle", ref y, () => LoadMap("Warehouse"));
        AddMapButton(mapPanel, "FEST", "Expert", ref y, () => LoadMap("fest"));
        y -= 20f;
        AddSmallButton(mapPanel, "BACK", ref y, ShowMainMenu);
    }

    private void CreateOptionsPanel()
    {
        optionsPanel = new GameObject("OptionsPanel", typeof(RectTransform));
        optionsPanel.transform.SetParent(canvas.transform, false);
        RectTransform rt = optionsPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        float y = 200f;
        AddTitle(optionsPanel, "SELECT DRONE", ref y, 44);
        y -= 40f;

        string[] droneNames = { "PBR Racing Drone", "Quad", "UFO", "Toad", "BumbleBee" };
        for (int i = 0; i < droneNames.Length; i++)
        {
            int idx = i;
            AddMapButton(optionsPanel, droneNames[i], "", ref y, () => SelectDrone(idx));
        }
        y -= 20f;
        AddSmallButton(optionsPanel, "BACK", ref y, ShowMainMenu);
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
        Debug.Log("Selected drone index: " + index);
    }

    private void AddTitle(GameObject parent, string text, ref float yPos, int fontSize)
    {
        GameObject go = new GameObject("Title", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(600, 60);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    private void AddMapButton(GameObject parent, string title, string subtitle, ref float yPos, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(title + "Button", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(400, 60);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.3f, 0.15f);

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
        tmp.text = string.IsNullOrEmpty(subtitle) ? title : title + "\n<size=18>" + subtitle + "</size>";
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        yPos -= 75f;
    }

    private void AddSmallButton(GameObject parent, string text, ref float yPos, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(text + "Button", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(200, 45);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.2f, 0.2f);

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
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        yPos -= 60f;
    }
}
