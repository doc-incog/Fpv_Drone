using UnityEngine;

public class GateSetup : MonoBehaviour
{
    void Start()
    {
        GateTrigger[] existingGates = FindObjectsByType<GateTrigger>(FindObjectsSortMode.None);
        if (existingGates.Length > 0)
        {
            Debug.Log($"GateSetup: {existingGates.Length} gates already exist, skipping auto-setup");
            return;
        }

        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        int gateCount = 0;
        int index = 0;

        foreach (Transform t in allTransforms)
        {
            if (t.name.ToLower().Contains("tron_ring"))
            {
                GameObject go = t.gameObject;
                GateTrigger gt = go.GetComponent<GateTrigger>();
                if (gt == null) gt = go.AddComponent<GateTrigger>();

                gt.gateIndex = index;

                if (gateCount == 0)
                {
                    gt.isStartFinish = true;
                }

                BoxCollider col = go.GetComponent<BoxCollider>();
                if (col == null)
                {
                    col = go.AddComponent<BoxCollider>();
                }
                col.isTrigger = true;
                col.size = new Vector3(5, 5, 2);

                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = go.AddComponent<Rigidbody>();
                }
                rb.isKinematic = true;
                rb.useGravity = false;

                gateCount++;
                index++;
            }
        }

        Debug.Log($"GateSetup: Configured {gateCount} gates");
    }
}
