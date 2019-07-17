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
                string skillName = isMod ? doll.ModSkillName : doll.SkillName;
                string url = Path.Combine(ETC.server, "Data", "Images", "SkillIcons", $"{skillName}.png");
                string target = Path.Combine(ETC.cachePath, "Doll", "Skill", $"{skillName}.gfdcache");

                try
                {
                    if ((File.Exists(target) == false) || (isRefresh == true))
                        using (WebClient wc = new WebClient())
                            await wc.DownloadFileTaskAsync(url, target);

                    SkillViewer_SkillIconBox.ImageLocation = target;
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex);
                }

                SkillViewer_SkillName.Text = skillName;
                SkillViewer_SkillExplain.Text = isMod ? doll.ModSkillExplain : doll.SkillExplain;

                LoadSkillEffect(isMod);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("스킬 정보 불러오기 오류", "인형 스킬 정보를 불러오는 동안 오류가 발생했습니다.");
            }
        }

        private void LoadSkillEffect(bool isMod)
        {
            int hSize = 30;

            try
            {
                string[] skillEffect = isMod ? doll.ModSkillEffect : doll.SkillEffect;
                string[] skillMags = isMod ? doll.ModSkillMag : doll.SkillMag;

                SkillViewer_InitialCoolTime.Text = skillMags[0];
                SkillViewer_CoolTime.Text = skillMags[1];

                if (SkillViewer_SkillEffectTable.RowCount < skillEffect.Length - 2)
                {
                    SkillViewer_SkillEffectTable.Height += hSize * (skillEffect.Length - 4);
                    Height += hSize * (skillEffect.Length - 4);
                    SkillViewer_SkillEffectTable.RowCount = skillEffect.Length - 2;
                }

                for (int i = 2; i < skillEffect.Length; ++i)
                {
                    Label effect = new Label();
                    effect.Dock = DockStyle.Fill;
                    effect.TextAlign = ContentAlignment.MiddleCenter;
                    effect.Text = skillEffect[i];
                    effect.Font = new Font(effect.Font.FontFamily, effect.Font.Size, FontStyle.Bold);

                    Label mag = new Label();
                    mag.Dock = DockStyle.Fill;
                    mag.TextAlign = ContentAlignment.MiddleCenter;
                    mag.Text = skillMags[i];

                    SkillViewer_SkillEffectTable.Controls.Add(effect, 0, i - 2);
                    SkillViewer_SkillEffectTable.Controls.Add(mag, 1, i - 2);

                    if (i >= 4)
                        SkillViewer_SkillEffectTable.RowStyles.Add(new RowStyle(SizeType.Absolute));
                }

                for (int i = 0; i < SkillViewer_SkillEffectTable.RowCount; ++i)
                {
                    SkillViewer_SkillEffectTable.RowStyles[i].SizeType = SizeType.Absolute;
                    SkillViewer_SkillEffectTable.RowStyles[i].Height = 30;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("스킬 정보 불러오기 오류", "인형 스킬 세부 효과 정보를 불러오는 동안 오류가 발생했습니다.");
            }
        }
    }
}
