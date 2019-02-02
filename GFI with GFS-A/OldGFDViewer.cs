using Android.App;
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
        private ArrayAdapter Image_Adapter;
        private Android.Support.V4.App.FragmentTransaction ft;
        private Android.Support.V4.App.Fragment OldGFDViewer_F;

        internal DrawerLayout MainDrawerLayout;
        private ListView DrawerListView;
        internal CoordinatorLayout SnackbarLayout;

        string[] OldGFDImageList;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                if (ETC.Resources == null) ETC.Resources = Resources;

                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.OldGFDLayout);

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.OldGFDViewerSnackbarLayout);

                MainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.OldGFDViewerMainDrawerLayout);
                MainDrawerLayout.DrawerOpened += delegate
                {
                    if (ETC.UseLightTheme == true) SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen_WhiteTheme);
                    else SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen);
                };
                MainDrawerLayout.DrawerClosed += delegate
                {
                    if (ETC.UseLightTheme == true) SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
                    else SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);
                };

                SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.OldGFDViewerMainToolbar));
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetDisplayShowTitleEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);
                if (ETC.UseLightTheme == true) SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
                else SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);

                DrawerListView = FindViewById<ListView>(Resource.Id.OldGFDImageListView);
                DrawerListView.ItemClick += DrawerListView_ItemClick;

                OldGFDViewer_F = new OldGFDViewerScreen();

                ft = SupportFragmentManager.BeginTransaction();
                ft.Add(Resource.Id.OldGFDViewerContainer, OldGFDViewer_F, "OldGFDViewerScreen");
                ft.Commit();

                InitList();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
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
                    else MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void InitList()
        {
            try
            {
                ListImageList();

                Image_Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, OldGFDImageList);
                DrawerListView.Adapter = Image_Adapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, "Fail List Process", ToastLength.Short).Show();
            }
        }

        private void ListImageList()
        {
            OldGFDImageList = new string[]
            {
                Resources.GetString(Resource.String.OldGFDViewer_ProductDollTable),
                Resources.GetString(Resource.String.OldGFDViewer_ProductEquipTable),
                Resources.GetString(Resource.String.OldGFDViewer_ProductFairyTable),
                //Resources.GetString(Resource.String.OldGFDViewer_DollPerformance),
                Resources.GetString(Resource.String.OldGFDViewer_FairyAttribute),
                Resources.GetString(Resource.String.OldGFDViewer_RecommendDollRecipe),
                Resources.GetString(Resource.String.OldGFDViewer_RecommendEquipRecipe),
                Resources.GetString(Resource.String.OldGFDViewer_RecommendMD),
                Resources.GetString(Resource.String.OldGFDViewer_RecommendLeveling),
            };
        }

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                ((OldGFDViewerScreen)OldGFDViewer_F).ShowImage(e.Position);
                MainDrawerLayout.CloseDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
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

    public class OldGFDViewerScreen : Android.Support.V4.App.Fragment
    {
        private View v;

        private bool HasUpdate = false;
        private int Image_Index = 0;

        private LinearLayout ImageContainer;
        private CoordinatorLayout SnackbarLayout_F;

        private Dialog dialog = null;
        private ProgressBar totalProgressBar = null;
        private ProgressBar nowProgressBar = null;
        private TextView totalProgress = null;
        private TextView nowProgress = null;

        int p_now = 0;
        int p_total = 0;

        string[] ImageName =
        {
            "ProductTable_Doll",
            "ProductTable_Equipment",
            "ProductTable_Fairy",
            //"DollPerformance",
            "FairyAttribute",
            "RecommendDollRecipe",
            "RecommendEquipmentRecipe",
            "RecommendMD",
            "RecommendLeveling"
        };

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.OldGFDScreenLayout, container, false);

            ImageContainer = v.FindViewById<LinearLayout>(Resource.Id.OldGFDImageContainer);
            SnackbarLayout_F = ((OldGFDViewer)Activity).SnackbarLayout;

            InitProcess();

            return v;
        }

        private void ImageList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            int index = e.Position;
            ShowImage(index);
        }

        private async Task InitProcess()
        {
            try
            {
                if (CheckImage() == true) await DownloadGFDImage();

                ShowImage(0);
                CheckUpdate();
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout_F, "Error InitProcess", Snackbar.LengthShort);
            }
        }

        internal void ShowImage(int index)
        {
            try
            {
                Image_Index = index;
                ImageContainer.RemoveAllViews();

                Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "OldGFD", "Images", $"{ImageName[index]}.gfdcache"));
                Android.Graphics.Bitmap bitmap = ((BitmapDrawable)drawable).Bitmap;

                int height = 0;

                while (height < bitmap.Height)
                {
                    int remain_height = bitmap.Height - height;

                    Android.Graphics.Bitmap bitmap_fix;

                    if (remain_height >= 1000)
                    {
                        bitmap_fix = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, 1000);
                        height += 1000;
                    }
                    else
                    {
                        bitmap_fix = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, remain_height);
                        height += remain_height;
                    }

                    ImageView iv = new ImageView(Activity);
                    iv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                    iv.SetImageBitmap(bitmap_fix);
                    iv.SetScaleType(ImageView.ScaleType.FitXy);
                    iv.SetAdjustViewBounds(true);

                    ImageContainer.AddView(iv);
                }

                GC.Collect();
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout_F, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private bool CheckImage()
        {
            foreach (string s in ImageName)
                if (File.Exists(Path.Combine(ETC.CachePath, "OldGFD", "Images", $"{s}.gfdcache")) == false)
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

                if (IsMissing == false)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string LocalDBVerPath = Path.Combine(ETC.SystemPath, "OldGFDVer.txt");

                        if (File.Exists(LocalDBVerPath) == false) HasUpdate = true;
                        else
                        {
                            int server_ver = int.Parse(await wc.DownloadStringTaskAsync(Path.Combine(ETC.Server, "OldGFDVer.txt")));
                            int local_ver = 0;

                            using (StreamReader sr = new StreamReader(new FileStream(LocalDBVerPath, FileMode.Open, FileAccess.Read)))
                                local_ver = int.Parse(sr.ReadToEnd());

                            if (local_ver < server_ver) HasUpdate = true;
                            else HasUpdate = false;
                        }
                    }
                }

                if ((HasUpdate == true) || (IsMissing == true))
                {
                    Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(Activity);
                    builder.SetTitle(Resource.String.UpdateDialog_Title);
                    builder.SetMessage(Resource.String.UpdateDialog_Message);
                    builder.SetCancelable(true);
                    builder.SetPositiveButton(Resource.String.AlertDialog_Confirm, async delegate { await DownloadGFDImage(); });
                    builder.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

                    var dialog = builder.Create();
                    dialog.Show();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout_F, Resource.String.UpdateCheck_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task DownloadGFDImage()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG_Download);
            builder.SetTitle(Resource.String.UpdateDownloadDialog_Title);
            builder.SetView(v);

            dialog = builder.Create();
            dialog.Show();

            await Task.Delay(100);

            try
            {
                totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
                totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
                nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
                nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

                p_total = ImageName.Length;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;

                    foreach (string s in ImageName)
                    {
                        string url = Path.Combine(ETC.Server, "Data", "Images", "OldGFD", "Images", $"{s}.png");
                        string target = Path.Combine(ETC.CachePath, "OldGFD", "Images", $"{s}.gfdcache");

                        await wc.DownloadFileTaskAsync(url, target);
                    }

                    wc.DownloadFile(Path.Combine(ETC.Server, "OldGFDVer.txt"), Path.Combine(ETC.SystemPath, "OldGFDVer.txt"));
                }

                ETC.ShowSnackbar(SnackbarLayout_F, Resource.String.UpdateDownload_Complete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout_F, Resource.String.UpdateDownload_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                dialog.Dismiss();
                dialog = null;
                totalProgressBar = null;
                totalProgress = null;
                nowProgressBar = null;
                nowProgress = null;
            }

            //ShowImage(Image_Index);
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            p_now += 1;

            totalProgressBar.Progress = Convert.ToInt32(p_now / Convert.ToDouble(p_total) * 100);
            totalProgress.Text = $"{totalProgressBar.Progress}%";
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            nowProgressBar.Progress = e.ProgressPercentage;
            nowProgress.Text = $"{e.ProgressPercentage}%";
        }
    }
}