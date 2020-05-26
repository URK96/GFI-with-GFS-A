using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using System.Collections.Generic;
using System.Security.Cryptography;
using Xamarin.Essentials;
using Android.Content.PM;

namespace GFI_with_GFS_A
{
    internal static class ETC
    {
        internal static string server = "http://chlwlsgur96.ipdisk.co.kr/publist/HDD1/Data/Project/GFS/";
        internal static string sdCardPath;
        internal static string tempPath;
        internal static string appDataPath;
        internal static string dbPath;
        internal static string systemPath;
        internal static string cachePath;
        internal static string logPath;

        internal static bool isReleaseMode = true;
        internal static bool hasBasicInit = false;
        internal static bool isLowRAM = false;
        internal static bool useLightTheme = false;

        internal static int dialogBG = 0;
        internal static int dialogBGVertical = 0;
        internal static int dialogBGDownload = 0;
        internal static int dbVersion = 0;

        internal static DataTable dollList = new DataTable();
        internal static DataTable equipmentList = new DataTable();
        internal static DataTable fairyList = new DataTable();
        internal static DataTable enemyList = new DataTable();
        internal static DataTable fstList = new DataTable();
        internal static DataTable coalitionList = new DataTable();
        internal static DataTable skillTrainingList = new DataTable();
        internal static DataTable mdSupportList = new DataTable();
        internal static DataTable freeOPList = new DataTable();
        internal static string[] dbFiles =
        {
            "Doll.gfs",
            "Equipment.gfs",
            "Fairy.gfs",
            "Enemy.gfs",
            "FST.gfs",
            "Coalition.gfs",
            "MDSupportList.gfs",
            "FreeOP.gfs",
            "SkillTraining.gfs",
            "FairyAttribution.gfs"
        };

        internal static Java.Util.Locale locale; // ko, en
        internal static Android.Content.Res.Resources Resources = null;
        internal static Context baseContext;

        internal static ISharedPreferences sharedPreferences;

        internal static bool isServerDown = false;

        internal static void SetDialogTheme()
        {
            dialogBG = useLightTheme ? Resource.Style.GFD_Dialog_Light : Resource.Style.GFD_Dialog;
            dialogBGVertical = useLightTheme ? Resource.Style.GFD_Dialog_Vertical_Light : Resource.Style.GFD_Dialog_Vertical;
            dialogBGDownload = useLightTheme ? Resource.Style.GFD_Dialog_Download_Light : Resource.Style.GFD_Dialog_Download;
        }

        internal static void BasicInitializeApp(Context context)
        {
#if DEBUG
            isReleaseMode = false;
#endif

            sharedPreferences = Android.Support.V7.Preferences.PreferenceManager.GetDefaultSharedPreferences(context);
            useLightTheme = sharedPreferences.GetBoolean("UseLightTheme", false);

            SetDialogTheme();
            SetLanguage(context);

            Resources = baseContext.Resources;
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjQzMzY5QDMxMzgyZTMxMmUzMG9YTFRBTHp5bERLSm55aDF3YTdxKzcwWHNMK2k3QTZTTGxiajBoRU5odDg9");

            //sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            appDataPath = FileSystem.AppDataDirectory; //baseContext.GetExternalFilesDir(null).AbsolutePath; //Path.Combine(sdCardPath, "Android", "data", "com.gfl.dic");
            tempPath = Path.Combine(appDataPath, "GFDTemp");
            dbPath = Path.Combine(appDataPath, "DB");
            systemPath = Path.Combine(appDataPath, "System");
            cachePath = Path.Combine(appDataPath, "Cache");
            logPath = Path.Combine(systemPath, "Log");

            if (isReleaseMode)
            {
                AppCenter.Start("aca0ed39-4b25-4548-bf2a-ac92ccee2977", typeof(Analytics), typeof(Crashes), typeof(Push));
            }
            else
            {
                AppCenter.Start("aca0ed39-4b25-4548-bf2a-ac92ccee2977", typeof(Push));
            }
        }

