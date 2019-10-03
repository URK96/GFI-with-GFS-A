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
        internal static Doll cDoll;
        private DataRow DollInfoDR = null;
        private int modIndex = 0;
        private int vCostumeIndex = 0;
        private List<string> VoiceList = new List<string>();

        internal static int abilityLevel = 1;
        private List<int> Level_List = new List<int>();
        internal static int abilityFavor = 0;
        private List<string> Favor_List = new List<string>();
        internal static DollAbilitySet DAS;
        internal static int[] AbilityValues = new int[6];

        private bool IsExtraFeatureOpen = false;
        private bool isOpenFABMenu = false;
        private bool isEnableFABMenu = false;
        private bool isChartLoad = false;

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

                if (ETC.UseLightTheme)
                    SetTheme(Resource.Style.GFS_Light);

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
                
                if (doll.HasMod)
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

                if (doll.HasVoice)
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
                PercentTableFAB.Visibility = (doll.ProductTime == 0) ? ViewStates.Gone : ViewStates.Visible;
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
                    abilityLevel = Level_List[e.Position];
                    _ = LoadAbility();
                };
                AbilityFavorSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityFavorSelector);
                AbilityFavorSelector.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => 
                {
                    switch (e.Position)
                    {
                        case 0:
                            abilityFavor = 5;
                            break;
                        case 1:
                            abilityFavor = 50;
                            break;
                        case 2:
                            abilityFavor = 115;
                            break;
                        case 3:
                            abilityFavor = 165;
                            break;
                        case 4:
                            abilityFavor = 195;
                            break;
                    }
                    _ = LoadAbility();
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

                _ = InitLoadProcess(false);

                if ((ETC.Language.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DollDBDetail", true)))
                    ETC.RunHelpActivity(this, "DollDBDetail");
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
                ETC.LogError(ex, this);
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

                switch (modIndex)
                {
                    case 0:
                        for (int i = 1; i <= 100; ++i)
                            Level_List.Add(i);

                        Favor_List.Add("141 ~ 150");
                        break;
                    case 1:
                        for (int i = 100; i <= 110; ++i)
                            Level_List.Add(i);

                        Favor_List.Add("141 ~ 150");
                        break;
                    case 2:
                        for (int i = 110; i <= 115; ++i)
                            Level_List.Add(i);

                        Favor_List.Add("141 ~ 150");
                        break;
                    case 3:
                        for (int i = 115; i <= 120; ++i)
                            Level_List.Add(i);

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
                ETC.LogError(ex, this);
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
                        if (isEnableFABMenu == false) return;
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
                ETC.LogError(ex, this);
            }
        }

        private void InitCompareList()
        {
            Doll cDoll;

            try
            {
                CompareList = new List<string>();
                CompareList.Add("Type Average");

                foreach (DataRow dr in ETC.DollList.Rows)
                {
                    cDoll = new Doll(dr);

                    if (doll.DicNumber == cDoll.DicNumber)
                        continue;
                    if (cDoll.Type != doll.Type)
                        continue;

                    CompareList.Add(cDoll.Name);
                }

                CompareList.TrimExcess();

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, CompareList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                ChartCompareList.Adapter = adapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, "Test Error", Snackbar.LengthShort);
            }
        }

        private void ChartCompareList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            _ = LoadChart(e.Position);
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
                        data = await wc.DownloadStringTaskAsync(Path.Combine(ETC.Server, "Data", "Text", "Gun", "ModelData", FileName));
                }).Wait();

                ad.SetMessage(data);
                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private void RefreshCacheFAB_Click(object sender, EventArgs e)
        {
            _ = InitLoadProcess(true);
        }

        private async Task LoadChart(int CompareIndex)
        {
            await Task.Delay(100);

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

            chart.Legend.LabelStyle.TextColor = ETC.UseLightTheme ? Android.Graphics.Color.DarkGray : Android.Graphics.Color.LightGray;

            /*if (ETC.UseLightTheme == true)
                chart.Legend.LabelStyle.TextColor = Android.Graphics.Color.DarkGray;
            else
                chart.Legend.LabelStyle.TextColor = Android.Graphics.Color.LightGray;*/

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

            radar2.Label = (CompareIndex == 0) ? $"{doll.Type}{Resources.GetString(Resource.String.DollDBDetail_RadarAverageString)}" : cDoll.Name;

            /*if (CompareIndex == 0)
                radar2.Label = $"{doll.Type}{Resources.GetString(Resource.String.DollDBDetail_RadarAverageString)}";
            else
                radar2.Label = cDoll.Name;*/

            radar2.TooltipEnabled = true;

            chart.Series.Add(radar2);

            isChartLoad = true;
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                if (isChartLoad)
                    _ = LoadChart(ChartCompareList.SelectedItemPosition);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
                ETC.LogError(ex, this);
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

                switch (vCostumeIndex)
                {
                    case 0:
                        VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", "Doll", doll.krName, $"{doll.krName}_{voice}_JP.wav");
                        target = Path.Combine(ETC.CachePath, "Voices", "Doll", $"{doll.DicNumber}_{voice}_JP.gfdcache");
                        break;
                    default:
                        VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", "Doll", $"{doll.krName}_{vCostumeIndex - 1}", $"{doll.krName}_{vCostumeIndex - 1}_{voice}_JP.wav");
                        target = Path.Combine(ETC.CachePath, "Voices", "Doll", $"{doll.DicNumber}_{vCostumeIndex - 1}_{voice}_JP.gfdcache");
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
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.VoiceStreaming_Error, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.VoiceStreaming_PlayError, Snackbar.LengthShort, Android.Graphics.Color.DarkCyan);
            }
        }

        private void InitializeVoiceList()
        {
            try
            {
                List<string> vcList = new List<string>()
                {
                    "Default"
                };

                if (doll.CostumeVoices != null)
                    for (int i = 0; i < (doll.CostumeVoices.Length / doll.CostumeVoices.Rank); ++i)
                        vcList.Add(doll.Costumes[int.Parse(doll.CostumeVoices[i, 0])]);

                vcList.TrimExcess();

                var adater = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, vcList);
                adater.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                VoiceCostumeSelector.Adapter = adater;

                VoiceCostumeSelector.SetSelection(0);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.VoiceList_InitError, ToastLength.Short).Show();
            }
        }

        private void VoiceCostumeSelector_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                vCostumeIndex = e.Position;

                VoiceList.Clear();

                switch(vCostumeIndex)
                {
                    case 0:
                        VoiceList.AddRange(doll.Voices);
                        break;
                    default:
                        VoiceList.AddRange(doll.CostumeVoices[vCostumeIndex - 1, 1].Split(';'));
                        break;
                }

                VoiceList.TrimExcess();

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, VoiceList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                VoiceSelector.Adapter = adapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                MainFAB_Click(MainFAB, new EventArgs());
            }
        }

        private void MainFAB_Click(object sender, EventArgs e)
        {
            if (!isEnableFABMenu)
            {
                MainFAB.SetImageResource(Resource.Drawable.SideLinkIcon);
                isEnableFABMenu = true;
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
                    switch (isOpenFABMenu)
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
                            isOpenFABMenu = true;
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
                            isOpenFABMenu = false;
                            PercentTableFAB.Show();
                            RefreshCacheFAB.Show();
                            FABTimer.Start();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                    ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeSubMenuError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                }
            }
        }

        private void DollDBDetailSmallImage_Click(object sender, EventArgs e)
        {
            try
            {
                var DollImageViewer = new Intent(this, typeof(DollDBImageViewer));
                DollImageViewer.PutExtra("Data", $"{doll.DicNumber};{modIndex}");
                StartActivity(DollImageViewer);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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

                if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false))
                {
                    try
                    {
                        string url = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", $"{doll.DicNumber}.png");
                        string target = Path.Combine(ETC.CachePath, "Doll", "Normal", $"{doll.DicNumber}.gfdcache");

                        if (!File.Exists(target) || IsRefresh)
                        {
                            using (WebClient wc = new WebClient())
                                await wc.DownloadFileTaskAsync(url, target);
                        }

                        Drawable drawable = Drawable.CreateFromPath(target);
                        drawable.SetAlpha(40);

                        FindViewById<RelativeLayout>(Resource.Id.DollDBDetailMainLayout).Background = drawable;
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex, this);
                    }
                }

                string FileName = doll.DicNumber.ToString();

                if (modIndex == 3)
                    FileName += "_M";

                try
                {
                    string url = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", $"{FileName}.png");
                    string target = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", $"{FileName}.gfdcache");

                    if (!File.Exists(target) || IsRefresh == true)
                    {
                        using (WebClient wc = new WebClient())
                            await wc.DownloadFileTaskAsync(url, target);
                    }

                    ImageView DollSmallImage = FindViewById<ImageView>(Resource.Id.DollDBDetailSmallImage);
                    DollSmallImage.SetImageDrawable(Drawable.CreateFromPath(target));
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                FindViewById<TextView>(Resource.Id.DollDBDetailDollName).Text = doll.Name;
                FindViewById<TextView>(Resource.Id.DollDBDetailDollDicNumber).Text = $"No. {doll.DicNumber}";
                FindViewById<TextView>(Resource.Id.DollDBDetailDollProductTime).Text = ETC.CalcTime(doll.ProductTime);
                FindViewById<TextView>(Resource.Id.DollDBDetailDollProductDialog).Text = doll.ProductDialog;


                // 인형 기본 정보 초기화

                int[] GradeStarIds = 
                {
                    Resource.Id.DollDBDetailInfoGrade1,
                    Resource.Id.DollDBDetailInfoGrade2,
                    Resource.Id.DollDBDetailInfoGrade3,
                    Resource.Id.DollDBDetailInfoGrade4,
                    Resource.Id.DollDBDetailInfoGrade5,
                    Resource.Id.DollDBDetailInfoGrade6
                };

                int Grade = (modIndex > 0) ? doll.ModGrade : doll.Grade;

                if (Grade == 0)
                {
                    for (int i = 1; i < GradeStarIds.Length; ++i)
                        FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;

                    FindViewById<ImageView>(GradeStarIds[0]).SetImageResource(Resource.Drawable.Grade_Star_EX);
                }
                else
                {
                    for (int i = Grade; i < GradeStarIds.Length; ++i)
                        FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;

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

                int[] buffIds = 
                {
                    Resource.Id.DollDBDetailBuff1, Resource.Id.DollDBDetailBuff2, Resource.Id.DollDBDetailBuff3,
                    Resource.Id.DollDBDetailBuff4, Resource.Id.DollDBDetailBuff5, Resource.Id.DollDBDetailBuff6,
                    Resource.Id.DollDBDetailBuff7, Resource.Id.DollDBDetailBuff8, Resource.Id.DollDBDetailBuff9
                };
                int[] buffData = (modIndex == 0) ? doll.BuffFormation : doll.ModBuffFormation;

                for (int i = 0; i < buffData.Length; ++i)
                {
                    Android.Graphics.Color color;

                    switch (buffData[i])
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

                    FindViewById<View>(buffIds[i]).SetBackgroundColor(color);
                }

                int[] buffIconIds = { Resource.Id.DollDBDetailBuffIcon1, Resource.Id.DollDBDetailBuffIcon2 };
                int[] buffIconNameIds = { Resource.Id.DollDBDetailBuffName1, Resource.Id.DollDBDetailBuffName2 };
                int[] buffDetailIds = { Resource.Id.DollDBDetailBuffDetail1, Resource.Id.DollDBDetailBuffDetail2 };

                string[] buff = (modIndex >= 1) ? doll.ModBuffInfo : doll.BuffInfo;
                string[] buffType = buff[0].Split(',');

                FindViewById<LinearLayout>(Resource.Id.DollDBDetailBuffLayout2).Visibility = (buffType.Length == 1) ? ViewStates.Gone : ViewStates.Visible;

                for (int i = 0; i < buffType.Length; ++i)
                {
                    int id = 0;
                    string name = "";

                    switch (buffType[i])
                    {
                        case "AC":
                            id = ETC.UseLightTheme ? Resource.Drawable.AC_Icon_WhiteTheme : Resource.Drawable.AC_Icon;
                            name = Resources.GetString(Resource.String.Common_AC);
                            break;
                        case "AM":
                            id = ETC.UseLightTheme ? Resource.Drawable.AM_Icon_WhiteTheme : Resource.Drawable.AM_Icon;
                            name = Resources.GetString(Resource.String.Common_AM);
                            break;
                        case "AS":
                            id = ETC.UseLightTheme ? Resource.Drawable.AS_Icon_WhiteTheme : Resource.Drawable.AS_Icon;
                            name = Resources.GetString(Resource.String.Common_AS);
                            break;
                        case "CR":
                            id = ETC.UseLightTheme ? Resource.Drawable.CR_Icon_WhiteTheme : Resource.Drawable.CR_Icon;
                            name = Resources.GetString(Resource.String.Common_CR);
                            break;
                        case "EV":
                            id = ETC.UseLightTheme ? Resource.Drawable.EV_Icon_WhiteTheme : Resource.Drawable.EV_Icon;
                            name = Resources.GetString(Resource.String.Common_EV);
                            break;
                        case "FR":
                            id = ETC.UseLightTheme ? Resource.Drawable.FR_Icon_WhiteTheme : Resource.Drawable.FR_Icon;
                            name = Resources.GetString(Resource.String.Common_FR);
                            break;
                        case "CL":
                            id = ETC.UseLightTheme ? Resource.Drawable.CL_Icon_WhiteTheme : Resource.Drawable.CL_Icon;
                            name = Resources.GetString(Resource.String.Common_CL);
                            break;
                        default:
                            break;
                    }

                    FindViewById<ImageView>(buffIconIds[i]).SetImageResource(id);
                    FindViewById<TextView>(buffIconNameIds[i]).Text = name;
                }

                StringBuilder sb1 = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();
                StringBuilder[] EffectString = { sb1, sb2 };

                for (int i = 1; i < buff.Length; ++i)
                {
                    string[] s = buff[i].Split(',');

                    for (int j = 0; j < s.Length; ++j)
                    {
                        EffectString[j].Append(s[j]);
                        EffectString[j].Append("%");

                        if (i < (buff.Length - 1))
                            EffectString[j].Append(" | ");
                    }
                }

                for (int i = 0; i < buffType.Length; ++i)
                    FindViewById<TextView>(buffDetailIds[i]).Text = EffectString[i].ToString();

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

                    if (!File.Exists(target) || IsRefresh)
                    {
                        using (WebClient wc = new WebClient())
                            wc.DownloadFile(url, target);
                    }

                    FindViewById<ImageView>(Resource.Id.DollDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(target));
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                FindViewById<TextView>(Resource.Id.DollDBDetailSkillName).Text = SkillName;

                if (ETC.UseLightTheme)
                {
                    FindViewById<ImageView>(Resource.Id.DollDBDetailSkillInitCoolTimeIcon).SetImageResource(Resource.Drawable.FirstCoolTime_Icon_WhiteTheme);
                    FindViewById<ImageView>(Resource.Id.DollDBDetailSkillCoolTimeIcon).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                }

                string[] SkillAbilities = doll.SkillEffect;
                string[] SkillMags = (modIndex == 0) ? doll.SkillMag : doll.SkillMagAfterMod;

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
                    LinearLayout layout = new LinearLayout(this)
                    {
                        Orientation = Android.Widget.Orientation.Horizontal,
                        LayoutParameters = FindViewById<LinearLayout>(Resource.Id.DollDBDetailSkillAbilityTopLayout).LayoutParameters
                    };

                    TextView ability = new TextView(this)
                    {
                        LayoutParameters = FindViewById<TextView>(Resource.Id.DollDBDetailSkillAbilityTopText1).LayoutParameters,
                        Text = SkillAbilities[i],
                        Gravity = GravityFlags.Center
                    };
                    TextView mag = new TextView(this)
                    {
                        LayoutParameters = FindViewById<TextView>(Resource.Id.DollDBDetailSkillAbilityTopText2).LayoutParameters,
                        Text = SkillMags[i],
                        Gravity = GravityFlags.Center
                    };

                    layout.AddView(ability);
                    layout.AddView(mag);

                    SkillTableSubLayout.AddView(layout);
                }


                // 인형 Mod 스킬 정보 초기화

                if (doll.HasMod)
                {
                    string mSkillName = doll.ModSkillName;

                    try
                    {
                        string url = Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", $"{mSkillName}.png");
                        string target = Path.Combine(ETC.CachePath, "Doll", "Skill", $"{mSkillName}.gfdcache");

                        if (!File.Exists(target) || IsRefresh)
                        {
                            using (WebClient wc = new WebClient())
                                wc.DownloadFile(url, target);
                        }

                        FindViewById<ImageView>(Resource.Id.DollDBDetailModSkillIcon).SetImageDrawable(Drawable.CreateFromPath(target));
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex, this);
                    }

                    FindViewById<TextView>(Resource.Id.DollDBDetailModSkillName).Text = mSkillName;
                    FindViewById<TextView>(Resource.Id.DollDBDetailModSkillExplain).Text = doll.ModSkillExplain;

                    string[] mSkillAbilities = doll.ModSkillEffect;
                    string[] mSkillMags = doll.ModSkillMag;

                    ModSkillTableSubLayout.RemoveAllViews();

                    for (int i = 0; i < mSkillAbilities.Length; ++i)
                    {
                        LinearLayout layout = new LinearLayout(this)
                        {
                            Orientation = Android.Widget.Orientation.Horizontal,
                            LayoutParameters = FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSkillAbilityTopLayout).LayoutParameters
                        };

                        TextView ability = new TextView(this)
                        {
                            LayoutParameters = FindViewById<TextView>(Resource.Id.DollDBDetailModSkillAbilityTopText1).LayoutParameters,
                            Text = mSkillAbilities[i],
                            Gravity = GravityFlags.Center
                        };
                        TextView mag = new TextView(this)
                        {
                            LayoutParameters = FindViewById<TextView>(Resource.Id.DollDBDetailModSkillAbilityTopText2).LayoutParameters,
                            Text = mSkillMags[i],
                            Gravity = GravityFlags.Center
                        };

                        layout.AddView(ability);
                        layout.AddView(mag);

                        ModSkillTableSubLayout.AddView(layout);
                    }
                }


                // 인형 능력치 초기화

                await LoadAbility();

                double[] DPS = ETC.CalcDPS(AbilityValues[1], AbilityValues[4], 0, AbilityValues[3], 3, int.Parse(doll.Abilities["Critical"]), 5);
                FindViewById<TextView>(Resource.Id.DollInfoDPSStatus).Text = $"{DPS[0].ToString("F2")} ~ {DPS[1].ToString("F2")}";

                if (ETC.UseLightTheme)
                    SetCardTheme();

                _ = LoadChart(ChartCompareList.SelectedItemPosition);

                ShowCardViewVisibility();
                ShowTitleSubLayout();
                HideFloatingActionButtonAnimation();

                //LoadAD();
            }
            catch (WebException ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.RetryLoad_CauseNetwork, Snackbar.LengthShort, Android.Graphics.Color.DarkMagenta);

                _ = InitLoadProcess(false);

                return;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
                int[] progresses = { Resource.Id.DollInfoHPProgress, Resource.Id.DollInfoFRProgress, Resource.Id.DollInfoEVProgress, Resource.Id.DollInfoACProgress, Resource.Id.DollInfoASProgress };
                int[] progressMaxTexts = { Resource.Id.DollInfoHPProgressMax, Resource.Id.DollInfoFRProgressMax, Resource.Id.DollInfoEVProgressMax, Resource.Id.DollInfoACProgressMax, Resource.Id.DollInfoASProgressMax };
                int[] statusTexts = { Resource.Id.DollInfoHPStatus, Resource.Id.DollInfoFRStatus, Resource.Id.DollInfoEVStatus, Resource.Id.DollInfoACStatus, Resource.Id.DollInfoASStatus };

                string[] growRatio = doll.Abilities["Grow"].Split(';');

                for (int i = 0; i < progresses.Length; ++i)
                {
                    FindViewById<TextView>(progressMaxTexts[i]).Text = FindViewById<ProgressBar>(progresses[i]).Max.ToString();

                    string[] basicRatio = doll.Abilities[abilities[i]].Split(';');
                    int value = (modIndex == 0) ? DAS.CalcAbility(abilities[i], int.Parse(basicRatio[0]), int.Parse(growRatio[0]), abilityLevel, abilityFavor, false) :
                        DAS.CalcAbility(abilities[i], int.Parse(basicRatio[1]), int.Parse(growRatio[1]), abilityLevel, abilityFavor, false);

                    ProgressBar pb = FindViewById<ProgressBar>(progresses[i]);
                    pb.Progress = value;

                    AbilityValues[i] = value;

                    FindViewById<TextView>(statusTexts[i]).Text = $"{value} ({doll.AbilityGrade[i]})";
                }

                if ((doll.Type == "MG") || (doll.Type == "SG"))
                {
                    FindViewById<LinearLayout>(Resource.Id.DollInfoBulletLayout).Visibility = ViewStates.Visible;
                    FindViewById<LinearLayout>(Resource.Id.DollInfoReloadLayout).Visibility = ViewStates.Visible;

                    double reloadTime = CalcReloadTime(doll, doll.Type, AbilityValues[4]);
                    int bullet = doll.HasMod ? int.Parse(doll.Abilities["Bullet"].Split(';')[modIndex]) : int.Parse(doll.Abilities["Bullet"]);

                    FindViewById<TextView>(Resource.Id.DollInfoBulletProgressMax).Text = FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress).Max.ToString();

                    FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress).Progress = bullet;
                    FindViewById<TextView>(Resource.Id.DollInfoBulletStatus).Text = bullet.ToString();
                    FindViewById<TextView>(Resource.Id.DollInfoReloadStatus).Text = $"{reloadTime} {Resources.GetString(Resource.String.Time_Second)}";
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

                    string[] basicRatio = doll.Abilities["Armor"].Split(';');
                    int value = (modIndex == 0) ? DAS.CalcAbility("Armor", int.Parse(basicRatio[0]), int.Parse(growRatio[0]), abilityLevel, abilityFavor, false) :
                        DAS.CalcAbility("Armor", int.Parse(basicRatio[1]), int.Parse(growRatio[1]), abilityLevel, abilityFavor, true);

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
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, "Error Load Ability", Snackbar.LengthShort);
            }
        }

        private void ShowTitleSubLayout()
        {
            if (doll.HasVoice)
                FindViewById<LinearLayout>(Resource.Id.DollDBDetailVoiceLayout).Visibility = ViewStates.Visible;
            if (doll.HasMod)
                FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSelectLayout).Visibility = ViewStates.Visible;

            FindViewById<LinearLayout>(Resource.Id.DollDBDetailExtraButtonLayout).Visibility = ViewStates.Visible;
        }

        private void HideFloatingActionButtonAnimation()
        {
            FABTimer.Stop();
            isEnableFABMenu = false;

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

            switch (modIndex)
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

            //FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityChartCompareList).Visibility = ViewStates.Gone;
            //FindViewById<FrameLayout>(Resource.Id.DollDBDetailAbilityRadarChartLayout).Visibility = ViewStates.Gone;
            //FindViewById<TextView>(Resource.Id.DollDBDetailChartMessage).Visibility = ViewStates.Visible;
        }

        private void DollDBDetailModSelectButton_Click(object sender, EventArgs e)
        {
            try
            {
                ImageView ModButton = sender as ImageView;

                switch (ModButton.Id)
                {
                    case Resource.Id.DollDBDetailModSelect0:
                        modIndex = 0;
                        break;
                    case Resource.Id.DollDBDetailModSelect1:
                        modIndex = 1;
                        break;
                    case Resource.Id.DollDBDetailModSelect2:
                        modIndex = 2;
                        break;
                    case Resource.Id.DollDBDetailModSelect3:
                        modIndex = 3;
                        break;
                    default:
                        modIndex = 0;
                        break;
                }

                foreach (int id in ModButtonIds)
                    FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);

                ModButton.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                ListAbilityLevelFavor();

                _ = InitLoadProcess(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
                    result = (tAS == 0) ? 0 : (4 + 200 / tAS);
                    break;
                case "SG":
                    int tB = int.Parse(doll.Abilities["Bullet"]);
                    result = 1.5 + 0.5 * tB;
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
                    cDoll = new Doll(ETC.FindDataRow(ETC.DollList, "Name", CompareList[CompareIndex]));

                    string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
                    int[] compareAbilityValues = { 0, 0, 0, 0, 0, 0 };
                    int growRatio = int.Parse(cDoll.Abilities["Grow"].Split(';')[0]);

                    for (int i = 0; i < abilities.Length; ++i)
                    {
                        int baseRatio = int.Parse(cDoll.Abilities[abilities[i]].Split(';')[0]);

                        compareAbilityValues[i] = DAS.CalcAbility(abilities[i], baseRatio, growRatio, abilityLevel, abilityFavor, false);
                    }

                    if (doll.Type == "SG")
                    {
                        int baseRatio = int.Parse(cDoll.Abilities["Armor"].Split(';')[0]);

                        compareAbilityValues[5] = DAS.CalcAbility("Armor", baseRatio, growRatio, abilityLevel, abilityFavor, false);
                    }
                    else compareAbilityValues[5] = 0;

                    CompareAbilityList = new ObservableCollection<DollMaxAbility>()
                    {
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), compareAbilityValues[0]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), compareAbilityValues[1]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), compareAbilityValues[2]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), compareAbilityValues[3]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), compareAbilityValues[4]),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), compareAbilityValues[5])
                    };
                }
            }
        }
    }
}