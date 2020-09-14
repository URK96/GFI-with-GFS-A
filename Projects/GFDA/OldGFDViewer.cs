using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;

using Google.Android.Material.Snackbar;

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFDA
{
    [Activity(Name = "com.gfl.dic.OldGFDActivity", Label = "GFD v1", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class OldGFDViewer : BaseAppCompatActivity
    {
        private ArrayAdapter imageAdapter;
        private AndroidX.Fragment.App.FragmentTransaction ft;
        private AndroidX.Fragment.App.Fragment oldGFDViewerF;

        private DrawerLayout mainDrawerLayout;
        private ListView drawerListView;
        internal CoordinatorLayout snackbarLayout;

        private string[] oldGFDImageList;

        internal int initImageIndex = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                if (ETC.Resources == null)
                {
                    ETC.BasicInitializeApp(this);
                }

                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Toolbar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.OldGFDMainLayout);

                initImageIndex = Intent.GetIntExtra("Index", 0);

                // Set View & Connet Event

                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.OldGFDViewerSnackbarLayout);
                mainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.OldGFDViewerMainDrawerLayout);
                mainDrawerLayout.DrawerOpened += delegate
                {
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen);                        
                };
                mainDrawerLayout.DrawerClosed += delegate
                {
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);
                };
                drawerListView = FindViewById<ListView>(Resource.Id.OldGFDImageListView);
                drawerListView.ItemClick += DrawerListView_ItemClick;

                // Set Action Bar

                SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.OldGFDViewerMainToolbar));
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.Title = "GFD v1";
                SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);

                // Set Fragment

                oldGFDViewerF = new OldGFDViewerScreen();

                ft = SupportFragmentManager.BeginTransaction();
                ft.Add(Resource.Id.OldGFDViewerContainer, oldGFDViewerF, "OldGFDViewerScreen");
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
            try
            {
                MenuInflater.Inflate(Resource.Menu.OldGFDMenu, menu);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Cannot create option menu", ToastLength.Short).Show();
            }

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
                case Resource.Id.RefreshOldGFDImageCache:
                    _ = ((OldGFDViewerScreen)oldGFDViewerF).DownloadGFDImage();
                    ((OldGFDViewerScreen)oldGFDViewerF).ShowImage(0);
                    break;
                case Resource.Id.OldGFDExit:
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
            if (ETC.locale.Language == "ko")
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
                ((OldGFDViewerScreen)oldGFDViewerF).ShowImage(e.Position);
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

    public class OldGFDViewerScreen : AndroidX.Fragment.App.Fragment
    {
        private View v;

        private bool hasUpdate = false;

        private LinearLayout imageContainer;
        private CoordinatorLayout snackbarLayout_F;

        string[] imageName = null;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater?.Inflate(Resource.Layout.OldGFDScreenLayout, container, false);

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
            if (ETC.locale.Language == "ko")
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

                drawable = Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "OldGFD", "Images", $"{ETC.locale.Language}_{imageName[index]}.gfdcache"));
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
            {
                if (!File.Exists(Path.Combine(ETC.cachePath, "OldGFD", "Images", $"{ETC.locale.Language}_{s}.gfdcache")))
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
                        string LocalDBVerPath = Path.Combine(ETC.systemPath, "OldGFDVer.txt");

                        if (!File.Exists(LocalDBVerPath))
                        {
                            hasUpdate = true;
                        }
                        else
                        {
                            int serverVer = int.Parse(await wc.DownloadStringTaskAsync(Path.Combine(ETC.server, "OldGFDVer.txt")));
                            int localVer = 0;

                            using (StreamReader sr = new StreamReader(new FileStream(LocalDBVerPath, FileMode.Open, FileAccess.Read)))
                            {
                                localVer = int.Parse(sr.ReadToEnd());
                            }

                            hasUpdate = localVer < serverVer;
                        }
                    }
                }

                if (hasUpdate || isMissing)
                {
                    var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
                    builder.SetTitle(Resource.String.UpdateDialog_Title);
                    builder.SetMessage(Resource.String.UpdateDialog_Message);
                    builder.SetCancelable(true);
                    builder.SetPositiveButton(Resource.String.AlertDialog_Confirm, async delegate { await DownloadGFDImage(); });
                    builder.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

                    builder.Create().Show();
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

            var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBGDownload);
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
                        string url = Path.Combine(ETC.server, "Data", "Images", "OldGFD", "Images", ETC.locale.Language, $"{s}.png");
                        string target = Path.Combine(ETC.cachePath, "OldGFD", "Images", $"{ETC.locale.Language}_{s}.gfdcache");

                        await wc.DownloadFileTaskAsync(url, target);
                    }

                    wc.DownloadFile(Path.Combine(ETC.server, "OldGFDVer.txt"), Path.Combine(ETC.systemPath, "OldGFDVer.txt"));
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
            
            await Task.Delay(100);
        }
    }
}