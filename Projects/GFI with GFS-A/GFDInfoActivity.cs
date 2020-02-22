using Android.App;
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
    [Activity(Label = "GFDInfoActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GFDInfoActivity : BaseAppCompatActivity
    {
        Button updateButton;
        TextView nowVersion;
        TextView serverVersion;
        CoordinatorLayout snackbarLayout;

        FloatingActionButton discordFAB;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_NoActionBar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.GFDInfoLayout);

                SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.GFDInfoMainToolbar));
                SupportActionBar.SetTitle(Resource.String.GFDInfo_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                updateButton = FindViewById<Button>(Resource.Id.GFDInfo_AppUpdateButton);
                updateButton.Click += UpdateButton_Click;
                nowVersion = FindViewById<TextView>(Resource.Id.GFDInfo_NowAppVersion);
                serverVersion = FindViewById<TextView>(Resource.Id.GFDInfo_ServerAppVersion);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.GFDInfoSnackbarLayout);

                discordFAB = FindViewById<FloatingActionButton>(Resource.Id.DiscordFAB);
                discordFAB.Click += HelpFAB_Click;
                discordFAB.LongClick += HelpFAB_LongClick;

                Initialize();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void HelpFAB_LongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                string tip = "";

                switch ((sender as FloatingActionButton).Id)
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
                string url = "";

                switch ((sender as FloatingActionButton).Id)
                {
                    case Resource.Id.DiscordFAB:
                        url = "https://discord.gg/sWRw4MN";
                        break;
                }

                var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.SideLinkOpen_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
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
                sb.Append("츠보우, ");
                sb.Append("잉여군, ");
                sb.Append("MADCORE\n");
                sb.Append("천솜향, ");
                sb.Append("우용곡, ");
                sb.Append("MMM, ");
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
            await Task.Delay(100);

            bool hasUpdate = false;

            try
            {
                var context = ApplicationContext;
                string[] nowVer = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName.Split('.');
                string[] serverVer = new string[nowVer.Length];

                nowVersion.Text = string.Format("{0} : {1} - ", Resources.GetString(Resource.String.GFDInfo_NowVersion), context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName);
#if DEBUG
                nowVersion.Text += "Debug";
#else
                nowVersion.Text += "Release";
#endif
                string url = Path.Combine(ETC.server, "GFD_AppVer.txt");
                string target = Path.Combine(ETC.tempPath, "AppVer.txt");

                using (WebClient wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(url, target);
                }

                using (StreamReader sr = new StreamReader(new FileStream(target, FileMode.Open, FileAccess.Read)))
                {
                    serverVer = (sr.ReadToEnd()).Split('.');
                }

                for (int i = 0; i < serverVer.Length; ++i)
                {
                    if (int.Parse(nowVer[i]) < int.Parse(serverVer[i]))
                    {
                        hasUpdate = true;
                    }
                    else if (int.Parse(nowVer[i]) == int.Parse(serverVer[i]))
                    {
                        continue;
                    }
                    else
                    {
                        hasUpdate = false;
                        break;
                    }
                }

                if (hasUpdate)
                {
                    updateButton.Visibility = ViewStates.Visible;
                    updateButton.Animate().Alpha(1.0f).SetDuration(500).Start();
                    serverVersion.Text = string.Format("{0} : {1}.{2}.{3}", Resources.GetString(Resource.String.GFDInfo_NewVersion), serverVer[0], serverVer[1],  serverVer[2]);
                }
                else
                {
                    serverVersion.Text = Resources.GetString(Resource.String.LatestUpdateVersion);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.UpdateCheck_Fail, Snackbar.LengthShort, Android.Graphics.Color.DarkMagenta);

                serverVersion.Text = Resources.GetString(Resource.String.UnableCheckUpdate);
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