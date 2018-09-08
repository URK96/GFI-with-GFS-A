﻿using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
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
    [Activity(Label = "GFOSTPlayerActivity", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GFOSTPlayerActivity : AppCompatActivity
    {
        private bool IsCategory = true;
        private string Category = "";
        private string Server_MusicPath = Path.Combine(ETC.Server, "Data", "Music");
        private string MusicPath = "";

        private ArrayAdapter Category_Adapter;
        private FragmentTransaction ft;

        private DrawerLayout MainDrawerLayout;
        private ListView DrawerListView;

        private List<string> Item_List = new List<string>();
        private string[] Category_List =
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

            ft = FragmentManager.BeginTransaction();
            ft.Add(Resource.Id.GFOSTPlayerMainLayout, new GFOSTPlayerScreen(), "GFOSTPlayerScreen");

            ft.Commit();

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
                Category_Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, Category_List);

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
                    int Array_Id = 0;

                    switch (e.Position)
                    {
                        case 0:
                            Category = "Normal";
                            Array_Id = Resource.Array.Normal;
                            break;
                        case 1:
                            Array_Id = Resource.Array.ContinuumTurbulence;
                            Category = "Continuum Turbulence";
                            break;
                        case 2:
                            Array_Id = Resource.Array.Cube;
                            Category = "Cube";
                            break;
                        case 3:
                            Array_Id = Resource.Array.DeepDive;
                            Category = "Deep Dive";
                            break;
                        case 4:
                            Array_Id = Resource.Array.DJMAX;
                            Category = "DJMAX";
                            break;
                        case 5:
                            Array_Id = Resource.Array.GuiltyGear;
                            Category = "GuiltyGear";
                            break;
                        case 6:
                            Array_Id = Resource.Array.Houkai2;
                            Category = "Houkai2";
                            break;
                        case 7:
                            Array_Id = Resource.Array.Hypothermia;
                            Category = "Hypothermia";
                            break;
                        case 8:
                            Array_Id = Resource.Array.Singularity;
                            Category = "Singularity";
                            break;
                    }

                    Item_List.Clear();
                    Item_List.Add("...");
                    Item_List.AddRange(Resources.GetStringArray(Array_Id));
                    Item_List.TrimExcess();

                    var item_adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, Item_List);

                    DrawerListView.Adapter = null;
                    DrawerListView.Adapter = item_adapter;

                    IsCategory = false;
                }
                else
                {
                    switch (e.Position)
                    {
                        case 0:
                            Category = "";
                            DrawerListView.Adapter = Category_Adapter;
                            IsCategory = true;
                            break;
                        default:
                            LoadMusicPath(Item_List[e.Position]);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void LoadMusicPath(string MusicName)
        {
            try
            {
                string FileName = string.Format("{0}.mp3", MusicName);
                MusicPath = Path.Combine(Server_MusicPath, "OST", Category, FileName);

                var intent = new Intent(this, typeof(GFOSTService));
                intent.PutExtra("MusicPath", MusicPath);
                intent.PutExtra("Command", "Start_C");
                StartService(intent);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        public string GetMusicPath() { return MusicPath; }
    }

    public class GFOSTPlayerScreen : Fragment
    {
        private string MusicPath;

        private View v;

        private Button PlayButton;
        private Button StopButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.GFOSTPlayerScreen, container, false);

            PlayButton = v.FindViewById<Button>(Resource.Id.GFOSTPlayerPlayButton);
            PlayButton.Click += MusicControlButton_Click;
            StopButton = v.FindViewById<Button>(Resource.Id.GFOSTPlayerStopButton);
            StopButton.Click += MusicControlButton_Click;

            return v;
        }

        private void MusicControlButton_Click(object sender, EventArgs e)
        {
            try
            {
                Button bt = sender as Button;
                string command = "";

                var intent = new Intent(Activity, typeof(GFOSTService));
                intent.PutExtra("MusicPath", ((GFOSTPlayerActivity)Activity).GetMusicPath());

                switch (bt.Id)
                {
                    case Resource.Id.GFOSTPlayerPlayButton:
                        command = "Start";
                        break;
                    case Resource.Id.GFOSTPlayerStopButton:
                        command = "Stop";
                        break;
                }

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
        MediaPlayer player;

        bool IsLoad = false;

        public override void OnCreate()
        {
            base.OnCreate();

            player = new MediaPlayer();
            player.Prepared += delegate { player.Start(); };
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            string path = intent.Extras.GetString("MusicPath");
            string command = intent.Extras.GetString("Command");

            switch (command)
            {
                case "Start":
                    player.Start();
                    break;
                case "Start_C":
                    LoadMusic(path, true);
                    break;
                case "Stop":
                    player.Pause();
                    break;
            }

            return base.OnStartCommand(intent, flags, startId);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            player.Release();
            player.Dispose();
        }

        private void LoadMusic(string path, bool IsChange)
        {
            try
            {
                if (IsChange == true)
                {
                    player.Stop();
                    player.Reset();
                }

                /*using (TimeOutWebClient wc = new TimeOutWebClient())
                {
                    await wc.DownloadFileTaskAsync(path, Path.Combine(ETC.CachePath, "test.mp3"));
                }*/

                player.SetDataSource(path);
                player.Prepare();

                IsLoad = true;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
            }
        }
    }
}