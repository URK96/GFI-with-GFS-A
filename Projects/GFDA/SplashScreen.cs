using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;

using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFDA
{
    [MetaData("android.app.shortcuts", Resource = "@xml/appshortcut")]
    [Activity(MainLauncher = true, Label = "@string/App_TitleName", Theme = "@style/GFS.Splash", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : BaseAppCompatActivity
    {
        private TextView statusText;

        private ISharedPreferencesEditor preferenceEditor;

        protected override void AttachBaseContext(Context @base)
        {
            try
            {
                ETC.BasicInitializeApp(@base);

                base.AttachBaseContext(ETC.baseContext);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                Toast.MakeText(this, "Fail Basic Initialize", ToastLength.Long).Show();
            }
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Toolbar_Light);
                }
                else
                {
                    SetTheme(Resource.Style.GFS_Toolbar);
                }

                SetContentView(Resource.Layout.SplashLayout);

                Platform.Init(this, savedInstanceState);

                //preferenceEditor = ETC.sharedPreferences.Edit();

                /*Random r = new Random(DateTime.Now.Millisecond);

                if ((ETC.sharedPreferences.GetInt("SplashBG_Index", 0) == 1) || ((r.Next() % 20) == 0))
                {
                    SplashImageView.SetImageResource(Resource.Drawable.Splash_Special);
                }
                else
                {
                    SplashImageView.SetImageResource(Resource.Drawable.SplashBG2);
                }*/
                statusText = FindViewById<TextView>(Resource.Id.SplashStatusText);

                // Check Permission

                if (int.Parse(Build.VERSION.Release.Split('.')[0]) >= 6)
                {
                    await CheckPermission();
                }
                else
                {
                    _ = InitProcess();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private async Task InitProcess()
        {
            await Task.Delay(500);

            try
            {
                // Initialize

                FindViewById<TextView>(Resource.Id.SplashAppVersion).Text = $"v{AppInfo.VersionString}({AppInfo.BuildString})";

                await ETC.AnimateText(statusText, "Initializing");

                if (Preferences.Get("CheckInitLowMemory", true))
                {
                    CheckDeviceMemory();
                }

                ETC.isLowRAM = Preferences.Get("LowMemoryOption", false);
              
                ETC.CheckInitFolder();


                // Check DB Update

                if (!CheckDBFiles())
                {
                    await ETC.AnimateText(statusText, "Download DB First");

                    try
                    {
                        await ETC.CheckServerNetwork();

                        if (!ETC.isServerDown)
                        {
                            await ETC.UpdateDB(this);
                        }
                        else
                        {
                            throw new Exception("Server is down");
                        }
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex, this);
                        Toast.MakeText(this, Resource.String.Splash_SkipCheckUpdate, ToastLength.Long).Show();
                    }
                }

                try
                {
                    int.TryParse(File.ReadAllText(Path.Combine(ETC.systemPath, "DBVer.txt")), out ETC.dbVersion);

                    //using (StreamReader sr = new StreamReader(new FileStream(, FileMode.Open, FileAccess.Read)))
                    //{
                    //    _ = int.TryParse(sr.ReadToEnd(), out ETC.dbVersion);
                    //}
                }
                catch
                {
                    ETC.dbVersion = 0;
                }


                // Finalize & Start Main

                StartActivity(typeof(Main));
                OverridePendingTransition(Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight);
                Finish();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.InitLoad_Error, ToastLength.Long).Show();
            }
            finally
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, false, false);
            }
        }

        private async Task CheckPermission()
        {
            try
            {
                await Permissions.RequestAsync<Permissions.StorageRead>();
                await Permissions.RequestAsync<Permissions.StorageWrite>();
                await Permissions.RequestAsync<Permissions.NetworkState>();

                //string[] check = { Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage, Manifest.Permission.AccessNetworkState, Manifest.Permission.AccessWifiState };
                //ArrayList request = new ArrayList();

                //foreach (string permission in check)
                //{
                //    if (CheckSelfPermission(permission) == Permission.Denied)
                //    {
                //        request.Add(permission);
                //    }
                //}

                //request.TrimToSize();

                //if (request.Count == 0)
                //{
                //    _ = InitProcess();
                //}
                //else
                //{
                //    RequestPermissions((string[])request.ToArray(typeof(string)), 0);
                //}
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Permission_Error, ToastLength.Long).Show();
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
                preferenceEditor.PutBoolean("LowMemoryOption", true);
                preferenceEditor.PutBoolean("DBListImageShow", false);
                preferenceEditor.PutBoolean("DBDetailBackgroundImage", false);
            }

            preferenceEditor.PutBoolean("CheckInitLowMemory", false);
            preferenceEditor.Apply();
        }

        private bool CheckDBFiles()
        {
            foreach (string s in ETC.dbFiles)
            {
                if (!File.Exists(Path.Combine(ETC.dbPath, s)))
                {
                    return false;
                }
            }

            return true;
        }

        public override void OnBackPressed()
        {
            var exitDialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this, Resource.Style.GFD_Dialog);

            exitDialog.SetTitle(Resource.String.Main_CheckExitTitle);
            exitDialog.SetMessage(Resource.String.Main_CheckExit);
            exitDialog.SetCancelable(true);
            exitDialog.SetPositiveButton(Resource.String.AlertDialog_Exit, delegate
            {
                FinishAffinity();
                Process.KillProcess(Process.MyPid());
            });
            exitDialog.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            exitDialog.Show();
        }
    }
}

