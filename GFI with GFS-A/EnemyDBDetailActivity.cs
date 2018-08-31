using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EnemyDBDetailActivity : Activity
    {
        private DataRow[] EnemyInfoDRs;
        private string EnemyName;
        private string EnemyCodeName;
        private int EnemyTypeIndex = 0;
        private bool IsBoss;

        private bool ListingComplete = false;

        private CoordinatorLayout SnackbarLayout;

        private ProgressBar InitLoadProgressBar;
        private Spinner TypeSelector;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzY0NkAzMTM2MmUzMjJlMzBmNFFDVVZlU2NDRTVmYVJqQ0ZyOTVPOGhYWnFIazlQNFNPeGVEMU9WMjZnPQ==");

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.EnemyDBDetailLayout);

                EnemyCodeName = Intent.GetStringExtra("Keyword");

                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.EnemyDBDetailInitLoadProgress);

                FindViewById<ImageView>(Resource.Id.EnemyDBDetailSmallImage).Click += EnemyDBDetailSmallImage_Click;

                TypeSelector = FindViewById<Spinner>(Resource.Id.EnemyDBDetailEnemyTypeSelector);
                TypeSelector.ItemSelected += TypeSelector_ItemSelected;

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EnemyDBDetailSnackbarLayout);

                InitLoadProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void TypeSelector_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            EnemyTypeIndex = e.Position;

            InitLoadProcess();
        }

        private void EnemyDBDetailSmallImage_Click(object sender, EventArgs e)
        {
            try
            {
                var EnemyImageViewer = new Intent(this, typeof(EnemyDBImageViewer));
                EnemyImageViewer.PutExtra("Data", EnemyCodeName);
                StartActivity(EnemyImageViewer);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ImageViewer_ActivityOpenError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task InitLoadProcess()
        {
            InitLoadProgressBar.Visibility = ViewStates.Visible;

            await Task.Delay(100);

            try
            {
                if (ListingComplete == false) await InitializeTypeList();

                // 철혈 타이틀 바 초기화

                if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false) == true)
                {
                    if (File.Exists(Path.Combine(ETC.CachePath, "Enemy", "Normal", EnemyCodeName + ".gfdcache")) == false)
                    {
                        using (TimeOutWebClient wc = new TimeOutWebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Enemy", "Normal", EnemyCodeName + ".png"), Path.Combine(ETC.CachePath, "Enemy", "Normal", EnemyCodeName + ".gfdcache"));
                        }
                    }

                    Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Enemy", "Normal", EnemyCodeName + ".gfdcache"));
                    drawable.SetAlpha(40);
                    FindViewById<RelativeLayout>(Resource.Id.EnemyDBDetailMainLayout).Background = drawable;
                }

                string FileName = EnemyCodeName;

                if (File.Exists(Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", FileName + ".gfdcache")) == false)
                {
                    using (TimeOutWebClient wc = new TimeOutWebClient())
                    {
                        await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Enemy", "Normal_Crop", EnemyCodeName + ".png"), Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", FileName + ".gfdcache"));
                    }
                }

                ImageView EnemySmallImage = FindViewById<ImageView>(Resource.Id.EnemyDBDetailSmallImage);
                EnemySmallImage.SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", FileName + ".gfdcache")));

                if (IsBoss == true) FindViewById<TextView>(Resource.Id.EnemyDBDetailType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Boss);
                else FindViewById<TextView>(Resource.Id.EnemyDBDetailType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Normal);
                FindViewById<TextView>(Resource.Id.EnemyDBDetailEnemyName).Text = EnemyName;
                FindViewById<TextView>(Resource.Id.EnemyDBDetailEnemyCodeName).Text = EnemyCodeName;


                // 철혈 기본 정보 초기화

                int GradeIconId = 0;

                switch (IsBoss)
                {
                    case true:
                        GradeIconId = Resource.Drawable.Type_Boss;
                        break;
                    case false:
                        GradeIconId = Resource.Drawable.Type_Normal;
                        break;
                }
                FindViewById<ImageView>(Resource.Id.EnemyDBDetailInfoGrade).SetImageResource(GradeIconId);

                if (IsBoss == true) FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoEnemyType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Boss);
                else FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoEnemyType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Normal);
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoName).Text = EnemyName;
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoCodeName).Text = EnemyCodeName;
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoVoiceActor).Text = "";
                if (IsBoss == true) FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoAppearPlace).Text = (string)EnemyInfoDRs[EnemyTypeIndex]["Type"];


                // 철혈 능력치 초기화

                string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed", "Penetration", "Armor", "Range" };
                int[] Progresses = { Resource.Id.EnemyInfoHPProgress, Resource.Id.EnemyInfoFRProgress, Resource.Id.EnemyInfoEVProgress, Resource.Id.EnemyInfoACProgress, Resource.Id.EnemyInfoASProgress, Resource.Id.EnemyInfoPTProgress, Resource.Id.EnemyInfoAMProgress, Resource.Id.EnemyInfoRangeProgress };
                int[] ProgressMaxTexts = { Resource.Id.EnemyInfoHPProgressMax, Resource.Id.EnemyInfoFRProgressMax, Resource.Id.EnemyInfoEVProgressMax, Resource.Id.EnemyInfoACProgressMax, Resource.Id.EnemyInfoASProgressMax, Resource.Id.EnemyInfoPTProgressMax, Resource.Id.EnemyInfoAMProgressMax, Resource.Id.EnemyInfoRangeProgressMax };
                int[] StatusTexts = { Resource.Id.EnemyInfoHPStatus, Resource.Id.EnemyInfoFRStatus, Resource.Id.EnemyInfoEVStatus, Resource.Id.EnemyInfoACStatus, Resource.Id.EnemyInfoASStatus, Resource.Id.EnemyInfoPTStatus, Resource.Id.EnemyInfoAMStatus, Resource.Id.EnemyInfoRangeStatus };

                for (int i = 0; i < Progresses.Length; ++i)
                {
                    FindViewById<TextView>(ProgressMaxTexts[i]).Text = FindViewById<ProgressBar>(Progresses[i]).Max.ToString();

                    int value = (int)EnemyInfoDRs[EnemyTypeIndex][abilities[i]];

                    FindViewById<ProgressBar>(Progresses[i]).Progress = value;

                    FindViewById<TextView>(StatusTexts[i]).Text = value.ToString();
                }


                ShowCardViewAnimation();
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

        private async Task InitializeTypeList()
        {
            List<string> TypeList = new List<string>();
            List<int> row_index = new List<int>();

            for (int i = 0; i < ETC.EnemyList.Rows.Count; ++i)
            {
                DataRow dr = ETC.EnemyList.Rows[i];

                if ((string)dr["CodeName"] != EnemyCodeName) continue;

                row_index.Add(i);
                TypeList.Add((string)dr["Type"]);
            }

            row_index.TrimExcess();
            TypeList.TrimExcess();

            EnemyInfoDRs = new DataRow[row_index.Count];

            for (int i = 0; i < EnemyInfoDRs.Length; ++i) EnemyInfoDRs[i] = ETC.EnemyList.Rows[row_index[i]];

            EnemyName = (string)EnemyInfoDRs[0]["Name"];
            IsBoss = (bool)EnemyInfoDRs[0]["IsBoss"];

            var TypeListAdapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, TypeList);
            TypeListAdapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

            TypeSelector.Adapter = TypeListAdapter;

            ListingComplete = true;
        }

        private void SetCardTheme()
        {
            int[] CardViewIds = { Resource.Id.EnemyDBDetailBasicInfoCardLayout, Resource.Id.EnemyDBDetailAbilityCardLayout };

            foreach (int id in CardViewIds)
            {
                CardView cv = FindViewById<CardView>(id);

                cv.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                cv.Radius = 15.0f;
            }
        }

        private async Task ShowCardViewAnimation()
        {
            if (FindViewById<CardView>(Resource.Id.EnemyDBDetailBasicInfoCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.EnemyDBDetailBasicInfoCardLayout).Animate().Alpha(1.0f).SetDuration(500).Start();
            if (FindViewById<CardView>(Resource.Id.EnemyDBDetailAbilityCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.EnemyDBDetailAbilityCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(1500).Start();
        }

    }
}