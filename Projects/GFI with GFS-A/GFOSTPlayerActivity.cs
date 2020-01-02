using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Plugin.SimpleAudioPlayer;

namespace GFI_with_GFS_A
{
    /*internal static class MusicRepository
    {
        private static readonly string serverMusicPath = Path.Combine(ETC.server, "Data", "Music");

        internal static string musicServerPath = "";
        internal static int categoryIndex = 0;
        internal static int itemIndex = 0;

        internal static string[] categoryList;
        internal static string[] categoryRealPath =
        {
            "Normal",
            "Continuum Turbulence",
            "Cube",
            "Deep Dive",
            "DJMAX",
            "GuiltyGear",
            "Houkai2",
            "Hypothermia",
            "Singularity"
        };
        internal static List<List<string>> itemList = new List<List<string>>();

        private static void ListCategory()
        {
            categoryList = new string[]
            {
                ETC.Resources.GetString(Resource.String.Music_Category_Normal),
                ETC.Resources.GetString(Resource.String.Music_Category_ContinuumTurbulence),
                ETC.Resources.GetString(Resource.String.Music_Category_Cube),
                ETC.Resources.GetString(Resource.String.Music_Category_DeepDive),
                ETC.Resources.GetString(Resource.String.Music_Category_DJMAX),
                ETC.Resources.GetString(Resource.String.Music_Category_GuiltyGear),
                ETC.Resources.GetString(Resource.String.Music_Category_Houkai2),
                ETC.Resources.GetString(Resource.String.Music_Category_Hypothermia),
                ETC.Resources.GetString(Resource.String.Music_Category_Singularity)
            };
        }

        internal static void MusicItemInitialize()
        {
            try
            {
                itemList.Clear();

                for (int i = 0; i < categoryList.Length; ++i)
                {
                    int arrayId = 0;

                    categoryIndex = i;

                    switch (i)
                    {
                        case 0:
                            arrayId = Resource.Array.Normal;
                            break;
                        case 1:
                            arrayId = Resource.Array.ContinuumTurbulence;
                            break;
                        case 2:
                            arrayId = Resource.Array.Cube;
                            break;
                        case 3:
                            arrayId = Resource.Array.DeepDive;
                            break;
                        case 4:
                            arrayId = Resource.Array.DJMAX;
                            break;
                        case 5:
                            arrayId = Resource.Array.GuiltyGear;
                            break;
                        case 6:
                            arrayId = Resource.Array.Houkai2;
                            break;
                        case 7:
                            arrayId = Resource.Array.Hypothermia;
                            break;
                        case 8:
                            arrayId = Resource.Array.Singularity;
                            break;
                    }

                    List<string> list = new List<string>();

                    list.Add("...");
                    list.AddRange(ETC.Resources.GetStringArray(arrayId));
                    list.TrimExcess();

                    itemList.Add(list);
                }

                itemList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }

        internal static void SelectMusic()
        {
            try
            {
                string fileName = string.Format("{0}.mp3", itemList[categoryIndex][itemIndex]);
                musicServerPath = Path.Combine(serverMusicPath, "OST", categoryRealPath[categoryIndex], fileName);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }
    }

    [Activity(Name = "com.gfl.dic.OSTPlayer", Label = "OST (Beta)", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GFOSTPlayerActivity : BaseAppCompatActivity
    {
        private bool isCategory = true;

        private ArrayAdapter categoryAdapter;
        private Android.Support.V4.App.FragmentTransaction ft;
        private Android.Support.V4.App.Fragment gfOSTPlayerScreenF;

        private DrawerLayout mainDrawerLayout;
        private ListView drawerListView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.GFOSTPlayer);

            if (ETC.ostPlayer == null)
            {
                ETC.ostPlayer = CrossSimpleAudioPlayer.Current;
            }

            mainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.GFOSTPlayerMainDrawerLayout);
            mainDrawerLayout.DrawerOpened += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen); };
            mainDrawerLayout.DrawerClosed += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu); };
            drawerListView = FindViewById<ListView>(Resource.Id.GFOSTPlayerNavigationListView);
            drawerListView.ItemClick += DrawerListView_ItemSelected;

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.GFOSTPlayerToolbar));
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
                MusicRepository.MusicItemInitialize();

                categoryAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, MusicRepository.categoryList);
                drawerListView.Adapter = categoryAdapter;

                mainDrawerLayout.OpenDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private void DrawerListView_ItemSelected(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                if (isCategory)
                {
                    MusicRepository.categoryIndex = e.Position;

                    var itemAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, MusicRepository.itemList[MusicRepository.categoryIndex]);

                    drawerListView.Adapter = null;
                    drawerListView.Adapter = itemAdapter;

                    isCategory = false;
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
                            MusicRepository.itemIndex = e.Position;
                            MusicRepository.SelectMusic();

                            ((GFOSTPlayerScreen)gfOSTPlayerScreenF).CommandService("Start");

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

        public override void OnBackPressed()
        {
            if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                return;
            }
            else
            {
                ETC.ostIndex[0] = MusicRepository.categoryIndex;
                ETC.ostIndex[1] = MusicRepository.itemIndex;

                base.OnBackPressed();
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
        }
    }

    public class GFOSTPlayerScreen : Android.Support.V4.App.Fragment
    {
        private bool isPlaying = false;

        private View v;

        private ImageView musicAlbumArt;
        private TextView musicName;
        private SeekBar musicSeekBar;
        private TextView musicLengthText;
        private TextView musicNowPosition;
        private ImageView skipPreviousButton;
        private ImageView playPauseButton;
        private ImageView skipNextButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater?.Inflate(Resource.Layout.GFOSTPlayerScreen, container, false);

            musicAlbumArt = v.FindViewById<ImageView>(Resource.Id.GFOSTPlayerMusicAlbumArt);
            musicName = v.FindViewById<TextView>(Resource.Id.GFOSTPlayerMusicName);
            musicSeekBar = v.FindViewById<SeekBar>(Resource.Id.GFOSTPlayerMusicSeekBar);
            musicSeekBar.StopTrackingTouch += (sender, e) =>
            {
                try
                {
                    ETC.ostPlayer.Seek(e.SeekBar.Progress);
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

            if (ETC.ostPlayer != null)
            {
                MusicRepository.categoryIndex = ETC.ostIndex[0];
                MusicRepository.itemIndex = ETC.ostIndex[1];
            }

            return v;
        }

        private void MusicControlButton_Click(object sender, EventArgs e)
        {
            try
            {
                ImageView iv = sender as ImageView;
                string command = "";

                switch (iv.Id)
                {
                    case Resource.Id.GFOSTPlayerSkipPreviousButton:
                        command = "SkipPrevious";
                        break;
                    case Resource.Id.GFOSTPlayerPlayPauseButton when isPlaying == false:
                        command = "Play";
                        isPlaying = true;
                        break;
                    case Resource.Id.GFOSTPlayerPlayPauseButton when isPlaying == true:
                        command = "Pause";
                        isPlaying = false;
                        break;
                    case Resource.Id.GFOSTPlayerSkipNextButton:
                        command = "SkipNext";
                        break;
                }

                CommandService(command);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        internal void CommandService(string command)
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
        }
    }

    public class ServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            ETC.ostService.
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            throw new NotImplementedException();
        }
    }

    [Service]
    public class GFOSTService : Service
    {
        public override void OnCreate()
        {
            base.OnCreate();

            if (CheckInit() == true)
            {
                MusicRepository.PlayerInitialize();
                MusicRepository.MusicItemInitialize();
            }

            MusicRepository.player.Completion += delegate
            {
                MusicRepository.SkipMusic(1);
                LoadMusic(MusicRepository.musicServerPath, true);
            };
        }

        private bool CheckInit()
        {
            try
            {
                if (MusicRepository.player == null) return true;

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