using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace GFD_W
{
    public partial class SplashForm : Form
    {
        public SplashForm()
        {
            InitializeComponent();
            _ = InitializeApp();
        }

        private async Task InitializeApp()
        {
            SplashProgressLabel.Text = "";

            for (int i = 0; i <= 50; ++i)
            {
                Opacity = i * 0.02;
                await Task.Delay(10);
            }

            SplashProgressLabel.Text = "Initialize App";

            BasicInitialize();

            SplashProgressLabel.Text = "Checking App Version";

            await Task.Delay(500);

            if (await ETC.CheckAppVersion())
                if (MessageBox.Show
                    (
                    "프로그램 업데이트가 있습니다. 업데이트를 진행하시겠습니까?",
                    "프로그램 업데이트 존재",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                    ) == DialogResult.Yes)
                    await ETC.UpdateProgram(SplashProgressLabel);

            SplashProgressLabel.Text = "Checking DB Version";

            await Task.Delay(500);

            if (await ETC.CheckDBVersion())
                await ETC.UpdateDB(SplashProgressLabel);

            await Task.Delay(500);
                
            SplashProgressLabel.Text = "Load DB";

            await ETC.LoadDB();
            await Task.Delay(500);

            SplashProgressLabel.Text = "Welcome to GFD-W";

            await Task.Delay(500);

            Main main = new Main();
            main.Show();

            Visible = false;
        }

        private void BasicInitialize()
        {
            try
            {
#if DEBUG
                ETC.isReleaseMode = false;
#endif
                CheckInitFolder();

                if (ETC.isReleaseMode == true)
                {
                    AppCenter.Start("62727ad7-8a10-4667-902b-5414f5abd730", typeof(Analytics), typeof(Crashes));
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }

        private static void CheckInitFolder()
        {
            if (Directory.Exists(ETC.tempPath) == false)
                Directory.CreateDirectory(ETC.tempPath);
            else
            {
                Directory.Delete(ETC.tempPath, true);
                Directory.CreateDirectory(ETC.tempPath);
            }

            DirectoryInfo AppDataDI = new DirectoryInfo(ETC.dataPath);

            if (AppDataDI.Exists == false)
                AppDataDI.Create();

            string[] MainPaths =
            {
                ETC.dbPath,
                ETC.systemPath,
                ETC.logPath,
                ETC.cachePath
            };

            string[] SubPaths =
            {
                Path.Combine(ETC.cachePath, "Doll"),
                Path.Combine(ETC.cachePath, "Doll", "SD"),
                Path.Combine(ETC.cachePath, "Doll", "SD", "Animation"),
                Path.Combine(ETC.cachePath, "Doll", "Normal_Crop"),
                Path.Combine(ETC.cachePath, "Doll", "Normal"),
                Path.Combine(ETC.cachePath, "Doll", "Skill"),
                Path.Combine(ETC.cachePath, "Equip"),
                Path.Combine(ETC.cachePath, "Equip", "Normal"),
                Path.Combine(ETC.cachePath, "Fairy"),
                Path.Combine(ETC.cachePath, "Fairy", "Normal"),
                Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop"),
                Path.Combine(ETC.cachePath, "Fairy", "Skill"),
                Path.Combine(ETC.cachePath, "Enemy"),
                Path.Combine(ETC.cachePath, "Enemy", "SD"),
                Path.Combine(ETC.cachePath, "Enemy", "Normal_Crop"),
                Path.Combine(ETC.cachePath, "Enemy", "Normal"),
                Path.Combine(ETC.cachePath, "FST"),
                Path.Combine(ETC.cachePath, "FST", "SD"),
                Path.Combine(ETC.cachePath, "FST", "Normal_Crop"),
                Path.Combine(ETC.cachePath, "FST", "Normal"),
                Path.Combine(ETC.cachePath, "FST", "Normal_Icon"),
                Path.Combine(ETC.cachePath, "FST", "Skill"),
                Path.Combine(ETC.cachePath, "OldGFD"),
                Path.Combine(ETC.cachePath, "OldGFD", "Images"),
                Path.Combine(ETC.cachePath, "Event"),
                Path.Combine(ETC.cachePath, "Event", "Images"),
                Path.Combine(ETC.cachePath, "Voices"),
                Path.Combine(ETC.cachePath, "Voices", "Doll"),
                Path.Combine(ETC.cachePath, "Voices", "Enemy"),
                Path.Combine(ETC.cachePath, "GuideBook", "PDFs")
            };

            foreach (string path in MainPaths)
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
            foreach (string path in SubPaths)
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
        }
    }
}
