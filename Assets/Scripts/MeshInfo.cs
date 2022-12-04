using EasyButtons;
using UnityEngine;

public class MeshInfo: MonoBehaviour
{
    public int trisCount;
    
    [Button]
    private void GetInfo()
    {
        var sumTris = 0;
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer is SkinnedMeshRenderer)
            {
                var tris = (renderer as SkinnedMeshRenderer).sharedMesh.triangles.Length / 3;
                sumTris += tris;
            }
            
            if (renderer is MeshRenderer)
            {
                var tris = (renderer as MeshRenderer).GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3;
                sumTris += tris;
            }
        }

        trisCount = sumTris;
    }
}