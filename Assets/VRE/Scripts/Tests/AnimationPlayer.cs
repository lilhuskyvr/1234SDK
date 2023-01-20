#if UNITY_EDITOR
using EasyButtons;
using UnityEditor;
using UnityEngine;

namespace VRE.Scripts.Tests
{
    public class AnimationPlayer : MonoBehaviour
    {
        public AnimationClip animationClip;
        [Range(0, 100)] public int time;
        
        public void Play()
        {
            if (ReferenceEquals(animationClip, null)) return;
            AnimationMode.StartAnimationMode();
            AnimationMode.SampleAnimationClip(gameObject, animationClip, time * animationClip.length / 100);
        }

        private void OnValidate()
        {
            Play();
        }

        [Button]
        public void Reset()
        {
            AnimationMode.StopAnimationMode();
        }
    }
}

#endif