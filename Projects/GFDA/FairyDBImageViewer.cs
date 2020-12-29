using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;

using Com.Wang.Avi;

using Google.Android.Material.Snackbar;

using ImageViews.Photo;

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFDA
{
    [Activity(Label = "FairyDBImageViewer", Theme="@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FairyDBImageViewer : BaseAppCompatActivity
    {
        private Fairy fairy;

        private AndroidX.AppCompat.Widget.Toolbar toolbar;

        private RelativeLayout loadingLayout;
        private AVLoadingIndicatorView loadingIndicator;
        private TextView loadingText;
        private CoordinatorLayout snackbarLayout;
        private PhotoView fairyImageView;
        private TextView imageStatus;
        private TextView fairyNumTitle;

        private Drawable imageDrawable;

        private int imageNum = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Toolbar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.FairyDBImageViewer);

                fairy = new Fairy(ETC.FindDataRow(ETC.fairyList, "Name", Intent.GetStringExtra("Keyword")));

                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.FairyDBImageViewerMainToolbar);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                loadingLayout = FindViewById<RelativeLayout>(Resource.Id.FairyDBImageViewerLoadingLayout);
                loadingIndicator = FindViewById<AVLoadingIndicatorView>(Resource.Id.FairyDBImageViewerLoadingIndicatorView);
                loadingText = FindViewById<TextView>(Resource.Id.FairyDBImageViewerLoadingIndicatorExplainText);
                fairyImageView = FindViewById<PhotoView>(Resource.Id.FairyDBImageViewerImageView);
                imageStatus = FindViewById<TextView>(Resource.Id.FairyDBImageViewerImageStatus);
                fairyNumTitle = FindViewById<TextView>(Resource.Id.FairyDBImageViewerImageNum);

                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FairyDBImageViewerSnackbarLayout);

                _ = LoadImage();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.FairyDBImageViewerMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.FairyDBImageViewerPreviousButton:
                case Resource.Id.FairyDBImageViewerNextButton:
                    _ = FairyImageStageChange(item.ItemId);
                    break;
                case Resource.Id.RefreshFairyImageCache:
                    _ = LoadImage(true);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async Task FairyImageStageChange(int id)
        {
            try
            {
                switch (id)
                {
                    case Resource.Id.FairyDBImageViewerNextButton when imageNum is 3:
                        imageNum = 1;
                        break;
                    case Resource.Id.FairyDBImageViewerNextButton:
                        imageNum += 1;
                        break;
                    case Resource.Id.FairyDBImageViewerPreviousButton when imageNum is 1:
                        imageNum = 3;
                        break;
                    case Resource.Id.FairyDBImageViewerPreviousButton:
                        imageNum -= 1;
                        break;
                }

                await LoadImage();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ImageViewer_ChangeImageError, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task LoadImage(bool isRefresh = false)
        {
            await Task.Delay(100);

            try
            {
                fairyImageView.SetImageDrawable(null);
                imageDrawable?.Dispose();
                loadingLayout.Visibility = ViewStates.Visible;
                loadingIndicator.SmoothToShow();
                loadingText.SetText(Resource.String.Common_Load);

                string imageName = $"{fairy.DicNumber}_{imageNum}";
                string imagePath = Path.Combine(ETC.cachePath, "Fairy", "Normal", $"{imageName}.gfdcache");
                string url = Path.Combine(ETC.server, "Data", "Images", "Fairy", "Normal", $"{imageName}.png");

                if (!File.Exists(imagePath) || isRefresh)
                {
                    string dTemp = Resources.GetString(Resource.String.Common_Downloading);

                    loadingText.Text = dTemp;

                    using (var wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += (sender, e) => { loadingText.Text = $"{dTemp}{e.ProgressPercentage}%"; };

                        await wc.DownloadFileTaskAsync(url, imagePath);
                    }
                }

                await Task.Delay(500);

                loadingText.SetText(Resource.String.Common_Load);

                imageDrawable = await Drawable.CreateFromPathAsync(imagePath);

                fairyImageView.SetImageDrawable(imageDrawable);

                fairyNumTitle.Text = $"{imageNum} 단계";
                imageStatus.Text = $"{fairy.Name} - {imageNum}단계";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                loadingText.Text = "";
                loadingIndicator.SmoothToHide();
                loadingLayout.Visibility = ViewStates.Gone;
            }
        }

        public override void OnBackPressed()
        {
            imageDrawable?.Dispose();

            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }
}