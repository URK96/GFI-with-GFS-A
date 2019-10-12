using Android;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EquipDBDetailActivity : BaseFragmentActivity
    {
        System.Timers.Timer FABTimer = new System.Timers.Timer();

        private LinearLayout AbilityTableSubLayout;

        private Equip equip;
        private DataRow EquipInfoDR = null;

        private bool IsEnableFABMenu = false;

        private ProgressBar InitLoadProgressBar;
        private FloatingActionButton RefreshCacheFAB;
        private FloatingActionButton PercentTableFAB;
        private CoordinatorLayout SnackbarLayout = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.EquipDBDetailLayout);

                EquipInfoDR = ETC.FindDataRow(ETC.EquipmentList, "Id", Intent.GetIntExtra("Id", 0));
                equip = new Equip(EquipInfoDR);
                
                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.EquipDBDetailInitLoadProgress);
                AbilityTableSubLayout = FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAbilitySubLayout);

                RefreshCacheFAB = FindViewById<FloatingActionButton>(Resource.Id.EquipDBDetailRefreshCacheFAB);
                RefreshCacheFAB.Click += RefreshCacheFAB_Click;
                RefreshCacheFAB.LongClick += DBDetailFAB_LongClick;
                PercentTableFAB = FindViewById<FloatingActionButton>(Resource.Id.EquipDBDetailProductPercentFAB);
                PercentTableFAB.Click += PercentTableFAB_Click;
                PercentTableFAB.LongClick += DBDetailFAB_LongClick;

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EquipDBDetailSnackbarLayout);

                FABTimer.Interval = 3000;
                FABTimer.Elapsed += FABTimer_Elapsed;

                _ = InitLoadProcess(false);

                if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_EquipDBDetail", true) == true)) ETC.RunHelpActivity(this, "EquipDBDetail");
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
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
                    case Resource.Id.EquipDBDetailRefreshCacheFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_CacheRefresh);
                        break;
                    case Resource.Id.EquipDBDetailProductPercentFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_ProductPercentage);
                        break;
                }

                Toast.MakeText(this, tip, ToastLength.Short).Show();
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

        private void FABTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HideFloatingActionButtonAnimation();
        }

        private void PercentTableFAB_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsEnableFABMenu == false)
                {
                    PercentTableFAB.SetImageResource(Resource.Drawable.ProductPercentTable_Icon);
                    IsEnableFABMenu = true;
                    PercentTableFAB.Animate().Alpha(1.0f).SetDuration(500).Start();
                    PercentTableFAB.Show();
                    RefreshCacheFAB.Show();
                    FABTimer.Start();
                }
                else
                {
                    if ((int)EquipInfoDR["ProductTIme"] != 0)
                    {
                        var intent = new Intent(this, typeof(ProductPercentTableActivity));
                        intent.PutExtra("Info", new string[] { "Equip", equip.Id.ToString() });
                        StartActivity(intent);
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeSubMenuError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void HideFloatingActionButtonAnimation()
        {
            FABTimer.Stop();
            IsEnableFABMenu = false;

            RefreshCacheFAB.Hide();
            PercentTableFAB.Alpha = 0.3f;
            PercentTableFAB.SetImageResource(Resource.Drawable.HideFloating_Icon);
        }

        private async Task InitLoadProcess(bool IsRefresh)
        {
            InitLoadProgressBar.Visibility = ViewStates.Visible;

            await Task.Delay(100);

            try
            {
                     // 장비 타이틀 바 초기화

                try
                {
                    if ((File.Exists(Path.Combine(ETC.CachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache")) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Equipments", $"{equip.Icon}.png"), Path.Combine(ETC.CachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache"));
                        }
                    }

                    if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", true) == true)
                    {
                        Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache"));
                        drawable.SetAlpha(40);
                        FindViewById<RelativeLayout>(Resource.Id.EquipDBDetailMainLayout).Background = drawable;
                    }

                    FindViewById<ImageView>(Resource.Id.EquipDBDetailImage).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                }

                FindViewById<TextView>(Resource.Id.EquipDBDetailEquipName).Text = equip.Name;
                FindViewById<TextView>(Resource.Id.EquipDBDetailEquipType).Text = equip.Type;
                FindViewById<TextView>(Resource.Id.EquipDBDetailEquipProductTime).Text = ETC.CalcTime(equip.ProductTime);


                             // 장비 기본 정보 초기화

                int[] GradeStarIds = { Resource.Id.EquipDBDetailInfoGrade1, Resource.Id.EquipDBDetailInfoGrade2, Resource.Id.EquipDBDetailInfoGrade3, Resource.Id.EquipDBDetailInfoGrade4, Resource.Id.EquipDBDetailInfoGrade5 };

                if (equip.Grade == 0)
                {
                    for (int i = 1; i < GradeStarIds.Length; ++i) FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;
                    FindViewById<ImageView>(GradeStarIds[0]).SetImageResource(Resource.Drawable.Grade_Star_EX);
                }
                else
                {
                    for (int i = equip.Grade; i < GradeStarIds.Length; ++i) FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;
                    for (int i = 0; i < equip.Grade; ++i) FindViewById<ImageView>(GradeStarIds[i]).SetImageResource(Resource.Drawable.Grade_Star);
                }

                FindViewById<TextView>(Resource.Id.EquipDBDetailInfoCategory).Text = equip.Category;
                FindViewById<TextView>(Resource.Id.EquipDBDetailInfoType).Text = equip.Type;
                FindViewById<TextView>(Resource.Id.EquipDBDetailInfoName).Text = equip.Name;
                FindViewById<TextView>(Resource.Id.EquipDBDetailInfoETC).Text = equip.Note;


                            // 장비 사용여부 정보 초기화

                bool IsOnlyUse = false;

                if (equip.OnlyUse != null)
                {
                    if (string.IsNullOrWhiteSpace(equip.OnlyUse[0]) == false) IsOnlyUse = true;
                    else IsOnlyUse = false;
                }
                else IsOnlyUse = false;

                switch (IsOnlyUse)
                {
                    case false:
                        FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAvailableInfoOnlyUseLayout).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAvailableInfoRecommendLayout).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAvailableInfoUseLayout).Visibility = ViewStates.Visible;

                        string[] TotalAvailable = equip.DollType;
                        List<string> RecommendType = new List<string>();
                        List<string> UseType = new List<string>();

                        foreach (string s in TotalAvailable)
                        {
                            string[] temp = s.Split(',');

                            if (temp[1] == "F") RecommendType.Add(temp[0]);
                            else if (temp[1] == "U") UseType.Add(temp[0]);
                        }

                        RecommendType.TrimExcess();
                        UseType.TrimExcess();

                        StringBuilder sb1 = new StringBuilder();
                        StringBuilder sb2 = new StringBuilder();

                        for (int i = 0; i < RecommendType.Count; ++i)
                        {
                            sb1.Append(RecommendType[i]);
                            if (i < (RecommendType.Count - 1)) sb1.Append(" | ");
                        }
                        for (int i = 0; i < UseType.Count; ++i)
                        {
                            sb2.Append(UseType[i]);
                            if (i < (UseType.Count - 1)) sb2.Append(" | ");
                        }

                        FindViewById<TextView>(Resource.Id.EquipDBDetailAvailableInfoRecommend).Text = sb1.ToString();
                        FindViewById<TextView>(Resource.Id.EquipDBDetailAvailableInfoUse).Text = sb2.ToString();
                        break;
                    case true:
                        FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAvailableInfoRecommendLayout).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAvailableInfoUseLayout).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAvailableInfoOnlyUseLayout).Visibility = ViewStates.Visible;

                        StringBuilder sb = new StringBuilder();
                        
                        for (int i = 0; i < equip.OnlyUse.Length; ++i)
                        {
                            sb.Append(equip.OnlyUse[i]);
                            if (i < (equip.OnlyUse.Length - 1)) sb.Append(" | ");
                        }

                        FindViewById<TextView>(Resource.Id.EquipDBDetailAvailableInfoOnlyUse).Text = sb.ToString();
                        break;
                }


                            // 장비 능력치 초기화

                string[] Abilities = equip.Abilities;
                string[] AbilityInitMags = equip.InitMags;
                string[] AbilityMaxMags = equip.MaxMags;

                AbilityTableSubLayout.RemoveAllViews();

                for (int i = 0; i < equip.Abilities.Length; ++i)
                {
                    LinearLayout layout = new LinearLayout(this)
                    {
                        Orientation = Orientation.Horizontal,
                        LayoutParameters = FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAbilityTopLayout).LayoutParameters
                    };

                    TextView ability = new TextView(this);
                    TextView initmag = new TextView(this);
                    TextView maxmag = new TextView(this);

                    ability.LayoutParameters = FindViewById<TextView>(Resource.Id.EquipDBDetailAbilityTopText1).LayoutParameters;
                    initmag.LayoutParameters = FindViewById<TextView>(Resource.Id.EquipDBDetailAbilityTopText2).LayoutParameters;
                    maxmag.LayoutParameters = FindViewById<TextView>(Resource.Id.EquipDBDetailAbilityTopText3).LayoutParameters;

                    ability.Text = equip.Abilities[i];
                    ability.SetTextColor(Android.Graphics.Color.LimeGreen);
                    ability.Gravity = GravityFlags.Center;
                    initmag.Text = equip.InitMags[i];
                    initmag.Gravity = GravityFlags.Center;
                    if (equip.CanUpgrade == false) maxmag.Text = "X";
                    else maxmag.Text = equip.MaxMags[i];
                    maxmag.Gravity = GravityFlags.Center;

                    layout.AddView(ability);
                    layout.AddView(initmag);
                    layout.AddView(maxmag);

                    AbilityTableSubLayout.AddView(layout);
                }

                if (ETC.useLightTheme == true) SetCardTheme();
                ShowCardViewVisibility();
                HideFloatingActionButtonAnimation();
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
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBDetail_LoadDetailFail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                InitLoadProgressBar.Visibility = ViewStates.Invisible;
            }
        }

        private void SetCardTheme()
        {
            int[] CardViewIds = { Resource.Id.EquipDBDetailBasicInfoCardLayout, Resource.Id.EquipDBDetailAvailableInfoCardLayout, Resource.Id.EquipDBDetailAbilityCardLayout };

            foreach (int id in CardViewIds)
            {
                CardView cv = FindViewById<CardView>(id);

                cv.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                cv.Radius = 15.0f;
            }
        }

        private void ShowCardViewVisibility()
        {
            FindViewById<CardView>(Resource.Id.EquipDBDetailBasicInfoCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.EquipDBDetailAvailableInfoCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.EquipDBDetailAbilityCardLayout).Visibility = ViewStates.Visible;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }
    }
}