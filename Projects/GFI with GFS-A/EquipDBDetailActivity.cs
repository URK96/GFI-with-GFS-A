using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
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
    [Activity(Label = "", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EquipDBDetailActivity : BaseAppCompatActivity
    {
        private LinearLayout abilityTableSubLayout;

        private Equip equip;
        private DataRow equipInfoDR = null;

        private SwipeRefreshLayout refreshMainLayout;
        private Android.Support.V7.Widget.Toolbar toolbar;
        private CoordinatorLayout snackbarLayout;

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
                SetContentView(Resource.Layout.EquipDBDetailLayout);

                equipInfoDR = ETC.FindDataRow(ETC.equipmentList, "Id", Intent.GetIntExtra("Id", 0));
                equip = new Equip(equipInfoDR);

                toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.EquipDBDetailMainToolbar);

                SetSupportActionBar(toolbar);
                SupportActionBar.Title = $"No.{equip.Id} {equip.Name}";
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                refreshMainLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.EquipDBDetailMainRefreshLayout);
                refreshMainLayout.Refresh += delegate { _ = InitLoadProcess(true); };
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EquipDBDetailSnackbarLayout);

                abilityTableSubLayout = FindViewById<LinearLayout>(Resource.Id.EquipDBDetailAbilitySubLayout);

                await InitializeProcess();
                _ = InitLoadProcess(false);

                /*if ((ETC.locale.Language == "ko") && ETC.sharedPreferences.GetBoolean("Help_EquipDBDetail", true))
                {
                    ETC.RunHelpActivity(this, "EquipDBDetail");
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
                MenuInflater.Inflate(Resource.Menu.EquipDBDetailMenu, menu);

                if (equip.ProductTime == 0)
                {
                    menu?.FindItem(Resource.Id.EquipDBDetailProductPercentage).SetVisible(false);
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
                    case Resource.Id.EquipDBDetailLink:
                        Android.Support.V7.Widget.PopupMenu pMenu = new Android.Support.V7.Widget.PopupMenu(this, FindViewById<View>(Resource.Id.EquipDBDetailLink));
                        pMenu.Inflate(Resource.Menu.DBLinkMenu);
                        pMenu.MenuItemClick += PMenu_MenuItemClick;
                        pMenu.Show();
                        break;
                    case Resource.Id.EquipDBDetailProductPercentage:
                        var intent = new Intent(this, typeof(ProductPercentTableActivity));
                        intent.PutExtra("Info", new string[] { "Equip", equip.Id.ToString() });
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
                        url = $"https://namu.wiki/w/소녀전선/장비";
                        break;
                    case Resource.Id.DBLinkInven:
                        url = $"http://gf.inven.co.kr/dataninfo/item/";
                        break;
                    case Resource.Id.DBLink36Base:
                        url = $"https://girlsfrontline.kr/equip/{equip.Id}";
                        break;
                    case Resource.Id.DBLinkGFDB:
                        url = $"https://gfl.zzzzz.kr/equip.php?lang=ko";
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
                Android.Graphics.Color toolbarColor;

                switch (equip.Grade)
                {
                    case 2:
                        toolbarColor = Android.Graphics.Color.SlateGray;
                        break;
                    case 3:
                        toolbarColor = Android.Graphics.Color.ParseColor("#55CCEE");
                        break;
                    case 4:
                        toolbarColor = Android.Graphics.Color.ParseColor("#AACC22");
                        break;
                    case 5:
                        toolbarColor = Android.Graphics.Color.ParseColor("#FFBB22");
                        break;
                    default:
                    case 0:
                        toolbarColor = Android.Graphics.Color.ParseColor("#C040B0");
                        break;
                }

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


        private async Task InitLoadProcess(bool IsRefresh)
        {
            await Task.Delay(100);

            try
            {
                refreshMainLayout.Refreshing = true;


                // 장비 타이틀 바 초기화

                try
                {
                    if ((File.Exists(Path.Combine(ETC.cachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache")) == false) || (IsRefresh == true))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.server, "Data", "Images", "Equipments", $"{equip.Icon}.png"), Path.Combine(ETC.cachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache"));
                        }
                    }

                    if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", true) == true)
                    {
                        Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache"));
                        drawable.SetAlpha(40);
                        FindViewById<RelativeLayout>(Resource.Id.EquipDBDetailMainLayout).Background = drawable;
                    }

                    FindViewById<ImageView>(Resource.Id.EquipDBDetailImage).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache")));
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

                abilityTableSubLayout.RemoveAllViews();

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

                    abilityTableSubLayout.AddView(layout);
                }

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
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBDetail_LoadDetailFail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                refreshMainLayout.Refreshing = false;
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