using Android.App;
using Android.Content;
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
using Xfinium.Pdf.View;

namespace GFI_with_GFS_A
{
    [Activity(Label = "GuideBookViewer", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GuideBookViewer : AppCompatActivity
    {
        private ArrayAdapter PDF_Adapter;

        internal DrawerLayout MainDrawerLayout;
        private ListView DrawerListView;
        private PdfCoreView PDFViewer;
        internal CoordinatorLayout SnackbarLayout;

        private string[] GuideBookPDFList;
        private string[] PDFName;

        private bool HasUpdate = false;

        private Dialog dialog = null;
        private ProgressBar totalProgressBar = null;
        private ProgressBar nowProgressBar = null;
        private TextView totalProgress = null;
        private TextView nowProgress = null;

        int p_now = 0;
        int p_total = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.GuideBookViewerLayout);

            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.GuideBookViewerSnackbarLayout);

            MainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.GuideBookViewerMainDrawerLayout);
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

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.GuideBookViewerMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            if (ETC.UseLightTheme == true) SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
            else SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);

            DrawerListView = FindViewById<ListView>(Resource.Id.GuideBookPDFListView);
            DrawerListView.ItemClick += DrawerListView_ItemClick;

            PDFViewer = FindViewById<PdfCoreView>(Resource.Id.GuidBookViewerPDFViewer);
            PDFViewer.GraphicRendererFactory = new Xfinium.Graphics.Skia.SkiaRendererFactory();
            PDFViewer.Document = new PdfVisualDocument();
            PDFViewer.ZoomMode = PdfZoomMode.FitWidth;
            PDFViewer.FitWidthOnDoubleTap = true;

            _ = InitProcess();
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
                GuideBookPDFList = new string[]
                {
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartA),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartB_1),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartB_2),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartB_3),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartB_4),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartB_5),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartB_6),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartC),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartD),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartE_1),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartE_2),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartE_3),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartG),
                    Resources.GetString(Resource.String.GuideBookViewer_PDF_PartJ)
                };
                PDFName = new string[]
                {
                    "PartA",
                    "PartB1",
                    "PartB2",
                    "PartB3",
                    "PartB4",
                    "PartB5",
                    "PartB6",
                    "PartC",
                    "PartD",
                    "PartE1",
                    "PartE2",
                    "PartE3",
                    "PartG",
                    "PartJ",
                };

                PDF_Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, GuideBookPDFList);
                DrawerListView.Adapter = PDF_Adapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail List Process", ToastLength.Short).Show();
            }
        }

        private async Task InitProcess()
        {
            try
            {
                InitList();

                if (CheckPDF() == true) await DownloadGuideBookPDF();

                _ = CheckUpdate();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, "Error InitProcess", Snackbar.LengthShort);
            }
        }

        private bool CheckPDF()
        {
            foreach (string s in PDFName)
                if (File.Exists(Path.Combine(ETC.CachePath, "GuideBook", "PDFs", $"{s}.pdf")) == false)
                    return true;

            return false;
        }

        internal void ShowPDF(int index)
        {
            try
            {
                //PDFViewer.Unload();
                //PDFViewer.LoadDocument(new FileStream(Path.Combine(ETC.CachePath, "GuideBook", "PDFs", $"{PDFName[index]}.gfdcache"), FileMode.Open, FileAccess.Read));

                //Assembly asm = Assembly.GetExecutingAssembly();
                //PDFViewer.Document.Load(new FileStream(Path.Combine(ETC.CachePath, "GuideBook", "PDFs", $"{PDFName[index]}.gfdcache"), FileMode.Open, FileAccess.Read));

                Intent intent = new Intent();
                intent.AddFlags(ActivityFlags.NewTask);
                intent.SetAction(Intent.ActionView);
                //intent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(Path.Combine(ETC.CachePath, "GuideBook", "PDFs", $"{PDFName[index]}.pdf")));
                intent.SetFlags(ActivityFlags.ClearTop);
                intent.SetType("application/pdf");
                intent.SetDataAndType(Android.Net.Uri.Parse(Path.Combine(ETC.CachePath, "GuideBook", "PDFs", $"{PDFName[index]}.pdf")), "application/pdf");
                StartActivity(Intent.CreateChooser(intent, "Open PDF"));
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                ShowPDF(e.Position);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private async Task CheckUpdate()
        {
            await Task.Delay(100);

            bool IsMissing = false;

            try
            {
                IsMissing = CheckPDF();

                if (IsMissing == false)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string LocalDBVerPath = Path.Combine(ETC.SystemPath, "GuideBookVer.txt");

                        if (File.Exists(LocalDBVerPath) == false) HasUpdate = true;
                        else
                        {
                            int server_ver = int.Parse(await wc.DownloadStringTaskAsync(Path.Combine(ETC.Server, "GuideBookVer.txt")));
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
                    Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                    builder.SetTitle(Resource.String.UpdateDialog_Title);
                    builder.SetMessage(Resource.String.UpdateDialog_Message);
                    builder.SetCancelable(true);
                    builder.SetPositiveButton(Resource.String.AlertDialog_Confirm, async delegate 
                    {
                        //PDFViewer.Unload();
                        await DownloadGuideBookPDF();
                    });
                    builder.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

                    var dialog = builder.Create();
                    dialog.Show();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.UpdateCheck_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task DownloadGuideBookPDF()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Download);
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

                p_total = PDFName.Length;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;

                    foreach (string s in PDFName)
                    {
                        string url = Path.Combine(ETC.Server, "Data", "PDF", "GuideBook", $"{s}.pdf");
                        string target = Path.Combine(ETC.CachePath, "GuideBook", "PDFs", $"{s}.pdf");

                        await wc.DownloadFileTaskAsync(url, target);
                    }

                    wc.DownloadFile(Path.Combine(ETC.Server, "GuideBookVer.txt"), Path.Combine(ETC.SystemPath, "GuideBookVer.txt"));
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.UpdateDownload_Complete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
            nowProgress.Text = $"{e.BytesReceived / 1024}KB";
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
                //PDFViewer.Unload();
                base.OnBackPressed();
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                GC.Collect();
            }
        }
    }
}