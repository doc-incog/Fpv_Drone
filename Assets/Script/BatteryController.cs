using UnityEngine;

public class BatteryController : MonoBehaviour
{
    [Header("Battery Settings")]
    public float maxBattery = 100f;
    public float drainRate = 8f;
    public float idleDrainRate = 0.5f;
    public float rechargeRate = 50f;
    public float lowBatteryThreshold = 20f;

    [Header("State")]
    public float currentBattery;
    public bool isCharging = false;

    private DroneController drone;
    private AudioController audioCtrl;

    void Start()
    {
        drone = GetComponent<DroneController>();
        audioCtrl = GetComponent<AudioController>();
        currentBattery = maxBattery;
    }

    void Update()
    {
        if (isCharging)
        {
            currentBattery = Mathf.Min(maxBattery, currentBattery + rechargeRate * Time.deltaTime);
            if (currentBattery >= maxBattery)
                isCharging = false;
            return;
        }

        float throttle = drone != null ? drone.throttleInput : 0f;
        float stick = Mathf.Abs(throttle);
        float deadzone = 0.1f;
        float effectiveThrottle = stick < deadzone ? 0f : stick;

        float drain = idleDrainRate * Time.deltaTime;
        drain += drainRate * effectiveThrottle * Time.deltaTime;

        currentBattery = Mathf.Max(0, currentBattery - drain);

        if (currentBattery <= 0 && drone != null)
        {
            drone.throttleInput = -1f;
        }

        if (audioCtrl != null && currentBattery <= lowBatteryThreshold && currentBattery > 0)
            audioCtrl.PlayLowBatteryBeep();
    }

    public float GetPercentage()
    {
        return (currentBattery / maxBattery) * 100f;
    }

    public void StartCharging()
    {
        isCharging = true;
    }

    public void ForceFullCharge()
    {
        currentBattery = maxBattery;
        isCharging = false;
    }
}
