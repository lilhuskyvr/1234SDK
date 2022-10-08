using EasyButtons;
using UnityEngine;

public class MeshInfo: MonoBehaviour
{
    public int trisCount;
    
    [Button]
    private void GetInfo()
    {
        var sumTris = 0;
        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var tris = skinnedMeshRenderer.sharedMesh.triangles.Length / 3;
            sumTris += tris;
        }

        trisCount = sumTris;
    }
}