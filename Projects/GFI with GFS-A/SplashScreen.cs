using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Widget;

using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFI_with_GFS_A
{
    [MetaData("android.app.shortcuts", Resource = "@xml/appshortcut")]
    [Activity(MainLauncher = true, Label = "@string/App_TitleName", Theme = "@style/GFS.Splash", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : BaseAppCompatActivity
    {
        private CoordinatorLayout SnackbarLayout;
        private ImageView SplashImageView;
        private TextView StatusText;

        private ISharedPreferencesEditor PreferenceEditor;

        protected override void AttachBaseContext(Context @base)
        {
            ETC.BasicInitializeApp(@base);

            base.AttachBaseContext(ETC.baseContext);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Splash_Light);
                }

                SetContentView(Resource.Layout.SplashLayout);

                //ETC.BasicInitializeApp(this);
                PreferenceEditor = ETC.sharedPreferences.Edit();

                SplashImageView = FindViewById<ImageView>(Resource.Id.SplashImageView);

                Random r = new Random(DateTime.Now.Millisecond);

                if ((ETC.sharedPreferences.GetInt("SplashBG_Index", 0) == 1) || ((r.Next() % 20) == 0))
                    SplashImageView.SetImageResource(Resource.Drawable.Splash_Special);
                else
                    SplashImageView.SetImageResource(Resource.Drawable.SplashBG2);

                StatusText = FindViewById<TextView>(Resource.Id.SplashStatusText);

                // Check Permission

                if (int.Parse(Build.VERSION.Release.Split('.')[0]) >= 6)
                    CheckPermission();
                else
                    _ = InitProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private async Task InitProcess()
        {
            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.SplashSnackbarLayout);

            await Task.Delay(500);

            try
            {
                // Initialize

                FindViewById<TextView>(Resource.Id.SplashAppVersion).Text = $"v{AppInfo.VersionString}({AppInfo.BuildString})";

                await ETC.AnimateText(StatusText, "Initializing");

                if (ETC.sharedPreferences.GetBoolean("CheckInitLowMemory", true))
                    CheckDeviceMemory();

                ETC.isLowRAM = ETC.sharedPreferences.GetBoolean("LowMemoryOption", false);
              
                ETC.CheckInitFolder();


                // Check DB Update

                if (!CheckDBFiles())
                {
                    await ETC.AnimateText(StatusText, "Download DB First");

                    try
                    {
                        await ETC.CheckServerNetwork();

                        if (!ETC.isServerDown)
                            await ETC.UpdateDB(this);
                        else
                            throw new Exception("Server is down");
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex, this);
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.Splash_SkipCheckUpdate, 1000, Android.Graphics.Color.DarkBlue);
                    }
                }

                try
                {
                    using (StreamReader sr = new StreamReader(new FileStream(Path.Combine(ETC.systemPath, "DBVer.txt"), FileMode.Open, FileAccess.Read)))
                        int.TryParse(sr.ReadToEnd(), out ETC.dbVersion);
                }
                catch (Exception)
                {
                    ETC.dbVersion = 0;
                }


                // Finalize & Start Main

                SnackbarLayout?.Dispose();
                SplashImageView?.Dispose();
                StatusText?.Dispose();
                PreferenceEditor?.Dispose();

                StartActivity(typeof(Main));
                OverridePendingTransition(Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight);
                Finish();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
                    if (CheckSelfPermission(permission) == Permission.Denied)
                        request.Add(permission);

                request.TrimToSize();

                if (request.Count == 0)
                    _ = InitProcess();
                else
                    RequestPermissions((string[])request.ToArray(typeof(string)), 0);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
                _ = InitProcess();
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
            foreach (string s in ETC.dbFiles)
                if (!File.Exists(Path.Combine(ETC.dbPath, s)))
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

