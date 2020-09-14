using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using Net.ArcanaStudio.ColorPicker;
using System;
using System.Collections.Generic;
using System.Data;
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

    public class MainSettingFragment : AndroidX.Preference.PreferenceFragmentCompat
    {
        private ISharedPreferencesEditor SaveSetting;

        private CoordinatorLayout SnackbarLayout;

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
            SnackbarLayout = Activity.FindViewById<CoordinatorLayout>(Resource.Id.SettingSnackbarLayout);
            SnackbarLayout.BringToFront();

            AddPreferencesFromResource(Resource.Xml.MainSetting);

            InitPreferences();
        }

        private void InitPreferences()
        {
            SaveSetting =  ETC.sharedPreferences.Edit();

            Preference updateLog = FindPreference("UpdateLog");
            if (ETC.useLightTheme)
            {
                updateLog.SetIcon(Resource.Drawable.updatehistory_icon_wt);
            }
            else
            {
                updateLog.SetIcon(Resource.Drawable.updatehistory_icon);
            }
            updateLog.PreferenceClick += delegate
            {
                var ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBGVertical);
                ad.SetTitle(Resource.String.NewFeatureDialog_Title);
                ad.SetCancelable(true);
                ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });

                try
                {
                    string assetName = "";

                    switch (ETC.locale.Language)
                    {
                        case "ko":
                            assetName = "UpdateFeature_ko.txt";
                            break;
                        default:
                            assetName = "UpdateFeature_en.txt";
                            break;
                    }

                    using (StreamReader sr = new StreamReader(Activity.Assets.Open(assetName)))
                    {
                        ad.SetMessage(sr.ReadToEnd());
                    }

                    ad.Show();
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, Activity);
                    ETC.ShowSnackbar(SnackbarLayout, Resource.String.Main_NotificationInitializeFail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
                }
            };

            ListPreference startAppMode = (ListPreference)FindPreference("StartAppMode");
            if (ETC.useLightTheme)
            {
                startAppMode.SetIcon(Resource.Drawable.appmode_icon_wt);
            }
            else
            {
                startAppMode.SetIcon(Resource.Drawable.appmode_icon);
            }
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
            startAppMode.SetValueIndex(int.Parse(ETC.sharedPreferences.GetString("StartAppMode", "0")));


            /*SwitchPreference lowMemoryOption = (SwitchPreference)FindPreference("LowMemoryOption");
            if (ETC.useLightTheme)
            {
                lowMemoryOption.SetIcon(Resource.Drawable.LowMemoryOptionIcon_WhiteTheme);
            }
            else
            {
                lowMemoryOption.SetIcon(Resource.Drawable.LowMemoryOptionIcon);
            }
            lowMemoryOption.Checked = ETC.sharedPreferences.GetBoolean("LowMemoryOption", false);
            lowMemoryOption.PreferenceChange += delegate
            {
                SaveSetting.PutBoolean("LowMemoryOption", lowMemoryOption.Checked);
                SaveSetting.Apply();
            };*/


            SwitchPreference useLightTheme = (SwitchPreference)FindPreference("UseLightTheme");
            if (ETC.useLightTheme)
            {
                useLightTheme.SetIcon(Resource.Drawable.themecolor_icon_wt);
            }
            else
            {
                useLightTheme.SetIcon(Resource.Drawable.themecolor_icon);
            }
            useLightTheme.Checked = ETC.sharedPreferences.GetBoolean("UseLightTheme", false);
            useLightTheme.PreferenceChange += delegate
            {
                ETC.useLightTheme = useLightTheme.Checked;
                SaveSetting.PutBoolean("UseLightTheme", useLightTheme.Checked);
                SaveSetting.Apply();
            };


            SwitchPreference enableServerCheck = (SwitchPreference)FindPreference("EnableServerCheck");
            if (ETC.useLightTheme)
            {
                enableServerCheck.SetIcon(Resource.Drawable.server_icon_wt);
            }
            else
            {
                enableServerCheck.SetIcon(Resource.Drawable.server_icon);
            }
            enableServerCheck.Checked = ETC.sharedPreferences.GetBoolean("EnableServerCheck", true);
            enableServerCheck.PreferenceChange += delegate
            {
                SaveSetting.PutBoolean("EnableServerCheck", enableServerCheck.Checked);
                SaveSetting.Apply();
            };

            /*ListPreference MainButtonColor = (ListPreference)FindPreference("MainButtonColor");
            if (ETC.UseLightTheme)
                MainButtonColor.SetIcon(Resource.Drawable.AppStartModeIcon_WhiteTheme);
            else
                MainButtonColor.SetIcon(Resource.Drawable.AppStartModeIcon);
            MainButtonColor.SetEntries(new string[]
            {
                "Green (Default)",
                "Orange"
            });
            MainButtonColor.SetEntryValues(new string[] { "0", "1" });
            MainButtonColor.SetValueIndex(int.Parse(ETC.sharedPreferences.GetString("MainButtonColor", "0")));*/

            ListPreference startMainFragment = (ListPreference)FindPreference("StartMainFragment");
            if (ETC.useLightTheme)
            {
                startMainFragment.SetIcon(Resource.Drawable.startscreen_icon_wt);
            }
            else
            {
                startMainFragment.SetIcon(Resource.Drawable.startscreen_icon);
            }
            startMainFragment.SetEntries(new string[]
            {
                Resources.GetString(Resource.String.Main_Category_Home),
                Resources.GetString(Resource.String.Main_Category_DB),
                Resources.GetString(Resource.String.Main_Category_GFDv1),
                Resources.GetString(Resource.String.Main_Category_GFUtil),
                Resources.GetString(Resource.String.Main_Category_Other),
            });
            startMainFragment.SetEntryValues(new string[] { "0", "1", "2", "3", "4" });
            startMainFragment.SetValueIndex(int.Parse(ETC.sharedPreferences.GetString("StartMainFragment", "0")));

            ListPreference appLanguage = (ListPreference)FindPreference("AppLanguage");
            if (ETC.useLightTheme)
            {
                appLanguage.SetIcon(Resource.Drawable.language_icon_wt);
            }
            else
            {
                appLanguage.SetIcon(Resource.Drawable.language_icon);
            }
            appLanguage.SetEntries(new string[]
            {
                "System Default",
                Resources.GetString(Resource.String.Common_Language_KR),
                Resources.GetString(Resource.String.Common_Language_EN)
            });
            appLanguage.SetEntryValues(new string[] { "0", "1", "2" });
            appLanguage.SetValueIndex(int.Parse(ETC.sharedPreferences.GetString("AppLanguage", "0")));


            SwitchPreference autoDBUpdate = (SwitchPreference)FindPreference("AutoDBUpdate");
            if (ETC.useLightTheme)
            {
                autoDBUpdate.SetIcon(Resource.Drawable.databasesync_icon_wt);
            }
            else
            {
                autoDBUpdate.SetIcon(Resource.Drawable.databasesync_icon);
            }
            autoDBUpdate.Checked = ETC.sharedPreferences.GetBoolean("AutoDBUpdate", true);
            autoDBUpdate.PreferenceChange += delegate
            {
                SaveSetting.PutBoolean("AutoDBUpdate", autoDBUpdate.Checked);
                SaveSetting.Apply();
            };


            Preference checkDBUpdate = FindPreference("CheckDBUpdate");
            if (ETC.useLightTheme)
            {
                checkDBUpdate.SetIcon(Resource.Drawable.databaseupdate_icon_wt);
            }
            else
            {
                checkDBUpdate.SetIcon(Resource.Drawable.databaseupdate_icon);
            }
            checkDBUpdate.PreferenceClick += CheckDBUpdate_PreferenceClick;


            Preference repairDB = FindPreference("RepairDB");
            if (ETC.useLightTheme)
            {
                repairDB.SetIcon(Resource.Drawable.repair_icon_wt);
            }
            else
            {
                repairDB.SetIcon(Resource.Drawable.repair_icon);
            }
            repairDB.PreferenceClick += RepairDB_PreferenceClick;


            SwitchPreference dbListImage = (SwitchPreference)FindPreference("DBListImageShow");
            if (ETC.useLightTheme)
            {
                dbListImage.SetIcon(Resource.Drawable.listmode_icon_wt);
            }
            else
            {
                dbListImage.SetIcon(Resource.Drawable.listmode_icon);
            }
            dbListImage.Checked = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);
            dbListImage.PreferenceChange += delegate 
            {
                SaveSetting.PutBoolean("DBListImageShow", dbListImage.Checked);
                SaveSetting.Apply();
            };


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


            SwitchPreference dbDetailBackgroundImage = (SwitchPreference)FindPreference("DBDetailBackgroundImage");
            if (ETC.useLightTheme)
            {
                dbDetailBackgroundImage.SetIcon(Resource.Drawable.backgroundimage_icon_wt);
            }
            else
            {
                dbDetailBackgroundImage.SetIcon(Resource.Drawable.backgroundimage_icon);
            }
            dbDetailBackgroundImage.Checked = ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false);
            dbDetailBackgroundImage.PreferenceChange += delegate
            {
                SaveSetting.PutBoolean("DBDetailBackgroundImage", dbDetailBackgroundImage.Checked);
                SaveSetting.Apply();
            };


            Preference textViewerTextSize = FindPreference("TextViewerTextSize");
            if (ETC.useLightTheme)
            {
                textViewerTextSize.SetIcon(Resource.Drawable.fontsize_icon_wt);
            }
            else
            {
                textViewerTextSize.SetIcon(Resource.Drawable.fontsize_icon);
            }
            textViewerTextSize.PreferenceClick += TextViewerTextSize_PreferenceClick;


            Preference textViewerTextColor = FindPreference("TextViewerTextColor");
            if (ETC.useLightTheme)
            {
                textViewerTextColor.SetIcon(Resource.Drawable.fontcolor_icon_wt);
            }
            else
            {
                textViewerTextColor.SetIcon(Resource.Drawable.fontcolor_icon);
            }
            textViewerTextColor.PreferenceClick += TextViewerTextColor_PreferenceClick;


            Preference textViewerBackgroundColor = FindPreference("TextViewerBackgroundColor");
            if (ETC.useLightTheme)
            {
                textViewerBackgroundColor.SetIcon(Resource.Drawable.backgroundcolor_icon_wt);
            }
            else
            {
                textViewerBackgroundColor.SetIcon(Resource.Drawable.backgroundcolor_icon);
            }
            textViewerBackgroundColor.PreferenceClick += TextViewerBackgroundColor_PreferenceClick;

            /*Preference DownloadAllCache = FindPreference("DownloadAllCache");
            if (ETC.UseLightTheme == true) DownloadAllCache.SetIcon(Resource.Drawable.DownloadAllCacheIcon_WhiteTheme);
            else DownloadAllCache.SetIcon(Resource.Drawable.DownloadAllCacheIcon);
            DownloadAllCache.PreferenceClick += DownloadAllCache_PreferenceClick;*/

            Preference cleanCache = FindPreference("CleanCache");
            if (ETC.useLightTheme)
            {
                cleanCache.SetIcon(Resource.Drawable.delete_icon_wt);
            }
            else
            {
                cleanCache.SetIcon(Resource.Drawable.delete_icon);
            }
            cleanCache.PreferenceClick += CleanCache_PreferenceClick;

            //Preference OpenLogFolder = FindPreference("OpenLogFolder");

            Preference deleteAllLogFile = FindPreference("DeleteAllLogFile");
            if (ETC.useLightTheme)
            {
                deleteAllLogFile.SetIcon(Resource.Drawable.deletelog_icon_wt);
            }
            else
            {
                deleteAllLogFile.SetIcon(Resource.Drawable.deletelog_icon);
            }
            deleteAllLogFile.PreferenceClick += DeleteAllLogFile_PreferenceClick;

            Preference externalLibraryLicense = FindPreference("ExternalLibraryLicense");
            if (ETC.useLightTheme)
            {
                externalLibraryLicense.SetIcon(Resource.Drawable.license_icon_wt);
            }
            else
            {
                externalLibraryLicense.SetIcon(Resource.Drawable.license_icon);
            }
            externalLibraryLicense.PreferenceClick += delegate { Activity.StartActivity(typeof(ExternLibraryCopyright)); };

            Preference gfdSourceCode = FindPreference("GFDSourceCode");
            if (ETC.useLightTheme)
            {
                gfdSourceCode.SetIcon(Resource.Drawable.sourcecode_icon_wt);
            }
            else
            {
                gfdSourceCode.SetIcon(Resource.Drawable.sourcecode_icon);
            }
            gfdSourceCode.PreferenceClick += async delegate
            {
                Toast.MakeText(Activity, Resource.String.Switch_ExternalBrowser, ToastLength.Short).Show();

                Uri uri = new Uri("https://github.com/URK96/GFI-with-GFS-A.git");
                await Browser.OpenAsync(uri, BrowserLaunchMode.External).ConfigureAwait(false);
            };
        }

        private async void CheckDBUpdate_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Dialog dialog = null;

            try
            {
                var ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBG);
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
                dialog.Dismiss();
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
                    Preference p = sender as Preference;

                    p.SetTitle(Resource.String.Firewall);
                    p.SetSummary(Resource.String.Firewall_Message);

                    if (ETC.useLightTheme)
                    {
                        p.SetIcon(Resource.Drawable.Hack_WhiteTheme);
                    }
                    else
                    {
                        p.SetIcon(Resource.Drawable.Hack);
                    }
                }
                else if (count > 5)
                {
                    var ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBG);
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
                    var ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBG);
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
                var ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBGDownload);
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
                dialog.Dismiss();
            }
        }

        private void TextViewerTextSize_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            View view = Activity.LayoutInflater.Inflate(Resource.Layout.NumberPickerDialogLayout, null);

            NumberPicker np = view.FindViewById<NumberPicker>(Resource.Id.NumberPickerControl);
            np.MaxValue = 30;
            np.MinValue = 4;
            np.Value = ETC.sharedPreferences.GetInt("TextViewerTextSize", 12);

            var ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBG);
            ad.SetTitle(Resource.String.Common_TextSize);
            ad.SetCancelable(true);
            ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            ad.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate
            {
                SaveSetting.PutInt("TextViewerTextSize", 12);
                SaveSetting.Apply();
            });
            ad.SetPositiveButton(Resource.String.AlertDialog_Set, delegate
            {
                SaveSetting.PutInt("TextViewerTextSize", np.Value);
                SaveSetting.Apply();
            });
            ad.SetView(view);

            ad.Show();
        }

        private void TextViewerTextColor_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            View view = Activity.LayoutInflater.Inflate(Resource.Layout.ColorPickerDialogLayout, null);

            ColorPickerView cp = view.FindViewById<ColorPickerView>(Resource.Id.ColorPickerControl);

            var ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBG);
            ad.SetTitle(Resource.String.Common_TextColor);
            ad.SetCancelable(true);
            ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            ad.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate
            {
                SaveSetting.PutString("TextViewerTextColorHex", "None");
                SaveSetting.Apply();
            });
            ad.SetPositiveButton(Resource.String.AlertDialog_Set, delegate
            {
                SaveSetting.PutString("TextViewerTextColorHex", $"#{cp.GetColor().ToArgb().ToString("X")}");
                SaveSetting.Apply();
            });
            ad.SetView(view);

            ad.Show();
        }

        private void TextViewerBackgroundColor_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            View view = Activity.LayoutInflater.Inflate(Resource.Layout.ColorPickerDialogLayout, null);

            ColorPickerView cp = view.FindViewById<ColorPickerView>(Resource.Id.ColorPickerControl);

            var ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBG);
            ad.SetTitle(Resource.String.Common_BackgroundColor);
            ad.SetCancelable(true);
            ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            ad.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate
            {
                SaveSetting.PutString("TextViewerBackgroundColorHex", "None");
                SaveSetting.Apply();
            });
            ad.SetPositiveButton(Resource.String.AlertDialog_Set, delegate
            {
                SaveSetting.PutString("TextViewerBackgroundColorHex", $"#{cp.GetColor().ToArgb().ToString("X")}");
                SaveSetting.Apply();
            });
            ad.SetView(view);

            ad.Show();
        }

        /*private void DownloadAllCache_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG);
            alert.SetTitle(Resource.String.SettingActivity_DownloadAllCache_DialogTitle);
            alert.SetMessage(Resource.String.SettingActivity_DownloadAllCache_DialogCheckMessage);
            alert.SetCancelable(true);
            alert.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            alert.SetPositiveButton(Resource.String.AlertDialog_Download, async delegate
            {
                await ETC.CheckServerNetwork();

                if (ETC.IsServerDown == true)
                {
                    Toast.MakeText(Activity, Resource.String.Common_ServerMaintenance, ToastLength.Short).Show();
                    return;
                }
                if (await CheckStorageCapacity() == false) return;
                string[] OSVer = Build.VERSION.Release.Split('.');
                if (int.Parse(OSVer[0]) <= 4) await DownloadAllCacheProcess_OldVer();
                else await DownloadAllCacheProcess();
            });

            alert.Show();
        }*/

        /*private async Task<bool> CheckStorageCapacity()
        {
            ProgressDialog pd = new ProgressDialog(Activity, ETC.dialogBG);
            pd.SetTitle(Resource.String.SettingActivity_CheckFreeStorage_DialogTitle);
            pd.SetMessage(Resources.GetString(Resource.String.SettingActivity_CheckFreeStorage_DialogMessage));
            pd.SetCancelable(false);

            pd.Show();

            Java.IO.File path = Android.OS.Environment.ExternalStorageDirectory;
            StatFs stat = new StatFs(path.Path);

            try
            {

                long blockSize = stat.BlockSizeLong;
                long availableBlocks = stat.AvailableBlocksLong;
                long availableSize = availableBlocks * blockSize;

                int FreeSpace = Convert.ToInt32((availableSize / 1024) / 1024);

                if (FreeSpace >= 600)
                {
                    pd.SetMessage(string.Format("{0} : {1}MB\n\n{2}", Resources.GetString(Resource.String.SettingActivity_CheckFreeStorage_DialogMessage2), FreeSpace, Resources.GetString(Resource.String.SettingActivity_CheckFreeStorage_DialogMessage2_1)));
                    await Task.Delay(2000);
                    return true;
                }
                else
                {
                    pd.Dismiss();

                    AlertDialog.Builder ad = new AlertDialog.Builder(Activity);
                    ad.SetTitle(Resource.String.SettingActivity_CheckFreeStorage_FailDialogTitle);
                    ad.SetMessage(string.Format("{0} {1}MB",Resources.GetString(Resource.String.SettingActivity_CheckFreeStorage_FailDialogMessage), FreeSpace));
                    ad.SetNeutralButton(Resource.String.AlertDialog_Confirm, delegate { });
                    ad.SetCancelable(true);

                    ad.Show();
                    return false;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.SettingActivity_CheckFreeStorage_CheckFail, Snackbar.LengthLong);
            }
            finally
            {
                pd.Dismiss();
                pd.Dispose();
                stat.Dispose();
            }

            return false;
        }*/

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
        }

        private void Wc_DownloadProgressChanged_OldVer(object sender, DownloadProgressChangedEventArgs e)
        {
            p_dialog.SecondaryProgress = e.ProgressPercentage;
        }

        private void Wc_DownloadFileCompleted_OldVer(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            now += 1;

            p_dialog.Progress = Convert.ToInt32((now / Convert.ToDouble(total)) * 100);
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            now += 1;

            totalProgressBar.Progress = Convert.ToInt32((now / Convert.ToDouble(total)) * 100);
            totalProgress.Text = string.Format("{0}%", totalProgressBar.Progress);
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            nowProgressBar.Progress = e.ProgressPercentage;
            nowProgress.Text = string.Format("{0}%", e.ProgressPercentage);
        }*/

        private void CleanCache_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            long tTotalSize = 0;

            foreach (string s in Directory.GetFiles(ETC.cachePath, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fi = new FileInfo(s);
                tTotalSize += fi.Length;
            }

            int totalSize = Convert.ToInt32(tTotalSize / 1024 / 1024);

            var alert = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBG);
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
                var ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBGDownload);
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