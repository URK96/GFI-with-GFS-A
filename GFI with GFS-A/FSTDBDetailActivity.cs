using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
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
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FSTDBDetailActivity : FragmentActivity
    {
        System.Timers.Timer FABTimer = new System.Timers.Timer();

        private DataRow FSTInfoDR = null;
        private string FSTName;
        private int FSTDicNum;
        private int FSTGrade;
        private string FSTType;

        private bool IsOpenFABMenu = false;
        private bool IsEnableFABMenu = false;

        private ScrollView ScrollLayout;
        private CoordinatorLayout SnackbarLayout;

        private ProgressBar InitLoadProgressBar;
        private RatingBar GradeControl;
        private RatingBar VersionGradeControl;
        private Spinner ChipsetBonusSelector;
        private FloatingActionButton RefreshCacheFAB;
        private FloatingActionButton MainFAB;
        private FloatingActionButton NamuWikiFAB;
        private FloatingActionButton InvenFAB;
        private FloatingActionButton BaseFAB;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.FSTDBDetailLayout);

                FSTName = Intent.GetStringExtra("Keyword");

                FSTInfoDR = ETC.FindDataRow(ETC.FSTList, "Name", FSTName);
                //FSTDicNum = (int)FSTInfoDR["DicNumber"];
                FSTType = (string)FSTInfoDR["Type"];

                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.FSTDBDetailInitLoadProgress);
                GradeControl = FindViewById<RatingBar>(Resource.Id.FSTDBDetailGradeControl1);
                GradeControl.Rating = 1;
                GradeControl.RatingBarChange += FSTDBDetailActivity_RatingBarChange;
                VersionGradeControl = FindViewById<RatingBar>(Resource.Id.FSTDBDetailGradeControl2);
                VersionGradeControl.Rating = 0;
                VersionGradeControl.RatingBarChange += delegate { CalcAbility(); };

                ChipsetBonusSelector = FindViewById<Spinner>(Resource.Id.FSTDBDetailChipsetBonusSelector);
                ChipsetBonusSelector.ItemSelected += delegate { CalcAbility(); };

                SetChipsetBonusList();

                //FindViewById<ImageView>(Resource.Id.FSTDBDetailSmallImage).Click += DollDBDetailSmallImage_Click;

                ScrollLayout = FindViewById<ScrollView>(Resource.Id.FSTDBDetailScrollLayout);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FSTDBDetailSnackbarLayout);

                RefreshCacheFAB = FindViewById<FloatingActionButton>(Resource.Id.FSTDBDetailRefreshCacheFAB);
                MainFAB = FindViewById<FloatingActionButton>(Resource.Id.FSTDBDetailSideLinkMainFAB);
                NamuWikiFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkNamuWikiFAB);
                InvenFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkInvenFAB);
                BaseFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkBaseFAB);

                RefreshCacheFAB.Click += RefreshCacheFAB_Click;
                MainFAB.Click += MainFAB_Click;
                NamuWikiFAB.Click += MainSubFAB_Click;
                InvenFAB.Click += MainSubFAB_Click;
                BaseFAB.Click += MainSubFAB_Click;

                FABTimer.Interval = 3000;
                FABTimer.Elapsed += FABTimer_Elapsed;

                InitLoadProcess(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void SetChipsetBonusList()
        {
            try
            {
                int GradeSetting = Convert.ToInt32(GradeControl.Rating);

                List<string> list = new List<string>
                {
                    Resources.GetString(Resource.String.FSTDBDetail_Chipset_Default)
                };
                
                if (GradeSetting > 0)
                {
                    string[] temp = ((string)FSTInfoDR["ChipsetBonusCount"]).Split(',');

                    for (int i = 0; i < GradeSetting; ++i) list.Add(temp[i]);
                }

                list.TrimExcess();

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, list);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                ChipsetBonusSelector.Adapter = adapter;

                ChipsetBonusSelector.SetSelection(0);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.List_InitError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void FSTDBDetailActivity_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            try
            {
                if (e.Rating >= 5) FindViewById<LinearLayout>(Resource.Id.FSTDBDetailGradeControlLayout2).Visibility = ViewStates.Visible;
                else
                {
                    FindViewById<LinearLayout>(Resource.Id.FSTDBDetailGradeControlLayout2).Visibility = ViewStates.Gone;
                    VersionGradeControl.Rating = 0;
                }

                SetChipsetBonusList();
                SetCircuit();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FSTDBDetail_SubGradeError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void RefreshCacheFAB_Click(object sender, EventArgs e)
        {
            InitLoadProcess(true);
        }

        private void FABTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HideFloatingActionButtonAnimation();
        }

        private void MainSubFAB_Click(object sender, EventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                switch (fab.Id)
                {
                    case Resource.Id.SideLinkNamuWikiFAB:
                        string uri = string.Format("https://namu.wiki/w/{0}(소녀전선)", FSTName);
                        var intent = new Intent(this, typeof(WebBrowserActivity));
                        intent.PutExtra("url", uri);
                        StartActivity(intent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.SideLinkInvenFAB:
                        /*string uri2 = string.Format("http://gf.inven.co.kr/dataninfo/dolls/detail.php?d=126&c={0}", FSTDicNum);
                        var intent2 = new Intent(this, typeof(WebBrowserActivity));
                        intent2.PutExtra("url", uri2);
                        StartActivity(intent2);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);*/
                        break;
                    case Resource.Id.SideLinkBaseFAB:
                        /*string uri3 = string.Format("https://girlsfrontline.kr/doll/{0}", FSTDicNum);
                        var intent3 = new Intent(this, typeof(WebBrowserActivity));
                        intent3.PutExtra("url", uri3);
                        StartActivity(intent3);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);*/
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
                IsEnableFABMenu = true;
                MainFAB.Animate().Alpha(1.0f).SetDuration(500).Start();
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
                var FSTImageViewer = new Intent(this, typeof(DollDBImageViewer));
                FSTImageViewer.PutExtra("Data", FSTName);
                StartActivity(FSTImageViewer);
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
                // 중화기 타이틀 바 초기화

                /*if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false) == true)
                {
                    try
                    {
                        if ((File.Exists(Path.Combine(ETC.CachePath, "Doll", "Normal", FSTDicNum + ".gfdcache")) == false) || (IsRefresh == true))
                        {
                            using (WebClient wc = new WebClient())
                            {
                                await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", FSTDicNum + ".png"), Path.Combine(ETC.CachePath, "Doll", "Normal", FSTDicNum + ".gfdcache"));
                            }
                        }

                        Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Normal", FSTDicNum + ".gfdcache"));
                        drawable.SetAlpha(40);
                        FindViewById<RelativeLayout>(Resource.Id.DollDBDetailMainLayout).Background = drawable;
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(this, ex.ToString());
                    }
                }*/

                try
                {
                    if ((File.Exists(Path.Combine(ETC.CachePath, "FST", "Normal_Crop", FSTName + ".gfdcache")) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "FST", "Normal_Crop", FSTName + ".png"), Path.Combine(ETC.CachePath, "FST", "Normal_Crop", FSTName + ".gfdcache"));
                        }
                    }

                    ImageView FSTSmallImage = FindViewById<ImageView>(Resource.Id.FSTDBDetailSmallImage);
                    FSTSmallImage.SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "FST", "Normal_Crop", FSTName + ".gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }

                /*FindViewById<TextView>(Resource.Id.FSTDBDetailFSTName).Text = FSTName;
                FindViewById<TextView>(Resource.Id.FSTDBDetailFSTDicNumber).Text = "No. " + 1;
                FindViewById<TextView>(Resource.Id.FSTDBDetailFSTProductTime).Text = "제조 불가";*/

                // 인형 기본 정보 초기화

                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoType).Text = (string)FSTInfoDR["Type"];
                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoName).Text = FSTName;
                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoNickName).Text = "";
                if (FSTInfoDR["Illustrator"] == DBNull.Value) FindViewById<TextView>(Resource.Id.FSTDBDetailInfoIllustrator).Text = "";
                else FindViewById<TextView>(Resource.Id.FSTDBDetailInfoIllustrator).Text = (string)FSTInfoDR["Illustrator"];
                if (FSTInfoDR["VoiceActor"] == DBNull.Value) FindViewById<TextView>(Resource.Id.FSTDBDetailInfoVoiceActor).Text = "";
                else FindViewById<TextView>(Resource.Id.FSTDBDetailInfoVoiceActor).Text = (string)FSTInfoDR["VoiceActor"];
                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoRealModel).Text = (string)FSTInfoDR["Model"];
                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoCountry).Text = (string)FSTInfoDR["Country"];
                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoHowToGain).Text = "정보 없음"; //(string)FSTInfoDR["DropEvent"];


                // 중화기 회로 정보 초기화

                SetCircuit();


                // 중화기 스킬 정보 초기화

                int[] SkillIconIds = { Resource.Id.FSTDBDetailSkillIcon1, Resource.Id.FSTDBDetailSkillIcon2, Resource.Id.FSTDBDetailSkillIcon3 };
                int[] SkillNameIds = { Resource.Id.FSTDBDetailSkillName1, Resource.Id.FSTDBDetailSkillName2, Resource.Id.FSTDBDetailSkillName3 };
                int[] SkillInitCoolTimeIconIds = { Resource.Id.FSTDBDetailSkillInitCoolTimeIcon1, Resource.Id.FSTDBDetailSkillInitCoolTimeIcon2, Resource.Id.FSTDBDetailSkillInitCoolTimeIcon3 };
                int[] SkillInitCoolTimeIds = { Resource.Id.FSTDBDetailSkillInitCoolTime1, Resource.Id.FSTDBDetailSkillInitCoolTime2, Resource.Id.FSTDBDetailSkillInitCoolTime3 };
                int[] SkillCoolTimeIconIds = { Resource.Id.FSTDBDetailSkillCoolTimeIcon1, Resource.Id.FSTDBDetailSkillCoolTimeIcon2, Resource.Id.FSTDBDetailSkillCoolTimeIcon3 };
                int[] SkillCoolTimeIds = { Resource.Id.FSTDBDetailSkillCoolTime1, Resource.Id.FSTDBDetailSkillCoolTime2, Resource.Id.FSTDBDetailSkillCoolTime3 };
                int[] SkillExplainIds = { Resource.Id.FSTDBDetailSkillExplain1, Resource.Id.FSTDBDetailSkillExplain2, Resource.Id.FSTDBDetailSkillExplain3 };
                int[] SkillTableSubLayoutIds = { Resource.Id.FSTDBDetailSkillAbilitySubLayout1, Resource.Id.FSTDBDetailSkillAbilitySubLayout2, Resource.Id.FSTDBDetailSkillAbilitySubLayout3 };
                int[] SkillAbilityTopLayoutIds = { Resource.Id.FSTDBDetailSkillAbilityTopLayout1, Resource.Id.FSTDBDetailSkillAbilityTopLayout2, Resource.Id.FSTDBDetailSkillAbilityTopLayout3 };
                int[] SkillAbilityTopTextIds_1 = { Resource.Id.FSTDBDetailSkillAbilityTopText11, Resource.Id.FSTDBDetailSkillAbilityTopText21, Resource.Id.FSTDBDetailSkillAbilityTopText31 };
                int[] SkillAbilityTopTextIds_2 = { Resource.Id.FSTDBDetailSkillAbilityTopText12, Resource.Id.FSTDBDetailSkillAbilityTopText22, Resource.Id.FSTDBDetailSkillAbilityTopText32 };


                for (int i = 0; i < 3; ++i)
                {
                    string SkillName = (string)FSTInfoDR[string.Format("SkillName{0}", (i + 1))];

                    try
                    {
                        if ((File.Exists(Path.Combine(ETC.CachePath, "FST", "Skill", SkillName + ".gfdcache")) == false) || (IsRefresh == true))
                        {

                            using (WebClient wc = new WebClient())
                            {
                                wc.DownloadFile(Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", "FST", SkillName + ".png"), Path.Combine(ETC.CachePath, "FST", "Skill", SkillName + ".gfdcache"));
                            }

                        }
                        FindViewById<ImageView>(SkillIconIds[i]).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "FST", "Skill", SkillName + ".gfdcache")));
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(this, ex.ToString());
                    }

                    FindViewById<TextView>(SkillNameIds[i]).Text = SkillName;

                    if (ETC.UseLightTheme == true)
                    {
                        FindViewById<ImageView>(SkillInitCoolTimeIconIds[i]).SetImageResource(Resource.Drawable.FirstCoolTime_Icon_WhiteTheme);
                        FindViewById<ImageView>(SkillCoolTimeIconIds[i]).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                    }

                    string[] SkillAbilities = ((string)FSTInfoDR[string.Format("SkillEffect{0}", (i + 1))]).Split(';');
                    string[] SkillMags = ((string)FSTInfoDR[string.Format("SkillMag{0}", (i + 1))]).Split(',');

                    TextView SkillInitCoolTime = FindViewById<TextView>(SkillInitCoolTimeIds[i]);
                    SkillInitCoolTime.SetTextColor(Android.Graphics.Color.Orange);
                    SkillInitCoolTime.Text = "0"; //SkillMags[0];
                    TextView SkillCoolTime = FindViewById<TextView>(SkillCoolTimeIds[i]);
                    SkillCoolTime.SetTextColor(Android.Graphics.Color.DarkOrange);
                    SkillCoolTime.Text = "0"; //SkillMags[1];

                    FindViewById<TextView>(SkillExplainIds[i]).Text = (string)FSTInfoDR[string.Format("SkillExplain{0}", (i + 1))];

                    LinearLayout SkillTableSubLayout = FindViewById<LinearLayout>(SkillTableSubLayoutIds[i]);
                    SkillTableSubLayout.RemoveAllViews();

                    for (int k = 0; k < SkillAbilities.Length; ++k)
                    {
                        LinearLayout layout = new LinearLayout(this);
                        layout.Orientation = Orientation.Horizontal;
                        layout.LayoutParameters = FindViewById<LinearLayout>(SkillAbilityTopLayoutIds[i]).LayoutParameters;

                        TextView ability = new TextView(this);
                        TextView mag = new TextView(this);

                        ability.LayoutParameters = FindViewById<TextView>(SkillAbilityTopTextIds_1[i]).LayoutParameters;
                        mag.LayoutParameters = FindViewById<TextView>(SkillAbilityTopTextIds_2[i]).LayoutParameters;
                        
                        ability.Text = SkillAbilities[k];
                        ability.Gravity = GravityFlags.Center;
                        mag.Text = SkillMags[k];
                        mag.Gravity = GravityFlags.Center;

                        layout.AddView(ability);
                        layout.AddView(mag);

                        SkillTableSubLayout.AddView(layout);
                    }
                }


                // 인형 능력치 초기화

                CalcAbility();

                if (ETC.UseLightTheme == true) SetCardTheme();

                ShowCardViewAnimation();
                HideFloatingActionButtonAnimation();
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

        private void SetCircuit()
        {
            int[] CircuitRowIds =
            {
                Resource.Id.FSTDBDetailCircuitRow1,
                Resource.Id.FSTDBDetailCircuitRow2,
                Resource.Id.FSTDBDetailCircuitRow3,
                Resource.Id.FSTDBDetailCircuitRow4,
                Resource.Id.FSTDBDetailCircuitRow5,
                Resource.Id.FSTDBDetailCircuitRow6,
                Resource.Id.FSTDBDetailCircuitRow7,
                Resource.Id.FSTDBDetailCircuitRow8
            };

            foreach (int id in CircuitRowIds) FindViewById<LinearLayout>(id).RemoveAllViews();

            int Grade = Convert.ToInt32(GradeControl.Rating);

            try
            {
                View testView = FindViewById<View>(Resource.Id.FSTDBDetailCircuitRowItem);

                GradeControl.IsIndicator = true;
                VersionGradeControl.IsIndicator = true;

                if (Grade == 0) Grade = 1;

                string[] row_values = ((string)FSTInfoDR[string.Format("Circuit{0}", Grade)]).Split(';');

                for (int i = 0; i < row_values.Length; ++i)
                {
                    string[] values = row_values[i].Split(',');

                    if (values.Length > 1)
                    {
                        int count = 0;

                        for (int k = 1; k <= 8; ++k)
                        {
                            View view = new View(this);
                            view.LayoutParameters = testView.LayoutParameters;

                            if ((count < values.Length) && (k == int.Parse(values[count])))
                            {
                                view.SetBackgroundColor(Android.Graphics.Color.Yellow);
                                count += 1;
                            }
                            else view.SetBackgroundColor(Android.Graphics.Color.LightGray);

                            FindViewById<LinearLayout>(CircuitRowIds[i]).AddView(view);
                        }
                    }
                    else
                    {
                        int value = int.Parse(values[0]);
                        int start = value / 10;
                        int end = value % 10;

                        for (int k = 1; k <= 8; ++k)
                        {
                            View view = new View(this);
                            view.LayoutParameters = testView.LayoutParameters;
                            view.Visibility = ViewStates.Visible;

                            if ((k >= start) && (k <= end)) view.SetBackgroundColor(Android.Graphics.Color.Yellow);
                            else view.SetBackgroundColor(Android.Graphics.Color.LightGray);

                            FindViewById<LinearLayout>(CircuitRowIds[i]).AddView(view);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FSTDBDetail_CircuitCalcError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                GradeControl.IsIndicator = false;
                VersionGradeControl.IsIndicator = false;
            }
        }

        private void CalcAbility()
        {
            string[] abilities = { "Kill", "Crush", "Accuracy", "Reload" };
            int[] Progresses = { Resource.Id.FSTInfoKillProgress, Resource.Id.FSTInfoCrushProgress, Resource.Id.FSTInfoACProgress, Resource.Id.FSTInfoRLProgress };
            int[] ProgressMaxTexts = { Resource.Id.FSTInfoKillProgressMax, Resource.Id.FSTInfoCrushProgressMax, Resource.Id.FSTInfoACProgressMax, Resource.Id.FSTInfoRLProgressMax };
            int[] StatusTexts = { Resource.Id.FSTInfoKillStatus, Resource.Id.FSTInfoCrushStatus, Resource.Id.FSTInfoACStatus, Resource.Id.FSTInfoRLStatus };

            int ChipsetIndex = ChipsetBonusSelector.SelectedItemPosition;
            int VersionIndex = Convert.ToInt32(VersionGradeControl.Rating * 2);

            int[] BonusUp = { 0, 0, 0, 0 };

            try
            {
                GradeControl.IsIndicator = true;
                VersionGradeControl.IsIndicator = true;

                if (ChipsetIndex > 0)
                {
                    string[] bonus_list = ((string)FSTInfoDR["ChipsetBonusMag"]).Split(';');

                    for (int i = 0; i < ChipsetIndex; ++i)
                    {
                        string[] values = bonus_list[i].Split(',');

                        for (int k = 0; k < values.Length; ++k) BonusUp[k] += int.Parse(values[k]);
                    }
                }

                if (VersionIndex > 0)
                {
                    string[] up_list = ((string)FSTInfoDR["VersionUp"]).Split(';');

                    for (int i = 0; i < VersionIndex; ++i)
                    {
                        string[] values = up_list[i].Split(',');

                        for (int k = 0; k < values.Length; ++k) BonusUp[k] += int.Parse(values[k]);
                    }
                }

                for (int i = 0; i < Progresses.Length; ++i)
                {
                    FindViewById<TextView>(ProgressMaxTexts[i]).Text = FindViewById<ProgressBar>(Progresses[i]).Max.ToString();

                    int MinValue = 0;

                    string temp = (((string)FSTInfoDR[abilities[i]]).Split('/'))[0];

                    if (temp.Contains("?") == true) MinValue = 0;
                    else MinValue = int.Parse(temp);

                    int MaxValue = int.Parse((((string)FSTInfoDR[abilities[i]]).Split('/'))[1]);

                    ProgressBar pb = FindViewById<ProgressBar>(Progresses[i]);
                    pb.Progress = MinValue;
                    pb.SecondaryProgress = MaxValue + BonusUp[i];

                    FindViewById<TextView>(StatusTexts[i]).Text = string.Format("{0}/{1}(+{2})", MinValue, MaxValue, BonusUp[i]);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FSTDBDetail_AbilityCalcError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                GradeControl.IsIndicator = false;
                VersionGradeControl.IsIndicator = false;
            }
        }

        private void HideFloatingActionButtonAnimation()
        {
            FABTimer.Stop();
            IsEnableFABMenu = false;

            RefreshCacheFAB.Hide();
            MainFAB.Alpha = 0.3f;
        }

        private void SetCardTheme()
        {
            int[] CardViewIds = { Resource.Id.FSTDBDetailBasicInfoCardLayout, Resource.Id.FSTDBDetailCircuitCardLayout, Resource.Id.FSTDBDetailSkillCardLayout1, Resource.Id.FSTDBDetailSkillCardLayout2, Resource.Id.FSTDBDetailSkillCardLayout3, Resource.Id.FSTDBDetailAbilityCardLayout };

            foreach (int id in CardViewIds)
            {
                CardView cv = FindViewById<CardView>(id);

                cv.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                cv.Radius = 15.0f;
            }
        }

        private async Task ShowCardViewAnimation()
        {
            if (FindViewById<CardView>(Resource.Id.FSTDBDetailBasicInfoCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.FSTDBDetailBasicInfoCardLayout).Animate().Alpha(1.0f).SetDuration(500).Start();
            if (FindViewById<CardView>(Resource.Id.FSTDBDetailCircuitCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.FSTDBDetailCircuitCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(500).Start();
            if (FindViewById<CardView>(Resource.Id.FSTDBDetailSkillCardLayout1).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.FSTDBDetailSkillCardLayout1).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(1000).Start();
            if (FindViewById<CardView>(Resource.Id.FSTDBDetailSkillCardLayout2).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.FSTDBDetailSkillCardLayout2).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(1500).Start();
            if (FindViewById<CardView>(Resource.Id.FSTDBDetailSkillCardLayout3).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.FSTDBDetailSkillCardLayout3).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(2000).Start();
            if (FindViewById<CardView>(Resource.Id.FSTDBDetailAbilityCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.FSTDBDetailAbilityCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(2500).Start();
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