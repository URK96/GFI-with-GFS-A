using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Com.Syncfusion.Sfbusyindicator;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Diagnostics;

namespace GFI_with_GFS_A
{
    [MetaData("android.app.shortcuts", Resource = "@xml/appshortcut")]
    [Activity(Label = "소전사전", MainLauncher = true, Theme = "@style/GFS.Splash", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashScreen : AppCompatActivity
    {
        private CoordinatorLayout SnackbarLayout = null;

        private ImageView MainSplashImageView;
        private SfBusyIndicator indicator;
        //private Stopwatch sw = new Stopwatch();

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

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzAxMzlAMzEzNjJlMzMyZTMwaFNuV1Y2bEpzK25pSDlVREdzZHpPcW15TG54Slg3Z3JjQm1IYSs3SENoYz0=");

                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.SplashLayout);

                MainSplashImageView = FindViewById<ImageView>(Resource.Id.SplashBackImageLayout);
                indicator = new SfBusyIndicator(this)
                {
                    AnimationType = Com.Syncfusion.Sfbusyindicator.Enums.AnimationTypes.GearBox,
                    TextColor = Android.Graphics.Color.White
                };
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
                ETC.client = new UptimeSharp.UptimeClient("m780844852-8bd2516bb93800a9eb7e3d58");
                await ETC.CheckServerNetwork();

                indicator.IsBusy = true;

                MobileAds.Initialize(this, "ca-app-pub-4576756770200148~8135834453");

                if (ETC.sharedPreferences.GetBoolean("CheckInitLowMemory", true) == true) CheckDeviceMemory();
                ETC.IsLowRAM = ETC.sharedPreferences.GetBoolean("LowMemoryOption", false);

                if (VersionTracking.IsFirstLaunchForCurrentVersion == true) PreferenceEditor.PutBoolean("ShowNewFeatureDialog", true);
                
                ETC.CheckInitFolder();

                if ((ETC.sharedPreferences.GetBoolean("AutoDBUpdate", true) == true) && (ETC.IsServerDown = true))
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
                        ETC.LogError(this, ex.ToString());
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.Splash_SkipCheckUpdate, 1000, Android.Graphics.Color.DarkBlue);
                    }
                }
                else if (ETC.IsServerDown == true) ETC.ShowSnackbar(SnackbarLayout, Resource.String.Common_ServerMaintenance, 1000, Android.Graphics.Color.DarkBlue);

                ETC.EnableDynamicDB = ETC.sharedPreferences.GetBoolean("DynamicDBLoad", false);

                if (ETC.EnableDynamicDB == false)
                {
                    while (ETC.LoadDB() == false)
                    {
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.DB_Recovery, Snackbar.LengthShort);

                        if (ETC.IsServerDown == false) await ETC.UpdateDB(this);
                        else break;
                    }

                    ETC.InitializeAverageAbility();
                }

                ETC.SetDialogTheme();

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

        private void ReadServerChecking()
        {
            if (ETC.IsServerDown == true) return;

            string url = Path.Combine(ETC.Server, "ServerCheck.txt");
            string s = "";

            try
            {
                using (WebClient wc = new WebClient())
                {
                    string[] temp = wc.DownloadString(url).Split(';');

                    if (temp[0] == "Y") s = temp[1];
                    else return;
                }
            }
            catch (WebException)
            {

            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }

            try
            {
                Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG);
                ad.SetTitle(Resource.String.NotifyCheckServer_Title);
                ad.SetMessage(s);
                ad.SetCancelable(false);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.AlertDialog_Error, Snackbar.LengthShort, Android.Graphics.Color.DarkKhaki);
            }
        }
    }
}

