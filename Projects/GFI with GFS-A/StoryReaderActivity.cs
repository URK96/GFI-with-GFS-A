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
    [Activity(Label = "StoryReaderActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class StoryReaderActivity : BaseAppCompatActivity
    {
        private string top = "";
        private string category = "";
        private int itemIndex = 0;
        private int itemCount = 0;
        private int dollDicNumber = 0;
        private string[] itemList;
        private string[] topTitleList;

        private string language = "ko";

        private string textColor;
        private string backgroundColor;
        private int textSize = 12;

        private ProgressBar loadProgressBar;
        private TextView mainTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.StoryReaderLayout);

            string[] info = Intent.GetStringArrayExtra("Info");
            top = info[0];
            category = info[1];
            itemIndex = int.Parse(info[2]) + 1;
            itemCount = int.Parse(info[3]);

            if (category == "ModStory")
            {
                dollDicNumber = int.Parse(info[4]);
            }

            itemList = Intent.GetStringArrayExtra("List");
            topTitleList = Intent.GetStringArrayExtra("TopList");

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.StoryReaderMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            textSize = ETC.sharedPreferences.GetInt("TextViewerTextSize", 12);
            textColor = ETC.sharedPreferences.GetString("TextViewerTextColorHex", "None");
            backgroundColor = ETC.sharedPreferences.GetString("TextViewerBackgroundColorHex", "None");

            loadProgressBar = FindViewById<ProgressBar>(Resource.Id.StoryReaderLoadProgress);
            mainTextView = FindViewById<TextView>(Resource.Id.StoryReaderMainTextView);

            mainTextView.TextSize = textSize;

            if (textColor != "None")
            {
                mainTextView.SetTextColor(Android.Graphics.Color.ParseColor(textColor));
            }
            if (backgroundColor != "None")
            {
                mainTextView.SetBackgroundColor(Android.Graphics.Color.ParseColor(backgroundColor));
            }

            _ = LoadProcess(false);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.StoryReaderToolbarMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.StoryReaderSkipPrevious:
                case Resource.Id.StoryReaderSkipNext:
                    SkipStory(item.ItemId);
                    break;
                case Resource.Id.RefreshStoryTextCache:
                    _ = LoadProcess(true);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void SkipStory(int resId)
        {
            switch (resId)
            {
                case Resource.Id.StoryReaderSkipPrevious:
                    if (itemIndex == 1)
                    {
                        return;
                    }

                    itemIndex -= 1;

                    _ = LoadProcess(false);
                    break;
                case Resource.Id.StoryReaderSkipNext:
                    if (itemIndex == itemCount)
                    {
                        return;
                    }

                    itemIndex += 1;

                    _ = LoadProcess(false);
                    break;
            }
        }

        private async Task LoadProcess(bool isRefresh)
        {
            loadProgressBar.Visibility = ViewStates.Visible;

            string file = (category == "ModStory") ? Path.Combine(ETC.cachePath, "Story", category, $"{dollDicNumber}_{itemIndex}.gfdcache") :
                Path.Combine(ETC.cachePath, "Story", category, $"{itemIndex}.gfdcache");

            if (!File.Exists(file) || isRefresh)
            {
                await DownloadStory();
            }

            await LoadText(file);

            SupportActionBar.Title = $"{itemIndex}. {itemList[itemIndex - 1]}";

            FindViewById<TextView>(Resource.Id.StoryReaderToolbarNowStoryCategory).Text = topTitleList[itemIndex - 1];
            FindViewById<TextView>(Resource.Id.StoryReaderToolbarNowStoryTitle).Text = itemList[itemIndex - 1];

            loadProgressBar.Visibility = ViewStates.Invisible;
        }

        private async Task LoadText(string path)
        {
            mainTextView.Text = "";

            string s = "";

            using (var sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                s = await sr.ReadToEndAsync();
            }
            
            mainTextView.Text = s;
        }

        private async Task DownloadStory()
        {
            try
            {
                string server = "";
                string target = "";

                if (category == "ModStory")
                {
                    server = Path.Combine(ETC.server, "Data", "Text", "Story", language, top, category, $"{dollDicNumber}_{itemIndex}.txt");
                    target = Path.Combine(ETC.cachePath, "Story", category, $"{dollDicNumber}_{itemIndex}.gfdcache");
                }
                else
                {
                    server = Path.Combine(ETC.server, "Data", "Text", "Story", language, top, category, $"{itemIndex}.txt");
                    target = Path.Combine(ETC.cachePath, "Story", category, $"{itemIndex}.gfdcache");
                }

                DirectoryInfo di = new DirectoryInfo(Path.Combine(ETC.cachePath, "Story", category));

                if (!di.Exists)
                {
                    di.Create();
                }

                using (var wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(server, target);
                }
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