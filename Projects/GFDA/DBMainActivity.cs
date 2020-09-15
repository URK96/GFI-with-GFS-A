using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;
using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Snackbar;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFDA
{
    [Activity(Label = "@string/Activity_DollMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DBMainActivity : BaseAppCompatActivity
    {
        internal delegate void DownloadProcess<T>(List<T> downloadList, string serverPath, string targetPath);
        internal delegate Task ListProcess();

        internal ListProcess ListItem = null;

        internal enum SortType { Name, Number, ProductTime, HP, FR, EV, AC, AS }
        internal enum SortOrder { Ascending, Descending }

        internal SortType sortType = SortType.Name;
        internal SortOrder sortOrder = SortOrder.Ascending;

        internal bool canRefresh = false;
        internal string searchViewText = "";

        private AndroidX.AppCompat.Widget.Toolbar toolbar;
        private AndroidX.AppCompat.Widget.SearchView searchView;
        internal RecyclerView recyclerView;
        internal CoordinatorLayout snackbarLayout;

        internal RecyclerView.Adapter adapter;
        
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
                SetContentView(Resource.Layout.DBMainLayout);

                canRefresh = Preferences.Get("DBListImageShow", false);

                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.DBMainToolbar);
                searchView = FindViewById<AndroidX.AppCompat.Widget.SearchView>(Resource.Id.DBSearchView);
                searchView.QueryTextChange += (sender, e) =>
                {
                    searchViewText = e.NewText;

                    _ = ListItem();
                };
                recyclerView = FindViewById<RecyclerView>(Resource.Id.DBRecyclerView);
                recyclerView.SetLayoutManager(new LinearLayoutManager(this));
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DBSnackbarLayout);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetTitle(Resource.String.DollDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                /*if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true)))
                {
                    ETC.RunHelpActivity(this, "DBList");
                }*/
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.DBMenu, menu);

            var cacheItem = menu?.FindItem(Resource.Id.RefreshCropImageCache);
            _ = canRefresh ? cacheItem.SetVisible(true) : cacheItem.SetVisible(false);

            return base.OnCreateOptionsMenu(menu);
        }

        internal void RefreshAdapter() => adapter.NotifyDataSetChanged();

        internal void ShowDownloadCheckMessage<T>(List<T> downloadList, string serverPath, string targetPath)
        {
            var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(this, ETC.dialogBG);
            ad.SetTitle(Resource.String.DBList_DownloadCropImageCheckTitle);
            ad.SetMessage(Resource.String.DBList_DownloadCropImageCheckMessage);
            ad.SetCancelable(true);
            ad.SetPositiveButton(Resource.String.AlertDialog_Download, delegate { _ = CropImageDownloadProcess(downloadList, serverPath, targetPath); });
            ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

            ad.Show();
        }

        private async Task CropImageDownloadProcess<T>(List<T> downloadList, string serverPath, string targetPath)
        {
            Dialog dialog;
            ProgressBar totalProgressBar;
            ProgressBar nowProgressBar;
            TextView totalProgress;
            TextView nowProgress;

            var v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            int pNow = 0;
            int pTotal = 0;

            var pd = new AndroidX.AppCompat.App.AlertDialog.Builder(this, ETC.dialogBGDownload);   
            pd.SetTitle(Resource.String.DBList_DownloadCropImageTitle);
            pd.SetCancelable(false);
            pd.SetView(v);

            dialog = pd.Create();
            dialog.Show();

            try
            {
                totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
                totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
                nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
                nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

                pTotal = downloadList.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = pNow;

                using (var wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (sender, e) =>
                    {
                        nowProgressBar.Progress = e.ProgressPercentage;

                        MainThread.BeginInvokeOnMainThread(() => { nowProgress.Text = $"{e.ProgressPercentage}%"; });
                    };
                    wc.DownloadFileCompleted += (sender, e) =>
                    {
                        pNow += 1;
                        totalProgressBar.Progress = Convert.ToInt32((pNow / Convert.ToDouble(pTotal)) * 100);

                        MainThread.BeginInvokeOnMainThread(() => { totalProgress.Text = $"{totalProgressBar.Progress}%"; });
                    };

                    for (int i = 0; i < pTotal; ++i)
                    {
                        string url = Path.Combine(serverPath, $"{downloadList[i]}.png");
                        string target = Path.Combine(targetPath, $"{downloadList[i]}.gfdcache");

                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                _ = ListItem();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_DownloadCropImageFail, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
            finally
            {
                dialog.Dismiss();
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }
}