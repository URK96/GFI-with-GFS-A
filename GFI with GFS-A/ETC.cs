using Android.App;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    internal static class ETC
    {
        internal static string Server = "http://192.168.0.10/publist/HDD1/Data/Project/GFS/";
        internal static string SDCardPath = (string)Android.OS.Environment.ExternalStorageDirectory;
        internal static string tempPath = Path.Combine(SDCardPath, "GFDTemp");
        internal static string AppDataPath = Path.Combine(SDCardPath, "Android", "data", "GFD");
        internal static string DBPath = Path.Combine(AppDataPath, "DB");
        internal static string SystemPath = Path.Combine(AppDataPath, "System");
        internal static string CachePath = Path.Combine(AppDataPath, "Cache");
        internal static string LogPath = Path.Combine(SystemPath, "Log");

        internal static bool EnableDynamicDB = false;
        internal static bool HasInitDollAvgAbility = false;
        internal static bool IsLowRAM = false;
        internal static bool UseLightTheme = false;
        internal static int DialogBG = 0;
        internal static int DialogBG_Vertical = 0;
        internal static int DialogBG_Download = 0;
        internal static bool HasEvent = false;
        internal static bool ServerStatusError = false;

        internal static DataTable DollList = new DataTable();
        internal static DataTable EquipmentList = new DataTable();
        internal static DataTable FairyList = new DataTable();
        internal static DataTable EnemyList = new DataTable();
        internal static DataTable FSTList = new DataTable();
        internal static DataTable SkillTrainingList = new DataTable();
        internal static DataTable MDSupportList = new DataTable();

        internal static AverageAbility[] Avg_List;

        internal static Android.Content.Res.Resources Resources;

        internal static ISharedPreferences sharedPreferences;

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
            int[,] total = new int[TypeCount, AbilityCount];

            for (int i = 0; i < TypeCount; ++i)
            {
                for (int j = 0; j < AbilityCount; ++j) total[i, j] = 0;
            }

            for (int i = 0; i < DollList.Rows.Count; ++i)
            {
                DataRow dr = DollList.Rows[i];

                string type = (string)dr["Type"];
                int index = 0;
                
                switch (type)
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
                {
                    int value = int.Parse((((string)dr[AbilityList[j]]).Split(';')[0].Split('/'))[1]);
                    total[index, j] += value;
                }

                if (type == "SG")
                {
                    int value = int.Parse((((string)dr["Armor"]).Split('/'))[1]);
                    total[index, 5] += value;
                }
            }

            for (int i = 0; i < TypeCount; ++i)
            {
                for (int j = 0; j < AbilityCount; ++j)
                {
                    int value = Convert.ToInt32(Math.Round((double)total[i, j] / count[i]));

                    switch (j)
                    {
                        case 0:
                            Avg_List[i].HP = value;
                            break;
                        case 1:
                            Avg_List[i].FR = value;
                            break;
                        case 2:
                            Avg_List[i].EV = value;
                            break;
                        case 3:
                            Avg_List[i].AC = value;
                            break;
                        case 4:
                            Avg_List[i].AS = value;
                            break;
                        case 5:
                            Avg_List[i].AM = value;
                            break;
                    }
                }
            }

            HasInitDollAvgAbility = true;
        }

        internal static async Task UpViewAlpha(View view, int rate, int delay)
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
        }

        internal static DataRow FindDataRow<T>(DataTable table, string index, T value)
        {
            for (int i = 0; i < table.Rows.Count; ++i)
            {
                DataRow dr = table.Rows[i];
                if (((T)dr[index]).Equals(value)) return dr;
            }

            return null;
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
            string LocalDBVerPath = Path.Combine(SystemPath, "DBVer.txt");
            string ServerDBVerPath = Path.Combine(Server, "DBVer.txt");
            string TempDBVerPath = Path.Combine(tempPath, "DBVer.txt");

            bool HasDBUpdate = false;

            if (File.Exists(LocalDBVerPath) == false) HasDBUpdate = true;
            else
            {
                using (TimeOutWebClient wc = new TimeOutWebClient())
                {
                    await wc.DownloadFileTaskAsync(ServerDBVerPath, TempDBVerPath);
                }

                await Task.Delay(1);

                using (StreamReader sr1 = new StreamReader(new FileStream(LocalDBVerPath, FileMode.Open, FileAccess.Read)))
                {
                    using (StreamReader sr2 = new StreamReader(new FileStream(TempDBVerPath, FileMode.Open, FileAccess.Read)))
                    {
                        int localVer = Convert.ToInt32(sr1.ReadToEnd());
                        int serverVer = Convert.ToInt32(sr2.ReadToEnd());

                        if (localVer < serverVer) HasDBUpdate = true;
                    }
                }
            }

            return HasDBUpdate;
        }

        internal static async Task<bool> CheckEventVersion()
        {
            string LocalEventVerPath = Path.Combine(CachePath, "Event", "EventVer.txt");
            string ServerEventVerPath = Path.Combine(Server, "EventVer.txt");
            string TempEventVerPath = Path.Combine(tempPath, "EventVer.txt");

            bool HasEventUpdate = false;

            if (File.Exists(LocalEventVerPath) == false) HasEventUpdate = true;
            else
            {
                using (TimeOutWebClient wc = new TimeOutWebClient())
                {
                    await wc.DownloadFileTaskAsync(ServerEventVerPath, TempEventVerPath);
                }

                await Task.Delay(1);

                using (StreamReader sr1 = new StreamReader(new FileStream(LocalEventVerPath, FileMode.Open, FileAccess.Read)))
                {
                    using (StreamReader sr2 = new StreamReader(new FileStream(TempEventVerPath, FileMode.Open, FileAccess.Read)))
                    {
                        int localVer = Convert.ToInt32((sr1.ReadToEnd()).Split(';')[1]);
                        int serverVer = Convert.ToInt32((sr2.ReadToEnd()).Split(';')[1]);

                        if (localVer < serverVer) HasEventUpdate = true;
                    }
                }
            }

            return HasEventUpdate;
        }

        //ProgressDialog 교체
        internal static async Task UpdateDB(Activity activity)
        {
            if (ServerStatusError == true) return;

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
            pd.SetTitle(Resource.String.CheckDBUpdateDialog_Title);
            pd.SetMessage(Resources.GetString(Resource.String.UpdateDBDialog_Message));
            pd.SetCancelable(false);
            pd.Max = 100;
            pd.Show();

            using (TimeOutWebClient wc = new TimeOutWebClient())
            {
                for (int i = 0; i < DBFiles.Length; ++i)
                {
                    string url = Path.Combine(Server, "Data", "DB", DBFiles[i]);
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

            await Task.Delay(1000);

            pd.Dismiss();
        }

        //ProgressDialog 교체
        internal static async Task UpdateEvent(Activity activity)
        {
            ProgressDialog pd = new ProgressDialog(activity, DialogBG_Download);
            pd.SetProgressStyle(ProgressDialogStyle.Horizontal);
            pd.SetTitle(Resource.String.UpdateEventDialog_Title);
            pd.SetMessage(Resources.GetString(Resource.String.UpdateEventDialog_Message));
            pd.SetCancelable(false);
            pd.Max = 100;
            pd.Show();

            using (TimeOutWebClient wc = new TimeOutWebClient())
            {
                string url = Path.Combine(Server, "EventVer.txt");
                string target = Path.Combine(tempPath, "EventVer.txt");
                await wc.DownloadFileTaskAsync(url, target);
                await Task.Delay(100);
            }

            int image_count = 0;

            using (StreamReader sr = new StreamReader(new FileStream(Path.Combine(tempPath, "EventVer.txt"), FileMode.Open, FileAccess.Read)))
            {
                image_count = int.Parse((sr.ReadToEnd()).Split(';')[2]);
            }

            pd.Max = image_count;

            using (TimeOutWebClient wc2 = new TimeOutWebClient())
            {
                for (int i = 1; i <= image_count; ++i)
                {
                    string url2 = Path.Combine(Server, "Data", "Images", "Events", "Event_" + i + ".png");
                    string target2 = Path.Combine(CachePath, "Event", "Images", "Event_" + i + ".png");
                    await wc2.DownloadFileTaskAsync(url2, target2);
                    pd.Progress = i;
                    await Task.Delay(100);
                }
            }

            await Task.Delay(500);

            activity.RunOnUiThread(() => { pd.SetMessage(Resources.GetString(Resource.String.UpdateEventDialog_RefreshVersionMessage)); });

            string oldVersion = Path.Combine(CachePath, "Event", "EventVer.txt");
            string newVersion = Path.Combine(tempPath, "EventVer.txt");
            File.Copy(newVersion, oldVersion, true);

            await Task.Delay(1000);

            pd.Dismiss();
        }

        internal static void LogError(Activity activity, string error)
        {
            try
            {
                DateTime now = DateTime.Now;

                string nowDateTime = now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString();
                string ErrorFileName = string.Format("{0}-ErrorLog.txt", nowDateTime);

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(LogPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                {
                    sw.Write(error);
                } 
            }
            catch (Exception ex)
            {
                activity.RunOnUiThread(() => { Toast.MakeText(activity, "Error Write Log", ToastLength.Long).Show(); });
            }
        }

        internal static void LogError(string error)
        {
            try
            {
                DateTime now = DateTime.Now;

                string nowDateTime = now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString();
                string ErrorFileName = string.Format("{0}-ErrorLog.txt", nowDateTime);

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(LogPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                {
                    sw.Write(error);
                }
            }
            catch (Exception ex)
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
            return string.Format("{0} : {1}", (minute / 60), (minute % 60).ToString("D2"));
        }

        internal static void CheckServerStatus()
        {
            WebRequest request = WebRequest.Create(Server);
            request.Timeout = 5000;
            
            try
            {
                request.GetResponse();
            }
            catch (Exception)
            {
                ServerStatusError = true;
                return;
            }

            ServerStatusError = false;
        }

        internal static async Task CheckServerStatusAsync()
        {
            WebRequest request = WebRequest.Create(Server);
            request.Timeout = 5000;
            WebResponse response = null;

            try
            {
                response = request.GetResponse();
            }
            catch (Exception)
            {
                ServerStatusError = true;
                response.Close();
                response.Dispose();
                return;
            }

            ServerStatusError = false;
            response.Close();
            response.Dispose();
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

    public class TimeOutWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            request.Timeout = 3000;
            return request;
        }
    }
}