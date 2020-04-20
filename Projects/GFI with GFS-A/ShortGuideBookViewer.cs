using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using ImageViews.Photo;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "요약 가이드북", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ShortGuideBookViewer : BaseAppCompatActivity
    {
        private ArrayAdapter imageAdapter;
        private AndroidX.Fragment.App.FragmentTransaction ft;
        private AndroidX.Fragment.App.Fragment ShortGuideBookF;

        internal DrawerLayout mainDrawerLayout;
        private ListView drawerListView;
        internal CoordinatorLayout snackbarLayout;

        readonly string[] shortGuideImageList = new string[]
            {
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A0),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A1),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A2_1),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A2_2),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A3),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A4_1),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A4_2),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A5_1),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A5_2),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A6_1),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A6_2),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_A7),
                ETC.Resources.GetString(Resource.String.ShortGuideBookViewer_EP)
            };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                //ETC.BasicInitializeApp(this);

                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_NoActionBar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.ShortGuideBookLayout);

                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.ShortGuideBookSnackbarLayout);

                mainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.ShortGuideBookMainDrawerLayout);
                mainDrawerLayout.DrawerOpened += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen); };
                mainDrawerLayout.DrawerClosed += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu); };

                SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.ShortGuideBookMainToolbar));
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetDisplayShowTitleEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);
                SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);

                drawerListView = FindViewById<ListView>(Resource.Id.ShortGuideBookImageListView);
                drawerListView.ItemClick += DrawerListView_ItemClick;

                ShortGuideBookF = new ShortGuideBookScreen();

                ft = SupportFragmentManager.BeginTransaction();
                ft.Add(Resource.Id.ShortGuideBookContainer, ShortGuideBookF, "ShortGuideBookScreen");
                ft.Commit();

                InitList();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ShortGuideBookMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
                    {
                        mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    }
                    else
                    {
                        mainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    }
                    break;
                case Resource.Id.RefreshShortGuideBookImageCache:
                    _ = (ShortGuideBookF as ShortGuideBookScreen).DownloadShortGuideImage();
                    (ShortGuideBookF as ShortGuideBookScreen).ShowImage(0);
                    break;
                case Resource.Id.ShortGuideBookExit:
                    mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    OnBackPressed();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void InitList()
        {
            try
            {
                imageAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, shortGuideImageList);
                drawerListView.Adapter = imageAdapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail List Process", ToastLength.Short).Show();
            }
        }

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                (ShortGuideBookF as ShortGuideBookScreen).ShowImage(e.Position);
                mainDrawerLayout.CloseDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        public override void OnBackPressed()
        {
            if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                mainDrawerLayout.CloseDrawer(GravityCompat.Start);

                return;
            }
            else
            {
                base.OnBackPressed();
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                GC.Collect();
            }
        }
    }

    public class ShortGuideBookScreen : AndroidX.Fragment.App.Fragment
    {
        private View v;

        private bool hasUpdate = false;

        private PhotoView guideBookImageView;
        private CoordinatorLayout snackbarLayoutF;

        readonly string[] imageName = new string[]
            {
                "SA0",
                "SA1",
                "SA2-1",
                "SA2-2",
                "SA3",
                "SA4-1",
                "SA4-2",
                "SA5-1",
                "SA5-2",
                "SA6-1",
                "SA6-2",
                "SA7",
                "EP"
            };

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater?.Inflate(Resource.Layout.ShortGuideBookScreenLayout, container, false);

            guideBookImageView = v.FindViewById<PhotoView>(Resource.Id.ShortGuideBookImageView);
            snackbarLayoutF = (Activity as ShortGuideBookViewer).snackbarLayout;

            _ = InitProcess();

            return v;
        }

        private async Task InitProcess()
        {
            try
            {
                if (CheckImage())
                {
                    await DownloadShortGuideImage();
                }

                ShowImage(0);
                _ = CheckUpdate();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(snackbarLayoutF, "Error InitProcess", Snackbar.LengthShort);
            }
        }

        internal void ShowImage(int index)
        {
            string imagePath = Path.Combine(ETC.cachePath, "GuideBook", "Images", $"{ETC.locale.Language}_{imageName[index]}.gfdcache");

            try
            {
                guideBookImageView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(imagePath));
                GC.Collect();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(snackbarLayoutF, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private bool CheckImage()
        {
            foreach (string s in imageName)
            {
                if (!File.Exists(Path.Combine(ETC.cachePath, "GuideBook", "Images", $"{ETC.locale.Language}_{s}.gfdcache")))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task CheckUpdate()
        {
            await Task.Delay(100);

            bool isMissing = false;

            try
            {
                isMissing = CheckImage();

                if (!isMissing)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string LocalVerPath = Path.Combine(ETC.systemPath, "ShortGuideVer.txt");

                        if (!File.Exists(LocalVerPath))
                        {
                            hasUpdate = true;
                        }
                        else
                        {
                            int serverVer = int.Parse(await wc.DownloadStringTaskAsync(Path.Combine(ETC.server, "ShortGuideVer.txt")));
                            int localVer = 0;

                            using (StreamReader sr = new StreamReader(new FileStream(LocalVerPath, FileMode.Open, FileAccess.Read)))
                            {
                                localVer = int.Parse(sr.ReadToEnd());
                            }

                            hasUpdate = localVer < serverVer;
                        }
                    }
                }

                if (hasUpdate || isMissing)
                {
                    var builder = new Android.Support.V7.App.AlertDialog.Builder(Activity);
                    builder.SetTitle(Resource.String.UpdateDialog_Title);
                    builder.SetMessage(Resource.String.UpdateDialog_Message);
                    builder.SetCancelable(true);
                    builder.SetPositiveButton(Resource.String.AlertDialog_Confirm, async delegate { await DownloadShortGuideImage(); });
                    builder.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

                    var dialog = builder.Create();
                    dialog.Show();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(snackbarLayoutF, Resource.String.UpdateCheck_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        internal async Task DownloadShortGuideImage()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            ProgressBar totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
            TextView totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
            ProgressBar nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
            TextView nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

            var builder = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBGDownload);
            builder.SetTitle(Resource.String.UpdateDownloadDialog_Title);
            builder.SetView(v);
            builder.SetCancelable(false);

            Dialog dialog = builder.Create();
            dialog.Show();

            await Task.Delay(100);

            try
            {
                totalProgressBar.Max = imageName.Length;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                    {
                        totalProgressBar.Progress += 1;
                        totalProgress.Text = $"{totalProgressBar.Progress.ToString()} / {totalProgressBar.Max.ToString()}";
                    };
                    wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                    {
                        nowProgressBar.Progress = e.ProgressPercentage;
                        nowProgress.Text = $"{(e.BytesReceived / 1024).ToString()}KB";
                    };

                    foreach (string s in imageName)
                    {
                        string url = Path.Combine(ETC.server, "Data", "PDF", "ShortGuideBook", "Image", ETC.locale.Language, $"{s}.png");
                        string target = Path.Combine(ETC.cachePath, "GuideBook", "Images", $"{ETC.locale.Language}_{s}.gfdcache");

                        await wc.DownloadFileTaskAsync(url, target);
                    }

                    wc.DownloadFile(Path.Combine(ETC.server, "ShortGuideVer.txt"), Path.Combine(ETC.systemPath, "ShortGuideVer.txt"));
                }

                ETC.ShowSnackbar(snackbarLayoutF, Resource.String.UpdateDownload_Complete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(snackbarLayoutF, Resource.String.UpdateDownload_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                dialog.Dismiss();
            }

            await Task.Delay(100);
        }
    }
}