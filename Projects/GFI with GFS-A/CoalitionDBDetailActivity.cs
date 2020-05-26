using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.Transitions;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using Com.Syncfusion.Charts;

using Plugin.SimpleAudioPlayer;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CoalitionDBDetailActivity : BaseAppCompatActivity
    {
        internal struct AverageAbility
        {
            public int HP { get; set; }
            public int FR { get; set; }
            public int EV { get; set; }
            public int AC { get; set; }
            public int AS { get; set; }
            public int AM { get; set; }
        }

        private AverageAbility avgAbility;
        private List<int[]> typeAbilities;

        private LinearLayout skillTableSubLayout;
        private LinearLayout modSkillTableSubLayout;
        private LinearLayout scrollMainLayout;

        private Coalition coalition;
        private DataRow coalitionInfoDR = null;
        private int analysisIndex = 0;
        private int vCostumeIndex = 0;
        private List<string> voiceList = new List<string>();

        private int abilityLevel = 1;
        private List<int> levelList = new List<int>();
        private int abilityFavor = 0;
        private List<string> favorList = new List<string>();
        //private CoalitionAbilitySet das;
        private int[] abilityValues = new int[6];

        private bool isExtraFeatureOpen = false;
        private bool isChartLoad = false;
        private bool initLoadComplete = false;

        private ISimpleAudioPlayer voicePlayer;
        private FileStream stream;

        private AndroidX.AppCompat.Widget.Toolbar toolbar;
        private SwipeRefreshLayout refreshMainLayout;
        private NestedScrollView scrollLayout;
        private CoordinatorLayout snackbarLayout;

        private Spinner voiceCostumeSelector;
        private Spinner voiceSelector;
        private Button voicePlayButton;
        private Button modelDataButton;
        private Spinner abilityLevelSelector;
        private Spinner abilityFavorSelector;
        private Spinner chartCompareList;
        private SfChart chart;

        private List<CardView> scrollCardViews = new List<CardView>();

        private int curtainUpIcon = ETC.useLightTheme ? Resource.Drawable.ArrowUp_WhiteTheme : Resource.Drawable.ArrowUp;
        private int curtainDownIcon = ETC.useLightTheme ? Resource.Drawable.ArrowDown_WhiteTheme : Resource.Drawable.ArrowDown;

        int[] analysisButtonIds = { Resource.Id.CoalitionDBDetailAnalysisSelect0, Resource.Id.CoalitionDBDetailAnalysisSelect1, Resource.Id.CoalitionDBDetailAnalysisSelect2, Resource.Id.CoalitionDBDetailAnalysisSelect3, Resource.Id.CoalitionDBDetailAnalysisSelect4 };
        private List<string> compareList;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Toolbar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.CoalitionDBDetailLayout);

                coalitionInfoDR = ETC.FindDataRow(ETC.coalitionList, "DicNumber", Intent.GetIntExtra("DicNum", 0));
                coalition = new Coalition(coalitionInfoDR);
                //das = new CoalitionAbilitySet(coalition.Type);

                voicePlayer = CrossSimpleAudioPlayer.Current;

                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.CoalitionDBDetailMainToolbar);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                FindViewById<TextView>(Resource.Id.CoalitionDBDetailToolbarDicNumber).Text = $"No. {coalition.DicNumber}";
                FindViewById<TextView>(Resource.Id.CoalitionDBDetailToolbarName).Text = $"{coalition.Name} - {coalition.CodeName}";

                refreshMainLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.CoalitionDBDetailMainRefreshLayout);
                refreshMainLayout.Refresh += async delegate { await InitLoadProcess(true); };
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.CoalitionDBDetailSnackbarLayout);

                scrollMainLayout = FindViewById<LinearLayout>(Resource.Id.CoalitionDBDetailScrollMainLayout);

                FindViewById<ImageView>(Resource.Id.CoalitionDBDetailSmallImage).Click += CoalitionDBDetailSmallImage_Click;

                /*if (coalition.HasVoice)
                {
                    voiceCostumeSelector = FindViewById<Spinner>(Resource.Id.CoalitionDBDetailVoiceCostumeSelector);
                    voiceCostumeSelector.ItemSelected += VoiceCostumeSelector_ItemSelected;
                    voiceSelector = FindViewById<Spinner>(Resource.Id.CoalitionDBDetailVoiceSelector);
                    voicePlayButton = FindViewById<Button>(Resource.Id.CoalitionDBDetailVoicePlayButton);
                    voicePlayButton.Click += delegate { _ = PlayVoiceProcess(); };
                }*/

                foreach (int id in analysisButtonIds)
                {
                    FindViewById<ImageButton>(id).Click += CoalitionDBDetailAnalysisSelectButton_Click;
                    FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);
                }

                FindViewById<ImageButton>(analysisButtonIds[0]).SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                FindViewById<ImageButton>(Resource.Id.CoalitionDBExtraFeatureButton).Click += ExtraMenuButton_Click;
                FindViewById<ImageButton>(Resource.Id.CoalitionDBExtraFeatureButton).SetImageResource(curtainDownIcon);

                scrollLayout = FindViewById<NestedScrollView>(Resource.Id.CoalitionDBDetailScrollLayout);

                /*abilityLevelSelector = FindViewById<Spinner>(Resource.Id.CoalitionDBDetailAbilityLevelSelector);
                abilityLevelSelector.ItemSelected += async (object sender, AdapterView.ItemSelectedEventArgs e) => 
                {
                    abilityLevel = levelList[e.Position];

                    await LoadAbility();
                };
                abilityFavorSelector = FindViewById<Spinner>(Resource.Id.CoalitionDBDetailAbilityFavorSelector);
                abilityFavorSelector.ItemSelected += async (object sender, AdapterView.ItemSelectedEventArgs e) => 
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

                    if (initLoadComplete)
                    {
                        await LoadAbility();
                    }
                };
                chartCompareList = FindViewById<Spinner>(Resource.Id.CoalitionDBDetailAbilityChartCompareList);
                chartCompareList.ItemSelected += (sender, e) => { _ = LoadChart(e.Position); };
                chart = FindViewById<SfChart>(Resource.Id.CoalitionDBDetailAbilityRadarChart);

                InitCompareList();
                ListAbilityLevelFavor();*/

                await InitializeProcess();
                await InitLoadProcess(false);

                /*if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_CoalitionDBDetail", true)))
                    ETC.RunHelpActivity(this, "CoalitionDBDetail");*/
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            try
            {
                MenuInflater.Inflate(Resource.Menu.CoalitionDBDetailMenu, menu);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Cannot create option menu", ToastLength.Short).Show();
            }

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            try
            {
                switch (item?.ItemId)
                {
                    case Android.Resource.Id.Home:
                        OnBackPressed();
                        break;
                    case Resource.Id.CoalitionDBDetailLink:
                        var pMenu = new Android.Support.V7.Widget.PopupMenu(this, FindViewById<View>(Resource.Id.CoalitionDBDetailLink));
                        pMenu.Inflate(Resource.Menu.DBLinkMenu);
                        pMenu.MenuItemClick += PMenu_MenuItemClick;
                        pMenu.Show();
                        break;
                    /*case Resource.Id.CoalitionDBDetailGuideBookDetail:
                        var intent2 = new Intent(this, typeof(CoalitionDBGuideImageViewer));
                        intent2.PutExtra("Info", coalition.DicNumber);
                        StartActivity(intent2);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;*/
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Cannot execute option menu", ToastLength.Short).Show();
            }

            return base.OnOptionsItemSelected(item);
        }

        /*private void CalcAverageAbility()
        {
            string[] abilityList = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
            const int abilityCount = 6;

            var das = new CoalitionAbilitySet(coalition.Type);
            int[] total = new int[abilityCount];

            if (typeAbilities == null)
            {
                typeAbilities = new List<int[]>();
                Coalition tCoalition;

                foreach (DataRow row in ETC.CoalitionList.Rows)
                {
                    if ((string)row["Type"] != coalition.Type)
                    {
                        continue;
                    }

                    tCoalition = new Coalition(row);

                    int[] abilityValues = new int[abilityCount + 1]; // HP, FireRate, Evasion, Accuracy, AttackSpeed, Armor, Grow

                    abilityValues[6] = int.Parse(tCoalition.Abilities["Grow"].Split(';')[0]);

                    for (int i = 0; i < abilityList.Length; ++i)
                    {
                        abilityValues[i] = int.Parse(tCoalition.Abilities[abilityList[i]].Split(';')[0]);
                        total[i] += das.CalcAbility(abilityList[i], abilityValues[i], abilityValues[6], abilityLevel, abilityFavor, false);
                    }

                    if (coalition.Type == "SG")
                    {
                        abilityValues[5] = int.Parse(tCoalition.Abilities["Armor"].Split(';')[0]);
                        total[5] += das.CalcAbility("Armor", abilityValues[5], abilityValues[6], abilityLevel, abilityFavor, false);
                    }

                    typeAbilities.Add(abilityValues);
                }

                typeAbilities.TrimExcess();
            }
            else
            {
                foreach (var abilityValues in typeAbilities)
                {
                    for (int i = 0; i < abilityList.Length; ++i)
                    {
                        total[i] += das.CalcAbility(abilityList[i], abilityValues[i], abilityValues[6], abilityLevel, abilityFavor, false);
                    }

                    if (coalition.Type == "SG")
                    {
                        total[5] += das.CalcAbility("Armor", abilityValues[5], abilityValues[6], abilityLevel, abilityFavor, false);
                    }
                }
            }

            int value;

            for (int i = 0; i < abilityCount; ++i)
            {
                value = Convert.ToInt32(Math.Round((double)total[i] / typeAbilities.Count));

                switch (i)
                {
                    case 0:
                        avgAbility.HP = value;
                        break;
                    case 1:
                        avgAbility.FR = value;
                        break;
                    case 2:
                        avgAbility.EV = value;
                        break;
                    case 3:
                        avgAbility.AC = value;
                        break;
                    case 4:
                        avgAbility.AS = value;
                        break;
                    case 5:
                        avgAbility.AM = value;
                        break;
                }
            }
        }*/

        private void PMenu_MenuItemClick(object sender, Android.Support.V7.Widget.PopupMenu.MenuItemClickEventArgs e)
        {
            try
            {
                string url = "";

                switch (e.Item.ItemId)
                {
                    case Resource.Id.DBLinkNamu:
                        url = $"https://namu.wiki/w/{coalition.NameKR}(소녀전선)";
                        break;
                    case Resource.Id.DBLinkInven:
                        url = $"http://gf.inven.co.kr/dataninfo/Coalitions/detail.php?d=126&c={coalition.DicNumber}";
                        break;
                    case Resource.Id.DBLink36Base:
                        url = $"https://girlsfrontline.kr/Coalition/{coalition.DicNumber}";
                        break;
                    case Resource.Id.DBLinkGFDB:
                        url = $"https://gfl.zzzzz.kr/Coalition.php?id={coalition.DicNumber}&lang=ko";
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
                Toast.MakeText(this, "Cannot execute link menu", ToastLength.Short).Show();
            }
        }

        private async Task InitializeProcess()
        {
            await Task.Delay(100);

            try
            {
                /*if (coalition.HasVoice)
                {
                    InitializeVoiceList();
                }*/

                var toolbarColor = coalition.Grade switch
                {
                    1 => Android.Graphics.Color.SlateGray,
                    2 => Android.Graphics.Color.ParseColor("#55CCEE"),
                    3 => Android.Graphics.Color.ParseColor("#AACC22"),
                    4 => Android.Graphics.Color.ParseColor("#FFBB22"),
                    _ => Android.Graphics.Color.ParseColor("#C040B0"),
                };
                toolbar.SetBackgroundColor(toolbarColor);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window.SetStatusBarColor(toolbarColor);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail Initialize Process", ToastLength.Short).Show();
            }
        }

        private void ExtraMenuButton_Click(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            try
            {
                TransitionManager.BeginDelayedTransition(refreshMainLayout);

                switch (isExtraFeatureOpen)
                {
                    case false:
                        isExtraFeatureOpen = true;
                        b.SetImageResource(curtainUpIcon);
                        FindViewById<LinearLayout>(Resource.Id.CoalitionDBExtraFeatureLayout).Visibility = ViewStates.Visible;
                        scrollLayout.Visibility = ViewStates.Gone;
                        break;
                    case true:
                        isExtraFeatureOpen = false;
                        b.SetImageResource(curtainDownIcon);
                        FindViewById<LinearLayout>(Resource.Id.CoalitionDBExtraFeatureLayout).Visibility = ViewStates.Gone;
                        scrollLayout.Visibility = ViewStates.Visible;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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

                switch (analysisIndex)
                {
                    case 0:
                        for (int i = 1; i <= 100; ++i)
                        {
                            levelList.Add(i);
                        }

                        favorList.Add("140 ~ 150");
                        break;
                    case 1:
                        for (int i = 100; i <= 110; ++i)
                        {
                            levelList.Add(i);
                        }

                        favorList.Add("140 ~ 150");
                        break;
                    case 2:
                        for (int i = 110; i <= 115; ++i)
                        {
                            levelList.Add(i);
                        }

                        favorList.Add("140 ~ 150");
                        break;
                    case 3:
                        for (int i = 115; i <= 120; ++i)
                        {
                            levelList.Add(i);
                        }

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

        private void InitCompareList()
        {
            Coalition cCoalition;

            try
            {
                compareList = new List<string>()
                {
                    "Type Average"
                };

                foreach (DataRow dr in ETC.coalitionList.Rows)
                {
                    cCoalition = new Coalition(dr);

                    if ((coalition.DicNumber == cCoalition.DicNumber) || (cCoalition.Type != coalition.Type))
                    {
                        continue;
                    }

                    compareList.Add(cCoalition.Name);
                }

                compareList.TrimExcess();

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, compareList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                chartCompareList.Adapter = adapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Fail Initialize Compare List", Snackbar.LengthShort);
            }
        }

        /*private async Task LoadChart(int compareIndex)
        {
            await Task.Delay(100);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                chart.Series.Clear();

                if (compareIndex == 0)
                {
                    CalcAverageAbility();
                }

                var ZoomBehavior = new ChartZoomPanBehavior
                {
                    ZoomMode = ZoomMode.Xy,
                    SelectionZoomingEnabled = true,
                    MaximumZoomLevel = 2.0f,
                    ZoomingEnabled = true,
                    DoubleTapEnabled = true,
                    ScrollingEnabled = true
                };

                chart.Behaviors.Add(ZoomBehavior);

                chart.PrimaryAxis = new CategoryAxis();
                chart.SecondaryAxis = new NumericalAxis();
                chart.Legend.Visibility = Com.Syncfusion.Charts.Visibility.Visible;

                chart.Legend.LabelStyle.TextColor = ETC.useLightTheme ? Android.Graphics.Color.DarkGray : Android.Graphics.Color.LightGray;

                
                var model = new DataModel(compareIndex, coalition, abilityValues, compareList, modIndex, abilityLevel, abilityFavor, ref avgAbility);

                var radar = new RadarSeries
                {
                    ItemsSource = model.MaxAbilityList,
                    XBindingPath = "AbilityType",
                    YBindingPath = "AbilityValue",
                    DrawType = PolarChartDrawType.Line,
                    Color = Android.Graphics.Color.LightGreen,
                    EnableAnimation = true,
                    Label = coalition.Name,
                    TooltipEnabled = true
                };

                chart.Series.Add(radar);

                RadarSeries radar2 = new RadarSeries
                {
                    ItemsSource = model.CompareAbilityList,
                    XBindingPath = "AbilityType",
                    YBindingPath = "AbilityValue",
                    DrawType = PolarChartDrawType.Line,
                    Color = Android.Graphics.Color.Magenta,
                    EnableAnimation = true,
                    Label = (compareIndex == 0) ? $"{coalition.Type}{Resources.GetString(Resource.String.CoalitionDBDetail_RadarAverageString)}" : compareList[compareIndex],
                    TooltipEnabled = true
                };

                chart.Series.Add(radar2);

                isChartLoad = true;
            });
        }*/

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                /*if (isChartLoad)
                {
                    _ = LoadChart(chartCompareList.SelectedItemPosition);
                }*/
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        /*private async Task PlayVoiceProcess()
        {
            await Task.Delay(100);

            string voice = voiceList[voiceSelector.SelectedItemPosition];
            string voiceServerURL = "";
            string target = "";

            try
            {
                stream?.Dispose();

                switch (vCostumeIndex)
                {
                    case 0:
                        voiceServerURL = Path.Combine(ETC.server, "Data", "Voice", "Coalition", coalition.NameKR, $"{coalition.NameKR}_{voice}_JP.wav");
                        target = Path.Combine(ETC.cachePath, "Voices", "Coalition", $"{coalition.DicNumber}_{voice}_JP.gfdcache");
                        break;
                    default:
                        voiceServerURL = Path.Combine(ETC.server, "Data", "Voice", "Coalition", $"{coalition.NameKR}_{vCostumeIndex - 1}", $"{coalition.NameKR}_{vCostumeIndex - 1}_{voice}_JP.wav");
                        target = Path.Combine(ETC.cachePath, "Voices", "Coalition", $"{coalition.DicNumber}_{vCostumeIndex - 1}_{voice}_JP.gfdcache");
                        break;
                }

                if (!File.Exists(target))
                {
                    using (WebClient wc = new WebClient())
                    {
                        MainThread.BeginInvokeOnMainThread(() => { voicePlayButton.Enabled = false; });

                        wc.DownloadProgressChanged += (sender, e) =>
                        {
                            MainThread.BeginInvokeOnMainThread(() => { voicePlayButton.Text = $"{e.ProgressPercentage}%"; });
                        };

                        await wc.DownloadFileTaskAsync(voiceServerURL, target);
                    }

                    await Task.Delay(500);
                }

                stream = new FileStream(target, FileMode.Open, FileAccess.Read);

                voicePlayer.Load(stream);
                voicePlayer.Play();
            }
            catch (WebException ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.VoiceStreaming_DownloadError, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);

                if (File.Exists(target))
                {
                    File.Delete(target);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.VoiceStreaming_PlayError, Snackbar.LengthShort, Android.Graphics.Color.DarkCyan);

                if (File.Exists(target))
                {
                    File.Delete(target);
                }
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    voicePlayButton.Enabled = true;
                    voicePlayButton.SetText(Resource.String.Common_Play);
                });
            }
        }*/

        /*private void InitializeVoiceList()
        {
            try
            {
                List<string> vcList = new List<string>()
                {
                    "Default"
                };

                if (coalition.CostumeVoices != null)
                {
                    for (int i = 0; i < (coalition.CostumeVoices.Length / coalition.CostumeVoices.Rank); ++i)
                    {
                        vcList.Add(coalition.Costumes[int.Parse(coalition.CostumeVoices[i, 0])]);
                    }
                }

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
        }*/

        /*private void VoiceCostumeSelector_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                vCostumeIndex = e.Position;

                voiceList.Clear();

                switch (vCostumeIndex)
                {
                    case 0:
                        voiceList.AddRange(coalition.Voices);
                        break;
                    default:
                        voiceList.AddRange(coalition.CostumeVoices[vCostumeIndex - 1, 1].Split(';'));
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
        }*/

        private void CoalitionDBDetailSmallImage_Click(object sender, EventArgs e)
        {
            try
            {
                /*var coalitionImageViewer = new Intent(this, typeof(CoalitionDBImageViewer));
                coalitionImageViewer.PutExtra("Data", $"{coalition.DicNumber};{modIndex}");
                StartActivity(coalitionImageViewer);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);*/
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ImageViewer_ActivityOpenError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task InitLoadProcess(bool isRefresh)
        {
            await Task.Delay(100);

            try
            {
                refreshMainLayout.Refreshing = true;

                if (!isExtraFeatureOpen)
                {
                    TransitionManager.BeginDelayedTransition(refreshMainLayout);
                }

                scrollCardViews.Clear();
                scrollMainLayout.RemoveAllViews();


                // 인형 타이틀 바 초기화

                if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false))
                {
                    try
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "Coalition", "BG", $"{coalition.Affiliation}.png");
                        string target = Path.Combine(ETC.cachePath, "Coalition", "BG", $"{coalition.Affiliation}.gfdcache");

                        if (!File.Exists(target) || isRefresh)
                        {
                            using (WebClient wc = new WebClient())
                            {
                                await wc.DownloadFileTaskAsync(url, target);
                            }
                        }

                        Drawable drawable = Drawable.CreateFromPath(target);
                        drawable.SetAlpha(40);

                        FindViewById<ImageView>(Resource.Id.CoalitionDBDetailBackgroundImageView).SetImageDrawable(drawable);
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex, this);
                    }
                }

                try
                {
                    string url = Path.Combine(ETC.server, "Data", "Images", "Coalition", "Normal_Crop", $"{coalition.DicNumber}.png");
                    string target = Path.Combine(ETC.cachePath, "Coalition", "Normal_Crop", $"{coalition.DicNumber}.gfdcache");

                    if (!File.Exists(target) || isRefresh)
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(url, target);
                        }
                    }

                    FindViewById<ImageView>(Resource.Id.CoalitionDBDetailSmallImage).SetImageDrawable(Drawable.CreateFromPath(target));
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                FindViewById<TextView>(Resource.Id.CoalitionDBDetailCoalitionName).Text = coalition.Name;
                FindViewById<TextView>(Resource.Id.CoalitionDBDetailCoalitionDicNumber).Text = $"No. {coalition.DicNumber}";
                FindViewById<TextView>(Resource.Id.CoalitionDBDetailCoalitionType).Text = coalition.GetTypeString;
                //FindViewById<TextView>(Resource.Id.CoalitionDBDetailCoalitionProductDialog).Text = coalition.ProductDialog;


                // 인형 기본 정보 초기화

                var basicLayout = LayoutInflater.Inflate(Resource.Layout.CoalitionDBDetailLayout_CardView_Basic, new LinearLayout(this), true);
                var gradeText = new StringBuilder();

                for (int i = 0; i < coalition.Grade; ++i)
                {
                    gradeText.Append("★");
                }

                basicLayout.FindViewById<TextView>(Resource.Id.CoalitionDBDetailInfoBornGrade).Text = gradeText.ToString();

                basicLayout.FindViewById<TextView>(Resource.Id.CoalitionDBDetailInfoType).Text = coalition.GetTypeString;
                basicLayout.FindViewById<TextView>(Resource.Id.CoalitionDBDetailInfoName).Text = coalition.Name;
                basicLayout.FindViewById<TextView>(Resource.Id.CoalitionDBDetailInfoNickName).Text = coalition.NickName;
                basicLayout.FindViewById<TextView>(Resource.Id.CoalitionDBDetailInfoIllustrator).Text = coalition.Illustrator;
                basicLayout.FindViewById<TextView>(Resource.Id.CoalitionDBDetailInfoVoiceActor).Text = coalition.VoiceActor;
                basicLayout.FindViewById<TextView>(Resource.Id.CoalitionDBDetailInfoAffiliation).Text = coalition.Affiliation;
                //FindViewById<TextView>(Resource.Id.CoalitionDBDetailInfoHowToGain).Text = (string)coalitionInfoDR["DropEvent"];

                scrollCardViews.Add(basicLayout.FindViewById<CardView>(Resource.Id.CoalitionDBDetailBasicInfoCardLayout));
                scrollMainLayout.AddView(basicLayout);


                // 인형 버프 정보 초기화

                if (coalition.BuffInfo != null)
                {
                    await LoadBuff();
                }


                // 인형 스킬 정보 초기화

                await LoadSkill(isRefresh);

                /*string SkillName = coalition.SkillName;

                try
                {
                    string url = Path.Combine(ETC.server, "Data", "Images", "SkillIcons", $"{SkillName}.png");
                    string target = Path.Combine(ETC.cachePath, "Coalition", "Skill", $"{SkillName}.gfdcache");

                    if (!File.Exists(target) || isRefresh)
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFile(url, target);
                        }
                    }

                    FindViewById<ImageView>(Resource.Id.CoalitionDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(target));
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillName).Text = SkillName;

                if (ETC.useLightTheme)
                {
                    FindViewById<ImageView>(Resource.Id.CoalitionDBDetailSkillInitCoolTimeIcon).SetImageResource(Resource.Drawable.FirstCoolTime_Icon_WhiteTheme);
                    FindViewById<ImageView>(Resource.Id.CoalitionDBDetailSkillCoolTimeIcon).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                }

                string[] SkillAbilities = coalition.SkillEffect;
                string[] SkillMags = (modIndex == 0) ? coalition.SkillMag : coalition.SkillMagAfterMod;

                TextView SkillInitCoolTime = FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillInitCoolTime);
                SkillInitCoolTime.SetTextColor(Android.Graphics.Color.Orange);
                SkillInitCoolTime.Text = SkillMags[0];

                TextView SkillCoolTime = FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillCoolTime);
                SkillCoolTime.SetTextColor(Android.Graphics.Color.DarkOrange);
                SkillCoolTime.Text = SkillMags[1];

                FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillExplain).Text = coalition.SkillExplain;

                skillTableSubLayout.RemoveAllViews();

                for (int i = 2; i < SkillAbilities.Length; ++i)
                {
                    LinearLayout layout = new LinearLayout(this)
                    {
                        Orientation = Android.Widget.Orientation.Horizontal,
                        LayoutParameters = FindViewById<LinearLayout>(Resource.Id.CoalitionDBDetailSkillAbilityTopLayout).LayoutParameters
                    };

                    TextView ability = new TextView(this)
                    {
                        LayoutParameters = FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillAbilityTopText1).LayoutParameters,
                        Text = SkillAbilities[i],
                        Gravity = GravityFlags.Center
                    };
                    TextView mag = new TextView(this)
                    {
                        LayoutParameters = FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillAbilityTopText2).LayoutParameters,
                        Text = SkillMags[i],
                        Gravity = GravityFlags.Center
                    };

                    layout.AddView(ability);
                    layout.AddView(mag);

                    skillTableSubLayout.AddView(layout);
                }*/


                // 인형 능력치 초기화

                /*await LoadAbility();

                double[] dps = ETC.CalcDPS(abilityValues[1], abilityValues[4], 0, abilityValues[3], 3, int.Parse(coalition.Abilities["Critical"]), 5);
                FindViewById<TextView>(Resource.Id.CoalitionInfoDPSStatus).Text = $"{dps[0].ToString("F2")} ~ {dps[1].ToString("F2")}";*/

                if (ETC.useLightTheme)
                {
                    SetCardTheme();
                }

                ShowCardViewVisibility();
                ShowTitleSubLayout();
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
                initLoadComplete = true;
            }
        }

        private async Task LoadBuff()
        {
            const int buffCardCount = 3;

            await Task.Delay(10);

            try
            {
                int[] buffIds =
                {
                    Resource.Id.CoalitionDBDetailBuff1, Resource.Id.CoalitionDBDetailBuff2, Resource.Id.CoalitionDBDetailBuff3,
                    Resource.Id.CoalitionDBDetailBuff4, Resource.Id.CoalitionDBDetailBuff5, Resource.Id.CoalitionDBDetailBuff6,
                    Resource.Id.CoalitionDBDetailBuff7, Resource.Id.CoalitionDBDetailBuff8, Resource.Id.CoalitionDBDetailBuff9
                };
                int[] buffData = coalition.BuffFormation;

                int buffIconId = Resource.Id.CoalitionDBDetailBuffIcon;
                int buffIconNameId = Resource.Id.CoalitionDBDetailBuffName;
                int buffDetailId = Resource.Id.CoalitionDBDetailBuffDetail;

                for (int i = 0; i < buffCardCount; ++i)
                {
                    if (coalition.BuffInfo[i] == null)
                    {
                        break;
                    }

                    var rootView = LayoutInflater.Inflate(Resource.Layout.CoalitionDBDetailLayout_CardView_Buff, new LinearLayout(this), true);

                    for (int k = 0; k < buffData.Length; ++k)
                    {
                        Android.Graphics.Color color;

                        switch (buffData[k])
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

                        rootView.FindViewById<View>(buffIds[k]).SetBackgroundColor(color);
                    }

                    string[] buffInfo = coalition.BuffInfo[i];
                    string[] buffMag = coalition.BuffMag[i][analysisIndex].Split(",");

                    for (int k = 0; k < buffInfo.Length; ++k)
                    {
                        int id = 0;
                        string name = "";
                        var subView = LayoutInflater.Inflate(Resource.Layout.CoalitionDBDetailLayout_CardView_Buff_IconDetail, new LinearLayout(this), false);

                        switch (buffInfo[k])
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

                        subView.FindViewById<ImageView>(buffIconId).SetImageResource(id);
                        subView.FindViewById<TextView>(buffIconNameId).Text = name;
                        subView.FindViewById<TextView>(buffDetailId).Text = $"{buffMag[k]}%";

                        rootView.FindViewById<LinearLayout>(Resource.Id.CoalitionDBDetailBuffDetailLayout).AddView(subView);
                    }

                    var buffTypeView = rootView.FindViewById<TextView>(Resource.Id.CoalitionDBDetailEffectType);

                    if (coalition.BuffType[i] == "All")
                    {
                        buffTypeView.Text = Resources.GetString(Resource.String.CoalitionDBDetail_BuffType_All);
                    }
                    else
                    {
                        buffTypeView.Text = $"{coalition.BuffType[i]} {Resources.GetString(Resource.String.CoalitionDBDetail_BuffType_ConfirmString)}";
                    }

                    scrollCardViews.Add(rootView.FindViewById<CardView>(Resource.Id.CoalitionDBDetailBuffCardLayout));
                    scrollMainLayout.AddView(rootView);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Error Load Formation Buff", Snackbar.LengthShort);
            }
        }

        private async Task LoadSkill(bool isRefresh)
        {
            await Task.Delay(10);

            try
            {
                for (int i = 0; i < coalition.SkillName.Length; ++i)
                {
                    if (coalition.SkillName[i] == null)
                    {
                        break;
                    }

                    var rootView = LayoutInflater.Inflate(Resource.Layout.CoalitionDBDetailLayout_CardView_Skill, new LinearLayout(this), true);

                    string sName = coalition.SkillName[i];

                    try
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "SkillIcons", "Coalition", $"{sName}.png");
                        string target = Path.Combine(ETC.cachePath, "Coalition", "Skill", $"{sName}.gfdcache");

                        if (!File.Exists(target) || isRefresh)
                        {
                            using (var wc = new WebClient())
                            {
                                wc.DownloadFile(url, target);
                            }
                        }

                        rootView.FindViewById<ImageView>(Resource.Id.CoalitionDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(target));
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex, this);
                    }

                    rootView.FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillName).Text = sName;

                    if (ETC.useLightTheme)
                    {
                        rootView.FindViewById<ImageView>(Resource.Id.CoalitionDBDetailSkillInitCoolTimeIcon).SetImageResource(Resource.Drawable.FirstCoolTime_Icon_WhiteTheme);
                        rootView.FindViewById<ImageView>(Resource.Id.CoalitionDBDetailSkillCoolTimeIcon).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                    }

                    string[] sAbilities = coalition.SkillEffect[i];
                    string[] sMags = ((i == 0) && (coalition.IsBoss) && (analysisIndex == 4)) ? coalition.FASkillMag[i] : coalition.SkillMag[i];

                    var sInitCoolTimeView = rootView.FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillInitCoolTime);
                    sInitCoolTimeView.SetTextColor(Android.Graphics.Color.Orange);
                    sInitCoolTimeView.Text = sMags[0];

                    var sCoolTimeView = rootView.FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillCoolTime);
                    sCoolTimeView.SetTextColor(Android.Graphics.Color.DarkOrange);
                    sCoolTimeView.Text = sMags[1];

                    rootView.FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillExplain).Text = coalition.SkillExplain[i];

                    var skillTableSubLayout = rootView.FindViewById<LinearLayout>(Resource.Id.CoalitionDBDetailSkillAbilitySubLayout);
                    //skillTableSubLayout.RemoveAllViews();

                    for (int k = 2; k < sAbilities.Length; ++k)
                    {
                        var layout = new LinearLayout(this)
                        {
                            Orientation = Android.Widget.Orientation.Horizontal,
                            LayoutParameters = rootView.FindViewById<LinearLayout>(Resource.Id.CoalitionDBDetailSkillAbilityTopLayout).LayoutParameters
                        };

                        var ability = new TextView(this)
                        {
                            LayoutParameters = rootView.FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillAbilityTopText1).LayoutParameters,
                            Text = sAbilities[k],
                            Gravity = GravityFlags.Center
                        };
                        var mag = new TextView(this)
                        {
                            LayoutParameters = rootView.FindViewById<TextView>(Resource.Id.CoalitionDBDetailSkillAbilityTopText2).LayoutParameters,
                            Text = sMags[k],
                            Gravity = GravityFlags.Center
                        };

                        layout.AddView(ability);
                        layout.AddView(mag);

                        skillTableSubLayout.AddView(layout);
                    }

                    scrollCardViews.Add(rootView.FindViewById<CardView>(Resource.Id.CoalitionDBDetailSkillCardLayout));
                    scrollMainLayout.AddView(rootView);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Error Load Skill", Snackbar.LengthShort);
            }
        }

        /*private async Task LoadAbility()
        {
            await Task.Delay(10);

            try
            {
                string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
                int[] progresses = { Resource.Id.CoalitionInfoHPProgress, Resource.Id.CoalitionInfoFRProgress, Resource.Id.CoalitionInfoEVProgress, Resource.Id.CoalitionInfoACProgress, Resource.Id.CoalitionInfoASProgress };
                int[] progressMaxTexts = { Resource.Id.CoalitionInfoHPProgressMax, Resource.Id.CoalitionInfoFRProgressMax, Resource.Id.CoalitionInfoEVProgressMax, Resource.Id.CoalitionInfoACProgressMax, Resource.Id.CoalitionInfoASProgressMax };
                int[] statusTexts = { Resource.Id.CoalitionInfoHPStatus, Resource.Id.CoalitionInfoFRStatus, Resource.Id.CoalitionInfoEVStatus, Resource.Id.CoalitionInfoACStatus, Resource.Id.CoalitionInfoASStatus };

                string[] growRatio = coalition.Abilities["Grow"].Split(';');

                for (int i = 0; i < progresses.Length; ++i)
                {
                    FindViewById<TextView>(progressMaxTexts[i]).Text = FindViewById<ProgressBar>(progresses[i]).Max.ToString();

                    string[] basicRatio = coalition.Abilities[abilities[i]].Split(';');
                    int value = (modIndex == 0) ? das.CalcAbility(abilities[i], int.Parse(basicRatio[0]), int.Parse(growRatio[0]), abilityLevel, abilityFavor, false) :
                        das.CalcAbility(abilities[i], int.Parse(basicRatio[1]), int.Parse(growRatio[1]), abilityLevel, abilityFavor, true);

                    ProgressBar pb = FindViewById<ProgressBar>(progresses[i]);
                    pb.Progress = value;

                    abilityValues[i] = value;

                    FindViewById<TextView>(statusTexts[i]).Text = $"{value} ({coalition.AbilityGrade[i]})";
                }

                if ((coalition.Type == "MG") || (coalition.Type == "SG"))
                {
                    FindViewById<LinearLayout>(Resource.Id.CoalitionInfoBulletLayout).Visibility = ViewStates.Visible;
                    FindViewById<LinearLayout>(Resource.Id.CoalitionInfoReloadLayout).Visibility = ViewStates.Visible;

                    double reloadTime = CalcReloadTime(coalition, coalition.Type, abilityValues[4]);
                    int bullet = coalition.HasMod ? int.Parse(coalition.Abilities["Bullet"].Split(';')[modIndex]) : int.Parse(coalition.Abilities["Bullet"]);

                    FindViewById<TextView>(Resource.Id.CoalitionInfoBulletProgressMax).Text = FindViewById<ProgressBar>(Resource.Id.CoalitionInfoBulletProgress).Max.ToString();

                    FindViewById<ProgressBar>(Resource.Id.CoalitionInfoBulletProgress).Progress = bullet;
                    FindViewById<TextView>(Resource.Id.CoalitionInfoBulletStatus).Text = bullet.ToString();
                    FindViewById<TextView>(Resource.Id.CoalitionInfoReloadStatus).Text = $"{reloadTime} {Resources.GetString(Resource.String.Time_Second)}";
                }
                else
                {
                    FindViewById<LinearLayout>(Resource.Id.CoalitionInfoBulletLayout).Visibility = ViewStates.Gone;
                    FindViewById<LinearLayout>(Resource.Id.CoalitionInfoReloadLayout).Visibility = ViewStates.Gone;
                }

                if (coalition.Type == "SG")
                {
                    FindViewById<LinearLayout>(Resource.Id.CoalitionInfoAMLayout).Visibility = ViewStates.Visible;
                    FindViewById<TextView>(Resource.Id.CoalitionInfoAMProgressMax).Text = FindViewById<ProgressBar>(Resource.Id.CoalitionInfoAMProgress).Max.ToString();

                    string[] basicRatio = coalition.Abilities["Armor"].Split(';');
                    int value = (modIndex == 0) ? das.CalcAbility("Armor", int.Parse(basicRatio[0]), int.Parse(growRatio[0]), abilityLevel, abilityFavor, false) :
                        das.CalcAbility("Armor", int.Parse(basicRatio[1]), int.Parse(growRatio[1]), abilityLevel, abilityFavor, true);

                    FindViewById<ProgressBar>(Resource.Id.CoalitionInfoAMProgress).Progress = value;

                    abilityValues[5] = value;
                    FindViewById<TextView>(Resource.Id.CoalitionInfoAMStatus).Text = $"{value} ({coalition.AbilityGrade[6]})";
                }
                else
                {
                    abilityValues[5] = 0;
                    FindViewById<LinearLayout>(Resource.Id.CoalitionInfoAMLayout).Visibility = ViewStates.Gone;
                }

                await LoadChart(chartCompareList.SelectedItemPosition);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Error Load Ability", Snackbar.LengthShort);
            }
        }*/

        private void ShowTitleSubLayout()
        {
            if (coalition.HasVoice)
            {
                //FindViewById<LinearLayout>(Resource.Id.CoalitionDBDetailVoiceLayout).Visibility = ViewStates.Visible;
            }

            //FindViewById<LinearLayout>(Resource.Id.CoalitionDBDetailExtraButtonLayout).Visibility = ViewStates.Visible;
        }

        private void SetCardTheme()
        {
            const float radius = 15.0f;

            foreach (var cv in scrollCardViews)
            {
                cv.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                cv.Radius = radius;
            }
        }

        private void ShowCardViewVisibility()
        {
            foreach (var cv in scrollCardViews)
            {
                cv.Visibility = ViewStates.Visible;
                cv.Alpha = 0.7f;
            }
        }

        private async void CoalitionDBDetailAnalysisSelectButton_Click(object sender, EventArgs e)
        {
            try
            {
                var analysisButton = sender as ImageView;

                analysisIndex = analysisButton.Id switch
                {
                    Resource.Id.CoalitionDBDetailAnalysisSelect0 => 0,
                    Resource.Id.CoalitionDBDetailAnalysisSelect1 => 1,
                    Resource.Id.CoalitionDBDetailAnalysisSelect2 => 2,
                    Resource.Id.CoalitionDBDetailAnalysisSelect3 => 3,
                    Resource.Id.CoalitionDBDetailAnalysisSelect4 => 4,
                    _ => 0,
                };

                foreach (int id in analysisButtonIds)
                {
                    FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);
                }

                analysisButton.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                if (initLoadComplete)
                {
                    //ListAbilityLevelFavor();
                    await InitLoadProcess(false);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.CoalitionDBDetail_AnalysisChangeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private double CalcReloadTime(Coalition Coalition, string type, int AttackSpeed)
        {
            double result = 0;

            switch (type)
            {
                case "MG":
                    int tAS = AttackSpeed;
                    result = (tAS == 0) ? 0 : (4 + 200 / tAS);
                    break;
                case "SG":
                    int tB = int.Parse(Coalition.Abilities["Bullet"]);
                    result = 1.5 + 0.5 * tB;
                    break;
            }

            return result;
        }

        public override void OnBackPressed()
        {
            if (voicePlayer.IsPlaying)
            {
                voicePlayer.Stop();
            }

            stream?.Dispose();
            
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }

        /*internal class CoalitionMaxAbility
        {
            public string AbilityType { get; private set; }
            public int AbilityValue { get; private set; }

            public CoalitionMaxAbility(string type, int value)
            {
                AbilityType = type;
                AbilityValue = value;
            }
        }

        internal class DataModel
        {
            public ObservableCollection<CoalitionMaxAbility> MaxAbilityList { get; private set; }
            public ObservableCollection<CoalitionMaxAbility> CompareAbilityList { get; private set; }

            private Coalition Coalition;

            private readonly int[] abilityValues;
            private readonly List<string> compareList;
            private readonly int compareIndex;
            private readonly int modIndex;
            private readonly int abilityLevel;
            private readonly int abilityFavor;
            private readonly AverageAbility averageAbility;
            
            public DataModel(int compareIndex, Coalition Coalition, int[] abilityValues, List<string> compareList, int modIndex, int abilityLevel, int abilityFavor, ref AverageAbility averageAbility)
            {
                this.compareIndex = compareIndex;
                this.Coalition = Coalition;
                this.abilityValues = abilityValues;
                this.compareList = compareList;
                this.modIndex = modIndex;
                this.abilityLevel = abilityLevel;
                this.abilityFavor = abilityFavor;
                this.averageAbility = averageAbility;

                MaxAbilityList = new ObservableCollection<CoalitionMaxAbility>()
                {
                    new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), abilityValues[0]),
                    new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), abilityValues[1]),
                    new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), abilityValues[2]),
                    new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), abilityValues[3]),
                    new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), abilityValues[4]),
                    new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), abilityValues[5])
                };

                CreateCompareCollection();
            }

            private void CreateCompareCollection()
            {
                try
                {
                    if (compareIndex == 0)
                    {
                        CompareAbilityList = new ObservableCollection<CoalitionMaxAbility>()
                        {
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), averageAbility.HP),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), averageAbility.FR),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), averageAbility.EV),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), averageAbility.AC),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), averageAbility.AS),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), averageAbility.AM)
                        };
                    }
                    else
                    {
                        var cCoalition = new Coalition(ETC.FindDataRow(ETC.CoalitionList, "Name", compareList[compareIndex]));
                        var das = new CoalitionAbilitySet(cCoalition.Type);

                        string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
                        int[] compareAbilityValues = { 0, 0, 0, 0, 0, 0 };
                        int growRatio = 0;

                        if (modIndex > 0)
                        {
                            growRatio = cCoalition.HasMod ? int.Parse(cCoalition.Abilities["Grow"].Split(';')[1]) : int.Parse(cCoalition.Abilities["Grow"].Split(';')[0]);
                        }
                        else
                        {
                            growRatio = int.Parse(cCoalition.Abilities["Grow"].Split(';')[0]);
                        }

                        for (int i = 0; i < abilities.Length; ++i)
                        {
                            int baseRatio = (modIndex == 0) ? int.Parse(cCoalition.Abilities[abilities[i]].Split(';')[0]) : int.Parse(cCoalition.Abilities[abilities[i]].Split(';')[1]);

                            if (modIndex > 0)
                            {
                                baseRatio = cCoalition.HasMod ? int.Parse(cCoalition.Abilities[abilities[i]].Split(';')[1]) : int.Parse(cCoalition.Abilities[abilities[i]].Split(';')[0]);
                            }
                            else
                            {
                                baseRatio = int.Parse(cCoalition.Abilities[abilities[i]].Split(';')[0]);
                            }

                            compareAbilityValues[i] = das.CalcAbility(abilities[i], baseRatio, growRatio, abilityLevel, abilityFavor, false);
                        }

                        if (Coalition.Type == "SG")
                        {
                            int baseRatio = (modIndex == 0) ? int.Parse(cCoalition.Abilities["Armor"].Split(';')[0]) : int.Parse(cCoalition.Abilities["Armor"].Split(';')[1]);

                            if (modIndex > 0)
                            {
                                baseRatio = cCoalition.HasMod ? int.Parse(cCoalition.Abilities["Armor"].Split(';')[1]) : int.Parse(cCoalition.Abilities["Armor"].Split(';')[0]);
                            }
                            else
                            {
                                baseRatio = int.Parse(cCoalition.Abilities["Armor"].Split(';')[0]);
                            }

                            compareAbilityValues[5] = das.CalcAbility("Armor", baseRatio, growRatio, abilityLevel, abilityFavor, false);
                        }
                        else
                        {
                            compareAbilityValues[5] = 0;
                        }

                        CompareAbilityList = new ObservableCollection<CoalitionMaxAbility>()
                        {
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), compareAbilityValues[0]),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), compareAbilityValues[1]),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), compareAbilityValues[2]),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), compareAbilityValues[3]),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), compareAbilityValues[4]),
                            new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), compareAbilityValues[5])
                        };
                    }
                }
                catch
                {
                    CompareAbilityList = new ObservableCollection<CoalitionMaxAbility>()
                    {
                        new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), 0),
                        new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), 0),
                        new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), 0),
                        new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), 0),
                        new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), 0),
                        new CoalitionMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), 0)
                    };
                }
            }
        }*/
    }
}