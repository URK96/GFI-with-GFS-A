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

        internal static Doll doll;
        internal static Doll c_doll;
        private DataRow DollInfoDR = null;
        private int ModIndex = 0;
        private int V_Costume_Index = 0;
        private List<string> VoiceList = new List<string>();

        internal static int Ability_Level = 1;
        private List<int> Level_List = new List<int>();
        internal static int Ability_Favor = 0;
        private List<string> Favor_List = new List<string>();
        internal static DollAbilitySet DAS;
        internal static int[] AbilityValues = new int[6];

        private bool IsExtraFeatureOpen = false;
        private bool IsOpenFABMenu = false;
        private bool IsEnableFABMenu = false;
        private bool IsChartLoad = false;

        private ScrollView ScrollLayout;
        private CoordinatorLayout SnackbarLayout;

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
        private Spinner AbilityLevelSelector;
        private Spinner AbilityFavorSelector;
        private Spinner ChartCompareList;
        private SfChart chart;

        int[] ModButtonIds = { Resource.Id.DollDBDetailModSelect0, Resource.Id.DollDBDetailModSelect1, Resource.Id.DollDBDetailModSelect2, Resource.Id.DollDBDetailModSelect3 };
        internal static List<string> CompareList;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.DollDBDetailLayout);

                await Task.Delay(100);

                DollInfoDR = ETC.FindDataRow(ETC.DollList, "DicNumber", Intent.GetIntExtra("DicNum", 0));
                doll = new Doll(DollInfoDR);

                DAS = new DollAbilitySet(doll.Type);

                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.DollDBDetailInitLoadProgress);
                SkillTableSubLayout = FindViewById<LinearLayout>(Resource.Id.DollDBDetailSkillAbilitySubLayout);
                ModSkillTableSubLayout = FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSkillAbilitySubLayout);

                FindViewById<Button>(Resource.Id.DollDBExtraFeatureButton).Click += ExtraMenuButton_Click;
                
                if (doll.HasMod == true)
                {
                    foreach (int id in ModButtonIds)
                    {
                        FindViewById<ImageButton>(id).Click += DollDBDetailModSelectButton_Click;
                        FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);
                    }

                    FindViewById<ImageButton>(ModButtonIds[0]).SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                    Button ModStoryButton = FindViewById<Button>(Resource.Id.DollDBDetailModStoryButton);
                    ModStoryButton.Visibility = ViewStates.Visible;
                    ModStoryButton.Click += ModStoryButton_Click;
                }

                FindViewById<ImageView>(Resource.Id.DollDBDetailSmallImage).Click += DollDBDetailSmallImage_Click;

                
                if (doll.HasVoice == true)
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
                AbilityLevelSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityLevelSelector);
                AbilityLevelSelector.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => 
                {
                    Ability_Level = Level_List[e.Position];
                    LoadAbility();
                };
                AbilityFavorSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityFavorSelector);
                AbilityFavorSelector.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => 
                {
                    switch (e.Position)
                    {
                        case 0:
                            Ability_Favor = 5;
                            break;
                        case 1:
                            Ability_Favor = 50;
                            break;
                        case 2:
                            Ability_Favor = 115;
                            break;
                        case 3:
                            Ability_Favor = 165;
                            break;
                        case 4:
                            Ability_Favor = 195;
                            break;
                    }
                    LoadAbility();
                };
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
                ListAbilityLevelFavor();

                InitLoadProcess(false);

                if ((ETC.Language.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DollDBDetail", true) == true)) ETC.RunHelpActivity(this, "DollDBDetail");
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void ModStoryButton_Click(object sender, EventArgs e)
        {
            try
            {
                string[] StoryList = new string[]
                {
                    $"{doll.Name} MOD Story 1",
                    $"{doll.Name} MOD Story 2",
                    $"{doll.Name} MOD Story 3",
                    $"{doll.Name} MOD Story 4"
                };

                var intent = new Intent(this, typeof(StoryReaderActivity));
                intent.PutExtra("Info", new string[] { "Sub", "ModStory", "0", StoryList.Length.ToString(), doll.DicNumber.ToString() });
                intent.PutExtra("List", StoryList);
                StartActivity(intent);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void ExtraMenuButton_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;

            switch (IsExtraFeatureOpen)
            {
                case false:
                    IsExtraFeatureOpen = true;
                    b.Text = "△△△";
                    FindViewById<LinearLayout>(Resource.Id.DollDBExtraFeatureLayout).Visibility = ViewStates.Visible;
                    break;
                case true:
                    IsExtraFeatureOpen = false;
                    b.Text = "▽▽▽";
                    FindViewById<LinearLayout>(Resource.Id.DollDBExtraFeatureLayout).Visibility = ViewStates.Gone;
                    break;
            }
        }

        private void ListAbilityLevelFavor()
        {
            try
            {
                Level_List.Clear();
                Favor_List.Clear();

                Favor_List.Add("0 ~ 10");
                Favor_List.Add("11 ~ 90");
                Favor_List.Add("91 ~ 140");

                switch (ModIndex)
                {
                    case 0:
                        for (int i = 1; i <= 100; ++i) Level_List.Add(i);
                        Favor_List.Add("141 ~ 150");
                        break;
                    case 1:
                        for (int i = 100; i <= 110; ++i) Level_List.Add(i);
                        Favor_List.Add("141 ~ 150");
                        break;
                    case 2:
                        for (int i = 110; i <= 115; ++i) Level_List.Add(i);
                        Favor_List.Add("141 ~ 150");
                        break;
                    case 3:
                        for (int i = 115; i <= 120; ++i) Level_List.Add(i);
                        Favor_List.Add("141 ~ 190");
                        Favor_List.Add("191 ~ 200");
                        break;
                }

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, Level_List.ToArray());
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                var adapter2 = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, Favor_List.ToArray());
                adapter2.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                AbilityLevelSelector.Adapter = adapter;
                AbilityFavorSelector.Adapter = adapter2;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "Error List Level", Snackbar.LengthShort);
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
                    Doll c_doll = new Doll(dr);

                    if (doll.DicNumber == c_doll.DicNumber) continue;
                    if (c_doll.Type != doll.Type) continue;

                    CompareList.Add(c_doll.Name);
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
            string FileName = $"{doll.DicNumber}.txt";
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

            radar.Label = doll.Name;
            radar.TooltipEnabled = true;

            chart.Series.Add(radar);

            RadarSeries radar2 = new RadarSeries();

            radar2.ItemsSource = model.CompareAbilityList;
            radar2.XBindingPath = "AbilityType";
            radar2.YBindingPath = "AbilityValue";
            radar2.DrawType = PolarChartDrawType.Line;
            radar2.Color = Android.Graphics.Color.Magenta;
            radar2.EnableAnimation = true;

            if (CompareIndex == 0) radar2.Label = $"{doll.Type}{Resources.GetString(Resource.String.DollDBDetail_RadarAverageString)}";
            else radar2.Label = c_doll.Name;

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
                intent.PutExtra("Info", new string[] { "Doll", doll.DicNumber.ToString() });
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
                        VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", "Doll", doll.krName, $"{doll.krName}_{voice}_JP.wav");
                        target = Path.Combine(ETC.CachePath, "Voices", "Doll", $"{doll.DicNumber}_{voice}_JP.gfdcache");
                        break;
                    case 1:
                        VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", "Doll", $"{doll.krName}_{V_Costume_Index - 1}", $"{doll.krName}_{V_Costume_Index - 1}_{voice}_JP.wav");
                        target = Path.Combine(ETC.CachePath, "Voices", "Doll", $"{doll.DicNumber}_{V_Costume_Index - 1}_{voice}_JP.gfdcache");
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
                /*V_Costume_List.Clear();
                V_Costume_List.Add($"Default:{(string)DollInfoDR["Voices"]}");
                if (DollInfoDR["CostumeVoices"] != DBNull.Value)
                {
                    if (ETC.IsDBNullOrBlank(DollInfoDR, "CostumeVoices") == false)
                        V_Costume_List.AddRange(((string)DollInfoDR["CostumeVoices"]).Split(','));
                }
                V_Costume_List.TrimExcess();*/

                List<string> V_C_List = new List<string>()
                {
                    "Default"
                };

                if (doll.CostumeVoices != null)
                {
                    for (int i = 0; i < (doll.CostumeVoices.Length / doll.CostumeVoices.Rank); ++i)
                        V_C_List.Add(doll.Costumes[int.Parse(doll.CostumeVoices[i, 0])]);
                }

                /*for (int i = 0; i < V_Costume_List.Count; ++i)
                {
                    if (i >= 1)
                    {
                        string[] Costumes = ((string)DollInfoDR["Costume"]).Split(';');
                        V_C_List.Add(Costumes[int.Parse(V_Costume_List[i].Split(':')[0])]);
                    }
                    else V_C_List.Add(V_Costume_List[i].Split(':')[0]);
                }*/

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
                switch(V_Costume_Index)
                {
                    case 0:
                        VoiceList.AddRange(doll.Voices);
                        break;
                    default:
                        VoiceList.AddRange(doll.CostumeVoices[V_Costume_Index - 1, 1].Split(';'));
                        break;
                }
                //VoiceList.AddRange(V_Costume_List[e.Position].Split(':')[1].Split(';'));
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

                string url = "";
                
                switch (fab.Id)
                {
                    case Resource.Id.SideLinkFAB1:
                        url = $"https://namu.wiki/w/{doll.krName}(소녀전선)";
                        break;
                    case Resource.Id.SideLinkFAB2:
                        url = $"http://gf.inven.co.kr/dataninfo/dolls/detail.php?d=126&c={doll.DicNumber}";
                        break;
                    case Resource.Id.SideLinkFAB3:
                        url = $"https://girlsfrontline.kr/doll/{doll.DicNumber}";
                        break;
                }

                Intent intent = new Intent(this, typeof(WebBrowserActivity));
                intent.PutExtra("url", url);
                StartActivity(intent);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
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
                DollImageViewer.PutExtra("Data", $"{doll.DicNumber};{ModIndex}");
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
                        string url = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", $"{doll.DicNumber}.png");
                        string target = Path.Combine(ETC.CachePath, "Doll", "Normal", $"{doll.DicNumber}.gfdcache");

                        if ((File.Exists(target) == false) || (IsRefresh == true))
                        {
                            using (WebClient wc = new WebClient())
                            {
                                await wc.DownloadFileTaskAsync(url, target);
                            }
                        }

                        Drawable drawable = Drawable.CreateFromPath(target);
                        drawable.SetAlpha(40);
                        FindViewById<RelativeLayout>(Resource.Id.DollDBDetailMainLayout).Background = drawable;
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(this, ex.ToString());
                    }
                }

                string FileName = doll.DicNumber.ToString();
                if (ModIndex == 3) FileName += "_M";

                try
                {
                    string url = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", $"{FileName}.png");
                    string target = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", $"{FileName}.gfdcache");

                    if ((File.Exists(target) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(url, target);
                        }
                    }

                    ImageView DollSmallImage = FindViewById<ImageView>(Resource.Id.DollDBDetailSmallImage);
                    DollSmallImage.SetImageDrawable(Drawable.CreateFromPath(target));
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }

                FindViewById<TextView>(Resource.Id.DollDBDetailDollName).Text = doll.Name;
                FindViewById<TextView>(Resource.Id.DollDBDetailDollDicNumber).Text = $"No. {doll.DicNumber}";
                FindViewById<TextView>(Resource.Id.DollDBDetailDollProductTime).Text = ETC.CalcTime(doll.ProductTime);
                FindViewById<TextView>(Resource.Id.DollDBDetailDollProductDialog).Text = doll.ProductDialog;


                // 인형 기본 정보 초기화

                int[] GradeStarIds = { Resource.Id.DollDBDetailInfoGrade1, Resource.Id.DollDBDetailInfoGrade2, Resource.Id.DollDBDetailInfoGrade3, Resource.Id.DollDBDetailInfoGrade4, Resource.Id.DollDBDetailInfoGrade5 };

                int Grade = 0;

                if (ModIndex > 0) Grade = doll.ModGrade;
                else Grade = doll.Grade;

                if (Grade == 0)
                {
                    for (int i = 1; i < GradeStarIds.Length; ++i) FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;
                    FindViewById<ImageView>(GradeStarIds[0]).SetImageResource(Resource.Drawable.Grade_Star_EX);
                }
                else
                {
                    for (int i = Grade; i < GradeStarIds.Length; ++i) FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;
                    for (int i = 0; i < Grade; ++i)
                    {
                        FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Visible;
                        FindViewById<ImageView>(GradeStarIds[i]).SetImageResource(Resource.Drawable.Grade_Star);
                    }
                }

                FindViewById<TextView>(Resource.Id.DollDBDetailInfoType).Text = doll.Type;
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoName).Text = doll.Name;
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoNickName).Text = doll.NickName;
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoIllustrator).Text = doll.Illustrator;
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoVoiceActor).Text = doll.VoiceActor;
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoRealModel).Text = doll.RealModel;
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoCountry).Text = doll.Country;
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoHowToGain).Text = (string)DollInfoDR["DropEvent"];


                // 인형 버프 정보 초기화

                int[] BuffIds = { Resource.Id.DollDBDetailBuff1, Resource.Id.DollDBDetailBuff2, Resource.Id.DollDBDetailBuff3, Resource.Id.DollDBDetailBuff4, Resource.Id.DollDBDetailBuff5, Resource.Id.DollDBDetailBuff6, Resource.Id.DollDBDetailBuff7, Resource.Id.DollDBDetailBuff8, Resource.Id.DollDBDetailBuff9 };

                int[] Buff_Data = new int[9];

                if (ModIndex == 0) Buff_Data = doll.BuffFormation;
                else Buff_Data = doll.ModBuffFormation;

                for (int i = 0; i < Buff_Data.Length; ++i)
                {
                    Android.Graphics.Color color;

                    switch (Buff_Data[i])
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

                if (ModIndex >= 1) Buff = doll.ModBuffInfo;
                else Buff = doll.BuffInfo;

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
                    FindViewById<TextView>(BuffDetailIds[i]).Text = EffectString[i].ToString();

                var EffectTypeView = FindViewById<TextView>(Resource.Id.DollDBDetailEffectType);

                if (doll.BuffType[0] == "ALL")
                    EffectTypeView.Text = Resources.GetString(Resource.String.DollDBDetail_BuffType_All);
                else
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (string type in doll.BuffType)
                        sb.Append($"{type} ");
                    EffectTypeView.Text = $"{sb.ToString()} {Resources.GetString(Resource.String.DollDBDetail_BuffType_ConfirmString)}";
                }


                // 인형 스킬 정보 초기화

                string SkillName = doll.SkillName;

                try
                {
                    string url = Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", $"{SkillName}.png");
                    string target = Path.Combine(ETC.CachePath, "Doll", "Skill", $"{SkillName}.gfdcache");

                    if ((File.Exists(target) == false) || (IsRefresh == true))
                    {

                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFile(url, target);
                        }

                    }
                    FindViewById<ImageView>(Resource.Id.DollDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(target));
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

                string[] SkillAbilities = doll.SkillEffect;
                string[] SkillMags;

                if (ModIndex == 0 ) SkillMags = doll.SkillMag;
                else SkillMags = doll.SkillMagAfterMod;

                TextView SkillInitCoolTime = FindViewById<TextView>(Resource.Id.DollDBDetailSkillInitCoolTime);
                SkillInitCoolTime.SetTextColor(Android.Graphics.Color.Orange);
                SkillInitCoolTime.Text = SkillMags[0];

                TextView SkillCoolTime = FindViewById<TextView>(Resource.Id.DollDBDetailSkillCoolTime);
                SkillCoolTime.SetTextColor(Android.Graphics.Color.DarkOrange);
                SkillCoolTime.Text = SkillMags[1];

                FindViewById<TextView>(Resource.Id.DollDBDetailSkillExplain).Text = doll.SkillExplain;

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

                if (doll.HasMod == true)
                {
                    string MSkillName = doll.ModSkillName;

                    try
                    {
                        string url = Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", $"{MSkillName}.png");
                        string target = Path.Combine(ETC.CachePath, "Doll", "Skill", $"{MSkillName}.gfdcache");

                        if ((File.Exists(target) == false) || (IsRefresh == true))
                        {
                            using (WebClient wc = new WebClient())
                            {
                                wc.DownloadFile(url, target);
                            }
                        }

                        FindViewById<ImageView>(Resource.Id.DollDBDetailModSkillIcon).SetImageDrawable(Drawable.CreateFromPath(target));
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(this, ex.ToString());
                    }

                    FindViewById<TextView>(Resource.Id.DollDBDetailModSkillName).Text = MSkillName;
                    FindViewById<TextView>(Resource.Id.DollDBDetailModSkillExplain).Text = doll.ModSkillExplain;

                    string[] MSkillAbilities = doll.ModSkillEffect;
                    string[] MSkillMags = doll.ModSkillMag;

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

                        ability.Text = MSkillAbilities[i];
                        ability.Gravity = GravityFlags.Center;
                        mag.Text = MSkillMags[i];
                        mag.Gravity = GravityFlags.Center;

                        layout.AddView(ability);
                        layout.AddView(mag);

                        ModSkillTableSubLayout.AddView(layout);
                    }
                }


                // 인형 능력치 초기화

                await LoadAbility();

                double[] DPS = ETC.CalcDPS(AbilityValues[1], AbilityValues[4], 0, AbilityValues[3], 3, int.Parse(doll.Abilities["Critical"]), 5);
                FindViewById<TextView>(Resource.Id.DollInfoDPSStatus).Text = $"{DPS[0].ToString("F2")} ~ {DPS[1].ToString("F2")}";

                if (ETC.UseLightTheme == true) SetCardTheme();

                LoadChart(ChartCompareList.SelectedItemPosition);

                ShowCardViewVisibility();
                ShowTitleSubLayout();
                HideFloatingActionButtonAnimation();

                //LoadAD();
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

        private async Task LoadAbility()
        {
            await Task.Delay(10);

            try
            {
                string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
                int[] Progresses = { Resource.Id.DollInfoHPProgress, Resource.Id.DollInfoFRProgress, Resource.Id.DollInfoEVProgress, Resource.Id.DollInfoACProgress, Resource.Id.DollInfoASProgress };
                int[] ProgressMaxTexts = { Resource.Id.DollInfoHPProgressMax, Resource.Id.DollInfoFRProgressMax, Resource.Id.DollInfoEVProgressMax, Resource.Id.DollInfoACProgressMax, Resource.Id.DollInfoASProgressMax };
                int[] StatusTexts = { Resource.Id.DollInfoHPStatus, Resource.Id.DollInfoFRStatus, Resource.Id.DollInfoEVStatus, Resource.Id.DollInfoACStatus, Resource.Id.DollInfoASStatus };

                string[] grow_ratio = doll.Abilities["Grow"].Split(';');

                for (int i = 0; i < Progresses.Length; ++i)
                {
                    FindViewById<TextView>(ProgressMaxTexts[i]).Text = FindViewById<ProgressBar>(Progresses[i]).Max.ToString();

                    string[] basic_ratio = doll.Abilities[abilities[i]].Split(';');

                    int value = 0;

                    switch (ModIndex)
                    {
                        case 0:
                            value = DAS.CalcAbility(abilities[i], int.Parse(basic_ratio[0]), int.Parse(grow_ratio[0]), Ability_Level, Ability_Favor, false);
                            break;
                        case 1:
                        case 2:
                        case 3:
                            value = DAS.CalcAbility(abilities[i], int.Parse(basic_ratio[1]), int.Parse(grow_ratio[1]), Ability_Level, Ability_Favor, false);
                            break;
                    }

                    ProgressBar pb = FindViewById<ProgressBar>(Progresses[i]);

                    pb.Progress = value;

                    AbilityValues[i] = value;

                    FindViewById<TextView>(StatusTexts[i]).Text = $"{value} ({doll.AbilityGrade[i]})";
                }

                if ((doll.Type == "MG") || (doll.Type == "SG"))
                {
                    FindViewById<LinearLayout>(Resource.Id.DollInfoBulletLayout).Visibility = ViewStates.Visible;
                    FindViewById<LinearLayout>(Resource.Id.DollInfoReloadLayout).Visibility = ViewStates.Visible;

                    double ReloadTime = CalcReloadTime(doll, doll.Type, AbilityValues[4]);
                    int Bullet = 0;

                    if (doll.HasMod == false) Bullet = int.Parse(doll.Abilities["Bullet"]);
                    else Bullet = int.Parse(doll.Abilities["Bullet"].Split(';')[ModIndex]);

                    FindViewById<TextView>(Resource.Id.DollInfoBulletProgressMax).Text = FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress).Max.ToString();

                    FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress).Progress = Bullet;
                    FindViewById<TextView>(Resource.Id.DollInfoBulletStatus).Text = Bullet.ToString();
                    FindViewById<TextView>(Resource.Id.DollInfoReloadStatus).Text = $"{ReloadTime} {Resources.GetString(Resource.String.Time_Second)}";
                }
                else
                {
                    FindViewById<LinearLayout>(Resource.Id.DollInfoBulletLayout).Visibility = ViewStates.Gone;
                    FindViewById<LinearLayout>(Resource.Id.DollInfoReloadLayout).Visibility = ViewStates.Gone;
                }

                if (doll.Type == "SG")
                {
                    FindViewById<LinearLayout>(Resource.Id.DollInfoAMLayout).Visibility = ViewStates.Visible;

                    FindViewById<TextView>(Resource.Id.DollInfoAMProgressMax).Text = FindViewById<ProgressBar>(Resource.Id.DollInfoAMProgress).Max.ToString();

                    string[] basic_ratio = doll.Abilities["Armor"].Split(';');
                    int value = 0;

                    switch (ModIndex)
                    {
                        case 0:
                            value = DAS.CalcAbility("Armor", int.Parse(basic_ratio[0]), int.Parse(grow_ratio[0]), Ability_Level, Ability_Favor, false);
                            break;
                        case 1:
                        case 2:
                        case 3:
                            value = DAS.CalcAbility("Armor", int.Parse(basic_ratio[1]), int.Parse(grow_ratio[1]), Ability_Level, Ability_Favor, true);
                            break;
                    }

                    FindViewById<ProgressBar>(Resource.Id.DollInfoAMProgress).Progress = value;

                    AbilityValues[5] = value;
                    FindViewById<TextView>(Resource.Id.DollInfoAMStatus).Text = $"{value} ({doll.AbilityGrade[6]})";
                }
                else
                {
                    AbilityValues[5] = 0;
                    FindViewById<LinearLayout>(Resource.Id.DollInfoAMLayout).Visibility = ViewStates.Gone;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "Error Load Ability", Snackbar.LengthShort);
            }
        }

        private void ShowTitleSubLayout()
        {
            if (doll.HasVoice == true) FindViewById<LinearLayout>(Resource.Id.DollDBDetailVoiceLayout).Visibility = ViewStates.Visible;
            if (doll.HasMod == true) FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSelectLayout).Visibility = ViewStates.Visible;
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

        private void SetCardTheme()
        {
            int[] CardViewIds = 
            {
                Resource.Id.DollDBDetailBasicInfoCardLayout,
                Resource.Id.DollDBDetailBuffCardLayout,
                Resource.Id.DollDBDetailSkillCardLayout,
                Resource.Id.DollDBDetailModSkillCardLayout,
                Resource.Id.DollDBDetailAbilityCardLayout,
                Resource.Id.DollDBDetailAbilityRadarChartCardLayout
            };

            foreach (int id in CardViewIds)
            {
                CardView cv = FindViewById<CardView>(id);

                cv.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                cv.Radius = 15.0f;
            }
        }

        private void ShowCardViewVisibility()
        {
            FindViewById<CardView>(Resource.Id.DollDBDetailBasicInfoCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.DollDBDetailBuffCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.DollDBDetailSkillCardLayout).Visibility = ViewStates.Visible;

            CardView ModCardView = FindViewById<CardView>(Resource.Id.DollDBDetailModSkillCardLayout);

            switch (ModIndex)
            {
                case 0:
                case 1:
                default:
                    ModCardView.Visibility = ViewStates.Gone;
                    break;
                case 2:
                case 3:
                    ModCardView.Visibility = ViewStates.Visible;
                    break;
            }

            FindViewById<CardView>(Resource.Id.DollDBDetailAbilityCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.DollDBDetailAbilityRadarChartCardLayout).Visibility = ViewStates.Visible;
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

                foreach (int id in ModButtonIds)
                    FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);

                ModButton.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                ListAbilityLevelFavor();

                InitLoadProcess(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DollDBDetail_MODChangeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private double CalcReloadTime(Doll doll, string type, int AttackSpeed)
        {
            double result = 0;

            switch (type)
            {
                case "MG":
                    int tAS = AttackSpeed;
                    if (tAS == 0) result = 0;
                    else result = 4 + (200 / tAS);
                    break;
                case "SG":
                    int tB = int.Parse(doll.Abilities["Bullet"]);
                    result = 1.5 + (0.5 * tB);
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

                    switch (doll.Type)
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
                    c_doll = new Doll(ETC.FindDataRow(ETC.DollList, "Name", CompareList[CompareIndex]));

                    string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };

                    int[] CompareAbilityValues = { 0, 0, 0, 0, 0, 0 };
                    int grow_ratio = int.Parse(c_doll.Abilities["Grow"].Split(';')[0]);

                    for (int i = 0; i < abilities.Length; ++i)
                    {
                        int base_ratio = int.Parse(c_doll.Abilities[abilities[i]].Split(';')[0]);

                        CompareAbilityValues[i] = DAS.CalcAbility(abilities[i], base_ratio, grow_ratio, Ability_Level, Ability_Favor, false);
                    }

                    if (doll.Type == "SG")
                    {
                        int base_ratio = int.Parse(c_doll.Abilities["Armor"].Split(';')[0]);

                        CompareAbilityValues[5] = DAS.CalcAbility("Armor", base_ratio, grow_ratio, Ability_Level, Ability_Favor, false);
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