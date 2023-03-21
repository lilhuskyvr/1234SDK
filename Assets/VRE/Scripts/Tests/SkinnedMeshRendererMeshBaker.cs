#if UNITY_EDITOR
using System.IO;
using EasyButtons;
using UnityEditor;
using UnityEngine;

namespace VRE.Scripts.Tests
{
    public class SkinnedMeshRendererMeshBaker : MonoBehaviour
    {
        public string newSmrName = "NewSmr";
        [Button]
        public void Bake()
        {
            var mesh = new Mesh();
            var smr = GetComponent<SkinnedMeshRenderer>();
            smr.BakeMesh(mesh);
            var sharedMesh = smr.sharedMesh;
            mesh.bindposes = sharedMesh.bindposes;
            mesh.boneWeights = sharedMesh.boneWeights;

            var newSmr = Instantiate(smr, smr.transform.parent);
            newSmr.name = newSmrName;
            newSmr.transform.SetSiblingIndex(smr.transform.GetSiblingIndex() + 1);
            var path =
                $"{Path.GetDirectoryName(AssetDatabase.GetAssetPath(smr.sharedMesh))}\\{newSmr.name}.asset";
            AssetDatabase.CreateAsset(mesh,
                path);
            AssetDatabase.SaveAssets();

            newSmr.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        }
    }
}
#endif