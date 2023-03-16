using UnityEngine;

namespace VRE.Scripts.Infos
{

    public class OutfitItemInfo : Info
    {
        [Tooltip("Min = 0, Max = 1. The lower the worse")]
        public float minQuality = 1f;
        public bool belongsToBody;
    }
}