using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JP.Live2d;
using System.IO;

namespace GFI_with_GFS_A
{
    [Activity(Label = "Live2DViewer")]
    public class Live2DViewer : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(Path.Combine(ETC.Server, "Data", "Live2D", "KP31", "model.moc"), Path.Combine(ETC.CachePath, "model.moc"));
                wc.DownloadFile(Path.Combine(ETC.Server, "Data", "Live2D", "KP31", "texture_00.png"), Path.Combine(ETC.CachePath, "texture_00.png"));
            }

            Live2D.Init();

            Live2DSurfaceView surfaceview = new Live2DSurfaceView(this, Path.Combine(ETC.CachePath, "model.moc"), new string[] { Path.Combine(ETC.CachePath, "texture_00.png") });
            SetContentView(surfaceview);
        }
    }
}