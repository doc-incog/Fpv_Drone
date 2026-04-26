using UnityEngine;
using UnityEngine.InputSystem;

public class DroneController : MonoBehaviour
{
    [Header("Flight Characteristics")]
    [SerializeField] private float throttleStrength = 35f;
    [SerializeField] private float minThrottleForce = 6f;
    [SerializeField] private float throttleExpo = 0.5f;

    [Header("Rates (deg/s at full stick)")]
    [SerializeField] private float rcRate = 1.0f;
    [SerializeField] private float superRate = 0.7f;
    [SerializeField] private float expo = 0.3f;

    [SerializeField] private float maxPitchRate = 600f;
    [SerializeField] private float maxRollRate = 600f;
    [SerializeField] private float maxYawRate = 300f;

    [Header("Airmode")]
    [SerializeField] private bool airmodeEnabled = true;
    [SerializeField] private float airmodeStrength = 0.7f;

    [Header("Propellers")]
    [SerializeField] private GameObject[] propellers;
    [SerializeField] private float propMaxRPM = 3000f;
    [SerializeField] private float propIdleRPM = 400f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float cameraSmoothTime = 0.04f;

    [Header("Respawn")]
    [SerializeField] private Vector3 respawnPosition = new Vector3(0f, 0.101f, 0f);

    [Header("Auto-Flip Recovery")]
    [SerializeField] private bool autoFlipEnabled = true;
    [SerializeField] private float flipDuration = 0.8f;         // How long the flip takes
    [SerializeField] private float flipHeightThreshold = 0.101f; // Y position to trigger flip

    // Input Actions
    private InputAction throttleAction;
    private InputAction pitchAction;
    private InputAction rollAction;
    private InputAction yawAction;
    private InputAction respawnAction;

    private float throttleInput;
    private float pitchInput;
    private float rollInput;
    private float yawInput;

    private Rigidbody rb;
    private Vector3 cameraVelocity;

    // Auto-flip state
    private bool isFlipping = false;
    private Quaternion flipStartRotation;
    private Quaternion flipTargetRotation;
    private float flipProgress = 0f;

    private void Awake()
    {
        throttleAction = new InputAction("Throttle", binding: "<Gamepad>/leftStick/y");
        pitchAction   = new InputAction("Pitch",   binding: "<Gamepad>/rightStick/y");
        rollAction    = new InputAction("Roll",    binding: "<Gamepad>/rightStick/x");
        yawAction     = new InputAction("Yaw",     binding: "<Gamepad>/leftStick/x");
        respawnAction = new InputAction("Respawn", binding: "<Gamepad>/buttonEast");
    }

    private void OnEnable()
    {
        throttleAction.Enable();
        pitchAction.Enable();
        rollAction.Enable();
        yawAction.Enable();
        respawnAction.Enable();

        respawnAction.performed += OnRespawn;
    }

    private void OnDisable()
    {
        throttleAction.Disable();
        pitchAction.Disable();
        rollAction.Disable();
        yawAction.Disable();
        respawnAction.Disable();

        respawnAction.performed -= OnRespawn;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = 1.2f;
        rb.linearDamping = 0f;
        rb.angularDamping = 2f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        AutoFindPropellers();
        if (cameraTarget == null)
        {
            Transform cam = transform.Find("Racing Drone Merged/Cam_Parent");
            if (cam != null) cameraTarget = cam;
        }
    }

    private void Update()
    {
        // Only read input if not currently auto-flipping
        if (!isFlipping)
        {
            throttleInput = throttleAction.ReadValue<float>();
            pitchInput    = -pitchAction.ReadValue<float>();
            rollInput     = -rollAction.ReadValue<float>();
            yawInput      = yawAction.ReadValue<float>();
        }

        SpinPropellers();
    }

    private void FixedUpdate()
    {
        // Check for auto-flip condition
        if (autoFlipEnabled && !isFlipping && ShouldAutoFlip())
        {
            StartAutoFlip();
        }

        if (isFlipping)
        {
            UpdateAutoFlip();
        }
        else
        {
            ApplyThrottle();
            ApplyRotation();
            ApplyCustomDrag();
        }

        UpdateCameraFollow();
    }

    private void ApplyThrottle()
    {
        float rawStick = throttleInput;
        float deadzone = 0.1f;
        float stick = Mathf.Abs(rawStick) < deadzone ? 0f : rawStick;

        float throttleFactor;
        if (stick >= 0)
        {
            throttleFactor = stick;
        }
        else
        {
            throttleFactor = stick * 0.3f;
        }

        float force;
        if (throttleFactor >= 0)
        {
            force = Mathf.Lerp(minThrottleForce, throttleStrength, throttleFactor);
        }
        else
        {
            float descentMultiplier = 1f + throttleFactor;
            force = minThrottleForce * Mathf.Max(descentMultiplier, 0.2f);
        }

        rb.AddForce(transform.up * force, ForceMode.Acceleration);
    }

