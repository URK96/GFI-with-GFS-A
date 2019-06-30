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
                StatusStrip_AppVerLabel.Text = $"Ver. {ETC.appVer}";
                StatusStrip_AppVerLabel.Text += ETC.isReleaseMode ? "R" : "D";
                StatusStrip_DBVerLabel.Text = $"DB Ver. {ETC.dbVer}";

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

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
