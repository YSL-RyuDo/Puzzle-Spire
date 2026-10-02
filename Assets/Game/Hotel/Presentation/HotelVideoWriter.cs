using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PuzzleSpire.Hotel
{
    /// <summary>QA-only raw-frame pipe. Does not create UI or alter gameplay state.</summary>
    public sealed class HotelVideoWriter : IDisposable
    {
        private readonly Process encoder;
        private readonly Stream input;
        private readonly Camera camera;
        private readonly RenderTexture target;
        private readonly Texture2D pixels;
        private readonly string errorPath;
        private bool closed;
        public int Frames { get; private set; }
        public const int Fps = 24;

        public HotelVideoWriter(HotelView view, string executable, string output)
        {
            if (!File.Exists(executable)) throw new FileNotFoundException("Video encoder missing", executable);
            HotelCapture.PrepareHiddenPlayer(view);camera=Camera.main;
            target=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);target.Create();
            pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            errorPath=Path.Combine(Path.GetDirectoryName(output),"encoder.log");
            var start=new ProcessStartInfo(executable,
                "-y -f rawvideo -pixel_format rgb24 -video_size "+Screen.width+"x"+Screen.height+
                " -framerate "+Fps+" -i pipe:0 -vf vflip -an -c:v libx264 -preset veryfast -crf 21 -pix_fmt yuv420p -movflags +faststart \""+output+"\"")
            { UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardInput=true,RedirectStandardError=true };
            encoder=new Process{StartInfo=start};
            encoder.ErrorDataReceived+=(_,e)=>{if(e.Data!=null)File.AppendAllText(errorPath,e.Data+Environment.NewLine);};
            encoder.Start();encoder.BeginErrorReadLine();input=encoder.StandardInput.BaseStream;
        }
        public void Capture()
        {
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            try
            {
                camera.targetTexture=target;Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
                RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0,false);pixels.Apply(false);
                var bytes=pixels.GetRawTextureData<byte>().ToArray();input.Write(bytes,0,bytes.Length);Frames++;
            }
            finally { camera.targetTexture=oldTarget;RenderTexture.active=oldActive; }
        }
        public void Dispose()
        {
            if(closed)return;closed=true;
            input.Dispose();
            bool completed=encoder.WaitForExit(15000);
            if(!completed)encoder.Kill();
            int exit=completed?encoder.ExitCode:-1;encoder.Dispose();
            target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);
            if(exit!=0)throw new IOException("Video encoder failed; see "+errorPath);
        }
    }
}