    private void ApplyRotation()
    {
        float pitchCommand = ApplyRateCurve(pitchInput) * maxPitchRate * Mathf.Deg2Rad;
        float rollCommand  = ApplyRateCurve(rollInput)  * maxRollRate  * Mathf.Deg2Rad;
        float yawCommand   = ApplyRateCurve(yawInput)   * maxYawRate   * Mathf.Deg2Rad;

        Vector3 targetAngularVel = new Vector3(pitchCommand, yawCommand, -rollCommand);
        Vector3 worldTarget = transform.TransformDirection(targetAngularVel);

        Vector3 currentAngularVel = rb.angularVelocity;
        Vector3 torque = (worldTarget - currentAngularVel) * rb.mass;

        float throttleScale = airmodeEnabled ? 1f : Mathf.Clamp01((throttleInput + 1f) * 0.5f);
        throttleScale = Mathf.Lerp(airmodeStrength, 1f, throttleScale);
        torque *= throttleScale;

        rb.AddTorque(torque, ForceMode.Acceleration);
    }

    private float ApplyRateCurve(float stickInput)
    {
        float absStick = Mathf.Abs(stickInput);
        float sign = Mathf.Sign(stickInput);

        float expoFactor = 1f - expo * (1f - absStick);
        float rcCommand = stickInput * rcRate;
        float superFactor = 1f + superRate * absStick * absStick;
        float angleRate = rcCommand * superFactor * expoFactor;

        return angleRate;
    }

    private void ApplyCustomDrag()
    {
        rb.linearVelocity *= 0.97f;
    }

    private void SpinPropellers()
    {
        float stick = (throttleInput + 1f) * 0.5f;
        float rpm = Mathf.Lerp(propIdleRPM, propMaxRPM, stick);
        foreach (GameObject prop in propellers)
        {
            if (prop != null)
                prop.transform.Rotate(Vector3.forward * rpm * Time.deltaTime);
        }
    }

    private void UpdateCameraFollow()
    {
        if (cameraTarget != null && Camera.main != null)
        {
            Camera.main.transform.position = Vector3.SmoothDamp(
                Camera.main.transform.position,
                cameraTarget.position,
                ref cameraVelocity,
                cameraSmoothTime
            );
            Camera.main.transform.rotation = cameraTarget.rotation;
        }
    }

    private void AutoFindPropellers()
    {
        propellers = new GameObject[4];
        string[] motorNames = { "FL_Motor_Parent", "FR_Motor_Parent", "RL_Motor_Parent", "RR_Motor_Parent" };
        for (int i = 0; i < motorNames.Length; i++)
        {
            Transform motor = transform.Find("Racing Drone Merged/" + motorNames[i]);
            if (motor != null) propellers[i] = motor.Find("Prop")?.gameObject;
        }
    }

    // --- Respawn ---
    private void OnRespawn(InputAction.CallbackContext context)
    {
        Respawn();
    }

    private void Respawn()
    {
        // Cancel any ongoing flip
        isFlipping = false;

        transform.position = respawnPosition;
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Drone respawned at " + respawnPosition);
    }

    // --- Auto-Flip Recovery ---
    private bool ShouldAutoFlip()
    {
        // Upside down? (dot product of up vectors < 0)
        bool upsideDown = Vector3.Dot(transform.up, Vector3.up) < 0f;
        // At or below threshold?
        bool lowEnough = transform.position.y <= flipHeightThreshold;

        return upsideDown && lowEnough;
    }

    private void StartAutoFlip()
    {
        isFlipping = true;
        flipProgress = 0f;
        flipStartRotation = transform.rotation;

        // Target rotation is upright but preserving current yaw
        Vector3 currentEuler = transform.rotation.eulerAngles;
        flipTargetRotation = Quaternion.Euler(0f, currentEuler.y, 0f);

        // Zero out velocity during flip for stability
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Auto-flip started");
    }

    private void UpdateAutoFlip()
    {
        flipProgress += Time.fixedDeltaTime / flipDuration;

        if (flipProgress >= 1f)
        {
            // Finish flip
            transform.rotation = flipTargetRotation;
            isFlipping = false;

            // Ensure we are slightly above threshold to prevent immediate re-trigger
            Vector3 pos = transform.position;
            if (pos.y <= flipHeightThreshold)
            {
                pos.y = flipHeightThreshold + 0.05f;
                transform.position = pos;
            }

            Debug.Log("Auto-flip completed");
        }
        else
        {
            // Smooth spherical interpolation
            transform.rotation = Quaternion.Slerp(flipStartRotation, flipTargetRotation, flipProgress);
        }
    }
}