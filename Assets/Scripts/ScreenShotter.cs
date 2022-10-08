using EasyButtons;
using UnityEngine;

namespace DefaultNamespace
{
    public class ScreenShotter: MonoBehaviour
    {
        [Button]
        public void ScreenShot()
        {
            ScreenCapture.CaptureScreenshot($"Assets/ExcludedFromGithub/ScreenShots/{Time.time}.png");
        }
    } 
}