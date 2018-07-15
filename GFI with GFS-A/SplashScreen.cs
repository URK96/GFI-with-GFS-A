using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using System;
using System.Collections;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GFI_with_GFS_A
{
    [Activity(Label = "소전사전", MainLauncher = true, Theme = "@style/GFS.Splash", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashScreen : AppCompatActivity
    {
        private CoordinatorLayout SnackbarLayout = null;

        private ImageView MainSplashImageView;
        private ImageView LoadImage;

        private ISharedPreferencesEditor PreferenceEditor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                ETC.sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
                PreferenceEditor = ETC.sharedPreferences.Edit();

                ETC.UseLightTheme = ETC.sharedPreferences.GetBoolean("UseLightTheme", false);

                if (ETC.UseLightTheme == true)
                {
                    SetTheme(Resource.Style.GFS_Splash_Light);
                }

                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.SplashLayout);

                MainSplashImageView = FindViewById<ImageView>(Resource.Id.SplashBackImageLayout);
                MainSplashImageView.SetBackgroundResource(Resource.Drawable.SplashBG2);

                LoadImage = FindViewById<ImageView>(Resource.Id.SplashLoadingStatusImage);
                LoadImage.SetImageResource(Resource.Drawable.Splash_DataBuild);

                VersionTracking.Track();

                if ((int.Parse(Build.VERSION.Release.Split('.')[0])) >= 6) CheckPermission();
                else InitProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private async Task InitProcess()
        {
            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.SplashSnackbarLayout);

            await Task.Delay(500);

            try
            {
                FindViewById<LinearLayout>(Resource.Id.SplashLoadingStatusImageLayout).BringToFront();

                LoadImage.BringToFront();

                MainSplashImageView.Animate().Alpha(1.0f).SetDuration(500).Start();
                LoadImage.Animate().Alpha(1.0f).SetDuration(300).SetStartDelay(500).Start();

                //await Task.Delay(1000);

                if (ETC.sharedPreferences.GetBoolean("CheckInitLowMemory", true) == true) CheckDeviceMemory();
                ETC.IsLowRAM = ETC.sharedPreferences.GetBoolean("LowMemoryOption", false);

                if (VersionTracking.IsFirstLaunchForCurrentVersion == true) PreferenceEditor.PutBoolean("ShowNewFeatureDialog", true);

                ETC.CheckInitFolder();

                if (ETC.sharedPreferences.GetBoolean("AutoDBUpdate", true) == true)
                {
                    try
                    {
                        if (await ETC.CheckDBVersion() == true)
                        {
                            await ETC.UpdateDB(this);
                        }
                    }
                    catch (Exception ex)
                    {
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.Splash_SkipCheckUpdate, 1000, Android.Graphics.Color.DarkBlue);
                    }
                }

                LoadImage.Animate().Alpha(0.0f).SetDuration(500).Start();
                LoadImage.SetImageResource(Resource.Drawable.Splash_DataLoad);
                LoadImage.Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(600).Start();

                await Task.Delay(500);

                while (await ETC.LoadDB() == false)
                {
                    ETC.ShowSnackbar(SnackbarLayout, Resource.String.DB_Recovery, Snackbar.LengthShort);

                    await ETC.UpdateDB(this);
                }

                ETC.InitializeAverageAbility();

                if (ETC.UseLightTheme == true)
                {
                    ETC.DialogBG = Resource.Style.GFD_Dialog_Light;
                    ETC.DialogBG_Vertical = Resource.Style.GFD_Dialog_Vertical_Light;
                    ETC.DialogBG_Download = Resource.Style.GFD_Dialog_Download_Light;
                }
                else
                {
                    ETC.DialogBG = Resource.Style.GFD_Dialog;
                    ETC.DialogBG_Vertical = Resource.Style.GFD_Dialog_Vertical;
                    ETC.DialogBG_Download = Resource.Style.GFD_Dialog_Download;
                }

                LoadImage.Animate().Alpha(0.0f).SetDuration(300).Start();

                GC.Collect();
                await Task.Delay(500);

                StartActivity(typeof(Main));
                OverridePendingTransition(Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight);
                Finish();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "앗···! 앱 초기화 실패!", Snackbar.LengthIndefinite, Android.Graphics.Color.DarkRed);
            }
        }

        private void CheckPermission()
        {
            try
            {
                string[] check = { Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage, Manifest.Permission.AccessNetworkState, Manifest.Permission.AccessWifiState };
                ArrayList request = new ArrayList();

                foreach (string permission in check)
                {
                    if (CheckSelfPermission(permission) == Permission.Denied) request.Add(permission);
                }

                request.TrimToSize();

                if (request.Count == 0) InitProcess();
                else RequestPermissions((string[])request.ToArray(typeof(string)), 0);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "권한 부여 실패", Snackbar.LengthIndefinite, Android.Graphics.Color.DarkMagenta);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (grantResults[0] == Permission.Denied)
            {
                Toast.MakeText(this, "해당 권한을 허용하지 않으면 소전사전을 정상적으로 이용하실 수 없습니다.", ToastLength.Short).Show();
                FinishAffinity();
            }
            else
            {
                InitProcess();
            }
        }

        private void CheckDeviceMemory()
        {
            var activityManager = GetSystemService(ActivityService) as ActivityManager;
            var memoryInfo = new ActivityManager.MemoryInfo();
            activityManager.GetMemoryInfo(memoryInfo);

            var totalRam = ((memoryInfo.TotalMem / 1024) / 1024);

            if (totalRam < 2048) PreferenceEditor.PutBoolean("LowMemoryOption", true);

            PreferenceEditor.PutBoolean("CheckInitLowMemory", false);
            PreferenceEditor.Apply();
        }

        public override void OnBackPressed()
        {
            Android.Support.V7.App.AlertDialog.Builder ExitDialog = new Android.Support.V7.App.AlertDialog.Builder(this, Resource.Style.GFD_Dialog);

            ExitDialog.SetTitle(Resource.String.Main_CheckExitTitle);
            ExitDialog.SetMessage(Resource.String.Main_CheckExit);
            ExitDialog.SetCancelable(true);
            ExitDialog.SetPositiveButton("종료", delegate
            {
                FinishAffinity();
                Process.KillProcess(Process.MyPid());
            });
            ExitDialog.SetNegativeButton("취소", delegate { });
            ExitDialog.Show();
        }
    }
}

