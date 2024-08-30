#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace VRE.Scripts.Infos
{
    public class OutfitItemInfo : Info
    {
        [Tooltip("Min = 0, Max = 1. The lower the worse")]
        public float minQuality = 1f;

        public bool belongsToBody;
        public string[] coverSkinMeshRendererNames = {};
        [Tooltip("Example #CA9D8E")]
        public List<string> colors = new();
    }
}
#endif