using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;

using Plugin.SimpleAudioPlayer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFDA
{
    internal static class MusicRepo
    {
        internal static ISimpleAudioPlayer ostPlayer;
        internal static bool isOSTLoad = false;
        internal static bool isReload = false;

        internal static readonly string serverMusicPath = Path.Combine(ETC.server, "Data", "Music");
        internal static readonly string localMusicCachePath = Path.Combine(ETC.cachePath, "OST");

        internal static string musicServerPath = "";
        internal static int categoryIndex = 0;
        internal static int itemIndex = 0;

        internal static string[] categoryList;
        internal static string[] categoryRealPath;
        internal static List<string> itemList = new List<string>();
    }

    [Activity(Name = "com.gfl.dic.OSTPlayer", Label = "OST (Beta)", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GFOSTPlayerActivity : BaseAppCompatActivity
    {
        private bool isCategory = true;

        private ArrayAdapter categoryAdapter;
        private AndroidX.Fragment.App.FragmentTransaction ft;
        private AndroidX.Fragment.App.Fragment gfOSTPlayerScreenF;

        private DrawerLayout mainDrawerLayout;
        private ListView drawerListView;

        private FileStream stream;

        private string musicServerPath = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.GFOSTPlayer);

            if (MusicRepo.ostPlayer == null)
            {
                MusicRepo.ostPlayer = CrossSimpleAudioPlayer.Current;
            }

            mainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.GFOSTPlayerMainDrawerLayout);
            mainDrawerLayout.DrawerOpened += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen); };
            mainDrawerLayout.DrawerClosed += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu); };
            drawerListView = FindViewById<ListView>(Resource.Id.GFOSTPlayerNavigationListView);
            drawerListView.ItemClick += DrawerListView_ItemSelected;

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.GFOSTPlayerToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);

            gfOSTPlayerScreenF = new GFOSTPlayerScreen();

            ft = SupportFragmentManager.BeginTransaction();
            ft.Add(Resource.Id.GFOSTPlayerMainLayout, gfOSTPlayerScreenF, "GFOSTPlayerScreen");
            ft.Commit();

            LoadCategoryList();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
                    {
                        mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    }
                    else
                    {
                        mainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    }
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void LoadCategoryList()
        {
            try
            {
                if (MusicRepo.categoryList == null)
                {
                    MusicRepo.categoryList = ETC.Resources.GetStringArray(Resource.Array.MusicCategory);
                }
                if (MusicRepo.categoryRealPath == null)
                {
                    MusicRepo.categoryRealPath = ETC.Resources.GetStringArray(Resource.Array.MusicCategory_RealPath);
                }

                categoryAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, MusicRepo.categoryList);
                drawerListView.Adapter = categoryAdapter;

                mainDrawerLayout.OpenDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private void ListItems()
        {
            try
            {
                int arrayId = MusicRepo.categoryIndex switch
                {
                    0 => Resource.Array.Normal,
                    1 => Resource.Array.ContinuumTurbulence,
                    2 => Resource.Array.Cube,
                    3 => Resource.Array.DeepDive,
                    4 => Resource.Array.DJMAX,
                    5 => Resource.Array.GuiltyGear,
                    6 => Resource.Array.Houkai2,
                    7 => Resource.Array.Arctic,
                    8 => Resource.Array.Singularity,
                    9 => Resource.Array.Isomer,
                    10 => Resource.Array.VA,
                    11 => Resource.Array.ShatteredConnexion,
                    12 => Resource.Array.PhantomSyndrome,
                    13 => Resource.Array.HolyNightRhapsody,
                    14 => Resource.Array.PolarizedLight,
                    _ => 0,
                };

                MusicRepo.itemList.AddRange(ETC.Resources.GetStringArray(arrayId));
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }

        private void DrawerListView_ItemSelected(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                if (isCategory)
                {
                    MusicRepo.categoryIndex = e.Position;

                    MusicRepo.itemList.Clear();
                    MusicRepo.itemList.Add("...");

                    ListItems();
                    MusicRepo.itemList.TrimExcess();

                    var itemAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, MusicRepo.itemList);
                    drawerListView.Adapter = itemAdapter;

                    isCategory = false;
                    MusicRepo.isOSTLoad = true;
                }
                else
                {
                    switch (e.Position)
                    {
                        case 0:
                            drawerListView.Adapter = categoryAdapter;
                            isCategory = true;
                            break;
                        default:
                            MusicRepo.itemIndex = e.Position;
                            _ = LoadMusic();

                            //((GFOSTPlayerScreen)gfOSTPlayerScreenF).CommandService("Start");

                            mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        internal async Task LoadMusic()
        {
            await Task.Delay(100);

            try
            {
                stream?.Dispose();

                string category = MusicRepo.categoryRealPath[MusicRepo.categoryIndex];
                string item = MusicRepo.itemList[MusicRepo.itemIndex];

                musicServerPath = Path.Combine(MusicRepo.serverMusicPath, "OST", category, $"{item}.mp3");

                string locaclFileName = $"{category}_{item}.mp3";

                using (var wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(musicServerPath, Path.Combine(MusicRepo.localMusicCachePath, locaclFileName));
                }

                stream = new FileStream(Path.Combine(MusicRepo.localMusicCachePath, locaclFileName), FileMode.Open, FileAccess.Read);

                (gfOSTPlayerScreenF as GFOSTPlayerScreen).ChangeMusicAlbum();

                MusicRepo.ostPlayer.Load(stream);
                MusicRepo.ostPlayer.Play();

                _ = (gfOSTPlayerScreenF as GFOSTPlayerScreen).infoMethod();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        internal void PlayPauseMusic()
        {
            try
            {
                if (MusicRepo.ostPlayer.IsPlaying)
                {
                    MusicRepo.ostPlayer.Pause();
                }
                else
                {
                    MusicRepo.ostPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        public override void OnBackPressed()
        {
            if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                mainDrawerLayout.CloseDrawer(GravityCompat.Start);

                return;
            }
            else
            {
                base.OnBackPressed();
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
        }
    }

    public class GFOSTPlayerScreen : AndroidX.Fragment.App.Fragment
    {
        internal delegate Task InfoMethod();

        private View v;

        private GFOSTPlayerActivity ostActivity;

        private ImageView musicAlbumArt;
        private TextView musicName;
        private SeekBar musicSeekBar;
        private TextView musicLengthText;
        private TextView musicNowPosition;
        private ImageView skipPreviousButton;
        private ImageView playPauseButton;
        private ImageView skipNextButton;

        internal InfoMethod infoMethod;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater?.Inflate(Resource.Layout.GFOSTPlayerScreen, container, false);

            ostActivity = Activity as GFOSTPlayerActivity;

            musicAlbumArt = v.FindViewById<ImageView>(Resource.Id.GFOSTPlayerMusicAlbumArt);
            musicName = v.FindViewById<TextView>(Resource.Id.GFOSTPlayerMusicName);
            musicSeekBar = v.FindViewById<SeekBar>(Resource.Id.GFOSTPlayerMusicSeekBar);
            musicSeekBar.StopTrackingTouch += (sender, e) =>
            {
                try
                {
                    MusicRepo.ostPlayer.Seek((double)e.SeekBar.Progress / 100 * MusicRepo.ostPlayer.Duration);
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, Activity);
                }
            };
            musicLengthText = v.FindViewById<TextView>(Resource.Id.GFOSTPlayerMusicLengthText);
            musicNowPosition = v.FindViewById<TextView>(Resource.Id.GFOSTPlayerMusicNowPositionText);

            skipPreviousButton = v.FindViewById<ImageView>(Resource.Id.GFOSTPlayerSkipPreviousButton);
            skipPreviousButton.Clickable = true;
            skipPreviousButton.Click += MusicControlButton_Click;
            playPauseButton = v.FindViewById<ImageView>(Resource.Id.GFOSTPlayerPlayPauseButton);
            playPauseButton.Clickable = true;
            playPauseButton.Click += MusicControlButton_Click;
            skipNextButton = v.FindViewById<ImageView>(Resource.Id.GFOSTPlayerSkipNextButton);
            skipNextButton.Clickable = true;
            skipNextButton.Click += MusicControlButton_Click;

            if (ETC.useLightTheme)
            {
                skipPreviousButton.SetImageResource(Resource.Drawable.SkipPrevious_WhiteTheme);
                playPauseButton.SetImageResource(Resource.Drawable.PlayPause_WhiteTheme);
                skipNextButton.SetImageResource(Resource.Drawable.SkipNext_WhiteTheme);
            }
            else
            {
                skipPreviousButton.SetImageResource(Resource.Drawable.SkipPrevious);
                playPauseButton.SetImageResource(Resource.Drawable.PlayPause);
                skipNextButton.SetImageResource(Resource.Drawable.SkipNext);
            }

            infoMethod = new InfoMethod(RefreshInfo);

            if (MusicRepo.isOSTLoad && MusicRepo.ostPlayer.IsPlaying)
            {
                ChangeMusicAlbum();
                infoMethod();
            }

            return v;
        }

        private void MusicControlButton_Click(object sender, EventArgs e)
        {
            try
            {
                var iv = sender as ImageView;

                switch (iv.Id)
                {
                    case Resource.Id.GFOSTPlayerSkipPreviousButton:
                        MusicRepo.ostPlayer.Stop();

                        if (MusicRepo.itemIndex > 1)
                        {
                            MusicRepo.itemIndex -= 1;
                        }

                        _ = ostActivity.LoadMusic();
                        break;
                    case Resource.Id.GFOSTPlayerPlayPauseButton:
                        ostActivity.PlayPauseMusic();
                        break;
                    case Resource.Id.GFOSTPlayerSkipNextButton:
                        MusicRepo.ostPlayer.Stop();

                        if (MusicRepo.itemIndex < MusicRepo.itemList.Count)
                        {
                            MusicRepo.itemIndex += 1;
                        }

                        _ = ostActivity.LoadMusic();
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        internal void ChangeMusicAlbum()
        {
            try
            {
                int resId = 0;

                switch (MusicRepo.categoryIndex)
                {
                    case 0:
                        resId = Resource.Drawable.Album_Normal;
                        break;
                    case 1:
                        resId = Resource.Drawable.Album_ContinuumTurbulence;
                        break;
                    case 2:
                        resId = Resource.Drawable.Album_Cube;
                        break;
                    case 3:
                        resId = Resource.Drawable.Album_DeepDive;
                        break;
                    case 4:
                        resId = Resource.Drawable.Album_DJMAX;
                        break;
                    case 5:
                        resId = Resource.Drawable.Album_HuntingRabbit;
                        break;
                    case 6:
                        resId = Resource.Drawable.Album_Houkai2;
                        break;
                    case 7:
                        resId = Resource.Drawable.Album_Arctic;
                        break;
                    case 8:
                        resId = Resource.Drawable.Album_Singularity;
                        break;
                    case 9:
                        resId = Resource.Drawable.Album_Isomer;
                        break;
                    case 10:
                        resId = Resource.Drawable.Album_VA;
                        break;
                    case 11:
                        resId = Resource.Drawable.Album_ShatteredConnexion;
                        break;
                    case 12:
                        resId = Resource.Drawable.Album_PhantomSyndrome;
                        break;
                    case 13:
                        resId = Resource.Drawable.Album_HolyNightRhapsody;
                        break;
                    case 14:
                        resId = Resource.Drawable.Album_PolarizedLight;
                        break;
                }

                musicAlbumArt.SetImageResource(resId);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                musicAlbumArt.SetImageDrawable(null);
            }
        }

        private async Task RefreshInfo()
        {
            try
            { 
                while (true)
                {
                    if (MusicRepo.isOSTLoad && MusicRepo.ostPlayer.IsPlaying)
                    {
                        musicName.Text = MusicRepo.itemList[MusicRepo.itemIndex];
                        musicLengthText.Text = MusicRepo.ostPlayer.Duration.ToString("F0");
                        musicNowPosition.Text = MusicRepo.ostPlayer.CurrentPosition.ToString("F0");
                        musicSeekBar.Progress = Convert.ToInt32(Math.Ceiling(MusicRepo.ostPlayer.CurrentPosition / MusicRepo.ostPlayer.Duration * 100));
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(Activity, "Refresh Error", ToastLength.Short).Show();
                ETC.LogError(ex, Activity);
            }
        }

        /*internal void CommandService(string command)
        {
            try
            {
                var intent = new Intent(Activity, typeof(GFOSTService));
                intent.PutExtra("Command", command);
                Activity.StartService(intent);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }*/
    }

    /*[Service]
    public class GFOSTService : Service
    {
        public override void OnCreate()
        {
            base.OnCreate();
        }

        private bool CheckInit()
        {
            try
            {
                if (MusicRepository.player == null) 
                    return true;

                foreach (List<string> list in MusicRepository.itemList)
                {
                    if (list.Count == 0) return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                return true;
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            string command = intent.Extras.GetString("Command");

            switch (command)
            {
                case "Play":
                    MusicRepository.player.Start();
                    MusicRepository.IsPlayerPlaying = true;
                    MusicRepository.StartSeekBarTask();
                    break;
                case "Start":
                case "SkipPrevious":
                case "SkipNext":
                    LoadMusic(MusicRepository.musicServerPath, true);
                    MusicRepository.IsPlayerPlaying = true;
                    MusicRepository.StartSeekBarTask();
                    break;
                case "Pause":
                    MusicRepository.player.Pause();
                    MusicRepository.IsPlayerPlaying = false;
                    break;
            }

            return base.OnStartCommand(intent, flags, startId);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            MusicRepository.IsPlayerPlaying = false;
            MusicRepository.player.Release();
            MusicRepository.player.Dispose();
            ETC.ostPlayer.Release();
            ETC.ostPlayer = null;
        }

        private void LoadMusic(string path, bool IsChange)
        {
            try
            {
                if (IsChange == true)
                {
                    MusicRepository.player.Stop();
                    MusicRepository.player.Reset();
                    MusicRepository.IsPlayerPlaying = false;
                }

                using (TimeOutWebClient wc = new TimeOutWebClient())
                {
                    await wc.DownloadFileTaskAsync(path, Path.Combine(ETC.CachePath, "test.mp3"));
                }

                MusicRepository.player.SetDataSource(path);
                MusicRepository.player.Prepare();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }
    }*/
}