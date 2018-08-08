using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "소전사전", Theme = "@style/GFS", ScreenOrientation = ScreenOrientation.Portrait)]
    public class Main : AppCompatActivity
    {
        System.Timers.Timer ExitTimer = new System.Timers.Timer();

        CoordinatorLayout SnackbarLayout = null;

        private TextView NotificationView = null;

        private bool SubMenuMode = false;

        readonly int[] MainMenuButtonIds =
        {
            Resource.Id.DollDBMainButton,
            Resource.Id.EquipmentDBMainButton,
            Resource.Id.FairyDBMainButton,
            Resource.Id.EnemyDBMainButton,
            Resource.Id.OldGFDMainButton,
            Resource.Id.CalcMainButton,
            Resource.Id.GFDInfoMainButton,
            Resource.Id.ExtraMainButton,
            Resource.Id.SettingMainButton
        };
        readonly int[] MainMenuButtonBackgroundIds = 
        {
            Resource.Drawable.Main_DollDBSelector,
            Resource.Drawable.Main_EquipDBSelector,
            Resource.Drawable.Main_FairyDBSelector,
            Resource.Drawable.Main_EnemyDBSelector,
            Resource.Drawable.Main_OldGFDSelector,
            Resource.Drawable.Main_CalcSelector,
            Resource.Drawable.Main_GFDInfoSelector,
            Resource.Drawable.Main_ExtraSelector,
            Resource.Drawable.Main_SettingSelector
        };
        readonly string[] MainMenuButtonText = 
        {
            "인형 DB",
            "장비 DB",
            "요정 DB",
            "철혈 DB",
            "소전사전v1 이미지",
            "계산기",
            "정보",
            "추가 기능",
            "설정"
        };

        readonly int[] ExtraMenuButtonIds = 
        {
            Resource.Id.EventExtraButton,
            Resource.Id.GFNewsExtraButton,
            Resource.Id.RFBotExtraButton,
            Resource.Id.ProductSimulatorExtraButton,
            Resource.Id.FormationSimulatorExtraButton
        };
        readonly int[] ExtraMenuButtonBackgroundIds = 
        {
            Resource.Drawable.Extra_EventSelector,
            Resource.Drawable.Extra_GFNewsSelector,
            Resource.Drawable.Extra_RFBotSelector,
            Resource.Drawable.Extra_ProductSimulatorSelector,
            Resource.Drawable.Extra_FormationSimulatorSelector
        };
        readonly string[] ExtraMenuButtonText =
        {
            "이벤트",
            "소전 공지사항",
            "라플봇",
            "제조 시뮬레이터",
            "진형 시뮬레이터"
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.MainLayout);

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.MainSnackbarLayout);

                NotificationView = FindViewById<TextView>(Resource.Id.MainNotificationText);
                NotificationView.Click += NotificationView_Click;

                SupportActionBar.SetIcon(int.Parse(ETC.sharedPreferences.GetString("MainActionbarIcon", Resource.Drawable.AppIcon2.ToString())));
                SupportActionBar.SetDisplayShowHomeEnabled(true);

                ExitTimer.Interval = 2000;
                ExitTimer.Elapsed += ExitTimer_Elapsed;

                RunStartMode();
                InitializeProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private async Task RunStartMode()
        {
            switch (ETC.sharedPreferences.GetInt("StartAppMode", 0))
            {
                case 1:
                    MainMenuButton_Click(Resource.Id.OldGFDMainButton, new EventArgs());
                    break;
                case 2:
                    ExtraMenuButton_Click(Resource.Id.RFBotExtraButton, new EventArgs());
                    break;
            }
        }

        private void NotificationView_Click(object sender, EventArgs e)
        {
            string notification = "";

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Vertical);
            ad.SetTitle(Resource.String.Main_NotificationTitle);
            ad.SetCancelable(true);
            ad.SetPositiveButton("닫기", delegate { });

            try
            {
                string url = Path.Combine(ETC.Server, "Android_Notification.txt");

                using (WebClient wc = new WebClient())
                {
                    notification = wc.DownloadString(url);
                }

                ad.SetMessage(notification);
                ad.Show();
            }
            catch (WebException ex)
            {
                ETC.LogError(this, ex.ToString());

                ad.SetMessage(Resource.String.Main_NotificationLoadFail);
                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Main_NotificationInitializeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task InitializeProcess()
        {
            GC.Collect();

            try
            {
                if (ETC.sharedPreferences.GetBoolean("LowMemoryOption", false) == false)
                {
                    for (int i = 0; i < MainMenuButtonIds.Length; ++i) FindViewById<Button>(MainMenuButtonIds[i]).SetBackgroundResource(MainMenuButtonBackgroundIds[i]);
                    for (int i = 0; i < ExtraMenuButtonIds.Length; ++i) FindViewById<Button>(ExtraMenuButtonIds[i]).SetBackgroundResource(ExtraMenuButtonBackgroundIds[i]);
                }
                else
                {
                    for (int i = 0; i < MainMenuButtonIds.Length; ++i) FindViewById<Button>(MainMenuButtonIds[i]).Text = MainMenuButtonText[i];
                    for (int i = 0; i < ExtraMenuButtonIds.Length; ++i) FindViewById<Button>(ExtraMenuButtonIds[i]).Text = ExtraMenuButtonText[i];
                }

                SetMainMenuEvent(1);

                foreach (int id in MainMenuButtonIds)
                {
                    Button button = FindViewById<Button>(id);
                    button.Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(500).Start();
                    await Task.Delay(100);
                }

                SetEventImage();

                //ETC.ShowSnackbar(SnackbarLayout, Resource.String.Main_StartUpHello, Snackbar.LengthShort, Android.Graphics.Color.DarkCyan);

                FindViewById<LinearLayout>(Resource.Id.MainMenuButtonLayout1).BringToFront();

                if(ETC.sharedPreferences.GetBoolean("ShowNewFeatureDialog", true) == true) ShowNewVersionFeatureDialog();

                ReadServerChecking();
                LoadTopNotification();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "Init error", Snackbar.LengthShort);
            }
        }

        private void ShowNewVersionFeatureDialog()
        {
            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Vertical);
            ad.SetTitle(Resource.String.NewFeatureDialog_Title);
            ad.SetMessage(Resource.String.NewFeature);
            ad.SetCancelable(true);
            ad.SetPositiveButton("닫기", delegate { });

            try
            {
                ad.Show();
                ISharedPreferencesEditor Editor = ETC.sharedPreferences.Edit();
                Editor.PutBoolean("ShowNewFeatureDialog", false);
                Editor.Apply();
            }
            catch (WebException ex)
            {
                ETC.LogError(this, ex.ToString());
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Main_NotificationInitializeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private void SetEventImage()
        {
            try
            {
                Button EventButton = FindViewById<Button>(Resource.Id.EventExtraButton);

                //if (ETC.HasEvent == true) EventButton.Background = Android.Graphics.Drawables.Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Event", "Images", "Event_1.png"));
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "Init error", Snackbar.LengthShort);
            }
        }

        private void SetMainMenuEvent(int mode)
        {
            try
            {
                switch (mode)
                {
                    case 1:
                        foreach (int id in MainMenuButtonIds)
                        {
                            Button button = FindViewById<Button>(id);
                            button.Click += MainMenuButton_Click;
                        }
                        break;
                    case 0:
                    default:
                        foreach (int id in MainMenuButtonIds)
                        {
                            Button button = FindViewById<Button>(id);
                            if (button.HasOnClickListeners == true) button.Click -= MainMenuButton_Click;
                        }
                        break;
                }              
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "Button Event Link Error", Snackbar.LengthShort);
            }
        }

        private void SetExtraMenuEvent(int mode)
        {
            try
            {
                switch (mode)
                {
                    case 1:
                        foreach (int id in ExtraMenuButtonIds)
                        {
                            Button button = FindViewById<Button>(id);
                            button.Click += ExtraMenuButton_Click;
                        }
                        break;
                    case 0:
                    default:
                        foreach (int id in ExtraMenuButtonIds)
                        {
                            Button button = FindViewById<Button>(id);
                            if (button.HasOnClickListeners == true) button.Click -= ExtraMenuButton_Click;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "Extra Button Event Link Error", Snackbar.LengthShort);
            }
        }

        private async Task ReadServerChecking()
        {
            string url = Path.Combine(ETC.Server, "ServerCheck.txt");
            bool HasCheck = false;
            string s = "";

            try
            {
                using (WebClient wc = new WebClient())
                {
                    string[] temp = (await wc.DownloadStringTaskAsync(url)).Split(';');

                    if (temp[0] == "Y")
                    {
                        HasCheck = true;
                        s = temp[1];
                    }
                    else HasCheck = false;
                }
            }
            catch (WebException ex)
            {
                
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }

            if (HasCheck == true)
            {
                try
                {
                    Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG);
                    ad.SetTitle(Resource.String.NotifyCheckServer_Title);
                    ad.SetMessage(s);
                    ad.SetCancelable(false);
                    ad.SetPositiveButton("확인", delegate { });

                    ad.Show();
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                    ETC.ShowSnackbar(SnackbarLayout, Resource.String.AlertDialog_Error, Snackbar.LengthShort, Android.Graphics.Color.DarkKhaki);
                }
            }
        }

        private async Task LoadTopNotification()
        {
            await Task.Delay(100);
            string url = Path.Combine(ETC.Server, "Android_Notification.txt");
            string notification = "";

            using (WebClient wc = new WebClient())
            {
                try
                {
                    notification = await wc.DownloadStringTaskAsync(url);

                    NotificationView.Text = notification;
                    NotificationView.Selected = true;
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                    ETC.ShowSnackbar(SnackbarLayout, Resource.String.Main_NotificationInitializeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
                }
            } 
        }

        private void MainMenuButton_Click(object sender, EventArgs e)
        {
            try
            {
                int id = (sender as Button).Id;

                switch (id)
                {
                    case Resource.Id.DollDBMainButton:
                        StartActivity(typeof(DollDBMainActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.EquipmentDBMainButton:
                        StartActivity(typeof(EquipDBMainActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.FairyDBMainButton:
                        StartActivity(typeof(FairyDBMainActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.EnemyDBMainButton:
#if DEBUG
                        //StartActivity(typeof(EnemyDBMainActivity));
                        StartActivity(typeof(EnemyDBMainActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
#else
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.DevMode, Snackbar.LengthShort, Android.Graphics.Color.DarkMagenta);
#endif
                        break;
                    case Resource.Id.OldGFDMainButton:
                        StartActivity(typeof(OldGFDViewer));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.CalcMainButton:
                        StartActivity(typeof(CalcMainActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.GFDInfoMainButton:
                        StartActivity(typeof(GFDInfoActivity));
                        OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
                        break;
                    case Resource.Id.ExtraMainButton:
                        SwitchExtraMenu(1);
                        break;
                    case Resource.Id.SettingMainButton:
                        StartActivity(typeof(SettingActivity));
                        OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
                        break;
                    default:
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.AbnormalAccess, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private void ExtraMenuButton_Click(object sender, EventArgs e)
        {
            try
            {
                int id = (sender as Button).Id;

                switch (id)
                {
                    case Resource.Id.RFBotExtraButton:
                        StartActivity(typeof(RFBotMainActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.GFNewsExtraButton:
                        string news_url = "http://www.girlsfrontline.co.kr/archives/category/news";
                        var intent = new Intent(this, typeof(WebBrowserActivity));
                        intent.PutExtra("url", news_url);
                        StartActivity(intent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.EventExtraButton:
                        if ((int.Parse(Build.VERSION.Release.Split('.')[0])) >= 6) CheckPermission(Manifest.Permission.Internet);
                        StartActivity(typeof(EventListActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.ProductSimulatorExtraButton:
#if RELEASE
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.DevMode, Snackbar.LengthShort, Android.Graphics.Color.DarkMagenta);
#else
                        StartActivity(typeof(ProductSimulatorCategorySelectActivity));
#endif
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.FormationSimulatorExtraButton:
                        string pack_name = "com.Cosmos.GfTileSim";
                        Intent AppIntent = PackageManager.GetLaunchIntentForPackage(pack_name);
                        if (AppIntent != null) StartActivity(AppIntent);
                        else
                        {
                            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG);
                            ad.SetTitle("연결할 앱이 없네요 :(");
                            ad.SetMessage("이 기능을 사용하려면 외부 앱이 설치되어야 해요. 설치 화면으로 이동할건가요?");
                            ad.SetCancelable(true);
                            ad.SetPositiveButton("설치", delegate 
                            {
                                string url = "https://play.google.com/store/apps/details?id=" + pack_name;
                                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
                            });
                            ad.SetNegativeButton("취소", delegate { });

                            ad.Show();
                        }
                        break;
                    default:
                        ETC.ShowSnackbar(SnackbarLayout, Resource.String.AbnormalAccess, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private void SwitchExtraMenu(int mode)
        {
            try
            {
                LinearLayout MainMenuLayout = FindViewById<LinearLayout>(Resource.Id.MainMenuButtonLayout1);
                LinearLayout ExtraMenuLayout = FindViewById<LinearLayout>(Resource.Id.ExtraMenuButtonLayout);

                switch (mode)
                {
                    case 0:
                    default:
                        SetMainMenuEvent(1);
                        SetExtraMenuEvent(0);
                        ExtraMenuLayout.Animate().Alpha(0.0f).SetDuration(500).Start();
                        MainMenuLayout.Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(200).Start();
                        MainMenuLayout.BringToFront();
                        SubMenuMode = false;
                        break;
                    case 1:
                        SetMainMenuEvent(0);
                        SetExtraMenuEvent(1);
                        MainMenuLayout.Animate().Alpha(0.0f).SetDuration(500).Start();
                        ExtraMenuLayout.Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(200).Start();
                        ExtraMenuLayout.BringToFront();
                        SubMenuMode = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private void ExitTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ExitTimer.Stop();
        }

        public override void OnBackPressed()
        {
            if (SubMenuMode == true)
            {
                SwitchExtraMenu(0);
                return;
            }

            if (ExitTimer.Enabled == false)
            {
                ExitTimer.Start();
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Main_CheckExit, Snackbar.LengthLong, Android.Graphics.Color.DarkOrange);
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
                if (CheckSelfPermission(permission) == Permission.Denied) RequestPermissions(new string[] { permission }, 0);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "권한 부여 실패", Snackbar.LengthIndefinite, Android.Graphics.Color.DarkMagenta);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (grantResults[0] == Permission.Denied)
            {
                Toast.MakeText(this, "해당 권한을 허용하지 않으면 소전사전을 정상적으로 이용하실 수 없습니다.", ToastLength.Short).Show();
            }
        }
    }
}