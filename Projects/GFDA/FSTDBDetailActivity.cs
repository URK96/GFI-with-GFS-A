using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.CardView.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.Transitions;

using Google.Android.Material.Snackbar;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFDA
{
    [Activity(Label = "", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FSTDBDetailActivity : BaseAppCompatActivity
    {
        private DataRow fstInfoDR;
        private FST fst;

        private CoordinatorLayout snackbarLayout;

        private SwipeRefreshLayout refreshMainLayout;
        private AndroidX.AppCompat.Widget.Toolbar toolbar;
        private RatingBar gradeControl;
        private RatingBar versionGradeControl;
        private Spinner chipsetBonusSelector;

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
                SetContentView(Resource.Layout.FSTDBDetailLayout);

                fst = new FST(ETC.FindDataRow(ETC.fstList, "CodeName", Intent.GetStringExtra("Keyword")));

                refreshMainLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.FSTDBDetailMainRefreshLayout);
                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.FSTDBDetailMainToolbar);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                FindViewById<TextView>(Resource.Id.FSTDBDetailToolbarType).Text = fst.Type switch
                {
                    "ATW" => ETC.Resources.GetString(Resource.String.FSTDBDetail_Type_ATW),
                    "AGL" => ETC.Resources.GetString(Resource.String.FSTDBDetail_Type_AGL),
                    "MTR" => ETC.Resources.GetString(Resource.String.FSTDBDetail_Type_MTR),
                    _ => ""
                };
                FindViewById<TextView>(Resource.Id.FSTDBDetailToolbarName).Text = $"{fst.Name} - {fst.CodeName}";

                refreshMainLayout.Refresh += async delegate { await InitLoadProcess(true); };

                gradeControl = FindViewById<RatingBar>(Resource.Id.FSTDBDetailGradeControl1);
                gradeControl.Rating = 1;
                gradeControl.RatingBarChange += FSTDBDetailActivity_RatingBarChange;
                versionGradeControl = FindViewById<RatingBar>(Resource.Id.FSTDBDetailGradeControl2);
                versionGradeControl.Rating = 0;
                versionGradeControl.RatingBarChange += delegate { CalcAbility(); };

                chipsetBonusSelector = FindViewById<Spinner>(Resource.Id.FSTDBDetailChipsetBonusSelector);
                chipsetBonusSelector.ItemSelected += delegate { CalcAbility(); };

                SetChipsetBonusList();

                //FindViewById<ImageView>(Resource.Id.FSTDBDetailSmallImage).Click += DollDBDetailSmallImage_Click;

                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FSTDBDetailSnackbarLayout);

                _ = InitLoadProcess(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
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
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Cannot execute option menu", ToastLength.Short).Show();
            }

            return base.OnOptionsItemSelected(item);
        }

        private void SetChipsetBonusList()
        {
            try
            {
                int gradeSetting = Convert.ToInt32(gradeControl.Rating);

                var list = new List<string>
                {
                    Resources.GetString(Resource.String.FSTDBDetail_Chipset_Default)
                };

                foreach (int i in fst.ChipsetBonusCount)
                {
                    list.Add(i.ToString());
                }

                list.TrimExcess();

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, list);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                chipsetBonusSelector.Adapter = adapter;

                chipsetBonusSelector.SetSelection(0);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.List_InitError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void FSTDBDetailActivity_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            try
            {
                if (e.Rating >= 5)
                {
                    FindViewById<LinearLayout>(Resource.Id.FSTDBDetailGradeControlLayout2).Visibility = ViewStates.Visible;
                }
                else
                {
                    FindViewById<LinearLayout>(Resource.Id.FSTDBDetailGradeControlLayout2).Visibility = ViewStates.Gone;
                    versionGradeControl.Rating = 0;
                }

                SetChipsetBonusList();
                SetCircuit();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.FSTDBDetail_SubGradeError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        /*private void MainSubFAB_Click(object sender, EventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                switch (fab.Id)
                {
                    case Resource.Id.SideLinkFAB1:
                        string uri = string.Format("https://namu.wiki/w/{0}(소녀전선)", fst.Name);
                        var intent = new Intent(this, typeof(WebBrowserActivity));
                        intent.PutExtra("url", uri);
                        StartActivity(intent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.SideLinkFAB2:
                        string uri2 = string.Format("http://gf.inven.co.kr/dataninfo/dolls/detail.php?d=126&c={0}", FSTDicNum);
                        var intent2 = new Intent(this, typeof(WebBrowserActivity));
                        intent2.PutExtra("url", uri2);
                        StartActivity(intent2);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.SideLinkFAB3:
                        string uri3 = string.Format("https://girlsfrontline.kr/doll/{0}", FSTDicNum);
                        var intent3 = new Intent(this, typeof(WebBrowserActivity));
                        intent3.PutExtra("url", uri3);
                        StartActivity(intent3);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }*/

        // Not Use Now
        private void FSTDBDetailSmallImage_Click(object sender, EventArgs e)
        {
            try
            {
                var fstImageViewer = new Intent(this, typeof(DollDBImageViewer));
                fstImageViewer.PutExtra("Data", fst.Name);
                StartActivity(fstImageViewer);
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

                // 중화기 타이틀 바 초기화

                /*if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false) == true)
                {
                    try
                    {
                        if ((File.Exists(Path.Combine(ETC.CachePath, "Doll", "Normal", FSTDicNum + ".gfdcache")) == false) || (IsRefresh == true))
                        {
                            using (TimeOutWebClient wc = new TimeOutWebClient())
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
                        ETC.LogError(ex, this);
                    }
                }*/

                try
                {
                    var smallImage = FindViewById<ImageView>(Resource.Id.FSTDBDetailSmallImage);
                    string cropimagePath = Path.Combine(ETC.cachePath, "FST", "Normal_Crop", $"{fst.CodeName}.gfdcache");

                    if (!File.Exists(cropimagePath) || isRefresh)
                    {
                        using (var wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.server, "Data", "Images", "FST", "Normal_Crop", $"{fst.CodeName}.png"), cropimagePath);
                        }
                    }

                    var dm = ApplicationContext.Resources.DisplayMetrics;

                    int width = dm.WidthPixels;
                    int height = dm.HeightPixels;

                    var drawable = Drawable.CreateFromPath(cropimagePath);
                    smallImage.SetImageDrawable(drawable);
                    var param = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, drawable.IntrinsicHeight * (width / drawable.IntrinsicWidth));
                    smallImage.LayoutParameters = param;
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                /*FindViewById<TextView>(Resource.Id.FSTDBDetailFSTName).Text = FSTName;
                FindViewById<TextView>(Resource.Id.FSTDBDetailFSTDicNumber).Text = "No. " + 1;
                FindViewById<TextView>(Resource.Id.FSTDBDetailFSTProductTime).Text = "제조 불가";*/

                // 중화기 기본 정보 초기화

                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoType).Text = fst.Type;
                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoName).Text = fst.Name;
                //FindViewById<TextView>(Resource.Id.FSTDBDetailInfoNickName).Text = fst.NickName;
                //FindViewById<TextView>(Resource.Id.FSTDBDetailInfoIllustrator).Text = fst.Illustrator;
                //FindViewById<TextView>(Resource.Id.FSTDBDetailInfoVoiceActor).Text = fst.VoiceActor;
                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoRealModel).Text = fst.RealModel;
                FindViewById<TextView>(Resource.Id.FSTDBDetailInfoCountry).Text = fst.Country;
                //FindViewById<TextView>(Resource.Id.FSTDBDetailInfoHowToGain).Text = "정보 없음"; //(string)FSTInfoDR["DropEvent"];


                // 중화기 회로 정보 초기화

                SetCircuit();


                // 중화기 스킬 정보 초기화

                int[] skillIconIds = { Resource.Id.FSTDBDetailSkillIcon1, Resource.Id.FSTDBDetailSkillIcon2, Resource.Id.FSTDBDetailSkillIcon3 };
                int[] skillNameIds = { Resource.Id.FSTDBDetailSkillName1, Resource.Id.FSTDBDetailSkillName2, Resource.Id.FSTDBDetailSkillName3 };
                int[] skillInitCoolTimeIconIds = { Resource.Id.FSTDBDetailSkillInitCoolTimeIcon1, Resource.Id.FSTDBDetailSkillInitCoolTimeIcon2, Resource.Id.FSTDBDetailSkillInitCoolTimeIcon3 };
                int[] skillInitCoolTimeIds = { Resource.Id.FSTDBDetailSkillInitCoolTime1, Resource.Id.FSTDBDetailSkillInitCoolTime2, Resource.Id.FSTDBDetailSkillInitCoolTime3 };
                int[] skillCoolTimeIconIds = { Resource.Id.FSTDBDetailSkillCoolTimeIcon1, Resource.Id.FSTDBDetailSkillCoolTimeIcon2, Resource.Id.FSTDBDetailSkillCoolTimeIcon3 };
                int[] skillCoolTimeIds = { Resource.Id.FSTDBDetailSkillCoolTime1, Resource.Id.FSTDBDetailSkillCoolTime2, Resource.Id.FSTDBDetailSkillCoolTime3 };
                int[] skillExplainIds = { Resource.Id.FSTDBDetailSkillExplain1, Resource.Id.FSTDBDetailSkillExplain2, Resource.Id.FSTDBDetailSkillExplain3 };
                int[] skillTableSubLayoutIds = { Resource.Id.FSTDBDetailSkillAbilitySubLayout1, Resource.Id.FSTDBDetailSkillAbilitySubLayout2, Resource.Id.FSTDBDetailSkillAbilitySubLayout3 };
                int[] skillAbilityTopLayoutIds = { Resource.Id.FSTDBDetailSkillAbilityTopLayout1, Resource.Id.FSTDBDetailSkillAbilityTopLayout2, Resource.Id.FSTDBDetailSkillAbilityTopLayout3 };
                int[] skillAbilityTopTextIds1 = { Resource.Id.FSTDBDetailSkillAbilityTopText11, Resource.Id.FSTDBDetailSkillAbilityTopText21, Resource.Id.FSTDBDetailSkillAbilityTopText31 };
                int[] skillAbilityTopTextIds2 = { Resource.Id.FSTDBDetailSkillAbilityTopText12, Resource.Id.FSTDBDetailSkillAbilityTopText22, Resource.Id.FSTDBDetailSkillAbilityTopText32 };


                for (int i = 0; i < 3; ++i)
                {
                    string skillName = fst.SkillName[i];

                    try
                    {
                        string skillIconPath = Path.Combine(ETC.cachePath, "Skill", $"{skillName}.gfdcache");

                        if (!File.Exists(skillIconPath) || isRefresh)
                        {
                            using (var wc = new WebClient())
                            {
                                wc.DownloadFile(Path.Combine(ETC.server, "Data", "Images", "SkillIcons", "FST", skillName + ".png"), skillIconPath);
                            }
                        }

                        FindViewById<ImageView>(skillIconIds[i]).SetImageDrawable(Drawable.CreateFromPath(skillIconPath));
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex, this);
                    }

                    FindViewById<TextView>(skillNameIds[i]).Text = skillName;

                    if (ETC.useLightTheme)
                    { 
                        FindViewById<ImageView>(skillInitCoolTimeIconIds[i]).SetImageResource(Resource.Drawable.FirstCoolTime_Icon_WhiteTheme);
                        FindViewById<ImageView>(skillCoolTimeIconIds[i]).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                    }

                    string[] skillEffects = fst.SkillEffect[i];
                    string[] skillMags = fst.SkillMag[i];

                    TextView skillInitCoolTime = FindViewById<TextView>(skillInitCoolTimeIds[i]);
                    skillInitCoolTime.SetTextColor(Android.Graphics.Color.Orange);
                    skillInitCoolTime.Text = "0"; //SkillMags[0];

                    TextView skillCoolTime = FindViewById<TextView>(skillCoolTimeIds[i]);
                    skillCoolTime.SetTextColor(Android.Graphics.Color.DarkOrange);
                    skillCoolTime.Text = "0"; //SkillMags[1];

                    FindViewById<TextView>(skillExplainIds[i]).Text = fst.SkillExplain[i];

                    LinearLayout skillTableSubLayout = FindViewById<LinearLayout>(skillTableSubLayoutIds[i]);
                    skillTableSubLayout.RemoveAllViews();

                    for (int k = 0; k < skillEffects.Length; ++k)
                    {
                        LinearLayout layout = new LinearLayout(this);
                        layout.Orientation = Orientation.Horizontal;
                        layout.LayoutParameters = FindViewById<LinearLayout>(skillAbilityTopLayoutIds[i]).LayoutParameters;

                        TextView ability = new TextView(this);
                        TextView mag = new TextView(this);

                        ability.LayoutParameters = FindViewById<TextView>(skillAbilityTopTextIds1[i]).LayoutParameters;
                        mag.LayoutParameters = FindViewById<TextView>(skillAbilityTopTextIds2[i]).LayoutParameters;
                        
                        ability.Text = skillEffects[k];
                        ability.Gravity = GravityFlags.Center;
                        mag.Text = skillMags[k];
                        mag.Gravity = GravityFlags.Center;

                        layout.AddView(ability);
                        layout.AddView(mag);

                        skillTableSubLayout.AddView(layout);
                    }
                }


                // 인형 능력치 초기화

                CalcAbility();

                if (ETC.useLightTheme)
                {
                    SetCardTheme();
                }

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
            }
            finally
            {
                refreshMainLayout.Refreshing = false;
            }
        }

        private void SetCircuit()
        {
            int[] circuitRowIds =
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

            foreach (int id in circuitRowIds)
            {
                FindViewById<LinearLayout>(id).RemoveAllViews();
            }

            int grade = Convert.ToInt32(gradeControl.Rating);

            try
            {
                View testView = FindViewById<View>(Resource.Id.FSTDBDetailCircuitRowItem);

                gradeControl.IsIndicator = true;
                versionGradeControl.IsIndicator = true;

                grade = (grade == 0) ? 1 : grade;

                int[,] rowValues = fst.ChipsetCircuit[grade - 1];

                for (int i = 0; i < fst.CircuitHeight; ++i)
                {
                    var circuitRow = FindViewById<LinearLayout>(circuitRowIds[i]);

                    for (int k = 0; k < fst.CircuitLength; ++k)
                    {
                        View view = new View(this)
                        {
                            LayoutParameters = testView.LayoutParameters,
                            Visibility = ViewStates.Visible
                        };

                        if (rowValues[i, k] == 1)
                        {
                            int colorId = fst.ChipsetType switch
                            {
                                "Orange" => Resource.Color.FST_OrangeChipset,
                                _ => Resource.Color.FST_BlueChipset,
                            };

                            view.SetBackgroundResource(colorId);
                        }
                        else
                        {
                            view.SetBackgroundColor(Android.Graphics.Color.LightGray);
                        }

                        circuitRow.AddView(view);
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.FSTDBDetail_CircuitCalcError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                gradeControl.IsIndicator = false;
                versionGradeControl.IsIndicator = false;
            }
        }

        private void CalcAbility()
        {
            int[] progresses = { Resource.Id.FSTInfoKillProgress, Resource.Id.FSTInfoCrushProgress, Resource.Id.FSTInfoACProgress, Resource.Id.FSTInfoRLProgress };
            int[] progressMaxTexts = { Resource.Id.FSTInfoKillProgressMax, Resource.Id.FSTInfoCrushProgressMax, Resource.Id.FSTInfoACProgressMax, Resource.Id.FSTInfoRLProgressMax };
            int[] statusTexts = { Resource.Id.FSTInfoKillStatus, Resource.Id.FSTInfoCrushStatus, Resource.Id.FSTInfoACStatus, Resource.Id.FSTInfoRLStatus };

            int chipsetIndex = chipsetBonusSelector.SelectedItemPosition;
            int versionIndex = Convert.ToInt32(versionGradeControl.Rating * 2);

            int[] bonusUp = { 0, 0, 0, 0 };

            try
            {
                gradeControl.IsIndicator = true;
                versionGradeControl.IsIndicator = true;

                if (chipsetIndex > 0)
                {
                    for (int i = 0; i < chipsetIndex; ++i)
                    {
                        for (int k = 0; k < fst.AbilityList.Length; ++k)
                        {
                            bonusUp[k] += fst.ChipsetBonusMag[i][fst.AbilityList[k]];
                        }
                    }
                }

                if (versionIndex > 0)
                {
                    for (int i = 0; i < versionIndex; ++i)
                    {
                        for (int k = 0; k < fst.AbilityList.Length; ++k)
                        {
                            bonusUp[k] += fst.VersionUpPlus[i][fst.AbilityList[k]];
                        }
                    }
                }

                for (int i = 0; i < progresses.Length; ++i)
                {
                    FindViewById<TextView>(progressMaxTexts[i]).Text = FindViewById<ProgressBar>(progresses[i]).Max.ToString();

                    int minValue = fst.Abilities[$"{fst.AbilityList[i]}_Min"];
                    int maxValue = fst.Abilities[$"{fst.AbilityList[i]}_Max"];

                    ProgressBar pb = FindViewById<ProgressBar>(progresses[i]);
                    pb.Progress = minValue;
                    pb.SecondaryProgress = maxValue + bonusUp[i];

                    FindViewById<TextView>(statusTexts[i]).Text = $"{minValue}/{maxValue}(+{bonusUp[i]})";
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.FSTDBDetail_AbilityCalcError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                gradeControl.IsIndicator = false;
                versionGradeControl.IsIndicator = false;
            }
        }

        private void SetCardTheme()
        {
            int[] cardViewIds = 
            { 
                Resource.Id.FSTDBDetailBasicInfoCardLayout, 
                Resource.Id.FSTDBDetailCircuitCardLayout, 
                Resource.Id.FSTDBDetailSkillCardLayout1, 
                Resource.Id.FSTDBDetailSkillCardLayout2, 
                Resource.Id.FSTDBDetailSkillCardLayout3, 
                Resource.Id.FSTDBDetailAbilityCardLayout 
            };

            foreach (int id in cardViewIds)
            {
                var cv = FindViewById<CardView>(id);

                cv.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                cv.Radius = 15.0f;
            }
        }

        private void ShowCardViewVisibility()
        {
            FindViewById<CardView>(Resource.Id.FSTDBDetailBasicInfoCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.FSTDBDetailCircuitCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.FSTDBDetailSkillCardLayout1).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.FSTDBDetailSkillCardLayout2).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.FSTDBDetailSkillCardLayout3).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.FSTDBDetailAbilityCardLayout).Visibility = ViewStates.Visible;
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