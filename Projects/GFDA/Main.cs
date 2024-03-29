using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Transitions;

using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Snackbar;

using System;
using System.Threading.Tasks;

using Xamarin.Essentials;

using Resource = GFDA.Resource;

namespace GFDA
{
    [Activity(Label = "GFD", Theme = "@style/GFS.Toolbar", ScreenOrientation = ScreenOrientation.Portrait)]
    public partial class Main : BaseAppCompatActivity
    {
        System.Timers.Timer exitTimer = new System.Timers.Timer();

        private CoordinatorLayout snackbarLayout;
        private AndroidX.AppCompat.Widget.Toolbar toolbar;
        private FrameLayout fContainer;
        private BottomNavigationView bottomNavigation;

        private AndroidX.Fragment.App.Fragment mainHomeF;
        private AndroidX.Fragment.App.Fragment mainDBF;
        private AndroidX.Fragment.App.Fragment mainGFDv1F;
        private AndroidX.Fragment.App.Fragment mainGFUtilF;
        private AndroidX.Fragment.App.Fragment mainOtherF;

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(ETC.baseContext);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Toolbar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.MainLayout);


                // Find View & Connect Event

                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.MainMainToolbar);
                fContainer = FindViewById<FrameLayout>(Resource.Id.MainFragmentContainer);
                bottomNavigation = FindViewById<BottomNavigationView>(Resource.Id.MainBottomNavigation);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetTitle(Resource.String.MainActivity_Title);

                bottomNavigation.ItemSelected += (sender, e) => { ChangeFragment(e.Item.ItemId); };

                // Set Fragment

                mainHomeF = new HomeFragment();
                mainDBF = new DBFragment();
                mainGFDv1F = new GFDv1Fragment();
                mainGFUtilF = new GFUtilFragment();
                mainOtherF = new OtherFragment();

                // Set Program Exit Timer

                exitTimer.Interval = 2000;
                exitTimer.Elapsed += delegate { exitTimer.Stop(); };

                // Load Init Process

                int startIndex = int.Parse(Preferences.Get("StartMainFragment", "0"));

                bottomNavigation.SelectedItemId = startIndex switch
                {
                    1 => Resource.Id.MainNavigation_DB,
                    2 => Resource.Id.MainNavigation_GFDv1,
                    3 => Resource.Id.MainNavigation_GFUtil,
                    4 => Resource.Id.MainNavigation_Other,
                    _ => Resource.Id.MainNavigation_Home,
                };

                _ = InitializeProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MainToolbarMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Resource.Id.MainToolbar_GFDInfo:
                    StartActivity(typeof(GFDInfoActivity));
                    OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
                    break;
                case Resource.Id.MainToolbar_Setting:
                    StartActivity(typeof(SettingActivity));
                    OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void ChangeFragment(int id)
        {
            var fragment = id switch
            {
                Resource.Id.MainNavigation_Home => mainHomeF,
                Resource.Id.MainNavigation_DB => mainDBF,
                Resource.Id.MainNavigation_GFDv1 => mainGFDv1F,
                Resource.Id.MainNavigation_GFUtil => mainGFUtilF,
                Resource.Id.MainNavigation_Other => mainOtherF,
                _ => null,
            };

            if (fragment == null)
            {
                return;
            }

            TransitionManager.BeginDelayedTransition(fContainer);

            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.MainFragmentContainer, fragment).Commit();

            GC.Collect();
        }

        protected override async void OnResume()
        {
            try
            {
                base.OnResume();

                // Refresh Server Data

                await ETC.CheckServerNetwork();

                _ = CheckNetworkData();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        // Auto Run Mode
        private void RunStartMode()
        {
            switch (Preferences.Get("StartAppMode", "0"))
            {
                case "1":
                    ChangeFragment(Resource.Id.MainNavigation_DB); // DB Sub Menu
                    break;
                case "2":
                    var oldGFDIntent = new Intent(this, typeof(OldGFDViewer));
                    oldGFDIntent.PutExtra("Index", 0);
                    StartActivity(oldGFDIntent);
                    OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut); // OldGFD
                    break;
                case "3":
                    (mainGFUtilF as GFUtilFragment).Adapter_ItemClick(null, 4); // Area Tip
                    break;
                case "4":
                    (mainGFUtilF as GFUtilFragment).Adapter_ItemClick(null, 3); // Calc
                    break;
                case "5":
                    (mainGFUtilF as GFUtilFragment).Adapter_ItemClick(null, 1); // Event
                    break;
                case "6":
                    (mainGFUtilF as GFUtilFragment).Adapter_ItemClick(null, 0); // Offical Notification
                    break;
                case "7":
                    (mainOtherF as OtherFragment).Adapter_ItemClick(null, 2); // OST Player
                    break;
                default:
                    break;
            }
        }

        private async Task InitializeProcess()
        {
            GC.Collect();
            await Task.Delay(100);

            try
            {
                // Check Auto Run Mode

                if (Preferences.Get("StartAppMode", "0") != "0")
                {
                    RunStartMode();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Main menu initialize error", Snackbar.LengthShort);
            }
        }

        /// <summary>
        /// Check DB version & Refresh notification data
        /// </summary>
        private async Task CheckNetworkData()
        {
            await Task.Delay(100);

            //tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBChecking)})";

            try
            {
                // Check Server Status


                /*await Task.Run(async () =>
                {
                    // Check DB Version

                    if (await ETC.CheckDBVersion())
                    {
                        //RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateAvailable)})"; });

                        var ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBG);
                        ad.SetTitle(Resource.String.CheckDBUpdateDialog_Title);
                        ad.SetMessage(Resource.String.CheckDBUpdateDialog_Question);
                        ad.SetCancelable(true);
                        ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                        ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, async delegate
                        {
                            await ETC.UpdateDB(this, true);

                            if (!await ETC.CheckDBVersion())
                            {
                                //RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateNewest)})"; });
                            }
                            else
                            {
                                //RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateAvailable)})"; });
                            }

                        });

                        RunOnUiThread(() => { ad.Show(); });
                    }
                    else
                    {
                        //RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateNewest)})"; });
                    }
                });*/

                if (await ETC.CheckDBVersion())
                {
                    //RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateAvailable)})"; });

                    var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(this, ETC.dialogBG);
                    ad.SetTitle(Resource.String.CheckDBUpdateDialog_Title);
                    ad.SetMessage(Resource.String.CheckDBUpdateDialog_Question);
                    ad.SetCancelable(true);
                    ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                    ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, async delegate
                    {
                        await ETC.UpdateDB(this, true);

                        if (!await ETC.CheckDBVersion())
                        {
                            //RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateNewest)})"; });
                        }
                        else
                        {
                            //RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateAvailable)})"; });
                        }

                    });

                    RunOnUiThread(() => { ad.Show(); });
                }
                else
                {
                    //RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateNewest)})"; });
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Main_NotificationInitializeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        public override void OnBackPressed()
        {
            if (!exitTimer.Enabled)
            {
                exitTimer.Start();
                Toast.MakeText(this, Resource.String.Main_CheckExit, ToastLength.Short).Show();
            }
            else
            {
                FinishAffinity();
                OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
                Process.KillProcess(Process.MyPid());
            }
        }

        private void CheckPermission(string permission)
        {
            try
            {
                if (CheckSelfPermission(permission) == Permission.Denied)
                {
                    RequestPermissions(new string[] { permission }, 0);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                Toast.MakeText(this, Resource.String.Permission_Error, ToastLength.Short).Show();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (grantResults[0] == Permission.Denied)
            {
                Toast.MakeText(this, Resource.String.PermissionDeny_Message, ToastLength.Short).Show();
            }
        }
    }
}