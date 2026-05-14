using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public int gateIndex = 0;
    public bool isStartFinish = false;

    public Material defaultMaterial;
    public Material passedMaterial;
    public Light gateLight;

    private bool wasPassed = false;
    private Renderer gateRenderer;

    void Start()
    {
        gateRenderer = GetComponent<Renderer>();
        if (gateRenderer == null)
            gateRenderer = GetComponentInChildren<Renderer>();

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) gateRenderer = mr;

        if (defaultMaterial == null && gateRenderer != null)
            defaultMaterial = gateRenderer.sharedMaterial;

        if (gateLight == null)
            gateLight = GetComponentInChildren<Light>();

        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(5, 5, 2);
        }
        else
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.GetComponent<DroneController>())
            return;

        if (wasPassed && !isStartFinish) return;

        RaceManager race = FindFirstObjectByType<RaceManager>();
        if (race != null)
        {
            bool valid = race.OnGatePassed(this);
            if (valid)
            {
                wasPassed = !isStartFinish;
                SetPassedVisual();

                AudioController audio = FindFirstObjectByType<AudioController>();
                if (audio != null) audio.PlayGateChime();
            }
        }
    }

    public void ResetGate()
    {
        wasPassed = false;
        if (gateRenderer != null && defaultMaterial != null)
            gateRenderer.material = defaultMaterial;

        if (gateLight != null)
            gateLight.color = Color.white;
    }

    private void SetPassedVisual()
    {
        if (gateRenderer != null)
        {
            if (passedMaterial != null)
                gateRenderer.material = passedMaterial;
            else
                gateRenderer.material.color = Color.green;
        }

        if (gateLight != null)
            gateLight.color = Color.green;
    }
}
