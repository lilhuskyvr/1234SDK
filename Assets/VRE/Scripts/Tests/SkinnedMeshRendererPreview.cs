#if UNITY_EDITOR
using EasyButtons;
using UniGLTF;
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
                var goName = $"{skinnedMeshRenderer.name}Preview";
                var goTransform = skinnedMeshRenderer.transform.Find(goName);
                GameObject meshFilterGo;
                
                if (goTransform == null)
                {
                    meshFilterGo = new GameObject(goName);
                }
                else
                {
                    meshFilterGo = goTransform.gameObject;
                }
                
                meshFilterGo.SetActive(true);

                meshFilterGo.transform.SetParentOrigin(skinnedMeshRenderer.transform);

                var meshFilter = meshFilterGo.GetOrAddComponent<MeshFilter>();
                var meshRenderer = meshFilterGo.GetOrAddComponent<MeshRenderer>();

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
                meshFilter.gameObject.SetActive(false);
            }
        }
    }
}
#endif