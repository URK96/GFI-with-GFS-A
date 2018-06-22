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

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBDetailActivity : FragmentActivity
    {
        private LinearLayout SkillTableSubLayout;
        private LinearLayout ModSkillTableSubLayout;

        private DataRow DollInfoDR = null;
        private string DollName;
        private int DollDicNum;
        private int DollGrade;
        private string DollType;
        private int ModIndex = 0;
        private string[] VoiceList;

        private bool IsOpenFABMenu = false;

        private ScrollView ScrollLayout;
        private CoordinatorLayout SnackbarLayout = null;

        private ProgressBar InitLoadProgressBar;
        private Spinner VoiceSelector;
        private Button VoicePlayButton;
        private FloatingActionButton PercentTableFAB;
        private FloatingActionButton MainFAB;
        private FloatingActionButton NamuWikiFAB;
        private FloatingActionButton InvenFAB;
        private FloatingActionButton BaseFAB;

        private AdView adview;

        int[] ModButtonIds = { Resource.Id.DollDBDetailModSelect0, Resource.Id.DollDBDetailModSelect1, Resource.Id.DollDBDetailModSelect2, Resource.Id.DollDBDetailModSelect3 };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.DollDBDetailLayout);

                DollName = Intent.GetStringExtra("Keyword");

                DollInfoDR = ETC.FindDataRow(ETC.DollList, "Name", DollName);
                DollDicNum = (int)DollInfoDR["DicNumber"];
                DollGrade = (int)DollInfoDR["Grade"];
                DollType = (string)DollInfoDR["Type"];

                adview = FindViewById<AdView>(Resource.Id.DollDBDetail_adView);
                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.DollDBDetailInitLoadProgress);
                SkillTableSubLayout = FindViewById<LinearLayout>(Resource.Id.DollDBDetailSkillAbilitySubLayout);
                ModSkillTableSubLayout = FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSkillAbilitySubLayout);

                if ((bool)DollInfoDR["HasMod"] == false) FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSelectLayout).Visibility = ViewStates.Gone;
                else
                {
                    foreach (int id in ModButtonIds)
                    {
                        FindViewById<ImageButton>(id).Click += DollDBDetailModSelectButton_Click;
                        FindViewById<ImageButton>(id).SetBackgroundColor(Android.Graphics.Color.Transparent);
                    }

                    FindViewById<ImageButton>(ModButtonIds[0]).SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                }

                FindViewById<ImageView>(Resource.Id.DollDBDetailSmallImage).Click += DollDBDetailSmallImage_Click;

                VoiceSelector = FindViewById<Spinner>(Resource.Id.DollDBDetailVoiceSelector);
                InitializeVoiceList();
                VoicePlayButton = FindViewById<Button>(Resource.Id.DollDBDetailVoicePlayButton);
                VoicePlayButton.Click += VoicePlayButton_Click;

                ScrollLayout = FindViewById<ScrollView>(Resource.Id.DollDBDetailScrollLayout);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBDetailSnackbarLayout);

                PercentTableFAB = FindViewById<FloatingActionButton>(Resource.Id.DollDBDetailProductPercentFAB);
                if ((int)DollInfoDR["ProductTIme"] == 0) PercentTableFAB.Visibility = ViewStates.Gone;
                MainFAB = FindViewById<FloatingActionButton>(Resource.Id.DollDBDetailSideLinkMainFAB);
                NamuWikiFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkNamuWikiFAB);
                InvenFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkInvenFAB);
                BaseFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkBaseFAB);

                PercentTableFAB.Click += PercentTableFAB_Click;
                MainFAB.Click += MainFAB_Click;
                NamuWikiFAB.Click += MainSubFAB_Click;
                InvenFAB.Click += MainSubFAB_Click;
                BaseFAB.Click += MainSubFAB_Click;

                InitLoadProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void PercentTableFAB_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(DollDBProductPercentTableActivity));
                intent.PutExtra("DollNum", DollDicNum.ToString());
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

                string VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", DollName, DollName + "_" + voice + "_JP.wav");
                string target = Path.Combine(ETC.CachePath, "Voices", DollName + "_" + voice + "_JP.wav");

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
                if ((bool)DollInfoDR["HasVoice"] == false)
                {
                    FindViewById<LinearLayout>(Resource.Id.DollDBDetailVoiceLayout).Visibility = ViewStates.Gone;
                    return;
                }
                
                VoiceList = ((string)DollInfoDR["Voices"]).Split(';');

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, VoiceList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                VoiceSelector.Adapter = adapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, "보이스 리스트 초기화 실패", ToastLength.Short).Show();
            }
        }

        private void MainSubFAB_Click(object sender, EventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                switch (fab.Id)
                {
                    case Resource.Id.SideLinkNamuWikiFAB:
                        string uri = "https://namu.wiki/w/" + DollName + "(소녀전선)";
                        var intent = new Intent(this, typeof(WebBrowserActivity));
                        intent.PutExtra("url", uri);
                        StartActivity(intent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.SideLinkInvenFAB:
                        string uri2 = "http://gf.inven.co.kr/dataninfo/dolls/detail.php?d=126&c=" + DollDicNum;
                        var intent2 = new Intent(this, typeof(WebBrowserActivity));
                        intent2.PutExtra("url", uri2);
                        StartActivity(intent2);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.SideLinkBaseFAB:
                        string uri3 = "https://girlsfrontline.kr/doll/" + DollDicNum;
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
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "FAB 작동 실패!", Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void DollDBDetailSmallImage_Click(object sender, EventArgs e)
        {
            try
            {
                var DollImageViewer = new Intent(this, typeof(DollDBImageViewer));
                DollImageViewer.PutExtra("Data", DollName + ";" + ModIndex);
                StartActivity(DollImageViewer);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "이미지 뷰어 실행 실패!", Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task InitLoadProcess()
        {
            InitLoadProgressBar.Visibility = ViewStates.Visible;

            await Task.Delay(100);

            try
            {
                // 인형 타이틀 바 초기화

                if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false) == true)
                {
                    if (File.Exists(Path.Combine(ETC.CachePath, "Doll", "Normal", DollDicNum + ".gfdcache")) == false)
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

                string FileName = DollDicNum.ToString();
                if (ModIndex == 3) FileName += "_M";

                if (File.Exists(Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", FileName + ".gfdcache")) == false)
                {
                    using (WebClient wc = new WebClient())
                    {
                        await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", FileName + ".png"), Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", FileName + ".gfdcache"));
                    }
                }

                ImageView DollSmallImage = FindViewById<ImageView>(Resource.Id.DollDBDetailSmallImage);
                DollSmallImage.SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", FileName + ".gfdcache")));

                FindViewById<TextView>(Resource.Id.DollDBDetailDollName).Text = DollName;
                FindViewById<TextView>(Resource.Id.DollDBDetailDollDicNumber).Text = "No. " + DollDicNum;
                FindViewById<TextView>(Resource.Id.DollDBDetailDollProductTime).Text = ETC.CalcTime((int)DollInfoDR["ProductTime"]);

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
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoNickName).Text = "";
                if (DollInfoDR["Illustrator"] == DBNull.Value) FindViewById<TextView>(Resource.Id.DollDBDetailInfoIllustrator).Text = "";
                else FindViewById<TextView>(Resource.Id.DollDBDetailInfoIllustrator).Text = (string)DollInfoDR["Illustrator"];
                if (DollInfoDR["VoiceActor"] == DBNull.Value) FindViewById<TextView>(Resource.Id.DollDBDetailInfoVoiceActor).Text = "";
                else FindViewById<TextView>(Resource.Id.DollDBDetailInfoVoiceActor).Text = (string)DollInfoDR["VoiceActor"];
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoRealModel).Text = (string)DollInfoDR["Model"];
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoCountry).Text = (string)DollInfoDR["Country"];
                FindViewById<TextView>(Resource.Id.DollDBDetailInfoHowToGain).Text = (string)DollInfoDR["DropEvent"];


                // 인형 버프 정보 초기화

                int[] BuffIds = { Resource.Id.DollDBDetailBuff1, Resource.Id.DollDBDetailBuff2, Resource.Id.DollDBDetailBuff3, Resource.Id.DollDBDetailBuff4, Resource.Id.DollDBDetailBuff5, Resource.Id.DollDBDetailBuff6, Resource.Id.DollDBDetailBuff7, Resource.Id.DollDBDetailBuff8, Resource.Id.DollDBDetailBuff9 };

                string[] Buff_Data = ((string)DollInfoDR["EffectFormation"]).Split(',');

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

                if (BuffType.Length == 1) FindViewById<RelativeLayout>(Resource.Id.DollDBDetailBuffLayout2).Visibility = ViewStates.Gone;
                else FindViewById<RelativeLayout>(Resource.Id.DollDBDetailBuffLayout2).Visibility = ViewStates.Visible;

                for (int i = 0; i < BuffType.Length; ++i)
                {
                    int id = 0;
                    string name = "";

                    switch (BuffType[i])
                    {
                        case "AC":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.AC_Icon_WhiteTheme;
                            else id = Resource.Drawable.AC_Icon;
                            name = "명중";
                            break;
                        case "AM":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.AM_Icon_WhiteTheme;
                            else id = Resource.Drawable.AM_Icon;
                            name = "장갑";
                            break;
                        case "AS":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.AS_Icon_WhiteTheme;
                            else id = Resource.Drawable.AS_Icon;
                            name = "공속";
                            break;
                        case "CR":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.CR_Icon_WhiteTheme;
                            else id = Resource.Drawable.CR_Icon;
                            name = "치명타";
                            break;
                        case "EV":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.EV_Icon_WhiteTheme;
                            else id = Resource.Drawable.EV_Icon;
                            name = "회피";
                            break;
                        case "FR":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.FR_Icon_WhiteTheme;
                            else id = Resource.Drawable.FR_Icon;
                            name = "화력";
                            break;
                        case "CL":
                            if (ETC.UseLightTheme == true) id = Resource.Drawable.CL_Icon_WhiteTheme;
                            else id = Resource.Drawable.CL_Icon;
                            name = "쿨타임";
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
                if (EffectType == "ALL") EffectTypeView.Text = "모든 총기 적용";
                else EffectTypeView.Text = EffectType + " 적용";


                // 인형 스킬 정보 초기화

                string SkillName = (string)DollInfoDR["Skill"];
                if (File.Exists(Path.Combine(ETC.CachePath, "Doll", "Skill", SkillName + ".gfdcache")) == false)
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", SkillName + ".png"), Path.Combine(ETC.CachePath, "Doll", "Skill", SkillName + ".gfdcache"));
                    }
                }

                FindViewById<ImageView>(Resource.Id.DollDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Skill", SkillName + ".gfdcache")));

                FindViewById<TextView>(Resource.Id.DollDBDetailSkillName).Text = SkillName;

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<ImageView>(Resource.Id.DollDBDetailSkillInitCoolTimeIcon).SetImageResource(Resource.Drawable.FirstCoolTime_Icon_WhiteTheme);
                    FindViewById<ImageView>(Resource.Id.DollDBDetailSkillCoolTimeIcon).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                }

                string[] S_Effect = ((string)DollInfoDR["SkillEffect"]).Split(';');
                string[] S_Mag = ((string)DollInfoDR["SkillMag"]).Split(',');

                TextView SkillInitCoolTime = FindViewById<TextView>(Resource.Id.DollDBDetailSkillInitCoolTime);
                SkillInitCoolTime.SetTextColor(Android.Graphics.Color.Orange);
                SkillInitCoolTime.Text = S_Mag[0];
                TextView SkillCoolTime = FindViewById<TextView>(Resource.Id.DollDBDetailSkillCoolTime);
                SkillCoolTime.SetTextColor(Android.Graphics.Color.DarkOrange);
                SkillCoolTime.Text = S_Mag[1];

                FindViewById<TextView>(Resource.Id.DollDBDetailSkillExplain).Text = (string)DollInfoDR["SkillExplain"];

                string[] SkillAbilities = ((string)DollInfoDR["SkillEffect"]).Split(';');
                string[] SkillMags = ((string)DollInfoDR["SkillMag"]).Split(',');

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
                    if (File.Exists(Path.Combine(ETC.CachePath, "Doll", "Skill", MSkillName + ".gfdcache")) == false)
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFile(Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", MSkillName + ".png"), Path.Combine(ETC.CachePath, "Doll", "Skill", MSkillName + ".gfdcache"));
                        }
                    }

                    FindViewById<ImageView>(Resource.Id.DollDBDetailModSkillIcon).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Skill", MSkillName + ".gfdcache")));

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

                    int MaxValue = 0;

                    switch (ModIndex)
                    {
                        case 0:
                            MaxValue = Int32.Parse((((string)DollInfoDR[abilities[i]]).Split(';')[0].Split('/'))[1]);
                            break;
                        case 1:
                        case 2:
                        case 3:
                            MaxValue = Int32.Parse((((string)DollInfoDR[abilities[i]]).Split(';'))[ModIndex]);
                            break;
                    }

                    ETC.UpProgressBarProgress(FindViewById<ProgressBar>(Progresses[i]), 0, MaxValue, delay);
                    FindViewById<TextView>(StatusTexts[i]).Text = ((string)DollInfoDR[abilities[i]]).Split('/')[0] + "/" + MaxValue + " (" + AbilityGrade[i] + ")";
                }

                if ((DollType == "MG") || (DollType == "SG"))
                {
                    FindViewById<LinearLayout>(Resource.Id.DollInfoBulletLayout).Visibility = ViewStates.Visible;
                    FindViewById<LinearLayout>(Resource.Id.DollInfoReloadLayout).Visibility = ViewStates.Visible;

                    double ReloadTime = CalcReloadTime(DollInfoDR, DollType);
                    int Bullet = (int)DollInfoDR["Bullet"];
                    FindViewById<TextView>(Resource.Id.DollInfoBulletProgressMax).Text = FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress).Max.ToString();
                    ETC.UpProgressBarProgress(FindViewById<ProgressBar>(Resource.Id.DollInfoBulletProgress), 0, Bullet, delay);
                    FindViewById<TextView>(Resource.Id.DollInfoBulletStatus).Text = Bullet.ToString();
                    FindViewById<TextView>(Resource.Id.DollInfoReloadStatus).Text = ReloadTime.ToString() + " 초";
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
                    ETC.UpProgressBarProgress(FindViewById<ProgressBar>(Resource.Id.DollInfoAMProgress), 0, Int32.Parse((((string)DollInfoDR["Armor"]).Split('/'))[1]), delay);
                    FindViewById<TextView>(Resource.Id.DollInfoAMStatus).Text = (string)DollInfoDR["Armor"] + " (" + AbilityGrade[6] + ")";
                }
                else FindViewById<LinearLayout>(Resource.Id.DollInfoAMLayout).Visibility = ViewStates.Gone;


                if (ETC.UseLightTheme == true) SetCardTheme();
                if ((bool)DollInfoDR["HasVoice"] == true) FindViewById<LinearLayout>(Resource.Id.DollDBDetailVoiceLayout).Visibility = ViewStates.Visible;
                if ((bool)DollInfoDR["HasMod"] == true) FindViewById<LinearLayout>(Resource.Id.DollDBDetailModSelectLayout).Visibility = ViewStates.Visible;

                ShowCardViewAnimation();

                LoadAD();
            }
            catch (WebException ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.RetryLoad_CauseNetwork, Snackbar.LengthShort, Android.Graphics.Color.DarkMagenta);
                InitLoadProcess();
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

        private async Task LoadAD()
        {
            await Task.Delay(100);

            adview.LoadAd(new AdRequest.Builder().Build());
        }

        private void SetCardTheme()
        {
            int[] CardViewIds = { Resource.Id.DollDBDetailBasicInfoCardLayout, Resource.Id.DollDBDetailBuffCardLayout, Resource.Id.DollDBDetailSkillCardLayout, Resource.Id.DollDBDetailModSkillCardLayout, Resource.Id.DollDBDetailAbilityCardLayout };

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

                InitLoadProcess();
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
    }
}