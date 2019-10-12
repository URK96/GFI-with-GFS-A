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
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FairyDBDetailActivity : BaseFragmentActivity
    {
        System.Timers.Timer FABTimer = new System.Timers.Timer();

        private Fairy fairy;
        private DataRow FairyInfoDR = null;

        private bool IsOpenFABMenu = false;
        private bool IsEnableFABMenu = false;

        private ProgressBar InitLoadProgressBar;
        private FloatingActionButton RefreshCacheFAB;
        private FloatingActionButton PercentTableFAB;
        private FloatingActionButton MainFAB;
        private FloatingActionButton GFDBFAB;
        private FloatingActionButton InvenFAB;
        private FloatingActionButton BaseFAB;
        private CoordinatorLayout SnackbarLayout = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                    SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.FairyDBDetailLayout);

                FairyInfoDR = ETC.FindDataRow(ETC.FairyList, "DicNumber", Intent.GetIntExtra("DicNum", 0));
                fairy = new Fairy(FairyInfoDR);

                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.FairyDBDetailInitLoadProgress);
                FindViewById<ImageView>(Resource.Id.FairyDBDetailSmallImage).Click += FairyDBDetailSmallImage_Click;

                RefreshCacheFAB = FindViewById<FloatingActionButton>(Resource.Id.FairyDBDetailRefreshCacheFAB);
                PercentTableFAB = FindViewById<FloatingActionButton>(Resource.Id.FairyDBDetailProductPercentFAB);
                if (fairy.ProductTime == 0) PercentTableFAB.Visibility = ViewStates.Gone;
                MainFAB = FindViewById<FloatingActionButton>(Resource.Id.FairyDBDetailSideLinkMainFAB);
                GFDBFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkFAB1);
                GFDBFAB.SetImageResource(Resource.Drawable.GFDB_Logo);
                InvenFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkFAB2);
                InvenFAB.SetImageResource(Resource.Drawable.Inven_Logo);
                BaseFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkFAB3);
                BaseFAB.SetImageResource(Resource.Drawable.Base36_Logo);

                RefreshCacheFAB.Click += delegate { _ = InitLoadProcess(true); };
                PercentTableFAB.Click += PercentTableFAB_Click;
                MainFAB.Click += MainFAB_Click;
                GFDBFAB.Click += MainSubFAB_Click;
                InvenFAB.Click += MainSubFAB_Click;
                BaseFAB.Click += MainSubFAB_Click;

                RefreshCacheFAB.LongClick += DBDetailFAB_LongClick;
                PercentTableFAB.LongClick += DBDetailFAB_LongClick;
                MainFAB.LongClick += DBDetailFAB_LongClick;
                GFDBFAB.LongClick += DBDetailFAB_LongClick;
                InvenFAB.LongClick += DBDetailFAB_LongClick;
                BaseFAB.LongClick += DBDetailFAB_LongClick;

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FairyDBSnackbarLayout);

                FABTimer.Interval = 3000;
                FABTimer.Elapsed += delegate { HideFloatingActionButtonAnimation(); };

                _ = InitLoadProcess(false);

                if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_FairyDBDetail", true)))
                    ETC.RunHelpActivity(this, "FairyDBDetail");
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
                    case Resource.Id.FairyDBDetailRefreshCacheFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_CacheRefresh);
                        break;
                    case Resource.Id.FairyDBDetailProductPercentFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_ProductPercentage);
                        break;
                    case Resource.Id.FairyDBDetailSideLinkMainFAB:
                        if (IsEnableFABMenu == false) return;
                        tip = Resources.GetString(Resource.String.Tooltip_DB_SideLink);
                        break;
                    case Resource.Id.SideLinkFAB1:
                        tip = Resources.GetString(Resource.String.Tooltip_SideLink_GFDB);
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

        private void HideFloatingActionButtonAnimation()
        {
            FABTimer.Stop();
            IsEnableFABMenu = false;

            PercentTableFAB.Hide();
            RefreshCacheFAB.Hide();
            MainFAB.Alpha = 0.3f;
            MainFAB.SetImageResource(Resource.Drawable.HideFloating_Icon);
        }

        private void PercentTableFAB_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(ProductPercentTableActivity));
                intent.PutExtra("Info", new string[] { "Fairy", fairy.DicNumber.ToString() });
                StartActivity(intent);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void MainSubFAB_Click(object sender, EventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                string url = "";
                Intent intent = null;

                switch (fab.Id)
                {
                    default:
                    case Resource.Id.SideLinkFAB1:
                        url = string.Format("http://gfl.zzzzz.kr/fairy.php?id={0}&lang=ko", fairy.DicNumber);
                        intent = new Intent(this, typeof(WebBrowserActivity));               
                        break;
                    case Resource.Id.SideLinkFAB2:
                        url = string.Format("http://girlsfrontline.inven.co.kr/dataninfo/fairy/?d=133&c={0}", fairy.DicNumber);
                        intent = new Intent(this, typeof(WebBrowserActivity));
                        break;
                    case Resource.Id.SideLinkFAB3:
                        url = string.Format("https://girlsfrontline.kr/doll/{0}", fairy.DicNumber);
                        intent = new Intent(this, typeof(WebBrowserActivity));
                        break;
                }

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
            if (!IsEnableFABMenu)
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
                FloatingActionButton[] FABs = { GFDBFAB, InvenFAB, BaseFAB };
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
                    ETC.LogError(ex, this);
                    ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeSubMenuError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                }
            }
        }

        private void FairyDBDetailSmallImage_Click(object sender, EventArgs e)
        {
            try
            {
                var FairyImageViewer = new Intent(this, typeof(FairyDBImageViewer));
                FairyImageViewer.PutExtra("Keyword", fairy.Name);
                StartActivity(FairyImageViewer);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.ImageViewer_ActivityOpenError, ToastLength.Long).Show();
            }
        }

        private async Task InitLoadProcess(bool IsRefresh)
        {
            InitLoadProgressBar.Visibility = ViewStates.Visible;

            await Task.Delay(100);

            try
            {
                // 요정 타이틀 바 초기화
                try
                {
                    if ((File.Exists(Path.Combine(ETC.CachePath, "Fairy", "Normal", $"{fairy.DicNumber}_1.gfdcache")) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Fairy", $"{fairy.DicNumber}_1.png"), Path.Combine(ETC.CachePath, "Fairy", "Normal", $"{fairy.DicNumber}_1.gfdcache"));
                        }
                    }

                    if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", true) == true)
                    {
                        Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Fairy", "Normal", $"{fairy.DicNumber}_1.gfdcache"));
                        drawable.SetAlpha(40);
                        FindViewById<RelativeLayout>(Resource.Id.FairyDBDetailMainLayout).Background = drawable;
                    }

                    ImageView FairySmallImage = FindViewById<ImageView>(Resource.Id.FairyDBDetailSmallImage);
                    FairySmallImage.SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Fairy", "Normal", $"{fairy.DicNumber}_1.gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                FindViewById<TextView>(Resource.Id.FairyDBDetailFairyType).Text = fairy.Type;
                FindViewById<TextView>(Resource.Id.FairyDBDetailFairyName).Text = fairy.Name;
                FindViewById<TextView>(Resource.Id.FairyDBDetailFairyProductTime).Text = ETC.CalcTime(fairy.ProductTime);


                // 요정 기본 정보 초기화

                FindViewById<TextView>(Resource.Id.FairyDBDetailInfoType).Text = fairy.Type;
                FindViewById<TextView>(Resource.Id.FairyDBDetailInfoName).Text = fairy.Name;
                FindViewById<TextView>(Resource.Id.FairyDBDetailInfoIllustrator).Text = "";
                FindViewById<TextView>(Resource.Id.FairyDBDetailInfoETC).Text = fairy.Note;


                // 요정 스킬 정보 초기화

                try
                {
                    if ((File.Exists(Path.Combine(ETC.CachePath, "Fairy", "Skill", $"{fairy.SkillName}.gfdcache")) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "FairySkill", $"{fairy.SkillName}.png"), Path.Combine(ETC.CachePath, "Fairy", "Skill", $"{fairy.SkillName}.gfdcache"));
                        }
                    }

                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Fairy", "Skill", $"{fairy.SkillName}.gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillName).Text = fairy.SkillName;

                if (ETC.useLightTheme == true)
                {
                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillTicketIcon).SetImageResource(Resource.Drawable.FairyTicket_Icon_WhiteTheme);
                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillCoolTimeIcon).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                }

                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillTicket).Text = fairy.OrderConsume.ToString();
                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillCoolTime).Text = fairy.CoolDown.ToString();
                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillExplain).Text = fairy.SkillExplain.ToString();

                string[] effect = fairy.SkillEffect;
                string[] rate = fairy.SkillRate;

                StringBuilder sb1 = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();

                for (int i = 0; i < effect.Length; ++i)
                {
                    sb1.Append(effect[i]);
                    sb2.Append(rate[i]);

                    if (i < (effect.Length -1))
                    {
                        sb1.Append(" || ");
                        sb2.Append(" || ");
                    }
                }

                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillEffect).Text = sb1.ToString();
                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillRate).Text = sb2.ToString();


                // 요정 능력치 초기화

                string[] abilities = { "FireRate", "Accuracy", "Evasion", "Armor", "Critical" };
                int[] Progresses = { Resource.Id.FairyInfoFRProgress, Resource.Id.FairyInfoACProgress, Resource.Id.FairyInfoEVProgress, Resource.Id.FairyInfoAMProgress, Resource.Id.FairyInfoCRProgress };
                int[] ProgressMaxTexts = { Resource.Id.FairyInfoFRProgressMax, Resource.Id.FairyInfoACProgressMax, Resource.Id.FairyInfoEVProgressMax, Resource.Id.FairyInfoAMProgressMax, Resource.Id.FairyInfoCRProgressMax };
                int[] StatusTexts = { Resource.Id.FairyInfoFRStatus, Resource.Id.FairyInfoACStatus, Resource.Id.FairyInfoEVStatus, Resource.Id.FairyInfoAMStatus, Resource.Id.FairyInfoCRStatus };

                for (int i = 0; i < Progresses.Length; ++i)
                {
                    FindViewById<TextView>(ProgressMaxTexts[i]).Text = FindViewById<ProgressBar>(Progresses[i]).Max.ToString();

                    int MaxValue = int.Parse(fairy.Abilities[abilities[i]].Split('/')[1]);

                    FindViewById<ProgressBar>(Progresses[i]).Progress = MaxValue;
                    FindViewById<TextView>(StatusTexts[i]).Text = fairy.Abilities[abilities[i]];
                }

                if (ETC.useLightTheme == true) SetCardTheme();
                ShowCardViewVisibility();
                HideFloatingActionButtonAnimation();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.DBDetail_LoadDetailFail, ToastLength.Long).Show();
            }
            finally
            {
                InitLoadProgressBar.Visibility = ViewStates.Invisible;
            }
        }

        private void SetCardTheme()
        {
            int[] CardViewIds = { Resource.Id.FairyDBDetailBasicInfoCardLayout, Resource.Id.FairyDBDetailSkillCardLayout, Resource.Id.FairyDBDetailAbilityCardLayout };

            foreach (int id in CardViewIds)
            {
                CardView cv = FindViewById<CardView>(id);

                cv.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                cv.Radius = 15.0f;
            }
        }

        private void ShowCardViewVisibility()
        {
            FindViewById<CardView>(Resource.Id.FairyDBDetailBasicInfoCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.FairyDBDetailSkillCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.FairyDBDetailAbilityCardLayout).Visibility = ViewStates.Visible;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }
    }
}