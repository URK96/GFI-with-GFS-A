using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Media;
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
        Enemy enemy = null;

        private int EnemyTypeIndex = 0;
        private bool IsExtraFeatureOpen = false;

        private CoordinatorLayout SnackbarLayout;

        private ProgressBar InitLoadProgressBar;
        private Spinner TypeSelector;
        private Button ExtraMenuButton;
        private Spinner VoiceSelector;
        private Button VoicePlayButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.EnemyDBDetailLayout);

                enemy = new Enemy(ETC.FindDataRow(ETC.EnemyList, "CodeName", Intent.GetStringExtra("Keyword")));

                InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.EnemyDBDetailInitLoadProgress);

                FindViewById<ImageView>(Resource.Id.EnemyDBDetailSmallImage).Click += EnemyDBDetailSmallImage_Click;

                TypeSelector = FindViewById<Spinner>(Resource.Id.EnemyDBDetailEnemyTypeSelector);
                TypeSelector.ItemSelected += TypeSelector_ItemSelected;

                var t_adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, enemy.Types);
                t_adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                TypeSelector.Adapter = t_adapter;

                ExtraMenuButton = FindViewById<Button>(Resource.Id.EnemyDBDetailExtraFeatureButton);
                ExtraMenuButton.Click += ExtraMenuButton_Click;

                if (enemy.HasVoice == true)
                {
                    ExtraMenuButton.Visibility = ViewStates.Visible;
                    FindViewById<LinearLayout>(Resource.Id.EnemyDBDetailVoiceLayout).Visibility = ViewStates.Visible;
                    VoiceSelector = FindViewById<Spinner>(Resource.Id.EnemyDBDetailVoiceSelector);
                    VoicePlayButton = FindViewById<Button>(Resource.Id.EnemyDBDetailVoicePlayButton);
                    VoicePlayButton.Click += VoicePlayButton_Click;
                    InitializeVoiceList();
                }

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EnemyDBDetailSnackbarLayout);

                InitLoadProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void ExtraMenuButton_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;

            switch (IsExtraFeatureOpen)
            {
                case false:
                    IsExtraFeatureOpen = true;
                    b.Text = "△△△";
                    FindViewById<LinearLayout>(Resource.Id.EnemyDBDetailExtraFeatureLayout).Visibility = ViewStates.Visible;
                    break;
                case true:
                    IsExtraFeatureOpen = false;
                    b.Text = "▽▽▽";
                    FindViewById<LinearLayout>(Resource.Id.EnemyDBDetailExtraFeatureLayout).Visibility = ViewStates.Gone;
                    break;
            }
        }

        private async void VoicePlayButton_Click(object sender, EventArgs e)
        {
            ProgressBar pb = FindViewById<ProgressBar>(Resource.Id.EnemyDBDetailVoiceDownloadProgress);

            try
            {
                pb.Visibility = ViewStates.Visible;
                pb.Indeterminate = true;

                string voice = enemy.Voices[VoiceSelector.SelectedItemPosition];

                string VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", "Enemy", enemy.CodeName, $"{enemy.CodeName}_{voice}_JP.wav");
                string target = Path.Combine(ETC.CachePath, "Voices", "Enemy", $"{enemy.CodeName}_{voice}_JP.gfdcache");

                /*switch (V_Costume_Index)
                {
                    case 0:
                        VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", doll.krName, $"{doll.krName}_{voice}_JP.wav");
                        target = Path.Combine(ETC.CachePath, "Voices", $"{doll.DicNumber}_{voice}_JP.gfdcache");
                        break;
                    case 1:
                        VoiceServerURL = Path.Combine(ETC.Server, "Data", "Voice", $"{doll.krName}_{V_Costume_Index - 1}", $"{doll.krName}_{V_Costume_Index - 1}_{voice}_JP.wav");
                        target = Path.Combine(ETC.CachePath, "Voices", $"{doll.DicNumber}_{V_Costume_Index - 1}_{voice}_JP.gfdcache");
                        break;
                }*/

                MediaPlayer SoundPlayer = new MediaPlayer();
                SoundPlayer.Completion += delegate { SoundPlayer.Release(); };

                await Task.Delay(100);

                try
                {
                    SoundPlayer.SetDataSource(target);
                }
                catch (Exception)
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += (object s, DownloadProgressChangedEventArgs args) =>
                        {
                            pb.Indeterminate = false;
                            pb.Progress = args.ProgressPercentage;
                        };
                        await wc.DownloadFileTaskAsync(VoiceServerURL, target);
                    }

                    SoundPlayer.SetDataSource(target);
                }

                pb.Visibility = ViewStates.Invisible;

                SoundPlayer.Prepare();
                SoundPlayer.Start();
            }
            catch (WebException ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.VoiceStreaming_Error, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.VoiceStreaming_PlayError, Snackbar.LengthShort, Android.Graphics.Color.DarkCyan);
            }
        }

        private void InitializeVoiceList()
        {
            try
            {
                var v_adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, enemy.Voices);
                v_adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                VoiceSelector.Adapter = v_adapter;

                VoiceSelector.SetSelection(0);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.VoiceList_InitError, ToastLength.Short).Show();
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
                EnemyImageViewer.PutExtra("Data", enemy.CodeName);
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
                //if (ListingComplete == false) await InitializeTypeList();

                // 철혈 타이틀 바 초기화

                if (ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false) == true)
                {
                    string image_path = Path.Combine(ETC.CachePath, "Enemy", "Normal", $"{enemy.CodeName}.gfdcache");

                    if (File.Exists(image_path) == false)
                        using (WebClient wc = new WebClient())
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Enemy", "Normal", $"{enemy.CodeName}.png"), image_path);

                    Drawable drawable = Drawable.CreateFromPath(image_path);
                    drawable.SetAlpha(40);
                    FindViewById<RelativeLayout>(Resource.Id.EnemyDBDetailMainLayout).Background = drawable;
                }

                string cropimage_path = Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", $"{enemy.CodeName}.gfdcache");

                if (File.Exists(cropimage_path) == false)
                    using (WebClient wc = new WebClient())
                        await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Enemy", "Normal_Crop", $"{enemy.CodeName}.png"), cropimage_path);

                FindViewById<ImageView>(Resource.Id.EnemyDBDetailSmallImage).SetImageDrawable(Drawable.CreateFromPath(cropimage_path));

                if (enemy.IsBoss == true)
                    FindViewById<TextView>(Resource.Id.EnemyDBDetailType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Boss);
                else
                    FindViewById<TextView>(Resource.Id.EnemyDBDetailType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Normal);
                FindViewById<TextView>(Resource.Id.EnemyDBDetailEnemyName).Text = enemy.Name;
                FindViewById<TextView>(Resource.Id.EnemyDBDetailEnemyCodeName).Text = enemy.CodeName;


                // 철혈 기본 정보 초기화

                int GradeIconId = 0;

                switch (enemy.IsBoss)
                {
                    case true:
                        GradeIconId = Resource.Drawable.Type_Boss;
                        break;
                    case false:
                        GradeIconId = Resource.Drawable.Type_Normal;
                        break;
                }
                FindViewById<ImageView>(Resource.Id.EnemyDBDetailInfoGrade).SetImageResource(GradeIconId);

                if (enemy.IsBoss == true)
                    FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoEnemyType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Boss);
                else
                    FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoEnemyType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Normal);
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoName).Text = enemy.Name;
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoCodeName).Text = enemy.CodeName;
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoVoiceActor).Text = "";
                if (enemy.IsBoss == true)
                    FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoAppearPlace).Text = enemy.Types[EnemyTypeIndex];


                // 철혈 능력치 초기화

                string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed", "Penetration", "Armor", "Range" };
                int[] Progresses = { Resource.Id.EnemyInfoHPProgress, Resource.Id.EnemyInfoFRProgress, Resource.Id.EnemyInfoEVProgress, Resource.Id.EnemyInfoACProgress, Resource.Id.EnemyInfoASProgress, Resource.Id.EnemyInfoPTProgress, Resource.Id.EnemyInfoAMProgress, Resource.Id.EnemyInfoRangeProgress };
                int[] ProgressMaxTexts = { Resource.Id.EnemyInfoHPProgressMax, Resource.Id.EnemyInfoFRProgressMax, Resource.Id.EnemyInfoEVProgressMax, Resource.Id.EnemyInfoACProgressMax, Resource.Id.EnemyInfoASProgressMax, Resource.Id.EnemyInfoPTProgressMax, Resource.Id.EnemyInfoAMProgressMax, Resource.Id.EnemyInfoRangeProgressMax };
                int[] StatusTexts = { Resource.Id.EnemyInfoHPStatus, Resource.Id.EnemyInfoFRStatus, Resource.Id.EnemyInfoEVStatus, Resource.Id.EnemyInfoACStatus, Resource.Id.EnemyInfoASStatus, Resource.Id.EnemyInfoPTStatus, Resource.Id.EnemyInfoAMStatus, Resource.Id.EnemyInfoRangeStatus };

                for (int i = 0; i < Progresses.Length; ++i)
                {
                    FindViewById<TextView>(ProgressMaxTexts[i]).Text = FindViewById<ProgressBar>(Progresses[i]).Max.ToString();

                    int value = enemy.Abilities[EnemyTypeIndex][abilities[i]];

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

        /*private async Task InitializeTypeList()
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
        }*/

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
            if (FindViewById<CardView>(Resource.Id.EnemyDBDetailAbilityCardLayout).Alpha == 0.0f) FindViewById<CardView>(Resource.Id.EnemyDBDetailAbilityCardLayout).Animate().Alpha(1.0f).SetDuration(500).SetStartDelay(1000).Start();
        }

    }
}