using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private Canvas canvas;
    private GameObject panel;
    private TextMeshProUGUI entriesText;
    public bool isOpen = false;

    [Header("Settings")]
    public int maxEntries = 5;

    void Awake()
    {
        Instance = this;
        CreateLeaderboardUI();
        panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            isOpen = !isOpen;
            panel.SetActive(isOpen);
            if (isOpen) RefreshDisplay();
        }
    }

    public void AddTime(float time, int lap)
    {
        List<LeaderboardEntry> entries = LoadEntries();
        entries.Add(new LeaderboardEntry { time = time, lap = lap, date = DateTime.Now.ToString("MM/dd") });
        entries.Sort((a, b) => a.time.CompareTo(b.time));

        while (entries.Count > maxEntries)
            entries.RemoveAt(entries.Count - 1);

        SaveEntries(entries);
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (entriesText == null) return;

        List<LeaderboardEntry> entries = LoadEntries();
        string text = "<b>LEADERBOARD</b>\n\n";

        if (entries.Count == 0)
        {
            text += "No times yet";
        }
        else
        {
            for (int i = 0; i < entries.Count; i++)
            {
                text += $"#{i + 1}:  {entries[i].time:F2}s  Lap {entries[i].lap}  ({entries[i].date})\n";
            }
        }

        entriesText.text = text;
    }

    private List<LeaderboardEntry> LoadEntries()
    {
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        for (int i = 0; i < maxEntries; i++)
        {
            string timeStr = PlayerPrefs.GetString($"lb_time_{i}", "");
            if (string.IsNullOrEmpty(timeStr)) continue;

            if (float.TryParse(timeStr, out float time))
            {
                entries.Add(new LeaderboardEntry
                {
                    time = time,
                    lap = PlayerPrefs.GetInt($"lb_lap_{i}", 0),
                    date = PlayerPrefs.GetString($"lb_date_{i}", "")
                });
            }
        }
        return entries;
    }

    private void SaveEntries(List<LeaderboardEntry> entries)
    {
        for (int i = 0; i < maxEntries; i++)
        {
            if (i < entries.Count)
            {
                PlayerPrefs.SetString($"lb_time_{i}", entries[i].time.ToString());
                PlayerPrefs.SetInt($"lb_lap_{i}", entries[i].lap);
                PlayerPrefs.SetString($"lb_date_{i}", entries[i].date);
            }
            else
            {
                PlayerPrefs.DeleteKey($"lb_time_{i}");
                PlayerPrefs.DeleteKey($"lb_lap_{i}");
                PlayerPrefs.DeleteKey($"lb_date_{i}");
            }
        }
        PlayerPrefs.Save();
    }

    private void CreateLeaderboardUI()
    {
        GameObject canvasGO = new GameObject("LeaderboardCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        panel = new GameObject("LeaderboardPanel", typeof(RectTransform));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(400, 350);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        GameObject textGO = new GameObject("EntriesText", typeof(RectTransform));
        textGO.transform.SetParent(panelRT, false);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = new Vector2(-40, -40);
        textRT.anchoredPosition = Vector2.zero;

        entriesText = textGO.AddComponent<TextMeshProUGUI>();
        entriesText.fontSize = 24;
        entriesText.alignment = TextAlignmentOptions.TopLeft;
        entriesText.color = Color.white;
        entriesText.text = "<b>LEADERBOARD</b>\n\nNo times yet";
    }

    [System.Serializable]
    private struct LeaderboardEntry
    {
        public float time;
        public int lap;
        public string date;
    }
}
