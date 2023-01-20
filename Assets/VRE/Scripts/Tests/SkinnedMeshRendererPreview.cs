#if UNITY_EDITOR
using System;
using EasyButtons;
using UnityEditor;
using UnityEngine;
using VRE.Scripts.Extensions;

namespace VRE.Scripts.Tests
{
    [DisallowMultipleComponent]
    public class SkinnedMeshRendererPreview : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Start()
        {
            Clean();
        }
#endif
        [Button]
        public void Preview()
        {
            Clean();
            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var meshFilterGo = new GameObject($"{skinnedMeshRenderer.name}Preview");
                meshFilterGo.transform.SetParentOrigin(skinnedMeshRenderer.transform);

                var meshFilter = meshFilterGo.AddComponent<MeshFilter>();
                var meshRenderer = meshFilterGo.AddComponent<MeshRenderer>();
                meshFilter.sharedMesh = skinnedMeshRenderer.sharedMesh;

                meshRenderer.sharedMaterials = new[] { skinnedMeshRenderer.sharedMaterial };
            }

            EditorUtility.SetDirty(gameObject);
        }

        [Button]
        public void UnPreview()
        {
            Clean();
        }

        public void Clean()
        {
            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                DestroyImmediate(meshFilter.gameObject, true);
            }
        }
    }
}
#endif