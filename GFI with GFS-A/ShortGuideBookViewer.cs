using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UK.CO.Senab.Photoview;

namespace GFI_with_GFS_A
{
    [Activity(Label = "요약 가이드북", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ShortGuideBookViewer : AppCompatActivity
    {
        private ArrayAdapter ImageAdapter;
        private Android.Support.V4.App.FragmentTransaction ft;
        private Android.Support.V4.App.Fragment ShortGuideBook_F;

        internal DrawerLayout MainDrawerLayout;
        private ListView DrawerListView;
        internal CoordinatorLayout SnackbarLayout;
        private FloatingActionButton RefreshFAB;

        string[] ShortGuideImageList = new string[]
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

                if (ETC.UseLightTheme)
                    SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.ShortGuideBookLayout);

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.ShortGuideBookSnackbarLayout);

                MainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.ShortGuideBookMainDrawerLayout);
                MainDrawerLayout.DrawerOpened += delegate
                {
                    if (ETC.UseLightTheme)
                        SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen_WhiteTheme);
                    else
                        SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen);
                };
                MainDrawerLayout.DrawerClosed += delegate
                {
                    if (ETC.UseLightTheme)
                        SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
                    else
                        SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);
                };
                RefreshFAB = FindViewById<FloatingActionButton>(Resource.Id.ShortGuideBookRefreshFAB);
                RefreshFAB.Click += delegate
                {
                    _ = ((ShortGuideBookScreen)ShortGuideBook_F).DownloadShortGuideImage();
                    ((ShortGuideBookScreen)ShortGuideBook_F).ShowImage(0);
                };

                SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.ShortGuideBookMainToolbar));
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetDisplayShowTitleEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);
                if (ETC.UseLightTheme)
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
                else
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);

                DrawerListView = FindViewById<ListView>(Resource.Id.ShortGuideBookImageListView);
                DrawerListView.ItemClick += DrawerListView_ItemClick;

                ShortGuideBook_F = new ShortGuideBookScreen();

                ft = SupportFragmentManager.BeginTransaction();
                ft.Add(Resource.Id.ShortGuideBookContainer, ShortGuideBook_F, "ShortGuideBookScreen");
                ft.Commit();

                InitList();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == false)
                        MainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    else
                        MainDrawerLayout.CloseDrawer(GravityCompat.Start);

                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void InitList()
        {
            try
            {
                ImageAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, ShortGuideImageList);
                DrawerListView.Adapter = ImageAdapter;
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
                ((ShortGuideBookScreen)ShortGuideBook_F).ShowImage(e.Position);
                MainDrawerLayout.CloseDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        public override void OnBackPressed()
        {
            if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == true)
            {
                MainDrawerLayout.CloseDrawer(GravityCompat.Start);
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

    public class ShortGuideBookScreen : Android.Support.V4.App.Fragment
    {
        private View v;

        private bool HasUpdate = false;

        private PhotoView GuideBookImageView;
        private CoordinatorLayout SnackbarLayout_F;

        string[] ImageName = new string[]
            {
                "OP",
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
            v = inflater.Inflate(Resource.Layout.ShortGuideBookScreenLayout, container, false);

            GuideBookImageView = v.FindViewById<PhotoView>(Resource.Id.ShortGuideBookImageView);
            SnackbarLayout_F = ((ShortGuideBookViewer)Activity).SnackbarLayout;

            _ = InitProcess();

            return v;
        }

        private async Task InitProcess()
        {
            try
            {
                if (CheckImage())
                    await DownloadShortGuideImage();

                ShowImage(0);
                _ = CheckUpdate();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(SnackbarLayout_F, "Error InitProcess", Snackbar.LengthShort);
            }
        }

        internal void ShowImage(int index)
        {
            string ImagePath = Path.Combine(ETC.CachePath, "GuideBook", "Images", $"{ETC.Language.Language}_{ImageName[index]}.gfdcache");

            try
            {
                GuideBookImageView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(ImagePath));
                GC.Collect();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(SnackbarLayout_F, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private bool CheckImage()
        {
            foreach (string s in ImageName)
                if (!File.Exists(Path.Combine(ETC.CachePath, "GuideBook", "Images", $"{ETC.Language.Language}_{s}.gfdcache")))
                    return true;

            return false;
        }

        private async Task CheckUpdate()
        {
            await Task.Delay(100);

            bool IsMissing = false;

            try
            {
                IsMissing = CheckImage();

                if (!IsMissing)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string LocalVerPath = Path.Combine(ETC.SystemPath, "ShortGuideVer.txt");

                        if (!File.Exists(LocalVerPath))
                            HasUpdate = true;
                        else
                        {
                            int server_ver = int.Parse(await wc.DownloadStringTaskAsync(Path.Combine(ETC.Server, "ShortGuideVer.txt")));
                            int local_ver = 0;

                            using (StreamReader sr = new StreamReader(new FileStream(LocalVerPath, FileMode.Open, FileAccess.Read)))
                                local_ver = int.Parse(sr.ReadToEnd());

                            if (local_ver < server_ver)
                                HasUpdate = true;
                            else
                                HasUpdate = false;
                        }
                    }
                }

                if (HasUpdate || IsMissing)
                {
                    Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(Activity);
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
                ETC.ShowSnackbar(SnackbarLayout_F, Resource.String.UpdateCheck_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        internal async Task DownloadShortGuideImage()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            ProgressBar totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
            TextView totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
            ProgressBar nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
            TextView nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG_Download);
            builder.SetTitle(Resource.String.UpdateDownloadDialog_Title);
            builder.SetView(v);
            builder.SetCancelable(false);

            Dialog dialog = builder.Create();
            dialog.Show();

            await Task.Delay(100);

            try
            {
                totalProgressBar.Max = ImageName.Length;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                    {
                        totalProgressBar.Progress += 1;
                        totalProgress.Text = $"{totalProgressBar.Progress} / {totalProgressBar.Max}";
                    };
                    wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                    {
                        nowProgressBar.Progress = e.ProgressPercentage;
                        nowProgress.Text = $"{e.BytesReceived / 1024}KB";
                    };

                    foreach (string s in ImageName)
                    {
                        string url = Path.Combine(ETC.Server, "Data", "PDF", "ShortGuideBook", "Image", ETC.Language.Language, $"{s}.png");
                        string target = Path.Combine(ETC.CachePath, "GuideBook", "Images", $"{ETC.Language.Language}_{s}.gfdcache");

                        await wc.DownloadFileTaskAsync(url, target);
                    }

                    wc.DownloadFile(Path.Combine(ETC.Server, "ShortGuideVer.txt"), Path.Combine(ETC.SystemPath, "ShortGuideVer.txt"));
                }

                ETC.ShowSnackbar(SnackbarLayout_F, Resource.String.UpdateDownload_Complete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(SnackbarLayout_F, Resource.String.UpdateDownload_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                dialog.Dismiss();
            }

            await Task.Delay(100);
        }
    }
}