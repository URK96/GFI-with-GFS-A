using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using ImageViews.Photo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Com.Wang.Avi;
using Xamarin.Essentials;
using Android.Graphics.Drawables;

namespace GFI_with_GFS_A
{
    [Activity(Label = "DollDBImageViewer", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBImageViewer : BaseAppCompatActivity
    {
        private Doll doll;

        private AndroidX.AppCompat.Widget.Toolbar toolbar;

        private RelativeLayout loadingLayout;
        private AVLoadingIndicatorView loadingIndicator;
        private TextView loadingText;
        private CoordinatorLayout snackbarLayout;
        private Spinner costumeList;
        private PhotoView dollImageView;
        private TextView imageStatus;

        private List<string> costumes;

        private Drawable imageDrawable;

        private IMenuItem censorMenuItem;

        private bool isDamage = false;
        private int costumeIndex = 0;
        private int modIndex = 0;
        private bool enableCensored = true;
        private string[] censorType;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_NoActionBar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.DollDBImageViewer);

                string[] temp = Intent.GetStringExtra("Data").Split(';');

                modIndex = int.Parse(temp[1]);
                doll = new Doll(ETC.FindDataRow(ETC.dollList, "DicNumber", int.Parse(temp[0])));
                censorType = doll.HasCensored ? doll.CensorType : null;

                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.DollDBImageViewerMainToolbar);
                loadingLayout = FindViewById<RelativeLayout>(Resource.Id.DollDBImageViewerLoadingLayout);
                loadingIndicator = FindViewById<AVLoadingIndicatorView>(Resource.Id.DollDBImageViewerLoadingIndicatorView);
                loadingText = FindViewById<TextView>(Resource.Id.DollDBImageViewerLoadingIndicatorExplainText);
                dollImageView = FindViewById<PhotoView>(Resource.Id.DollDBImageViewerImageView);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBImageViewerSnackbarLayout);
                costumeList = FindViewById<Spinner>(Resource.Id.DollDBImageViewerCostumeList);
                costumeList.ItemSelected += (sender , e) =>
                {
                    isDamage = false;
                    costumeIndex = e.Position;
                    censorMenuItem.SetVisible(CheckCensorType());

                    _ = LoadImage(costumeIndex, false);
                };
                imageStatus = FindViewById<TextView>(Resource.Id.DollDBImageViewerImageStatus);

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
            MenuInflater.Inflate(Resource.Menu.DollDBImageViewerMenu, menu);

            censorMenuItem = menu?.FindItem(Resource.Id.DollDBImageViewerCensoredOption);

            censorMenuItem.SetVisible(ETC.sharedPreferences.GetBoolean("ImageCensoredUnlock", false));

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.DollDBImageViewerCensoredOption:
                    enableCensored = !enableCensored;

                    _ = LoadImage(costumeIndex, false);
                    break;
                case Resource.Id.DollDBImageViewerChangeStateButton:
                    isDamage = !isDamage;
                    censorMenuItem.SetVisible(CheckCensorType());

                    _ = LoadImage(costumeIndex, false);
                    break;
                case Resource.Id.RefreshDollImageCache:
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
                var toolbarColor = doll.Grade switch
                {
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
                    Resources.GetString(Resource.String.DollDBImageViewer_DefaultCostume)
                };

                if (doll.Costumes != null)
                {
                    costumes.AddRange(doll.Costumes);
                }

                costumes.TrimExcess();

                var CostumeListAdapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout_ImageViewer, costumes);
                CostumeListAdapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout_ImageViewer);

                costumeList.Adapter = CostumeListAdapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private bool CheckCensorType()
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
        }

        private async Task LoadImage(int costumeIndex, bool isRefresh = false)
        {
            await Task.Delay(100);

            string imageName = doll.DicNumber.ToString();

            try
            {
                dollImageView.SetImageDrawable(null);
                imageDrawable?.Dispose();
                loadingLayout.Visibility = ViewStates.Visible;
                loadingIndicator.SmoothToShow();
                MainThread.BeginInvokeOnMainThread(() => { loadingText.SetText(Resource.String.Common_Load); });

                if (costumeIndex >= 1)
                {
                    imageName += $"_{costumeIndex + 1}";
                }
                else if ((costumeIndex == 0) && (modIndex == 3))
                {
                    imageName += "_M";
                }

                imageName += isDamage ? "_D" : "";

                if (doll.HasCensored && enableCensored && (modIndex != 3))
                {
                    imageName += CheckCensorType() ? "_C" : "";
                }

                string imagePath = Path.Combine(ETC.cachePath, "Doll", "Normal", $"{imageName}.gfdcache");
                string url = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal", $"{imageName}.png");

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

                dollImageView.SetImageDrawable(imageDrawable);

                string damageText = isDamage ? Resources.GetString(Resource.String.DollDBImageViewer_ImageStatusDamage) : Resources.GetString(Resource.String.DollDBImageViewer_ImageStatusNormal);
                string censorText = Resources.GetString(Resource.String.DollDBImageViewer_ImageCensored);

                imageStatus.Text = censorMenuItem.IsVisible ? $"{doll.Name} - {costumes[costumeIndex]} - {damageText} - {censorText}{(enableCensored ? "O" : "X")}" : 
                    $"{doll.Name} - {costumes[costumeIndex]} - {damageText}";
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