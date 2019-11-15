﻿using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Support.V4.Widget;
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
    [Activity(Label = "", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FairyDBDetailActivity : BaseAppCompatActivity
    {
        private Fairy fairy;
        private DataRow fairyInfoDR = null;

        private Android.Support.V7.Widget.Toolbar toolbar;
        private SwipeRefreshLayout refreshMainLayout;
        private CoordinatorLayout snackbarLayout;

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
                SetContentView(Resource.Layout.FairyDBDetailLayout);

               

                fairyInfoDR = ETC.FindDataRow(ETC.fairyList, "DicNumber", Intent.GetIntExtra("DicNum", 0));
                fairy = new Fairy(fairyInfoDR);

                toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.FairyDBDetailMainToolbar);

                SetSupportActionBar(toolbar);
                SupportActionBar.Title = $"No.{fairy.DicNumber} {fairy.Name}";
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                refreshMainLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.FairyDBDetailMainRefreshLayout);
                refreshMainLayout.Refresh += delegate { _ = InitLoadProcess(true); };
                FindViewById<ImageView>(Resource.Id.FairyDBDetailSmallImage).Click += FairyDBDetailSmallImage_Click;
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FairyDBSnackbarLayout);

                await InitializeProcess();
                _ = InitLoadProcess(false);

                /*if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_FairyDBDetail", true)))
                {
                    ETC.RunHelpActivity(this, "FairyDBDetail");
                }*/
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
                MenuInflater.Inflate(Resource.Menu.FairyDBDetailMenu, menu);
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
                    case Resource.Id.FairyDBDetailLink:
                        Android.Support.V7.Widget.PopupMenu pMenu = new Android.Support.V7.Widget.PopupMenu(this, FindViewById<View>(Resource.Id.FairyDBDetailLink));
                        pMenu.Inflate(Resource.Menu.DBLinkMenu);
                        pMenu.MenuItemClick += PMenu_MenuItemClick;
                        pMenu.Show();
                        break;
                    case Resource.Id.FairyDBDetailProductPercentage:
                        var intent = new Intent(this, typeof(ProductPercentTableActivity));
                        intent.PutExtra("Info", new string[] { "Fairy", fairy.DicNumber.ToString() });
                        StartActivity(intent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
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

        private void PMenu_MenuItemClick(object sender, Android.Support.V7.Widget.PopupMenu.MenuItemClickEventArgs e)
        {
            try
            {
                string url = "";

                switch (e.Item.ItemId)
                {
                    case Resource.Id.DBLinkNamu:
                        url = $"https://namu.wiki/w/{fairy.Name}{((fairy.DicNumber >= 1000) ? "(소녀전선)" : "")}";
                        break;
                    case Resource.Id.DBLinkInven:
                        url = $"http://gf.inven.co.kr/dataninfo/item/";
                        break;
                    case Resource.Id.DBLink36Base:
                        url = $"https://girlsfrontline.kr/fairy/{fairy.DicNumber}";
                        break;
                    case Resource.Id.DBLinkGFDB:
                        url = $"https://gfl.zzzzz.kr/fairy.php?id={fairy.DicNumber}&lang=ko";
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
                Android.Graphics.Color toolbarColor = Android.Graphics.Color.ParseColor("#C040B0");

                if (fairy.DicNumber >= 1000)
                {
                    toolbar.SetBackgroundColor(toolbarColor);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Window.SetStatusBarColor(toolbarColor);
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail Initialize Process", ToastLength.Short).Show();
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

        private async Task InitLoadProcess(bool isRefresh)
        {
            await Task.Delay(100);

            try
            {
                refreshMainLayout.Refreshing = true;


                // 요정 타이틀 바 초기화

                try
                {
                    if (!File.Exists(Path.Combine(ETC.cachePath, "Fairy", "Normal", $"{fairy.DicNumber}_1.gfdcache")) || isRefresh)
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.server, "Data", "Images", "Fairy", $"{fairy.DicNumber}_1.png"), Path.Combine(ETC.cachePath, "Fairy", "Normal", $"{fairy.DicNumber}_1.gfdcache"));
                        }
                    }

                    if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", true))
                    {
                        Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "Fairy", "Normal", $"{fairy.DicNumber}_1.gfdcache"));
                        drawable.SetAlpha(40);
                        FindViewById<RelativeLayout>(Resource.Id.FairyDBDetailMainLayout).Background = drawable;
                    }

                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSmallImage).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "Fairy", "Normal", $"{fairy.DicNumber}_1.gfdcache")));
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
                    if (!File.Exists(Path.Combine(ETC.cachePath, "Fairy", "Skill", $"{fairy.SkillName}.gfdcache")) || isRefresh)
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.server, "Data", "Images", "FairySkill", $"{fairy.SkillName}.png"), Path.Combine(ETC.cachePath, "Fairy", "Skill", $"{fairy.SkillName}.gfdcache"));
                        }
                    }

                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "Fairy", "Skill", $"{fairy.SkillName}.gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillName).Text = fairy.SkillName;

                if (ETC.useLightTheme)
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

                if (ETC.useLightTheme)
                {
                    SetCardTheme();
                }

                ShowCardViewVisibility();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.DBDetail_LoadDetailFail, ToastLength.Long).Show();
            }
            finally
            {
                refreshMainLayout.Refreshing = false;
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