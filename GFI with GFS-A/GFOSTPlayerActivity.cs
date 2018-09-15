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

namespace GFI_with_GFS_A
{
    internal static class MusicRepository
    {
        internal static MediaPlayer player = null;
        internal static SeekBar PlayerSeekBar = null;
        internal static TextView MusicTimeText = null;
        internal static TextView MusicNameText = null;
        internal static TextView MusicNowPositionText = null;
        internal static Activity activity;
        internal static Fragment PlayerScreenFragment;
        internal static bool IsPlayerPlaying = false;

        private static readonly string Server_MusicPath = Path.Combine(ETC.Server, "Data", "Music");

        internal static string MusicServerPath = "";
        internal static int CategoryIndex = 0;
        internal static int ItemIndex = 0;

        private static int[] MusicDuration = new int[3];

        internal static string[] Category_List =
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
        internal static string[] Category_RealPath =
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
        internal static List<List<string>> Item_List = new List<List<string>>();

        internal static void PlayerInitialize()
        {
            if (ETC.OSTPlayer == null)
            {
                player = new MediaPlayer();
                ETC.OSTPlayer = player;
            }
            else player = ETC.OSTPlayer;

            player.Prepared += delegate 
            {
                IsPlayerPlaying = true;
                StartSeekBarTask();
                player.Start();
            };
        }

        internal static void MusicItemInitialize()
        {
            try
            {
                Item_List.Clear();

                for (int i = 0; i < Category_List.Length; ++i)
                {
                    int Array_Id = 0;

                    CategoryIndex = i;

                    switch (i)
                    {
                        case 0:
                            Array_Id = Resource.Array.Normal;
                            break;
                        case 1:
                            Array_Id = Resource.Array.ContinuumTurbulence;
                            break;
                        case 2:
                            Array_Id = Resource.Array.Cube;
                            break;
                        case 3:
                            Array_Id = Resource.Array.DeepDive;
                            break;
                        case 4:
                            Array_Id = Resource.Array.DJMAX;
                            break;
                        case 5:
                            Array_Id = Resource.Array.GuiltyGear;
                            break;
                        case 6:
                            Array_Id = Resource.Array.Houkai2;
                            break;
                        case 7:
                            Array_Id = Resource.Array.Hypothermia;
                            break;
                        case 8:
                            Array_Id = Resource.Array.Singularity;
                            break;
                    }

                    List<string> list = new List<string>();

                    list.Add("...");
                    list.AddRange(ETC.Resources.GetStringArray(Array_Id));
                    list.TrimExcess();

                    Item_List.Add(list);
                }

                Item_List.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
            }
        }

        internal static void SkipMusic(int mode)
        {
            try
            {
                switch (mode)
                {
                    case 0:
                        if (ItemIndex == 1)
                        {
                            if (CategoryIndex == 0) CategoryIndex = Category_List.Length - 1;
                            else CategoryIndex -= 1;

                            ItemIndex = Item_List[CategoryIndex].Count - 1;
                        }
                        else ItemIndex -= 1;
                        break;
                    case 1:
                        if (ItemIndex == (Item_List[CategoryIndex].Count - 1))
                        {
                            if (CategoryIndex == Category_List.Length - 1) CategoryIndex = 0;
                            else CategoryIndex += 1;

                            ItemIndex = 1;
                        }
                        else ItemIndex += 1;
                        break;
                }

                SelectMusic();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
            }
        }

        internal static void SelectMusic()
        {
            try
            {
                string FileName = string.Format("{0}.mp3", Item_List[CategoryIndex][ItemIndex]);
                MusicServerPath = Path.Combine(Server_MusicPath, "OST", Category_RealPath[CategoryIndex], FileName);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
            }
        }

