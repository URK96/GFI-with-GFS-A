using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using System;
using System.IO;

namespace GFI_with_GFS_A
{
    [Activity(Label = "EventActivity", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EventActivity : FragmentActivity
    {
        private static ProgressBar LoadProgress;

        private WebView web;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.EventWebLayout);

            LoadProgress = FindViewById<ProgressBar>(Resource.Id.EventWebViewerProgressBar);       
            web = FindViewById<WebView>(Resource.Id.EventWebViewer);
            web.Settings.JavaScriptEnabled = true;
            web.SetWebViewClient(new EventWebClient());
            web.Settings.BuiltInZoomControls = true;

            InitProcess();
        }

        private void InitProcess()
        {
            try
            {
                string url = "";

                using (StreamReader sr = new StreamReader(new FileStream(System.IO.Path.Combine(ETC.CachePath, "Event", "EventVer.txt"), FileMode.Open, FileAccess.Read)))
                {
                    string temp = sr.ReadToEnd();
                    url = temp.Split(';')[4];
                }

                web.LoadUrl(url);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        public override void OnBackPressed()
        {
            if (web.CanGoBack() == true) web.GoBack();
            else
            {
                base.OnBackPressed();
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                GC.Collect();
            }
        }

        private class EventWebClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                view.LoadUrl(url);
                return false;
            }

            public override void OnLoadResource(WebView view, string url)
            {
                base.OnLoadResource(view, url);
                LoadProgress.Progress = view.Progress;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
                LoadProgress.Visibility = ViewStates.Visible;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
                LoadProgress.Visibility = ViewStates.Gone;
            }
        }
    }
}