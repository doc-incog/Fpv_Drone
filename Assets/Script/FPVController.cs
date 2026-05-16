using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FPVController : MonoBehaviour
{
    [Header("Camera Modes")]
    public bool fpvMode = false;
    public KeyCode toggleKey = KeyCode.Y;

    private InputAction fpvToggleAction;

    [Header("References")]
    public Transform thirdPersonTarget;
    public Transform fpvTarget;
    public Camera fpvCamera;

    [Header("FPV Settings")]
    public float defaultFOV = 80f;

    [Header("OSD Elements")]
    public TextMeshProUGUI osdSpeedText;
    public TextMeshProUGUI osdAltText;
    public TextMeshProUGUI osdBatteryText;
    public TextMeshProUGUI osdThrottleText;
    public Image osdCrosshair;

    private Canvas osdCanvas;
    private Vector3 thirdPersonOffset;
    private DroneController drone;
    private BatteryController battery;

    void Awake()
    {
        fpvToggleAction = new InputAction("FPVToggle", binding: "<Gamepad>/leftShoulder");
    }

    void OnEnable()
    {
        fpvToggleAction.Enable();
        fpvToggleAction.performed += OnFPVInput;
    }

    void OnDisable()
    {
        fpvToggleAction.Disable();
        fpvToggleAction.performed -= OnFPVInput;
    }

    void Start()
    {
        drone = GetComponent<DroneController>();
        battery = GetComponent<BatteryController>();

        if (fpvCamera == null)
        {
            Transform camParent = transform.Find("Racing Drone Merged/Cam_Parent/Cam");
            if (camParent != null)
                fpvCamera = camParent.GetComponentInChildren<Camera>();
        }

        if (fpvCamera != null)
        {
            fpvCamera.fieldOfView = PlayerPrefs.GetFloat("fpv_fov", defaultFOV);
        }

        if (thirdPersonTarget == null && drone != null)
            thirdPersonTarget = drone.cameraTarget;

        CreateOSDCanvas();
        osdCanvas.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFPV();
        }

        if (fpvMode)
            UpdateOSD();
    }

    private void OnFPVInput(InputAction.CallbackContext context)
    {
        ToggleFPV();
    }

    public void ToggleFPV()
    {
        fpvMode = !fpvMode;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            ThirdPersonCamera tpcam = mainCam.GetComponent<ThirdPersonCamera>();

            if (fpvMode)
            {
                if (tpcam != null) tpcam.enabled = false;
                mainCam.transform.SetParent(fpvTarget != null ? fpvTarget : transform);
                mainCam.transform.localPosition = Vector3.zero;
                mainCam.transform.localRotation = Quaternion.identity;
                mainCam.fieldOfView = PlayerPrefs.GetFloat("fpv_fov", defaultFOV);
            }
            else
            {
                mainCam.transform.SetParent(null);
                if (tpcam != null) tpcam.enabled = true;
            }
        }

        if (osdCanvas != null)
            osdCanvas.enabled = fpvMode;
    }

    private void UpdateOSD()
    {
        if (drone == null) return;

        Rigidbody rb = drone.GetComponent<Rigidbody>();
        if (rb == null) return;

        float speed = rb.linearVelocity.magnitude * 3.6f;
        float altitude = drone.transform.position.y;

        if (osdSpeedText != null)
            osdSpeedText.text = $"{speed:F0}";

        if (osdAltText != null)
            osdAltText.text = $"{altitude:F1}m";

        if (battery != null && osdBatteryText != null)
        {
            float pct = battery.GetPercentage();
            osdBatteryText.text = $"{pct:F0}%";
            osdBatteryText.color = pct <= 20f ? Color.red : Color.green;
        }

        if (osdThrottleText != null)
        {
            float pct = (drone.throttleInput + 1f) * 50f;
            osdThrottleText.text = $"THR {pct:F0}%";
        }
    }

    public void SetFOV(float fov)
    {
        if (fpvCamera != null)
            fpvCamera.fieldOfView = fov;
    }

    private void CreateOSDCanvas()
    {
        GameObject canvasGO = new GameObject("FPV_OSD_Canvas");
        canvasGO.transform.SetParent(transform);
        osdCanvas = canvasGO.AddComponent<Canvas>();
        osdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        osdCanvas.sortingOrder = 50;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        Color green = new Color(0, 1, 0.3f);
        Color dimGreen = new Color(0, 0.8f, 0.2f, 0.8f);

        osdSpeedText = CreateOSDText("OSD_Speed", new Vector2(-800, -470), new Vector2(200, 80), $"{0:F0}", 72, FontStyles.Bold, green);

        GameObject unitLabel = new GameObject("OSD_Unit", typeof(RectTransform));
        unitLabel.transform.SetParent(osdCanvas.transform, false);
        RectTransform unitRT = unitLabel.GetComponent<RectTransform>();
        unitRT.anchorMin = new Vector2(0.5f, 0);
        unitRT.anchorMax = new Vector2(0.5f, 0);
        unitRT.pivot = new Vector2(0.5f, 0);
        unitRT.anchoredPosition = new Vector2(-690, -480);
        unitRT.sizeDelta = new Vector2(100, 30);
        TextMeshProUGUI unitTmp = unitLabel.AddComponent<TextMeshProUGUI>();
        unitTmp.text = "km/h";
        unitTmp.fontSize = 22;
        unitTmp.alignment = TextAlignmentOptions.BottomLeft;
        unitTmp.color = dimGreen;

        osdAltText = CreateOSDText("OSD_Alt", new Vector2(-800, -530), new Vector2(200, 35), "0.0m", 24, FontStyles.Normal, dimGreen);

        osdBatteryText = CreateOSDText("OSD_Battery", new Vector2(600, -470), new Vector2(150, 40), "100%", 28, FontStyles.Bold, Color.green);

        osdThrottleText = CreateOSDText("OSD_Throttle", new Vector2(600, -510), new Vector2(150, 30), "THR 0%", 22, FontStyles.Normal, dimGreen);

        GameObject crosshairGO = new GameObject("OSD_Crosshair", typeof(RectTransform));
        crosshairGO.transform.SetParent(osdCanvas.transform, false);
        RectTransform chRT = crosshairGO.GetComponent<RectTransform>();
        chRT.anchorMin = new Vector2(0.5f, 0.5f);
        chRT.anchorMax = new Vector2(0.5f, 0.5f);
        chRT.pivot = new Vector2(0.5f, 0.5f);
        chRT.anchoredPosition = Vector2.zero;
        chRT.sizeDelta = new Vector2(20, 20);
        osdCrosshair = crosshairGO.AddComponent<Image>();
        osdCrosshair.color = green;
    }

    private TextMeshProUGUI CreateOSDText(string name, Vector2 pos, Vector2 size, string text, int fontSize, FontStyles style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(osdCanvas.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.color = color;

        return tmp;
    }
}
