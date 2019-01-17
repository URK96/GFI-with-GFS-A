using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GFI_with_GFS_A
{
    [MetaData("android.app.shortcuts", Resource = "@xml/appshortcut")]
    [Activity(MainLauncher = true, Theme = "@style/GFS.Splash", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashScreen : AppCompatActivity
    {
        private CoordinatorLayout SnackbarLayout = null;
        private ImageView SplashImageView;
        private TextView StatusText;

        private ISharedPreferencesEditor PreferenceEditor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                ETC.BasicInitializeApp(this);

                PreferenceEditor = ETC.sharedPreferences.Edit();

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Splash_Light);

                SetContentView(Resource.Layout.SplashLayout);
                SetTitle(Resource.String.app_name);

                SplashImageView = FindViewById<ImageView>(Resource.Id.SplashImageView);

                Random r = new Random(DateTime.Now.Millisecond);

                if ((ETC.sharedPreferences.GetInt("SplashBG_Index", 0) == 1) || ((r.Next() % 20) == 0))
                    SplashImageView.SetImageResource(Resource.Drawable.Splash_Special);
                else SplashImageView.SetImageResource(Resource.Drawable.SplashBG2);

                StatusText = FindViewById<TextView>(Resource.Id.SplashStatusText);

                // Check Permission

                if ((int.Parse(Build.VERSION.Release.Split('.')[0])) >= 6) CheckPermission();
                else InitProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private async Task Animation()
        {
            while (SplashImageView.Alpha < 1.0f)
            {
                SplashImageView.Alpha += 0.1f;
                await Task.Delay(10);
            }
        }

        private async Task InitProcess()
        {
            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.SplashSnackbarLayout);

            await Task.Delay(500);

            try
            {
                await Animation();
                FindViewById<TextView>(Resource.Id.SplashAppVersion).Text = $"v{AppInfo.VersionString}({AppInfo.BuildString}";


                // Initialize

                await ETC.AnimateText(StatusText, "Initializing");

                if (ETC.sharedPreferences.GetBoolean("CheckInitLowMemory", true) == true) CheckDeviceMemory();
                ETC.IsLowRAM = ETC.sharedPreferences.GetBoolean("LowMemoryOption", false);

                if (VersionTracking.IsFirstLaunchForCurrentBuild == true) PreferenceEditor.PutBoolean("ShowNewFeatureDialog", true);
                
                ETC.CheckInitFolder();


                // Check Server

                /*if (ETC.sharedPreferences.GetBoolean("EnableServerCheck", false) == true)
                {
                    await ETC.AnimateText(StatusText, "Check Server");
                    await ETC.CheckServerNetwork();
                }*/


                // Check DB Update

                if (CheckDBFiles() == false)
                {
                    await ETC.AnimateText(StatusText, "Download DB First");

                    try
                    {
                        await ETC.CheckServerNetwork();

                        if (ETC.IsServerDown == false)
                            await ETC.UpdateDB(this);
                        else
                        {
                            Toast.MakeText(this, Resource.String.NoDBFileFound_Message, ToastLength.Long).Show();
                            FinishAffinity();
                            Process.KillProcess(Process.MyPid());
                        }
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(this, ex.ToString());
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.Splash_SkipCheckUpdate, 1000, Android.Graphics.Color.DarkBlue);
                    }
                }


                // Finalize & Start Main

                using (StreamReader sr = new StreamReader(new FileStream(Path.Combine(ETC.SystemPath, "DBVer.txt"), FileMode.Open, FileAccess.Read)))
                    int.TryParse(sr.ReadToEnd(), out ETC.DBVersion);

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

            var totalRam = memoryInfo.TotalMem / 1024 / 1024;

            if (totalRam <= 2048)
            {
                PreferenceEditor.PutBoolean("LowMemoryOption", true);
                PreferenceEditor.PutBoolean("DBListImageShow", false);
                PreferenceEditor.PutBoolean("DBDetailBackgroundImage", false);
            }

            PreferenceEditor.PutBoolean("CheckInitLowMemory", false);
            PreferenceEditor.Apply();
        }

        private bool CheckDBFiles()
        {
            string[] DBFiles =
            {
                "Doll.gfs",
                "Equipment.gfs",
                "Fairy.gfs",
                "Enemy.gfs",
                "FST.gfs",
                "MDSupportList.gfs",
                "FreeOP.gfs",
                "SkillTraining.gfs",
                "FairyAttribution.gfs"
            };

            foreach (string s in DBFiles)
                if (File.Exists(Path.Combine(ETC.DBPath, s)) == false)
                    return false;

            return true;
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
                Process.KillProcess(Process.MyPid());
            });
            ExitDialog.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            ExitDialog.Show();
        }
    }
}

