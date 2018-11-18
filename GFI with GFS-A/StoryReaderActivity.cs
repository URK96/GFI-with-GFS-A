using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Label = "StoryReaderActivity", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class StoryReaderActivity : AppCompatActivity
    {
        private string Top = "";
        private string Category = "";
        private int Item_Index = 0;
        private int Item_Count = 0;

        private string Language = "ko";

        private ProgressBar LoadProgressBar;
        private TextView MainTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.StoryReaderLayout);

            string[] info = Intent.GetStringArrayExtra("Info");
            Top = info[0];
            Category = info[1];
            Item_Index = int.Parse(info[2]) + 1;
            Item_Count = int.Parse(info[3]);

            LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.StoryReaderLoadProgress);
            MainTextView = FindViewById<TextView>(Resource.Id.StoryReaderMainTextView);

            LoadProcess();
        }

        private async Task LoadProcess()
        {
            LoadProgressBar.Visibility = ViewStates.Visible;

            string file = Path.Combine(ETC.CachePath, "Story", Category, $"{Item_Index}.gfdcache");

            if (File.Exists(file) == false) await DownloadStory();

            await LoadText(file);

            LoadProgressBar.Visibility = ViewStates.Invisible;
        }

        private async Task LoadText(string path)
        {
            MainTextView.Text = "";

            string s = "";

            using (StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                s = await sr.ReadToEndAsync();
            }

            MainTextView.Text = s;
        }

        private async Task DownloadStory()
        {
            try
            {
                string server = Path.Combine(ETC.Server, "Data", "Text", "Story", Language, Top, Category, $"{Item_Index}.txt");
                string target = Path.Combine(ETC.CachePath, "Story", Category, $"{Item_Index}.gfdcache");

                DirectoryInfo di = new DirectoryInfo(Path.Combine(ETC.CachePath, "Story", Category));
                if (di.Exists == false) di.Create();

                using (WebClient wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(server, target);
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
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
        }
    }
}