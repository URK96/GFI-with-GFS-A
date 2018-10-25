using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UK.CO.Senab.Photoview;
using System.Collections.Generic;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.OldGFDActivity", Label = "OldGFDViewer", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class OldGFDViewer : FragmentActivity
    {
        private bool HasUpdate = false;

        private Spinner ImageList = null;
        private PhotoView ImageViewer = null;
        private LinearLayout ImageContainer;
        //private List<ImageView> ImageViewer_List = new List<ImageView>();
        private CoordinatorLayout SnackbarLayout = null;

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
            "DollPerformance",
            "RecommendDollRecipe",
            "RecommendEquipmentRecipe",
            "RecommendMD",
            "RecommendLeveling_1",
            "RecommendLeveling_2"
        };
        string[] SpinnerList;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                if (ETC.Resources == null) ETC.Resources = Resources;

                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.OldGFDLayout);

                ImageList = FindViewById<Spinner>(Resource.Id.OldGFDImageList);
                ImageList.ItemSelected += ImageList_ItemSelected;
                //ImageViewer = FindViewById<PhotoView>(Resource.Id.OldGFDImageView);
                ImageContainer = FindViewById<LinearLayout>(Resource.Id.OldGFDImageContainer);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.OldGFDViewerSnackbarLayout);

                InitProcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void ImageList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            int index = e.Position;
            ShowImage(index);
        }

        private async void InitProcess()
        {
            try
            {
                ListImageList();

                var ImageListAdapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, SpinnerList);
                ImageListAdapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                ImageList.Adapter = ImageListAdapter;

                ShowImage(0);

                await Task.Delay(1000);

                await CheckUpdate();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "Error InitProcess", Snackbar.LengthShort);
            }
        }

        private void ListImageList()
        {
            SpinnerList = new string[]
            {
                Resources.GetString(Resource.String.OldGFDViewer_ProductDollTable),
                Resources.GetString(Resource.String.OldGFDViewer_ProductEquipTable),
                Resources.GetString(Resource.String.OldGFDViewer_ProductFairyTable),
                Resources.GetString(Resource.String.OldGFDViewer_DollPerformance),
                Resources.GetString(Resource.String.OldGFDViewer_RecommendDollRecipe),
                Resources.GetString(Resource.String.OldGFDViewer_RecommendEquipRecipe),
                Resources.GetString(Resource.String.OldGFDViewer_RecommendMD),
                Resources.GetString(Resource.String.OldGFDViewer_RecommendLeveling1),
                Resources.GetString(Resource.String.OldGFDViewer_RecommendLeveling2)
            };
        }

        private void ShowImage(int index)
        {
            try
            {
                ImageContainer.RemoveAllViews();

                Drawable drawable = Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "OldGFD", "Images", ImageName[index] + ".gfdcache"));
                Android.Graphics.Bitmap bitmap = ((BitmapDrawable)drawable).Bitmap;

                if (bitmap.Height > 1000)
                {
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

                        ImageView iv = new ImageView(this);
                        iv.SetImageBitmap(bitmap_fix);
                        iv.SetScaleType(ImageView.ScaleType.FitXy);
                        iv.SetAdjustViewBounds(true);

                        ImageContainer.AddView(iv);
                    }
                }

                //ImageViewer.SetImageDrawable(drawable);

                GC.Collect();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task CheckUpdate()
        {
            await Task.Delay(100);

            bool IsMissing = false;

            try
            {
                foreach (string s in ImageName)
                {
                    if (File.Exists(Path.Combine(ETC.CachePath, "OldGFD", "Images", s + ".gfdcache")) == false)
                    {
                        IsMissing = true;
                        break;
                    }
                }

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
                            {
                                local_ver = int.Parse(sr.ReadToEnd());
                            }

                            if (local_ver < server_ver) HasUpdate = true;
                            else HasUpdate = false;
                        }
                    }
                }

                if ((HasUpdate == true) || (IsMissing == true))
                {
                    Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
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
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.UpdateCheck_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task DownloadGFDImage()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
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
                        string url = Path.Combine(ETC.Server, "Data", "Images", "OldGFD", "Images", s + ".png");
                        string target = Path.Combine(ETC.CachePath, "OldGFD", "Images", s + ".gfdcache");

                        await wc.DownloadFileTaskAsync(url, target);
                    }

                    wc.DownloadFile(Path.Combine(ETC.Server, "OldGFDVer.txt"), Path.Combine(ETC.SystemPath, "OldGFDVer.txt"));
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.UpdateDownload_Complete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.UpdateDownload_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
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

            ShowImage(ImageList.SelectedItemPosition);
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            p_now += 1;

            totalProgressBar.Progress = Convert.ToInt32((p_now / Convert.ToDouble(p_total)) * 100);
            totalProgress.Text = string.Format("{0}%", totalProgressBar.Progress);
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            nowProgressBar.Progress = e.ProgressPercentage;
            nowProgress.Text = string.Format("{0}%", e.ProgressPercentage);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }
}