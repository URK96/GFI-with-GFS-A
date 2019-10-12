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
    public class StoryReaderActivity : BaseAppCompatActivity
    {
        private string top = "";
        private string category = "";
        private int itemIndex = 0;
        private int itemCount = 0;
        private int dollDicNumber = 0;
        private string[] itemList;

        private string Language = "ko";

        private string textColor;
        private string backgroundColor;
        private int textSize = 12;

        private ProgressBar LoadProgressBar;
        private Button PreviousButton;
        private Button NextButton;
        private ImageButton RefreshButton;
        private TextView MainTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
                SetTheme(Resource.Style.GFS_NoActionBar_Light);

            // Create your application here
            SetContentView(Resource.Layout.StoryReaderLayout);

            string[] info = Intent.GetStringArrayExtra("Info");
            top = info[0];
            category = info[1];
            itemIndex = int.Parse(info[2]) + 1;
            itemCount = int.Parse(info[3]);

            if (category == "ModStory")
                dollDicNumber = int.Parse(info[4]);

            itemList = Intent.GetStringArrayExtra("List");

            textSize = ETC.sharedPreferences.GetInt("TextViewerTextSize", 12);
            textColor = ETC.sharedPreferences.GetString("TextViewerTextColorHex", "None");
            backgroundColor = ETC.sharedPreferences.GetString("TextViewerBackgroundColorHex", "None");

            LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.StoryReaderLoadProgress);
            PreviousButton = FindViewById<Button>(Resource.Id.StoryReaderPreviousButton);
            PreviousButton.Click += StatusButton_Click;
            NextButton = FindViewById<Button>(Resource.Id.StoryReaderNextButton);
            NextButton.Click += StatusButton_Click;
            RefreshButton = FindViewById<ImageButton>(Resource.Id.StoryReaderRefreshButton);
            RefreshButton.Click += delegate { _ = LoadProcess(true); };
            MainTextView = FindViewById<TextView>(Resource.Id.StoryReaderMainTextView);

            MainTextView.TextSize = textSize;
            if (textColor != "None")
                MainTextView.SetTextColor(Android.Graphics.Color.ParseColor(textColor));
            if (backgroundColor != "None")
                MainTextView.SetBackgroundColor(Android.Graphics.Color.ParseColor(backgroundColor));

            if (itemIndex == 1)
                PreviousButton.Enabled = false;
            if (itemIndex == itemCount)
                NextButton.Enabled = false;

            _ = LoadProcess(false);
        }

        private void StatusButton_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;

            switch (b.Id)
            {
                case Resource.Id.StoryReaderPreviousButton:
                    if (itemIndex == 1)
                        return;
                    if (itemIndex == 2)
                        PreviousButton.Enabled = false;

                    itemIndex -= 1;

                    if (itemIndex != itemCount)
                        NextButton.Enabled = true;

                    _ = LoadProcess(false);
                    break;
                case Resource.Id.StoryReaderNextButton:
                    if (itemIndex == itemCount)
                        return;
                    if (itemIndex == (itemCount - 1))
                        NextButton.Enabled = false;

                    itemIndex += 1;

                    if (itemIndex != 1)
                        PreviousButton.Enabled = true;

                    _ = LoadProcess(false);
                    break;
            }
        }

        private async Task LoadProcess(bool IsRefresh)
        {
            LoadProgressBar.Visibility = ViewStates.Visible;

            string file;

            if (category == "ModStory")
                file = Path.Combine(ETC.CachePath, "Story", category, $"{dollDicNumber}_{itemIndex}.gfdcache");
            else
                file = Path.Combine(ETC.CachePath, "Story", category, $"{itemIndex}.gfdcache");

            if ((File.Exists(file) == false) || (IsRefresh == true))
                await DownloadStory();

            await LoadText(file);

            FindViewById<TextView>(Resource.Id.StoryReaderNowStoryText).Text = itemList[itemIndex - 1];

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

                if (category == "ModStory")
                {
                    server = Path.Combine(ETC.Server, "Data", "Text", "Story", Language, top, category, $"{dollDicNumber}_{itemIndex}.txt");
                    target = Path.Combine(ETC.CachePath, "Story", category, $"{dollDicNumber}_{itemIndex}.gfdcache");
                }
                else
                {
                    server = Path.Combine(ETC.Server, "Data", "Text", "Story", Language, top, category, $"{itemIndex}.txt");
                    target = Path.Combine(ETC.CachePath, "Story", category, $"{itemIndex}.gfdcache");
                }

                DirectoryInfo di = new DirectoryInfo(Path.Combine(ETC.CachePath, "Story", category));

                if (di.Exists == false)
                    di.Create();

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