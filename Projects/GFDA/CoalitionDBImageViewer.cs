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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFDA
{
    [Activity(Label = "CoalitionDBImageViewer", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CoalitionDBImageViewer : BaseAppCompatActivity
    {
        private Coalition coalition;

        private AndroidX.AppCompat.Widget.Toolbar toolbar;

        private RelativeLayout loadingLayout;
        private AVLoadingIndicatorView loadingIndicator;
        private TextView loadingText;
        private CoordinatorLayout snackbarLayout;
        private Spinner costumeList;
        private PhotoView coalitionImageView;
        private TextView imageStatus;

        private List<string> costumes;

        private Drawable imageDrawable;

        private bool isDamage;
        private bool isAwake;
        private int costumeIndex;
        private int modIndex;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Toolbar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.CoalitionDBImageViewer);

                coalition = new Coalition(ETC.FindDataRow(ETC.coalitionList, "DicNumber", int.Parse(Intent.GetStringExtra("Data"))));

                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.CoalitionDBImageViewerMainToolbar);
                loadingLayout = FindViewById<RelativeLayout>(Resource.Id.CoalitionDBImageViewerLoadingLayout);
                loadingIndicator = FindViewById<AVLoadingIndicatorView>(Resource.Id.CoalitionDBImageViewerLoadingIndicatorView);
                loadingText = FindViewById<TextView>(Resource.Id.CoalitionDBImageViewerLoadingIndicatorExplainText);
                coalitionImageView = FindViewById<PhotoView>(Resource.Id.CoalitionDBImageViewerImageView);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.CoalitionDBImageViewerSnackbarLayout);
                costumeList = FindViewById<Spinner>(Resource.Id.CoalitionDBImageViewerCostumeList);
                costumeList.ItemSelected += (sender , e) =>
                {
                    isDamage = false;
                    costumeIndex = e.Position;

                    _ = LoadImage(costumeIndex, false);
                };
                imageStatus = FindViewById<TextView>(Resource.Id.CoalitionDBImageViewerImageStatus);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                await InitializeProcess();

                LoadCostumeList();
                _ = LoadImage(0, false);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.CoalitionDBImageViewerMenu, menu);

            var awakeItem = menu.FindItem(Resource.Id.CoalitionDBImageViewerAwakeButton);
            
            if (!coalition.IsBoss)
            {
                awakeItem.SetVisible(false);
            }

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                /*case Resource.Id.CoalitionDBImageViewerCensoredOption:
                    enableCensored = !enableCensored;

                    _ = LoadImage(costumeIndex, false);
                    break;
                case Resource.Id.CoalitionDBImageViewerChangeStateButton:
                    isDamage = !isDamage;
                    censorMenuItem.SetVisible(CheckCensorType());

                    _ = LoadImage(costumeIndex, false);
                    break;*/
                case Resource.Id.CoalitionDBImageViewerAwakeButton:
                    isAwake = !isAwake;

                    _ = LoadImage(costumeIndex, false);
                    break;
                case Resource.Id.RefreshCoalitionImageCache:
                    _ = LoadImage(costumeIndex, true);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async Task InitializeProcess()
        {
            await Task.Delay(100);

            try
            {
                var toolbarColor = coalition.Grade switch
                {
                    1 => Android.Graphics.Color.DarkSlateGray,
                    2 => Android.Graphics.Color.SlateGray,
                    3 => Android.Graphics.Color.ParseColor("#55CCEE"),
                    4 => Android.Graphics.Color.ParseColor("#AACC22"),
                    5 => Android.Graphics.Color.ParseColor("#FFBB22"),
                    _ => Android.Graphics.Color.ParseColor("#C040B0"),
                };
                toolbar.SetBackgroundColor(toolbarColor);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window.SetStatusBarColor(toolbarColor);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail Initialize Process", ToastLength.Short).Show();
            }
        }

        private void LoadCostumeList()
        {
            try
            {
                costumes = new List<string>()
                {
                    Resources.GetString(Resource.String.CoalitionDBImageViewer_DefaultCostume)
                };

                if (coalition.Costumes != null)
                {
                    costumes.AddRange(coalition.Costumes);
                }

                costumes.TrimExcess();

                var costumeListAdapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout_ImageViewer, costumes);
                costumeListAdapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout_ImageViewer);

                costumeList.Adapter = costumeListAdapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        /*private bool CheckCensorType()
        {
            if (censorType == null)
            {
                return false;
            }

            string cType;

            switch (costumeIndex)
            {
                case 0:
                    cType = isDamage ? "D" : "N";
                    break;
                default:
                    cType = $"C{costumeIndex}";
                    cType += isDamage ? "D" : "";
                    break;
            }

            foreach (string type in censorType)
            {
                if (type == cType)
                {
                    return true;
                }
            }

            return false;
        }*/

        private async Task LoadImage(int costumeIndex, bool isRefresh = false)
        {
            await Task.Delay(100);

            string imageName = coalition.DicNumber.ToString();

            try
            {
                coalitionImageView.SetImageDrawable(null);
                imageDrawable?.Dispose();
                loadingLayout.Visibility = ViewStates.Visible;
                loadingIndicator.SmoothToShow();
                MainThread.BeginInvokeOnMainThread(() => { loadingText.SetText(Resource.String.Common_Load); });

                if (costumeIndex >= 1)
                {
                    imageName += $"_{costumeIndex + 1}";
                }

                imageName += isAwake ? "_F" : "";
                imageName += isDamage ? "_D" : "";

                /*if (coalition.HasCensored && enableCensored && (modIndex != 3))
                {
                    imageName += CheckCensorType() ? "_C" : "";
                }*/

                string imagePath = Path.Combine(ETC.cachePath, "Coalition", "Normal", $"{imageName}.gfdcache");
                string url = Path.Combine(ETC.server, "Data", "Images", "Coalition", "Normal", $"{imageName}.png");

                if (!File.Exists(imagePath) || isRefresh)
                {
                    string dTemp = Resources.GetString(Resource.String.Common_Downloading);

                    MainThread.BeginInvokeOnMainThread(() => { loadingText.Text = dTemp; });

                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += (sender, e) => { loadingText.Text = $"{dTemp}{e.ProgressPercentage}%"; };

                        await wc.DownloadFileTaskAsync(url, imagePath);
                    }
                }

                await Task.Delay(500);

                MainThread.BeginInvokeOnMainThread(() => { loadingText.SetText(Resource.String.Common_Load); });

                imageDrawable = await Drawable.CreateFromPathAsync(imagePath);

                coalitionImageView.SetImageDrawable(imageDrawable);

                string awakeText = Resources.GetString(Resource.String.CoalitionDBImageViewer_ImageStatusAwake);
                string damageText = isDamage ? Resources.GetString(Resource.String.CoalitionDBImageViewer_ImageStatusDamage) : Resources.GetString(Resource.String.CoalitionDBImageViewer_ImageStatusNormal);
                string censorText = Resources.GetString(Resource.String.CoalitionDBImageViewer_ImageCensored);

                /*imageStatus.Text = censorMenuItem.IsVisible ? $"{coalition.Name} - {costumes[costumeIndex]} - {damageText} - {censorText}{(enableCensored ? "O" : "X")}" : 
                    $"{coalition.Name} - {costumes[costumeIndex]} - {damageText}";*/

                imageStatus.Text = isAwake ? $"{coalition.Name} - {costumes[costumeIndex]} - {damageText} - {awakeText}" :
                    $"{coalition.Name} - {costumes[costumeIndex]} - {damageText}";
            }
            catch (WebException ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
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