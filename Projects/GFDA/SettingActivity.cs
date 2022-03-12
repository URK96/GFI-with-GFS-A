using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Preference;

using Google.Android.Material.Snackbar;

using JaredRummler.Android.ColorPicker;

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFDA
{
    [Activity(Label = "@string/Activity_SettingActivity", Theme = "@style/GFS.Setting", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SettingActivity : BaseAppCompatActivity
    {
        private CoordinatorLayout SnackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Setting_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.SettingMainLayout);

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.SettingSnackbarLayout);

                SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.SettingMainToolbar));
                SupportActionBar.SetTitle(Resource.String.SettingActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.SettingFragmentContainer, new MainSettingFragment(), null).Commit();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
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

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
        }
    }

    public partial class MainSettingFragment : PreferenceFragmentCompat
    {
        private CoordinatorLayout snackbarLayout;

        private Dialog dialog;
        private ProgressDialog p_dialog;
        private ProgressBar totalProgressBar;
        private ProgressBar nowProgressBar;
        private TextView totalProgress;
        private TextView nowProgress;

        private int total = 0;
        private int now = 0;

        private int count = 0;

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            snackbarLayout = Activity.FindViewById<CoordinatorLayout>(Resource.Id.SettingSnackbarLayout);
            snackbarLayout.BringToFront();

            AddPreferencesFromResource(Resource.Xml.MainSetting);

            InitPreferences();
        }

        private void InitPreferences()
        {
            var updateLog = FindPreference("UpdateLog");
            updateLog.SetIcon(ETC.useLightTheme ? Resource.Drawable.updatehistory_icon_wt : Resource.Drawable.updatehistory_icon);
            updateLog.PreferenceClick += delegate
            {
                var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBGVertical);
                ad.SetTitle(Resource.String.NewFeatureDialog_Title);
                ad.SetCancelable(true);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                try
                {
                    string assetName = ETC.locale.Language switch
                    {
                        "ko" => "UpdateFeature_ko.txt",
                        _ => "UpdateFeature_en.txt",
                    };

                    using (var sr = new StreamReader(Activity.Assets.Open(assetName)))
                    {
                        ad.SetMessage(sr.ReadToEnd());
                    }

                    ad.Show();
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, Activity);
                    ETC.ShowSnackbar(snackbarLayout, Resource.String.Main_NotificationInitializeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
                }
            };

            var startAppMode = FindPreference("StartAppMode") as ListPreference;
            startAppMode.SetIcon(ETC.useLightTheme ? Resource.Drawable.appmode_icon_wt : Resource.Drawable.appmode_icon);
            startAppMode.SetEntries(new string[] 
            {
                Resources.GetString(Resource.String.Common_Default),
                Resources.GetString(Resource.String.Main_Category_GFDv1),
                Resources.GetString(Resource.String.Main_GFUtil_AreaTip),
                Resources.GetString(Resource.String.Main_GFUtil_Calc),
                Resources.GetString(Resource.String.Main_GFUtil_Event),
                Resources.GetString(Resource.String.Main_GFUtil_OfficialNotification),
                Resources.GetString(Resource.String.Main_Other_GFOSTPlayer)
            });
            startAppMode.SetEntryValues(new string[] { "0", "1", "2", "3", "4", "5", "6" });
            startAppMode.SetValueIndex(int.Parse(Preferences.Get("StartAppMode", "0")));

            var useLightTheme = FindPreference("UseLightTheme") as SwitchPreference;
            useLightTheme.SetIcon(ETC.useLightTheme ? Resource.Drawable.themecolor_icon_wt : Resource.Drawable.themecolor_icon);
            useLightTheme.Checked = Preferences.Get("UseLightTheme", false);
            useLightTheme.PreferenceChange += delegate { Preferences.Set("UseLightTheme", ETC.useLightTheme = useLightTheme.Checked); };

            var enableServerCheck = FindPreference("EnableServerCheck") as SwitchPreference;
            enableServerCheck.SetIcon(ETC.useLightTheme ? Resource.Drawable.server_icon_wt : Resource.Drawable.server_icon);
            enableServerCheck.Checked = Preferences.Get("EnableServerCheck", true);
            enableServerCheck.PreferenceChange += delegate { Preferences.Set("EnableServerCheck", enableServerCheck.Checked); };

            var startMainFragment = FindPreference("StartMainFragment") as ListPreference;
            startMainFragment.SetIcon(ETC.useLightTheme ? Resource.Drawable.startscreen_icon_wt : Resource.Drawable.startscreen_icon);
            startMainFragment.SetEntries(new string[]
            {
                Resources.GetString(Resource.String.Main_Category_Home),
                Resources.GetString(Resource.String.Main_Category_DB),
                Resources.GetString(Resource.String.Main_Category_GFDv1),
                Resources.GetString(Resource.String.Main_Category_GFUtil),
                Resources.GetString(Resource.String.Main_Category_Other),
            });
            startMainFragment.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
            startMainFragment.SetValueIndex(int.Parse(Preferences.Get("StartMainFragment", "0")));

            var appLanguage = FindPreference("AppLanguage") as ListPreference;
            appLanguage.SetIcon(ETC.useLightTheme ? Resource.Drawable.language_icon_wt : Resource.Drawable.language_icon);
            appLanguage.SetEntries(new string[]
            {
                "System Default",
                Resources.GetString(Resource.String.Common_Language_KR),
                Resources.GetString(Resource.String.Common_Language_EN)
            });
            appLanguage.SetEntryValues(new string[] { "0", "1", "2" });
            appLanguage.SetValueIndex(int.Parse(Preferences.Get("AppLanguage", "0")));


            var autoDBUpdate = FindPreference("AutoDBUpdate") as SwitchPreference;
            autoDBUpdate.SetIcon(ETC.useLightTheme ? Resource.Drawable.databasesync_icon_wt : Resource.Drawable.databasesync_icon);
            autoDBUpdate.Checked = Preferences.Get("AutoDBUpdate", true);
            autoDBUpdate.PreferenceChange += delegate { Preferences.Set("AutoDBUpdate", autoDBUpdate.Checked); };

            var checkDBUpdate = FindPreference("CheckDBUpdate");
            checkDBUpdate.SetIcon(ETC.useLightTheme ? Resource.Drawable.databaseupdate_icon_wt : Resource.Drawable.databaseupdate_icon);
            checkDBUpdate.PreferenceClick += CheckDBUpdate_PreferenceClick;

            var repairDB = FindPreference("RepairDB");
            repairDB.SetIcon(ETC.useLightTheme ? Resource.Drawable.repair_icon_wt : Resource.Drawable.repair_icon);
            repairDB.PreferenceClick += RepairDB_PreferenceClick;

            var dbListImage = FindPreference("DBListImageShow") as SwitchPreference;
            dbListImage.SetIcon(ETC.useLightTheme ? Resource.Drawable.listmode_icon_wt : Resource.Drawable.listmode_icon);
            dbListImage.Checked = Preferences.Get("DBListImageShow", false);
            dbListImage.PreferenceChange += delegate { Preferences.Set("DBListImageShow", dbListImage.Checked); };

            /*SwitchPreference PreviewDBListLayout = (SwitchPreference)FindPreference("PreviewDBListLayout");
            if (ETC.useLightTheme)
            {
                PreviewDBListLayout.SetIcon(Resource.Drawable.BetaIcon_WhiteTheme);
            }
            else
            {
                PreviewDBListLayout.SetIcon(Resource.Drawable.BetaIcon);
            }
            PreviewDBListLayout.Checked = ETC.sharedPreferences.GetBoolean("PreviewDBListLayout", true);
            PreviewDBListLayout.PreferenceChange += delegate
            {
                SaveSetting.PutBoolean("PreviewDBListLayout", PreviewDBListLayout.Checked);
                SaveSetting.Apply();
            };*/

            var dbDetailBackgroundImage = FindPreference("DBDetailBackgroundImage") as SwitchPreference;
            dbDetailBackgroundImage.SetIcon(ETC.useLightTheme ? Resource.Drawable.backgroundimage_icon_wt : Resource.Drawable.backgroundimage_icon);
            dbDetailBackgroundImage.Checked = Preferences.Get("DBDetailBackgroundImage", false);
            dbDetailBackgroundImage.PreferenceChange += delegate { Preferences.Set("DBDetailBackgroundImage", dbDetailBackgroundImage.Checked); };

            var textViewerTextSize = FindPreference("TextViewerTextSize");
            textViewerTextSize.SetIcon(ETC.useLightTheme ? Resource.Drawable.fontsize_icon_wt : Resource.Drawable.fontsize_icon);
            textViewerTextSize.PreferenceClick += TextViewerTextSize_PreferenceClick;


            var textViewerTextColor = FindPreference("TextViewerTextColor");
            textViewerTextColor.SetIcon(ETC.useLightTheme ? Resource.Drawable.fontcolor_icon_wt : Resource.Drawable.fontcolor_icon);
            textViewerTextColor.PreferenceChange += (sender, e) =>
            {
                Preferences.Set("TextViewerTextColorHex", $"#{(int)e.NewValue:X}");
            };

            var textViewerBackgroundColor = FindPreference("TextViewerBackgroundColor");
            textViewerBackgroundColor.SetIcon(ETC.useLightTheme ? Resource.Drawable.backgroundcolor_icon_wt : Resource.Drawable.backgroundcolor_icon);
            textViewerBackgroundColor.PreferenceChange += (sender, e) => 
            { 
                Preferences.Set("TextViewerBackgroundColorHex", $"#{(int)e.NewValue:X}"); 
            };



            var DownloadAllCache = FindPreference("DownloadAllCache");
            DownloadAllCache.SetIcon(ETC.useLightTheme ? Resource.Drawable.download_icon_wt : Resource.Drawable.download_icon);
            DownloadAllCache.PreferenceClick += DownloadAllCache_PreferenceClick;

            var cleanCache = FindPreference("CleanCache");
            cleanCache.SetIcon(ETC.useLightTheme ? Resource.Drawable.delete_icon_wt : Resource.Drawable.delete_icon);
            cleanCache.PreferenceClick += CleanCache_PreferenceClick;

            //Preference OpenLogFolder = FindPreference("OpenLogFolder");

            var deleteAllLogFile = FindPreference("DeleteAllLogFile");
            deleteAllLogFile.SetIcon(ETC.useLightTheme ? Resource.Drawable.deletelog_icon_wt : Resource.Drawable.deletelog_icon);
            deleteAllLogFile.PreferenceClick += DeleteAllLogFile_PreferenceClick;

            var externalLibraryLicense = FindPreference("ExternalLibraryLicense");
            externalLibraryLicense.SetIcon(ETC.useLightTheme ? Resource.Drawable.license_icon_wt : Resource.Drawable.license_icon);
            externalLibraryLicense.PreferenceClick += delegate { Activity.StartActivity(typeof(ExternLibraryCopyright)); };

            var gfdSourceCode = FindPreference("GFDSourceCode");
            gfdSourceCode.SetIcon(ETC.useLightTheme ? Resource.Drawable.sourcecode_icon_wt : Resource.Drawable.sourcecode_icon);
            gfdSourceCode.PreferenceClick += async delegate
            {
                Toast.MakeText(Activity, Resource.String.Switch_ExternalBrowser, ToastLength.Short).Show();

                await Browser.OpenAsync(new Uri("https://github.com/URK96/GFI-with-GFS-A.git"), BrowserLaunchMode.External).ConfigureAwait(false);
            };
        }

        private async void CheckDBUpdate_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Dialog dialog = null;

            try
            {
                var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBG);
                ad.SetTitle(Resource.String.CheckDBUpdateDialog_Title);
                ad.SetMessage(Resource.String.CheckDBUpdateDialog_Message);
                ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);
                ad.SetCancelable(false);

                dialog = ad.Show();

                await ETC.CheckServerNetwork();
                await Task.Delay(100);

                if (ETC.isServerDown)
                {
                    Toast.MakeText(Activity, Resource.String.Common_ServerMaintenance, ToastLength.Short).Show();
                }
                else if (await ETC.CheckDBVersion())
                {
                    await ETC.UpdateDB(Activity);

                    await Task.Delay(500);

                    await ETC.LoadDB();

                    Toast.MakeText(Activity, Resource.String.CheckDBUpdate_Complete, ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(Activity, Resource.String.CheckDBUpdate_UpToDate, ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.UnableCheckUpdate, ToastLength.Short).Show();
            }
            finally
            {
                dialog?.Dismiss();
            }
        }

        private async void RepairDB_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                await ETC.CheckServerNetwork();

                await Task.Delay(100);

                if (ETC.isServerDown)
                {
                    Toast.MakeText(Activity, Resource.String.Common_ServerMaintenance, ToastLength.Short).Show();
                }
                else
                {
                    await ETC.UpdateDB(Activity, false, Resource.String.RepairDBDialog_Title, Resource.String.RepairDBDialog_Message);

                    await Task.Delay(500);

                    await ETC.LoadDB();

                    Toast.MakeText(Activity, Resource.String.RepairDB_Complete, ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.RepairDB_Fail, ToastLength.Short).Show();
            }
        }

        private void DeleteAllLogFile_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                count += 1;

                if (count == 5)
                {
                    var p = sender as Preference;

                    p.SetTitle(Resource.String.Firewall);
                    p.SetSummary(Resource.String.Firewall_Message);
                    p.SetIcon(ETC.useLightTheme ? Resource.Drawable.Hack_WhiteTheme : Resource.Drawable.Hack);
                }
                else if (count > 5)
                {
                    var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBG);
                    ad.SetTitle("¿Quiere neutralizar el cortafuegos?");
                    ad.SetMessage("SANGVIS FERRI, Annulation - Chaos, OK - Inscrire cachée");
                    ad.SetCancelable(true);
                    ad.SetNegativeButton("䍡湣敬", delegate { PreferenceScreen.RemoveAll(); });
                    ad.SetPositiveButton("佋", delegate
                    {
                        Activity.StartActivity(typeof(ZinaOS));
                        Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                    });

                    ad.Show();
                }
                else
                {
                    var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBG);
                    ad.SetTitle(Resource.String.SettingActivity_DeleteLogFile_DialogTitle);
                    ad.SetMessage(Resource.String.SettingActivity_DeleteLogFile_DialogCheckMessage);
                    ad.SetCancelable(true);
                    ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                    ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { CleanLogFolderProcess(); });

                    ad.Show();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.SettingActivity_DeleteLogFile_Fail, ToastLength.Short).Show();
            }
        }

        private async void CleanLogFolderProcess()
        {
            Dialog dialog = null;

            try
            {
                var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBGDownload);
                ad.SetTitle(Resource.String.SettingActivity_DeleteLogFile_DialogTitle);
                ad.SetMessage(Resource.String.SettingActivity_DeleteLogFile_DialogMessage);
                ad.SetCancelable(false);
                ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);

                dialog = ad.Show();

                await Task.Delay(1000);

                await Task.Run(() =>
                {
                    if (Directory.Exists(ETC.logPath))
                    {
                        Directory.Delete(ETC.logPath, true);
                    }

                    Directory.CreateDirectory(ETC.logPath);
                    ETC.CheckInitFolder();
                });

                Toast.MakeText(Activity, Resource.String.SettingActivity_DeleteLogFile_Complete, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.SettingActivity_DeleteLogFile_Fail, ToastLength.Short).Show();
            }
            finally
            {
                dialog?.Dismiss();
            }
        }

        private void TextViewerTextSize_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            var view = Activity.LayoutInflater.Inflate(Resource.Layout.NumberPickerDialogLayout, null);

            var np = view.FindViewById<NumberPicker>(Resource.Id.NumberPickerControl);
            np.MaxValue = 30;
            np.MinValue = 4;
            np.Value = Preferences.Get("TextViewerTextSize", 12);

            var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBG);
            ad.SetTitle(Resource.String.Common_TextSize);
            ad.SetCancelable(true);
            ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            ad.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate { Preferences.Set("TextViewerTextSize", 12); });
            ad.SetPositiveButton(Resource.String.AlertDialog_Set, delegate { Preferences.Set("TextViewerTextSize", np.Value); });
            ad.SetView(view);

            ad.Show();
        }

        private void DownloadAllCache_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            var alert = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBG);
            alert.SetTitle(Resource.String.SettingActivity_DownloadAllCache_DialogTitle);
            alert.SetMessage(Resource.String.SettingActivity_DownloadAllCache_DialogCheckMessage);
            alert.SetCancelable(true);
            alert.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            alert.SetPositiveButton(Resource.String.AlertDialog_Download, delegate
            {
                _ = ETC.CheckServerNetwork();

                if (ETC.isServerDown)
                {
                    Toast.MakeText(Activity, Resource.String.Common_ServerMaintenance, ToastLength.Short).Show();

                    return;
                }

                if (!CheckStorageCapacity())
                {
                    return;
                }

                string[] OSVer = Build.VERSION.Release.Split('.');

                SelectCache();
            });

            alert.Show();
        }

        private bool CheckStorageCapacity()
        {
            //var pd = new ProgressDialog(Activity, ETC.dialogBG);
            //pd.SetTitle(Resource.String.SettingActivity_CheckFreeStorage_DialogTitle);
            //pd.SetMessage(Resources.GetString(Resource.String.SettingActivity_CheckFreeStorage_DialogMessage));
            //pd.SetCancelable(false);

            //pd.Show();

            var stat = new StatFs(ETC.appDataPath);

            try
            {
                long blockSize = stat.BlockSizeLong;
                long availableBlocks = stat.AvailableBlocksLong;
                long availableSize = availableBlocks * blockSize;

                freeSize = Convert.ToInt32(availableSize / 1024 / 1024);

                if (freeSize >= 600)
                {
                    return true;
                }
                else
                {
                    var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
                    ad.SetTitle(Resource.String.SettingActivity_CheckFreeStorage_FailDialogTitle);
                    ad.SetMessage($"{Resources.GetString(Resource.String.SettingActivity_CheckFreeStorage_FailDialogMessage)} {freeSize}MB");
                    ad.SetNeutralButton(Resource.String.AlertDialog_Confirm, delegate { });
                    ad.SetCancelable(true);

                    ad.Show();

                    return false;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.SettingActivity_CheckFreeStorage_CheckFail, Snackbar.LengthLong);
            }
            finally
            {
                stat.Dispose();
            }

            return false;
        }

        /*private async Task DownloadAllCacheProcess()
        {
            View v = Activity.LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG);
            Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG_Download);
            pd.SetTitle(Resource.String.SettingActivity_DownloadAllCache_DialogTitle);
            pd.SetMessage(Resource.String.SettingActivity_DownloadAllCache_DialogMessage);
            pd.SetCancelable(false);
            pd.SetView(v);

            dialog = pd.Create();
            dialog.Show();

            try
            {
                TextView file_status = dialog.FindViewById<TextView>(Resource.Id.ProgressNowFile);
                TextView status = dialog.FindViewById<TextView>(Resource.Id.ProgressStatusMessage);

                totalProgressBar = dialog.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
                totalProgress = dialog.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
                nowProgressBar = dialog.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
                nowProgress = dialog.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;
                nowProgressBar.Max = 100;
                nowProgressBar.Progress = 0;

                int totalCount = 0;

                status.Text = Resources.GetString(Resource.String.SettingActivity_DownloadAllCache_StatusDollDB);

                foreach (DataRow dr in ETC.DollList.Rows)
                {
                    // 코스튬 별 일러스트 2장
                    if (dr["Costume"] != DBNull.Value)
                    {
                        if (System.String.IsNullOrEmpty((string)dr["Costume"]) == false)
                        {
                            string[] costume_list = ((string)dr["Costume"]).Split(';');

                            totalCount += costume_list.Length * 2;
                        }
                    }

                    // Mod 일러스트 2장 + Crop 이미지 1장
                    if ((bool)dr["HasMod"] == true) totalCount += 3;

                    // 기본 일러스트 2장 + Crop 이미지 1장 + 스킬 아이콘 1개 + SD 이미지 1개;
                    totalCount += 5;
                }

                total = totalCount;
                now = 0;

                status.Text = string.Format("{0}...(1/4)", Resources.GetString(Resource.String.SettingActivity_DownloadAllCache_StatusDollDB2));

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    foreach (DataRow dr in ETC.DollList.Rows)
                    {
                        int count = 1;

                        if (dr["Costume"] != DBNull.Value)
                        {
                            if (string.IsNullOrEmpty((string)dr["Costume"]) == false)
                            {
                                string[] costume_list = ((string)dr["Costume"]).Split(';');
                                count += costume_list.Length;
                            }
                        }

                        for (int i = 0; i < count; ++i)
                        {
                            for (int j = 0; j < 2; ++j)
                            {
                                string filename = ((int)dr["DicNumber"]).ToString();

                                if ((i == 0) && (j == 0))
                                {
                                    string filename_sd = filename + "_SD";
                                    string cachepath_sd = Path.Combine(ETC.CachePath, "Doll", "SD", filename_sd + ".gfdcache");
                                    string url_sd = Path.Combine(ETC.Server, "Data", "Images", "Guns", "SD", filename + ".png");

                                    file_status.Text = filename_sd;

                                    await wc.DownloadFileTaskAsync(url_sd, cachepath_sd);
                                }

                                if (i >= 1)
                                {
                                    filename += "_" + (i + 1);
                                }

                                if (j == 1) filename += "_D";

                                string url = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", filename + ".png");
                                string cachepath = Path.Combine(ETC.CachePath, "Doll", "Normal", filename + ".gfdcache");
                                file_status.Text = filename;
                                await wc.DownloadFileTaskAsync(url, cachepath);
                            }
                        }

                        string filename2 = (string)dr["Skill"];
                        string cachepath2 = Path.Combine(ETC.CachePath, "Doll", "Skill", filename2 + ".gfdcache");
                        string url2 = Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", filename2 + ".png");
                        file_status.Text = filename2;
                        await wc.DownloadFileTaskAsync(url2, cachepath2);

                        string filename3 = ((int)dr["DicNumber"]).ToString();
                        string cachepath3 = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", filename3 + ".gfdcache");
                        string url3 = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", filename3 + ".png");
                        file_status.Text = filename3;
                        await wc.DownloadFileTaskAsync(url3, cachepath3);

                        string filename4 = ((int)dr["DicNumber"]).ToString() + "_M";
                        string filename5 = filename4 + "_D";
                        string cachepath4 = Path.Combine(ETC.CachePath, "Doll", "Normal", filename4 + ".gfdcache");
                        string cachepath4_1 = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", filename4 + ".gfdcache");
                        string cachepath5 = Path.Combine(ETC.CachePath, "Doll", "Normal", filename5 + ".gfdcache");
                        string url4 = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", filename4 + ".png");
                        string url4_1 = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", filename4 + ".png");
                        string url5 = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", filename5 + ".png");
                        file_status.Text = filename4;
                        await wc.DownloadFileTaskAsync(url4, cachepath4);
                        await wc.DownloadFileTaskAsync(url4_1, cachepath4_1);
                        file_status.Text = filename5;
                        await wc.DownloadFileTaskAsync(url5, cachepath5);
                    }
                }

                await Task.Delay(500);

                totalProgressBar.Progress = 0;
                nowProgressBar.Progress = 0;
                totalProgress.Text = "0%";
                nowProgress.Text = "0%";

                status.Text = Resources.GetString(Resource.String.SettingActivity_DownloadAllCache_StatusFairyDB);

                await Task.Delay(500);

                // Normal 3장 + Crop 1장 + 스킬 1장
                totalCount = ETC.FairyList.Rows.Count * 5;
                total = totalCount;
                now = 0;

                status.Text = string.Format("{0}(2/4)", Resources.GetString(Resource.String.SettingActivity_DownloadAllCache_StatusFairyDB2));

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    foreach (DataRow dr in ETC.FairyList.Rows)
                    {
                        for (int i = 0; i < 3; ++i)
                        {
                            string filename = (string)dr["Name"] + "_" + (i + 1);
                            string cachepath = Path.Combine(ETC.CachePath, "Fairy", "Normal", filename + ".gfdcache");

                            string url = Path.Combine(ETC.Server, "Data", "Images", "Fairy", filename + ".png");

                            file_status.Text = filename;

                            await wc.DownloadFileTaskAsync(url, cachepath);
                        }

                        string filename2 = (string)dr["SkillName"];
                        string cachepath2 = Path.Combine(ETC.CachePath, "Fairy", "Skill", filename2 + ".gfdcache");
                        string url2 = Path.Combine(ETC.Server, "Data", "Images", "FairySkill", filename2 + ".png");
                        file_status.Text = filename2;
                        await wc.DownloadFileTaskAsync(url2, cachepath2);

                        string filename3 = (string)dr["Name"];
                        string cachepath3 = Path.Combine(ETC.CachePath, "Fairy", "Normal_Crop", filename3 + ".gfdcache");
                        string url3 = Path.Combine(ETC.Server, "Data", "Images", "Fairy", "Normal_Crop", filename3 + ".png");
                        file_status.Text = filename3;
                        await wc.DownloadFileTaskAsync(url3, cachepath3);
                    }
                }

                await Task.Delay(500);

                totalProgressBar.Progress = 0;
                nowProgressBar.Progress = 0;
                totalProgress.Text = "0%";
                nowProgress.Text = "0%";

                status.Text = Resources.GetString(Resource.String.SettingActivity_DownloadAllCache_StatusEquipDB);

                await Task.Delay(500);

                List<string> icons = new List<string>();

                for (int i = 0; i < ETC.EquipmentList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.EquipmentList.Rows[i];

                    bool IsSkip = false;
                    if (icons.Count == 0)
                    {
                        icons.Add((string)dr["Icon"]);
                        continue;
                    }

                    foreach (string s in icons)
                    {
                        if ((string)dr["Icon"] == s)
                        {
                            IsSkip = true;
                            break;
                        }
                    }

                    if (IsSkip == true) continue;
                    icons.Add((string)dr["Icon"]);
                }

                icons.TrimExcess();

                totalCount = icons.Count;
                total = totalCount;
                now = 0;

                status.Text = string.Format("{0}(3/4)", Resources.GetString(Resource.String.SettingActivity_DownloadAllCache_StatusEquipDB2));

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    foreach (string s in icons)
                    {
                        string filename = s;

                        string cachepath = Path.Combine(ETC.CachePath, "Equip", "Normal", filename + ".gfdcache");
                        string url = Path.Combine(ETC.Server, "Data", "Images", "Equipments", filename + ".png");
                        file_status.Text = filename;
                        await wc.DownloadFileTaskAsync(url, cachepath);
                    }
                }

                await Task.Delay(500);

                totalProgressBar.Progress = 0;
                nowProgressBar.Progress = 0;
                totalProgress.Text = "0%";
                nowProgress.Text = "0%";

                status.Text = Resources.GetString(Resource.String.SettingActivity_DownloadAllCache_StatusOldGFD);

                await Task.Delay(500);

                string[] ImageName = { "ProductTable_Doll", "ProductTable_Equipment", "ProductTable_Fairy", "RecommendDollRecipe", "RecommendEquipmentRecipe", "RecommendMD", "RecommendLeveling" };

                totalCount += ImageName.Length;
                total = totalCount;
                now = 0;

                status.Text = string.Format("{0}(4/4)", Resources.GetString(Resource.String.SettingActivity_DownloadAllCache_StatusOldGFD2));

                using (WebClient wc = new WebClient())
                {
                    foreach (string s in ImageName)
                    {
                        string url = Path.Combine(ETC.Server, "Data", "Images", "OldGFD", "Images", s + ".png");
                        string target = Path.Combine(ETC.CachePath, "OldGFD", "Images", s + ".gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }

                    wc.DownloadFile(Path.Combine(ETC.Server, "OldGFDVer.txt"), Path.Combine(ETC.SystemPath, "OldGFDVer.txt"));
                }

                await Task.Delay(500);

                ad.SetTitle(Resource.String.SettingActivity_DownloadAllCache_CompleteDialogTitle);
                ad.SetMessage(Resource.String.SettingActivity_DownloadAllCache_CompleteDialogMessage);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                ad.Show();
            }
            catch (Java.Lang.Exception ex)
            {
                ETC.LogError(ex, Activity);

                ad.SetTitle(Resource.String.SettingActivity_DownloadAllCache_FailDialogTitle);
                ad.SetMessage(Resource.String.SettingActivity_DownloadAllCache_FailDialogMessage);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);

                ad.SetTitle(Resource.String.SettingActivity_DownloadAllCache_FailDialogTitle);
                ad.SetMessage(Resource.String.SettingActivity_DownloadAllCache_FailDialogMessage);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                ad.Show();
            }
            finally
            {
                dialog.Dismiss();
                dialog = null;
            }
        }

        private async Task DownloadAllCacheProcess_OldVer()
        {
            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG);
            ProgressDialog pd = new ProgressDialog(Activity, ETC.DialogBG_Download);
            pd.SetTitle(Resource.String.SettingActivity_DownloadAllCache_DialogTitle);
            pd.SetMessage(Resources.GetString(Resource.String.SettingActivity_DownloadAllCache_DialogMessage));
            pd.SetProgressStyle(ProgressDialogStyle.Horizontal);
            pd.SetCancelable(false);
            pd.Max = 100;
            pd.Progress = 0;

            p_dialog = pd;
            p_dialog.Show();

            try
            {
                int totalCount = 0;

                foreach (DataRow dr in ETC.DollList.Rows)
                {
                    // 코스튬 별 일러스트 2장
                    if (dr["Costume"] != DBNull.Value)
                    {
                        if (string.IsNullOrEmpty((string)dr["Costume"]) == false)
                        {
                            string[] costume_list = ((string)dr["Costume"]).Split(';');

                            totalCount += costume_list.Length * 2;
                        }
                    }

                    // Mod 일러스트 2장 + Crop 이미지 1장
                    if ((bool)dr["HasMod"] == true) totalCount += 3;

                    // 기본 일러스트 2장 + Crop 이미지 1장 + 스킬 아이콘 1개 + SD 이미지 1개;
                    totalCount += 5;
                }

                // 요정별 일러스트 3장 + 스킬 아이콘 1개 + Crop 1장
                totalCount += ETC.FairyList.Rows.Count * 5;

                List<string> icons = new List<string>();

                for (int i = 0; i < ETC.EquipmentList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.EquipmentList.Rows[i];

                    bool IsSkip = false;
                    if (icons.Count == 0)
                    {
                        icons.Add((string)dr["Icon"]);
                        continue;
                    }

                    foreach (string s in icons)
                    {
                        if ((string)dr["Icon"] == s)
                        {
                            IsSkip = true;
                            break;
                        }
                    }

                    if (IsSkip == true) continue;
                    icons.Add((string)dr["Icon"]);
                }

                icons.TrimExcess();

                totalCount += icons.Count;

                // 소전사전v1 이미지
                string[] ImageName = { "ProductTable_Doll", "ProductTable_Equipment", "ProductTable_Fairy", "RecommendDollRecipe", "RecommendEquipmentRecipe", "RecommendMD", "RecommendLeveling" };

                totalCount += ImageName.Length;

                total = totalCount;
                now = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged_OldVer;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted_OldVer;

                    foreach (DataRow dr in ETC.DollList.Rows)
                    {
                        int count = 1;

                        if (dr["Costume"] != DBNull.Value)
                        {
                            if (string.IsNullOrEmpty((string)dr["Costume"]) == false)
                            {
                                string[] costume_list = ((string)dr["Costume"]).Split(';');
                                count += costume_list.Length;
                            }
                        }

                        for (int i = 0; i < count; ++i)
                        {
                            for (int j = 0; j < 2; ++j)
                            {
                                string filename = ((int)dr["DicNumber"]).ToString();

                                if (i >= 1)
                                {
                                    filename += "_" + (i + 1);
                                }

                                if (j == 1) filename += "_D";

                                string cachepath = Path.Combine(ETC.CachePath, "Doll", "Normal", filename + ".gfdcache");
                                string url = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", filename + ".png");
                                await wc.DownloadFileTaskAsync(url, cachepath);
                            }
                        }

                        string filename2 = (string)dr["Skill"];
                        string cachepath2 = Path.Combine(ETC.CachePath, "Doll", "Skill", filename2 + ".gfdcache");
                        string url2 = Path.Combine(ETC.Server, "Data", "Images", "SkillIcons", filename2 + ".png");
                        await wc.DownloadFileTaskAsync(url2, cachepath2);

                        string filename3 = ((int)dr["DicNumber"]).ToString();
                        string cachepath3 = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", filename3 + ".gfdcache");
                        string url3 = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", filename3 + ".png");
                        await wc.DownloadFileTaskAsync(url3, cachepath3);

                        string filename4 = ((int)dr["DicNumber"]).ToString() + "_M";
                        string filename5 = filename4 + "_D";
                        string cachepath4 = Path.Combine(ETC.CachePath, "Doll", "Normal", filename4 + ".gfdcache");
                        string cachepath4_1 = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", filename4 + ".gfdcache");
                        string cachepath5 = Path.Combine(ETC.CachePath, "Doll", "Normal", filename5 + ".gfdcache");
                        string url4 = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", filename4 + ".png");
                        string url4_1 = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", filename4 + ".png");
                        string url5 = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", filename5 + ".png");
                        await wc.DownloadFileTaskAsync(url4, cachepath4);
                        await wc.DownloadFileTaskAsync(url4_1, cachepath4_1);
                        await wc.DownloadFileTaskAsync(url5, cachepath5);
                    }

                    foreach (string s in icons)
                    {
                        string filename = s;
                        string cachepath = Path.Combine(ETC.CachePath, "Equip", "Normal", filename + ".gfdcache");
                        string url = Path.Combine(ETC.Server, "Data", "Images", "Equipments", filename + ".png");
                        await wc.DownloadFileTaskAsync(url, cachepath);
                    }

                    foreach (DataRow dr in ETC.FairyList.Rows)
                    {
                        for (int i = 0; i < 3; ++i)
                        {
                            string filename = (string)dr["Name"] + "_" + (i + 1);
                            string cachepath = Path.Combine(ETC.CachePath, "Fairy", filename + ".gfdcache");
                            string url = Path.Combine(ETC.Server, "Data", "Images", "Fairy", filename + ".png");
                            await wc.DownloadFileTaskAsync(url, cachepath);
                        }

                        string filename2 = (string)dr["SkillName"];
                        string cachepath2 = Path.Combine(ETC.CachePath, "Fairy", "Skill", filename2 + ".gfdcache");
                        string url2 = Path.Combine(ETC.Server, "Data", "Images", "FairySkill", filename2 + ".png");
                        await wc.DownloadFileTaskAsync(url2, cachepath2);

                        string filename3 = (string)dr["Name"];
                        string cachepath3 = Path.Combine(ETC.CachePath, "Fairy", "Normal_Crop", filename3 + ".gfdcache");
                        string url3 = Path.Combine(ETC.Server, "Data", "Images", "Fairy", "Normal_Crop", filename3 + ".png");
                        await wc.DownloadFileTaskAsync(url3, cachepath3);
                    }

                    foreach (string s in ImageName)
                    {
                        string url = Path.Combine(ETC.Server, "Data", "Images", "OldGFD", "Images", s + ".png");
                        string target = Path.Combine(ETC.CachePath, "OldGFD", "Images", s + ".gfdcache");

                        await wc.DownloadFileTaskAsync(url, target);
                    }
                    wc.DownloadFile(Path.Combine(ETC.Server, "OldGFDVer.txt"), Path.Combine(ETC.SystemPath, "OldGFDVer.txt"));

                    ad.SetTitle(Resource.String.SettingActivity_DownloadAllCache_CompleteDialogTitle);
                    ad.SetMessage(Resource.String.SettingActivity_DownloadAllCache_CompleteDialogMessage);
                    ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                    ad.Show();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                ETC.LogError(ex, Activity);

                ad.SetTitle(Resource.String.SettingActivity_DownloadAllCache_FailDialogTitle);
                ad.SetMessage(Resource.String.SettingActivity_DownloadAllCache_FailDialogMessage);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);

                ad.SetTitle(Resource.String.SettingActivity_DownloadAllCache_FailDialogTitle);
                ad.SetMessage(Resource.String.SettingActivity_DownloadAllCache_FailDialogMessage);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                ad.Show();
            }

            finally
            {
                p_dialog.Dismiss();
                p_dialog = null;
            }
        }*/

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            now += 1;

            totalProgressBar.Progress = Convert.ToInt32(now / Convert.ToDouble(total) * 100);
            totalProgress.Text = $"{totalProgressBar.Progress}%";
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            nowProgressBar.Progress = e.ProgressPercentage;
            nowProgress.Text = $"{e.ProgressPercentage}%";
        }

        private void CleanCache_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            long tTotalSize = 0;

            foreach (string s in Directory.GetFiles(ETC.cachePath, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fi = new FileInfo(s);
                tTotalSize += fi.Length;
            }

            int totalSize = Convert.ToInt32(tTotalSize / 1024 / 1024);

            var alert = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBG);
            alert.SetTitle(Resource.String.SettingActivity_DeleteAllCache_DialogTitle);
            alert.SetMessage(string.Format("{0} {1}{2}", Resources.GetString(Resource.String.SettingActivity_DeleteAllCache_DialogCheckMessage), totalSize, "MB"));
            alert.SetCancelable(true);
            alert.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            alert.SetPositiveButton(Resource.String.AlertDialog_Delete, delegate { CleanCacheProcess(); });

            alert.Show();
        }

        private async void CleanCacheProcess()
        {
            Dialog dialog = null;

            try
            {
                var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBGDownload);
                ad.SetTitle(Resource.String.SettingActivity_DeleteAllCache_DialogTitle);
                ad.SetMessage(Resource.String.SettingActivity_DeleteAllCache_DialogMessage);
                ad.SetCancelable(false);
                ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);

                dialog = ad.Show();

                await Task.Delay(1000);

                await Task.Run(() =>
                {
                    if (Directory.Exists(ETC.cachePath))
                    {
                        Directory.Delete(ETC.cachePath, true);
                    }

                    if (File.Exists(Path.Combine(ETC.systemPath, "OldGFDVer.txt")))
                    {
                        File.Delete(Path.Combine(ETC.systemPath, "OldGFDVer.txt"));
                    }

                    Directory.CreateDirectory(ETC.cachePath);
                    ETC.CheckInitFolder();
                });

                Toast.MakeText(Activity, Resource.String.SettingActivity_DeleteAllCache_Complete, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.SettingActivity_DeleteAllCache_Fail, ToastLength.Short).Show();
            }
            finally
            {
                dialog.Dismiss();
            }
        }
    }
}