        internal static void StartSeekBarTask()
        {
            try
            {
                if (PlayerSeekBar == null) return;

                MusicDuration[0] = player.Duration / 1000 / 60;
                MusicDuration[1] = player.Duration / 1000 - MusicDuration[0] * 60;
                MusicDuration[2] = player.Duration % 1000;

                if (MusicTimeText != null) MusicTimeText.Text = string.Format("{0}:{1}", MusicDuration[0], MusicDuration[1]);
                if (PlayerSeekBar != null) PlayerSeekBar.Max = player.Duration;
                if (MusicNameText != null) MusicNameText.Text = Item_List[CategoryIndex][ItemIndex];

                Task UpdateSeekBar = new Task(async () => 
                {
                    while (IsPlayerPlaying)
                    {
                        activity.RunOnUiThread(() => 
                        {
                            PlayerSeekBar.Progress = player.CurrentPosition;
                            int minute = player.CurrentPosition / 1000 / 60;
                            int second = player.CurrentPosition / 1000 - minute * 60;
                            MusicNowPositionText.Text = string.Format("{0}:{1}", minute, second);
                        });
                        await Task.Delay(1000);
                    }
                });
                UpdateSeekBar.Start();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
            }
        }
    }

    [Activity(Label = "GFOSTPlayerActivity", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GFOSTPlayerActivity : AppCompatActivity
    {
        private bool IsCategory = true;

        private ArrayAdapter Category_Adapter;
        private FragmentTransaction ft;
        private Fragment GFOSTPlayerScreen_F;

        private DrawerLayout MainDrawerLayout;
        private ListView DrawerListView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

            // Create your application here
            SetContentView(Resource.Layout.GFOSTPlayer);

            MainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.GFOSTPlayerMainDrawerLayout);

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.GFOSTPlayerToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            if (ETC.UseLightTheme == true) SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.OSTPlayer_WhiteTheme);
            else SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.OSTPlayer);

            DrawerListView = FindViewById<ListView>(Resource.Id.GFOSTPlayerNavigationListView);
            DrawerListView.ItemClick += DrawerListView_ItemSelected;

            GFOSTPlayerScreen_F = new GFOSTPlayerScreen();

            ft = FragmentManager.BeginTransaction();
            ft.Add(Resource.Id.GFOSTPlayerMainLayout, GFOSTPlayerScreen_F, "GFOSTPlayerScreen");

            ft.Commit();

            MusicRepository.activity = this;
            MusicRepository.PlayerScreenFragment = GFOSTPlayerScreen_F;

