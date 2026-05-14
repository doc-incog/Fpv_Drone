using UnityEngine;
using System.Collections.Generic;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    [Header("Race Settings")]
    public int totalLaps = 3;
    public bool autoStart = true;

    [Header("State")]
    public int currentLap = 1;
    public int currentGateIndex = 0;
    public int totalGates = 0;
    public float currentLapTime = 0f;
    public float totalRaceTime = 0f;
    public bool raceActive = false;
    public bool raceComplete = false;

    private List<GateTrigger> gates = new List<GateTrigger>();
    private int startFinishGateIndex = -1;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        FindAndSortGates();

        if (autoStart)
            StartRace();
    }

    void Update()
    {
        if (raceActive && !raceComplete)
        {
            currentLapTime += Time.deltaTime;
            totalRaceTime += Time.deltaTime;
        }
    }

    public void StartRace()
    {
        raceActive = true;
        raceComplete = false;
        currentLap = 1;
        currentGateIndex = 0;
        currentLapTime = 0f;
        totalRaceTime = 0f;
        ResetAllGates();
    }

    public bool OnGatePassed(GateTrigger gate)
    {
        if (!raceActive || raceComplete) return false;

        if (gate.isStartFinish)
        {
            if (currentGateIndex >= totalGates || currentGateIndex == 0)
            {
                CompleteLap();
                return true;
            }
            return false;
        }

        if (gate.gateIndex == currentGateIndex)
        {
            currentGateIndex++;
            return true;
        }

        return false;
    }

    private void CompleteLap()
    {
        if (currentLap >= totalLaps)
        {
            raceComplete = true;
            raceActive = false;
            Debug.Log($"Race Complete! Total time: {totalRaceTime:F1}s");

            LeaderboardManager lb = FindFirstObjectByType<LeaderboardManager>();
            if (lb != null) lb.AddTime(totalRaceTime, currentLap);
        }
        else
        {
            currentLap++;
            ResetAllGates();
        }

        currentGateIndex = 0;
        currentLapTime = 0f;
    }

    private void FindAndSortGates()
    {
        gates.Clear();
        GateTrigger[] allGates = FindObjectsByType<GateTrigger>(FindObjectsSortMode.None);
        foreach (GateTrigger g in allGates)
        {
            if (g.isStartFinish)
                startFinishGateIndex = gates.Count;
            gates.Add(g);
        }

        totalGates = gates.Count;
    }

    private void ResetAllGates()
    {
        foreach (GateTrigger g in gates)
            g.ResetGate();
    }
}
