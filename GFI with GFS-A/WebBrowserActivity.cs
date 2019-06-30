using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using System;
using Android.Support.Design.Widget;
using Xamarin.Essentials;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.WebViewActivity", Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class WebBrowserActivity : FragmentActivity
    {
        private static ProgressBar LoadProgress;

        private WebView web;
        private FloatingActionButton ExitFAB;
        private ImageButton PreviousButton;
        private ImageButton NextButton;
        private ImageButton CloseButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true)
                SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.WebBrowserLayout);

            if (CheckDeviceNetwork() == false)
            {
                Toast.MakeText(this, Resource.String.Web_NetworkNotInternet, ToastLength.Short).Show();
                Finish();
            }

            //LoadProgress = FindViewById<ProgressBar>(Resource.Id.WebBrowserProgressBar);       
            web = FindViewById<WebView>(Resource.Id.WebBrowser);
            //ExitFAB = FindViewById<FloatingActionButton>(Resource.Id.ExitWebFAB);
            //ExitFAB.Click += ExitFAB_Click;
            PreviousButton = FindViewById<ImageButton>(Resource.Id.WebBrowserToolbarPrevious);
            NextButton = FindViewById<ImageButton>(Resource.Id.WebBrowserToolbarNext);
            CloseButton = FindViewById<ImageButton>(Resource.Id.WebBrowserToolbarClose);

            PreviousButton.Click += delegate
            {
                if (web.CanGoBack())
                    web.GoBack();
            };
            NextButton.Click += delegate
            {
                if (web.CanGoForward())
                    web.GoForward();
            };
            CloseButton.Click += delegate { Finish(); };

            web.Settings.JavaScriptEnabled = true;
            web.SetWebViewClient(new WebBrowserWebClient());
            web.Settings.BuiltInZoomControls = true;
            web.Settings.AllowContentAccess = true;
            web.Settings.BlockNetworkImage = false;
            web.Settings.BlockNetworkLoads = false;
            web.Settings.LoadsImagesAutomatically = true;
            web.Settings.DomStorageEnabled = true;
            //web.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
            web.Settings.SetAppCacheEnabled(true);
            
            InitProcess();
        }

        private void InitProcess()
        {
            try
            {
                string url = Intent.GetStringExtra("url");

                if (url == null)
                    url = Intent.DataString;

                web.LoadUrl(url);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private bool CheckDeviceNetwork()
        {
            try
            {
                var network = Connectivity.NetworkAccess;
                if (network != NetworkAccess.Internet) return false;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                return false;
            }

            return true;
        }

        public override void OnBackPressed()
        {
            if (web.CanGoBack() == true)
                web.GoBack();
            else
            {
                base.OnBackPressed();
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                GC.Collect();
            }
        }

        private class WebBrowserWebClient : WebViewClient
        {
            [Obsolete]
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                view.LoadUrl(url);
                return false;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                view.LoadUrl(request.Url.ToString());
                return false;
            }

            public override void OnLoadResource(WebView view, string url)
            {
                base.OnLoadResource(view, url);
                if (LoadProgress != null) LoadProgress.Progress = view.Progress;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
                if (LoadProgress != null) LoadProgress.Visibility = ViewStates.Visible;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
                if (LoadProgress != null) LoadProgress.Visibility = ViewStates.Gone;
            }
        }
    }
}