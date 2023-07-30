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

using Plugin.SimpleAudioPlayer;

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFDA
{
    [Activity(Label = "", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EnemyDBDetailActivity : BaseAppCompatActivity
    {
        Enemy enemy = null;

        private int enemyTypeIndex = 0;

        private ISimpleAudioPlayer voicePlayer;
        private FileStream stream;

        private CoordinatorLayout snackbarLayout;

        private SwipeRefreshLayout refreshMainLayout;
        private AndroidX.AppCompat.Widget.Toolbar toolbar;
        private Spinner typeSelector;
        private Spinner voiceSelector;
        private Button voicePlayButton;

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
                SetContentView(Resource.Layout.EnemyDBDetailLayout);

                enemy = new Enemy(ETC.FindDataRow(ETC.enemyList, "CodeName", Intent.GetStringExtra("Keyword")));

                voicePlayer = CrossSimpleAudioPlayer.Current;

                refreshMainLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.EnemyDBDetailMainRefreshLayout);
                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.EnemyDBDetailMainToolbar);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                FindViewById<TextView>(Resource.Id.EnemyDBDetailToolbarAffiliation).Text = enemy.Affiliation;
                FindViewById<TextView>(Resource.Id.EnemyDBDetailToolbarName).Text = enemy.Name;

                refreshMainLayout.Refresh += async delegate { await InitLoadProcess(true); };

                FindViewById<ImageView>(Resource.Id.EnemyDBDetailSmallImage).Click += EnemyDBDetailSmallImage_Click;
                typeSelector = FindViewById<Spinner>(Resource.Id.EnemyDBDetailEnemyTypeSelector);
                typeSelector.ItemSelected += TypeSelector_ItemSelected;

                var tAdapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, enemy.Types);
                tAdapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                typeSelector.Adapter = tAdapter;

                if (enemy.HasVoice)
                {
                    FindViewById<LinearLayout>(Resource.Id.EnemyDBDetailVoiceLayout).Visibility = ViewStates.Visible;
                    voiceSelector = FindViewById<Spinner>(Resource.Id.EnemyDBDetailVoiceSelector);
                    voicePlayButton = FindViewById<Button>(Resource.Id.EnemyDBDetailVoicePlayButton);
                    voicePlayButton.Click += delegate { _ = PlayVoiceProcess(); };
                }

                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EnemyDBDetailSnackbarLayout);

                await InitializeProcess();
                await InitLoadProcess(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async Task InitializeProcess()
        {
            await Task.Delay(100);

            try
            {
                if (enemy.HasVoice)
                {
                    InitializeVoiceList();
                }

                var toolbarColor = enemy.Affiliation switch
                {
                    "SANGVIS FERRI" => Android.Graphics.Color.ParseColor("#441111"),
                    "Mind Map System" => Android.Graphics.Color.ParseColor("#262626"),
                    "KCCO" => Android.Graphics.Color.ParseColor("#47661E"),
                    "Paradeus" => Android.Graphics.Color.ParseColor("#00008B"),
                    "E.L.I.D." => Android.Graphics.Color.ParseColor("#441111"),
                    _ => Android.Graphics.Color.ParseColor("#262626")
                };
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

        private async Task PlayVoiceProcess()
        {
            await Task.Delay(100);

            string voice = enemy.Voices[voiceSelector.SelectedItemPosition];

            string voiceServerURL = Path.Combine(ETC.server, "Data", "Voice", "Enemy", enemy.CodeName, $"{enemy.CodeName}_{voice}_JP.wav");
            string target = Path.Combine(ETC.cachePath, "Voices", "Enemy", $"{enemy.CodeName}_{voice}_JP.gfdcache");

            try
            {
                stream?.Dispose();

                if (!File.Exists(target))
                {
                    using (WebClient wc = new WebClient())
                    {
                        MainThread.BeginInvokeOnMainThread(() => { voicePlayButton.Enabled = false; });

                        wc.DownloadProgressChanged += (sender, e) =>
                        {
                            MainThread.BeginInvokeOnMainThread(() => { voicePlayButton.Text = $"{e.ProgressPercentage}%"; });
                        };

                        await wc.DownloadFileTaskAsync(voiceServerURL, target);
                    }

                    await Task.Delay(500);
                }

                stream = new FileStream(target, FileMode.Open, FileAccess.Read);

                voicePlayer.Load(stream);
                voicePlayer.Play();
            }
            catch (WebException ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.VoiceStreaming_DownloadError, Snackbar.LengthShort, Android.Graphics.Color.DarkCyan);

                if (File.Exists(target))
                {
                    File.Delete(target);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.VoiceStreaming_PlayError, Snackbar.LengthShort, Android.Graphics.Color.DarkCyan);

                if (File.Exists(target))
                {
                    File.Delete(target);
                }
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    voicePlayButton.Enabled = true;
                    voicePlayButton.SetText(Resource.String.Common_Play);
                });
            }
        }

        private void InitializeVoiceList()
        {
            try
            {
                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, enemy.Voices);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
                voiceSelector.Adapter = adapter;

                voiceSelector.SetSelection(0);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.VoiceList_InitError, ToastLength.Short).Show();
            }
        }

        private void TypeSelector_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            enemyTypeIndex = e.Position;

            _ = InitLoadProcess(false);
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


                // 철혈 타이틀 바 초기화

                if (Preferences.Get("DBDetailBackgroundImage", false))
                {
                    string imagePath = Path.Combine(ETC.cachePath, "Enemy", "Normal", $"{enemy.CodeName}.gfdcache");

                    if (!File.Exists(imagePath) || isRefresh)
                    {
                        using (var wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.server, "Data", "Images", "Enemy", "Normal", $"{enemy.CodeName}.png"), imagePath);
                        }
                    }

                    var drawable = Drawable.CreateFromPath(imagePath);
                    drawable.SetAlpha(40);

                    FindViewById<ImageView>(Resource.Id.EnemyDBDetailBackgroundImageView).SetImageDrawable(drawable);
                }

                string cropImagePath = Path.Combine(ETC.cachePath, "Enemy", "Normal_Crop", $"{enemy.CodeName}.gfdcache");

                if (!File.Exists(cropImagePath) || isRefresh)
                {
                    using (WebClient wc = new WebClient())
                    {
                        await wc.DownloadFileTaskAsync(Path.Combine(ETC.server, "Data", "Images", "Enemy", "Normal_Crop", $"{enemy.CodeName}.png"), cropImagePath);
                    }
                }

                FindViewById<ImageView>(Resource.Id.EnemyDBDetailSmallImage).SetImageDrawable(Drawable.CreateFromPath(cropImagePath));

                TextView type = FindViewById<TextView>(Resource.Id.EnemyDBDetailType);

                if (enemy.IsBoss)
                {
                    FindViewById<TextView>(Resource.Id.EnemyDBDetailType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Boss);
                }
                else
                {
                    FindViewById<TextView>(Resource.Id.EnemyDBDetailType).Text = Resources.GetString(Resource.String.EnemyDBDetail_Normal);
                }

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
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoEnemyAffiliation).Text = enemy.Affiliation;
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoName).Text = enemy.Name;
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoVoiceActor).Text = "";
                FindViewById<TextView>(Resource.Id.EnemyDBDetailInfoETC).Text = enemy.Note;


                // 철혈 능력치 초기화

                string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed", "Penetration", "Armor", "Range" };
                int[] Progresses = { Resource.Id.EnemyInfoHPProgress, Resource.Id.EnemyInfoFRProgress, Resource.Id.EnemyInfoEVProgress, Resource.Id.EnemyInfoACProgress, Resource.Id.EnemyInfoASProgress, Resource.Id.EnemyInfoPTProgress, Resource.Id.EnemyInfoAMProgress, Resource.Id.EnemyInfoRangeProgress };
                int[] ProgressMaxTexts = { Resource.Id.EnemyInfoHPProgressMax, Resource.Id.EnemyInfoFRProgressMax, Resource.Id.EnemyInfoEVProgressMax, Resource.Id.EnemyInfoACProgressMax, Resource.Id.EnemyInfoASProgressMax, Resource.Id.EnemyInfoPTProgressMax, Resource.Id.EnemyInfoAMProgressMax, Resource.Id.EnemyInfoRangeProgressMax };
                int[] StatusTexts = { Resource.Id.EnemyInfoHPStatus, Resource.Id.EnemyInfoFRStatus, Resource.Id.EnemyInfoEVStatus, Resource.Id.EnemyInfoACStatus, Resource.Id.EnemyInfoASStatus, Resource.Id.EnemyInfoPTStatus, Resource.Id.EnemyInfoAMStatus, Resource.Id.EnemyInfoRangeStatus };

                for (int i = 0; i < Progresses.Length; ++i)
                {
                    FindViewById<TextView>(ProgressMaxTexts[i]).Text = FindViewById<ProgressBar>(Progresses[i]).Max.ToString();

                    int value = enemy.Abilities[enemyTypeIndex][abilities[i]];

                    FindViewById<ProgressBar>(Progresses[i]).Progress = value;
                    FindViewById<TextView>(StatusTexts[i]).Text = value.ToString();
                }

                SetCardTheme();
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

        private void SetCardTheme()
        {
            int[] CardViewIds =
            {
                Resource.Id.EnemyDBDetailEnemyTypeSelectCardLayout,
                Resource.Id.EnemyDBDetailBasicInfoCardLayout,
                Resource.Id.EnemyDBDetailAbilityCardLayout
            };

            foreach (int id in CardViewIds)
            {
                var cardView = FindViewById<CardView>(id);

                cardView.Alpha = 0.7f;

                if (ETC.useLightTheme)
                {
                    cardView.Background = new ColorDrawable(Android.Graphics.Color.WhiteSmoke);
                    cardView.Radius = 15.0f;
                }
            }
        }

        private void ShowCardViewVisibility()
        {
            FindViewById<CardView>(Resource.Id.EnemyDBDetailEnemyTypeSelectCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.EnemyDBDetailBasicInfoCardLayout).Visibility = ViewStates.Visible;
            FindViewById<CardView>(Resource.Id.EnemyDBDetailAbilityCardLayout).Visibility = ViewStates.Visible;
        }

        public override void OnBackPressed()
        {
            if (voicePlayer.IsPlaying)
            {
                voicePlayer.Stop();
            }

            stream?.Dispose();

            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }
    }
}