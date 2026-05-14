using UnityEngine;

public class WindController : MonoBehaviour
{
    [Header("Wind Settings")]
    public float baseWindStrength = 0.5f;
    public float gustStrength = 3f;
    public float gustFrequency = 0.3f;
    public float gustDuration = 2f;
    public float noiseSpeed = 0.5f;

    [Header("State")]
    public float currentWindStrength = 0f;
    public Vector3 currentWindDirection = Vector3.right;

    private float gustTimer;
    private bool inGust;
    private float gustTimerCounter;
    private Vector3 baseDirection;
    private Vector3 targetDirection;

    void Start()
    {
        baseDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        targetDirection = baseDirection;
        currentWindDirection = baseDirection;
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * noiseSpeed, 0f) * 2f - 1f;
        Vector3 noiseDir = new Vector3(noise, 0f, Mathf.PerlinNoise(0f, Time.time * noiseSpeed) * 2f - 1f).normalized;

        gustTimerCounter += Time.deltaTime;

        if (!inGust && gustTimerCounter > Random.Range(3f, 8f))
        {
            inGust = true;
            gustTimer = gustDuration;
            targetDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            gustTimerCounter = 0f;
        }

        if (inGust)
        {
            gustTimer -= Time.deltaTime;
            if (gustTimer <= 0f)
            {
                inGust = false;
                targetDirection = baseDirection;
            }
        }

        currentWindDirection = Vector3.Slerp(currentWindDirection, targetDirection + noiseDir * 0.3f, Time.deltaTime * 2f).normalized;
        float targetStrength = inGust ? baseWindStrength + gustStrength : baseWindStrength;
        currentWindStrength = Mathf.Lerp(currentWindStrength, targetStrength + noise * 0.5f, Time.deltaTime * 2f);
        currentWindStrength = Mathf.Max(0f, currentWindStrength);
    }

    public Vector3 GetWindForce()
    {
        return currentWindDirection * currentWindStrength;
    }

    void FixedUpdate()
    {
        DroneController drone = FindFirstObjectByType<DroneController>();
        if (drone != null)
        {
            Rigidbody rb = drone.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(GetWindForce(), ForceMode.Acceleration);
        }
    }
}
