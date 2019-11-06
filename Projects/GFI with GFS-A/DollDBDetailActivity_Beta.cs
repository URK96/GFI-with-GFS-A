using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
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
using Android.Support.Transitions;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBDetailActivity_Beta : BaseFragmentActivity
    {
        private LinearLayout skillTableSubLayout;
        private LinearLayout modSkillTableSubLayout;

        internal static Doll doll;
        internal static Doll cDoll;
        private DataRow dollInfoDR = null;
        internal static int modIndex = 0;
        private int vCostumeIndex = 0;
        private List<string> voiceList = new List<string>();

        internal static int abilityLevel = 1;
        private List<int> levelList = new List<int>();
        internal static int abilityFavor = 0;
        private List<string> favorList = new List<string>();
        internal static DollAbilitySet das;
        internal static int[] abilityValues = new int[6];

        private bool isExtraFeatureOpen = false;
        private bool isOpenFABMenu = false;
        private bool isEnableFABMenu = false;
        private bool isChartLoad = false;

        private SwipeRefreshLayout refreshMainLayout;
        private ScrollView scrollLayout;
        private CoordinatorLayout snackbarLayout;

        private Spinner voiceCostumeSelector;
        private Spinner voiceSelector;
        private Button voicePlayButton;
        private Button modelDataButton;
        private Spinner abilityLevelSelector;
        private Spinner abilityFavorSelector;
        private Spinner chartCompareList;
        private SfChart chart;

        private int curtainUpIcon = ETC.useLightTheme ? Resource.Drawable.ArrowUp_WhiteTheme : Resource.Drawable.ArrowUp;
        private int curtainDownIcon = ETC.useLightTheme ? Resource.Drawable.ArrowDown_WhiteTheme : Resource.Drawable.ArrowDown;

        int[] modButtonIds = { Resource.Id.bDollDBDetailModSelect0, Resource.Id.bDollDBDetailModSelect1, Resource.Id.bDollDBDetailModSelect2, Resource.Id.bDollDBDetailModSelect3 };
        internal static List<string> compareList;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.DollDBDetailLayout_Beta);

                dollInfoDR = ETC.FindDataRow(ETC.dollList, "DicNumber", Intent.GetIntExtra("DicNum", 0));
                doll = new Doll(dollInfoDR);
                das = new DollAbilitySet(doll.Type);

                refreshMainLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.bDollDBDetailMainRefreshLayout);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBDetailSnackbarLayout);

                FindViewById<ImageView>(Resource.Id.bDollDBDetailSmallImage).Click += DollDBDetailSmallImage_Click;

                if (doll.HasVoice)
                {
                    voiceCostumeSelector = FindViewById<Spinner>(Resource.Id.bDollDBDetailVoiceCostumeSelector);
                    voiceCostumeSelector.ItemSelected += VoiceCostumeSelector_ItemSelected;
                    voiceSelector = FindViewById<Spinner>(Resource.Id.bDollDBDetailVoiceSelector);
                    voicePlayButton = FindViewById<Button>(Resource.Id.bDollDBDetailVoicePlayButton);
                    voicePlayButton.Click += VoicePlayButton_Click;
                }

                if (doll.HasMod)
                {
                    foreach (int id in modButtonIds)
                    {
                        FindViewById<ImageButton>(id).Click += DollDBDetailModSelectButton_Click;
                        //FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);
                    }

                    FindViewById<ImageButton>(modButtonIds[0]).SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                    Button ModStoryButton = FindViewById<Button>(Resource.Id.DollDBDetailModStoryButton);
                    ModStoryButton.Visibility = ViewStates.Visible;
                    //ModStoryButton.Click += ModStoryButton_Click;
                }

                FindViewById<ImageButton>(Resource.Id.bDollDBExtraFeatureButton).Click += ExtraMenuButton_Click;
                modelDataButton = FindViewById<Button>(Resource.Id.bDollDBDetailModelDataButton);
                modelDataButton.Click += ModelDataButton_Click;

                /*skillTableSubLayout = FindViewById<LinearLayout>(Resource.Id.DollDBDetailSkillAbilitySubLayout);
                modSkillTableSubLayout = FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSkillAbilitySubLayout);

                scrollLayout = FindViewById<ScrollView>(Resource.Id.DollDBDetailScrollLayout);
                
                abilityLevelSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityLevelSelector);
                abilityLevelSelector.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => 
                {
                    abilityLevel = levelList[e.Position];
                    _ = LoadAbility();
                };
                abilityFavorSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityFavorSelector);
                abilityFavorSelector.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => 
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
                chartCompareList = FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityChartCompareList);
                chartCompareList.ItemSelected += ChartCompareList_ItemSelected;
                chart = FindViewById<SfChart>(Resource.Id.DollDBDetailAbilityRadarChart);*/

                //InitCompareList();
                //ListAbilityLevelFavor();

                await InitializeProcess();
                _ = InitLoadProcess(false);

                /*if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DollDBDetail", true)))
                    ETC.RunHelpActivity(this, "DollDBDetail");*/
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private async Task InitializeProcess()
        {
            await Task.Delay(100);

            try
            {
                if (doll.HasVoice)
                {
                    InitializeVoiceList();
                }
            }
            catch (Exception ex)
            {

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
            var b = sender as ImageButton;

            TransitionManager.BeginDelayedTransition(FindViewById<RelativeLayout>(Resource.Id.bDollDBDetailBasicLayout));

            switch (isExtraFeatureOpen)
            {
                case false:
                    isExtraFeatureOpen = true;
                    b.SetImageResource(curtainUpIcon);
                    FindViewById<LinearLayout>(Resource.Id.bDollDBExtraFeatureLayout).Visibility = ViewStates.Visible;
                    break;
                case true:
                    isExtraFeatureOpen = false;
                    b.SetImageResource(curtainDownIcon);
                    FindViewById<LinearLayout>(Resource.Id.bDollDBExtraFeatureLayout).Visibility = ViewStates.Gone;
                    break;
            }
        }

        private void ListAbilityLevelFavor()
        {
            try
            {
                levelList.Clear();
                favorList.Clear();

                favorList.Add("0 ~ 9");
                favorList.Add("10 ~ 89");
                favorList.Add("90 ~ 139");

                switch (modIndex)
                {
                    case 0:
                        for (int i = 1; i <= 100; ++i)
                            levelList.Add(i);

                        favorList.Add("140 ~ 150");
                        break;
                    case 1:
                        for (int i = 100; i <= 110; ++i)
                            levelList.Add(i);

                        favorList.Add("140 ~ 150");
                        break;
                    case 2:
                        for (int i = 110; i <= 115; ++i)
                            levelList.Add(i);

                        favorList.Add("140 ~ 150");
                        break;
                    case 3:
                        for (int i = 115; i <= 120; ++i)
                            levelList.Add(i);

                        favorList.Add("140 ~ 189");
                        favorList.Add("190 ~ 200");
                        break;
                }

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, levelList.ToArray());
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                var adapter2 = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, favorList.ToArray());
                adapter2.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                abilityLevelSelector.Adapter = adapter;
                abilityFavorSelector.Adapter = adapter2;

                abilityFavorSelector.SetSelection(1);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Error List Level", Snackbar.LengthShort);
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
                compareList = new List<string>();
                compareList.Add("Type Average");

                foreach (DataRow dr in ETC.dollList.Rows)
                {
                    cDoll = new Doll(dr);

                    if (doll.DicNumber == cDoll.DicNumber)
                        continue;
                    if (cDoll.Type != doll.Type)
                        continue;

                    compareList.Add(cDoll.Name);
                }

                compareList.TrimExcess();

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, compareList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                chartCompareList.Adapter = adapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Test Error", Snackbar.LengthShort);
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
                Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBG);
                ad.SetTitle(Resource.String.DollDBDetailLayout_ModelData);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                Task.Run(async () =>
                {
                    using (WebClient wc = new WebClient())
                        data = await wc.DownloadStringTaskAsync(Path.Combine(ETC.server, "Data", "Text", "Gun", "ModelData", FileName));
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
            chart.Legend.Visibility = Com.Syncfusion.Charts.Visibility.Visible;

            chart.Legend.LabelStyle.TextColor = ETC.useLightTheme ? Android.Graphics.Color.DarkGray : Android.Graphics.Color.LightGray;

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
                    _ = LoadChart(chartCompareList.SelectedItemPosition);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
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
                ETC.ShowSnackbar(snackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private async void VoicePlayButton_Click(object sender, EventArgs e)
        {
            try
            {
                FindViewById<ProgressBar>(Resource.Id.DollDBDetailVoiceDownloadProgress).Visibility = ViewStates.Visible;
                FindViewById<ProgressBar>(Resource.Id.DollDBDetailVoiceDownloadProgress).Indeterminate = true;

                string voice = voiceList[voiceSelector.SelectedItemPosition];
                string VoiceServerURL = "";
                string target = "";

                switch (vCostumeIndex)
                {
                    case 0:
                        VoiceServerURL = Path.Combine(ETC.server, "Data", "Voice", "Doll", doll.krName, $"{doll.krName}_{voice}_JP.wav");
                        target = Path.Combine(ETC.cachePath, "Voices", "Doll", $"{doll.DicNumber}_{voice}_JP.gfdcache");
                        break;
                    default:
                        VoiceServerURL = Path.Combine(ETC.server, "Data", "Voice", "Doll", $"{doll.krName}_{vCostumeIndex - 1}", $"{doll.krName}_{vCostumeIndex - 1}_{voice}_JP.wav");
                        target = Path.Combine(ETC.cachePath, "Voices", "Doll", $"{doll.DicNumber}_{vCostumeIndex - 1}_{voice}_JP.gfdcache");
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
                ETC.ShowSnackbar(snackbarLayout, Resource.String.VoiceStreaming_Error, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.VoiceStreaming_PlayError, Snackbar.LengthShort, Android.Graphics.Color.DarkCyan);
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
                voiceCostumeSelector.Adapter = adater;

                voiceCostumeSelector.SetSelection(0);
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

                voiceList.Clear();

                switch(vCostumeIndex)
                {
                    case 0:
                        voiceList.AddRange(doll.Voices);
                        break;
                    default:
                        voiceList.AddRange(doll.CostumeVoices[vCostumeIndex - 1, 1].Split(';'));
                        break;
                }

                voiceList.TrimExcess();

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, voiceList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                voiceSelector.Adapter = adapter;
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
                ETC.ShowSnackbar(snackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
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
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ImageViewer_ActivityOpenError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task InitLoadProcess(bool IsRefresh)
        {
            

            await Task.Delay(100);

            try
            {
                refreshMainLayout.Refreshing = true;

                // 인형 타이틀 바 초기화

                if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false))
                {
                    try
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal", $"{doll.DicNumber}.png");
                        string target = Path.Combine(ETC.cachePath, "Doll", "Normal", $"{doll.DicNumber}.gfdcache");

                        if (!File.Exists(target) || IsRefresh)
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
                        ETC.LogError(ex, this);
                    }
                }

                string FileName = doll.DicNumber.ToString();

                if (modIndex == 3)
                    FileName += "_M";

                try
                {
                    string url = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal_Crop", $"{FileName}.png");
                    string target = Path.Combine(ETC.cachePath, "Doll", "Normal_Crop", $"{FileName}.gfdcache");

                    if (!File.Exists(target) || IsRefresh == true)
                    {
                        using (WebClient wc = new WebClient())
                            await wc.DownloadFileTaskAsync(url, target);
                    }

                    ImageView DollSmallImage = FindViewById<ImageView>(Resource.Id.bDollDBDetailSmallImage);
                    DollSmallImage.SetImageDrawable(Drawable.CreateFromPath(target));
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                FindViewById<TextView>(Resource.Id.bDollDBDetailDollName).Text = doll.Name;
                FindViewById<TextView>(Resource.Id.bDollDBDetailDollDicNumber).Text = $"No. {doll.DicNumber}";
                FindViewById<TextView>(Resource.Id.bDollDBDetailDollProductTime).Text = ETC.CalcTime(doll.ProductTime);
                FindViewById<TextView>(Resource.Id.bDollDBDetailDollProductDialog).Text = doll.ProductDialog;


                // 인형 기본 정보 초기화

                /*int[] GradeStarIds = 
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
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoHowToGain).Text = (string)dollInfoDR["DropEvent"];


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
                            id = ETC.useLightTheme ? Resource.Drawable.AC_Icon_WhiteTheme : Resource.Drawable.AC_Icon;
                            name = Resources.GetString(Resource.String.Common_AC);
                            break;
                        case "AM":
                            id = ETC.useLightTheme ? Resource.Drawable.AM_Icon_WhiteTheme : Resource.Drawable.AM_Icon;
                            name = Resources.GetString(Resource.String.Common_AM);
                            break;
                        case "AS":
                            id = ETC.useLightTheme ? Resource.Drawable.AS_Icon_WhiteTheme : Resource.Drawable.AS_Icon;
                            name = Resources.GetString(Resource.String.Common_AS);
                            break;
                        case "CR":
                            id = ETC.useLightTheme ? Resource.Drawable.CR_Icon_WhiteTheme : Resource.Drawable.CR_Icon;
                            name = Resources.GetString(Resource.String.Common_CR);
                            break;
                        case "EV":
                            id = ETC.useLightTheme ? Resource.Drawable.EV_Icon_WhiteTheme : Resource.Drawable.EV_Icon;
                            name = Resources.GetString(Resource.String.Common_EV);
                            break;
                        case "FR":
                            id = ETC.useLightTheme ? Resource.Drawable.FR_Icon_WhiteTheme : Resource.Drawable.FR_Icon;
                            name = Resources.GetString(Resource.String.Common_FR);
                            break;
                        case "CL":
                            id = ETC.useLightTheme ? Resource.Drawable.CL_Icon_WhiteTheme : Resource.Drawable.CL_Icon;
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
                    string url = Path.Combine(ETC.server, "Data", "Images", "SkillIcons", $"{SkillName}.png");
                    string target = Path.Combine(ETC.cachePath, "Doll", "Skill", $"{SkillName}.gfdcache");

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

                if (ETC.useLightTheme)
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

                skillTableSubLayout.RemoveAllViews();

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

                    skillTableSubLayout.AddView(layout);
                }


                // 인형 Mod 스킬 정보 초기화

                if (doll.HasMod)
                {
                    string mSkillName = doll.ModSkillName;

                    try
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "SkillIcons", $"{mSkillName}.png");
                        string target = Path.Combine(ETC.cachePath, "Doll", "Skill", $"{mSkillName}.gfdcache");

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

                    modSkillTableSubLayout.RemoveAllViews();

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

                        modSkillTableSubLayout.AddView(layout);
                    }
                }


                // 인형 능력치 초기화

                await LoadAbility();

                double[] DPS = ETC.CalcDPS(abilityValues[1], abilityValues[4], 0, abilityValues[3], 3, int.Parse(doll.Abilities["Critical"]), 5);
                FindViewById<TextView>(Resource.Id.DollInfoDPSStatus).Text = $"{DPS[0].ToString("F2")} ~ {DPS[1].ToString("F2")}";

                if (ETC.useLightTheme)
                    SetCardTheme();

                _ = LoadChart(chartCompareList.SelectedItemPosition);

                ShowCardViewVisibility();
                
                HideFloatingActionButtonAnimation();*/

                ShowTitleSubLayout();

                //LoadAD();
            }
            catch (WebException ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.RetryLoad_CauseNetwork, Snackbar.LengthShort, Android.Graphics.Color.DarkMagenta);

                _ = InitLoadProcess(false);

                return;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBDetail_LoadDetailFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                refreshMainLayout.Refreshing = false;
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
                    int value = (modIndex == 0) ? das.CalcAbility(abilities[i], int.Parse(basicRatio[0]), int.Parse(growRatio[0]), abilityLevel, abilityFavor, false) :
                        das.CalcAbility(abilities[i], int.Parse(basicRatio[1]), int.Parse(growRatio[1]), abilityLevel, abilityFavor, true);

                    ProgressBar pb = FindViewById<ProgressBar>(progresses[i]);
                    pb.Progress = value;

                    abilityValues[i] = value;

                    FindViewById<TextView>(statusTexts[i]).Text = $"{value} ({doll.AbilityGrade[i]})";
                }

                if ((doll.Type == "MG") || (doll.Type == "SG"))
                {
                    FindViewById<LinearLayout>(Resource.Id.DollInfoBulletLayout).Visibility = ViewStates.Visible;
                    FindViewById<LinearLayout>(Resource.Id.DollInfoReloadLayout).Visibility = ViewStates.Visible;

                    double reloadTime = CalcReloadTime(doll, doll.Type, abilityValues[4]);
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
                    int value = (modIndex == 0) ? das.CalcAbility("Armor", int.Parse(basicRatio[0]), int.Parse(growRatio[0]), abilityLevel, abilityFavor, false) :
                        das.CalcAbility("Armor", int.Parse(basicRatio[1]), int.Parse(growRatio[1]), abilityLevel, abilityFavor, true);

                    FindViewById<ProgressBar>(Resource.Id.DollInfoAMProgress).Progress = value;

                    abilityValues[5] = value;
                    FindViewById<TextView>(Resource.Id.DollInfoAMStatus).Text = $"{value} ({doll.AbilityGrade[6]})";
                }
                else
                {
                    abilityValues[5] = 0;
                    FindViewById<LinearLayout>(Resource.Id.DollInfoAMLayout).Visibility = ViewStates.Gone;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Error Load Ability", Snackbar.LengthShort);
            }
        }

        private void ShowTitleSubLayout()
        {
            if (doll.HasVoice)
                FindViewById<LinearLayout>(Resource.Id.bDollDBDetailVoiceLayout).Visibility = ViewStates.Visible;
            if (doll.HasMod)
                FindViewById<LinearLayout>(Resource.Id.bDollDBDetailModSelectLayout).Visibility = ViewStates.Visible;

            FindViewById<LinearLayout>(Resource.Id.bDollDBDetailExtraButtonLayout).Visibility = ViewStates.Visible;
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

                foreach (int id in modButtonIds)
                    FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);

                ModButton.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                ListAbilityLevelFavor();

                _ = InitLoadProcess(false);
                _ = LoadChart(chartCompareList.SelectedItemPosition);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DollDBDetail_MODChangeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
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
            //OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
            FinishAfterTransition();
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
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), abilityValues[0]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), abilityValues[1]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), abilityValues[2]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), abilityValues[3]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), abilityValues[4]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), abilityValues[5])
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
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), ETC.avgList[index].HP),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), ETC.avgList[index].FR),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), ETC.avgList[index].EV),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), ETC.avgList[index].AC),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), ETC.avgList[index].AS),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), ETC.avgList[index].AM)
                    };
                }
                else
                {
                    cDoll = new Doll(ETC.FindDataRow(ETC.dollList, "Name", compareList[CompareIndex]));

                    string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
                    int[] compareAbilityValues = { 0, 0, 0, 0, 0, 0 };
                    int growRatio = (modIndex == 0) ? int.Parse(cDoll.Abilities["Grow"].Split(';')[0]) : int.Parse(cDoll.Abilities["Grow"].Split(';')[1]);

                    for (int i = 0; i < abilities.Length; ++i)
                    {
                        int baseRatio = (modIndex == 0) ? int.Parse(cDoll.Abilities[abilities[i]].Split(';')[0]) : int.Parse(cDoll.Abilities[abilities[i]].Split(';')[1]);

                        compareAbilityValues[i] = das.CalcAbility(abilities[i], baseRatio, growRatio, 100, 50, false);
                    }

                    if (doll.Type == "SG")
                    {
                        int baseRatio = (modIndex == 0) ? int.Parse(cDoll.Abilities["Armor"].Split(';')[0]) : int.Parse(cDoll.Abilities["Armor"].Split(';')[1]);

                        compareAbilityValues[5] = das.CalcAbility("Armor", baseRatio, growRatio, 100, 50, false);
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