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

        FloatingActionButton KakaoPlusFriendFAB;
        FloatingActionButton DiscordFAB;


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

                DiscordFAB = FindViewById<FloatingActionButton>(Resource.Id.DiscordFAB);
                DiscordFAB.Click += HelpFAB_Click;
                DiscordFAB.LongClick += HelpFAB_LongClick;

                Initialize();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void HelpFAB_LongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                string tip = "";

                switch (fab.Id)
                {
                    case Resource.Id.DiscordFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_GFDInfo_Discord);
                        break;
                }

                Toast.MakeText(this, tip, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private void HelpFAB_Click(object sender, EventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                Intent intent = null;
                string url = "";

                switch (fab.Id)
                {
                    case Resource.Id.DiscordFAB:
                        url = "https://discord.gg/sWRw4MN";
                        break;
                }

                intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void Initialize()
        {
            try
            {
                InitDeveloperInfo();
                _ = CheckAppVersion();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private void InitDeveloperInfo()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0}\n", Resources.GetString(Resource.String.GFDInfo_Developer_Soomae));
                sb.Append("eveia@naver.com\n\n");
                sb.AppendFormat("{0}\n", Resources.GetString(Resource.String.GFDInfo_Developer_URK96));
                sb.Append("chlwlsgur96@hotmail.com\n\n");
                sb.AppendFormat("{0}\n", Resources.GetString(Resource.String.GFDInfo_Developer_Bibitjyadame));
                sb.Append("bibitjyadame@gmail.com");
                sb.Append("\n\n## Special Thanks ##\n\n");
                sb.Append("츠보우\n");
                sb.Append("잉여군\n");
                sb.Append("MADCORE\n");
                sb.Append("천솜향\n");
                sb.Append("우용곡\n");
                sb.Append("MMM\n");
                sb.Append("Geo");

                FindViewById<TextView>(Resource.Id.GFDInfoDeveloperInfo).Text = sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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

                NowVersion.Text = string.Format("{0} : {1} - ", Resources.GetString(Resource.String.GFDInfo_NowVersion), context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName);
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
                    ServerVersion.Text = string.Format("{0} : {1}.{2}.{3}", Resources.GetString(Resource.String.GFDInfo_NewVersion), server_ver[0], server_ver[1],  server_ver[2]);
                }
                else
                {
                    ServerVersion.Text = Resources.GetString(Resource.String.LatestUpdateVersion);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.UpdateCheck_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkMagenta);
                ServerVersion.Text = Resources.GetString(Resource.String.UnableCheckUpdate);
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            LinkAppStore();
        }

        private void LinkAppStore()
        {
            string AppPackageName = PackageName;

            try
            {
                string url = string.Format("market://details?id={0}", PackageName);
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
            }
            catch (Exception)
            {
                string url = string.Format("https://play.google.com/store/apps/details?id={0}", PackageName);
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
            }
        }

        /*private async Task AppUpdate()
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
                ETC.LogError(ex, this);
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
        }*/

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }
    }
}