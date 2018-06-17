﻿using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using System;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class WebBrowserActivity : FragmentActivity
    {
        private static ProgressBar LoadProgress;

        private WebView web;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.WebBrowserLayout);

            LoadProgress = FindViewById<ProgressBar>(Resource.Id.WebBrowserProgressBar);       
            web = FindViewById<WebView>(Resource.Id.WebBrowser);
            web.Settings.JavaScriptEnabled = true;
            web.SetWebViewClient(new WebBrowserWebClient());
            web.Settings.BuiltInZoomControls = true;
            
            InitProcess();
        }

        private void InitProcess()
        {
            try
            {
                string url = Intent.GetStringExtra("url");

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

        private class WebBrowserWebClient : WebViewClient
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