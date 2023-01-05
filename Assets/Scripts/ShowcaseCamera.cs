using System;
using EasyButtons;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    [RequireComponent(typeof(FreeCamera))]
    public class ShowcaseCamera: MonoBehaviour
    {
        private FreeCamera _freeCamera;
        private void Start()
        {
            _freeCamera = GetComponent<FreeCamera>();
        }

        [Button]
        public void ScreenShot()
        {
            Debug.Log("Screenshot");
            ScreenCapture.CaptureScreenshot($"Assets/ExcludedFromGithub/ScreenShots/{Time.time}.png");
        }

        private void Update()
        {
            _freeCamera.enabled = Input.GetMouseButton(1);
            if (Input.GetKeyDown(KeyCode.F12))
                ScreenShot();
        }
    } 
}