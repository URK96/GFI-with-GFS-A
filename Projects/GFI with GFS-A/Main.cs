using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.Transitions;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "GFD", Theme = "@style/GFS.Toolbar", ScreenOrientation = ScreenOrientation.Portrait)]
    public partial class Main : BaseAppCompatActivity
    {
        System.Timers.Timer exitTimer = new System.Timers.Timer();

        CoordinatorLayout snackbarLayout;

        private TextView notificationView;

        private Android.Support.V7.Widget.Toolbar toolbar;
        private RecyclerView dbDictionarySubMenu;
        private RecyclerView gfUtilSubMenu;
        private RecyclerView extrasSubMenu;
        private RecyclerView oldGFDSubMenu;

        private LinearLayout.LayoutParams expandParams;
        private LinearLayout.LayoutParams collapseParams;

        private bool isCardOpen = false;

        private int openCardIndex = 0;

        private CardView[] mainCardViewList;

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

                toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.MainMainToolbar);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetTitle(Resource.String.MainActivity_Title);

                mainCardViewList = new CardView[]
                {
                    FindViewById<CardView>(Resource.Id.MainNotificationCardLayout),
                    FindViewById<CardView>(Resource.Id.MainDBDictionaryCardLayout),
                    FindViewById<CardView>(Resource.Id.MainGFUtilCardLayout),
                    FindViewById<CardView>(Resource.Id.MainExtrasCardLayout),
                    FindViewById<CardView>(Resource.Id.MainOldGFDCardLayout),
                    FindViewById<CardView>(Resource.Id.MainGFDInfoCardLayout),
                    FindViewById<CardView>(Resource.Id.MainSettingCardLayout)
                };

                mainCardViewList[0].Click += MainNotificationCardLayout_Click;
                mainCardViewList[1].Click += MainDBDictionaryCardLayout_Click;
                mainCardViewList[2].Click += MainGFUtilCardLayout_Click;
                mainCardViewList[3].Click += MainExtrasCardLayout_Click;
                mainCardViewList[4].Click += MainOldGFDCardLayout_Click;
                mainCardViewList[5].Click += delegate
                {
                    StartActivity(typeof(GFDInfoActivity));
                    OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
                };
                mainCardViewList[6].Click += delegate
                {
                    StartActivity(typeof(SettingActivity));
                    OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
                };

                dbDictionarySubMenu = FindViewById<RecyclerView>(Resource.Id.MainDBDictionaryRecyclerView);
                gfUtilSubMenu = FindViewById<RecyclerView>(Resource.Id.MainGFUtilRecyclerView);
                extrasSubMenu = FindViewById<RecyclerView>(Resource.Id.MainExtrasRecyclerView);
                oldGFDSubMenu = FindViewById<RecyclerView>(Resource.Id.MainOldGFDRecyclerView);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.MainSnackbarLayout);
                notificationView = FindViewById<TextView>(Resource.Id.MainNotificationText);

                // Set ActionBar Title Icon

                /*if ((DateTime.Now.Month == 10) && (DateTime.Now.Day == 31))
                    SupportActionBar.SetIcon(Resource.Drawable.AppIcon2_Core);
                else
                    SupportActionBar.SetIcon(int.Parse(ETC.sharedPreferences.GetString("MainActionbarIcon", Resource.Drawable.AppIcon2.ToString())));

                SupportActionBar.SetDisplayShowHomeEnabled(true);
                SupportActionBar.SetIcon(Resource.Mipmap.ic_launcher);*/

                SupportActionBar.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.ParseColor("#54A716")));

                // Set Program Exit Timer

                exitTimer.Interval = 2000;
                exitTimer.Elapsed += delegate { exitTimer.Stop(); };

                // Load Init Process

                _ = InitializeProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                // Refresh Notification Data

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
            switch (ETC.sharedPreferences.GetString("StartAppMode", "0"))
            {
                case "1":
                    MainDBDictionaryCardLayout_Click(mainCardViewList[1], new EventArgs()); // DB Sub Menu
                    break;
                case "2":
                    MainOldGFDSubMenu_Click(mainCardViewList[3], 0); // OldGFD
                    break;
                case "3":
                    MainExtrasSubMenu_Click(mainCardViewList[2], 3); // Area Tip
                    break;
                case "4":
                    MainExtrasSubMenu_Click(mainCardViewList[2], 2); // Calc
                    break;
                case "5":
                    MainExtrasSubMenu_Click(mainCardViewList[2], 0); // Event
                    break;
                case "6":
                    MainExtrasSubMenu_Click(mainCardViewList[2], 1); // Offical Notification
                    break;
                case "7":
                    MainExtrasSubMenu_Click(mainCardViewList[2], 9); // OST Player
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
                /*if (!ETC.sharedPreferences.GetBoolean("LowMemoryOption", false) && (ETC.Language.Language == "ko"))
                {
                    // Set Main Menu Button Color (1 = Orange, 0 = Default)
                    
                    switch (ETC.sharedPreferences.GetString("MainButtonColor", "0"))
                    {
                        case "1":
                            for (int i = 0; i < MainMenuButtonIds.Length; ++i)
                                FindViewById<Button>(MainMenuButtonIds[i]).SetBackgroundResource(MainMenuButtonBackgroundIds_Orange[i]);
                            for (int i = 0; i < DBSubMenuButtonIds.Length; ++i)
                                FindViewById<Button>(DBSubMenuButtonIds[i]).SetBackgroundResource(DBSubMenuButtonBackgroundIds_Orange[i]);
                            for (int i = 0; i < ExtraMenuButtonIds.Length; ++i)
                                FindViewById<Button>(ExtraMenuButtonIds[i]).SetBackgroundResource(ExtraMenuButtonBackgroundIds_Orange[i]);
                            break;
                        case "0":
                        default:
                            for (int i = 0; i < MainMenuButtonIds.Length; ++i)
                                FindViewById<Button>(MainMenuButtonIds[i]).SetBackgroundResource(MainMenuButtonBackgroundIds[i]);
                            for (int i = 0; i < DBSubMenuButtonIds.Length; ++i)
                                FindViewById<Button>(DBSubMenuButtonIds[i]).SetBackgroundResource(DBSubMenuButtonBackgroundIds[i]);
                            for (int i = 0; i < ExtraMenuButtonIds.Length; ++i)
                                FindViewById<Button>(ExtraMenuButtonIds[i]).SetBackgroundResource(ExtraMenuButtonBackgroundIds[i]);
                            break;
                    }
                }
                else
                {
                    for (int i = 0; i < MainMenuButtonIds.Length; ++i)
                        FindViewById<Button>(MainMenuButtonIds[i]).Text = MainMenuButtonText[i];
                    for (int i = 0; i < DBSubMenuButtonIds.Length; ++i)
                        FindViewById<Button>(DBSubMenuButtonIds[i]).Text = DBSubMenuButtonText[i];
                    for (int i = 0; i < ExtraMenuButtonIds.Length; ++i)
                        FindViewById<Button>(ExtraMenuButtonIds[i]).Text = ExtraMenuButtonText[i];
                }

                // Temporary Remove Button Images

                for (int i = 0; i < MainMenuButtonIds.Length; ++i) FindViewById<Button>(MainMenuButtonIds[i]).Text = MainMenuButtonText[i];
                for (int i = 0; i < DBSubMenuButtonIds.Length; ++i) FindViewById<Button>(DBSubMenuButtonIds[i]).Text = DBSubMenuButtonText[i];
                for (int i = 0; i < ExtraMenuButtonIds.Length; ++i) FindViewById<Button>(ExtraMenuButtonIds[i]).Text = ExtraMenuButtonText[i];*/

                // Set Layout Params Preset

                expandParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                collapseParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, 1);
                collapseParams.SetMargins(20, 20, 20, 20);

                // Set Sub Menu Adapter

                var dbSubAdapter = new MainListAdapter(DBSubMenuButtonText, this);
                dbSubAdapter.ItemClick += MainDBDictionarySubMenu_Click;
                dbDictionarySubMenu.SetLayoutManager(new LinearLayoutManager(this));
                dbDictionarySubMenu.SetAdapter(dbSubAdapter);

                var gfUtilSubAdapter = new MainListAdapter(GFUtilMenuButtonText, this);
                gfUtilSubAdapter.ItemClick += MainGFUtilSubMenu_Click;
                gfUtilSubMenu.SetLayoutManager(new LinearLayoutManager(this));
                gfUtilSubMenu.SetAdapter(gfUtilSubAdapter);

                var extrasSubAdapter = new MainListAdapter(ExtraMenuButtonText, this);
                extrasSubAdapter.ItemClick += MainExtrasSubMenu_Click;
                extrasSubMenu.SetLayoutManager(new LinearLayoutManager(this));
                extrasSubMenu.SetAdapter(extrasSubAdapter);

                string[] oldGFDList;

                switch (ETC.locale.Language)
                {
                    case "ko":
                        oldGFDList = oldGFDListText_ko;
                        break;
                    default:
                        oldGFDList = oldGFDListText_etc;
                        break;
                }

                var oldGFDSubAdapter = new MainListAdapter(oldGFDList, this);
                oldGFDSubAdapter.ItemClick += MainOldGFDSubMenu_Click;
                oldGFDSubMenu.SetLayoutManager(new LinearLayoutManager(this));
                oldGFDSubMenu.SetAdapter(oldGFDSubAdapter);

                // Check Auto Run Mode

                if (ETC.sharedPreferences.GetString("StartAppMode", "0") != "0")
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

            TextView tv = FindViewById<TextView>(Resource.Id.MainNowDBVersion);
            tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBChecking)})";

            string notificationText = "";

            try
            {
                // Check Server Status

                await ETC.CheckServerNetwork().ConfigureAwait(false);

                await Task.Run(async () =>
                {
                    // Check DB Version

                    if (await ETC.CheckDBVersion())
                    {
                        RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateAvailable)})"; });

                        Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBG);
                        ad.SetTitle(Resource.String.CheckDBUpdateDialog_Title);
                        ad.SetMessage(Resource.String.CheckDBUpdateDialog_Question);
                        ad.SetCancelable(true);
                        ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                        ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, async delegate
                        {
                            await ETC.UpdateDB(this, true);

                            if (!await ETC.CheckDBVersion())
                            {
                                RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateNewest)})"; });
                            }
                            else
                            {
                                RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateAvailable)})"; });
                            }

                        });

                        RunOnUiThread(() => { ad.Show(); });
                    }
                    else
                    {
                        RunOnUiThread(() => { tv.Text = $"DB Ver.{ETC.dbVersion} ({Resources.GetString(Resource.String.Main_DBUpdateNewest)})"; });
                    }


                    // Get Notification

                    string url = "";

                    if (ETC.locale.Language == "ko")
                    {
                        url = Path.Combine(ETC.server, "Android_Notification.txt");
                    }
                    else
                    {
                        url = Path.Combine(ETC.server, "Android_Notification_en.txt");
                    }

                    if (ETC.isServerDown)
                    {
                        notificationText = "& Server is Maintenance &";
                    }
                    else
                    {
                        using (WebClient wc = new WebClient())
                        {
                            notificationText = await wc.DownloadStringTaskAsync(url);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Main_NotificationInitializeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
                notificationText = "$ Load Fail $";
            }
            finally
            {
                notificationView.Text = notificationText;
            }
        }

        private async void MainDBDictionarySubMenu_Click(object sender, int position)
        {
            try
            {
                switch (position)
                {
                    case 0:
                        await Task.Run(() =>
                        {
                            if (string.IsNullOrEmpty(ETC.dollList.TableName))
                            {
                                ETC.LoadDBSync(ETC.dollList, "Doll.gfs", false);
                            }

                            if (!ETC.hasInitDollAvgAbility)
                            {
                                ETC.InitializeAverageAbility();
                            }
                        });
                        if (ETC.sharedPreferences.GetBoolean("PreviewDBListLayout", true))
                        {
                            StartActivity(typeof(DollDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        else
                        {
                            StartActivity(typeof(DollDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        break;
                    case 1:
                        await Task.Run(() =>
                        {
                            if (string.IsNullOrEmpty(ETC.equipmentList.TableName))
                            {
                                ETC.LoadDBSync(ETC.equipmentList, "Equipment.gfs", false);
                            }
                        });
                        if (ETC.sharedPreferences.GetBoolean("PreviewDBListLayout", true))
                        {
                            StartActivity(typeof(EquipDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        else
                        {
                            StartActivity(typeof(EquipDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        break;
                    case 2:
                        await Task.Run(() =>
                        {
                            if (string.IsNullOrEmpty(ETC.fairyList.TableName))
                            {
                                ETC.LoadDBSync(ETC.fairyList, "Fairy.gfs", false);
                            }
                        });
                        if (ETC.sharedPreferences.GetBoolean("PreviewDBListLayout", true))
                        {
                            StartActivity(typeof(FairyDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        else
                        {
                            StartActivity(typeof(FairyDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        break;
                    case 3:
                        await Task.Run(() =>
                        {
                            if (string.IsNullOrEmpty(ETC.enemyList.TableName))
                            {
                                ETC.LoadDBSync(ETC.enemyList, "Enemy.gfs", false);
                            }
                        });
                        if (ETC.sharedPreferences.GetBoolean("PreviewDBListLayout", true))
                        {
                            StartActivity(typeof(EnemyDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        else
                        {
                            StartActivity(typeof(EnemyDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        break;
                    case 4:
                        await Task.Run(() =>
                        {
                            if (string.IsNullOrEmpty(ETC.FSTList.TableName))
                            {
                                ETC.LoadDBSync(ETC.FSTList, "FST.gfs", false);
                            }
                        });
                        if (ETC.sharedPreferences.GetBoolean("PreviewDBListLayout", true))
                        {
                            StartActivity(typeof(FSTDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        else
                        {
                            StartActivity(typeof(FSTDBMainActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        break;
                    default:
                        ETC.ShowSnackbar(snackbarLayout, Resource.String.AbnormalAccess, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async void MainGFUtilSubMenu_Click(object sender, int position)
        {
            await Task.Delay(10);

            try
            {
                switch (position)
                {
                    case 0:
                        var newsIntent = new Intent(this, typeof(WebBrowserActivity));
                        newsIntent.PutExtra("url", "http://www.girlsfrontline.co.kr/archives/category/news");
                        StartActivity(newsIntent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case 1:
                        if (int.Parse(Build.VERSION.Release.Split('.')[0]) >= 6)
                        {
                            CheckPermission(Manifest.Permission.Internet);
                        }
                        StartActivity(typeof(EventListActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case 2:
                        if (int.Parse(Build.VERSION.Release.Split('.')[0]) >= 6)
                        {
                            CheckPermission(Manifest.Permission.Internet);
                        }
                        var mdIntent = new Intent(this, typeof(WebBrowserActivity));
                        mdIntent.PutExtra("url", "https://tempkaridc.github.io/gf/");
                        StartActivity(mdIntent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case 3:
                        StartActivity(typeof(CalcMainActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case 4:
                        if (int.Parse(Build.VERSION.Release.Split('.')[0]) >= 6)
                        {
                            CheckPermission(Manifest.Permission.Internet);
                        }
                        var areaTipIntent = new Intent(this, typeof(WebBrowserActivity));
                        areaTipIntent.PutExtra("url", "https://cafe.naver.com/girlsfrontlinekr/235663");
                        StartActivity(areaTipIntent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case 5:
                        //ETC.ShowSnackbar(SnackbarLayout, Resource.String.DevMode, Snackbar.LengthShort);
                        StartActivity(typeof(ShortGuideBookViewer));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case 6:
                        StartActivity(typeof(StoryActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    default:
                        ETC.ShowSnackbar(snackbarLayout, Resource.String.AbnormalAccess, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async void MainExtrasSubMenu_Click(object sender, int position)
        {
            await Task.Delay(10);

            try
            {
                switch (position)
                {
                    case 0:
                        if (ETC.dbVersion != 0)
                        {
                            StartActivity(typeof(ProductSimulatorCategorySelectActivity));
                            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        }
                        else
                        {
                            ETC.ShowSnackbar(snackbarLayout, Resource.String.NoDBFiles, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                        }
                        break;
                    case 1:
                        StartActivity(typeof(CartoonActivity));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case 2:
                        ETC.ShowSnackbar(snackbarLayout, Resource.String.DevMode, Snackbar.LengthShort);
                        //StartActivity(typeof(GFOSTPlayerActivity));
                        //OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    default:
                        ETC.ShowSnackbar(snackbarLayout, Resource.String.AbnormalAccess, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private void MainOldGFDSubMenu_Click(object sender, int position)
        {
            try
            {
                var oldGFDIntent = new Intent(this, typeof(OldGFDViewer));
                oldGFDIntent.PutExtra("Index", position);
                StartActivity(oldGFDIntent);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        public override void OnBackPressed()
        {
            if (isCardOpen)
            {
                switch (openCardIndex)
                {
                    case 0:
                        MainNotificationCardLayout_Click(mainCardViewList[openCardIndex], new EventArgs());
                        break;
                    case 1:
                        MainDBDictionaryCardLayout_Click(mainCardViewList[openCardIndex], new EventArgs());
                        break;
                    case 2:
                        MainGFUtilCardLayout_Click(mainCardViewList[openCardIndex], new EventArgs());
                        break;
                    case 3:
                        MainExtrasCardLayout_Click(mainCardViewList[openCardIndex], new EventArgs());
                        break;
                    case 4:
                        MainOldGFDCardLayout_Click(mainCardViewList[openCardIndex], new EventArgs());
                        break;
                }

                return;
            }

            if (!exitTimer.Enabled)
            {
                exitTimer.Start();
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Main_CheckExit, Snackbar.LengthLong, Android.Graphics.Color.DarkOrange);
            }
            else
            {
                FinishAffinity();
                OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
                Process.KillProcess(Process.MyPid());
            }
        }

        private void MainNotificationCardLayout_Click(object sender, EventArgs e)
        {
            var card = sender as CardView;

            try
            {
                TransitionManager.BeginDelayedTransition(card);

                if (notificationView.MaxLines == 1)
                {
                    card.CardElevation = 16;

                    for (int i = 1; i < mainCardViewList.Length; ++i)
                    {
                        mainCardViewList[i].Visibility = ViewStates.Gone;
                        mainCardViewList[i].CardElevation = 8;
                    }

                    notificationView.SetMaxLines(int.MaxValue);
                    isCardOpen = true;
                    openCardIndex = 0;
                }
                else
                {
                    for (int i = 1; i < mainCardViewList.Length; ++i)
                    {
                        mainCardViewList[i].Visibility = ViewStates.Visible;
                    }

                    notificationView.SetMaxLines(1);
                    isCardOpen = false;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Main Notification transition error", ToastLength.Short).Show();
            }
        }

        private void MainDBDictionaryCardLayout_Click(object sender, EventArgs e)
        {
            var card = sender as CardView;

            try
            {
                TransitionManager.BeginDelayedTransition(card);

                if (dbDictionarySubMenu.Visibility == ViewStates.Gone)
                {
                    card.LayoutParameters = expandParams;
                    card.CardElevation = 16;

                    for (int i = 0; i < mainCardViewList.Length; ++i)
                    {
                        if (mainCardViewList[i].Id == card.Id)
                        {
                            continue;
                        }

                        mainCardViewList[i].Visibility = ViewStates.Gone;
                        mainCardViewList[i].CardElevation = 8;
                    }

                    dbDictionarySubMenu.Visibility = ViewStates.Visible;
                    isCardOpen = true;
                    openCardIndex = 1;
                }
                else
                {
                    card.LayoutParameters = collapseParams;

                    for (int i = 0; i < mainCardViewList.Length; ++i)
                    {
                        if (mainCardViewList[i].Id == card.Id)
                        {
                            continue;
                        }

                        mainCardViewList[i].Visibility = ViewStates.Visible;
                    }

                    dbDictionarySubMenu.Visibility = ViewStates.Gone;
                    isCardOpen = false;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Main DB transition error", ToastLength.Short).Show();
            }
        }

        private void MainGFUtilCardLayout_Click(object sender, EventArgs e)
        {
            var card = sender as CardView;

            try
            {
                TransitionManager.BeginDelayedTransition(card);

                if (gfUtilSubMenu.Visibility == ViewStates.Gone)
                {
                    card.LayoutParameters = expandParams;
                    card.CardElevation = 16;

                    for (int i = 0; i < mainCardViewList.Length; ++i)
                    {
                        if (mainCardViewList[i].Id == card.Id)
                        {
                            continue;
                        }

                        mainCardViewList[i].Visibility = ViewStates.Gone;
                        mainCardViewList[i].CardElevation = 8;
                    }

                    gfUtilSubMenu.Visibility = ViewStates.Visible;
                    isCardOpen = true;
                    openCardIndex = 2;
                }
                else
                {
                    card.LayoutParameters = collapseParams;

                    for (int i = 0; i < mainCardViewList.Length; ++i)
                    {
                        if (mainCardViewList[i].Id == card.Id)
                        {
                            continue;
                        }

                        mainCardViewList[i].Visibility = ViewStates.Visible;
                    }

                    gfUtilSubMenu.Visibility = ViewStates.Gone;
                    isCardOpen = false;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Main GF Util transition error", ToastLength.Short).Show();
            }
        }

        private void MainExtrasCardLayout_Click(object sender, EventArgs e)
        {
            var card = sender as CardView;

            try
            {
                TransitionManager.BeginDelayedTransition(card);

                if (extrasSubMenu.Visibility == ViewStates.Gone)
                {
                    card.LayoutParameters = expandParams;
                    card.CardElevation = 16;

                    for (int i = 0; i < mainCardViewList.Length; ++i)
                    {
                        if (mainCardViewList[i].Id == card.Id)
                        {
                            continue;
                        }

                        mainCardViewList[i].Visibility = ViewStates.Gone;
                        mainCardViewList[i].CardElevation = 8;
                    }

                    extrasSubMenu.Visibility = ViewStates.Visible;
                    isCardOpen = true;
                    openCardIndex = 3;
                }
                else
                {
                    card.LayoutParameters = collapseParams;

                    for (int i = 0; i < mainCardViewList.Length; ++i)
                    {
                        if (mainCardViewList[i].Id == card.Id)
                        {
                            continue;
                        }

                        mainCardViewList[i].Visibility = ViewStates.Visible;
                    }

                    extrasSubMenu.Visibility = ViewStates.Gone;
                    isCardOpen = false;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Main Extras transition error", ToastLength.Short).Show();

            }
        }

        private void MainOldGFDCardLayout_Click(object sender, EventArgs e)
        {
            var card = sender as CardView;

            try
            {
                TransitionManager.BeginDelayedTransition(card);

                if (oldGFDSubMenu.Visibility == ViewStates.Gone)
                {
                    card.LayoutParameters = expandParams;
                    card.CardElevation = 16;

                    for (int i = 0; i < mainCardViewList.Length; ++i)
                    {
                        if (mainCardViewList[i].Id == card.Id)
                        {
                            continue;
                        }

                        mainCardViewList[i].Visibility = ViewStates.Gone;
                        mainCardViewList[i].CardElevation = 8;
                    }

                    oldGFDSubMenu.Visibility = ViewStates.Visible;
                    isCardOpen = true;
                    openCardIndex = 4;
                }
                else
                {
                    card.LayoutParameters = collapseParams;

                    for (int i = 0; i < mainCardViewList.Length; ++i)
                    {
                        if (mainCardViewList[i].Id == card.Id)
                        {
                            continue;
                        }

                        mainCardViewList[i].Visibility = ViewStates.Visible;
                    }

                    oldGFDSubMenu.Visibility = ViewStates.Gone;
                    isCardOpen = false;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Main OldGFD transition error", ToastLength.Short).Show();
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
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Permission_Error, Snackbar.LengthIndefinite, Android.Graphics.Color.DarkMagenta);
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

    class MainListViewHolder : RecyclerView.ViewHolder
    {
        public TextView subItem { get; private set; }

        public MainListViewHolder(View view, Action<int> listener) : base(view)
        {
            subItem = view.FindViewById<TextView>(Resource.Id.MainListSubItems);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    class MainListAdapter : RecyclerView.Adapter
    {
        string[] items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public MainListAdapter(string[] items, Activity context)
        {
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.MainListLayout, parent, false);

            MainListViewHolder vh = new MainListViewHolder(view, OnClick);

            return vh;
        }

        public override int ItemCount
        {
            get { return items.Length; }
        }

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        public bool HasOnItemClick()
        {
            return !(ItemClick == null);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            MainListViewHolder vh = holder as MainListViewHolder;

            var item = items[position];

            try
            {
                vh.subItem.Text = item;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, context);
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}