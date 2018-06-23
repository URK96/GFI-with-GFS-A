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
        internal static string Server = "http://chlwlsgur96.ipdisk.co.kr:80/publist/HDD1/Data/Project/GFS/";
        internal static string SDCardPath = (string)Android.OS.Environment.ExternalStorageDirectory;
        internal static string tempPath = Path.Combine(SDCardPath, "GFDTemp");
        internal static string AppDataPath = Path.Combine(SDCardPath, "Android", "data", "GFD");
        internal static string DBPath = Path.Combine(AppDataPath, "DB");
        internal static string SystemPath = Path.Combine(AppDataPath, "System");
        internal static string CachePath = Path.Combine(AppDataPath, "Cache");
        internal static string LogPath = Path.Combine(SystemPath, "Log");

        internal static bool IsLowRAM = false;
        internal static bool UseLightTheme = false;
        internal static int DialogBG = 0;
        internal static int DialogBG_Vertical = 0;
        internal static int DialogBG_Download = 0;
        internal static bool HasEvent = false;

        internal static DataTable DollList = new DataTable();
        internal static DataTable EquipmentList = new DataTable();
        internal static DataTable FairyList = new DataTable();
        internal static DataTable SkillTrainingList = new DataTable();
        internal static DataTable MDSupportList = new DataTable();

        internal static ISharedPreferences sharedPreferences;

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
            DirectoryInfo AppDataDI = new DirectoryInfo(AppDataPath);

            if (Directory.Exists(tempPath) == false) Directory.CreateDirectory(tempPath);
            else
            {
                Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);
            }

            if (AppDataDI.Exists == false) AppDataDI.Create();
            if (Directory.Exists(DBPath) == false) Directory.CreateDirectory(DBPath);
            if (Directory.Exists(SystemPath) == false) Directory.CreateDirectory(SystemPath);
            if (Directory.Exists(LogPath) == false) Directory.CreateDirectory(LogPath);

            if (Directory.Exists(CachePath) == false) Directory.CreateDirectory(CachePath);
            if (Directory.Exists(Path.Combine(CachePath, "Doll")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Doll"));
            if (Directory.Exists(Path.Combine(CachePath, "Doll", "SD")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Doll", "SD"));
            if (Directory.Exists(Path.Combine(CachePath, "Doll", "Normal_Crop")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Doll", "Normal_Crop"));
            if (Directory.Exists(Path.Combine(CachePath, "Doll", "Normal")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Doll", "Normal"));
            if (Directory.Exists(Path.Combine(CachePath, "Doll", "Skill")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Doll", "Skill"));
            if (Directory.Exists(Path.Combine(CachePath, "Equip")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Equip"));
            if (Directory.Exists(Path.Combine(CachePath, "Equip", "Normal")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Equip", "Normal"));
            if (Directory.Exists(Path.Combine(CachePath, "Fairy")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Fairy"));
            if (Directory.Exists(Path.Combine(CachePath, "Fairy", "Normal")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Fairy", "Normal"));
            if (Directory.Exists(Path.Combine(CachePath, "Fairy", "Normal_Crop")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Fairy", "Normal_Crop"));
            if (Directory.Exists(Path.Combine(CachePath, "Fairy", "Skill")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Fairy", "Skill"));
            if (Directory.Exists(Path.Combine(CachePath, "OldGFD")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "OldGFD"));
            if (Directory.Exists(Path.Combine(CachePath, "OldGFD", "Images")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "OldGFD", "Images"));
            if (Directory.Exists(Path.Combine(CachePath, "Event")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Event"));
            if (Directory.Exists(Path.Combine(CachePath, "Event", "Images")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Event", "Images"));
            if (Directory.Exists(Path.Combine(CachePath, "Voices")) == false) Directory.CreateDirectory(Path.Combine(CachePath, "Voices"));
        }

        internal static async Task<bool> LoadDB()
        {
            await Task.Delay(100);

            try
            {
                DollList.ReadXml(Path.Combine(DBPath, "Doll.gfs"));
                EquipmentList.ReadXml(Path.Combine(DBPath, "Equipment.gfs"));
                FairyList.ReadXml(Path.Combine(DBPath, "Fairy.gfs"));
                SkillTrainingList.ReadXml(Path.Combine(DBPath, "SkillTraining.gfs"));
                MDSupportList.ReadXml(Path.Combine(DBPath, "MDSupportList.gfs"));
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
                using (WebClient wc = new WebClient())
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
            string ServerEventVerPath = Path.Combine(Server, "EventVerNew.txt");
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
                        int localVer = Convert.ToInt32((sr1.ReadToEnd()).Split(';')[1]);
                        int serverVer = Convert.ToInt32((sr2.ReadToEnd()).Split(';')[1]);

                        if (localVer < serverVer) HasEventUpdate = true;
                    }
                }
            }

            return HasEventUpdate;
        }

        internal static async Task CheckHasEvent()
        {
            string LocalEventVerPath = Path.Combine(CachePath, "Event", "EventVer.txt");

            await Task.Delay(1);

            using (StreamReader sr = new StreamReader(new FileStream(LocalEventVerPath, FileMode.Open, FileAccess.Read)))
            {
                string result = (sr.ReadToEnd()).Split(';')[0];

                switch (result)
                {
                    case "Y":
                        HasEvent = true;
                        break;
                    case "N":
                    default:
                        HasEvent = false;
                        break;
                }
            }
        }

        //ProgressDialog 교체
        internal static async Task UpdateDB(Activity activity)
        {
            string[] DBFiles = { "Doll.gfs", "MDSupportList.gfs", "FreeOP.gfs", "SkillTraining.gfs", "Equipment.gfs", "Fairy.gfs", "Enemy.gfs", "FairyAttribution.gfs" };

            ProgressDialog pd = new ProgressDialog(activity, DialogBG_Download);
            pd.SetProgressStyle(ProgressDialogStyle.Horizontal);
            pd.SetTitle("DB 업데이트");
            pd.SetMessage("DB 업데이트 중...");
            pd.SetCancelable(false);
            pd.Max = 100;
            pd.Show();

            using (WebClient wc = new WebClient())
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

            activity.RunOnUiThread(() => { pd.SetMessage("DB 버전 갱신 중..."); });

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
            pd.SetTitle("이벤트 업데이트");
            pd.SetMessage("이벤트 업데이트 중...");
            pd.SetCancelable(false);
            pd.Max = 100;
            pd.Show();

            using (WebClient wc = new WebClient())
            {
                string url = Path.Combine(Server, "EventVerNew.txt");
                string target = Path.Combine(tempPath, "EventVer.txt");
                await wc.DownloadFileTaskAsync(url, target);
                await Task.Delay(100);
            }

            int image_count = 0;

            using (StreamReader sr = new StreamReader(new FileStream(Path.Combine(tempPath, "EventVer.txt"), FileMode.Open, FileAccess.Read)))
            {
                image_count = Int32.Parse((sr.ReadToEnd()).Split(';')[2]);
            }

            pd.Max = image_count;

            using (WebClient wc2 = new WebClient())
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

            activity.RunOnUiThread(() => { pd.SetMessage("이벤트 버전 갱신 중..."); });

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

                string nowDateTime = now.Year.ToString() + now.Month.ToString() + now.Date.ToString() + now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString();
                string ErrorFileName = nowDateTime + "-ErrorLog.txt";

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(LogPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                {
                    sw.Write(error);
                } 
            }
            catch (Exception ex)
            {
                activity.RunOnUiThread(() => { Toast.MakeText(activity, "Error write log", ToastLength.Long).Show(); });
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
            return (minute / 60) + " : " + (minute % 60).ToString("D2");
        }

        internal class CountDown : Android.OS.CountDownTimer
        {
            CoordinatorLayout SnackbarLayout;

            public CountDown(long millisInFuture, long countDownInterval, CoordinatorLayout layout) : base(millisInFuture, countDownInterval)
            {
                SnackbarLayout = layout;
            }

            public override void OnFinish()
            {
                ShowSnackbar(SnackbarLayout, Resource.String.NetworkDelay, Snackbar.LengthLong, Android.Graphics.Color.DarkMagenta);
            }

            public override void OnTick(long millisUntilFinished)
            {
                
            }
        }
    }
}