using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GFD_W
{
    internal static class ETC
    {
        internal static readonly string appVer = "0.0.1";
        internal static readonly string server = "http://chlwlsgur96.ipdisk.co.kr/publist/HDD1/Data/Project/GFS/";
        internal static readonly string currentPath = Environment.CurrentDirectory;
        internal static readonly string tempPath = Path.Combine(currentPath, "GFD_Temp");
        internal static readonly string dataPath = Path.Combine(currentPath, "Data");
        internal static readonly string dbPath = Path.Combine(dataPath, "DB");
        internal static readonly string systemPath = Path.Combine(dataPath, "System");
        internal static readonly string logPath = Path.Combine(systemPath, "Log");
        internal static readonly string cachePath = Path.Combine(dataPath, "Cache");
        internal static readonly string settingFile = Path.Combine(currentPath, "GFD-W.exe.config");

        internal static bool isReleaseMode = true;
        internal static bool isServerDown = false;

        internal static int dbVer = 0;

        internal static DataTable dollList = new DataTable();
        internal static DataTable equipmentList = new DataTable();
        internal static DataTable fairyList = new DataTable();
        internal static DataTable enemyList = new DataTable();
        internal static DataTable fstList = new DataTable();
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

        internal static AverageAbility[] Avg_List;

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
                for (int j = 0; j < AbilityCount; ++j)
                    total[i, j] = 0;

            for (int i = 0; i < dollList.Rows.Count; ++i)
            {
                Doll doll = new Doll(dollList.Rows[i]);
                DollAbilitySet DAS = new DollAbilitySet(doll.Type);
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

                int grow_value = int.Parse(doll.Abilities["Grow"].Split(';')[0]);

                for (int j = 0; j < AbilityList.Length; ++j)
                {
                    int ability_value = int.Parse(doll.Abilities[AbilityList[j]].Split(';')[0]);

                    total[index, j] += DAS.CalcAbility(AbilityList[j], ability_value, grow_value, 100, 100, false);
                }

                if (doll.Type == "SG")
                {
                    int ability_value = int.Parse(doll.Abilities["Armor"].Split(';')[0]);

                    total[index, 5] += DAS.CalcAbility("Armor", ability_value, grow_value, 100, 100, false);
                }
            }

            for (int i = 0; i < TypeCount; ++i)
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

        internal static string CalcTime(int minute)
        {
            if (minute != 0)
                return $"{minute / 60} : {(minute % 60).ToString("D2")}";
            else
                return "None";
        }

        internal static bool IsDBNullOrBlank(DataRow dr, string index)
        {
            if (dr[index] == DBNull.Value)
                return true;
            if (string.IsNullOrWhiteSpace((string)dr[index]) == true)
                return true;

            return false;
        }

        internal static void LogError(Exception ex)
        {
            try
            {
                DateTime now = DateTime.Now;

                string nowDateTime = $"{now.Year}{now.Month}{now.Day} {now.Hour}{now.Minute}{now.Second}";
                string ErrorFileName = $"{nowDateTime}-ErrorLog.txt";

                DirectoryInfo di = new DirectoryInfo(logPath);
                if (di.Exists == false)
                    di.Create();

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(logPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                    sw.Write(ex.ToString());
            }
            catch (Exception)
            {

            }
        }

        internal static void ShowErrorMessage(string title = "오류", string message = "오류 발생")
        {
            MessageBox.Show(title, message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal static async Task<bool> CheckDBVersion()
        {
            if (isServerDown == true)
                return false;

            string localDBVerPath = Path.Combine(systemPath, "DBVer.txt");
            string serverDBVerPath = Path.Combine(server, "DBVer.txt");
            string tempDBVerPath = Path.Combine(tempPath, "DBVer.txt");

            bool hasDBUpdate = false;

            if (File.Exists(localDBVerPath) == false)
                hasDBUpdate = true;
            else
            {
                using (WebClient wc = new WebClient())
                    await wc.DownloadFileTaskAsync(serverDBVerPath, tempDBVerPath);

                using (StreamReader sr1 = new StreamReader(new FileStream(localDBVerPath, FileMode.Open, FileAccess.Read)))
                using (StreamReader sr2 = new StreamReader(new FileStream(tempDBVerPath, FileMode.Open, FileAccess.Read)))
                {
                    int localVer = int.Parse(sr1.ReadToEnd());
                    int serverVer = int.Parse(sr2.ReadToEnd());

                    dbVer = localVer;

                    if (localVer < serverVer)
                        hasDBUpdate = true;
                }
            }

            return hasDBUpdate;
        }

        internal static async Task UpdateDB(Label status)
        {
            await Task.Delay(100);

            try
            {
                if (status == null)
                    status = new Label();

                using (WebClient wc = new WebClient())
                {
                    int now = 0;
                    int total = dbFiles.Length;

                    wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                    {
                        status.Text = $"DB Update Process - Update DB...{e.ProgressPercentage}% ({now}/{total})";
                    };
                    wc.DownloadFileCompleted += async (object sender, AsyncCompletedEventArgs e) =>
                    {
                        await Task.Delay(500);
                        status.Text = $"DB Update Process - Finalizing Update DB";
                    };

                    for (now = 0; now < dbFiles.Length; ++now)
                        await wc.DownloadFileTaskAsync(Path.Combine(server, "Data", "DB", dbFiles[now]), Path.Combine(tempPath, dbFiles[now]));

                    await wc.DownloadFileTaskAsync(Path.Combine(server, "DBVer.txt"), Path.Combine(tempPath, "DBVer.txt"));

                    await Task.Delay(100);
                }

                for (int i = 0; i < dbFiles.Length; ++i)
                {
                    File.Copy(Path.Combine(tempPath, dbFiles[i]), Path.Combine(dbPath, dbFiles[i]), true);
                    await Task.Delay(100);
                }

                await Task.Delay(500);

                string oldVersion = Path.Combine(systemPath, "DBVer.txt");
                string newVersion = Path.Combine(tempPath, "DBVer.txt");
                File.Copy(newVersion, oldVersion, true);

                using (StreamReader sr = new StreamReader(new FileStream(oldVersion, FileMode.Open, FileAccess.Read)))
                    int.TryParse(sr.ReadToEnd(), out dbVer);

                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        internal static async Task<bool> CheckAppVersion()
        {
            if (isServerDown == true)
                return false;

            try
            {
                string serverAppVerPath = Path.Combine(server, "GFDWVer.txt");
                string tempAppVerPath = Path.Combine(tempPath, "GFDWVer.txt");

                using (WebClient wc = new WebClient())
                    await wc.DownloadFileTaskAsync(serverAppVerPath, tempAppVerPath);

                using (StreamReader sr = new StreamReader(new FileStream(tempAppVerPath, FileMode.Open, FileAccess.Read)))
                {
                    string[] localVer = appVer.Split('.');
                    string[] serverVer = sr.ReadToEnd().Split('.');

                    for (int i = 0; i < localVer.Length; ++i)
                        if (int.Parse(localVer[i]) < int.Parse(serverVer[i]))
                            return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }

            return false;
        }

        internal static bool CreateSetting()
        {
            Configuration config;
            KeyValueConfigurationCollection configCollection;

            try
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configCollection = config.AppSettings.Settings;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }

            return true;
        }

        internal static string GetSettingValue(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
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
                    if (response.StatusCode == HttpStatusCode.OK)
                        isServerDown = false;
                    else
                        isServerDown = true;

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

        internal static async Task AppUpdate(Label statusLabel = null)
        {
            if (statusLabel == null)
                statusLabel = new Label();

            using (WebClient wc = new WebClient())
            {
                statusLabel.Text = "App Update Process - Check Server Network";

                await CheckServerNetwork();

                if (!isServerDown)
                {
                    try
                    {
                        statusLabel.Text = "App Update Process - Download Updater";

                        wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                        {
                            statusLabel.Text = $"App Update Process - Download Updater - {e.ProgressPercentage}%";
                        };

                        await wc.DownloadFileTaskAsync(Path.Combine(server, "GFDW Updater.exe"), Path.Combine(currentPath, "Updater.exe"));

                        await Task.Delay(500);

                        statusLabel.Text = "App Update Process - Run Updater";

                        await Task.Delay(500);

                        if (File.Exists("Updater.exe"))
                            Process.Start("Updater.exe", "Client").WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);

                        for (int i = 3; i >= 0; --i)
                        {
                            statusLabel.Text = $"Download App Updater Fail!...Skip after {i}s";
                            await Task.Delay(1000);
                        }
                    }
                }
            }
        }

        internal static async Task<bool> LoadDB()
        {
            await Task.Delay(1);

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
                skillTrainingList.Clear();
                skillTrainingList.ReadXml(Path.Combine(dbPath, "SkillTraining.gfs"));
                mdSupportList.Clear();
                mdSupportList.ReadXml(Path.Combine(dbPath, "MDSupportList.gfs"));
                freeOPList.Clear();
                freeOPList.ReadXml(Path.Combine(dbPath, "FreeOP.gfs"));
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }

            return true;
        }

        internal class Sorter : IComparer
        {
            internal int ColumnIndex = 0;
            internal SortOrder Order = SortOrder.None;

            public int Compare(object x, object y)
            {
                if ((!(x is ListViewItem)) || (!(y is ListViewItem)))
                    return 0;

                ListViewItem lvi1 = (ListViewItem)x;
                ListViewItem lvi2 = (ListViewItem)y;

                if ((lvi1.ListView.Columns[ColumnIndex].Tag == null) || ((string)lvi1.ListView.Columns[ColumnIndex].Tag == ""))
                    lvi1.ListView.Columns[ColumnIndex].Tag = "Text";

                string str1 = lvi1.SubItems[ColumnIndex].Text;
                string str2 = lvi2.SubItems[ColumnIndex].Text;

                switch ((string)lvi1.ListView.Columns[ColumnIndex].Tag)
                {
                    case "Numeric":
                        float f1 = float.Parse(str1);
                        float f2 = float.Parse(str2);

                        if (Order == SortOrder.Ascending)
                            return f1.CompareTo(f2);
                        else
                            return f2.CompareTo(f1);
                    case "Time":
                        int t_hour1 = int.Parse(str1.Split(':')[0]);
                        int t_hour2 = int.Parse(str2.Split(':')[0]);
                        int t_minute1 = int.Parse(str1.Split(':')[1]);
                        int t_minute2 = int.Parse(str2.Split(':')[1]);

                        int t1 = (t_hour1 * 60) + t_minute1;
                        int t2 = (t_hour2 * 60) + t_minute2;

                        if (Order == SortOrder.Ascending)
                            return t1.CompareTo(t2);
                        else
                            return t2.CompareTo(t1);
                    case "MDLocation":
                        string t_s1 = str1.Split('-')[0];
                        string t_s2 = str2.Split('-')[0];

                        int p1 = int.Parse(t_s1);
                        int p2 = int.Parse(t_s2);

                        if (Order == SortOrder.Ascending)
                            return p1.CompareTo(p2);
                        else
                            return p2.CompareTo(p1);
                    default:
                        if (Order == SortOrder.Ascending)
                            return str1.CompareTo(str2);
                        else
                            return str2.CompareTo(str1);
                }
            }
        }
    }
}
