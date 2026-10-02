using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PuzzleSpire.Hotel
{
    /// <summary>Offscreen QA capture of the authored Canvas, including when the test player is hidden.</summary>
    public static class HotelCapture
    {
        public static void PrepareHiddenPlayer(HotelView view)
        {
            // Hidden windows may skip normal presentation; keep one camera canvas for explicit QA renders.
            var canvas = view.titleScreen.GetComponentInParent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1;
        }
        public static void Save(HotelView view, string path)
        {
            var canvas = view.titleScreen.GetComponentInParent<Canvas>();
            var camera = Camera.main;
            var mode = canvas.renderMode; var previousCamera = canvas.worldCamera; var distance = canvas.planeDistance;
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                camera.targetTexture = target; Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                if (path == null) return;
                RenderTexture.active = target;
                image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode = mode; canvas.worldCamera = previousCamera; canvas.planeDistance = distance;
                camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(target); if (image != null) Object.Destroy(image);
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}
