using UnityEngine;

public class AddCityColliders : MonoBehaviour
{
    void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        int count = meshFilters.Length;
        CombineInstance[] combiners = new CombineInstance[count];

        for (int i = 0; i < count; i++)
        {
            if (meshFilters[i].sharedMesh == null) continue;
            combiners[i].mesh = meshFilters[i].sharedMesh;
            combiners[i].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combiners, true, true);

        MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = combinedMesh;
        mc.convex = false;
        mc.isTrigger = false;

        Debug.Log($"City collision added: {count} meshes combined into {combinedMesh.vertexCount} verts");
    }
}
