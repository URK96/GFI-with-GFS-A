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
    public class EquipDBDetailActivity : FragmentActivity
    {
        private LinearLayout AbilityTableSubLayout;

        private DataRow EquipInfoDR = null;
        private string EquipName;
        private int EquipGrade;
        private string EquipCategory;
        private string EquipType;
        private string IconName;

        private ProgressBar InitLoadProgressBar;
        private FloatingActionButton RefreshCacheFAB;
        private CoordinatorLayout SnackbarLayout = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.EquipDBDetailLayout);

                EquipName = Intent.GetStringExtra("Keyword");

                EquipInfoDR = ETC.FindDataRow(ETC.EquipmentList, "Name", EquipName);
                EquipGrade = (int)EquipInfoDR["Grade"];
                EquipCategory = (string)EquipInfoDR["Category"];
                EquipType = (string)EquipInfoDR["Type"];
                IconName = (string)EquipInfoDR["Icon"];

                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.EquipDBDetailInitLoadProgress);
                AbilityTableSubLayout = FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAbilitySubLayout);

                RefreshCacheFAB = FindViewById<FloatingActionButton>(Resource.Id.EquipDBDetailRefreshCacheFAB);

                RefreshCacheFAB.Click += RefreshCacheFAB_Click;

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EquipDBDetailSnackbarLayout);

                InitLoadProcess(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private void RefreshCacheFAB_Click(object sender, EventArgs e)
        {
            InitLoadProcess(true);
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
                    if ((File.Exists(Path.Combine(ETC.CachePath, "Equip", "Normal", IconName + ".gfdcache")) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Equipments", IconName + ".png"), Path.Combine(ETC.CachePath, "Equip", "Normal", IconName + ".gfdcache"));
                        }
                    }

                    if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", true) == true)
                    {
                        Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Equip", "Normal", IconName + ".gfdcache"));
                        drawable.SetAlpha(40);
                        FindViewById<RelativeLayout>(Resource.Id.EquipDBDetailMainLayout).Background = drawable;
                    }

                    ImageView EquipImage = FindViewById<ImageView>(Resource.Id.EquipDBDetailImage);
                    EquipImage.SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Equip", "Normal", IconName + ".gfdcache")));
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }

                FindViewById<TextView>(Resource.Id.EquipDBDetailEquipName).Text = EquipName;
                FindViewById<TextView>(Resource.Id.EquipDBDetailEquipType).Text = EquipType;
                FindViewById<TextView>(Resource.Id.EquipDBDetailEquipProductTime).Text = ETC.CalcTime((int)EquipInfoDR["ProductTime"]);


                // 장비 기본 정보 초기화

                int[] GradeStarIds = { Resource.Id.EquipDBDetailInfoGrade1, Resource.Id.EquipDBDetailInfoGrade2, Resource.Id.EquipDBDetailInfoGrade3, Resource.Id.EquipDBDetailInfoGrade4, Resource.Id.EquipDBDetailInfoGrade5 };

                if (EquipGrade == 0)
                {
                    for (int i = 1; i < GradeStarIds.Length; ++i) FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;
                    FindViewById<ImageView>(GradeStarIds[0]).SetImageResource(Resource.Drawable.Grade_Star_EX);
                }
                else
                {
                    for (int i = EquipGrade; i < GradeStarIds.Length; ++i) FindViewById<ImageView>(GradeStarIds[i]).Visibility = ViewStates.Gone;
                    for (int i = 0; i < EquipGrade; ++i) FindViewById<ImageView>(GradeStarIds[i]).SetImageResource(Resource.Drawable.Grade_Star);
                }

                FindViewById<TextView>(Resource.Id.EquipDBDetailInfoCategory).Text = EquipCategory;
                FindViewById<TextView>(Resource.Id.EquipDBDetailInfoType).Text = EquipType;
                FindViewById<TextView>(Resource.Id.EquipDBDetailInfoName).Text = EquipName;
                if (EquipInfoDR["Note"] != DBNull.Value) FindViewById<TextView>(Resource.Id.EquipDBDetailInfoETC).Text = (string)EquipInfoDR["Note"];


                // 장비 사용여부 정보 초기화

                bool IsOnlyUse = false;

                if (EquipInfoDR["OnlyUse"] != DBNull.Value)
                {
                    if (string.IsNullOrWhiteSpace((string)EquipInfoDR["OnlyUse"]) == false) IsOnlyUse = true;
                    else IsOnlyUse = false;
                }
                else IsOnlyUse = false;

                switch (IsOnlyUse)
                {
                    case false:
                        FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAvailableInfoOnlyUseLayout).Visibility = ViewStates.Gone;
                        FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAvailableInfoRecommendLayout).Visibility = ViewStates.Visible;
                        FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAvailableInfoUseLayout).Visibility = ViewStates.Visible;

                        string[] TotalAvailable = ((string)EquipInfoDR["DollType"]).Split(';');
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

                        FindViewById<TextView>(Resource.Id.EquipDBDetailAvailableInfoOnlyUse).Text = (string)EquipInfoDR["OnlyUse"];
                        break;
                }


                // 장비 능력치 초기화

                string[] Abilities = ((string)EquipInfoDR["Ability"]).Split(';');
                string[] AbilityInitMags = ((string)EquipInfoDR["InitialMagnification"]).Split(';');
                string[] AbilityMaxMags = ((string)EquipInfoDR["MaxMagnification"]).Split(';');

                AbilityTableSubLayout.RemoveAllViews();

                for (int i = 0; i < Abilities.Length; ++i)
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

                    ability.Text = Abilities[i];
                    ability.SetTextColor(Android.Graphics.Color.LimeGreen);
                    ability.Gravity = GravityFlags.Center;
                    initmag.Text = AbilityInitMags[i];
                    initmag.Gravity = GravityFlags.Center;
                    if (AbilityMaxMags[0] == "강화 불가") maxmag.Text = AbilityMaxMags[0];
                    else maxmag.Text = AbilityMaxMags[i];
                    maxmag.Gravity = GravityFlags.Center;

                    layout.AddView(ability);
                    layout.AddView(initmag);
                    layout.AddView(maxmag);

                    AbilityTableSubLayout.AddView(layout);
                }


                if (ETC.UseLightTheme == true) SetCardTheme();
                ShowCardViewAnimation();
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

        private void ShowCardViewAnimation()
        {
            FindViewById<CardView>(Resource.Id.EquipDBDetailBasicInfoCardLayout).Animate().Alpha(1.0f).SetDuration(500).Start();
            FindViewById<CardView>(Resource.Id.EquipDBDetailAvailableInfoCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(500).Start();
            FindViewById<CardView>(Resource.Id.EquipDBDetailAbilityCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(1000).Start();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }
    }
}