using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "StoryReaderActivity", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class StoryReaderActivity : AppCompatActivity
    {
        private string Top = "";
        private string Category = "";
        private int Item_Index = 0;
        private int Item_Count = 0;
        private int Doll_DicNumber = 0;
        private string[] Item_List;

        private string Language = "ko";

        private ProgressBar LoadProgressBar;
        private Button PreviousButton;
        private Button NextButton;
        private ImageButton RefreshButton;
        private TextView MainTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

            // Create your application here
            SetContentView(Resource.Layout.StoryReaderLayout);

            string[] info = Intent.GetStringArrayExtra("Info");
            Top = info[0];
            Category = info[1];
            Item_Index = int.Parse(info[2]) + 1;
            Item_Count = int.Parse(info[3]);

            if (Category == "ModStory") Doll_DicNumber = int.Parse(info[4]);

            Item_List = Intent.GetStringArrayExtra("List");

            LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.StoryReaderLoadProgress);
            PreviousButton = FindViewById<Button>(Resource.Id.StoryReaderPreviousButton);
            PreviousButton.Click += StatusButton_Click;
            NextButton = FindViewById<Button>(Resource.Id.StoryReaderNextButton);
            NextButton.Click += StatusButton_Click;
            RefreshButton = FindViewById<ImageButton>(Resource.Id.StoryReaderRefreshButton);
            RefreshButton.Click += delegate { _ = LoadProcess(true); };
            MainTextView = FindViewById<TextView>(Resource.Id.StoryReaderMainTextView);

            if (Item_Index == 1) PreviousButton.Enabled = false;
            if (Item_Index == Item_Count) NextButton.Enabled = false;

            _ = LoadProcess(false);
        }

        private void StatusButton_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;

            switch (b.Id)
            {
                case Resource.Id.StoryReaderPreviousButton:
                    if (Item_Index == 1)
                        return;
                    if (Item_Index == 2)
                        PreviousButton.Enabled = false;

                    Item_Index -= 1;

                    if (Item_Index != Item_Count)
                        NextButton.Enabled = true;

                    _ = LoadProcess(false);
                    break;
                case Resource.Id.StoryReaderNextButton:
                    if (Item_Index == Item_Count)
                        return;
                    if (Item_Index == (Item_Count - 1))
                        NextButton.Enabled = false;

                    Item_Index += 1;

                    if (Item_Index != 1)
                        PreviousButton.Enabled = true;

                    _ = LoadProcess(false);
                    break;
            }
        }

        private async Task LoadProcess(bool IsRefresh)
        {
            LoadProgressBar.Visibility = ViewStates.Visible;

            string file = "";

            if (Category == "ModStory")
                file = Path.Combine(ETC.CachePath, "Story", Category, $"{Doll_DicNumber}_{Item_Index}.gfdcache");
            else
                file = Path.Combine(ETC.CachePath, "Story", Category, $"{Item_Index}.gfdcache");

            if ((File.Exists(file) == false) || (IsRefresh == true)) await DownloadStory();

            await LoadText(file);

            FindViewById<TextView>(Resource.Id.StoryReaderNowStoryText).Text = Item_List[Item_Index - 1];

            LoadProgressBar.Visibility = ViewStates.Invisible;
        }

        private async Task LoadText(string path)
        {
            MainTextView.Text = "";

            string s = "";

            using (StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
                s = await sr.ReadToEndAsync();
            
            MainTextView.Text = s;
        }

        private async Task DownloadStory()
        {
            try
            {
                string server = "";
                string target = "";

                if (Category == "ModStory")
                {
                    server = Path.Combine(ETC.Server, "Data", "Text", "Story", Language, Top, Category, $"{Doll_DicNumber}_{Item_Index}.txt");
                    target = Path.Combine(ETC.CachePath, "Story", Category, $"{Doll_DicNumber}_{Item_Index}.gfdcache");
                }
                else
                {
                    server = Path.Combine(ETC.Server, "Data", "Text", "Story", Language, Top, Category, $"{Item_Index}.txt");
                    target = Path.Combine(ETC.CachePath, "Story", Category, $"{Item_Index}.gfdcache");
                }

                DirectoryInfo di = new DirectoryInfo(Path.Combine(ETC.CachePath, "Story", Category));
                if (di.Exists == false) di.Create();

                using (WebClient wc = new WebClient())
                    await wc.DownloadFileTaskAsync(server, target);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
        }
    }
}