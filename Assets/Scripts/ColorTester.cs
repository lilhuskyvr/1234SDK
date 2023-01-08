using EasyButtons;
using UnityEngine;

namespace DefaultNamespace
{
    public class ColorTester: MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [Button]
        public void TryColor(string colorHex)
        {
            if (ColorUtility.TryParseHtmlString(colorHex, out var color))
            {
                foreach (var material in GetComponent<SkinnedMeshRenderer>().sharedMaterials)
                {
                    material.SetColor(BaseColor, color);
                }
            }
        }
    }
}