﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "설정", Theme = "@style/GFS.Setting", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SettingActivity : Activity
    {
        private CoordinatorLayout SnackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Setting_Light);

                // Create your application here
                SetContentView(Resource.Layout.SettingMainLayout);

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.SettingSnackbarLayout);

                Fragment SettingFragment = new MainSettingFragment();
                FragmentTransaction ft = FragmentManager.BeginTransaction();
                ft.Add(Resource.Id.SettingFragmentContainer, SettingFragment);
                ft.Commit();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
        }
    }

    public class MainSettingFragment : PreferenceFragment
    {
        private ISharedPreferencesEditor SaveSetting;

        private CoordinatorLayout SnackbarLayout;

        private Dialog dialog;
        private ProgressDialog p_dialog = null;
        private ProgressBar totalProgressBar = null;
        private ProgressBar nowProgressBar = null;
        private TextView totalProgress = null;
        private TextView nowProgress = null;

        private int total = 0;
        private int now = 0;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SnackbarLayout = Activity.FindViewById<CoordinatorLayout>(Resource.Id.SettingSnackbarLayout);
            SnackbarLayout.BringToFront();

            AddPreferencesFromResource(Resource.Xml.MainSetting);
            InitPreferences();
        }

        private void InitPreferences()
        {
            SaveSetting =  ETC.sharedPreferences.Edit();

            SwitchPreference LowMemoryOption = (SwitchPreference)FindPreference("LowMemoryOption");
            if (ETC.UseLightTheme == true) LowMemoryOption.SetIcon(Resource.Drawable.LowMemoryOptionIcon_WhiteTheme);
            else LowMemoryOption.SetIcon(Resource.Drawable.LowMemoryOptionIcon);
            LowMemoryOption.Checked = ETC.sharedPreferences.GetBoolean("LowMemoryOption", false);
            LowMemoryOption.PreferenceChange += delegate
            {
                SaveSetting.PutBoolean("LowMemoryOption", LowMemoryOption.Checked);
                SaveSetting.Apply();
            };

            SwitchPreference UseLightTheme = (SwitchPreference)FindPreference("UseLightTheme");
            if (ETC.UseLightTheme == true) UseLightTheme.SetIcon(Resource.Drawable.UseLightThemeIcon_WhiteTheme);
            else UseLightTheme.SetIcon(Resource.Drawable.UseLightThemeIcon);
            UseLightTheme.Checked = ETC.sharedPreferences.GetBoolean("UseLightTheme", false);
            UseLightTheme.PreferenceChange += delegate
            {
                ETC.UseLightTheme = UseLightTheme.Checked;
                SaveSetting.PutBoolean("UseLightTheme", UseLightTheme.Checked);
                SaveSetting.Apply();
            };

            SwitchPreference DBListImage = (SwitchPreference)FindPreference("DBListImageShow");
            if (ETC.UseLightTheme == true) DBListImage.SetIcon(Resource.Drawable.DBListImageIcon_WhiteTheme);
            else DBListImage.SetIcon(Resource.Drawable.DBListImageIcon);
            DBListImage.Checked = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);
            DBListImage.PreferenceChange += delegate 
            {
                SaveSetting.PutBoolean("DBListImageShow", DBListImage.Checked);
                SaveSetting.Apply();
            };

            SwitchPreference DBDetailBackgroundImage = (SwitchPreference)FindPreference("DBDetailBackgroundImage");
            if (ETC.UseLightTheme == true) DBDetailBackgroundImage.SetIcon(Resource.Drawable.DBDetailBackgroundImageIcon_WhiteTheme);
            else DBDetailBackgroundImage.SetIcon(Resource.Drawable.DBDetailBackgroundImageIcon);
            DBDetailBackgroundImage.Checked = ETC.sharedPreferences.GetBoolean("DBDetailBackgroundImage", false);
            DBDetailBackgroundImage.PreferenceChange += delegate
            {
                SaveSetting.PutBoolean("DBDetailBackgroundImage", DBDetailBackgroundImage.Checked);
                SaveSetting.Apply();
            };

            Preference DownloadAllCache = FindPreference("DownloadAllCache");
            if (ETC.UseLightTheme == true) DownloadAllCache.SetIcon(Resource.Drawable.DownloadAllCacheIcon_WhiteTheme);
            else DownloadAllCache.SetIcon(Resource.Drawable.DownloadAllCacheIcon);
            DownloadAllCache.PreferenceClick += DownloadAllCache_PreferenceClick;

            Preference CleanCache = FindPreference("CleanCache");
            if (ETC.UseLightTheme == true) CleanCache.SetIcon(Resource.Drawable.CleanCacheIcon_WhiteTheme);
            else CleanCache.SetIcon(Resource.Drawable.CleanCacheIcon);
            CleanCache.PreferenceClick += CleanCache_PreferenceClick;

            //Preference OpenLogFolder = FindPreference("OpenLogFolder");

            Preference DeleteAllLogFile = FindPreference("DeleteAllLogFile");
            if (ETC.UseLightTheme == true) DeleteAllLogFile.SetIcon(Resource.Drawable.DeleteAllLogFileIcon_WhiteTheme);
            else DeleteAllLogFile.SetIcon(Resource.Drawable.DeleteAllLogFileIcon);
            DeleteAllLogFile.PreferenceClick += DeleteAllLogFile_PreferenceClick;
        }

        private void DeleteAllLogFile_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG);
                ad.SetTitle("로그 파일 삭제 확인");
                ad.SetMessage("로그 파일을 전부 삭제하실건가요?");
                ad.SetCancelable(true);
                ad.SetNegativeButton("취소", delegate { });
                ad.SetPositiveButton("확인", delegate { CleanLogFolderProcess(); });

                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                Toast.MakeText(Activity, Resource.String.Setting_DeleteAllLogFile_Fail, ToastLength.Short).Show();
            }
        }

        private async void CleanLogFolderProcess()
        {
            //View v = Activity.LayoutInflater.Inflate(Resource.Layout.SpinnerProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG_Download);
            ad.SetTitle(Resource.String.Setting_DeleteAllLogFile_Title);
            ad.SetMessage(Resource.String.Setting_DeleteAllLogFile_Message);
            ad.SetCancelable(false);
            //ad.SetView(v);
            ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);

            var dialog = ad.Show();

            await Task.Delay(1000);

            try
            {
                await Task.Run(() =>
                {
                    if (Directory.Exists(ETC.LogPath) == true) Directory.Delete(ETC.LogPath, true);

                    Directory.CreateDirectory(ETC.LogPath);
                    ETC.CheckInitFolder();
                });

                Toast.MakeText(Activity, Resource.String.Setting_DeleteAllLogFile_Complete, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                Toast.MakeText(Activity, Resource.String.Setting_DeleteAllLogFile_Fail, ToastLength.Short).Show();
            }
            finally
            {
                dialog.Dismiss();
            }
        }

        private void DownloadAllCache_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG);
            alert.SetTitle(Resource.String.Setting_DownloadAllCache_CheckTitle);
            alert.SetMessage(Resource.String.Setting_DownloadAllCache_CheckMessage);
            alert.SetCancelable(true);
            alert.SetNegativeButton("취소", delegate { });
            alert.SetPositiveButton("다운로드", async delegate
            {
                if (await CheckStorageCapacity() == false) return;
                string[] OSVer = Build.VERSION.Release.Split('.');
                if (Int32.Parse(OSVer[0]) <= 4) await DownloadAllCacheProcess_OldVer();
                else await DownloadAllCacheProcess();
            });

            alert.Show();
        }

        private async Task<bool> CheckStorageCapacity()
        {
            ProgressDialog pd = new ProgressDialog(Activity, ETC.DialogBG);
            pd.SetTitle(Resource.String.CheckFreeStorage_Title);
            pd.SetMessage(Resources.GetString(Resource.String.CheckFreeStorage_Message));
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
                    pd.SetMessage("저장 장치 여유 공간 : " + FreeSpace + "MB\n\n잠시 후 다운로드를 진행할게.");
                    await Task.Delay(2000);
                    return true;
                }
                else
                {
                    pd.Dismiss();

                    AlertDialog.Builder ad = new AlertDialog.Builder(Activity);
                    ad.SetTitle("다운로드 진행 불가");
                    ad.SetMessage("지휘관님! 여유 공간이 충분하지 않아 다운로드를 진행할 수 없어요... 현재 남은 용량은 " + FreeSpace + "MB");
                    ad.SetNeutralButton("확인", delegate { });
                    ad.SetCancelable(true);

                    ad.Show();
                    return false;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.CheckFreeStorage_CheckFail, Snackbar.LengthLong);
            }
            finally
            {
                pd.Dismiss();
                pd.Dispose();
                stat.Dispose();
            }

            return false;
        }

        private async Task DownloadAllCacheProcess()
        {
            View v = Activity.LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG);
            Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG_Download);
            pd.SetTitle(Resource.String.Setting_DownloadAllCache_Title);
            pd.SetMessage(Resource.String.Setting_DownloadAllCache_Message);
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

                status.Text = "인형 DB 읽는 중...";

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

                status.Text = "인형 캐시 다운로드 중...(1/4)";

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    foreach (DataRow dr in ETC.DollList.Rows)
                    {
                        int count = 1;

                        if (dr["Costume"] != DBNull.Value)
                        {
                            if (System.String.IsNullOrEmpty((string)dr["Costume"]) == false)
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

                status.Text = "요정 DB 읽는 중...";

                await Task.Delay(500);

                // Normal 3장 + Crop 1장 + 스킬 1장
                totalCount = ETC.FairyList.Rows.Count * 5;
                total = totalCount;
                now = 0;

                status.Text = "요정 캐시 다운로드 중...(2/4)";

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

                status.Text = "장비 DB 읽는 중...";

                await Task.Delay(500);

                ArrayList icons = new ArrayList();

                for (int i = 0; i < ETC.EquipmentList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.EquipmentList.Rows[i];

                    icons.TrimToSize();

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

                icons.TrimToSize();

                totalCount = icons.Count;
                total = totalCount;
                now = 0;

                status.Text = "장비 캐시 다운로드 중...(3/4)";

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

                status.Text = "소전사전 이미지 DB 읽는 중...";

                await Task.Delay(500);

                string[] ImageName = { "ProductTable_Doll", "ProductTable_Equipment", "ProductTable_Fairy", "RecommendDollRecipe", "RecommendEquipmentRecipe", "RecommendMD", "RecommendLeveling" };

                totalCount += ImageName.Length;
                total = totalCount;
                now = 0;

                status.Text = "소전사전 이미지 캐시 다운로드 중...(4/4)";

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

                ad.SetTitle(Resource.String.Setting_DownloadAllCache_CompleteTitle);
                ad.SetMessage(Resource.String.Setting_DownloadAllCache_CompleteMessage);
                ad.SetPositiveButton("확인", delegate { });

                ad.Show();
            }
            catch (Java.Lang.Exception ex)
            {
                ETC.LogError(this.Activity, ex.ToString());

                ad.SetTitle(Resource.String.Setting_DownloadAllCache_FailTitle);
                ad.SetMessage(Resource.String.Setting_DownloadAllCache_FailMessage);
                ad.SetPositiveButton("확인", delegate { });

                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this.Activity, ex.ToString());

                ad.SetTitle(Resource.String.Setting_DownloadAllCache_FailTitle);
                ad.SetMessage(Resource.String.Setting_DownloadAllCache_FailMessage);
                ad.SetPositiveButton("확인", delegate { });

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
            pd.SetTitle(Resource.String.Setting_DownloadAllCache_Title);
            pd.SetMessage(Resources.GetString(Resource.String.Setting_DownloadAllCache_Message));
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

                // 요정별 일러스트 3장 + 스킬 아이콘 1개 + Crop 1장
                totalCount += ETC.FairyList.Rows.Count * 5;

                ArrayList icons = new ArrayList();

                for (int i = 0; i < ETC.EquipmentList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.EquipmentList.Rows[i];

                    icons.TrimToSize();

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

                icons.TrimToSize();

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
                            if (System.String.IsNullOrEmpty((string)dr["Costume"]) == false)
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

                    ad.SetTitle(Resource.String.Setting_DownloadAllCache_CompleteTitle);
                    ad.SetMessage(Resource.String.Setting_DownloadAllCache_CompleteMessage);
                    ad.SetPositiveButton("확인", delegate { });

                    ad.Show();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                ETC.LogError(this.Activity, ex.ToString());

                ad.SetTitle(Resource.String.Setting_DownloadAllCache_FailTitle);
                ad.SetMessage(Resource.String.Setting_DownloadAllCache_FailMessage);
                ad.SetPositiveButton("확인", delegate { });

                ad.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this.Activity, ex.StackTrace);

                ad.SetTitle(Resource.String.Setting_DownloadAllCache_FailTitle);
                ad.SetMessage(Resource.String.Setting_DownloadAllCache_FailMessage);
                ad.SetPositiveButton("확인", delegate { });

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
            totalProgress.Text = totalProgressBar.Progress + "%";
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            nowProgressBar.Progress = e.ProgressPercentage;
            nowProgress.Text = e.ProgressPercentage.ToString() + "%";
        }

        private void CleanCache_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG);
            alert.SetTitle(Resource.String.Setting_DeleteAllCache_CheckTitle);
            alert.SetMessage(Resource.String.Setting_DeleteAllCache_CheckMessage);
            alert.SetCancelable(true);
            alert.SetNegativeButton("취소", delegate { });
            alert.SetPositiveButton("삭제", delegate { CleanCacheProcess(); });

            alert.Show();
        }

        private async void CleanCacheProcess()
        {
            //View v = Activity.LayoutInflater.Inflate(Resource.Layout.SpinnerProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG_Download);
            ad.SetTitle(Resource.String.Setting_DeleteAllCache_Title);
            ad.SetMessage(Resource.String.Setting_DeleteAllCache_Message);
            ad.SetCancelable(false);
            //ad.SetView(v);
            ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);

            var dialog = ad.Show();

            await Task.Delay(1000);

            try
            {
                await Task.Run(() =>
                {
                    if (Directory.Exists(ETC.CachePath) == true) Directory.Delete(ETC.CachePath, true);
                    if (File.Exists(Path.Combine(ETC.SystemPath, "OldGFDVer.txt")) == true) File.Delete(Path.Combine(ETC.SystemPath, "OldGFDVer.txt"));

                    Directory.CreateDirectory(ETC.CachePath);
                    ETC.CheckInitFolder();
                });

                Toast.MakeText(Activity, Resource.String.Setting_DeleteAllCache_Complete, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                Toast.MakeText(Activity, Resource.String.Setting_DeleteAllCache_Fail, ToastLength.Short).Show();
            }
            finally
            {
                dialog.Dismiss();
            }
        }
    }
}