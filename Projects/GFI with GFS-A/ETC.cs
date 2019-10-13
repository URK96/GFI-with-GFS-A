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
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GFI_with_GFS_A
{
    internal static class ETC
    {
        internal static string server = "http://chlwlsgur96.ipdisk.co.kr/publist/HDD1/Data/Project/GFS/";
        internal static string sdCardPath = (string)Android.OS.Environment.ExternalStorageDirectory;
        internal static string tempPath = Path.Combine(sdCardPath, "GFDTemp");
        internal static string appDataPath = Path.Combine(sdCardPath, "Android", "data", "com.gfl.dic");
        internal static string dbPath = Path.Combine(appDataPath, "DB");
        internal static string systemPath = Path.Combine(appDataPath, "System");
        internal static string cachePath = Path.Combine(appDataPath, "Cache");
        internal static string logPath = Path.Combine(systemPath, "Log");

        internal static bool isReleaseMode = true;
        internal static bool hasInitDollAvgAbility = false;
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
        internal static DataTable FSTList = new DataTable();
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
            "MDSupportList.gfs",
            "FreeOP.gfs",
            "SkillTraining.gfs",
            "FairyAttribution.gfs"
        };

        internal static AverageAbility[] avgList;

        internal static Android.Media.MediaPlayer ostPlayer = null;
        internal static int[] ostIndex = { 0, 0 };

        internal static Java.Util.Locale locale; // ko, en
        internal static Android.Content.Res.Resources Resources = null;
        internal static Context baseContext;

        internal static ISharedPreferences sharedPreferences;

        internal static bool isServerDown = false;

        internal struct AverageAbility
        {
            public int HP { get; set; }
            public int FR { get; set; }
            public int EV { get; set; }
            public int AC { get; set; }
            public int AS { get; set; }
            public int AM { get; set; }
        }

        internal static void InitializeAverageAbility()
        {
            string[] abilityList = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
            const int typeCount = 6;
            const int abilityCount = 6;

            Doll doll;
            DollAbilitySet das;

            AverageAbility avg_HG = new AverageAbility();
            AverageAbility avg_SMG = new AverageAbility();
            AverageAbility avg_AR = new AverageAbility();
            AverageAbility avg_RF = new AverageAbility();
            AverageAbility avg_MG = new AverageAbility();
            AverageAbility avg_SG = new AverageAbility();

            avgList = new AverageAbility[typeCount]
            {
                avg_HG,
                avg_SMG,
                avg_AR,
                avg_RF,
                avg_MG,
                avg_SG
            };
            
            int[] count = { 0, 0, 0, 0, 0, 0 };
            int[,] total = new int[typeCount, abilityCount];

            for (int i = 0; i < typeCount; ++i)
                for (int j = 0; j < abilityCount; ++j)
                    total[i, j] = 0;

            for (int i = 0; i < dollList.Rows.Count; ++i)
            {
                int index = 0;

                doll = new Doll(dollList.Rows[i]);
                das = new DollAbilitySet(doll.Type);
                
                switch (doll.Type)
                {
                    case "HG":
                        index = 0;
                        break;
                    case "SMG":
                        index = 1;
                        break;
                    case "AR":
                        index = 2;
                        break;
                    case "RF":
                        index = 3;
                        break;
                    case "MG":
                        index = 4;
                        break;
                    case "SG":
                        index = 5;
                        break;
                }

                count[index] += 1;

                int growValue = int.Parse(doll.Abilities["Grow"].Split(';')[0]);
                int abilityValue;

                for (int j = 0; j < abilityList.Length; ++j)
                {
                    abilityValue = int.Parse(doll.Abilities[abilityList[j]].Split(';')[0]);
                    total[index, j] += das.CalcAbility(abilityList[j], abilityValue, growValue, 100, 100, false);
                }
                    
                if (doll.Type == "SG")
                {
                    abilityValue = int.Parse(doll.Abilities["Armor"].Split(';')[0]);
                    total[index, 5] += das.CalcAbility("Armor", abilityValue, growValue, 100, 100, false);
                }
            }

            int value;

            for (int i = 0; i < typeCount; ++i)
                for (int j = 0; j < abilityCount; ++j)
                {
                    value = Convert.ToInt32(Math.Round((double)total[i, j] / count[i]));

                    switch (j)
                    {
                        case 0:
                            avgList[i].HP = value;
                            break;
                        case 1:
                            avgList[i].FR = value;
                            break;
                        case 2:
                            avgList[i].EV = value;
                            break;
                        case 3:
                            avgList[i].AC = value;
                            break;
                        case 4:
                            avgList[i].AS = value;
                            break;
                        case 5:
                            avgList[i].AM = value;
                            break;
                    }
                }

            hasInitDollAvgAbility = true;
        }

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

            sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            useLightTheme = sharedPreferences.GetBoolean("UseLightTheme", false);
            SetDialogTheme();
            SetLanguage(context);
            Resources = baseContext.Resources;
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTU2MTM3QDMxMzcyZTMzMmUzMFQzNnlVTCtEVkxvYVUwYWdGUWczTnVyemdLREV6NGdKK1JmejFZRzN0WGs9");

            if (isReleaseMode)
            {
                AppCenter.Start("aca0ed39-4b25-4548-bf2a-ac92ccee2977", typeof(Analytics), typeof(Crashes));
            }
        }

        internal static void SetLanguage(Context context)
        {
            int langIndex = int.Parse(ETC.sharedPreferences.GetString("AppLanguage", "0"));

            Java.Util.Locale locale;

            switch (langIndex)
            {
                default:
                case 0:
                    locale = Java.Util.Locale.Default;
                    break;
                case 1:
                    locale = Java.Util.Locale.Korea;
                    break;
                case 2:
                    locale = Java.Util.Locale.Us;
                    break;
            }

            Android.Content.Res.Configuration config = new Android.Content.Res.Configuration
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
                HttpWebRequest request = WebRequest.Create(server) as HttpWebRequest;
                request.Method = "HEAD";

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    isServerDown = !(response.StatusCode == HttpStatusCode.OK);

                    if (response != null)
                        response.Close();
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
                    return dr;
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
                Directory.CreateDirectory(tempPath);
            else
            {
                Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);
            }

            DirectoryInfo AppDataDI = new DirectoryInfo(appDataPath);

            if (!AppDataDI.Exists)
                AppDataDI.Create();

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
                Path.Combine(cachePath, "Equip"),
                Path.Combine(cachePath, "Equip", "Normal"),
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
            };

            foreach (string path in MainPaths)
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            foreach (string path in SubPaths)
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
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
                FSTList.Clear();
                FSTList.ReadXml(Path.Combine(dbPath, "FST.gfs"));
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

        internal static async Task<bool> LoadDB(DataTable table, string DBFile, bool BeforeClear)
        {
            await Task.Delay(10);

            try
            {
                if (BeforeClear)
                    table.Clear();

                table.ReadXml(Path.Combine(dbPath, DBFile));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static bool LoadDBSync(DataTable table, string DBFile, bool BeforeClear)
        {
            try
            {
                if (BeforeClear)
                    table.Clear();

                table.ReadXml(Path.Combine(dbPath, DBFile));
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
                return false;

            string LocalDBVerPath = Path.Combine(systemPath, "DBVer.txt");
            string ServerDBVerPath = Path.Combine(server, "DBVer.txt");
            string TempDBVerPath = Path.Combine(tempPath, "DBVer.txt");

            bool HasDBUpdate = false;

            if (!File.Exists(LocalDBVerPath))
                HasDBUpdate = true;
            else
            {
                using (WebClient wc = new WebClient())
                    await wc.DownloadFileTaskAsync(ServerDBVerPath, TempDBVerPath);

                using (StreamReader sr1 = new StreamReader(new FileStream(LocalDBVerPath, FileMode.Open, FileAccess.Read)))
                    using (StreamReader sr2 = new StreamReader(new FileStream(TempDBVerPath, FileMode.Open, FileAccess.Read)))
                    {
                        int localVer = int.Parse(sr1.ReadToEnd());
                        int serverVer = int.Parse(sr2.ReadToEnd());

                        dbVersion = localVer;

                        HasDBUpdate = (localVer < serverVer);
                    }
            }

            return HasDBUpdate;
        }

        internal static async Task UpdateDB(Activity activity, bool DBLoad = false, int TitleMsg = Resource.String.CheckDBUpdateDialog_Title, int MessageMgs = Resource.String.CheckDBUpdateDialog_Message)
        {
            View v = activity.LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            ProgressBar totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
            TextView totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
            ProgressBar nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
            TextView nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

            Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(activity, dialogBGDownload);
            pd.SetTitle(TitleMsg);
            pd.SetMessage(Resources.GetString(MessageMgs));
            pd.SetView(v);
            pd.SetCancelable(false);

            Dialog dialog = pd.Create();
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
                        await wc.DownloadFileTaskAsync(Path.Combine(server, "Data", "DB", dbFiles[i]), Path.Combine(tempPath, dbFiles[i]));

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

                activity.RunOnUiThread(() => { pd.SetMessage(Resources.GetString(Resource.String.UpdateDBDialog_RefreshVersionMessage)); });

                string oldVersion = Path.Combine(systemPath, "DBVer.txt");
                string newVersion = Path.Combine(tempPath, "DBVer.txt");

                //File.Copy(newVersion, oldVersion, true);
                CopyFile(newVersion, oldVersion);

                using (StreamReader sr = new StreamReader(new FileStream(oldVersion, FileMode.Open, FileAccess.Read)))
                    int.TryParse(sr.ReadToEnd(), out dbVersion);

                await Task.Delay(500);

                if (DBLoad)
                {
                    activity.RunOnUiThread(() => { pd.SetMessage(Resources.GetString(Resource.String.UpdateDBDialog_LoadDB)); });

                    await Task.Delay(100);
                    await LoadDB();
                }
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
                    di.Create();

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(logPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                    sw.Write(ex.ToString());
            }
            catch (Exception)
            {
                if (activity != null)
                    activity.RunOnUiThread(() => { Toast.MakeText(activity, "Error Write Log", ToastLength.Long).Show(); });
            }
        }

        internal static void ShowSnackbar(View v, string message, int time)
        {
            Snackbar sb = Snackbar.Make(v, message, time);
            v.BringToFront();
            sb.Show();
        }
        
        internal static void ShowSnackbar(View v, string message, int time, Android.Graphics.Color color)
        {
            Snackbar sb = Snackbar.Make(v, message, time);
            v.BringToFront();
            sb.View.SetBackgroundColor(color);
            sb.Show();
        }

        internal static void ShowSnackbar(View v, int StringResource, int time)
        {
            Snackbar sb = Snackbar.Make(v, StringResource, time);
            v.BringToFront();
            sb.Show();
        }

        internal static void ShowSnackbar(View v, int StringResource, int time, Android.Graphics.Color color)
        {
            Snackbar sb = Snackbar.Make(v, StringResource, time);
            v.BringToFront();
            sb.View.SetBackgroundColor(color);
            sb.Show();
        }

        internal static string CalcTime(int minute)
        {
            return (minute != 0) ? $"{minute / 60} : {(minute % 60).ToString("D2")}" : Resources.GetString(Resource.String.Common_NonProduct);

            /*if (minute != 0)
                return $"{minute / 60} : {(minute % 60).ToString("D2")}";
            else
                return Resources.GetString(Resource.String.Common_NonProduct);*/
        }

        internal static bool IsDBNullOrBlank(DataRow dr, string index)
        {
            return ((dr[index] == DBNull.Value) || (string.IsNullOrWhiteSpace((string)dr[index])));

            /*if (dr[index] == DBNull.Value)
                return true;
            if (string.IsNullOrWhiteSpace((string)dr[index]) == true)
                return true;

            return false;*/
        }

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

        internal static double[] CalcDPS(int FireRate, int AttackSpeed, int Enemy_Armor, int Accuracy, int Enemy_Evasion, int Critical, int Dummy, int Penetration = 10, int Critical_Rate = 150)
        {
            double dAccuracy = Accuracy / 100.0;
            double dCritical_Rate = Critical_Rate / 100.0;
            double APS = 29.999994 / Math.Floor(1500.0 / AttackSpeed);

            double[] Power = { 0, 0 };
            Power[0] = Math.Max(FireRate * 0.85 + Math.Min(Penetration - Enemy_Armor, 2), 1);
            Power[1] = Math.Max(FireRate * 1.15 + Math.Min(Penetration - Enemy_Armor, 2), 1);

            double[] Normal_Damage = { Math.Round(Power[0]), Math.Round(Power[1]) };
            double[] Critical_Damage = { Math.Round(Power[0] * dCritical_Rate), Math.Round(Power[1] * dCritical_Rate) };
            double Accuracy_Rate = (double)Accuracy / (Accuracy + Enemy_Evasion);

            double[] DPS =
            {
                (Normal_Damage[0] * (1 - dAccuracy) + Critical_Damage[0] * dAccuracy) * APS * Accuracy_Rate * Dummy,
                (Normal_Damage[1] * (1 - dAccuracy) + Critical_Damage[1] * dAccuracy) * APS * Accuracy_Rate * Dummy
            };

            return DPS;
        }

        internal static void ShuffleList<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            byte[] r_num = new byte[1];
            T temp;
            int r = 0;

            for (int i = 0; i < list.Count; ++i)
            {
                provider.GetBytes(r_num);
                r = r_num[0] % list.Count;

                temp = list[i];
                list[i] = list[r];
                list[r] = temp;
            }

            provider.Dispose();
        }

        internal static int CreateRandomNum(int mag = 1)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            byte[] r_num = new byte[1];

            provider.GetBytes(r_num);

            provider.Dispose();

            return r_num[0] * mag;
        }
    }
}