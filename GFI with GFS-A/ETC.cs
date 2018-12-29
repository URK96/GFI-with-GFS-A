using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.Preferences;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UptimeSharp;

namespace GFI_with_GFS_A
{
    internal static class ETC
    {
        internal static string Server = "http://chlwlsgur96.ipdisk.co.kr/publist/HDD1/Data/Project/GFS/";
        internal static string SDCardPath = (string)Android.OS.Environment.ExternalStorageDirectory;
        internal static string tempPath = Path.Combine(SDCardPath, "GFDTemp");
        internal static string AppDataPath = Path.Combine(SDCardPath, "Android", "data", "com.gfl.dic");
        internal static string DBPath = Path.Combine(AppDataPath, "DB");
        internal static string SystemPath = Path.Combine(AppDataPath, "System");
        internal static string CachePath = Path.Combine(AppDataPath, "Cache");
        internal static string LogPath = Path.Combine(SystemPath, "Log");

        internal static bool HasInitDollAvgAbility = false;
        internal static bool IsLowRAM = false;
        internal static bool UseLightTheme = false;
        internal static bool HasEvent = false;
        internal static Java.Util.Locale Language;

        internal static int DialogBG = 0;
        internal static int DialogBG_Vertical = 0;
        internal static int DialogBG_Download = 0;

        internal static DataTable DollList = new DataTable();
        internal static DataTable EquipmentList = new DataTable();
        internal static DataTable FairyList = new DataTable();
        internal static DataTable EnemyList = new DataTable();
        internal static DataTable FSTList = new DataTable();
        internal static DataTable SkillTrainingList = new DataTable();
        internal static DataTable MDSupportList = new DataTable();
        internal static DataTable FreeOPList = new DataTable();

        internal static AverageAbility[] Avg_List;

        internal static Android.Media.MediaPlayer OSTPlayer = null;
        internal static int[] OST_Index = { 0, 0 };

        internal static Android.Content.Res.Resources Resources = null;

        internal static ISharedPreferences sharedPreferences;

        internal static UptimeClient client = null;
        internal static bool IsServerDown = false;

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
            string[] AbilityList = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
            const int TypeCount = 6;
            const int AbilityCount = 6;

            AverageAbility Avg_HG = new AverageAbility();
            AverageAbility Avg_SMG = new AverageAbility();
            AverageAbility Avg_AR = new AverageAbility();
            AverageAbility Avg_RF = new AverageAbility();
            AverageAbility Avg_MG = new AverageAbility();
            AverageAbility Avg_SG = new AverageAbility();

            Avg_List = new AverageAbility[TypeCount]
            {
                Avg_HG,
                Avg_SMG,
                Avg_AR,
                Avg_RF,
                Avg_MG,
                Avg_SG
            };
            
            int[] count = { 0, 0, 0, 0, 0, 0 };
            int[,] total = new int[TypeCount, AbilityCount + 1];

            for (int i = 0; i < TypeCount; ++i)
                for (int j = 0; j < AbilityCount; ++j) total[i, j] = 0;

            for (int i = 0; i < DollList.Rows.Count; ++i)
            {
                Doll doll = new Doll(DollList.Rows[i]);
                int index = 0;
                
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

                for (int j = 0; j < AbilityList.Length; ++j)
                    total[index, j] += int.Parse(doll.Abilities[AbilityList[j]].Split(';')[0]);

                if (doll.Type == "SG")
                    total[index, 5] += int.Parse(doll.Abilities["Armor"].Split(';')[0]);

                total[index, 6] += int.Parse(doll.Abilities["Grow"].Split(';')[0]);
            }

            for (int i = 0; i < TypeCount; ++i)
            {
                DollAbilitySet DAS = null;

                switch (i)
                {
                    case 0:
                        DAS = new DollAbilitySet("HG");
                        break;
                    case 1:
                        DAS = new DollAbilitySet("SMG");
                        break;
                    case 2:
                        DAS = new DollAbilitySet("AR");
                        break;
                    case 3:
                        DAS = new DollAbilitySet("RF");
                        break;
                    case 4:
                        DAS = new DollAbilitySet("MG");
                        break;
                    case 5:
                        DAS = new DollAbilitySet("SG");
                        break;
                }

                for (int j = 0; j < AbilityCount; ++j)
                {
                    int value = Convert.ToInt32(Math.Round((double)total[i, j] / count[i]));
                    int grow = Convert.ToInt32(Math.Round((double)total[i, 6] / count[i]));

                    switch (j)
                    {
                        case 0:
                            Avg_List[i].HP = DAS.CalcAbility(AbilityList[j], value, grow, 100, 100, false);
                            break;
                        case 1:
                            Avg_List[i].FR = DAS.CalcAbility(AbilityList[j], value, grow, 100, 100, false);
                            break;
                        case 2:
                            Avg_List[i].EV = DAS.CalcAbility(AbilityList[j], value, grow, 100, 100, false);
                            break;
                        case 3:
                            Avg_List[i].AC = DAS.CalcAbility(AbilityList[j], value, grow, 100, 100, false);
                            break;
                        case 4:
                            Avg_List[i].AS = DAS.CalcAbility(AbilityList[j], value, grow, 100, 100, false);
                            break;
                        case 5:
                            Avg_List[i].AM = DAS.CalcAbility("Armor", value, grow, 100, 100, false);
                            break;
                    }
                }
            }

