using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace GFD_W
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            _ = InitializeMain();
        }

        private async Task InitializeMain()
        {
            try
            {
                StatusStrip_AppVerLabel.Text = $"App Ver : {ETC.appVer}";
                StatusStrip_AppVerLabel.Text += ETC.isReleaseMode ? "R" : "D";
                StatusStrip_DBVerLabel.Text = $"DB Ver : {ETC.dbVer}";

                try
                {
                    using (StreamReader sr = new StreamReader(new FileStream(Path.Combine(ETC.systemPath, "OldGFDVer.txt"), FileMode.Open, FileAccess.Read)))
                        int.TryParse(sr.ReadToEnd(), out ETC.oldGFDVer);
                }
                catch { }

                StatusStrip_OldGFDVerLabel.Text = $"GFDv1 Ver : {ETC.oldGFDVer}";

                UpdateCheckTimer.Start();
                UpdateCheckTimer_Tick(UpdateCheckTimer, new EventArgs());

                await LoadNotification();

                CreateObject();

                await InitailizeTDollDic();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }

        private async Task LoadNotification()
        {
            try
            {
                using (WebClient wc = new WebClient())
                    MainNotification.Text = await wc.DownloadStringTaskAsync(Path.Combine(ETC.server, "Android_Notification.txt"));
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                MainNotification.Text = "Server Network Bad :(";
            }
        }

        private void CreateObject()
        {
            try
            {
                foreach (DataRow dr in ETC.dollList.Rows)
                    dollRootList.Add(new Doll(dr));
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }


        // Event Functions

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void GFDW정보ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GFDWInfo info = new GFDWInfo();
            info.ShowDialog();
        }

        private void Main_DiscordButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/acg983T");
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Main_NotificationRefreshButton_Click(object sender, EventArgs e)
        {
            _ = LoadNotification();
        }

        private async void UpdateCheckTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (await ETC.CheckAppVersion())
                    StatusStrip_AppVerLabel.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                else
                    StatusStrip_AppVerLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;

                if (await ETC.CheckDBVersion())
                    StatusStrip_DBVerLabel.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                else
                    StatusStrip_DBVerLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;

                if (await ETC.CheckOldGFDVersion())
                    StatusStrip_OldGFDVerLabel.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                else
                    StatusStrip_OldGFDVerLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }

        private void StatusStrip_AppVerLabel_Click(object sender, EventArgs e)
        {
            ToolStripStatusLabel label = sender as ToolStripStatusLabel;

            //if (label.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText)
                
        }
    }
}
