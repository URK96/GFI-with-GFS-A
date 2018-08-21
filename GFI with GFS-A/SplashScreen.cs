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
using Android.Gms.Ads;
using System.Diagnostics;
using Android.Graphics.Drawables;
using Com.Syncfusion.Sfbusyindicator;

namespace GFI_with_GFS_A
{
    [Activity(Label = "소전사전", MainLauncher = true, Theme = "@style/GFS.Splash", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashScreen : AppCompatActivity
    {
        private CoordinatorLayout SnackbarLayout = null;

        private ImageView MainSplashImageView;
        private SfBusyIndicator indicator;
        private Stopwatch sw = new Stopwatch();

        private ISharedPreferencesEditor PreferenceEditor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                ETC.sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
                PreferenceEditor = ETC.sharedPreferences.Edit();
                ETC.Resources = Resources;

                ETC.UseLightTheme = ETC.sharedPreferences.GetBoolean("UseLightTheme", false);

                if (ETC.UseLightTheme == true)
                {
                    SetTheme(Resource.Style.GFS_Splash_Light);
                }

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzY0NkAzMTM2MmUzMjJlMzBmNFFDVVZlU2NDRTVmYVJqQ0ZyOTVPOGhYWnFIazlQNFNPeGVEMU9WMjZnPQ==");

                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.SplashLayout);

                MainSplashImageView = FindViewById<ImageView>(Resource.Id.SplashBackImageLayout);
                indicator = new SfBusyIndicator(this);
                indicator.AnimationType = Com.Syncfusion.Sfbusyindicator.Enums.AnimationTypes.GearBox;
                indicator.TextColor = Android.Graphics.Color.White;
                indicator.SetBackgroundResource(Resource.Drawable.SplashBG2);
                FindViewById<FrameLayout>(Resource.Id.SplashBusyIndicatorLayout).AddView(indicator);

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
                indicator.IsBusy = true;

                MobileAds.Initialize(this, "ca-app-pub-4576756770200148~8135834453");

                if (ETC.sharedPreferences.GetBoolean("CheckInitLowMemory", true) == true) CheckDeviceMemory();
                ETC.IsLowRAM = ETC.sharedPreferences.GetBoolean("LowMemoryOption", false);

                if (VersionTracking.IsFirstLaunchForCurrentVersion == true) PreferenceEditor.PutBoolean("ShowNewFeatureDialog", true);
                
                ETC.CheckInitFolder();

                if (System.IO.File.Exists(System.IO.Path.Combine(ETC.DBPath, "FST.gfs")) == false) await ETC.UpdateDB(this);

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

                ETC.EnableDynamicDB = ETC.sharedPreferences.GetBoolean("DynamicDBLoad", false);

                if (ETC.EnableDynamicDB == false)
                {
                    while (await ETC.LoadDB() == false)
                    {
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.DB_Recovery, Snackbar.LengthShort);

                        await ETC.UpdateDB(this);
                    }

                    ETC.InitializeAverageAbility();
                }

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

                StartActivity(typeof(Main));
                OverridePendingTransition(Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight);
                Finish();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.InitLoad_Error, Snackbar.LengthIndefinite, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, false, false);
            }
        }

        private async Task ImageAnimation()
        {
            ClipDrawable drawable = (ClipDrawable)MainSplashImageView.Drawable;

            for (int i = 0; i <= 10000; i += 200)
            {
                drawable.SetLevel(i);
                MainSplashImageView.SetImageDrawable(drawable);
                await Task.Delay(10);
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
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Permission_Error, Snackbar.LengthIndefinite, Android.Graphics.Color.DarkMagenta);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (grantResults[0] == Permission.Denied)
            {
                Toast.MakeText(this, Resource.String.PermissionDeny_Message, ToastLength.Short).Show();
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

            if (totalRam <= 2048)
            {
                PreferenceEditor.PutBoolean("LowMemoryOption", true);
                PreferenceEditor.PutBoolean("DBListImageShow", false);
                PreferenceEditor.PutBoolean("DBDetailBackgroundImage", false);
            }

            PreferenceEditor.PutBoolean("CheckInitLowMemory", false);
            PreferenceEditor.Apply();
        }

        public override void OnBackPressed()
        {
            Android.Support.V7.App.AlertDialog.Builder ExitDialog = new Android.Support.V7.App.AlertDialog.Builder(this, Resource.Style.GFD_Dialog);

            ExitDialog.SetTitle(Resource.String.Main_CheckExitTitle);
            ExitDialog.SetMessage(Resource.String.Main_CheckExit);
            ExitDialog.SetCancelable(true);
            ExitDialog.SetPositiveButton(Resource.String.AlertDialog_Exit, delegate
            {
                FinishAffinity();
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            });
            ExitDialog.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            ExitDialog.Show();
        }
    }
}

