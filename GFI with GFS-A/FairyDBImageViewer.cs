﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme="@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FairyDBImageViewer : AppCompatActivity
    {
        private DataRow FairyInfoDR = null;
        private string FairyName = "";

        private CoordinatorLayout SnackbarLayout;
        private ProgressBar LoadProgressBar;
        private ImageView FairyImageView;
        private TextView ImageStatus;
        private Button NextButton;
        private Button PreviousButton;
        private TextView FairyNumStatus;

        private int ImageNum = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.FairyDB_ImageViewer);

                FairyInfoDR = ETC.FindDataRow(ETC.FairyList, "Name", Intent.GetStringExtra("Keyword"));
                FairyName = (string)FairyInfoDR["Name"];

                FairyImageView = FindViewById<ImageView>(Resource.Id.FairyDBImageViewerImageView);
                LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.FairyDBImageViewerLoadProgress);
                ImageStatus = FindViewById<TextView>(Resource.Id.FairyDBImageViewerImageStatus);
                NextButton = FindViewById<Button>(Resource.Id.FairyDBImageViewerNextButton);
                PreviousButton = FindViewById<Button>(Resource.Id.FairyDBImageViewerPreviousButton);
                FairyNumStatus = FindViewById<TextView>(Resource.Id.FairyDBImageViewerImageNum);

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FairyDBImageViewerSnackbarLayout);

                InitProcess();

                LoadImage();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void InitProcess()
        {
            try
            {
                FairyNumStatus.Text = ImageNum.ToString();

                NextButton.Click += FairyImageNumChangeButton_Click;
                PreviousButton.Click += FairyImageNumChangeButton_Click;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "초기화 오류 발생!", Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private void FairyImageNumChangeButton_Click(object sender, EventArgs e)
        {
            try
            {
                Button b = sender as Button;

                switch (b.Id)
                {
                    case Resource.Id.FairyDBImageViewerNextButton:
                        if (ImageNum == 3) ImageNum = 1;
                        else ImageNum += 1;
                        break;
                    case Resource.Id.FairyDBImageViewerPreviousButton:
                        if (ImageNum == 1) ImageNum = 3;
                        else ImageNum -= 1;
                        break;
                }

                FairyNumStatus.Text = ImageNum.ToString();

                LoadImage();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "이미지 전환 오류 발생!", Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async void LoadImage()
        {
            try
            {
                LoadProgressBar.Visibility = ViewStates.Visible;

                await Task.Delay(100);

                string ImageName = FairyName + "_" + ImageNum;

                string ImagePath = Path.Combine(ETC.CachePath, "Fairy", "Normal", ImageName + ".gfdcache");

                if (File.Exists(ImagePath) == false)
                {
                    using (WebClient wc = new WebClient())
                    {
                        await Task.Run(async () => { await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Fairy", ImageName + ".png"), ImagePath); });
                    }
                }


                FairyImageView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(ImagePath));

                StringBuilder sb = new StringBuilder();
                sb.Append(FairyName);
                sb.Append(" - ");
                sb.Append(ImageNum);
                sb.Append("단계");

                ImageStatus.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                LoadProgressBar.Visibility = ViewStates.Invisible;
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