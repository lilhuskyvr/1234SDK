using System;
using EasyButtons;
using UnityEngine;

namespace DefaultNamespace
{
    public class ScreenShotter: MonoBehaviour
    {
        [Button]
        public void ScreenShot()
        {
            Debug.Log("Screenshot");
            ScreenCapture.CaptureScreenshot($"Assets/ExcludedFromGithub/ScreenShots/{Time.time}.png");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
                ScreenShot();
        }
    } 
}