using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace GFD_W
{
    public partial class SkillViewer : Form
    {
        enum Type { Doll, Fairy };

        Doll doll;
        Fairy fairy;

        public SkillViewer(Doll doll, bool isMod)
        {
            InitializeComponent();

            this.doll = doll;

            _ = LoadTDollSkill(false, isMod);
        }

        public SkillViewer(Fairy fairy)
        {
            InitializeComponent();

            this.fairy = fairy;
        }

        private async Task LoadTDollSkill(bool isRefresh, bool isMod)
        {
            try
            {
                if (isMod)
                {
                    try
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "SkillIcons", $"{doll.ModSkillName}.png");
                        string target = Path.Combine(ETC.cachePath, "Doll", "Skill", $"{doll.ModSkillName}.gfdcache");

                        if ((File.Exists(target) == false) || (isRefresh == true))
                            using (WebClient wc = new WebClient())
                                await wc.DownloadFileTaskAsync(url, target);

                        SkillViewer_SkillIconBox.ImageLocation = target;
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex);
                    }

                    SkillViewer_SkillName.Text = doll.ModSkillName;
                    SkillViewer_SkillExplain.Text = doll.ModSkillExplain;
                }
                else
                {
                    try
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "SkillIcons", $"{doll.SkillName}.png");
                        string target = Path.Combine(ETC.cachePath, "Doll", "Skill", $"{doll.SkillName}.gfdcache");

                        if ((File.Exists(target) == false) || (isRefresh == true))
                            using (WebClient wc = new WebClient())
                                await wc.DownloadFileTaskAsync(url, target);

                        SkillViewer_SkillIconBox.ImageLocation = target;
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex);
                    }

                    SkillViewer_SkillName.Text = doll.SkillName;
                    SkillViewer_SkillExplain.Text = doll.SkillExplain;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("스킬 정보 불러오기 오류", "인형 스킬 정보를 불러오는 동안 오류가 발생했습니다.");
            }
        }
    }
}
