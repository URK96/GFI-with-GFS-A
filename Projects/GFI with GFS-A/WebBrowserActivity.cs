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
using Android.Support.V4.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.WebViewActivity", Label = "", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class WebBrowserActivity : BaseAppCompatActivity
    {
        private static EditText webAddressEditText;
        private static SwipeRefreshLayout mainRefreshLayout;
        private WebView web;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.WebBrowserLayout);

            if (!CheckDeviceNetwork())
            {
                Toast.MakeText(this, Resource.String.Web_NetworkNotInternet, ToastLength.Short).Show();
                Finish();
            }

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.WebBrowserMainToolbar));

            mainRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.WebBrowserMainRefreshLayout);
            mainRefreshLayout.Refresh += delegate { web.Reload(); };
            mainRefreshLayout.ViewTreeObserver.ScrollChanged += (sender, e) =>
            {
                if (web.ScrollY == 0)
                {
                    mainRefreshLayout.Enabled = true;
                }
                else
                {
                    mainRefreshLayout.Enabled = false;
                }
            };

            web = FindViewById<WebView>(Resource.Id.WebBrowser);
            webAddressEditText = FindViewById<EditText>(Resource.Id.WebBrowserAddressBar);
            webAddressEditText.EditorAction += (object sender, TextView.EditorActionEventArgs e) =>
            {
                string url = (sender as EditText).Text;

                if (e.ActionId == Android.Views.InputMethods.ImeAction.Done)
                {
                    if (url.StartsWith("http"))
                    {
                        web.LoadUrl(url);
                    }
                    else
                    {
                        web.LoadUrl($"http://{url}");
                    }
                }
            };

            web.SetWebViewClient(new WebBrowserWebClient());
            web.Settings.JavaScriptEnabled = true;
            web.Settings.BuiltInZoomControls = true;
            web.Settings.AllowContentAccess = true;
            web.Settings.BlockNetworkImage = false;
            web.Settings.BlockNetworkLoads = false;
            web.Settings.LoadsImagesAutomatically = true;
            web.Settings.DomStorageEnabled = true;
            web.Settings.SetAppCacheEnabled(false);
            
            InitProcess();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.WebBrowserMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Resource.Id.WebBrowserPrevious:
                    if (web.CanGoBack())
                    {
                        web.GoBack();
                    }
                    break;
                case Resource.Id.WebBrowserNext:
                    if (web.CanGoForward())
                    {
                        web.GoForward();
                    }
                    break;
                case Resource.Id.WebBrowserClose:
                    Finish();
                    OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                    GC.Collect();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void InitProcess()
        {
            try
            {
                string url = Intent.GetStringExtra("url");

                url ??= Intent.DataString;

                if (url != null)
                {
                    web.LoadUrl(url);
                }
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

                return network == NetworkAccess.Internet;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);

                return false;
            }
        }

        public override void OnBackPressed()
        {
            if (web.CanGoBack())
            {
                web.GoBack();
            }
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
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);

                webAddressEditText.Text = url;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);

                mainRefreshLayout.Refreshing = false;
            }
        }
    }
}