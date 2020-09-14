using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Com.Wang.Avi;

using ImageViews.Photo;

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFDA
{
    [Activity(Label = "", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBGuideImageViewer : BaseAppCompatActivity
    {
        private string serverImagePath;
        private string localImagePath;
        private int dollNum = 0;

        private RelativeLayout loadingLayout;
        private AVLoadingIndicatorView loadingIndicator;
        private TextView loadingText;
        private PhotoView guideImageView;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.DollDBGuideImageViewerLayout);
            dollNum = Intent.GetIntExtra("Info", 0);

            serverImagePath = Path.Combine(ETC.server, "Data", "Images", "Guns", "GuideImage", $"{dollNum}.png");
            localImagePath = Path.Combine(ETC.cachePath, "Doll", "GuideImage", $"{dollNum}.png");

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.DollDBGuideImageViewerMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            FindViewById<TextView>(Resource.Id.DollDBGuideImageViewerToolbarDicNumber).Text = $"No. {dollNum}";
            FindViewById<TextView>(Resource.Id.DollDBGuideImageViewerToolbarTitle).SetText(Resource.String.DollDBGuideImageViewer_Title);

            loadingLayout = FindViewById<RelativeLayout>(Resource.Id.DollDBGuideImageViewerLoadingLayout);
            loadingIndicator = FindViewById<AVLoadingIndicatorView>(Resource.Id.DollDBGuideImageViewerLoadingIndicatorView);
            loadingText = FindViewById<TextView>(Resource.Id.DollDBGuideImageViewerLoadingIndicatorExplainText);
            guideImageView = FindViewById<PhotoView>(Resource.Id.DollDBGuideImageViewerImageView);

            _ = InitProcess();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.DollDBGuideImageViewerMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.RefreshDollGuideImageCache:
                    _ = LoadImage(true);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async Task InitProcess()
        {
            try
            {
                if (!File.Exists(localImagePath))
                {
                    if (!await DownloadImage())
                    {
                        return;
                    }
                }

                _ = LoadImage();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Init process error", ToastLength.Short).Show();
            }
        }

        private async Task<bool> CheckExistGuideImage()
        {
            try
            {
                loadingText.SetText(Resource.String.DollDBGuideImageViewer_LoadingText_CheckServerImage);

                await Task.Delay(500);

                Uri.TryCreate(serverImagePath, UriKind.RelativeOrAbsolute, out Uri uri);
                var request = WebRequest.Create(uri);

                using (var response = await request.GetResponseAsync().ConfigureAwait(false))
                {
                    if (response.ContentType == "image/png")
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail to check server image", ToastLength.Short).Show();
            }

            return false;
        }

        private async Task<bool> DownloadImage()
        {
            try
            {
                loadingIndicator.SmoothToShow();

                if (!await CheckExistGuideImage())
                {
                    loadingIndicator.SmoothToHide();
                    loadingText.SetText(Resource.String.DollDBGuideImageViewer_LoadingText_NotExistServerImage);

                    return false;
                }

                string defaultText = Resources.GetString(Resource.String.DollDBGuideImageViewer_LoadingText_DownloadImage);

                loadingText.Text = defaultText;

                await Task.Delay(500);

                using (var wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (sender, e) => { loadingText.Text = $"{defaultText}{e.ProgressPercentage}%"; };

                    await wc.DownloadFileTaskAsync(serverImagePath, localImagePath);
                }

                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                loadingIndicator.SmoothToHide();
                loadingText.SetText(Resource.String.DollDBGuideImageViewer_LoadingText_FailDownloadImage);

                return false;
            }

            return true;
        }

        private async Task LoadImage(bool isRefresh = false)
        {
            try
            {
                guideImageView.SetImageDrawable(null);
                loadingLayout.Visibility = ViewStates.Visible;
                loadingIndicator.SmoothToShow();

                if (isRefresh)
                {
                    if (!await DownloadImage())
                    {
                        return;
                    }
                }

                loadingText.SetText(Resource.String.DollDBGuideImageViewer_LoadingText_LoadImage);

                await Task.Delay(100);

                guideImageView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(localImagePath));

                await Task.Delay(500);

                loadingIndicator.SmoothToHide();
                loadingText.Text = "";
                loadingLayout.Visibility = ViewStates.Gone;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                loadingIndicator.SmoothToHide();
                loadingText.SetText(Resource.String.DollDBGuideImageViewer_LoadingText_FailLoadImage);
            }
            finally
            {
                GC.Collect();
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }
}