        internal static void SetLanguage(Context context)
        {
            int langIndex = int.Parse(sharedPreferences.GetString("AppLanguage", "0"));

            var locale = langIndex switch
            {
                1 => Java.Util.Locale.Korea,
                2 => Java.Util.Locale.Us,
                _ => Java.Util.Locale.Default,
            };

            var config = new Android.Content.Res.Configuration
            {
                Locale = locale
            };

            ETC.locale = locale;
            baseContext = context.CreateConfigurationContext(config);
        }

        internal static void RunHelpActivity(Activity activity, string type)
        {
            /*Intent intent = new Intent(activity, typeof(HelpImageActivity));
            intent.PutExtra("Type", type);
            activity.StartActivity(intent);
            activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);*/
        }

        internal static async Task AnimateText(TextView view, string text)
        {
            view.Text = "";

            for (int i = 0; i < text.Length; ++i)
            {
                view.Text += text[i];
                await Task.Delay(50);
            }
        }

        internal static async Task CheckServerNetwork()
        {
            await Task.Delay(100);

            try
            {
                _ = Uri.TryCreate(server, UriKind.RelativeOrAbsolute, out Uri uri);
                var request = WebRequest.Create(uri) as HttpWebRequest;
                request.Method = "HEAD";

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    isServerDown = !(response.StatusCode == HttpStatusCode.OK);

                    if (response != null)
                    {
                        response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                isServerDown = true;
            }
        }

        /*internal static async Task UpViewAlpha(View view, int rate, int delay)
        {
            if (rate <= 0) rate = 1;
            int mag = 10 * rate;

            for (int i = 0; i < mag; ++i)
            {
                view.Alpha += Convert.ToSingle(1.0 / mag);
                await Task.Delay(delay);
                if (view.Alpha == 1) break;
            }
        }

        internal static async Task DownViewAlpha(View view, int rate, int delay)
        {
            if (rate <= 0) rate = 1;
            int mag = 10 * rate;

            for (int i = 0; i < mag; ++i)
            {
                view.Alpha -= Convert.ToSingle(1.0 / mag);
                await Task.Delay(delay);
                if (view.Alpha == 0) break;
            }
        }

        internal static async Task UpProgressBarProgress(ProgressBar pb, int start, int end, int delay)
        {
            for (int i = start; i <= end; ++i)
            {
                pb.Progress = i;
                await Task.Delay(delay);
            }
        }

        internal static async Task DownProgressBarProgress(ProgressBar pb, int start, int end, int delay)
        {
            for (int i = start; i <= end; --i)
            {
                pb.Progress = i;
                await Task.Delay(delay);
            }
        }*/

        internal static DataRow FindDataRow<T>(DataTable table, string index, T value)
        {
            for (int i = 0; i < table.Rows.Count; ++i)
            {
                DataRow dr = table.Rows[i];

                if (((T)dr[index]).Equals(value))
                {
                    return dr;
                }
            }

            return null;
        }

        internal static bool FindDataRow<T>(DataTable table, string index, T value, out DataRow row)
        {
            for (int i = 0; i < table.Rows.Count; ++i)
            {
                DataRow dr = table.Rows[i];

                if (((T)dr[index]).Equals(value))
                {
                    row = dr;
                    return true;
                }
            }

            row = null;
            return false;
        }

        internal static void CheckInitFolder()
        {
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            else
            {
                Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);
            }

            DirectoryInfo appDataDI = new DirectoryInfo(appDataPath);

            if (!appDataDI.Exists)
            {
                appDataDI.Create();
            }

            string[] MainPaths =
            {
                dbPath,
                systemPath,
                logPath,
                cachePath
            };

            string[] SubPaths =
            {
                Path.Combine(cachePath, "Doll"),
                Path.Combine(cachePath, "Doll", "SD"),
                Path.Combine(cachePath, "Doll", "SD", "Animation"),
                Path.Combine(cachePath, "Doll", "Normal_Crop"),
                Path.Combine(cachePath, "Doll", "Normal"),
                Path.Combine(cachePath, "Doll", "Skill"),
                Path.Combine(cachePath, "Doll", "ProductData"),
                Path.Combine(cachePath, "Doll", "GuideImage"),
                Path.Combine(cachePath, "Equip"),
                Path.Combine(cachePath, "Equip", "Normal"),
                Path.Combine(cachePath, "Equip", "ProductData"),
                Path.Combine(cachePath, "Fairy"),
                Path.Combine(cachePath, "Fairy", "Normal"),
                Path.Combine(cachePath, "Fairy", "Normal_Crop"),
                Path.Combine(cachePath, "Fairy", "Skill"),
                Path.Combine(cachePath, "Enemy"),
                Path.Combine(cachePath, "Enemy", "SD"),
                Path.Combine(cachePath, "Enemy", "Normal_Crop"),
                Path.Combine(cachePath, "Enemy", "Normal"),
                Path.Combine(cachePath, "FST"),
                Path.Combine(cachePath, "FST", "SD"),
                Path.Combine(cachePath, "FST", "Normal_Crop"),
                Path.Combine(cachePath, "FST", "Normal"),
                Path.Combine(cachePath, "FST", "Normal_Icon"),
                Path.Combine(cachePath, "FST", "Skill"),
                Path.Combine(cachePath, "Coalition"),
                Path.Combine(cachePath, "Coalition", "BG"),
                Path.Combine(cachePath, "Coalition", "Normal"),
                Path.Combine(cachePath, "Coalition", "Normal_Crop"),
                Path.Combine(cachePath, "Coalition", "Skill"),
                Path.Combine(cachePath, "OldGFD"),
                Path.Combine(cachePath, "OldGFD", "Images"),
                Path.Combine(cachePath, "Event"),
                Path.Combine(cachePath, "Event", "Images"),
                Path.Combine(cachePath, "Voices"),
                Path.Combine(cachePath, "Voices", "Doll"),
                Path.Combine(cachePath, "Voices", "Enemy"),
                Path.Combine(cachePath, "GuideBook"),
                Path.Combine(cachePath, "GuideBook", "PDFs"),
                Path.Combine(cachePath, "GuideBook", "Images"),
                Path.Combine(cachePath, "OST"),
                Path.Combine(cachePath, "Video", "PV")
            };

            foreach (string path in MainPaths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            foreach (string path in SubPaths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        internal static async Task<bool> LoadDB()
        {
            await Task.Delay(100);

            try
            {
                dollList.Clear();
                dollList.ReadXml(Path.Combine(dbPath, "Doll.gfs"));

                equipmentList.Clear();
                equipmentList.ReadXml(Path.Combine(dbPath, "Equipment.gfs"));

                fairyList.Clear();
                fairyList.ReadXml(Path.Combine(dbPath, "Fairy.gfs"));

                enemyList.Clear();
                enemyList.ReadXml(Path.Combine(dbPath, "Enemy.gfs"));

                fstList.Clear();
                fstList.ReadXml(Path.Combine(dbPath, "FST.gfs"));

                coalitionList.Clear();
                coalitionList.ReadXml(Path.Combine(dbPath, "Coalition.gfs"));

                skillTrainingList.Clear();
                skillTrainingList.ReadXml(Path.Combine(dbPath, "SkillTraining.gfs"));

                mdSupportList.Clear();
                mdSupportList.ReadXml(Path.Combine(dbPath, "MDSupportList.gfs"));

                freeOPList.Clear();
                freeOPList.ReadXml(Path.Combine(dbPath, "FreeOP.gfs"));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static async Task<bool> LoadDB(DataTable table, string dbFile, bool beforeClear)
        {
            await Task.Delay(10);

            try
            {
                if (beforeClear)
                {
                    table.Clear();
                }

                table.ReadXml(Path.Combine(dbPath, dbFile));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static bool LoadDBSync(DataTable table, string dbFile, bool beforeClear)
        {
            try
            {
                if (beforeClear)
                {
                    table.Clear();
                }

                _ = table.ReadXml(Path.Combine(dbPath, dbFile));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static async Task<bool> CheckDBVersion()
        {
            if (isServerDown)
            {
                return false;
            }

            string localDBVerPath = Path.Combine(systemPath, "DBVer.txt");
            string serverDBVerPath = Path.Combine(server, "DBVer.txt");
            string tempDBVerPath = Path.Combine(tempPath, "DBVer.txt");

            bool hasDBUpdate = false;

            if (!File.Exists(localDBVerPath))
            {
                hasDBUpdate = true;
            }
            else
            {
                using (WebClient wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(serverDBVerPath, tempDBVerPath);
                }

                using (StreamReader sr1 = new StreamReader(new FileStream(localDBVerPath, FileMode.Open, FileAccess.Read)))
                {
                    using (StreamReader sr2 = new StreamReader(new FileStream(tempDBVerPath, FileMode.Open, FileAccess.Read)))
                    {
                        int localVer = int.Parse(sr1.ReadToEnd());
                        int serverVer = int.Parse(sr2.ReadToEnd());

                        dbVersion = localVer;

                        hasDBUpdate = (localVer < serverVer);
                    }
                }
            }

            return hasDBUpdate;
        }

        internal static async Task UpdateDB(Activity activity, bool dbLoad = false, int titleMsg = Resource.String.CheckDBUpdateDialog_Title, int messageMgs = Resource.String.CheckDBUpdateDialog_Message)
        {
            Dialog dialog;
            View v = activity.LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            TextView status = v.FindViewById<TextView>(Resource.Id.ProgressStatusMessage);
            ProgressBar totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
            TextView totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
            ProgressBar nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
            TextView nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

            var pd = new Android.Support.V7.App.AlertDialog.Builder(activity, dialogBGDownload);
            pd.SetTitle(titleMsg);
            pd.SetMessage(Resources.GetString(messageMgs));
            pd.SetView(v);
            pd.SetCancelable(false);

            dialog = pd.Create();
            dialog.Show();

            await Task.Delay(100);

            try
            {
                totalProgressBar.Max = dbFiles.Length;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                    {
                        nowProgressBar.Progress = e.ProgressPercentage;
                        nowProgress.Text = (e.BytesReceived > 2048) ? $"{e.BytesReceived / 1024}KB" : $"{e.BytesReceived}B";
                    };
                    wc.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                    {
                        totalProgressBar.Progress += 1;
                        totalProgress.Text = $"{totalProgressBar.Progress} / {totalProgressBar.Max}";
                    };

                    for (int i = 0; i < dbFiles.Length; ++i)
                    {
                        await wc.DownloadFileTaskAsync(Path.Combine(server, "Data", "DB", dbFiles[i]), Path.Combine(tempPath, dbFiles[i]));
                    }

                    await wc.DownloadFileTaskAsync(Path.Combine(server, "DBVer.txt"), Path.Combine(tempPath, "DBVer.txt"));

                    await Task.Delay(100);
                }

                for (int i = 0; i < dbFiles.Length; ++i)
                {
                    //File.Copy(Path.Combine(tempPath, DBFiles[i]), Path.Combine(DBPath, DBFiles[i]), true);
                    CopyFile(Path.Combine(tempPath, dbFiles[i]), Path.Combine(dbPath, dbFiles[i]));

                    await Task.Delay(100);
                }

                await Task.Delay(500);

                activity.RunOnUiThread(() => { status.Text = Resources.GetString(Resource.String.UpdateDBDialog_RefreshVersionMessage); });

                string oldVersion = Path.Combine(systemPath, "DBVer.txt");
                string newVersion = Path.Combine(tempPath, "DBVer.txt");

                //File.Copy(newVersion, oldVersion, true);
                CopyFile(newVersion, oldVersion);

                using (StreamReader sr = new StreamReader(new FileStream(oldVersion, FileMode.Open, FileAccess.Read)))
                {
                    _ = int.TryParse(sr.ReadToEnd(), out dbVersion);
                }

                await Task.Delay(500);

                if (dbLoad)
                {
                    activity.RunOnUiThread(() => { status.Text = Resources.GetString(Resource.String.UpdateDBDialog_LoadDB); });

                    await Task.Delay(100);
                    await LoadDB();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, activity);
            }
            finally
            {
                dialog.Dismiss();
            }
        }

        internal static void LogError(Exception ex, Activity activity = null)
        {
            try
            {
                DateTime now = DateTime.Now;

                string nowDateTime = $"{now.Year}{now.Month}{now.Day} {now.Hour}{now.Minute}{now.Second}";
                string ErrorFileName = $"{nowDateTime}-ErrorLog.txt";

                DirectoryInfo di = new DirectoryInfo(logPath);

                if (!di.Exists)
                {
                    di.Create();
                }

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(logPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                {
                    sw.Write(ex.ToString());
                }
            }
            catch (Exception)
            {
                if (activity != null)
                {
                    activity.RunOnUiThread(() => { Toast.MakeText(activity, "Error Write Log", ToastLength.Long).Show(); });
                }

                Crashes.TrackError(ex);
            }
        }

        internal static void ShowSnackbar(View v, string message, int time)
        {
            Snackbar sb = Snackbar.Make(v, message, time);
            //v.BringToFront();
            MainThread.BeginInvokeOnMainThread(() => { sb.Show(); });
        }
        
        internal static void ShowSnackbar(View v, string message, int time, Android.Graphics.Color color)
        {
            Snackbar sb = Snackbar.Make(v, message, time);
            //v.BringToFront();
            sb.View.SetBackgroundColor(color);
            MainThread.BeginInvokeOnMainThread(() => { sb.Show(); });
        }

        internal static void ShowSnackbar(View v, int stringResource, int time)
        {
            Snackbar sb = Snackbar.Make(v, stringResource, time);
            //v.BringToFront();
            MainThread.BeginInvokeOnMainThread(() => { sb.Show(); });
        }

        internal static void ShowSnackbar(View v, int stringResource, int time, Android.Graphics.Color color)
        {
            Snackbar sb = Snackbar.Make(v, stringResource, time);
            //v.BringToFront();
            sb.View.SetBackgroundColor(color);
            MainThread.BeginInvokeOnMainThread(() => { sb.Show(); });
        }

        internal static string CalcTime(int minute) => (minute != 0) ? $"{minute / 60} : {(minute % 60).ToString("D2")}" : Resources.GetString(Resource.String.Common_NonProduct);

        internal static bool IsDBNullOrBlank(DataRow dr, string index) => (dr[index] == DBNull.Value) || string.IsNullOrWhiteSpace((string)dr[index]);

        /// <summary>
        /// File Copy Temporary Replace Method
        /// </summary>
        /// <param name="source">Source file path</param>
        /// <param name="target">Target file path</param>
        /// <returns></returns>
        internal static bool CopyFile(string source, string target)
        {
            try
            {
                var fileBytes = File.ReadAllBytes(source);
                File.WriteAllBytes(target, fileBytes);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }

            return true;
        }

        internal static double[] CalcDPS(int fireRate, int attackSpeed, int enemyArmor, int accuracy, int enemyEvasion, int critical, int dummy, int penetration = 20, int criticalRate = 150)
        {
            double dAccuracy = accuracy / 100.0;
            double dCriticalRate = criticalRate / 100.0;
            double aps = 29.999994 / Math.Floor(1500.0 / attackSpeed);

            double[] power = 
            {
                Math.Max(fireRate * 0.85 + Math.Min(penetration - enemyArmor, 2), 1),
                Math.Max(fireRate * 1.15 + Math.Min(penetration - enemyArmor, 2), 1)
            };

            double[] normalDamage = { Math.Round(power[0]), Math.Round(power[1]) };
            double[] criticalDamage = { Math.Round(power[0] * dCriticalRate), Math.Round(power[1] * dCriticalRate) };
            double accuracyRate = (double)accuracy / (accuracy + enemyEvasion);

            double[] DPS =
            {
                (normalDamage[0] * (1 - dAccuracy) + criticalDamage[0] * dAccuracy) * aps * accuracyRate * dummy,
                (normalDamage[1] * (1 - dAccuracy) + criticalDamage[1] * dAccuracy) * aps * accuracyRate * dummy
            };

            return DPS;
        }

        internal static void ShuffleList<T>(this IList<T> list)
        {
            var provider = new RNGCryptoServiceProvider();
            byte[] rNum = new byte[1];
            T temp;
            int r;

            for (int i = 0; i < list.Count; ++i)
            {
                provider.GetBytes(rNum);
                r = rNum[0] % list.Count;

                temp = list[i];
                list[i] = list[r];
                list[r] = temp;
            }

            provider.Dispose();
        }

        internal static int CreateRandomNum(int mag = 1)
        {
            var provider = new RNGCryptoServiceProvider();
            byte[] rNum = new byte[1];

            provider.GetBytes(rNum);
            provider.Dispose();

            return rNum[0] * mag;
        }
    }
}