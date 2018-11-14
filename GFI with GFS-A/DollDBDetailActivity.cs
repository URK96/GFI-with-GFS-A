using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Com.Syncfusion.Charts;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBDetailActivity : FragmentActivity
    {
        System.Timers.Timer FABTimer = new System.Timers.Timer();

        private LinearLayout SkillTableSubLayout;
        private LinearLayout ModSkillTableSubLayout;

        private DataRow DollInfoDR = null;
        private string DollName;
        private int DollDicNum;
        private int DollGrade;
        internal static string DollType;
        private int ModIndex = 0;
        private int V_Costume_Index = 0;
        private List<string> VoiceList = new List<string>();
        internal static int[] AbilityValues = new int[6];

        internal static DataRow CompareDollInfoDR = null;

        private bool IsOpenFABMenu = false;
        private bool IsEnableFABMenu = false;
        private bool IsChartLoad = false;

        private ScrollView ScrollLayout;
        private CoordinatorLayout SnackbarLayout = null;

        private ProgressBar InitLoadProgressBar;
        private Spinner VoiceCostumeSelector;
        private Spinner VoiceSelector;
        private Button VoicePlayButton;
        private Button ModelDataButton;
        private FloatingActionButton RefreshCacheFAB;
        private FloatingActionButton PercentTableFAB;
        private FloatingActionButton MainFAB;
        private FloatingActionButton NamuWikiFAB;
        private FloatingActionButton InvenFAB;
        private FloatingActionButton BaseFAB;
        private Spinner ChartCompareList;
        private SfChart chart;

        private AdView adview;

        int[] ModButtonIds = { Resource.Id.DollDBDetailModSelect0, Resource.Id.DollDBDetailModSelect1, Resource.Id.DollDBDetailModSelect2, Resource.Id.DollDBDetailModSelect3 };
        internal static List<string> CompareList;
        private List<string> V_Costume_List = new List<string>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzY0NkAzMTM2MmUzMjJlMzBmNFFDVVZlU2NDRTVmYVJqQ0ZyOTVPOGhYWnFIazlQNFNPeGVEMU9WMjZnPQ==");

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                ETC.Language = Resources.Configuration.Locale;

                // Create your application here
                SetContentView(Resource.Layout.DollDBDetailLayout);

                DollDicNum = Intent.GetIntExtra("Keyword", 0);

                DollInfoDR = ETC.FindDataRow(ETC.DollList, "DicNumber", DollDicNum);
                if (ETC.Language.Language == "ko") DollName = (string)DollInfoDR["Name"];
                else
                {
                    if (DollInfoDR["Name_EN"] == DBNull.Value) DollName = (string)DollInfoDR["Name"];
                    else if (string.IsNullOrWhiteSpace((string)DollInfoDR["Name_EN"])) DollName = (string)DollInfoDR["Name"];
                    else DollName = (string)DollInfoDR["Name_EN"];
                }
                DollDicNum = (int)DollInfoDR["DicNumber"];
                DollGrade = (int)DollInfoDR["Grade"];
                DollType = (string)DollInfoDR["Type"];

                adview = FindViewById<AdView>(Resource.Id.DollDBDetail_adView);
                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.DollDBDetailInitLoadProgress);
                SkillTableSubLayout = FindViewById<LinearLayout>(Resource.Id.DollDBDetailSkillAbilitySubLayout);
                ModSkillTableSubLayout = FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSkillAbilitySubLayout);

                
                if ((bool)DollInfoDR["HasMod"] == true)
                {
                    foreach (int id in ModButtonIds)
                    {
                        FindViewById<ImageButton>(id).Click += DollDBDetailModSelectButton_Click;
                        FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);
                    }

                    FindViewById<ImageButton>(ModButtonIds[0]).SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                }

                FindViewById<ImageView>(Resource.Id.DollDBDetailSmallImage).Click += DollDBDetailSmallImage_Click;

                
                if ((bool)DollInfoDR["HasVoice"] == true)
                {
                    VoiceCostumeSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailVoiceCostumeSelector);
                    VoiceCostumeSelector.ItemSelected += VoiceCostumeSelector_ItemSelected;
                    VoiceSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailVoiceSelector);
                    VoicePlayButton = FindViewById<Button>(Resource.Id.DollDBDetailVoicePlayButton);
                    VoicePlayButton.Click += VoicePlayButton_Click;
                    InitializeVoiceList();
                }

                ModelDataButton = FindViewById<Button>(Resource.Id.DollDBDetailModelDataButton);
                ModelDataButton.Click += ModelDataButton_Click;

                ScrollLayout = FindViewById<ScrollView>(Resource.Id.DollDBDetailScrollLayout);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBDetailSnackbarLayout);

                RefreshCacheFAB = FindViewById<FloatingActionButton>(Resource.Id.DollDBDetailRefreshCacheFAB);
                PercentTableFAB = FindViewById<FloatingActionButton>(Resource.Id.DollDBDetailProductPercentFAB);
                if ((int)DollInfoDR["ProductTIme"] == 0) PercentTableFAB.Visibility = ViewStates.Gone;
                MainFAB = FindViewById<FloatingActionButton>(Resource.Id.DollDBDetailSideLinkMainFAB);
                NamuWikiFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkFAB1);
                NamuWikiFAB.SetImageResource(Resource.Drawable.NamuWiki_Logo);
                InvenFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkFAB2);
                InvenFAB.SetImageResource(Resource.Drawable.Inven_Logo);
                BaseFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkFAB3);
                BaseFAB.SetImageResource(Resource.Drawable.Base36_Logo);
                ChartCompareList = FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityChartCompareList);
                ChartCompareList.ItemSelected += ChartCompareList_ItemSelected;
                chart = FindViewById<SfChart>(Resource.Id.DollDBDetailAbilityRadarChart);

                RefreshCacheFAB.Click += RefreshCacheFAB_Click;
                PercentTableFAB.Click += PercentTableFAB_Click;
                MainFAB.Click += MainFAB_Click;
                NamuWikiFAB.Click += MainSubFAB_Click;
                InvenFAB.Click += MainSubFAB_Click;
                BaseFAB.Click += MainSubFAB_Click;

                RefreshCacheFAB.LongClick += DBDetailFAB_LongClick;
                PercentTableFAB.LongClick += DBDetailFAB_LongClick;
                MainFAB.LongClick += DBDetailFAB_LongClick;
                NamuWikiFAB.LongClick += DBDetailFAB_LongClick;
                InvenFAB.LongClick += DBDetailFAB_LongClick;
                BaseFAB.LongClick += DBDetailFAB_LongClick;

                FABTimer.Interval = 3000;
                FABTimer.Elapsed += FABTimer_Elapsed;

                InitCompareList();

                InitLoadProcess(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void DBDetailFAB_LongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                string tip = "";

                switch (fab.Id)
                {
                    case Resource.Id.DollDBDetailRefreshCacheFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_CacheRefresh);
                        break;
                    case Resource.Id.DollDBDetailProductPercentFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_ProductPercentage);
                        break;
                    case Resource.Id.DollDBDetailSideLinkMainFAB:
                        if (IsEnableFABMenu == false) return;
                        tip = Resources.GetString(Resource.String.Tooltip_DB_SideLink);
                        break;
                    case Resource.Id.SideLinkFAB1:
                        tip = Resources.GetString(Resource.String.Tooltip_SideLink_NamuWiki);
                        break;
                    case Resource.Id.SideLinkFAB2:
                        tip = Resources.GetString(Resource.String.Tooltip_SideLink_Inven);
                        break;
                    case Resource.Id.SideLinkFAB3:
                        tip = Resources.GetString(Resource.String.Tooltip_SideLink_36Base);
                        break;
                }

                Toast.MakeText(this, tip, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void InitCompareList()
        {
            try
            {
                CompareList = new List<string>();
                CompareList.Add("Type Average");

                foreach (DataRow dr in ETC.DollList.Rows)
                {
                    if (DollDicNum == (int)dr["DicNumber"]) continue;
                    if ((string)dr["Type"] != DollType) continue;

                    string name = "";
                    if (ETC.Language.Language == "ko") name = (string)dr["Name"];
                    else
                    {
                        if (DollInfoDR["Name_EN"] == DBNull.Value) name = (string)dr["Name"];
                        else if (string.IsNullOrWhiteSpace((string)DollInfoDR["Name_EN"])) name = (string)dr["Name"];
                        else name = (string)dr["Name_EN"];
                    }
                    CompareList.Add(name);
                }

                CompareList.TrimExcess();

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, CompareList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                ChartCompareList.Adapter = adapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "Test Error", Snackbar.LengthShort);
            }
        }

        private void ChartCompareList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            LoadChart(e.Position);
        }

        private void ModelDataButton_Click(object sender, EventArgs e)
        {
            string FileName = string.Format("{0}.txt", DollDicNum);
            string data = "";

            try
            {
                Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG);
                ad.SetTitle(Resource.String.DollDBDetailLayout_ModelData);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                Task.Run(async () =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        data = await wc.DownloadStringTaskAsync(Path.Combine(ETC.Server, "Data", "Text", "Gun", "ModelData", FileName));
                    }
                }).Wait();

                ad.SetMessage(data);
                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void RefreshCacheFAB_Click(object sender, EventArgs e)
        {
            InitLoadProcess(true);
        }

        private async Task LoadChart(int CompareIndex)
        {
            chart.Series.Clear();

            ChartZoomPanBehavior ZoomBehavior = new ChartZoomPanBehavior();
            ZoomBehavior.ZoomMode = ZoomMode.Xy;
            ZoomBehavior.SelectionZoomingEnabled = true;
            ZoomBehavior.MaximumZoomLevel = 2.0f;
            ZoomBehavior.ZoomingEnabled = true;
            ZoomBehavior.DoubleTapEnabled = true;
            ZoomBehavior.ScrollingEnabled = true;

            chart.Behaviors.Add(ZoomBehavior);

            chart.PrimaryAxis = new CategoryAxis();
            chart.SecondaryAxis = new NumericalAxis();
            chart.Legend.Visibility = Visibility.Visible;

            if (ETC.UseLightTheme == true) chart.Legend.LabelStyle.TextColor = Android.Graphics.Color.DarkGray;
            else chart.Legend.LabelStyle.TextColor = Android.Graphics.Color.LightGray;

            RadarSeries radar = new RadarSeries();

            DataModel model = new DataModel(CompareIndex);

            radar.ItemsSource = model.MaxAbilityList;
            radar.XBindingPath = "AbilityType";
            radar.YBindingPath = "AbilityValue";
            radar.DrawType = PolarChartDrawType.Line;
            radar.Color = Android.Graphics.Color.LightGreen;
            radar.EnableAnimation = true;

            radar.Label = DollName;
            radar.TooltipEnabled = true;

            chart.Series.Add(radar);

            RadarSeries radar2 = new RadarSeries();

            radar2.ItemsSource = model.CompareAbilityList;
            radar2.XBindingPath = "AbilityType";
            radar2.YBindingPath = "AbilityValue";
            radar2.DrawType = PolarChartDrawType.Line;
            radar2.Color = Android.Graphics.Color.Magenta;
            radar2.EnableAnimation = true;

            if (CompareIndex == 0) radar2.Label = string.Format("{0}{1}", DollType, Resources.GetString(Resource.String.DollDBDetail_RadarAverageString));
            else radar2.Label = (string)CompareDollInfoDR["Name"];

            radar2.TooltipEnabled = true;

            chart.Series.Add(radar2);

            IsChartLoad = true;
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                if (IsChartLoad == true) LoadChart(ChartCompareList.SelectedItemPosition);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void FABTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HideFloatingActionButtonAnimation();
        }

        private void PercentTableFAB_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(ProductPercentTableActivity));
                intent.PutExtra("Info", new string[] { "Doll", DollDicNum.ToString() });
                StartActivity(intent);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private async void VoicePlayButton_Click(object sender, EventArgs e)
        {
            try
            {
                FindViewById<ProgressBar>(Resource.Id.DollDBDetailVoiceDownloadProgress).Visibility = ViewStates.Visible;
                FindViewById<ProgressBar>(Resource.Id.DollDBDetailVoiceDownloadProgress).Indeterminate = true;

                string voice = VoiceList[VoiceSelector.SelectedItemPosition];

                string VoiceServerURL = "";
                string target = "";

                switch (V_Costume_Index)
                {
                    case 0:
                        VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", DollName, $"{DollName}_{voice}_JP.wav");
                        target = Path.Combine(ETC.CachePath, "Voices", $"{DollName}_{voice}_JP.wav");
                        break;
                    case 1:
                        VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", $"{DollName}_{V_Costume_Index - 1}", $"{DollName}_{V_Costume_Index - 1}_{voice}_JP.wav");
                        target = Path.Combine(ETC.CachePath, "Voices", $"{DollName}_{V_Costume_Index - 1}_{voice}_JP.wav");
                        break;
                }

                MediaPlayer SoundPlayer = new MediaPlayer();
                SoundPlayer.Completion += delegate { SoundPlayer.Release(); };

                await Task.Delay(100);

                try
                {
                    SoundPlayer.SetDataSource(target);
                }
                catch (Exception)
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += (object s, DownloadProgressChangedEventArgs args) =>
                        {
                            FindViewById<ProgressBar>(Resource.Id.DollDBDetailVoiceDownloadProgress).Indeterminate = false;
                            FindViewById<ProgressBar>(Resource.Id.DollDBDetailVoiceDownloadProgress).Progress = args.ProgressPercentage;
                        };
                        await wc.DownloadFileTaskAsync(VoiceServerURL, target);
                    }

                    SoundPlayer.SetDataSource(target);
                }

                FindViewById<ProgressBar>(Resource.Id.DollDBDetailVoiceDownloadProgress).Visibility = ViewStates.Invisible;

                SoundPlayer.Prepare();
                SoundPlayer.Start();
            }
            catch (WebException ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.VoiceStreaming_Error, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.VoiceStreaming_PlayError, Snackbar.LengthShort, Android.Graphics.Color.DarkCyan);
            }
        }

        private void InitializeVoiceList()
        {
            try
            {
                V_Costume_List.Clear();
                V_Costume_List.Add($"Default:{(string)DollInfoDR["Voices"]}");
                if (DollInfoDR["CostumeVoices"] != DBNull.Value)
                {
                    if (string.IsNullOrWhiteSpace((string)DollInfoDR["CostumeVoices"]) == false) V_Costume_List.AddRange(((string)DollInfoDR["CostumeVoices"]).Split(','));
                }
                V_Costume_List.TrimExcess();

                List<string> V_C_List = new List<string>();

                for (int i = 0; i < V_Costume_List.Count; ++i)
                {
                    if (i >= 1)
                    {
                        string[] Costumes = ((string)DollInfoDR["Costume"]).Split(';');
                        V_C_List.Add(Costumes[int.Parse(V_Costume_List[i].Split(':')[0])]);
                    }
                    else V_C_List.Add(V_Costume_List[i].Split(':')[0]);
                }

                V_C_List.TrimExcess();

                var c_adater = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, V_C_List);
                c_adater.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                VoiceCostumeSelector.Adapter = c_adater;

                VoiceCostumeSelector.SetSelection(0);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.VoiceList_InitError, ToastLength.Short).Show();
            }
        }

        private void VoiceCostumeSelector_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                V_Costume_Index = e.Position;

                VoiceList.Clear();
                VoiceList.AddRange(V_Costume_List[e.Position].Split(':')[1].Split(';'));
                VoiceList.TrimExcess();

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, VoiceList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                VoiceSelector.Adapter = adapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.VoiceList_InitError, ToastLength.Short).Show();
            }
        }

        private void MainSubFAB_Click(object sender, EventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                switch (fab.Id)
                {
                    case Resource.Id.SideLinkFAB1:
                        string uri = string.Format("https://namu.wiki/w/{0}(소녀전선)", DollName);
                        var intent = new Intent(this, typeof(WebBrowserActivity));
                        intent.PutExtra("url", uri);
                        StartActivity(intent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.SideLinkFAB2:
                        string uri2 = string.Format("http://gf.inven.co.kr/dataninfo/dolls/detail.php?d=126&c={0}", DollDicNum);
                        var intent2 = new Intent(this, typeof(WebBrowserActivity));
                        intent2.PutExtra("url", uri2);
                        StartActivity(intent2);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.SideLinkFAB3:
                        string uri3 = string.Format("https://girlsfrontline.kr/doll/{0}", DollDicNum);
                        var intent3 = new Intent(this, typeof(WebBrowserActivity));
                        intent3.PutExtra("url", uri3);
                        StartActivity(intent3);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                MainFAB_Click(MainFAB, new EventArgs());
            }
        }

        private void MainFAB_Click(object sender, EventArgs e)
        {
            if (IsEnableFABMenu == false)
            {
                MainFAB.SetImageResource(Resource.Drawable.SideLinkIcon);
                IsEnableFABMenu = true;
                MainFAB.Animate().Alpha(1.0f).SetDuration(500).Start();
                PercentTableFAB.Show();
                RefreshCacheFAB.Show();
                FABTimer.Start();
            }
            else
            {
                int[] ShowAnimationIds = { Resource.Animation.SideLinkFAB1_Show, Resource.Animation.SideLinkFAB2_Show, Resource.Animation.SideLinkFAB3_Show };
                int[] HideAnimationIds = { Resource.Animation.SideLinkFAB1_Hide, Resource.Animation.SideLinkFAB2_Hide, Resource.Animation.SideLinkFAB3_Hide };
                FloatingActionButton[] FABs = { NamuWikiFAB, InvenFAB, BaseFAB };
                double[,] Mags = { { 1.80, 0.25 }, { 1.5, 1.5 }, { 0.25, 1.80 } };

                try
                {
                    switch (IsOpenFABMenu)
                    {
                        case false:
                            for (int i = 0; i < FABs.Length; ++i)
                            {
                                FrameLayout.LayoutParams layoutparams = (FrameLayout.LayoutParams)FABs[i].LayoutParameters;
                                layoutparams.RightMargin += (int)(FABs[i].Width * Mags[i, 0]);
                                layoutparams.BottomMargin += (int)(FABs[i].Height * Mags[i, 1]);

                                FABs[i].LayoutParameters = layoutparams;
                                FABs[i].StartAnimation(AnimationUtils.LoadAnimation(Application, ShowAnimationIds[i]));
                                FABs[i].Clickable = true;
                            }
                            IsOpenFABMenu = true;
                            PercentTableFAB.Hide();
                            RefreshCacheFAB.Hide();
                            FABTimer.Stop();
                            break;
                        case true:
                            for (int i = 0; i < FABs.Length; ++i)
                            {
                                FrameLayout.LayoutParams layoutparams = (FrameLayout.LayoutParams)FABs[i].LayoutParameters;
                                layoutparams.RightMargin -= (int)(FABs[i].Width * Mags[i, 0]);
                                layoutparams.BottomMargin -= (int)(FABs[i].Height * Mags[i, 1]);

                                FABs[i].LayoutParameters = layoutparams;
                                FABs[i].StartAnimation(AnimationUtils.LoadAnimation(Application, HideAnimationIds[i]));
                                FABs[i].Clickable = false;
                            }
                            IsOpenFABMenu = false;
                            PercentTableFAB.Show();
                            RefreshCacheFAB.Show();
                            FABTimer.Start();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                    ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeSubMenuError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                }
            }
        }

        private void DollDBDetailSmallImage_Click(object sender, EventArgs e)
        {
            try
            {
                var DollImageViewer = new Intent(this, typeof(DollDBImageViewer));
                DollImageViewer.PutExtra("Data", string.Format("{0};{1}", DollName, ModIndex));
                StartActivity(DollImageViewer);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ImageViewer_ActivityOpenError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task InitLoadProcess(bool IsRefresh)
        {
            InitLoadProgressBar.Visibility = ViewStates.Visible;

            await Task.Delay(100);

            try
            {
                // 인형 타이틀 바 초기화

                if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false) == true)
                {
                    try
                    {
                        if ((File.Exists(Path.Combine(ETC.CachePath, "Doll", "Normal", DollDicNum + ".gfdcache")) == false) || (IsRefresh == true))
                        {
                            using (WebClient wc = new WebClient())
                            {
                                await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", DollDicNum + ".png"), Path.Combine(ETC.CachePath, "Doll", "Normal", DollDicNum + ".gfdcache"));
                            }
                        }

                        Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Normal", DollDicNum + ".gfdcache"));
                        drawable.SetAlpha(40);
                        FindViewById<RelativeLayout>(Resource.Id.DollDBDetailMainLayout).Background = drawable;
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(this, ex.ToString());
                    }
                }

                string FileName = DollDicNum.ToString();
                if (ModIndex == 3) FileName += "_M";

                try
                {
                    if ((File.Exists(Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", FileName + ".gfdcache")) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", FileName + ".png"), Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", FileName + ".gfdcache"));
                        }
                    }

                    ImageView DollSmallImage = FindViewById<ImageView>(Resource.Id.DollDBDetailSmallImage);
                    DollSmallImage.SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", FileName + ".gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }

                FindViewById<TextView>(Resource.Id.DollDBDetailDollName).Text = DollName;
                FindViewById<TextView>(Resource.Id.DollDBDetailDollDicNumber).Text = string.Format("No. {0}", DollDicNum);
                FindViewById<TextView>(Resource.Id.DollDBDetailDollProductTime).Text = ETC.CalcTime((int)DollInfoDR["ProductTime"]);
                FindViewById<TextView>(Resource.Id.DollDBDetailDollProductDialog).Text = (string)DollInfoDR["ProductDialog"];


                // 인형 기본 정보 초기화

                int[] GradeStarIds = { Resource.Id.DollDBDetailInfoGrade1, Resource.Id.DollDBDetailInfoGrade2, Resource.Id.DollDBDetailInfoGrade3, Resource.Id.DollDBDetailInfoGrade4, Resource.Id.DollDBDetailInfoGrade5 };

                if (DollGrade == 0)
                {
                    for (int i = 1; i < GradeStarIds.Length; ++i) FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;
                    FindViewById<ImageView>(GradeStarIds[0]).SetImageResource(Resource.Drawable.Grade_Star_EX);
                }
                else
                {
                    for (int i = DollGrade; i < GradeStarIds.Length; ++i) FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;
                    for (int i = 0; i < DollGrade; ++i)
                    {
                        FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Visible;
                        FindViewById<ImageView>(GradeStarIds[i]).SetImageResource(Resource.Drawable.Grade_Star);
                    }
                }

                FindViewById<TextView>(Resource.Id.DollDBDetailInfoType).Text = (string)DollInfoDR["Type"];
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoName).Text = DollName;
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoNickName).Text = (string)DollInfoDR["NickName"];
                if (DollInfoDR["Illustrator"] == DBNull.Value) FindViewById<TextView>(Resource.Id.DollDBDetailInfoIllustrator).Text = "";
                else FindViewById<TextView>(Resource.Id.DollDBDetailInfoIllustrator).Text = (string)DollInfoDR["Illustrator"];
                if (DollInfoDR["VoiceActor"] == DBNull.Value) FindViewById<TextView>(Resource.Id.DollDBDetailInfoVoiceActor).Text = "";
                else FindViewById<TextView>(Resource.Id.DollDBDetailInfoVoiceActor).Text = (string)DollInfoDR["VoiceActor"];
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoRealModel).Text = (string)DollInfoDR["Model"];
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoCountry).Text = (string)DollInfoDR["Country"];
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoHowToGain).Text = (string)DollInfoDR["DropEvent"];


                // 인형 버프 정보 초기화

                int[] BuffIds = { Resource.Id.DollDBDetailBuff1, Resource.Id.DollDBDetailBuff2, Resource.Id.DollDBDetailBuff3, Resource.Id.DollDBDetailBuff4, Resource.Id.DollDBDetailBuff5, Resource.Id.DollDBDetailBuff6, Resource.Id.DollDBDetailBuff7, Resource.Id.DollDBDetailBuff8, Resource.Id.DollDBDetailBuff9 };

                string[] Buff_Data = new string[9];

                if (ModIndex == 0) Buff_Data = ((string)DollInfoDR["EffectFormation"]).Split(',');
                else Buff_Data = ((string)DollInfoDR["ModEffectFormation"]).Split(',');

                for (int i = 0; i < Buff_Data.Length; ++i)
                {
                    int data = int.Parse(Buff_Data[i]);

                    Android.Graphics.Color color;

                    switch (data)
                    {
                        case 0:
                            color = Android.Graphics.Color.Gray;
                            break;
                        case 1:
                            color = Android.Graphics.Color.ParseColor("#54A716");
                            break;
                        case 2:
                            color = Android.Graphics.Color.White;
                            break;
                        default:
                            color = Android.Graphics.Color.Red;
                            break;
                    }

                    FindViewById<View>(BuffIds[i]).SetBackgroundColor(color);
                }

                int[] BuffIconIds = { Resource.Id.DollDBDetailBuffIcon1, Resource.Id.DollDBDetailBuffIcon2 };
                int[] BuffIconNameIds = { Resource.Id.DollDBDetailBuffName1, Resource.Id.DollDBDetailBuffName2 };
                int[] BuffDetailIds = { Resource.Id.DollDBDetailBuffDetail1, Resource.Id.DollDBDetailBuffDetail2 };

                string[] Buff;
                string[] BuffType;

                if (ModIndex >= 1) Buff = ((string)DollInfoDR["ModEffect"]).Split(';');
                else Buff = ((string)DollInfoDR["Effect"]).Split(';');

                BuffType = Buff[0].Split(',');

                if (BuffType.Length == 1) FindViewById<LinearLayout>(Resource.Id.DollDBDetailBuffLayout2).Visibility = ViewStates.Gone;
                else FindViewById<LinearLayout>(Resource.Id.DollDBDetailBuffLayout2).Visibility = ViewStates.Visible;

                for (int i = 0; i < BuffType.Length; ++i)
                {
                    int id = 0;
                    string name = "";

                    switch (BuffType[i])
                    {
                        case "AC":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.AC_Icon_WhiteTheme;
                            else id = Resource.Drawable.AC_Icon;
                            name = Resources.GetString(Resource.String.Common_AC);
                            break;
                        case "AM":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.AM_Icon_WhiteTheme;
                            else id = Resource.Drawable.AM_Icon;
                            name = Resources.GetString(Resource.String.Common_AM);
                            break;
                        case "AS":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.AS_Icon_WhiteTheme;
                            else id = Resource.Drawable.AS_Icon;
                            name = Resources.GetString(Resource.String.Common_AS);
                            break;
                        case "CR":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.CR_Icon_WhiteTheme;
                            else id = Resource.Drawable.CR_Icon;
                            name = Resources.GetString(Resource.String.Common_CR);
                            break;
                        case "EV":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.EV_Icon_WhiteTheme;
                            else id = Resource.Drawable.EV_Icon;
                            name = Resources.GetString(Resource.String.Common_EV);
                            break;
                        case "FR":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.FR_Icon_WhiteTheme;
                            else id = Resource.Drawable.FR_Icon;
                            name = Resources.GetString(Resource.String.Common_FR);
                            break;
                        case "CL":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.CL_Icon_WhiteTheme;
                            else id = Resource.Drawable.CL_Icon;
                            name = Resources.GetString(Resource.String.Common_CL);
                            break;
                        default:
                            break;
                    }

                    FindViewById<ImageView>(BuffIconIds[i]).SetImageResource(id);
                    FindViewById<TextView>(BuffIconNameIds[i]).Text = name;
                }

                StringBuilder sb1 = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();
                StringBuilder[] EffectString = { sb1, sb2 };

                for (int i = 1; i < Buff.Length; ++i)
                {
                    string[] s = Buff[i].Split(',');

                    for (int j = 0; j < s.Length; ++j)
                    {
                        EffectString[j].Append(s[j]);
                        EffectString[j].Append("%");

                        if (i < (Buff.Length - 1)) EffectString[j].Append(" | ");
                    }
                }

                for (int i = 0; i < BuffType.Length; ++i)
                {
                    FindViewById<TextView>(BuffDetailIds[i]).Text = EffectString[i].ToString();
                }

                string EffectType = (string)DollInfoDR["EffectType"];

                var EffectTypeView = FindViewById<TextView>(Resource.Id.DollDBDetailEffectType);
                if (EffectType == "ALL") EffectTypeView.Text = Resources.GetString(Resource.String.DollDBDetail_BuffType_All);
                else EffectTypeView.Text = string.Format("{0} {1}", EffectType, Resources.GetString(Resource.String.DollDBDetail_BuffType_ConfirmString));


                // 인형 스킬 정보 초기화

                string SkillName = (string)DollInfoDR["Skill"];

                try
                {
                    if ((File.Exists(Path.Combine(ETC.CachePath, "Doll", "Skill", SkillName + ".gfdcache")) == false) || (IsRefresh == true))
                    {

                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFile(Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", SkillName + ".png"), Path.Combine(ETC.CachePath, "Doll", "Skill", SkillName + ".gfdcache"));
                        }

                    }
                    FindViewById<ImageView>(Resource.Id.DollDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Skill", SkillName + ".gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }

                FindViewById<TextView>(Resource.Id.DollDBDetailSkillName).Text = SkillName;

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<ImageView>(Resource.Id.DollDBDetailSkillInitCoolTimeIcon).SetImageResource(Resource.Drawable.FirstCoolTime_Icon_WhiteTheme);
                    FindViewById<ImageView>(Resource.Id.DollDBDetailSkillCoolTimeIcon).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                }

                string[] SkillAbilities = ((string)DollInfoDR["SkillEffect"]).Split(';');
                string[] SkillMags;
                if (ModIndex == 0 ) SkillMags = ((string)DollInfoDR["SkillMag"]).Split(',');
                else SkillMags = ((string)DollInfoDR["SkillMagAfterMod"]).Split(',');

                TextView SkillInitCoolTime = FindViewById<TextView>(Resource.Id.DollDBDetailSkillInitCoolTime);
                SkillInitCoolTime.SetTextColor(Android.Graphics.Color.Orange);
                SkillInitCoolTime.Text = SkillMags[0];
                TextView SkillCoolTime = FindViewById<TextView>(Resource.Id.DollDBDetailSkillCoolTime);
                SkillCoolTime.SetTextColor(Android.Graphics.Color.DarkOrange);
                SkillCoolTime.Text = SkillMags[1];

                FindViewById<TextView>(Resource.Id.DollDBDetailSkillExplain).Text = (string)DollInfoDR["SkillExplain"];

                SkillTableSubLayout.RemoveAllViews();

                for (int i = 2; i < SkillAbilities.Length; ++i)
                {
                    LinearLayout layout = new LinearLayout(this);
                    layout.Orientation = Android.Widget.Orientation.Horizontal;
                    layout.LayoutParameters = FindViewById<LinearLayout>(Resource.Id.DollDBDetailSkillAbilityTopLayout).LayoutParameters;

                    TextView ability = new TextView(this);
                    TextView mag = new TextView(this);

                    ability.LayoutParameters = FindViewById<TextView>(Resource.Id.DollDBDetailSkillAbilityTopText1).LayoutParameters;
                    mag.LayoutParameters = FindViewById<TextView>(Resource.Id.DollDBDetailSkillAbilityTopText2).LayoutParameters;

                    ability.Text = SkillAbilities[i];
                    ability.Gravity = GravityFlags.Center;
                    mag.Text = SkillMags[i];
                    mag.Gravity = GravityFlags.Center;

                    layout.AddView(ability);
                    layout.AddView(mag);

                    SkillTableSubLayout.AddView(layout);
                }


                // 인형 Mod 스킬 정보 초기화

                if ((bool)DollInfoDR["HasMod"] == true)
                {
                    string MSkillName = (string)DollInfoDR["ModSkill"];

                    try
                    {
                        if ((File.Exists(Path.Combine(ETC.CachePath, "Doll", "Skill", MSkillName + ".gfdcache")) == false) || (IsRefresh == true))
                        {
                            using (WebClient wc = new WebClient())
                            {
                                wc.DownloadFile(Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", MSkillName + ".png"), Path.Combine(ETC.CachePath, "Doll", "Skill", MSkillName + ".gfdcache"));
                            }
                        }

                        FindViewById<ImageView>(Resource.Id.DollDBDetailModSkillIcon).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Skill", MSkillName + ".gfdcache")));
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(this, ex.ToString());
                    }

                    FindViewById<TextView>(Resource.Id.DollDBDetailModSkillName).Text = MSkillName;
                    FindViewById<TextView>(Resource.Id.DollDBDetailModSkillExplain).Text = (string)DollInfoDR["ModSkillExplain"];

                    string[] MSkillAbilities = ((string)DollInfoDR["ModSkillEffect"]).Split(';');
                    string[] MSkillMags = ((string)DollInfoDR["ModSkillMag"]).Split(',');

                    ModSkillTableSubLayout.RemoveAllViews();

                    for (int i = 0; i < MSkillAbilities.Length; ++i)
                    {
                        LinearLayout layout = new LinearLayout(this);
                        layout.Orientation = Android.Widget.Orientation.Horizontal;
                        layout.LayoutParameters = FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSkillAbilityTopLayout).LayoutParameters;

                        TextView ability = new TextView(this);
                        TextView mag = new TextView(this);

                        ability.LayoutParameters = FindViewById<TextView>(Resource.Id.DollDBDetailModSkillAbilityTopText1).LayoutParameters;
                        mag.LayoutParameters = FindViewById<TextView>(Resource.Id.DollDBDetailModSkillAbilityTopText2).LayoutParameters;

                        ability.Text = SkillAbilities[i];
                        ability.Gravity = GravityFlags.Center;
                        mag.Text = SkillMags[i];
                        mag.Gravity = GravityFlags.Center;

                        layout.AddView(ability);
                        layout.AddView(mag);

                        ModSkillTableSubLayout.AddView(layout);
                    }
                }


                // 인형 능력치 초기화

                int delay = 1;

                string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
                int[] Progresses = { Resource.Id.DollInfoHPProgress, Resource.Id.DollInfoFRProgress, Resource.Id.DollInfoEVProgress, Resource.Id.DollInfoACProgress, Resource.Id.DollInfoASProgress };
                int[] ProgressMaxTexts = { Resource.Id.DollInfoHPProgressMax, Resource.Id.DollInfoFRProgressMax, Resource.Id.DollInfoEVProgressMax, Resource.Id.DollInfoACProgressMax, Resource.Id.DollInfoASProgressMax };
                int[] StatusTexts = { Resource.Id.DollInfoHPStatus, Resource.Id.DollInfoFRStatus, Resource.Id.DollInfoEVStatus, Resource.Id.DollInfoACStatus, Resource.Id.DollInfoASStatus };

                string[] AbilityGrade = ((string)DollInfoDR["AbilityGrade"]).Split(';');

                for (int i = 0; i < Progresses.Length; ++i)
                {
                    FindViewById<TextView>(ProgressMaxTexts[i]).Text = FindViewById<ProgressBar>(Progresses[i]).Max.ToString();

                    int MinValue = 0;

                    string temp = (((string)DollInfoDR[abilities[i]]).Split(';')[0].Split('/'))[0];

                    if (temp.Contains("?") == true) MinValue = 0;
                    else MinValue = int.Parse(temp);

                    int MaxValue = 0;

                    switch (ModIndex)
                    {
                        case 0:
                            MaxValue = int.Parse((((string)DollInfoDR[abilities[i]]).Split(';')[0].Split('/'))[1]);
                            break;
                        case 1:
                        case 2:
                        case 3:
                            MaxValue = int.Parse((((string)DollInfoDR[abilities[i]]).Split(';'))[ModIndex]);
                            break;
                    }

                    ProgressBar pb = FindViewById<ProgressBar>(Progresses[i]);

                    pb.Progress = MinValue;
                    pb.SecondaryProgress = MaxValue;

                    AbilityValues[i] = MaxValue;

                    FindViewById<TextView>(StatusTexts[i]).Text = string.Format("{0}/{1} ({2})", ((string)DollInfoDR[abilities[i]]).Split('/')[0], MaxValue, AbilityGrade[i]);
                }

                if ((DollType == "MG") || (DollType == "SG"))
                {
                    FindViewById<LinearLayout>(Resource.Id.DollInfoBulletLayout).Visibility = ViewStates.Visible;
                    FindViewById<LinearLayout>(Resource.Id.DollInfoReloadLayout).Visibility = ViewStates.Visible;

                    double ReloadTime = CalcReloadTime(DollInfoDR, DollType);
                    int Bullet = (int)DollInfoDR["Bullet"];
                    FindViewById<TextView>(Resource.Id.DollInfoBulletProgressMax).Text = FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress).Max.ToString();

                    FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress).Progress = Bullet;
                    FindViewById<TextView>(Resource.Id.DollInfoBulletStatus).Text = Bullet.ToString();
                    FindViewById<TextView>(Resource.Id.DollInfoReloadStatus).Text = string.Format("{0} {1}", ReloadTime, Resources.GetString(Resource.String.Time_Second));
                }
                else
                {
                    FindViewById<LinearLayout>(Resource.Id.DollInfoBulletLayout).Visibility = ViewStates.Gone;
                    FindViewById<LinearLayout>(Resource.Id.DollInfoReloadLayout).Visibility = ViewStates.Gone;
                }

                if (DollType == "SG")
                {
                    FindViewById<LinearLayout>(Resource.Id.DollInfoAMLayout).Visibility = ViewStates.Visible;

                    FindViewById<TextView>(Resource.Id.DollInfoAMProgressMax).Text = FindViewById<ProgressBar>(Resource.Id.DollInfoAMProgress).Max.ToString();

                    int MinValue = 0;
                    int MaxValue = int.Parse((((string)DollInfoDR["Armor"]).Split('/'))[1]);

                    string temp = (((string)DollInfoDR["Armor"]).Split('/'))[0];

                    if (temp.Contains("?") == true) MinValue = 0;
                    else MinValue = int.Parse(temp);

                    FindViewById<ProgressBar>(Resource.Id.DollInfoAMProgress).Progress = MinValue;
                    FindViewById<ProgressBar>(Resource.Id.DollInfoAMProgress).SecondaryProgress = MaxValue;

                    AbilityValues[5] = MaxValue;
                    FindViewById<TextView>(Resource.Id.DollInfoAMStatus).Text = string.Format("{0} ({1})", (string)DollInfoDR["Armor"], AbilityGrade[6]);
                }
                else
                {
                    AbilityValues[5] = 0;

                    FindViewById<LinearLayout>(Resource.Id.DollInfoAMLayout).Visibility = ViewStates.Gone;
                }

                double[] DPS = ETC.CalcDPS(AbilityValues[1], AbilityValues[4], 0, AbilityValues[3], 3, int.Parse((string)DollInfoDR["Critical"]), 5);
                FindViewById<TextView>(Resource.Id.DollInfoDPSStatus).Text = $"{DPS[0]} ~ {DPS[1]}";

                if (ETC.UseLightTheme == true) SetCardTheme();
                if ((bool)DollInfoDR["HasVoice"] == true) FindViewById<LinearLayout>(Resource.Id.DollDBDetailVoiceLayout).Visibility = ViewStates.Visible;
                if ((bool)DollInfoDR["HasMod"] == true) FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSelectLayout).Visibility = ViewStates.Visible;

                LoadChart(ChartCompareList.SelectedItemPosition);

                ShowCardViewAnimation();
                ShowTitleSubLayout();
                HideFloatingActionButtonAnimation();

                LoadAD();
            }
            catch (WebException ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.RetryLoad_CauseNetwork, Snackbar.LengthShort, Android.Graphics.Color.DarkMagenta);
                InitLoadProcess(false);
                return;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBDetail_LoadDetailFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                InitLoadProgressBar.Visibility = ViewStates.Invisible;
            }
        }

        private void ShowTitleSubLayout()
        {
            if ((bool)DollInfoDR["HasVoice"] == true) FindViewById<LinearLayout>(Resource.Id.DollDBDetailVoiceLayout).Visibility = ViewStates.Visible;
            if ((bool)DollInfoDR["HasMod"] == true) FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSelectLayout).Visibility = ViewStates.Visible;
            FindViewById<LinearLayout>(Resource.Id.DollDBDetailExtraButtonLayout).Visibility = ViewStates.Visible;
        }

        private void HideFloatingActionButtonAnimation()
        {
            FABTimer.Stop();
            IsEnableFABMenu = false;

            PercentTableFAB.Hide();
            RefreshCacheFAB.Hide();
            MainFAB.Alpha = 0.3f;
            MainFAB.SetImageResource(Resource.Drawable.HideFloating_Icon);
        }

        private async Task LoadAD()
        {
            await Task.Delay(100);

            adview.AdListener = new ETC.ADViewListener();
            adview.Clickable = false;

            adview.LoadAd(new AdRequest.Builder().Build());
        }

        private void SetCardTheme()
        {
            int[] CardViewIds = { Resource.Id.DollDBDetailBasicInfoCardLayout, Resource.Id.DollDBDetailBuffCardLayout, Resource.Id.DollDBDetailSkillCardLayout, Resource.Id.DollDBDetailModSkillCardLayout, Resource.Id.DollDBDetailAbilityCardLayout, Resource.Id.DollDBDetailAbilityRadarChartCardLayout };

            foreach (int id in CardViewIds)
            {
                CardView cv = FindViewById<CardView>(id);

                cv.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                cv.Radius = 15.0f;
            }
        }

        private async Task ShowCardViewAnimation()
        {
            if (FindViewById<CardView>(Resource.Id.DollDBDetailBasicInfoCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.DollDBDetailBasicInfoCardLayout).Animate().Alpha(1.0f).SetDuration(500).Start();
            if (FindViewById<CardView>(Resource.Id.DollDBDetailBuffCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.DollDBDetailBuffCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(500).Start();
            if (FindViewById<CardView>(Resource.Id.DollDBDetailSkillCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.DollDBDetailSkillCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(1000).Start();

            CardView ModCardView = FindViewById<CardView>(Resource.Id.DollDBDetailModSkillCardLayout);

            switch (ModIndex)
            {
                case 0:
                case 1:
                default:
                    if (ModCardView.Alpha == 1.0f)
                    {
                        ModCardView.Animate().Alpha(0.0f).SetDuration(500).Start();
                        await Task.Delay(500);
                        ModCardView.Visibility = ViewStates.Gone;
                    }
                    break;
                case 2:
                case 3:
                    if (ModCardView.Alpha == 0.0f)
                    {
                        ModCardView.Visibility = ViewStates.Visible;
                        ModCardView.Animate().Alpha(1.0f).SetDuration(500).Start();
                    }
                    break;
            }

            if (FindViewById<CardView>(Resource.Id.DollDBDetailAbilityCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.DollDBDetailAbilityCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(1500).Start();
            if (FindViewById<CardView>(Resource.Id.DollDBDetailAbilityRadarChartCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.DollDBDetailAbilityRadarChartCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(2000).Start();
        }

        private void DollDBDetailModSelectButton_Click(object sender, EventArgs e)
        {
            try
            {
                ImageView ModButton = sender as ImageView;

                switch (ModButton.Id)
                {
                    case Resource.Id.DollDBDetailModSelect0:
                        ModIndex = 0;
                        break;
                    case Resource.Id.DollDBDetailModSelect1:
                        ModIndex = 1;
                        break;
                    case Resource.Id.DollDBDetailModSelect2:
                        ModIndex = 2;
                        break;
                    case Resource.Id.DollDBDetailModSelect3:
                        ModIndex = 3;
                        break;
                    default:
                        ModIndex = 0;
                        break;
                }

                foreach (int id in ModButtonIds) FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);

                ModButton.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                if (ModIndex > 0) DollGrade = (int)DollInfoDR["ModGrade"];
                else DollGrade = (int)DollInfoDR["Grade"];

                InitLoadProcess(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DollDBDetail_MODChangeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private double CalcReloadTime(DataRow dr, string type)
        {
            double result = 0;

            switch (type)
            {
                case "MG":
                    int tAS = 0;
                    if ((bool)dr["HasMod"] == true) tAS = int.Parse((((string)dr["AttackSpeed"]).Split('/')[1]).Split(';')[0]);
                    else tAS = int.Parse(((string)dr["AttackSpeed"]).Split('/')[1]);
                    result = (4 + (200 / tAS));
                    break;
                case "SG":
                    int tB = (int)dr["Bullet"];
                    result = (1.5 + (0.5 * tB));
                    break;
            }

            return result;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
            Finish();
        }

        internal class DollMaxAbility
        {
            public string AbilityType { get; set; }
            public int AbilityValue { get; set; }

            public DollMaxAbility(string type, int value)
            {
                AbilityType = type;
                AbilityValue = value;
            }
        }

        internal class DataModel
        {
            public ObservableCollection<DollMaxAbility> MaxAbilityList { get; set; }
            public ObservableCollection<DollMaxAbility> CompareAbilityList { get; set; }

            public DataModel(int CompareIndex)
            {
                MaxAbilityList = new ObservableCollection<DollMaxAbility>()
                {
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), AbilityValues[0]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), AbilityValues[1]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), AbilityValues[2]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), AbilityValues[3]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), AbilityValues[4]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), AbilityValues[5])
                };

                if (CompareIndex == 0)
                {
                    int index = 0;

                    switch (DollType)
                    {
                        case "HG":
                            index = 0;
                            break;
                        case "SMG":
                            index = 1;
                            break;
                        case "AR":
                            index = 2;
                            break;
                        case "RF":
                            index = 3;
                            break;
                        case "MG":
                            index = 4;
                            break;
                        case "SG":
                            index = 5;
                            break;
                    }

                    CompareAbilityList = new ObservableCollection<DollMaxAbility>()
                    {
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), ETC.Avg_List[index].HP),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), ETC.Avg_List[index].FR),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), ETC.Avg_List[index].EV),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), ETC.Avg_List[index].AC),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), ETC.Avg_List[index].AS),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), ETC.Avg_List[index].AM)
                    };
                }
                else
                {
                    CompareDollInfoDR = ETC.FindDataRow(ETC.DollList, "Name", CompareList[CompareIndex]);

                    string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };

                    int[] CompareAbilityValues = { 0, 0, 0, 0, 0, 0 };

                    for (int i = 0; i < abilities.Length; ++i)
                    {
                        string data = (string)CompareDollInfoDR[abilities[i]];

                        if ((bool)CompareDollInfoDR["HasMod"] == true) CompareAbilityValues[i] = int.Parse(data.Split(';')[0].Split('/')[1]);
                        else CompareAbilityValues[i] = int.Parse(data.Split('/')[1]);
                    }

                    if (DollType == "SG")
                    {
                        string data = (string)CompareDollInfoDR["Armor"];

                        if ((bool)CompareDollInfoDR["HasMod"] == true) CompareAbilityValues[5] = int.Parse(data.Split(';')[0].Split('/')[1]);
                        else CompareAbilityValues[5] = int.Parse(data.Split('/')[1]);
                    }
                    else CompareAbilityValues[5] = 0;

                    CompareAbilityList = new ObservableCollection<DollMaxAbility>()
                    {
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), CompareAbilityValues[0]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), CompareAbilityValues[1]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), CompareAbilityValues[2]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), CompareAbilityValues[3]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), CompareAbilityValues[4]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), CompareAbilityValues[5])
                    };
                }
            }
        }
    }
}