            HasInitDollAvgAbility = true;
        }

        internal static void SetDialogTheme()
        {
            if (UseLightTheme == true)
            {
                DialogBG = Resource.Style.GFD_Dialog_Light;
                DialogBG_Vertical = Resource.Style.GFD_Dialog_Vertical_Light;
                DialogBG_Download = Resource.Style.GFD_Dialog_Download_Light;
            }
            else
            {
                DialogBG = Resource.Style.GFD_Dialog;
                DialogBG_Vertical = Resource.Style.GFD_Dialog_Vertical;
                DialogBG_Download = Resource.Style.GFD_Dialog_Download;
            }
        }

        internal static void BasicInitializeApp(Activity context)
        {
            sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            Resources = context.Resources;
            UseLightTheme = sharedPreferences.GetBoolean("UseLightTheme", false);
            SetDialogTheme();
            Language = Resources.Configuration.Locale;
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NTQzNDRAMzEzNjJlMzQyZTMwZHNFSDUyRjdlWXZ6WXNtelNkRWV3QVh1WmR0Q3hSbTFqZ0dKTTVsQlBOQT0=");
            MobileAds.Initialize(context, "ca-app-pub-4576756770200148~8135834453");
            client = new UptimeClient("m780844852-8bd2516bb93800a9eb7e3d58");
        }

