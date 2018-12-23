using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Spine;
using System.IO;
using WaveEngine.Framework.Services;
using System.Net;
using Android.Content.PM;

namespace GFI_with_GFS_A
{
    [Activity(Label = "SpineViewer")]
    public class SpineViewer : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(Path.Combine(ETC.Server, "Data", "SDData", "k2", "k2.atlas"), Path.Combine(ETC.CachePath, "k2.atlas"));
                wc.DownloadFile(Path.Combine(ETC.Server, "Data", "SDData", "k2", "k2.png"), Path.Combine(ETC.CachePath, "k2.png"));
                wc.DownloadFile(Path.Combine(ETC.Server, "Data", "SDData", "k2", "k2.skel"), Path.Combine(ETC.CachePath, "k2.skel"));
            }

            
            var screencontext = new ScreenContext(new SpineScene());
            WaveServices.InitializeServices();
            WaveServices.ScreenContextManager.To(screencontext);
        }
    }

    public class SpineScene : Scene
    {
        Entity Character;

        protected override void CreateScene()
        {
            Character = new Entity()
                .AddComponent(new Transform2D())
                .AddComponent(new SkeletalData(Path.Combine(ETC.CachePath, "k2.atlas")))
                .AddComponent(new SkeletalAnimation(Path.Combine(ETC.CachePath, "k2.skel")))
                .AddComponent(new SkeletalRenderer());

            var anim = Character.FindComponent<SkeletalAnimation>();
            anim.CurrentAnimation = "move";
            anim.Play(true);
        }
    }
}