﻿using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using System;
using System.Threading.Tasks;

namespace GFDA
{
    [Activity(Label = "ZinaOS", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ZinaOS : BaseFragmentActivity
    {
        private TextView bootView;

        private bool IsBooting = true;

        readonly int[] HiddenMainMenuButtonIds =
        {
            Resource.Id.ZinaOSMainMenuHiddenSettingButton,
            Resource.Id.ZinaOSMainMenuHiddenGalleryButton,
            Resource.Id.ZinaOSMainMenuHiddenEventButton,
            Resource.Id.ZinaOSMainMenuHiddenExtraButton
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ZinaOS);

            bootView = FindViewById<TextView>(Resource.Id.ZinaOSBootTextView);

            BootingZinaOS();
        }

        private async void BootingZinaOS()
        {
            try
            {
                Random r = new Random(DateTime.Now.Second);

                ImageView BootLogo = FindViewById<ImageView>(Resource.Id.ZinaOSMainBootLogo);
                ProgressBar BootProgressBar = FindViewById<ProgressBar>(Resource.Id.ZinaOSLoadProgressBar);

                await Task.Delay(1000);

                string temp_text1 = "Zina Protocol OS [Ver. 1.0.1]\n";

                for (int i = 0; i < temp_text1.Length; ++i)
                {
                    bootView.Text += temp_text1[i];
                    await Task.Delay(5);
                }

                string temp_text2 = "# 20xx I.O.P & GRIFON Corporation #\n\n";

                for (int i = 0; i < temp_text2.Length; ++i)
                {
                    bootView.Text += temp_text2[i];
                    await Task.Delay(5);
                }

                await Task.Delay(1000);

                await AnimateText(bootView, "Checking Hardware", r.Next(3, 6), "Success");

                for (int i = 0; i < 10; ++i)
                {
                    await Task.Delay(r.Next(1, 10) * 100);
                    bootView.Text += "Load Boot Module..." + (i + 1) + " / 10\n";
                }

                await Task.Delay(1000);

                FindViewById<LinearLayout>(Resource.Id.ZinaOSBootTextLayout).Animate().Alpha(0.0f).SetDuration(500).Start();
                FindViewById<LinearLayout>(Resource.Id.ZinaOSBootViewLayout).Animate().Alpha(1.0f).SetDuration(500).Start();

                BootLogo.Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(500).Start();

                await Task.Delay(1500);

                FindViewById<LinearLayout>(Resource.Id.ZinaOSLoadProgressLayout).Animate().Alpha(1.0f).SetDuration(500).Start();

                await Task.Delay(1000);

                BootProgressBar.Indeterminate = false;

                for (int i = 0; i <= 100; ++i)
                {
                    await Task.Delay(r.Next(1, 50));
                    BootProgressBar.Progress = i;
                }

                await Task.Delay(100);

                FindViewById<LinearLayout>(Resource.Id.ZinaOSLoadProgressLayout).Animate().Alpha(0.0f).SetDuration(500).Start();
                FindViewById<LinearLayout>(Resource.Id.ZinaOSBootViewLayout).Animate().Alpha(0.0f).SetDuration(500).SetStartDelay(800).Start();
                FindViewById<LinearLayout>(Resource.Id.ZinaOSMainMenuLayout).Animate().Alpha(1.0f).SetDuration(500).WithStartAction(new Java.Lang.Runnable(delegate { FindViewById<LinearLayout>(Resource.Id.ZinaOSMainMenuLayout).Visibility = ViewStates.Visible; })).SetStartDelay(1500).Start();
                FindViewById<LinearLayout>(Resource.Id.ZinaOSMainMenuLayout).BringToFront();
                SetHiddenMainMenuEvent(1);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
            finally
            {
                IsBooting = false;
            }
        }

        private async Task ShutdownZinaOS()
        {
            try
            {
                ImageView BootLogo = FindViewById<ImageView>(Resource.Id.ZinaOSMainBootLogo);
                ProgressBar BootProgressBar = FindViewById<ProgressBar>(Resource.Id.ZinaOSLoadProgressBar);

                await Task.Delay(1000);

                SetHiddenMainMenuEvent(0);
                FindViewById<LinearLayout>(Resource.Id.ZinaOSMainMenuLayout).Animate().Alpha(0.0f).SetDuration(500).Start();
                FindViewById<LinearLayout>(Resource.Id.ZinaOSBootViewLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(800).Start();

                await Task.Delay(2000);

                BootProgressBar.Indeterminate = true;
                FindViewById<LinearLayout>(Resource.Id.ZinaOSLoadProgressLayout).Animate().Alpha(1.0f).SetDuration(500).Start();

                await Task.Delay(1500);

                FindViewById<LinearLayout>(Resource.Id.ZinaOSLoadProgressLayout).Animate().Alpha(0.0f).SetDuration(500).Start();
                FindViewById<LinearLayout>(Resource.Id.ZinaOSBootViewLayout).Animate().Alpha(0.0f).SetDuration(500).WithEndAction(new Java.Lang.Runnable(delegate { Finish(); })).SetStartDelay(800).Start();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private async Task AnimateText(TextView view, string message, int loopCount)
        {
            view.Text += message;

            for (int i = 0; i < loopCount; ++i)
            {
                view.Text += '.';
                await Task.Delay(500);
            }

            view.Text += "\n";
        }

        private async Task AnimateText(TextView view, string message, int loopCount, string aftermessage)
        {
            view.Text += message;

            for (int i = 0; i < loopCount; ++i)
            {
                view.Text += '.';
                await Task.Delay(500);
            }

            view.Text += aftermessage + "\n";
        }

        private void SetHiddenMainMenuEvent(int mode)
        {
            try
            {
                switch (mode)
                {
                    case 1:
                        foreach (int id in HiddenMainMenuButtonIds)
                        {
                            Button button = FindViewById<Button>(id);
                            button.Click += HiddenMainMenu_Click;
                        }
                        break;
                    case 0:
                    default:
                        foreach (int id in HiddenMainMenuButtonIds)
                        {
                            Button button = FindViewById<Button>(id);
                            if (button.HasOnClickListeners == true) button.Click -= HiddenMainMenu_Click;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private void HiddenMainMenu_Click(object sender, EventArgs e)
        {
            try
            {
                int id = (sender as Button).Id;

                switch (id)
                {
                    case Resource.Id.ZinaOSMainMenuHiddenSettingButton:
                        StartActivity(typeof(HiddenSettingActivity));
                        OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
                        break;
                    case Resource.Id.ZinaOSMainMenuHiddenGalleryButton:
                        break;
                    case Resource.Id.ZinaOSMainMenuHiddenEventButton:
                        break;
                    case Resource.Id.ZinaOSMainMenuHiddenExtraButton:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        public override void OnBackPressed()
        {
            if (IsBooting == true) return;

            var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(this, Resource.Style.GFD_Dialog);
            ad.SetTitle("ShutDown Zina OS");
            ad.SetMessage("Do you want to shutdown Zina OS?");
            ad.SetPositiveButton("Shutdown", delegate
            {
                IsBooting = true;
                _ = ShutdownZinaOS();
            });
            ad.SetNegativeButton("Cancel", delegate { });
            ad.SetCancelable(true);

            try
            {
                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }
    }
}