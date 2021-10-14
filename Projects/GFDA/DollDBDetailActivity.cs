using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.CardView.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.Transitions;

using Com.Syncfusion.Charts;

using Google.Android.Material.Snackbar;

using Plugin.SimpleAudioPlayer;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFDA
{
    [Activity(Label = "", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBDetailActivity : BaseAppCompatActivity
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

        private Doll doll;
        private DataRow dollInfoDR;
        private int modIndex;
        private int vCostumeIndex;
        private List<string> voiceList = new List<string>();

        private int abilityLevel = 1;
        private List<int> levelList = new List<int>();
        private int abilityFavor;
        private List<string> favorList = new List<string>();
        private DollAbilitySet das;
        private int[] abilityValues = new int[6];

        private bool isExtraFeatureOpen;
        private bool isChartLoad;
        private bool initLoadComplete;
        private bool isApplyModVoice;

        private ISimpleAudioPlayer voicePlayer;
        private FileStream stream;

        private AndroidX.AppCompat.Widget.Toolbar toolbar;
        private SwipeRefreshLayout refreshMainLayout;
        private NestedScrollView scrollLayout;
        private LinearLayout scrollMainContainer;
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

        private View basicInfoRootLayout;
        private View buffInfoRootLayout;
        private View skillInfoRootLayout;
        private View modSkillInfoRootLayout;
        private View abilityInfoRootLayout;
        private View abilityRadarChartRootLayout;

        private int curtainUpIcon = ETC.useLightTheme ? Resource.Drawable.ArrowUp_WhiteTheme : Resource.Drawable.ArrowUp;
        private int curtainDownIcon = ETC.useLightTheme ? Resource.Drawable.ArrowDown_WhiteTheme : Resource.Drawable.ArrowDown;

        int[] modButtonIds = { Resource.Id.DollDBDetailModSelect0, Resource.Id.DollDBDetailModSelect1, Resource.Id.DollDBDetailModSelect2, Resource.Id.DollDBDetailModSelect3 };
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
                SetContentView(Resource.Layout.DollDBDetailLayout);
                 
                // Set Data
                dollInfoDR = ETC.FindDataRow(ETC.dollList, "DicNumber", Intent.GetIntExtra("DicNum", 0));
                doll = new Doll(dollInfoDR);
                das = new DollAbilitySet(doll.Type);

                voicePlayer = CrossSimpleAudioPlayer.Current;

                // Set Toolbar
                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.DollDBDetailMainToolbar);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                FindViewById<TextView>(Resource.Id.DollDBDetailToolbarDicNumber).Text = $"No. {doll.DicNumber}";
                FindViewById<TextView>(Resource.Id.DollDBDetailToolbarName).Text = $"{doll.Name} - {doll.CodeName}";

                // Inflate CardView Layouts
                basicInfoRootLayout = LayoutInflater.Inflate(Resource.Layout.DollDBDetailLayout_CardView_Basic, new LinearLayout(this), true);
                buffInfoRootLayout = LayoutInflater.Inflate(Resource.Layout.DollDBDetailLayout_CardView_Buff, new LinearLayout(this), true);
                skillInfoRootLayout = LayoutInflater.Inflate(Resource.Layout.DollDBDetailLayout_CardView_Skill, new LinearLayout(this), true);
                modSkillInfoRootLayout = LayoutInflater.Inflate(Resource.Layout.DollDBDetailLayout_CardView_ModSkill, new LinearLayout(this), true);
                abilityInfoRootLayout = LayoutInflater.Inflate(Resource.Layout.DollDBDetailLayout_CardView_Ability, new LinearLayout(this), true);
                abilityRadarChartRootLayout = LayoutInflater.Inflate(Resource.Layout.DollDBDetailLayout_CardView_AbilityChart, new LinearLayout(this), true);

                // Init Refresh and Snackbar Layout
                refreshMainLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.DollDBDetailMainRefreshLayout);
                refreshMainLayout.Refresh += async delegate { await InitLoadProcess(true); };
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBDetailSnackbarLayout);

                FindViewById<ImageView>(Resource.Id.DollDBDetailSmallImage).Click += DollDBDetailSmallImage_Click;

                // Init Doll Voice's Features
                if (doll.HasVoice)
                {
                    voiceCostumeSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailVoiceCostumeSelector);
                    voiceCostumeSelector.ItemSelected += VoiceCostumeSelector_ItemSelected;
                    voiceSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailVoiceSelector);
                    voicePlayButton = FindViewById<Button>(Resource.Id.DollDBDetailVoicePlayButton);
                    voicePlayButton.Click += delegate { _ = PlayVoiceProcess(); };
                }

                // Init Doll Mod's Features
                if (doll.HasMod)
                {
                    foreach (int id in modButtonIds)
                    {
                        FindViewById<ImageButton>(id).Click += DollDBDetailModSelectButton_Click;
                        FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);
                    }

                    FindViewById<ImageButton>(modButtonIds[0]).SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                    Button ModStoryButton = FindViewById<Button>(Resource.Id.DollDBDetailModStoryButton);
                    ModStoryButton.Visibility = ViewStates.Visible;
                    ModStoryButton.Click += ModStoryButton_Click;
                }

                // Init Extra Menu Layout
                FindViewById<ImageButton>(Resource.Id.DollDBExtraFeatureButton).Click += ExtraMenuButton_Click;
                FindViewById<ImageButton>(Resource.Id.DollDBExtraFeatureButton).SetImageResource(curtainDownIcon);
                modelDataButton = FindViewById<Button>(Resource.Id.DollDBDetailModelDataButton);
                modelDataButton.Click += ModelDataButton_Click;

                // Init Main Scroll Layout
                scrollLayout = FindViewById<NestedScrollView>(Resource.Id.DollDBDetailScrollLayout);
                scrollMainContainer = FindViewById<LinearLayout>(Resource.Id.DollDBDetailScrollMainLayout);

                
                abilityLevelSelector = abilityInfoRootLayout.FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityLevelSelector);
                abilityLevelSelector.ItemSelected += (sender, e) => 
                {
                    abilityLevel = levelList[e.Position];

                    LoadAbility();
                };
                abilityFavorSelector = abilityInfoRootLayout.FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityFavorSelector);
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

                    if (initLoadComplete)
                    {
                        LoadAbility();
                    }
                };

                // Init Chart
                chartCompareList = abilityRadarChartRootLayout.FindViewById<Spinner>(Resource.Id.DollDBDetailAbilityChartCompareList);
                chartCompareList.ItemSelected += (sender, e) => { LoadChart(e.Position); };
                chart = abilityRadarChartRootLayout.FindViewById<SfChart>(Resource.Id.DollDBDetailAbilityRadarChart);

                InitCompareList();
                ListAbilityLevelFavor();

                InitializeProcess();
                await InitLoadProcess(false);

                /*if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DollDBDetail", true)))
                    ETC.RunHelpActivity(this, "DollDBDetail");*/
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
                MenuInflater.Inflate(Resource.Menu.DollDBDetailMenu, menu);

                if (doll.ProductTime == 0)
                {
                    menu?.FindItem(Resource.Id.DollDBDetailProductPercentage).SetVisible(false);
                }
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
                    case Resource.Id.DollDBDetailLink:
                        var pMenu = new AndroidX.AppCompat.Widget.PopupMenu(this, FindViewById<View>(Resource.Id.DollDBDetailLink));
                        pMenu.Inflate(Resource.Menu.DBLinkMenu);
                        pMenu.MenuItemClick += PMenu_MenuItemClick;
                        pMenu.Show();
                        break;
                    case Resource.Id.DollDBDetailProductPercentage:
                        var intent = new Intent(this, typeof(ProductPercentTableActivity));
                        intent.PutExtra("Info", new string[] { "Doll", doll.DicNumber.ToString() });
                        StartActivity(intent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.DollDBDetailGuideBookDetail:
                        var intent2 = new Intent(this, typeof(DollDBGuideImageViewer));
                        intent2.PutExtra("Info", doll.DicNumber);
                        StartActivity(intent2);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Cannot execute option menu", ToastLength.Short).Show();
            }

            return base.OnOptionsItemSelected(item);
        }

        private void CalcAverageAbility()
        {
            string[] abilityList = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
            const int abilityCount = 6;

            var das = new DollAbilitySet(doll.Type);
            int[] total = new int[abilityCount];

            if (typeAbilities == null)
            {
                typeAbilities = new List<int[]>();
                Doll tDoll;

                foreach (DataRow row in ETC.dollList.Rows)
                {
                    if ((string)row["Type"] != doll.Type)
                    {
                        continue;
                    }

                    tDoll = new Doll(row);

                    int[] abilityValues = new int[abilityCount + 1]; // HP, FireRate, Evasion, Accuracy, AttackSpeed, Armor, Grow

                    abilityValues[6] = int.Parse(tDoll.Abilities["Grow"].Split(';')[0]);

                    for (int i = 0; i < abilityList.Length; ++i)
                    {
                        abilityValues[i] = int.Parse(tDoll.Abilities[abilityList[i]].Split(';')[0]);
                        total[i] += das.CalcAbility(abilityList[i], abilityValues[i], abilityValues[6], abilityLevel, abilityFavor, false);
                    }

                    if (doll.Type == "SG")
                    {
                        abilityValues[5] = int.Parse(tDoll.Abilities["Armor"].Split(';')[0]);
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

                    if (doll.Type == "SG")
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
        }

        private void PMenu_MenuItemClick(object sender, AndroidX.AppCompat.Widget.PopupMenu.MenuItemClickEventArgs e)
        {
            try
            {
                string url = e.Item.ItemId switch
                {
                    Resource.Id.DBLinkNamu => $"https://namu.wiki/w/{doll.NameKR}(소녀전선)",
                    Resource.Id.DBLinkInven => $"http://gf.inven.co.kr/dataninfo/dolls/detail.php?d=126&c={doll.DicNumber}",
                    Resource.Id.DBLink36Base => $"https://girlsfrontline.kr/doll/{doll.DicNumber}",
                    Resource.Id.DBLinkGFDB => $"https://gfl.zzzzz.kr/doll.php?id={doll.DicNumber}&lang=ko",
                    _ => "",
                };

                var intent = new Intent(this, typeof(WebBrowserActivity));
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

        private void InitializeProcess()
        {
            try
            {
                if (doll.HasVoice)
                {
                    InitializeVoiceList();
                }

                var toolbarColor = doll.Grade switch
                {
                    2 => Android.Graphics.Color.SlateGray,
                    3 => Android.Graphics.Color.ParseColor("#55CCEE"),
                    4 => Android.Graphics.Color.ParseColor("#AACC22"),
                    5 => Android.Graphics.Color.ParseColor("#FFBB22"),
                    _ => Android.Graphics.Color.ParseColor("#C040B0"),
                };
                toolbar.SetBackgroundColor(toolbarColor);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window.SetStatusBarColor(toolbarColor);
                }

                InitTitleSubLayout();
                InitCardViews();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail Initialize Process", ToastLength.Short).Show();
            }
        }

        private void ModStoryButton_Click(object sender, EventArgs e)
        {
            try
            {
                string[] storyList = new string[]
                {
                    $"{doll.Name} MOD Story 1",
                    $"{doll.Name} MOD Story 2",
                    $"{doll.Name} MOD Story 3",
                    $"{doll.Name} MOD Story 4"
                };
                string[] topTitleList = new string[]
                {
                    "MOD Story",
                    "MOD Story",
                    "MOD Story",
                    "MOD Story"
                };

                var intent = new Intent(this, typeof(StoryReaderActivity));
                intent.PutExtra("Info", new string[] { "Sub", "ModStory", "0", storyList.Length.ToString(), doll.DicNumber.ToString() });
                intent.PutExtra("List", storyList);
                intent.PutExtra("TopList", topTitleList);
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

            try
            {
                TransitionManager.BeginDelayedTransition(refreshMainLayout);

                switch (isExtraFeatureOpen)
                {
                    case false:
                        isExtraFeatureOpen = true;
                        b.SetImageResource(curtainUpIcon);
                        FindViewById<LinearLayout>(Resource.Id.DollDBExtraFeatureLayout).Visibility = ViewStates.Visible;
                        scrollLayout.Visibility = ViewStates.Gone;
                        break;
                    case true:
                        isExtraFeatureOpen = false;
                        b.SetImageResource(curtainDownIcon);
                        FindViewById<LinearLayout>(Resource.Id.DollDBExtraFeatureLayout).Visibility = ViewStates.Gone;
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

                switch (modIndex)
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
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Error List Level", Snackbar.LengthShort);
            }
        }

        private void InitCompareList()
        {
            Doll cDoll;

            try
            {
                compareList = new List<string>()
                {
                    "Type Average"
                };

                foreach (DataRow dr in ETC.dollList.Rows)
                {

                    cDoll = new Doll(dr);

                    if ((doll.DicNumber == cDoll.DicNumber) || (cDoll.Type != doll.Type))
                    {
                        continue;
                    }

                    compareList.Add(cDoll.Name);

                    if (cDoll.CodeName == "stg940")
                    {
                        continue;
                    }
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

        private void ModelDataButton_Click(object sender, EventArgs e)
        {
            string FileName = $"{doll.DicNumber}.txt";
            string data = "";

            try
            {
                var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(this, ETC.dialogBG);
                ad.SetTitle(Resource.String.DollDBDetailLayout_ModelData);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                Task.Run(async () =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        data = await wc.DownloadStringTaskAsync(Path.Combine(ETC.server, "Data", "Text", "Gun", "ModelData", FileName)).ConfigureAwait(false);
                    }
                }).Wait();

                ad.SetMessage(data);
                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private void LoadChart(int compareIndex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
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

                    var model = new DataModel(compareIndex, doll, abilityValues, compareList, modIndex, abilityLevel, abilityFavor, ref avgAbility);

                    var radar = new RadarSeries
                    {
                        ItemsSource = model.MaxAbilityList,
                        XBindingPath = "AbilityType",
                        YBindingPath = "AbilityValue",
                        DrawType = PolarChartDrawType.Line,
                        Color = Android.Graphics.Color.LightGreen,
                        EnableAnimation = true,
                        Label = doll.Name,
                        TooltipEnabled = true
                    };

                    chart.Series.Add(radar);

                    var radar2 = new RadarSeries
                    {
                        ItemsSource = model.CompareAbilityList,
                        XBindingPath = "AbilityType",
                        YBindingPath = "AbilityValue",
                        DrawType = PolarChartDrawType.Line,
                        Color = Android.Graphics.Color.Magenta,
                        EnableAnimation = true,
                        Label = (compareIndex == 0) ? $"{doll.Type}{Resources.GetString(Resource.String.DollDBDetail_RadarAverageString)}" : compareList[compareIndex],
                        TooltipEnabled = true
                    };

                    chart.Series.Add(radar2);

                    isChartLoad = true;
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                    Toast.MakeText(this, "Chart load error", ToastLength.Short).Show();
                }
            });
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                if (isChartLoad)
                {
                    LoadChart(chartCompareList.SelectedItemPosition);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private async Task PlayVoiceProcess()
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
                        string baseName = isApplyModVoice ? $"{doll.CodeName}Mod" : $"{doll.CodeName}";

                        voiceServerURL = Path.Combine(ETC.server, "Data", "Voice", "Doll", "New", baseName, $"{baseName}_{voice}_JP.wav");
                        target = Path.Combine(ETC.cachePath, "Voices", "Doll", $"{baseName}_{voice}_JP.gfdcache");
                        break;
                    default:
                        voiceServerURL = Path.Combine(ETC.server, "Data", "Voice", "Doll", "New", $"{doll.CodeName}_{vCostumeIndex - 1}", $"{doll.CodeName}_{vCostumeIndex - 1}_{voice}_JP.wav");
                        target = Path.Combine(ETC.cachePath, "Voices", "Doll", $"{doll.CodeName}_{vCostumeIndex - 1}_{voice}_JP.gfdcache");
                        break;
                }

                if (!File.Exists(target))
                {
                    using (var wc = new WebClient())
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
        }

        private void InitializeVoiceList()
        {
            try
            {
                var voiceCategoryList = new List<string>()
                {
                    "Default"
                };

                if (doll.CostumeVoices != null)
                {
                    for (int i = 0; i < (doll.CostumeVoices.Length / doll.CostumeVoices.Rank); ++i)
                    {
                        voiceCategoryList.Add(doll.Costumes[int.Parse(doll.CostumeVoices[i, 0])]);
                    }
                }

                // Create Voice Costume Selector Adapter
                var adapter = new ArrayAdapter<string>(this, Resource.Layout.SpinnerListLayout, voiceCategoryList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                voiceCostumeSelector.Adapter = adapter;

                // Create Voice Selector Adapter
                var adapter2 = new ArrayAdapter<string>(this, Resource.Layout.SpinnerListLayout, voiceList);
                adapter2.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                voiceSelector.Adapter = adapter2;

                voiceCostumeSelector.SetSelection(0);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.VoiceList_InitError, ToastLength.Short).Show();
            }
        }

        private void UpdateVoiceList()
        {
            try
            {
                var adapter = voiceCostumeSelector.Adapter as ArrayAdapter<string>;

                adapter.Remove(adapter.GetItem(0));
                adapter.Insert(isApplyModVoice ? "Default - MOD" : "Default", 0);

                adapter.NotifyDataSetChanged();
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

                switch (vCostumeIndex)
                {
                    case 0:
                        voiceList.AddRange(doll.Voices);
                        break;
                    default:
                        voiceList.AddRange(doll.CostumeVoices[vCostumeIndex - 1, 1].Split(';'));
                        break;
                }

                var voiceSelectorAdapter = (voiceSelector.Adapter as ArrayAdapter<string>);

                voiceSelectorAdapter.Clear();
                voiceSelectorAdapter.AddAll(voiceList);
                voiceSelectorAdapter.NotifyDataSetChanged();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.VoiceList_InitError, ToastLength.Short).Show();
            }
        }

        private void DollDBDetailSmallImage_Click(object sender, EventArgs e)
        {
            try
            {
                var dollImageViewer = new Intent(this, typeof(DollDBImageViewer));
                dollImageViewer.PutExtra("Data", $"{doll.DicNumber};{modIndex}");
                StartActivity(dollImageViewer);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
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

                TransitionManager.BeginDelayedTransition(refreshMainLayout);

                LoadTitle(isRefresh);
                LoadBasic();
                LoadBuff();
                LoadSkill(isRefresh);

                if (modIndex >= 2)
                {
                    LoadModSkill(isRefresh);
                }

                LoadAbility();

                ShowCardViewVisibility();          
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

                Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
            }
            finally
            {
                refreshMainLayout.Refreshing = false;
                initLoadComplete = true;
            }
        }

        private void LoadTitle(bool isRefresh)
        {
            if (Preferences.Get("DBDetailBackgroundImage", false))
            {
                try
                {
                    string url = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal", $"{doll.DicNumber}.png");
                    string target = Path.Combine(ETC.cachePath, "Doll", "Normal", $"{doll.DicNumber}.gfdcache");

                    if (!File.Exists(target) || isRefresh)
                    {
                        using (var wc = new WebClient())
                        {
                            wc.DownloadFile(url, target);
                        }
                    }

                    var drawable = Drawable.CreateFromPath(target);
                    drawable.SetAlpha(40);

                    FindViewById<ImageView>(Resource.Id.DollDBDetailBackgroundImageView).SetImageDrawable(drawable);
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }
            }

            string fileName = (modIndex == 3) ? $"{doll.DicNumber}_M" : doll.DicNumber.ToString();

            try
            {
                string url = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal_Crop", $"{fileName}.png");
                string target = Path.Combine(ETC.cachePath, "Doll", "Normal_Crop", $"{fileName}.gfdcache");

                if (!File.Exists(target) || isRefresh)
                {
                    using (var wc = new WebClient())
                    {
                        wc.DownloadFile(url, target);
                    }
                }

                FindViewById<ImageView>(Resource.Id.DollDBDetailSmallImage).SetImageDrawable(Drawable.CreateFromPath(target));
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }

            FindViewById<TextView>(Resource.Id.DollDBDetailDollName).Text = doll.Name;
            FindViewById<TextView>(Resource.Id.DollDBDetailDollDicNumber).Text = $"No. {doll.DicNumber}";
            FindViewById<TextView>(Resource.Id.DollDBDetailDollProductTime).Text = ETC.CalcTime(doll.ProductTime);
            FindViewById<TextView>(Resource.Id.DollDBDetailDollProductDialog).Text = doll.ProductDialog;
        }

        private void LoadBasic()
        {
            var gradeString = new StringBuilder();
            int nowGrade = (modIndex >= 1) ? doll.ModGrade : doll.Grade;

            for (int i = 0; i < nowGrade; ++i)
            {
                gradeString.Append('★');
            }

            basicInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailInfoGrade).Text = gradeString.ToString();
            basicInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailInfoType).Text = doll.Type;
            basicInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailInfoName).Text = doll.Name;
            basicInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailInfoNickName).Text = doll.NickName;
            basicInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailInfoIllustrator).Text = doll.Illustrator;
            basicInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailInfoVoiceActor).Text = doll.VoiceActor;
            basicInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailInfoRealModel).Text = doll.RealModel;
            basicInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailInfoCountry).Text = doll.Country;
            basicInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailInfoHowToGain).Text = (string)dollInfoDR["DropEvent"];
        }

        private void LoadBuff()
        {
            int[] buffIds =
            {
                Resource.Id.DollDBDetailBuff1, Resource.Id.DollDBDetailBuff2, Resource.Id.DollDBDetailBuff3,
                Resource.Id.DollDBDetailBuff4, Resource.Id.DollDBDetailBuff5, Resource.Id.DollDBDetailBuff6,
                Resource.Id.DollDBDetailBuff7, Resource.Id.DollDBDetailBuff8, Resource.Id.DollDBDetailBuff9
            };
            int[] buffData = (modIndex == 0) ? doll.BuffFormation : doll.ModBuffFormation;

            for (int i = 0; i < buffData.Length; ++i)
            {
                var color = buffData[i] switch
                {
                    0 => Android.Graphics.Color.Gray,
                    1 => Android.Graphics.Color.ParseColor("#54A716"),
                    2 => Android.Graphics.Color.White,
                    _ => Android.Graphics.Color.Red,
                };

                buffInfoRootLayout.FindViewById<View>(buffIds[i]).SetBackgroundColor(color);
            }

            string[] buff = (modIndex >= 1) ? doll.ModBuffInfo : doll.BuffInfo;
            string[] buffType = buff[0].Split(',');

            var buffContentRootLayout = buffInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollDBDetailBuffDetailLayout);
            buffContentRootLayout.RemoveAllViews();

            var magSb = new StringBuilder();

            for (int i = 0; i < buffType.Length; ++i)
            {
                var contentLayout = LayoutInflater.Inflate(Resource.Layout.FormationBuff_Content_Layout, null);

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

                contentLayout.FindViewById<ImageView>(Resource.Id.FormationBuffContentIcon).SetImageResource(id);
                contentLayout.FindViewById<TextView>(Resource.Id.FormationBuffContentType).Text = name;

                magSb.Clear();

                for (int k = 1; k < buff.Length; ++k)
                {
                    magSb.Append($"{buff[k].Split(',')[i]}%");
                    
                    if (k < (buff.Length - 1))
                    {
                        magSb.Append(" - ");
                    }
                }

                var magTextView = contentLayout.FindViewById<TextView>(Resource.Id.FormationBuffContentMag);

                magTextView.Text = magSb.ToString();
                magTextView.SetTextColor(magSb[0] switch
                {
                    '-' => Android.Graphics.Color.OrangeRed,
                    _ => Android.Graphics.Color.Green
                });

                buffContentRootLayout.AddView(contentLayout);
            }

            string effectTypeString;

            if (doll.BuffType[0] == "ALL")
            {
                effectTypeString = Resources.GetString(Resource.String.DollDBDetail_BuffType_All);
            }
            else
            {
                var sb = new StringBuilder();

                foreach (var type in doll.BuffType)
                {
                    sb.Append($"{type} ");
                }

                effectTypeString = $"{sb} {Resources.GetString(Resource.String.DollDBDetail_BuffType_ConfirmString)}";
            }

            buffInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailEffectType).Text = effectTypeString;

        }

        private void LoadSkill(bool isRefresh)
        {
            string skillName = doll.SkillName;

            try
            {
                string url = Path.Combine(ETC.server, "Data", "Images", "SkillIcons", $"{skillName}.png");
                string target = Path.Combine(ETC.cachePath, "Skill", $"{skillName}.gfdcache");

                if (!File.Exists(target) || isRefresh)
                {
                    using (var wc = new WebClient())
                    {
                        wc.DownloadFile(url, target);
                    }
                }

                skillInfoRootLayout.FindViewById<ImageView>(Resource.Id.DollDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(target));
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }

            skillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailSkillName).Text = skillName;

            if (ETC.useLightTheme)
            {
                skillInfoRootLayout.FindViewById<ImageView>(Resource.Id.DollDBDetailSkillInitCoolTimeIcon).SetImageResource(Resource.Drawable.FirstCoolTime_Icon_WhiteTheme);
                skillInfoRootLayout.FindViewById<ImageView>(Resource.Id.DollDBDetailSkillCoolTimeIcon).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
            }

            string[] skillAbilities = (modIndex == 0) ? doll.SkillEffect : doll.SkillEffectAfterMod;
            string[] skillMags = (modIndex == 0) ? doll.SkillMag : doll.SkillMagAfterMod;

            var skillInitCoolTime = skillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailSkillInitCoolTime);
            skillInitCoolTime.SetTextColor(Android.Graphics.Color.Orange);
            skillInitCoolTime.Text = skillMags[0];

            var skillCoolTime = skillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailSkillCoolTime);
            skillCoolTime.SetTextColor(Android.Graphics.Color.DarkOrange);
            skillCoolTime.Text = skillMags[1];

            skillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailSkillExplain).Text = doll.SkillExplain;

            var skillTableSubLayout = skillInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollDBDetailSkillAbilitySubLayout);
            skillTableSubLayout.RemoveAllViews();

            for (int i = 2; i < skillAbilities.Length; ++i)
            {
                var layout = new LinearLayout(this)
                {
                    Orientation = Orientation.Horizontal,
                    LayoutParameters = skillInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollDBDetailSkillAbilityTopLayout).LayoutParameters
                };

                var ability = new TextView(this)
                {
                    LayoutParameters = skillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailSkillAbilityTopText1).LayoutParameters,
                    Text = skillAbilities[i],
                    Gravity = GravityFlags.Center
                };
                var mag = new TextView(this)
                {
                    LayoutParameters = skillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailSkillAbilityTopText2).LayoutParameters,
                    Text = skillMags[i],
                    Gravity = GravityFlags.Center
                };

                layout.AddView(ability);
                layout.AddView(mag);

                skillTableSubLayout.AddView(layout);
            }
        }

        private void LoadModSkill(bool isRefresh)
        {
            string modSkillName = doll.ModSkillName;

            try
            {
                string url = Path.Combine(ETC.server, "Data", "Images", "SkillIcons", $"{modSkillName}.png");
                string target = Path.Combine(ETC.cachePath, "Skill", $"{modSkillName}.gfdcache");

                if (!File.Exists(target) || isRefresh)
                {
                    using (var wc = new WebClient())
                    {
                        wc.DownloadFile(url, target);
                    }
                }

                modSkillInfoRootLayout.FindViewById<ImageView>(Resource.Id.DollDBDetailModSkillIcon).SetImageDrawable(Drawable.CreateFromPath(target));
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }

            modSkillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailModSkillName).Text = modSkillName;
            modSkillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailModSkillExplain).Text = doll.ModSkillExplain;

            string[] modSkillAbilities = doll.ModSkillEffect;
            string[] modSkillMags = doll.ModSkillMag;

            var modSkillTableSubLayout = modSkillInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSkillAbilitySubLayout);
            modSkillTableSubLayout.RemoveAllViews();

            for (int i = 0; i < modSkillAbilities.Length; ++i)
            {
                var layout = new LinearLayout(this)
                {
                    Orientation = Orientation.Horizontal,
                    LayoutParameters = modSkillInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSkillAbilityTopLayout).LayoutParameters
                };

                var ability = new TextView(this)
                {
                    LayoutParameters = modSkillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailModSkillAbilityTopText1).LayoutParameters,
                    Text = modSkillAbilities[i],
                    Gravity = GravityFlags.Center
                };
                var mag = new TextView(this)
                {
                    LayoutParameters = modSkillInfoRootLayout.FindViewById<TextView>(Resource.Id.DollDBDetailModSkillAbilityTopText2).LayoutParameters,
                    Text = modSkillMags[i],
                    Gravity = GravityFlags.Center
                };

                layout.AddView(ability);
                layout.AddView(mag);

                modSkillTableSubLayout.AddView(layout);
            }
        }

        private void LoadAbility()
        {
            try
            {
                string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
                int[] progresses = { Resource.Id.DollInfoHPProgress, Resource.Id.DollInfoFRProgress, Resource.Id.DollInfoEVProgress, Resource.Id.DollInfoACProgress, Resource.Id.DollInfoASProgress };
                int[] progressMaxTexts = { Resource.Id.DollInfoHPProgressMax, Resource.Id.DollInfoFRProgressMax, Resource.Id.DollInfoEVProgressMax, Resource.Id.DollInfoACProgressMax, Resource.Id.DollInfoASProgressMax };
                int[] statusTexts = { Resource.Id.DollInfoHPStatus, Resource.Id.DollInfoFRStatus, Resource.Id.DollInfoEVStatus, Resource.Id.DollInfoACStatus, Resource.Id.DollInfoASStatus };

                string[] growRatio = doll.Abilities["Grow"].Split(';');

                for (int i = 0; i < progresses.Length; ++i)
                {
                    abilityInfoRootLayout.FindViewById<TextView>(progressMaxTexts[i]).Text = abilityInfoRootLayout.FindViewById<ProgressBar>(progresses[i]).Max.ToString();

                    string[] basicRatio = doll.Abilities[abilities[i]].Split(';');
                    int value = (modIndex == 0) ? das.CalcAbility(abilities[i], int.Parse(basicRatio[0]), int.Parse(growRatio[0]), abilityLevel, abilityFavor, false) :
                        das.CalcAbility(abilities[i], int.Parse(basicRatio[1]), int.Parse(growRatio[1]), abilityLevel, abilityFavor, true);

                    var pb = abilityInfoRootLayout.FindViewById<ProgressBar>(progresses[i]);
                    pb.Progress = value;

                    abilityValues[i] = value;

                    abilityInfoRootLayout.FindViewById<TextView>(statusTexts[i]).Text = $"{value} ({doll.AbilityGrade[i]})";
                }

                if ((doll.Type == "MG") || (doll.Type == "SG"))
                {
                    abilityInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollInfoBulletLayout).Visibility = ViewStates.Visible;
                    abilityInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollInfoReloadLayout).Visibility = ViewStates.Visible;

                    double reloadTime = CalcReloadTime(doll, doll.Type, abilityValues[4], modIndex);
                    int bullet = doll.HasMod ? int.Parse(doll.Abilities["Bullet"].Split(';')[modIndex]) : int.Parse(doll.Abilities["Bullet"]);

                    abilityInfoRootLayout.FindViewById<TextView>(Resource.Id.DollInfoBulletProgressMax).Text = abilityInfoRootLayout.FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress).Max.ToString();

                    abilityInfoRootLayout.FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress).Progress = bullet;
                    abilityInfoRootLayout.FindViewById<TextView>(Resource.Id.DollInfoBulletStatus).Text = bullet.ToString();
                    abilityInfoRootLayout.FindViewById<TextView>(Resource.Id.DollInfoReloadStatus).Text = $"{reloadTime} {Resources.GetString(Resource.String.Time_Second)}";
                }
                else
                {
                    abilityInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollInfoBulletLayout).Visibility = ViewStates.Gone;
                    abilityInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollInfoReloadLayout).Visibility = ViewStates.Gone;
                }

                if (doll.Type == "SG")
                {
                    abilityInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollInfoAMLayout).Visibility = ViewStates.Visible;
                    abilityInfoRootLayout.FindViewById<TextView>(Resource.Id.DollInfoAMProgressMax).Text = FindViewById<ProgressBar>(Resource.Id.DollInfoAMProgress).Max.ToString();

                    string[] basicRatio = doll.Abilities["Armor"].Split(';');
                    int value = (modIndex == 0) ? das.CalcAbility("Armor", int.Parse(basicRatio[0]), int.Parse(growRatio[0]), abilityLevel, abilityFavor, false) :
                        das.CalcAbility("Armor", int.Parse(basicRatio[1]), int.Parse(growRatio[1]), abilityLevel, abilityFavor, true);

                    abilityInfoRootLayout.FindViewById<ProgressBar>(Resource.Id.DollInfoAMProgress).Progress = value;

                    abilityValues[5] = value;
                    abilityInfoRootLayout.FindViewById<TextView>(Resource.Id.DollInfoAMStatus).Text = $"{value} ({doll.AbilityGrade[6]})";
                }
                else
                {
                    abilityValues[5] = 0;
                    abilityInfoRootLayout.FindViewById<LinearLayout>(Resource.Id.DollInfoAMLayout).Visibility = ViewStates.Gone;
                }

                double[] dps = ETC.CalcDPS(abilityValues[1], abilityValues[4], 0, abilityValues[3], 3, int.Parse(doll.Abilities["Critical"]), 5);
                abilityInfoRootLayout.FindViewById<TextView>(Resource.Id.DollInfoDPSStatus).Text = $"{dps[0].ToString("F2")} ~ {dps[1].ToString("F2")}";

                LoadChart(chartCompareList.SelectedItemPosition);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Error Load Ability", BaseTransientBottomBar.LengthShort);
            }
        }

        private void InitTitleSubLayout()
        {
            if (doll.HasVoice)
            {
                FindViewById<LinearLayout>(Resource.Id.DollDBDetailVoiceLayout).Visibility = ViewStates.Visible;
            }
            if (doll.HasMod)
            {
                FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSelectLayout).Visibility = ViewStates.Visible;
            }

            FindViewById<LinearLayout>(Resource.Id.DollDBDetailExtraButtonLayout).Visibility = ViewStates.Visible;
        }

        private void ShowCardViewVisibility()
        {
            foreach (var cardview in scrollCardViews)
            {
                cardview.Visibility = (cardview.Id == Resource.Id.DollDBDetailModSkillCardLayout) && (modIndex < 2) ?
                    ViewStates.Gone : ViewStates.Visible;
            }
        }

        private void InitCardViews()
        {
            try
            {
                scrollMainContainer.AddView(basicInfoRootLayout);
                scrollMainContainer.AddView(buffInfoRootLayout);
                scrollMainContainer.AddView(skillInfoRootLayout);
                scrollMainContainer.AddView(modSkillInfoRootLayout);
                scrollMainContainer.AddView(abilityInfoRootLayout);
                scrollMainContainer.AddView(abilityRadarChartRootLayout);

                scrollCardViews.Add(basicInfoRootLayout.FindViewById<CardView>(Resource.Id.DollDBDetailBasicInfoCardLayout));
                scrollCardViews.Add(buffInfoRootLayout.FindViewById<CardView>(Resource.Id.DollDBDetailBuffCardLayout));
                scrollCardViews.Add(skillInfoRootLayout.FindViewById<CardView>(Resource.Id.DollDBDetailSkillCardLayout));
                scrollCardViews.Add(modSkillInfoRootLayout.FindViewById<CardView>(Resource.Id.DollDBDetailModSkillCardLayout));
                scrollCardViews.Add(abilityInfoRootLayout.FindViewById<CardView>(Resource.Id.DollDBDetailAbilityCardLayout));
                scrollCardViews.Add(abilityRadarChartRootLayout.FindViewById<CardView>(Resource.Id.DollDBDetailAbilityRadarChartCardLayout));
                
                foreach (var cardview in scrollCardViews)
                {
                    cardview.Alpha = 0.7f;

                    if (ETC.useLightTheme)
                    {
                        cardview.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                        cardview.Radius = 15.0f;
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, "Error init cardviews", Snackbar.LengthShort);
            }
        }

        private async void DollDBDetailModSelectButton_Click(object sender, EventArgs e)
        {
            try
            {
                var modButton = sender as ImageView;

                modIndex = modButton.Id switch
                {
                    Resource.Id.DollDBDetailModSelect0 => 0,
                    Resource.Id.DollDBDetailModSelect1 => 1,
                    Resource.Id.DollDBDetailModSelect2 => 2,
                    Resource.Id.DollDBDetailModSelect3 => 3,
                    _ => 0,
                };

                foreach (int id in modButtonIds)
                {
                    FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);
                }

                modButton.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));

                isApplyModVoice = (modIndex == 3) && (doll.ModVoices != null);

                if (initLoadComplete)
                {
                    ListAbilityLevelFavor();
                    UpdateVoiceList();
                    await InitLoadProcess(false);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DollDBDetail_MODChangeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private double CalcReloadTime(Doll doll, string type, int attackSpeed, int mod = 0)
        {
            double result = 0;

            switch (type)
            {
                case "MG":
                    int tAS = attackSpeed;
                    result = (tAS == 0) ? 0 : (4 + 200 / tAS);
                    break;
                case "SG":
                    int tB = int.Parse(doll.Abilities["Bullet"].Split(';')[mod]);
                    result = 1.5 + 0.5 * tB;
                    break;
            }

            return result;
        }

        public override void OnBackPressed()
        {
            if (voicePlayer != null)
            {
                if (voicePlayer.IsPlaying)
                {
                    voicePlayer.Stop();
                }
            }

            stream?.Dispose();
            
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }

        internal class DollMaxAbility
        {
            public string AbilityType { get; private set; }
            public int AbilityValue { get; private set; }

            public DollMaxAbility(string type, int value)
            {
                AbilityType = type;
                AbilityValue = value;
            }
        }

        internal class DataModel
        {
            public ObservableCollection<DollMaxAbility> MaxAbilityList { get; private set; }
            public ObservableCollection<DollMaxAbility> CompareAbilityList { get; private set; }

            private Doll doll;

            private readonly int[] abilityValues;
            private readonly List<string> compareList;
            private readonly int compareIndex;
            private readonly int modIndex;
            private readonly int abilityLevel;
            private readonly int abilityFavor;
            private readonly AverageAbility averageAbility;
            
            public DataModel(int compareIndex, Doll doll, int[] abilityValues, List<string> compareList, int modIndex, int abilityLevel, int abilityFavor, ref AverageAbility averageAbility)
            {
                this.compareIndex = compareIndex;
                this.doll = doll;
                this.abilityValues = abilityValues;
                this.compareList = compareList;
                this.modIndex = modIndex;
                this.abilityLevel = abilityLevel;
                this.abilityFavor = abilityFavor;
                this.averageAbility = averageAbility;

                MaxAbilityList = new ObservableCollection<DollMaxAbility>()
                {
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), abilityValues[0]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), abilityValues[1]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), abilityValues[2]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), abilityValues[3]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), abilityValues[4]),
                    new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), abilityValues[5])
                };

                CreateCompareCollection();
            }

            private void CreateCompareCollection()
            {
                try
                {
                    if (compareIndex == 0)
                    {
                        CompareAbilityList = new ObservableCollection<DollMaxAbility>()
                        {
                            new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), averageAbility.HP),
                            new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), averageAbility.FR),
                            new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), averageAbility.EV),
                            new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), averageAbility.AC),
                            new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), averageAbility.AS),
                            new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), averageAbility.AM)
                        };
                    }
                    else
                    {
                        var cDoll = new Doll(ETC.FindDataRow(ETC.dollList, "Name", compareList[compareIndex]));
                        var das = new DollAbilitySet(cDoll.Type);

                        string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
                        int[] compareAbilityValues = { 0, 0, 0, 0, 0, 0 };
                        int growRatio = 0;

                        if (modIndex > 0)
                        {
                            growRatio = cDoll.HasMod ? int.Parse(cDoll.Abilities["Grow"].Split(';')[1]) : int.Parse(cDoll.Abilities["Grow"].Split(';')[0]);
                        }
                        else
                        {
                            growRatio = int.Parse(cDoll.Abilities["Grow"].Split(';')[0]);
                        }

                        for (int i = 0; i < abilities.Length; ++i)
                        {
                            int baseRatio = (modIndex == 0) ? int.Parse(cDoll.Abilities[abilities[i]].Split(';')[0]) : int.Parse(cDoll.Abilities[abilities[i]].Split(';')[1]);

                            if (modIndex > 0)
                            {
                                baseRatio = cDoll.HasMod ? int.Parse(cDoll.Abilities[abilities[i]].Split(';')[1]) : int.Parse(cDoll.Abilities[abilities[i]].Split(';')[0]);
                            }
                            else
                            {
                                baseRatio = int.Parse(cDoll.Abilities[abilities[i]].Split(';')[0]);
                            }

                            compareAbilityValues[i] = das.CalcAbility(abilities[i], baseRatio, growRatio, abilityLevel, abilityFavor, false);
                        }

                        if (doll.Type == "SG")
                        {
                            int baseRatio = (modIndex == 0) ? int.Parse(cDoll.Abilities["Armor"].Split(';')[0]) : int.Parse(cDoll.Abilities["Armor"].Split(';')[1]);

                            if (modIndex > 0)
                            {
                                baseRatio = cDoll.HasMod ? int.Parse(cDoll.Abilities["Armor"].Split(';')[1]) : int.Parse(cDoll.Abilities["Armor"].Split(';')[0]);
                            }
                            else
                            {
                                baseRatio = int.Parse(cDoll.Abilities["Armor"].Split(';')[0]);
                            }

                            compareAbilityValues[5] = das.CalcAbility("Armor", baseRatio, growRatio, abilityLevel, abilityFavor, false);
                        }
                        else
                        {
                            compareAbilityValues[5] = 0;
                        }

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
                catch
                {
                    CompareAbilityList = new ObservableCollection<DollMaxAbility>()
                    {
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_HP), 0),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_FR), 0),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_EV), 0),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AC), 0),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AS), 0),
                        new DollMaxAbility(ETC.Resources.GetString(Resource.String.Common_AM), 0)
                    };
                }
            }
        }
    }
}