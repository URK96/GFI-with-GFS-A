using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using ImageViews.Photo;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "EnemyDBImageViewer", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EnemyDBImageViewer : AppCompatActivity
    {
        private DataRow EnemyInfoDR = null;
        private string EnemyCodeName;
        private string EnemyName;

        private CoordinatorLayout SnackbarLayout;
        private ProgressBar LoadProgressBar;
        private Button RefreshCacheButton;
        private PhotoView EnemyImageView;
        private TextView ImageStatus;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.EnemyDB_ImageViewer);

                string temp = Intent.GetStringExtra("Data");

                EnemyInfoDR = ETC.FindDataRow(ETC.EnemyList, "CodeName", temp);
                EnemyCodeName = (string)EnemyInfoDR["CodeName"];
                EnemyName = (string)EnemyInfoDR["Name"];

                EnemyImageView = FindViewById<PhotoView>(Resource.Id.EnemyDBImageViewerImageView);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EnemyDBImageViewerSnackbarLayout);
                LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.EnemyDBImageViewerLoadProgress);
                LoadProgressBar.Visibility = ViewStates.Visible;
                RefreshCacheButton = FindViewById<Button>(Resource.Id.EnemyDBImageViewerRefreshImageCacheButton);
                RefreshCacheButton.Click += delegate { LoadImage(true); };
                ImageStatus = FindViewById<TextView>(Resource.Id.EnemyDBImageViewerImageStatus);

                LoadImage(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private async void LoadImage(bool IsRefresh)
        {
            try
            {
                LoadProgressBar.Visibility = ViewStates.Visible;

                await Task.Delay(100);

                string ImagePath = Path.Combine(ETC.CachePath, "Enemy", "Normal", EnemyCodeName + ".gfdcache");

                if ((File.Exists(ImagePath) == false) || (IsRefresh == true))
                {
                    using (WebClient wc = new WebClient())
                    {
                        await Task.Run(async () => { await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Enemy", "Normal", EnemyCodeName + ".png"), ImagePath); });
                    }
                }

                await Task.Delay(500);

                EnemyImageView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(ImagePath));

                ImageStatus.Text = string.Format("{0} - {1}", EnemyCodeName, EnemyName);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                LoadProgressBar.Visibility = ViewStates.Invisible;
                IsRefresh = false;
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }
}