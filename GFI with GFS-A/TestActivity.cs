using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Android.App;
using System.Net;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Urho.Urho2D;
using Spine;
using Felipecsl.GifImageViewLibrary;
using Android.Graphics.Drawables;
using Android.Graphics;
using Java.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "TestActivity")]
    public class TestActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.testLayout);

            InitProcess();
        }

        private async Task InitProcess()
        {
            GifImageView view = FindViewById<GifImageView>(Resource.Id.TestImageView);

            byte[] data;

            using (HttpClient wc = new HttpClient())
            {
                data = await wc.GetByteArrayAsync(System.IO.Path.Combine(ETC.Server, "Data", "Images", "SDAnimation", "Doll", "AK-12", "AK-12_die.gif"));
            }

            view.SetBytes(data);
            view.StartAnimation();
        }
    }
}