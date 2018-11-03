﻿using Android.App;
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
    public class FairyDBDetailActivity : FragmentActivity
    {
        System.Timers.Timer FABTimer = new System.Timers.Timer();

        private DataRow FairyInfoDR = null;
        private int FairyDicNum;
        private string FairyName;
        private string FairyType;

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

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.FairyDBDetailLayout);

                FairyName = Intent.GetStringExtra("Keyword");

                FairyInfoDR = ETC.FindDataRow(ETC.FairyList, "Name", FairyName);
                FairyDicNum = (int)FairyInfoDR["DicNumber"];
                FairyType = (string)FairyInfoDR["Type"];

                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.FairyDBDetailInitLoadProgress);
                FindViewById<ImageView>(Resource.Id.FairyDBDetailSmallImage).Click += FairyDBDetailSmallImage_Click;

                RefreshCacheFAB = FindViewById<FloatingActionButton>(Resource.Id.FairyDBDetailRefreshCacheFAB);
                PercentTableFAB = FindViewById<FloatingActionButton>(Resource.Id.FairyDBDetailProductPercentFAB);
                if ((int)FairyInfoDR["ProductTime"] == 0) PercentTableFAB.Visibility = ViewStates.Gone;
                MainFAB = FindViewById<FloatingActionButton>(Resource.Id.FairyDBDetailSideLinkMainFAB);
                GFDBFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkFAB1);
                GFDBFAB.SetImageResource(Resource.Drawable.GFDB_Logo);
                InvenFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkFAB2);
                InvenFAB.SetImageResource(Resource.Drawable.Inven_Logo);
                BaseFAB = FindViewById<FloatingActionButton>(Resource.Id.SideLinkFAB3);
                BaseFAB.SetImageResource(Resource.Drawable.Base36_Logo);

                RefreshCacheFAB.Click += RefreshCacheFAB_Click;
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
                FABTimer.Elapsed += FABTimer_Elapsed;

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
                ETC.LogError(this, ex.ToString());
            }
        }

        private void FABTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HideFloatingActionButtonAnimation();
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

        private void RefreshCacheFAB_Click(object sender, EventArgs e)
        {
            InitLoadProcess(true);
        }

        private void PercentTableFAB_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(ProductPercentTableActivity));
                intent.PutExtra("Info", new string[] { "Fairy", FairyDicNum.ToString() });
                StartActivity(intent);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
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
                        string uri = string.Format("http://gfl.zzzzz.kr/fairy.php?id={0}&lang=ko", FairyDicNum);
                        var intent = new Intent(this, typeof(WebBrowserActivity));
                        intent.PutExtra("url", uri);
                        StartActivity(intent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.SideLinkFAB2:
                        string uri2 = string.Format("http://girlsfrontline.inven.co.kr/dataninfo/fairy/?d=133&c={0}", FairyDicNum);
                        var intent2 = new Intent(this, typeof(WebBrowserActivity));
                        intent2.PutExtra("url", uri2);
                        StartActivity(intent2);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.SideLinkFAB3:
                        string uri3 = string.Format("https://girlsfrontline.kr/doll/{0}", FairyDicNum);
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
                    ETC.LogError(this, ex.ToString());
                    ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeSubMenuError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                }
            }
        }

        private void FairyDBDetailSmallImage_Click(object sender, EventArgs e)
        {
            try
            {
                var FairyImageViewer = new Intent(this, typeof(FairyDBImageViewer));
                FairyImageViewer.PutExtra("Keyword", FairyName);
                StartActivity(FairyImageViewer);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
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
                    if ((File.Exists(Path.Combine(ETC.CachePath, "Fairy", "Normal", FairyName + "_1" + ".gfdcache")) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Fairy", FairyName + "_1" + ".png"), Path.Combine(ETC.CachePath, "Fairy", "Normal", FairyName + "_1" + ".gfdcache"));
                        }
                    }

                    if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", true) == true)
                    {
                        Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Fairy", "Normal", FairyName + "_1" + ".gfdcache"));
                        drawable.SetAlpha(40);
                        FindViewById<RelativeLayout>(Resource.Id.FairyDBDetailMainLayout).Background = drawable;
                    }

                    ImageView FairySmallImage = FindViewById<ImageView>(Resource.Id.FairyDBDetailSmallImage);
                    FairySmallImage.SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Fairy", "Normal", FairyName + "_1" + ".gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }

                FindViewById<TextView>(Resource.Id.FairyDBDetailFairyType).Text = FairyType;
                FindViewById<TextView>(Resource.Id.FairyDBDetailFairyName).Text = FairyName;
                FindViewById<TextView>(Resource.Id.FairyDBDetailFairyProductTime).Text = ETC.CalcTime((int)FairyInfoDR["ProductTime"]);


                // 요정 기본 정보 초기화

                FindViewById<TextView>(Resource.Id.FairyDBDetailInfoType).Text = FairyType;
                FindViewById<TextView>(Resource.Id.FairyDBDetailInfoName).Text = FairyName;
                FindViewById<TextView>(Resource.Id.FairyDBDetailInfoIllustrator).Text = "";

                if (FairyInfoDR["Note"] == DBNull.Value) FindViewById<TextView>(Resource.Id.FairyDBDetailInfoETC).Text = "";
                else FindViewById<TextView>(Resource.Id.FairyDBDetailInfoETC).Text = (string)FairyInfoDR["Note"];


                // 요정 스킬 정보 초기화

                string SkillName = (string)FairyInfoDR["SkillName"];

                try
                {
                    if ((File.Exists(Path.Combine(ETC.CachePath, "Fairy", "Skill", SkillName + ".gfdcache")) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "FairySkill", SkillName + ".png"), Path.Combine(ETC.CachePath, "Fairy", "Skill", SkillName + ".gfdcache"));
                        }
                    }

                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Fairy", "Skill", SkillName + ".gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }

                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillName).Text = SkillName;

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillTicketIcon).SetImageResource(Resource.Drawable.FairyTicket_Icon_WhiteTheme);
                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillCoolTimeIcon).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                }

                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillTicket).Text = ((int)FairyInfoDR["OrderConsume"]).ToString();
                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillCoolTime).Text = ((int)FairyInfoDR["CoolDown"]).ToString();
                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillExplain).Text = (string)FairyInfoDR["SkillExplain"];

                string[] effect = ((string)FairyInfoDR["SkillEffect"]).Split(';');
                string[] rate = ((string)FairyInfoDR["SkillRate"]).Split(';');

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

                int delay = 1;

                string[] abilities = { "FireRate", "Accuracy", "Evasion", "Armor", "Critical" };
                int[] Progresses = { Resource.Id.FairyInfoFRProgress, Resource.Id.FairyInfoACProgress, Resource.Id.FairyInfoEVProgress, Resource.Id.FairyInfoAMProgress, Resource.Id.FairyInfoCRProgress };
                int[] ProgressMaxTexts = { Resource.Id.FairyInfoFRProgressMax, Resource.Id.FairyInfoACProgressMax, Resource.Id.FairyInfoEVProgressMax, Resource.Id.FairyInfoAMProgressMax, Resource.Id.FairyInfoCRProgressMax };
                int[] StatusTexts = { Resource.Id.FairyInfoFRStatus, Resource.Id.FairyInfoACStatus, Resource.Id.FairyInfoEVStatus, Resource.Id.FairyInfoAMStatus, Resource.Id.FairyInfoCRStatus };

                for (int i = 0; i < Progresses.Length; ++i)
                {
                    FindViewById<TextView>(ProgressMaxTexts[i]).Text = FindViewById<ProgressBar>(Progresses[i]).Max.ToString();

                    int MaxValue = 0;

                    MaxValue = int.Parse((((string)FairyInfoDR[abilities[i]]).Split('/'))[1]);

                    ETC.UpProgressBarProgress(FindViewById<ProgressBar>(Progresses[i]), 0, MaxValue, delay);
                    FindViewById<TextView>(StatusTexts[i]).Text = ((string)FairyInfoDR[abilities[i]]);
                }


                if (ETC.UseLightTheme == true) SetCardTheme();
                ShowCardViewAnimation();
                HideFloatingActionButtonAnimation();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
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

        private void ShowCardViewAnimation()
        {
            FindViewById<CardView>(Resource.Id.FairyDBDetailBasicInfoCardLayout).Animate().Alpha(1.0f).SetDuration(500).Start();
            FindViewById<CardView>(Resource.Id.FairyDBDetailSkillCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(500).Start();
            FindViewById<CardView>(Resource.Id.FairyDBDetailAbilityCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(1000).Start();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }
    }
}