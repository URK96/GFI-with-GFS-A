﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "GFDInfoActivity", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GFDInfoActivity : AppCompatActivity
    {
        Button UpdateButton;
        TextView NowVersion;
        TextView ServerVersion;
        CoordinatorLayout SnackbarLayout;

        ProgressDialog DownloadProgress;

        FloatingActionButton KakaoPlusFriendFAB;

        private bool IsOpenFABMenu = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.GFDInfoLayout);

                UpdateButton = FindViewById<Button>(Resource.Id.GFDInfo_AppUpdateButton);
                UpdateButton.Click += UpdateButton_Click;
                NowVersion = FindViewById<TextView>(Resource.Id.GFDInfo_NowAppVersion);
                ServerVersion = FindViewById<TextView>(Resource.Id.GFDInfo_ServerAppVersion);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.GFDInfoSnackbarLayout);

                KakaoPlusFriendFAB = FindViewById<FloatingActionButton>(Resource.Id.KakaoPlusFriendFAB);
                KakaoPlusFriendFAB.Click += KakaoPlusFriendFAB_Click;

                Initialize();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void KakaoPlusFriendFAB_Click(object sender, EventArgs e)
        {
            try
            {
                string url = "https://pf.kakao.com/_JEcmC/chat";
                var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void Initialize()
        {
            try
            {
                InitDeveloperInfo();
                CheckAppVersion();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void InitDeveloperInfo()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("총괄 & 디자인 : 수매\n");
                sb.Append("eveia@naver.com\n\n");
                sb.Append("App & 소스 제작 : URK96\n");
                sb.Append("chlwlsgur96@hotmail.com\n\n");
                sb.Append("App 검수 & 확인 : 스텡\n");
                sb.Append("yuiroa@naver.com");

                FindViewById<TextView>(Resource.Id.GFDInfoDeveloperInfo).Text = sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private async Task CheckAppVersion()
        {
            bool HasUpdate = false;
            await Task.Delay(100);

            try
            {
                var context = ApplicationContext;
                string[] now_ver = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName.Split('.');
                string[] server_ver = new string[now_ver.Length];

                NowVersion.Text = "현재 버전 : " + context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName + " - ";
#if DEBUG
                NowVersion.Text += "Debug";
#else
                NowVersion.Text += "Release";
#endif
                string url = Path.Combine(ETC.Server, "GFD_AppVer.txt");
                string target = Path.Combine(ETC.tempPath, "AppVer.txt");

                using (WebClient wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(url, target);
                }

                using (StreamReader sr = new StreamReader(new FileStream(target, FileMode.Open, FileAccess.Read)))
                {
                    server_ver = (sr.ReadToEnd()).Split('.');
                }

                for (int i = 0; i < server_ver.Length; ++i)
                {
                    if (int.Parse(now_ver[i]) < int.Parse(server_ver[i])) HasUpdate = true;
                    else if (int.Parse(now_ver[i]) == int.Parse(server_ver[i])) continue;
                    else
                    {
                        HasUpdate = false;
                        break;
                    }
                }

                if (HasUpdate == true)
                {
                    UpdateButton.Visibility = ViewStates.Visible;
                    UpdateButton.Animate().Alpha(1.0f).SetDuration(500).Start();
                    ServerVersion.Text = "최신 버전 : " + server_ver[0] + "." + server_ver[1] + "." + server_ver[2];
                }
                else
                {
                    ServerVersion.Text = Resources.GetString(Resource.String.LatestUpdateVersion);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.UpdateCheck_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkMagenta);
                ServerVersion.Text = Resources.GetString(Resource.String.UnableCheckUpdate);
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG);
            ad.SetTitle(Resource.String.UpdateDialog_Title);
            ad.SetMessage(Resource.String.UpdateDialog_Message);
            ad.SetCancelable(true);
            ad.SetPositiveButton("Play Store", delegate { LinkAppStore(); });
            ad.SetNegativeButton("APK 설치", async delegate { await AppUpdate(); });

            ad.Show();
        }

        private void LinkAppStore()
        {
            string AppPackageName = PackageName;

            try
            {
                string url = "market://details?id=" + PackageName;
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
            }
            catch (Exception ex)
            {
                string url = "https://play.google.com/store/apps/details?id=" + PackageName;
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
            }
        }

        private async Task AppUpdate()
        {
            ProgressDialog pd = new ProgressDialog(this, ETC.DialogBG_Download);
            pd.SetTitle(Resource.String.UpdateDownloadDialog_Title);
            pd.SetMessage(Resources.GetString(Resource.String.UpdateDownloadDialog_Message));
            pd.SetProgressStyle(ProgressDialogStyle.Horizontal);
            pd.SetCancelable(false);

            DownloadProgress = pd;
            pd.Show();

            string url = Path.Combine(ETC.Server, "Update", "GFD.apk");
            string target = Path.Combine(ETC.tempPath, "Update.apk");

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;

                    await wc.DownloadFileTaskAsync(url, target);
                }

                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.UpdateDownload_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkOrange);
            }
            finally
            {
                pd.Dismiss();
                DownloadProgress = null;
            }

            Intent installAPK = new Intent(Intent.ActionView);
            installAPK.SetDataAndType(Android.Net.Uri.FromFile(new Java.IO.File(target)), "application/vnd.android.package-archive");
            StartActivity(installAPK);
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgress.Progress = e.ProgressPercentage;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }
    }
}