        internal static void RunHelpActivity(Activity activity, string type)
        {
            Intent intent = new Intent(activity, typeof(HelpImageActivity));
            intent.PutExtra("Type", type);
            activity.StartActivity(intent);
            activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
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
            try
            {
                var monitor = await client.GetMonitor("m780844852-8bd2516bb93800a9eb7e3d58");

                switch (monitor.Status)
                {
                    case UptimeSharp.Models.Status.Up:
                        IsServerDown = false;
                        break;
                    case UptimeSharp.Models.Status.Pause:
                    case UptimeSharp.Models.Status.SeemsDown:
                    case UptimeSharp.Models.Status.Down:
                    default:
                        IsServerDown = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
                IsServerDown = true;
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
                if (((T)dr[index]).Equals(value)) return dr;
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
            if (Directory.Exists(tempPath) == false) Directory.CreateDirectory(tempPath);
            else
            {
                Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);
            }

            DirectoryInfo AppDataDI = new DirectoryInfo(AppDataPath);

            if (AppDataDI.Exists == false) AppDataDI.Create();

            string[] MainPaths =
            {
                DBPath,
                SystemPath,
                LogPath,
                CachePath
            };

            string[] SubPaths =
            {
                Path.Combine(CachePath, "Doll"),
                Path.Combine(CachePath, "Doll", "SD"),
                Path.Combine(CachePath, "Doll", "SD", "Animation"),
                Path.Combine(CachePath, "Doll", "Normal_Crop"),
                Path.Combine(CachePath, "Doll", "Normal"),
                Path.Combine(CachePath, "Doll", "Skill"),
                Path.Combine(CachePath, "Equip"),
                Path.Combine(CachePath, "Equip", "Normal"),
                Path.Combine(CachePath, "Fairy"),
                Path.Combine(CachePath, "Fairy", "Normal"),
                Path.Combine(CachePath, "Fairy", "Normal_Crop"),
                Path.Combine(CachePath, "Fairy", "Skill"),
                Path.Combine(CachePath, "Enemy"),
                Path.Combine(CachePath, "Enemy", "SD"),
                Path.Combine(CachePath, "Enemy", "Normal_Crop"),
                Path.Combine(CachePath, "Enemy", "Normal"),
                Path.Combine(CachePath, "FST"),
                Path.Combine(CachePath, "FST", "SD"),
                Path.Combine(CachePath, "FST", "Normal_Crop"),
                Path.Combine(CachePath, "FST", "Normal"),
                Path.Combine(CachePath, "FST", "Skill"),
                Path.Combine(CachePath, "OldGFD"),
                Path.Combine(CachePath, "OldGFD", "Images"),
                Path.Combine(CachePath, "Event"),
                Path.Combine(CachePath, "Event", "Images"),
                Path.Combine(CachePath, "Voices")
            };

            foreach (string path in MainPaths) if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            foreach (string path in SubPaths) if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
        }

        internal static async Task<bool> LoadDB()
        {
            await Task.Delay(1);

            try
            {
                DollList.Clear();
                DollList.ReadXml(Path.Combine(DBPath, "Doll.gfs"));
                EquipmentList.Clear();
                EquipmentList.ReadXml(Path.Combine(DBPath, "Equipment.gfs"));
                FairyList.Clear();
                FairyList.ReadXml(Path.Combine(DBPath, "Fairy.gfs"));
                EnemyList.Clear();
                EnemyList.ReadXml(Path.Combine(DBPath, "Enemy.gfs"));
                FSTList.Clear();
                FSTList.ReadXml(Path.Combine(DBPath, "FST.gfs"));
                SkillTrainingList.Clear();
                SkillTrainingList.ReadXml(Path.Combine(DBPath, "SkillTraining.gfs"));
                MDSupportList.Clear();
                MDSupportList.ReadXml(Path.Combine(DBPath, "MDSupportList.gfs"));
                FreeOPList.Clear();
                FreeOPList.ReadXml(Path.Combine(DBPath, "FreeOP.gfs"));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static bool LoadDBSync()
        {
            try
            {
                DollList.Clear();
                DollList.ReadXml(Path.Combine(DBPath, "Doll.gfs"));
                EquipmentList.Clear();
                EquipmentList.ReadXml(Path.Combine(DBPath, "Equipment.gfs"));
                FairyList.Clear();
                FairyList.ReadXml(Path.Combine(DBPath, "Fairy.gfs"));
                EnemyList.Clear();
                EnemyList.ReadXml(Path.Combine(DBPath, "Enemy.gfs"));
                FSTList.Clear();
                FSTList.ReadXml(Path.Combine(DBPath, "FST.gfs"));
                SkillTrainingList.Clear();
                SkillTrainingList.ReadXml(Path.Combine(DBPath, "SkillTraining.gfs"));
                MDSupportList.Clear();
                MDSupportList.ReadXml(Path.Combine(DBPath, "MDSupportList.gfs"));
                FreeOPList.Clear();
                FreeOPList.ReadXml(Path.Combine(DBPath, "FreeOP.gfs"));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static async Task<bool> LoadDB(DataTable table, string DBFile, bool BeforeClear)
        {
            await Task.Delay(1);

            try
            {
                if (BeforeClear == true) table.Clear();
                table.ReadXml(Path.Combine(DBPath, DBFile));
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
                if (BeforeClear == true) table.Clear();
                table.ReadXml(Path.Combine(DBPath, DBFile));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static async Task<bool> CheckDBVersion()
        {
            if (IsServerDown == true) return false;

            string LocalDBVerPath = Path.Combine(SystemPath, "DBVer.txt");
            string ServerDBVerPath = Path.Combine(Server, "DBVer.txt");
            string TempDBVerPath = Path.Combine(tempPath, "DBVer.txt");

            bool HasDBUpdate = false;

            if (File.Exists(LocalDBVerPath) == false) HasDBUpdate = true;
            else
            {
                using (WebClient wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(ServerDBVerPath, TempDBVerPath);
                }

                using (StreamReader sr1 = new StreamReader(new FileStream(LocalDBVerPath, FileMode.Open, FileAccess.Read)))
                {
                    using (StreamReader sr2 = new StreamReader(new FileStream(TempDBVerPath, FileMode.Open, FileAccess.Read)))
                    {
                        int localVer = int.Parse(sr1.ReadToEnd());
                        int serverVer = int.Parse(sr2.ReadToEnd());

                        if (localVer < serverVer) HasDBUpdate = true;
                    }
                }
            }

            return HasDBUpdate;
        }

        internal static async Task<bool> CheckEventVersion()
        {
            if (IsServerDown == true) return false;

            string LocalEventVerPath = Path.Combine(CachePath, "Event", "EventVer.txt");
            string ServerEventVerPath = Path.Combine(Server, "EventVer.txt");
            string TempEventVerPath = Path.Combine(tempPath, "EventVer.txt");

            bool HasEventUpdate = false;

            if (File.Exists(LocalEventVerPath) == false) HasEventUpdate = true;
            else
            {
                using (WebClient wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(ServerEventVerPath, TempEventVerPath);
                }

                await Task.Delay(1);

                using (StreamReader sr1 = new StreamReader(new FileStream(LocalEventVerPath, FileMode.Open, FileAccess.Read)))
                {
                    using (StreamReader sr2 = new StreamReader(new FileStream(TempEventVerPath, FileMode.Open, FileAccess.Read)))
                    {
                        int localVer = int.Parse(sr1.ReadToEnd().Split(';')[1]);
                        int serverVer = int.Parse(sr2.ReadToEnd().Split(';')[1]);

                        if (localVer < serverVer) HasEventUpdate = true;
                    }
                }
            }

            return HasEventUpdate;
        }

        //ProgressDialog 교체
        internal static async Task UpdateDB(Activity activity, int TitleMsg = Resource.String.CheckDBUpdateDialog_Title, int MessageMgs = Resource.String.CheckDBUpdateDialog_Message)
        {
            string[] DBFiles = 
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

            ProgressDialog pd = new ProgressDialog(activity, DialogBG_Download);
            pd.SetProgressStyle(ProgressDialogStyle.Horizontal);
            pd.SetTitle(TitleMsg);
            pd.SetMessage(Resources.GetString(MessageMgs));
            pd.SetCancelable(false);
            pd.Max = 100;
            pd.Show();

            using (WebClient wc = new WebClient())
            {
                for (int i = 0; i < DBFiles.Length; ++i)
                {
                    string url = Path.Combine(Server, "Data", "DB", "Test", DBFiles[i]);
                    string target = Path.Combine(tempPath, DBFiles[i]);
                    pd.SecondaryProgress = Convert.ToInt32(((double)pd.Max / DBFiles.Length) * (i + 1));
                    await wc.DownloadFileTaskAsync(url, target);
                }

                string url2 = Path.Combine(Server, "DBVer.txt");
                string target2 = Path.Combine(tempPath, "DBVer.txt");
                await wc.DownloadFileTaskAsync(url2, target2);
                await Task.Delay(100);
            }

            for (int i = 0; i < DBFiles.Length; ++i)
            {
                string originalFile = Path.Combine(tempPath, DBFiles[i]);
                string targetFile = Path.Combine(DBPath, DBFiles[i]);
                File.Copy(originalFile, targetFile, true);
                pd.Progress = Convert.ToInt32(((double)pd.Max / DBFiles.Length) * (i + 1));
                await Task.Delay(100);
            }

            await Task.Delay(500);

            activity.RunOnUiThread(() => { pd.SetMessage(Resources.GetString(Resource.String.UpdateDBDialog_RefreshVersionMessage)); });

            string oldVersion = Path.Combine(SystemPath, "DBVer.txt");
            string newVersion = Path.Combine(tempPath, "DBVer.txt");
            File.Copy(newVersion, oldVersion, true);

            await Task.Delay(500);

            pd.Dismiss();
        }

        internal static void LogError(Activity activity, string error)
        {
            try
            {
                DateTime now = DateTime.Now;

                string nowDateTime = $"{now.Year}{now.Month}{now.Day} {now.Hour}{now.Minute}{now.Second}";
                string ErrorFileName = $"{nowDateTime}-ErrorLog.txt";

                DirectoryInfo di = new DirectoryInfo(LogPath);
                if (di.Exists == false) di.Create();

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(LogPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                    sw.Write(error);
            }
            catch (Exception)
            {
                activity.RunOnUiThread(() => { Toast.MakeText(activity, "Error Write Log", ToastLength.Long).Show(); });
            }
        }

        internal static void LogError(string error)
        {
            try
            {
                DateTime now = DateTime.Now;

                string nowDateTime = $"{now.Year}{now.Month}{now.Day} {now.Hour}{now.Minute}{now.Second}";
                string ErrorFileName = $"{nowDateTime}-ErrorLog.txt";

                DirectoryInfo di = new DirectoryInfo(LogPath);
                if (di.Exists == false) di.Create();

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(LogPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                    sw.Write(error);
            }
            catch (Exception)
            {
                
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
            View sbView = sb.View;
            sbView.SetBackgroundColor(color);
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
            View sbView = sb.View;
            sbView.SetBackgroundColor(color);
            sb.Show();
        }

        internal static string CalcTime(int minute)
        {
            if (minute != 0) return $"{minute / 60} : {(minute % 60).ToString("D2")}";
            else return Resources.GetString(Resource.String.Common_NonProduct);
        }

        internal static bool IsDBNullOrBlank(DataRow dr, string index)
        {
            if (dr[index] == DBNull.Value) return true;
            if (string.IsNullOrWhiteSpace((string)dr[index]) == true) return true;

            return false;
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

        internal class ADViewListener : Android.Gms.Ads.AdListener
        {
            public override void OnAdOpened()
            {
                return;
            }

            public override void OnAdLeftApplication()
            {
                return;
            }
        }
    }
}