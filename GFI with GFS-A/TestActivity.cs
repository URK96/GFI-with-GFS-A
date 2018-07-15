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
using Felipecsl.GifImageViewLibrary;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Support.Design.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Label = "TestActivity")]
    public class TestActivity : Activity
    {
        GifImageView gifviewer;
        Spinner AnimationSelector;
        ProgressBar LoadProgressBar;

        CoordinatorLayout SnackbarLayout;

        string[] list = { "attack", "attack2", "die", "move", "victory", "victoryloop" };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.TestLayout);

            gifviewer = FindViewById<GifImageView>(Resource.Id.SDAnimationImageView);
            AnimationSelector = FindViewById<Spinner>(Resource.Id.SDAnimationListSelector);
            AnimationSelector.ItemSelected += AnimationSelector_ItemSelected;
            LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.SDAnimationViewerLoadProgress);

            //SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.SDAnimationSnackbarLayout);

            ListProcess();
        }

        private void AnimationSelector_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            LoadSDAnimation_GIF(e.Position);
        }

        private void ListProcess()
        {
            var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, list);
            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

            AnimationSelector.Adapter = adapter;
        }

        private async Task LoadSDAnimation_GIF(int index)
        {
            await Task.Delay(100);

            try
            {
                LoadProgressBar.Indeterminate = true;
                LoadProgressBar.Progress = 0;
                LoadProgressBar.Visibility = ViewStates.Visible;

                gifviewer.StopAnimation();

                string DollName = "AK-12";
                string FileName = DollName + "_" + list[index];
                string filepath = Path.Combine(ETC.CachePath, "Doll", "SD", "Animation", FileName + ".gfdcache");
                byte[] data;

                if (File.Exists(filepath) == false)
                {
                    using (HttpClient hc = new HttpClient())
                    {
                        data = await hc.GetByteArrayAsync(Path.Combine(ETC.Server, "Data", "Images", "SDAnimation", "Doll", DollName, FileName + ".gif"));
                    }

                    using (BinaryWriter bw = new BinaryWriter(new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite)))
                    {
                        bw.Write(data.Length);
                        bw.Write(data);
                        bw.Flush();
                    }
                }
                else
                {
                    using (BinaryReader br = new BinaryReader(new FileStream(filepath, FileMode.Open, FileAccess.Read)))
                    {
                        int length = br.ReadInt32();
                        data = br.ReadBytes(length);
                    }
                }

                gifviewer.SetBytes(data);
                gifviewer.StartAnimation();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "error load animation", Snackbar.LengthShort);
            }
            finally
            {
                LoadProgressBar.Visibility = ViewStates.Invisible;
            }
        }
    }
}