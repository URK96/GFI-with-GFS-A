﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;

using Google.Android.Material.Snackbar;

using ImageViews.Photo;

using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFDA
{
    [Activity(Label = "EnemyDBImageViewer", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EnemyDBImageViewer : BaseAppCompatActivity
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

                if (ETC.useLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.EnemyDBImageViewer);

                string temp = Intent.GetStringExtra("Data");

                EnemyInfoDR = ETC.FindDataRow(ETC.enemyList, "CodeName", temp);
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

                string ImagePath = Path.Combine(ETC.cachePath, "Enemy", "Normal", $"{EnemyCodeName}.gfdcache");

                if ((File.Exists(ImagePath) == false) || (IsRefresh == true))
                {
                    using (WebClient wc = new WebClient())
                    {
                        await Task.Run(async () => { await wc.DownloadFileTaskAsync(Path.Combine(ETC.server, "Data", "Images", "Enemy", "Normal", $"{EnemyCodeName}.png"), ImagePath); });
                    }
                }

                await Task.Delay(500);

                EnemyImageView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(ImagePath));

                ImageStatus.Text = $"{EnemyCodeName} - {EnemyName}";
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