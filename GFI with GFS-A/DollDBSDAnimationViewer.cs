using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Felipecsl.GifImageViewLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "DollDBSDAnimationViewer", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBSDAnimationViewer : AppCompatActivity
    {
        private DataRow DollInfoDR = null;
        private string DollName;

        private CoordinatorLayout SnackbarLayout = null;
        private Spinner SDCategoryList = null;
        private Spinner SDAnimationList = null;
        private ProgressBar LoadProgressBar = null;
        private GifImageView SDAnimationViewer;

        private List<string> Category;
        private List<string> Animations;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.DollDBSDAnimationLayout);

                DollName = Intent.GetStringExtra("Data");
                DollInfoDR = ETC.FindDataRow(ETC.DollList, "Name", DollName);

                LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.DollSDAnimationViewerLoadProgress);
                SDCategoryList = FindViewById<Spinner>(Resource.Id.DollSDAnimationCategoryList);
                SDCategoryList.ItemSelected += SDCategoryList_ItemSelected;
                SDAnimationList = FindViewById<Spinner>(Resource.Id.DollSDAnimationList);
                SDAnimationList.ItemSelected += SDAnimationList_ItemSelected;
                SDAnimationViewer = FindViewById<GifImageView>(Resource.Id.DollSDAnimationImageView);

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollSDAnimationSnackbarLayout);

                LoadCategoryList();
                LoadAnimationList(0);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void SDAnimationList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            LoadSDAnimation(SDCategoryList.SelectedItemPosition, e.Position);
        }

        private void SDCategoryList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            LoadAnimationList(e.Position);
        }

        private void LoadCategoryList()
        {
            try
            {
                Category = new List<string>()
                {
                    "Normal"
                };

                if (DollInfoDR["RoomAnimation"] != DBNull.Value)
                {
                    string list = (string)DollInfoDR["RoomAnimation"];
                    if (string.IsNullOrEmpty(list) == false) Category.Add("Room");
                }

                Category.TrimExcess();

                var CategoryAdapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, Category);
                CategoryAdapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                SDCategoryList.Adapter = CategoryAdapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Category_InitError, ToastLength.Short).Show();
            }
        }

        private void LoadAnimationList(int index)
        {
            try
            {
                Animations = new List<string>();

                string[] sd_list = null;

                switch (Category[index])
                {
                    case "Normal":
                        sd_list = ((string)DollInfoDR["NormalAnimation"]).Split(';');
                        break;
                    case "Room":
                        sd_list = ((string)DollInfoDR["RoomAnimation"]).Split(';');
                        break;
                }

                Animations.AddRange(sd_list);
                Animations.TrimExcess();

                var AnimationListAdapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, Animations);
                AnimationListAdapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                SDAnimationList.Adapter = AnimationListAdapter;

                LoadSDAnimation(index, 0);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.List_InitError, ToastLength.Short).Show();
            }
        }

        private async Task LoadSDAnimation(int c_index, int index)
        {
            await Task.Delay(100);

            try
            {
                string category = Category[c_index];

                LoadProgressBar.Indeterminate = true;
                LoadProgressBar.Progress = 0;
                LoadProgressBar.Visibility = ViewStates.Visible;

                SDAnimationViewer.StopAnimation();

                StringBuilder FileName = new StringBuilder();

                switch (category)
                {
                    case "Room":
                        FileName.Append("R");
                        break;
                }

                FileName.Append(DollName);
                FileName.Append("_");
                FileName.Append(Animations[index]);

                string filepath = Path.Combine(ETC.CachePath, "Doll", "SD", "Animation", FileName.ToString() + ".gfdcache");
                byte[] data;

                if (File.Exists(filepath) == false)
                {
                    using (HttpClient hc = new HttpClient())
                    {
                        data = await hc.GetByteArrayAsync(Path.Combine(ETC.Server, "Data", "Images", "SDAnimation", "Doll", DollName, FileName.ToString() + ".gif"));
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

                SDAnimationViewer.SetBytes(data);
                SDAnimationViewer.StartAnimation();
                
                data = null;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "Error Load Animation", Snackbar.LengthShort);
            }
            finally
            {
                LoadProgressBar.Visibility = ViewStates.Invisible;
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            SDAnimationViewer.StopAnimation();
        }
    }
}