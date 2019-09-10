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
        private static ProgressBar loadProgress;

        private WebView web;
        private ImageButton previousButton;
        private ImageButton nextButton;
        private ImageButton closeButton;
        private static EditText webAddressEditText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme)
                SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.WebBrowserLayout);

            if (!CheckDeviceNetwork())
            {
                Toast.MakeText(this, Resource.String.Web_NetworkNotInternet, ToastLength.Short).Show();
                Finish();
            }

            //loadProgress = FindViewById<ProgressBar>(Resource.Id.WebBrowserToolbarLoadProgress);
            web = FindViewById<WebView>(Resource.Id.WebBrowser);
            webAddressEditText = FindViewById<EditText>(Resource.Id.WebBrowserAddressBar);
            webAddressEditText.EditorAction += (object sender, TextView.EditorActionEventArgs e) =>
            {
                string url = (sender as EditText).Text;

                if (e.ActionId == Android.Views.InputMethods.ImeAction.Done)
                    if (url.StartsWith("http"))
                        web.LoadUrl(url);
                    else
                        web.LoadUrl($"http://{url}");
            };
            previousButton = FindViewById<ImageButton>(Resource.Id.WebBrowserToolbarPrevious);
            nextButton = FindViewById<ImageButton>(Resource.Id.WebBrowserToolbarNext);
            closeButton = FindViewById<ImageButton>(Resource.Id.WebBrowserToolbarClose);
            previousButton.Click += delegate
            {
                if (web.CanGoBack())
                    web.GoBack();
            };
            nextButton.Click += delegate
            {
                if (web.CanGoForward())
                    web.GoForward();
            };
            closeButton.Click += delegate 
            {
                Finish();
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            };

            web.Settings.JavaScriptEnabled = true;
            web.SetWebViewClient(new WebBrowserWebClient());
            web.Settings.BuiltInZoomControls = true;
            web.Settings.AllowContentAccess = true;
            web.Settings.BlockNetworkImage = false;
            web.Settings.BlockNetworkLoads = false;
            web.Settings.LoadsImagesAutomatically = true;
            web.Settings.DomStorageEnabled = true;
            web.Settings.SetAppCacheEnabled(true);
            
            InitProcess();
        }

        private void InitProcess()
        {
            try
            {
                string url = Intent.GetStringExtra("url");

                url = url ?? Intent.DataString;

                web.LoadUrl(url);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Cannot Load Web URL", ToastLength.Short).Show();
            }
        }

        private bool CheckDeviceNetwork()
        {
            try
            {
                var network = Connectivity.NetworkAccess;

                if (network != NetworkAccess.Internet)
                    return false;
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
            if (web.CanGoBack())
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

                if (loadProgress != null)
                    loadProgress.Progress = view.Progress;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
                webAddressEditText.Text = url;

                if (loadProgress != null)
                    loadProgress.Visibility = ViewStates.Visible;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);

                if (loadProgress != null)
                    loadProgress.Visibility = ViewStates.Invisible;
            }
        }
    }
}