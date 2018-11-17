using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Android.App;
using System.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Support.Design.Widget;
using Android;
using Spine;
using Android.Graphics.Drawables;
using Android.Support.V7.App;

namespace GFI_with_GFS_A
{
    [Activity(Label = "TestActivity", Theme = "@style/GFS")]
    public class TestActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.TestLayout);

            DownloadProcess();

            //AtlasPage page = new AtlasPage();

            TextureLoader textureLoader = new MyTextureLoader();
            Atlas atlas = new Atlas(Path.Combine(ETC.CachePath, "k2.atlas"), textureLoader);

            AtlasAttachmentLoader attachmentLoader = new AtlasAttachmentLoader(atlas);
            SkeletonBinary binary = new SkeletonBinary(attachmentLoader);
            SkeletonData skeletonData = binary.ReadSkeletonData(Path.Combine(ETC.CachePath, "k2.skel"));
            Skeleton skeleton = new Skeleton(skeletonData);

            AnimationStateData stateData = new AnimationStateData(skeletonData);
            AnimationState state = new AnimationState(stateData);
            state.SetAnimation(0, "attack", true);
            state.Update(0);
            state.Apply(skeleton);
            skeleton.UpdateWorldTransform();
        }

        private void DownloadProcess()
        {
            string SDDataPath = Path.Combine(ETC.Server, "Data", "SDData");

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(Path.Combine(SDDataPath, "k2", "k2.png"), Path.Combine(ETC.CachePath, "k2.png"));
                wc.DownloadFile(Path.Combine(SDDataPath, "k2", "k2.skel"), Path.Combine(ETC.CachePath, "k2.skel"));
                wc.DownloadFile(Path.Combine(SDDataPath, "k2", "k2.atlas"), Path.Combine(ETC.CachePath, "k2.atlas"));
            }
        }
    }

    internal class MyTextureLoader : TextureLoader
    {
        public void Load(AtlasPage page, string path)
        {
            Drawable drawable = Drawable.CreateFromPath(path);
            page.rendererObject = drawable;
        }

        public void Unload(object texture)
        {
            Drawable drawable = (Drawable)texture;
            drawable.Dispose();
        }
    }
}