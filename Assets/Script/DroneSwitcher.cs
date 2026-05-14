using UnityEngine;
using System.Collections.Generic;

public class DroneSwitcher : MonoBehaviour
{
    [System.Serializable]
    public struct DroneEntry
    {
        public string name;
        public GameObject prefab;
        public float mass;
        public float throttleStrength;
        public float maxPitchRate;
        public float maxRollRate;
        public float maxYawRate;
    }

    [Header("Drone Configs")]
    public DroneEntry[] drones;

    [Header("Spawn")]
    public Vector3 spawnPosition = new Vector3(0f, 2f, 0f);

    [Header("Input")]
    public KeyCode switchKey = KeyCode.Tab;

    private int currentIndex = 0;
    private GameObject currentDrone;
    private List<GameObject> allDroneInstances = new List<GameObject>();

    void Start()
    {
        if (drones == null || drones.Length == 0)
        {
            enabled = false;
            return;
        }

        GameObject existingDrone = GameObject.Find("Drone_Parent");
        if (existingDrone != null)
        {
            Vector3 pos = existingDrone.transform.position;
            Quaternion rot = existingDrone.transform.rotation;

            DroneEntry entry = new DroneEntry();
            entry.name = "PBR Racing Drone";
            entry.prefab = null;
            entry.mass = 1.2f;
            entry.throttleStrength = 35f;
            entry.maxPitchRate = 600f;
            entry.maxRollRate = 600f;
            entry.maxYawRate = 300f;

            List<DroneEntry> list = new List<DroneEntry>(drones);
            list.Insert(0, entry);
            drones = list.ToArray();

            allDroneInstances.Add(existingDrone);
            currentDrone = existingDrone;
            ApplyDroneStats(currentDrone, drones[0]);
        }
        else
        {
            SpawnDrone(0);
        }

        for (int i = 1; i < drones.Length; i++)
        {
            if (drones[i].prefab != null)
            {
                GameObject go = Instantiate(drones[i].prefab, spawnPosition, Quaternion.identity);
                go.SetActive(false);
                allDroneInstances.Add(go);

                DroneController dc = go.GetComponent<DroneController>();
                if (dc == null) dc = go.AddComponent<DroneController>();
                ApplyDroneStats(go, drones[i]);
            }
        }

        DestroyExistingCameras(currentDrone);
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            SwitchDrone();
        }
    }

    public void SwitchDrone()
    {
        if (allDroneInstances.Count <= 1) return;

        allDroneInstances[currentIndex].SetActive(false);

        currentIndex = (currentIndex + 1) % allDroneInstances.Count;

        GameObject next = allDroneInstances[currentIndex];
        next.transform.position = spawnPosition;
        next.transform.rotation = Quaternion.identity;
        next.SetActive(true);

        Rigidbody rb = next.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        currentDrone = next;

        BatteryController batt = next.GetComponent<BatteryController>();
        if (batt != null) batt.ForceFullCharge();

        DroneController dc = next.GetComponent<DroneController>();
        if (dc != null)
        {
            Rigidbody drb = dc.GetComponent<Rigidbody>();
            if (drb != null)
            {
                drb.mass = drones[currentIndex].mass;
            }
            dc.throttleStrength = drones[currentIndex].throttleStrength;
            dc.maxPitchRate = drones[currentIndex].maxPitchRate;
            dc.maxRollRate = drones[currentIndex].maxRollRate;
            dc.maxYawRate = drones[currentIndex].maxYawRate;
        }

        DestroyExistingCameras(next);

        Debug.Log($"Switched to: {drones[currentIndex].name}");
    }

    private void SpawnDrone(int index)
    {
        if (drones[index].prefab == null) return;
        currentDrone = Instantiate(drones[index].prefab, spawnPosition, Quaternion.identity);
        allDroneInstances.Add(currentDrone);
        ApplyDroneStats(currentDrone, drones[index]);
    }

    private void ApplyDroneStats(GameObject go, DroneEntry entry)
    {
        DroneController dc = go.GetComponent<DroneController>();
        if (dc != null)
        {
            dc.throttleStrength = entry.throttleStrength;
            dc.maxPitchRate = entry.maxPitchRate;
            dc.maxRollRate = entry.maxRollRate;
            dc.maxYawRate = entry.maxYawRate;
        }

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null) rb.mass = entry.mass;
    }

    private void DestroyExistingCameras(GameObject activeDrone)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        DroneController dc = activeDrone.GetComponent<DroneController>();
        if (dc != null)
        {
            Transform camTarget = activeDrone.transform.Find("Racing Drone Merged/Cam_Parent");
            if (camTarget != null)
                dc.cameraTarget = camTarget;
        }
    }
}
