using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GFD_W_Updater
{
    public partial class Updater : Form
    {
        static readonly string appVer = "0.0.1";
        static readonly string server = "http://chlwlsgur96.ipdisk.co.kr/publist/HDD1/Data/Project/GFS/";
        static readonly string currentPath = Environment.CurrentDirectory;
        static readonly string tempPath = Path.Combine(currentPath, "Updater_Temp");
        static readonly string extractPath = Path.Combine(tempPath, "extract");
        static readonly string logPath = Path.Combine(currentPath, "Updater_Log");
        static readonly string updateFileServerPath = Path.Combine(server, "Update", "GFD-W.zip");
        static readonly string target = Path.Combine(tempPath, Path.GetFileName(updateFileServerPath));

        public Updater()
        {
            InitializeComponent();

            Updater_VersionLabel.Text = $"Ver. {appVer}";

            _ = UpdateProcess();
        }

        private async Task UpdateProcess()
        {
            await Task.Delay(100);

            try
            {
                await Task.Delay(500);

                Updater_StatusLabel.Text = "Initializing";

                Updater_UpdateAnimationBox.Visible = true;

                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                if (!Directory.Exists(extractPath))
                    Directory.CreateDirectory(extractPath);

                Process[] plist = Process.GetProcessesByName("GFD-W");
                foreach (Process p in plist)
                    p.Kill();

                // Download update zip file

                Updater_StatusLabel.Text = "Download Update Zip File";

                await Task.Delay(500);

                using (WebClient wc = new WebClient())
                {
                    Updater_DownloadProgressBar.Style = ProgressBarStyle.Continuous;
                    wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                    {
                        Updater_DownloadProgressBar.Value = e.ProgressPercentage;
                        Updater_StatusLabel.Text = $"Download Update Zip File ({e.BytesReceived / 1024}KB / {e.TotalBytesToReceive / 1024} KB)";
                    };
                    await wc.DownloadFileTaskAsync(updateFileServerPath, target);
                }

                // Unzip update zip file

                Updater_StatusLabel.Text = "Unzipping Update File";
                Updater_DownloadProgressBar.Style = ProgressBarStyle.Marquee;

                await Task.Delay(500);

                await UnzipProcess(target, extractPath);

                // Copy update files

                Updater_StatusLabel.Text = "Updating Client";

                await Task.Delay(500);

                await CopyProcess(extractPath, currentPath);

                // Clear temp files
                
                Updater_StatusLabel.Text = "Finalizing Update Process";

                await Task.Delay(500);

                Directory.Delete(tempPath, true);

                Updater_StatusLabel.Text = "Starting GFD-W";

                await Task.Delay(1000);

                Process.Start(Path.Combine(currentPath, "GFD-W.exe"));

                await Task.Delay(500);

                Close();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private async Task UnzipProcess(string sourceZipFile, string targetDir)
        {
            await Task.Delay(100);

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(sourceZipFile))
                    await Task.Run(delegate { archive.ExtractToDirectory(targetDir); });
            }
            catch (Exception ex)
            {
                LogError(ex);   
            }
        }

        private async Task CopyProcess(string updateDir, string targetDir)
        {
            await Task.Delay(100);

            try
            {
                string[] updateFiles = Directory.GetFiles(updateDir);

                foreach (string s in updateFiles)
                    File.Copy(s, Path.GetFileName(s), true);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private static void LogError(Exception ex)
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
    }
}
