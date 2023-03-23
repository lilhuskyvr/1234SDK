using EasyButtons;
using UnityEditor;
using UnityEngine;

namespace VRE.Scripts.WeaponComponents
{
    public enum HandPoseEnum
    {
        GrabSmall
    }
    public class Handle : MonoBehaviour
    {
        public bool allowReverseGrip;
        public bool allowLeftHand = true;
        public bool allowRightHand = true;
        public bool allowTwoHands;
        public float handleLength;
        public string handPoseId;

        [Button]
        public void SetHandPose(HandPoseEnum handPoseEnum)
        {
            handPoseId = handPoseEnum.ToString();
        }
    }
}