            LoadCategoryList();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == false) MainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    else MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void LoadCategoryList()
        {
            try
            {
                MusicRepository.PlayerInitialize();
                MusicRepository.MusicItemInitialize();

                Category_Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, MusicRepository.Category_List);
                DrawerListView.Adapter = Category_Adapter;

                MainDrawerLayout.OpenDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void DrawerListView_ItemSelected(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                if (IsCategory == true)
                {
                    MusicRepository.CategoryIndex = e.Position;

                    var item_adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, MusicRepository.Item_List[MusicRepository.CategoryIndex]);

                    DrawerListView.Adapter = null;
                    DrawerListView.Adapter = item_adapter;

                    IsCategory = false;
                }
                else
                {
                    switch (e.Position)
                    {
                        case 0:
                            DrawerListView.Adapter = Category_Adapter;
                            IsCategory = true;
                            break;
                        default:
                            MusicRepository.ItemIndex = e.Position;
                            MusicRepository.SelectMusic();

                            ((GFOSTPlayerScreen)GFOSTPlayerScreen_F).CommandService("Start");

                            MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == true)
            {
                MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                return;
            }

            ETC.OSTPlayer = MusicRepository.player;
            ETC.OST_Index[0] = MusicRepository.CategoryIndex;
            ETC.OST_Index[1] = MusicRepository.ItemIndex;
        }
    }

    public class GFOSTPlayerScreen : Fragment
    {
        private bool IsPlaying = false;

        private View v;

        private ImageView MusicAlbumArt;
        private TextView MusicName;
        private SeekBar MusicSeekBar;
        private TextView MusicLengthText;
        private TextView MusicNowPosition;
        private ImageView SkipPreviousButton;
        private ImageView PlayPauseButton;
        private ImageView SkipNextButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.GFOSTPlayerScreen, container, false);

            MusicAlbumArt = v.FindViewById<ImageView>(Resource.Id.GFOSTPlayerMusicAlbumArt);
            MusicName = v.FindViewById<TextView>(Resource.Id.GFOSTPlayerMusicName);
            MusicSeekBar = v.FindViewById<SeekBar>(Resource.Id.GFOSTPlayerMusicSeekBar);
            MusicSeekBar.StopTrackingTouch += MusicSeekBar_StopTrackingTouch;
            MusicLengthText = v.FindViewById<TextView>(Resource.Id.GFOSTPlayerMusicLengthText);
            MusicNowPosition = v.FindViewById<TextView>(Resource.Id.GFOSTPlayerMusicNowPositionText);

            MusicRepository.PlayerSeekBar = MusicSeekBar;
            MusicRepository.MusicTimeText = MusicLengthText;
            MusicRepository.MusicNameText = MusicName;
            MusicRepository.MusicNowPositionText = MusicNowPosition;

            SkipPreviousButton = v.FindViewById<ImageView>(Resource.Id.GFOSTPlayerSkipPreviousButton);
            SkipPreviousButton.Clickable = true;
            SkipPreviousButton.Click += MusicControlButton_Click;
            PlayPauseButton = v.FindViewById<ImageView>(Resource.Id.GFOSTPlayerPlayPauseButton);
            PlayPauseButton.Clickable = true;
            PlayPauseButton.Click += MusicControlButton_Click;
            SkipNextButton = v.FindViewById<ImageView>(Resource.Id.GFOSTPlayerSkipNextButton);
            SkipNextButton.Clickable = true;
            SkipNextButton.Click += MusicControlButton_Click;

            if (ETC.UseLightTheme == true)
            {
                SkipPreviousButton.SetImageResource(Resource.Drawable.SkipPrevious_WhiteTheme);
                PlayPauseButton.SetImageResource(Resource.Drawable.PlayPause_WhiteTheme);
                SkipNextButton.SetImageResource(Resource.Drawable.SkipNext_WhiteTheme);
            }
            else
            {
                SkipPreviousButton.SetImageResource(Resource.Drawable.SkipPrevious);
                PlayPauseButton.SetImageResource(Resource.Drawable.PlayPause);
                SkipNextButton.SetImageResource(Resource.Drawable.SkipNext);
            }

            if (ETC.OSTPlayer != null)
            {
                MusicRepository.PlayerInitialize();
                MusicRepository.CategoryIndex = ETC.OST_Index[0];
                MusicRepository.ItemIndex = ETC.OST_Index[1];
                MusicRepository.StartSeekBarTask();
            }

            return v;
        }

        private void MusicSeekBar_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            try
            {
                MusicRepository.player.SeekTo(e.SeekBar.Progress);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
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
                        MusicRepository.SkipMusic(0);
                        command = "SkipPrevious";
                        break;
                    case Resource.Id.GFOSTPlayerPlayPauseButton when IsPlaying == false:
                        command = "Play";
                        IsPlaying = true;
                        break;
                    case Resource.Id.GFOSTPlayerPlayPauseButton when IsPlaying == true:
                        command = "Pause";
                        IsPlaying = false;
                        break;
                    case Resource.Id.GFOSTPlayerSkipNextButton:
                        MusicRepository.SkipMusic(1000);
                        command = "SkipNext";
                        break;
                }

                CommandService(command);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
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
                ETC.LogError(Activity, ex.ToString());
            }
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
                LoadMusic(MusicRepository.MusicServerPath, true);
            };
        }

        private bool CheckInit()
        {
            try
            {
                if (MusicRepository.player == null) return true;

                foreach (List<string> list in MusicRepository.Item_List)
                {
                    if (list.Count == 0) return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
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
                    LoadMusic(MusicRepository.MusicServerPath, true);
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
            ETC.OSTPlayer.Release();
            ETC.OSTPlayer = null;
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

                /*using (TimeOutWebClient wc = new TimeOutWebClient())
                {
                    await wc.DownloadFileTaskAsync(path, Path.Combine(ETC.CachePath, "test.mp3"));
                }*/

                MusicRepository.player.SetDataSource(path);
                MusicRepository.player.Prepare();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
            }
        }
    }
}