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
        private TableLayout SkillTableLayout1;
        private TableLayout SkillTableLayout2;

        private DataRow FairyInfoDR = null;
        private string FairyName;
        private string FairyType;

        private ProgressBar InitLoadProgressBar;
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
                FairyType = (string)FairyInfoDR["Type"];

                //SkillTableLayout1 = FindViewById<TableLayout>();
                //SkillTableLayout2 = FindViewById<TableLayout>();

                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.FairyDBDetailInitLoadProgress);
                FindViewById<ImageView>(Resource.Id.FairyDBDetailSmallImage).Click += FairyDBDetailSmallImage_Click;

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FairyDBSnackbarLayout);

                InitLoadProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
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
                Toast.MakeText(this, "이미지 뷰어 실행 실패!", ToastLength.Long).Show();
            }
        }

        private async Task InitLoadProcess()
        {
            InitLoadProgressBar.Visibility = ViewStates.Visible;

            await Task.Delay(100);

            try
            {
                // 요정 타이틀 바 초기화

                if (File.Exists(Path.Combine(ETC.CachePath, "Fairy", "Normal", FairyName + "_1" + ".gfdcache")) == false)
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
                if (File.Exists(Path.Combine(ETC.CachePath, "Fairy", "Skill", SkillName + ".gfdcache")) == false)
                {
                    using (WebClient wc = new WebClient())
                    {
                        await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "FairySkill", SkillName + ".png"), Path.Combine(ETC.CachePath, "Fairy", "Skill", SkillName + ".gfdcache"));
                    }
                }

                FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillIcon).SetImageDrawable(Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Fairy", "Skill", SkillName + ".gfdcache")));

                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillName).Text = SkillName;

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillTicketIcon).SetImageResource(Resource.Drawable.FairyTicket_Icon_WhiteTheme);
                    FindViewById<ImageView>(Resource.Id.FairyDBDetailSkillCoolTimeIcon).SetImageResource(Resource.Drawable.CoolTime_Icon_WhiteTheme);
                }

                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillTicket).Text = ((int)FairyInfoDR["OrderConsume"]).ToString();
                FindViewById<TextView>(Resource.Id.FairyDBDetailSkillCoolTime).Text = ((int)FairyInfoDR["CoolDown"]).ToString() + "턴";
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

                    MaxValue = Int32.Parse((((string)FairyInfoDR[abilities[i]]).Split('/'))[1]);

                    ETC.UpProgressBarProgress(FindViewById<ProgressBar>(Progresses[i]), 0, MaxValue, delay);
                    FindViewById<TextView>(StatusTexts[i]).Text = ((string)FairyInfoDR[abilities[i]]);
                }


                if (ETC.UseLightTheme == true) SetCardTheme();
                ShowCardViewAnimation();
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