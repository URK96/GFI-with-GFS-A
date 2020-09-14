using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;

using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFDA
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

                SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.GFDInfoMainToolbar));
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
                string tip = (sender as FloatingActionButton).Id switch
                {
                    Resource.Id.DiscordFAB => Resources.GetString(Resource.String.Tooltip_GFDInfo_Discord),
                    _ => "",
                };

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
                string url = (sender as FloatingActionButton).Id switch
                {
                    Resource.Id.DiscordFAB => "https://discord.gg/sWRw4MN",
                    _ => "",
                };

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
                var sb = new StringBuilder();

                sb.AppendLine(Resources.GetString(Resource.String.GFDInfo_Developer_Soomae));
                sb.Append("eveia@naver.com\n\n");
                sb.AppendLine(Resources.GetString(Resource.String.GFDInfo_Developer_URK96));
                sb.Append("chlwlsgur96@hotmail.com\n\n");
                sb.AppendLine(Resources.GetString(Resource.String.GFDInfo_Developer_Bibitjyadame));
                sb.Append("bibitjyadame@gmail.com");
                sb.Append("\n\n## Special Thanks ##\n\n");
                sb.AppendLine("츠보우");
                sb.AppendLine("잉여군");
                sb.AppendLine("MADCORE");
                sb.AppendLine("천솜향");
                sb.AppendLine("우용곡");
                sb.AppendLine("MMM");
                sb.AppendLine("Geo");
                sb.AppendLine("센롱");

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

                using (var wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(url, target);
                }

                using (var sr = new StreamReader(new FileStream(target, FileMode.Open, FileAccess.Read)))
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
                    serverVersion.Text = $"{Resources.GetString(Resource.String.GFDInfo_NewVersion)} : {serverVer[0]}.{serverVer[1]}.{serverVer[2]}";
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

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
            GC.Collect();
        }
    }
}