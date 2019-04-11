using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using System.IO;
using System.Threading.Tasks;
using Syncfusion.SfPdfViewer.Android;
using System.Net;

namespace GFI_with_GFS_A
{
    [Activity(Label = "PDFViewer", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PDFViewer : AppCompatActivity
    {
        SfPdfViewer viewer;

        string pdf_path = "";

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PDFViewerLayout);

            pdf_path = Intent.GetStringExtra("path");

            if (File.Exists(pdf_path) == false)
                await DownloadPDF();

            viewer = FindViewById<SfPdfViewer>(Resource.Id.PDFViewer);
            viewer.LoadDocument(new FileStream("", FileMode.Open, FileAccess.Read));
        }

        private async Task DownloadPDF()
        {
            await Task.Delay(100);

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Download);
            ad.SetTitle(Resource.String.PDFViewer_DownloadPDFTitle);
            ad.SetMessage(Resource.String.PDFViewer_DownloadPDFMessage);
            ad.SetCancelable(false);
            ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);

            Dialog dialog = ad.Show();

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => 
                    {
                        string message = Resources.GetString(Resource.String.PDFViewer_DownloadPDFMessage);
                        RunOnUiThread(() => { ad.SetMessage($"{message}({e.BytesReceived / 1024}KB)"); });
                    };
                    await wc.DownloadFileTaskAsync("", pdf_path); //URL 경로 추가
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, "Download Fail", ToastLength.Short).Show();
            }
            finally
            {
                dialog.Dismiss();
            }
        }

        public override void OnBackPressed()
        {
            viewer.Unload();
            viewer.Dispose();
            base.OnBackPressed();
        }
    }
}