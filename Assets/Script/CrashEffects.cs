using UnityEngine;

public class CrashEffects : MonoBehaviour
{
    [Header("Collision Detection")]
    public float minImpactForce = 5f;
    public float throttleCutDuration = 0.3f;

    [Header("Particle Effects")]
    public GameObject sparkPrefab;
    public int maxSparks = 3;

    [Header("Camera Shake")]
    public float shakeDuration = 0.25f;
    public float shakeMagnitude = 0.4f;
    public float shakeRotationMag = 5f;

    private DroneController drone;
    private Rigidbody rb;
    private float throttleCutTimer;
    private CameraShake currentShake;

    void Start()
    {
        drone = GetComponent<DroneController>();
        rb = GetComponent<Rigidbody>();

        if (sparkPrefab == null)
            sparkPrefab = Resources.Load<GameObject>("Prefabs/Spark_ParticleEffect");

        currentShake = FindFirstObjectByType<CameraShake>();
        if (currentShake == null)
        {
            GameObject shakeGO = new GameObject("CameraShake");
            currentShake = shakeGO.AddComponent<CameraShake>();
        }
    }

    void Update()
    {
        if (throttleCutTimer > 0)
        {
            throttleCutTimer -= Time.deltaTime;
            if (drone != null) drone.throttleInput = -1f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < minImpactForce) return;

        throttleCutTimer = throttleCutDuration;

        ContactPoint contact = collision.GetContact(0);

        AudioController audio = GetComponent<AudioController>();
        if (audio != null) audio.PlayImpact(contact.point, impactForce / 20f);

        if (currentShake != null)
            currentShake.TriggerShake(shakeDuration, shakeMagnitude * (impactForce / 20f), shakeRotationMag * (impactForce / 20f));

        if (sparkPrefab != null)
        {
            int count = Mathf.Min(maxSparks, Mathf.CeilToInt(impactForce / 10f));
            for (int i = 0; i < count; i++)
            {
                GameObject spark = Instantiate(sparkPrefab, contact.point, Quaternion.LookRotation(contact.normal));
                Destroy(spark, 2f);
            }
        }
    }
}

public class CameraShake : MonoBehaviour
{
    private float shakeTimer;
    private float shakeAmount;
    private float rotationAmount;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Transform camTransform;

    void Start()
    {
        camTransform = Camera.main?.transform;
        if (camTransform != null)
        {
            originalPos = camTransform.localPosition;
            originalRot = camTransform.localRotation;
        }
    }

    void Update()
    {
        if (camTransform == null)
        {
            camTransform = Camera.main?.transform;
            if (camTransform != null)
            {
                originalPos = camTransform.localPosition;
                originalRot = camTransform.localRotation;
            }
            return;
        }

        if (shakeTimer > 0)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            camTransform.localRotation = originalRot * Quaternion.Euler(
                Random.Range(-1f, 1f) * rotationAmount,
                Random.Range(-1f, 1f) * rotationAmount * 0.5f,
                Random.Range(-1f, 1f) * rotationAmount * 0.3f
            );
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            camTransform.localPosition = originalPos;
            camTransform.localRotation = originalRot;
        }
    }

    public void TriggerShake(float duration, float magnitude, float rotMag)
    {
        shakeTimer = Mathf.Max(shakeTimer, duration);
        shakeAmount = Mathf.Max(shakeAmount, magnitude);
        rotationAmount = Mathf.Max(rotationAmount, rotMag);
    }
}
