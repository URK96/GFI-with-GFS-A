﻿using Android.App;
using Android.Graphics.Drawables;
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

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.OldGFDActivity", Label = "GFD v1", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class OldGFDViewer : AppCompatActivity
    {
        private ArrayAdapter imageAdapter;
        private Android.Support.V4.App.FragmentTransaction ft;
        private Android.Support.V4.App.Fragment oldGFDViewer_F;

        private DrawerLayout mainDrawerLayout;
        private ListView drawerListView;
        internal CoordinatorLayout snackbarLayout;
        private FloatingActionButton refreshFAB;

        private string[] oldGFDImageList;

        internal int initImageIndex = 0;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            { 
                if (ETC.Resources == null)
                    ETC.BasicInitializeApp(this);

                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme)
                    SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.OldGFDLayout);

                initImageIndex = Intent.GetIntExtra("Index", 0);

                // Set View & Connet Event

                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.OldGFDViewerSnackbarLayout);
                mainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.OldGFDViewerMainDrawerLayout);
                mainDrawerLayout.DrawerOpened += delegate
                {
                    if (ETC.UseLightTheme)
                        SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen_WhiteTheme);
                    else
                        SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen);
                };
                mainDrawerLayout.DrawerClosed += delegate
                {
                    if (ETC.UseLightTheme)
                        SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
                    else
                        SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);
                };
                refreshFAB = FindViewById<FloatingActionButton>(Resource.Id.OldGFDViewerRefreshFAB);
                refreshFAB.Click += delegate 
                {
                    _ = ((OldGFDViewerScreen)oldGFDViewer_F).DownloadGFDImage();
                    ((OldGFDViewerScreen)oldGFDViewer_F).ShowImage(0);
                };
                drawerListView = FindViewById<ListView>(Resource.Id.OldGFDImageListView);
                drawerListView.ItemClick += DrawerListView_ItemClick;

                // Set Action Bar

                SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.OldGFDViewerMainToolbar));
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetDisplayShowTitleEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);

                if (ETC.UseLightTheme)
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
                else
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);

                // Set Fragment

                oldGFDViewer_F = new OldGFDViewerScreen();

                ft = SupportFragmentManager.BeginTransaction();
                ft.Add(Resource.Id.OldGFDViewerContainer, oldGFDViewer_F, "OldGFDViewerScreen");
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
                    if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
                        mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    else
                        mainDrawerLayout.OpenDrawer(GravityCompat.Start);

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void InitList()
        {
            try
            {
                ListImageList();

                imageAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, oldGFDImageList);
                drawerListView.Adapter = imageAdapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail List Process", ToastLength.Short).Show();
            }
        }

        private void ListImageList()
        {
            if (ETC.Language.Language == "ko")
            {
                oldGFDImageList = new string[]
                {
                    Resources.GetString(Resource.String.OldGFDViewer_ProductDollTable),
                    Resources.GetString(Resource.String.OldGFDViewer_ProductEquipTable),
                    Resources.GetString(Resource.String.OldGFDViewer_ProductFairyTable),
                    Resources.GetString(Resource.String.OldGFDViewer_MDTable),
                    //Resources.GetString(Resource.String.OldGFDViewer_DollPerformance),
                    Resources.GetString(Resource.String.OldGFDViewer_FairyAttribute),
                    Resources.GetString(Resource.String.OldGFDViewer_RecommendDollRecipe),
                    Resources.GetString(Resource.String.OldGFDViewer_RecommendEquipRecipe),
                    Resources.GetString(Resource.String.OldGFDViewer_RecommendMD),
                    Resources.GetString(Resource.String.OldGFDViewer_RecommendLeveling),
                    Resources.GetString(Resource.String.OldGFDViewer_RecommendBreeding)
                };
            }
            else
            {
                oldGFDImageList = new string[]
                {
                    Resources.GetString(Resource.String.OldGFDViewer_ProductDollTable),
                    Resources.GetString(Resource.String.OldGFDViewer_ProductEquipTable),
                    //Resources.GetString(Resource.String.OldGFDViewer_ProductFairyTable),
                    Resources.GetString(Resource.String.OldGFDViewer_MDTable),
                    //Resources.GetString(Resource.String.OldGFDViewer_DollPerformance),
                    //Resources.GetString(Resource.String.OldGFDViewer_FairyAttribute),
                    Resources.GetString(Resource.String.OldGFDViewer_RecommendDollRecipe),
                    Resources.GetString(Resource.String.OldGFDViewer_RecommendEquipRecipe),
                    //Resources.GetString(Resource.String.OldGFDViewer_RecommendMD),
                    Resources.GetString(Resource.String.OldGFDViewer_RecommendLeveling),
                    //Resources.GetString(Resource.String.OldGFDViewer_RecommendBreeding)
                };
            }
        }

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                ((OldGFDViewerScreen)oldGFDViewer_F).ShowImage(e.Position);
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

    public class OldGFDViewerScreen : Android.Support.V4.App.Fragment
    {
        private View v;

        private bool hasUpdate = false;

        private LinearLayout imageContainer;
        private CoordinatorLayout snackbarLayout_F;

        string[] imageName = null;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.OldGFDScreenLayout, container, false);

            imageContainer = v.FindViewById<LinearLayout>(Resource.Id.OldGFDImageContainer);
            snackbarLayout_F = ((OldGFDViewer)Activity).snackbarLayout;

            _ = InitProcess();

            return v;
        }

        private async Task InitProcess()
        {
            try
            {
                InitializeImageList();

                if (CheckImage())
                    await DownloadGFDImage();

                ShowImage(((OldGFDViewer)Activity).initImageIndex);
                _ = CheckUpdate();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(snackbarLayout_F, "Error InitProcess", Snackbar.LengthShort);
            }
        }

        private void InitializeImageList()
        {
            if (ETC.Language.Language == "ko")
            {
                imageName = new string[]
                {
                    "ProductTable_Doll",
                    "ProductTable_Equipment",
                    "ProductTable_Fairy",
                    "MD_Table",
                    //"DollPerformance",
                    "FairyAttribute",
                    "RecommendDollRecipe",
                    "RecommendEquipmentRecipe",
                    "RecommendMD",
                    "RecommendLeveling",
                    "RecommendBreeding"
                };
            }
            else
            {
                imageName = new string[]
                {
                    "ProductTable_Doll",
                    "ProductTable_Equipment",
                    //"ProductTable_Fairy",
                    "MD_Table",
                    //"DollPerformance",
                    //"FairyAttribute",
                    "RecommendDollRecipe",
                    "RecommendEquipmentRecipe",
                    //"RecommendMD",
                    "RecommendLeveling",
                    //"RecommendBreeding"
                };
            }
        }

        internal void ShowImage(int index)
        {
            int height = 0;
            Drawable drawable = null;

            try
            {
                imageContainer.RemoveAllViews();

                drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "OldGFD", "Images", $"{ETC.Language.Language}_{imageName[index]}.gfdcache"));
                Android.Graphics.Bitmap bitmap = ((BitmapDrawable)drawable).Bitmap;

                while (height < bitmap.Height)
                {
                    int remainHeight = bitmap.Height - height;

                    Android.Graphics.Bitmap bitmapFix;

                    if (remainHeight >= 1000)
                    {
                        bitmapFix = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, 1000);
                        height += 1000;
                    }
                    else
                    {
                        bitmapFix = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, remainHeight);
                        height += remainHeight;
                    }

                    ImageView iv = new ImageView(Activity)
                    {
                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    };
                    iv.SetImageBitmap(bitmapFix);
                    iv.SetScaleType(ImageView.ScaleType.FitXy);
                    iv.SetAdjustViewBounds(true);

                    imageContainer.AddView(iv);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(snackbarLayout_F, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                GC.Collect();
                drawable?.Dispose();
            }
        }

        private bool CheckImage()
        {
            foreach (string s in imageName)
                if (!File.Exists(Path.Combine(ETC.CachePath, "OldGFD", "Images", $"{ETC.Language.Language}_{s}.gfdcache")))
                    return true;

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
                        string LocalDBVerPath = Path.Combine(ETC.SystemPath, "OldGFDVer.txt");

                        if (!File.Exists(LocalDBVerPath))
                            hasUpdate = true;
                        else
                        {
                            int serverVer = int.Parse(await wc.DownloadStringTaskAsync(Path.Combine(ETC.Server, "OldGFDVer.txt")));
                            int localVer = 0;

                            using (StreamReader sr = new StreamReader(new FileStream(LocalDBVerPath, FileMode.Open, FileAccess.Read)))
                                localVer = int.Parse(sr.ReadToEnd());

                            hasUpdate = localVer < serverVer;
                        }
                    }
                }

                if (hasUpdate || isMissing)
                {
                    using (var builder = new Android.Support.V7.App.AlertDialog.Builder(Activity))
                    {
                        builder.SetTitle(Resource.String.UpdateDialog_Title);
                        builder.SetMessage(Resource.String.UpdateDialog_Message);
                        builder.SetCancelable(true);
                        builder.SetPositiveButton(Resource.String.AlertDialog_Confirm, async delegate { await DownloadGFDImage(); });
                        builder.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

                        var dialog = builder.Create();
                        dialog.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(snackbarLayout_F, Resource.String.UpdateCheck_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        internal async Task DownloadGFDImage()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            ProgressBar totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
            TextView totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
            ProgressBar nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
            TextView nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

            using (var builder = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG_Download))
            {
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
                            totalProgress.Text = $"{totalProgressBar.Progress} / {totalProgressBar.Max}";
                        };
                        wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                        {
                            nowProgressBar.Progress = e.ProgressPercentage;
                            nowProgress.Text = $"{e.BytesReceived / 1024}KB";
                        };

                        foreach (string s in imageName)
                        {
                            string url = Path.Combine(ETC.Server, "Data", "Images", "OldGFD", "Images", ETC.Language.Language, $"{s}.png");
                            string target = Path.Combine(ETC.CachePath, "OldGFD", "Images", $"{ETC.Language.Language}_{s}.gfdcache");

                            await wc.DownloadFileTaskAsync(url, target);
                        }

                        wc.DownloadFile(Path.Combine(ETC.Server, "OldGFDVer.txt"), Path.Combine(ETC.SystemPath, "OldGFDVer.txt"));
                    }

                    ETC.ShowSnackbar(snackbarLayout_F, Resource.String.UpdateDownload_Complete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, Activity);
                    ETC.ShowSnackbar(snackbarLayout_F, Resource.String.UpdateDownload_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
                }
                finally
                {
                    dialog.Dismiss();
                }
            }

            await Task.Delay(100);
        }
    }
}