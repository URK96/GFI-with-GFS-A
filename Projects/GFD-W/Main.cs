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
using System.Media;

namespace GFD_W
{
    public partial class Main : Form
    {
        SoundPlayer soundPlayer = new SoundPlayer();

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
                StatusStrip_OldGFDVerLabel.Text = $"GFDv1 Ver : {ETC.oldGFDVer}";

                UpdateCheckTimer.Start();
                UpdateCheckTimer_Tick(UpdateCheckTimer, new EventArgs());

                await LoadNotification();

                await CreateObject();

                await InitializeTDollDic();
                await InitializeEquipDic();
                await InitializeFairyDic();
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

        private async Task CreateObject()
        {
            await Task.Delay(10);

            try
            {
                dollRootList.Clear();
                equipRootList.Clear();
                fairyRootList.Clear();

                foreach (DataRow dr in ETC.dollList.Rows)
                    dollRootList.Add(new Doll(dr));

                foreach (DataRow dr in ETC.equipmentList.Rows)
                    equipRootList.Add(new Equip(dr));

                foreach (DataRow dr in ETC.fairyList.Rows)
                    fairyRootList.Add(new Fairy(dr));

                dollRootList.TrimExcess();
                equipRootList.TrimExcess();
                fairyRootList.TrimExcess();
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

                StatusStrip_DBVerLabel.Text = $"DB Ver : {ETC.dbVer}";
                StatusStrip_OldGFDVerLabel.Text = $"GFDv1 Ver : {ETC.oldGFDVer}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }

        private async void StatusStrip_AppVerLabel_Click(object sender, EventArgs e)
        {
            var label = sender as ToolStripStatusLabel;

            if (label.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText)
                if (MessageBox.Show("프로그램 업데이트가 확인되었습니다. 계속 진행할까요?", "프로그램 업데이트 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    UpdateCheckTimer.Stop();
                    GFDStatusLabel.Visible = true;

                    if (!await ETC.UpdateProgram(GFDStatusLabel))
                        GFDStatusLabel.Visible = false;
                }
        }

        private async void StatusStrip_DBVerLabel_Click(object sender, EventArgs e)
        {
            var label = sender as ToolStripStatusLabel;

            if (label.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText)
                if (MessageBox.Show("DB 업데이트가 확인되었습니다. 계속 진행할까요?", "DB 업데이트 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    UpdateCheckTimer.Stop();
                    Enabled = false;
                    GFDStatusLabel.Visible = true;

                    await ETC.UpdateDB(GFDStatusLabel);

                    GFDStatusLabel.Text = "Reload GFD...";

                    await ETC.CheckDBVersion();
                    await InitializeMain();

                    Enabled = true;
                    UpdateCheckTimer.Start();
                    GFDStatusLabel.Visible = false;
                }
        }

        private async void StatusStrip_OldGFDVerLabel_Click(object sender, EventArgs e)
        {
            var label = sender as ToolStripStatusLabel;

            /*if (label.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText)
                if (MessageBox.Show("소전사전v1 업데이트가 확인되었습니다. 계속 진행할까요?", "소전사전v1 업데이트 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    UpdateCheckTimer.Stop();
                    Enabled = false;
                    GFDStatusLabel.Visible = true;

                    await ETC.CheckOldGFDVersion();

                    GFDStatusLabel.Text = "Reload GFD...";

                    await ETC.CheckDBVersion();
                    await InitializeMain();

                    Enabled = true;
                    UpdateCheckTimer.Start();
                    GFDStatusLabel.Visible = false;
                }*/
        }
    }
}
