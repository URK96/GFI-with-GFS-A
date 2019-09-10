using GFD_W.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;

namespace GFD_W
{
    partial class Main
    {
        List<Doll> dollRootList = new List<Doll>();
        List<string> dollVoiceList = new List<string>();

        Doll doll;

        DollAbilitySet das;

        bool isDollLoad = false;
        // Type, Voice, Mod, Grade
        bool[] applyDollFilters = { false, false, false, false };

        int modIndex = 0;
        int abilityLv = 1;
        int abilityFavor = 0;
        int[] abilityValues = new int[6];
        int vCostumeIndex = 0;

        private async Task InitializeTDollDic()
        {
            await Task.Delay(1);

            try
            {
                TDollDic_SplitContainer.Panel1.Hide();

                for (int i = 0; i < TDollDic_TDollInfo_FormationBuffTable.RowCount; ++i)
                    for (int k = 0; k < TDollDic_TDollInfo_FormationBuffTable.ColumnCount; ++k)
                    {
                        PictureBox pb = new PictureBox();
                        pb.Dock = DockStyle.Fill;

                        TDollDic_TDollInfo_FormationBuffTable.Controls.Add(pb);
                    }

                _ = ListDoll(null);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("도감 초기화 오류", "인형 도감 초기화 중 오류가 발생했습니다.");
            }
        }

        private async Task ListDoll(string searchName)
        {
            await Task.Delay(10);

            try
            {
                TDollDic_TDollListView.Items.Clear();

                CheckApplyDollFilter();

                foreach (Doll doll in dollRootList)
                {
                    if ((searchName != null) && !doll.Name.ToUpper().Contains(searchName.ToUpper()))
                        continue;

                    if (!CheckFilter(doll))
                        continue;

                    string grade = "";

                    if (doll.Grade == 0)
                        grade = "★";
                    else
                        grade = $"☆{doll.Grade}";

                    TDollDic_TDollListView.Items.Add(new ListViewItem(new string[]
                    {
                        doll.DicNumber.ToString(),
                        doll.Type,
                        grade,
                        doll.Name,
                        doll.HasMod ? "O" : "X"
                    }));
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("리스트 오류", "인형 리스트를 생성하는 중 오류가 발생했습니다.");
            }
        }

        private void CheckApplyDollFilter()
        {
            for (int i = 0; i < applyDollFilters.Length; ++i)
                applyDollFilters[i] = false;

            var typeCollection = TDollDic_TDollFilterGroup_TypeGroup.Controls;
            var voiceCollection = TDollDic_TDollFilterGroup_VoiceGroup.Controls;
            var modCollection = TDollDic_TDollFilterGroup_ModGroup.Controls;
            var gradeCollection = TDollDic_TDollFilterGroup_GradeGroup.Controls;

            foreach (CheckBox c in typeCollection)
                if (c.Checked)
                    applyDollFilters[0] = true;

            foreach (CheckBox c in voiceCollection)
                if (c.Checked)
                    applyDollFilters[1] = true;

            foreach (CheckBox c in modCollection)
                if (c.Checked)
                    applyDollFilters[2] = true;

            foreach (CheckBox c in gradeCollection)
                if (c.Checked)
                    applyDollFilters[3] = true;
        }

        private bool CheckFilter(Doll doll)
        {
            var typeCollection = TDollDic_TDollFilterGroup_TypeGroup.Controls;
            var voiceCollection = TDollDic_TDollFilterGroup_VoiceGroup.Controls;
            var modCollection = TDollDic_TDollFilterGroup_ModGroup.Controls;
            var gradeCollection = TDollDic_TDollFilterGroup_GradeGroup.Controls;

            if (applyDollFilters[0])
                foreach (CheckBox c in typeCollection)
                    if ((doll.Type == c.Text) && !c.Checked)
                        return false;

            if (applyDollFilters[1])
                foreach (CheckBox c in voiceCollection)
                    if ((doll.HasVoice && (int.Parse(c.Tag.ToString()) == 1)) || (!doll.HasVoice && (int.Parse(c.Tag.ToString()) == 0)))
                        if (!c.Checked)
                            return false;

            if (applyDollFilters[2])
                foreach (CheckBox c in modCollection)
                    if ((doll.HasMod && (int.Parse(c.Tag.ToString()) == 1)) || (!doll.HasMod && (int.Parse(c.Tag.ToString()) == 0)))
                        if (!c.Checked)
                            return false;

            if (applyDollFilters[3])
                foreach (CheckBox c in gradeCollection)
                    if ((doll.Grade == int.Parse(c.Tag.ToString())) && !c.Checked)
                        return false;

            return true;
        }

        private async Task LoadTDollInfo(bool isRefresh = false)
        {
            await Task.Delay(10);
            isDollLoad = true;

            try
            {
                TDollDic_SplitContainer.Panel1.Show();
                TDollDic_TDollInfo_FullImageLoadProcessBar.Visible = true;
                TDollDic_TDollInfo_FullImageLoadProcessBar.Style = ProgressBarStyle.Marquee;

                if (doll.HasMod)
                {
                    TDollDic_TDollInfo_ModStoryButton.Enabled = true;
                    TDollDic_TDollInfo_ModSkillButton.Enabled = true;
                }
                else
                {
                    TDollDic_TDollInfo_ModStoryButton.Enabled = false;
                    TDollDic_TDollInfo_ModSkillButton.Enabled = false;
                }

                das = new DollAbilitySet(doll.Type);

                if (doll.HasMod)
                {
                    TDollDic_TDollInfo_ModSelector1.Enabled = true;
                    TDollDic_TDollInfo_ModSelector2.Enabled = true;
                    TDollDic_TDollInfo_ModSelector3.Enabled = true;
                }
                else
                {
                    TDollDic_TDollInfo_ModSelector1.Enabled = false;
                    TDollDic_TDollInfo_ModSelector2.Enabled = false;
                    TDollDic_TDollInfo_ModSelector3.Enabled = false;
                }

                TDollDic_TDollInfo_ModSelector0.Checked = true;

                TDollDic_TDollInfo_DicNumber.Text = $"No. {doll.DicNumber}";
                TDollDic_TDollInfo_Name.Text = doll.Name;
                TDollDic_TDollInfo_CodeName.Text = doll.CodeName;
                TDollDic_TDollInfo_ProductTimeLabel.Text = 
                    $"{(doll.GetProductTimeToString == "None" ? "제조 불가" : doll.GetProductTimeToString)}";
                TDollDic_TDollInfo_IllustratorInfo.Text = doll.Illustrator;
                TDollDic_TDollInfo_NickNameInfo.Text = doll.NickName;

                await LoadDollImages(isRefresh);

                Bitmap gBitmap;

                switch (doll.Grade)
                {
                    default:
                    case 0:
                        gBitmap = Resources.PSGrade_0;
                        break;
                    case 2:
                        gBitmap = Resources.PSGrade_2;
                        break;
                    case 3:
                        gBitmap = Resources.PSGrade_3;
                        break;
                    case 4:
                        gBitmap = Resources.PSGrade_4;
                        break;
                    case 5:
                        gBitmap = Resources.PSGrade_5;
                        break;
                }

                TDollDic_TDollInfo_FullImageView.BackgroundImage = gBitmap;
                
                switch (doll.Type)
                {
                    default:
                    case "HG":
                        TDollDic_TDollInfo_TypeIconImage.Image = Resources.HG;
                        break;
                    case "SMG":
                        TDollDic_TDollInfo_TypeIconImage.Image = Resources.SMG;
                        break;
                    case "AR":
                        TDollDic_TDollInfo_TypeIconImage.Image = Resources.AR;
                        break;
                    case "RF":
                        TDollDic_TDollInfo_TypeIconImage.Image = Resources.RF;
                        break;
                    case "MG":
                        TDollDic_TDollInfo_TypeIconImage.Image = Resources.MG;
                        break;
                    case "SG":
                        TDollDic_TDollInfo_TypeIconImage.Image = Resources.SG;
                        break;
                }

                LoadFormationBuff();

                // 능력치 정보 불러오기도 동시 실행됨
                abilityLv = 1;

                if (TDollDic_TDollInfo_AbilityFavorSelector.SelectedIndex != 1)
                    TDollDic_TDollInfo_AbilityFavorSelector.SelectedIndex = 1;
                else
                    _ = LoadAbility();

                if (doll.HasVoice)
                {
                    TDollDic_TDollInfo_VoiceGroup.Visible = true;
                    LoadCostumeVoiceList();
                }
                else
                    TDollDic_TDollInfo_VoiceGroup.Visible = false;

                TDollDic_TDollInfo_GainTooltip.SetToolTip(TDollDic_TDollInfo_FullImageView, doll.ProductDialog);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("인형 불러오기 오류", "인형 정보를 불러오는 중 오류가 발생했습니다.");
            }
            finally
            {
                isDollLoad = false;
            }
        }

        private async Task LoadDollImages(bool isRefresh)
        {
            try
            {
                string fileName = $"{doll.DicNumber}{(modIndex == 3 ? "_M" : "")}";

                try
                {
                    string url = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal", $"{fileName}.png");
                    string target = Path.Combine(ETC.cachePath, "Doll", "Normal", $"{fileName}.gfdcache");

                    if (!File.Exists(target) || isRefresh)
                        using (WebClient wc = new WebClient())
                            await wc.DownloadFileTaskAsync(url, target);

                    TDollDic_TDollInfo_FullImageView.ImageLocation = target;
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("인형 불러오기 오류", "이미지를 불러오는 동안 오류가 발생했습니다.");
            }
        }

        private void LoadFormationBuff()
        {
            int[] formation;
            string[] type;
            string[] mag;
            TableLayoutControlCollection collection;
            StringBuilder sb;

            try
            {
                if (modIndex == 0)
                {
                    formation = doll.BuffFormation;
                    type = doll.BuffType;
                    mag = doll.BuffInfo;
                }
                else
                {
                    formation = doll.ModBuffFormation;
                    type = doll.BuffType;
                    mag = doll.ModBuffInfo;
                }


                // 버프 진형 채우기
                collection = TDollDic_TDollInfo_FormationBuffTable.Controls;

                for (int i = 0; i < formation.Length; ++i)
                {
                    Color c;

                    switch (formation[i])
                    {
                        default:
                        case 0:
                            c = Color.LightGray;
                            break;
                        case 1:
                            c = Color.Green;
                            break;
                        case 2:
                            c = Color.Black;
                            break;
                    }

                    collection[i].BackColor = c;
                }


                // 버프 타입
                sb = new StringBuilder();

                for (int i = 0; i < type.Length; ++i)
                {
                    sb.Append(type[i]);
                    sb.Append(" ");
                }

                TDollDic_TDollInfo_FormationBuffType.Text = $"{sb.ToString()} 적용";


                // 버프 효과 및 효과치
                PictureBox[] buffTypeIcons =
                {
                    TDollDic_TDollInfo_FormationBuffIcon1,
                    TDollDic_TDollInfo_FormationBuffIcon2
                };
                Label[] buffMags =
                {
                    TDollDic_TDollInfo_FormationBuffMag1,
                    TDollDic_TDollInfo_FormationBuffMag2
                };
                string[] buffType = mag[0].Split(',');

                for (int i = 0; i < buffTypeIcons.Length; ++i)
                {
                    buffTypeIcons[i].Image = null;
                    buffMags[i].Text = "";
                }

                for (int i = 0; i < buffType.Length; ++i)
                {
                    Bitmap btBitmap;

                    switch (buffType[i])
                    {
                        case "AC":
                            btBitmap = Resources.AC_Icon;
                            break;
                        case "AM":
                            btBitmap = Resources.AM_Icon;
                            break;
                        case "AS":
                            btBitmap = Resources.AS_Icon;
                            break;
                        case "CR":
                            btBitmap = Resources.CR_Icon;
                            break;
                        case "EV":
                            btBitmap = Resources.EV_Icon;
                            break;
                        case "FR":
                            btBitmap = Resources.FR_Icon;
                            break;
                        case "CL":
                            btBitmap = Resources.CL_Icon;
                            break;
                        default:
                            btBitmap = null;
                            break;
                    }

                    buffTypeIcons[i].Image = btBitmap;

                    sb.Clear();
                    
                    for (int k = 1; k < mag.Length; ++k)
                    {
                        string[] m = mag[k].Split(',');

                        sb.Append(m[i]);
                        sb.Append("%");

                        if (k < mag.Length - 1)
                            sb.Append("/");
                    }

                    buffMags[i].Text = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("인형 불러오기 오류", "진형 버프를 불러오는 동안 오류가 발생했습니다.");
            }
        }

        private async Task LoadAbility()
        {
            await Task.Delay(10);

            Label[] abilityMags =
            {
                TDollDic_TDollInfo_AbilityHPMag,
                TDollDic_TDollInfo_AbilityFRMag,
                TDollDic_TDollInfo_AbilityEVMag,
                TDollDic_TDollInfo_AbilityACMag,
                TDollDic_TDollInfo_AbilityASMag
            };

            try
            {
                string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };


                string[] grow_ratio = doll.Abilities["Grow"].Split(';');

                for (int i = 0; i < abilityMags.Length; ++i)
                {
                    string[] basic_ratio = doll.Abilities[abilities[i]].Split(';');

                    int value = 0;

                    switch (modIndex)
                    {
                        case 0:
                            value = das.CalcAbility(abilities[i], int.Parse(basic_ratio[0]), int.Parse(grow_ratio[0]), abilityLv, abilityFavor, false);
                            break;
                        case 1:
                        case 2:
                        case 3:
                            value = das.CalcAbility(abilities[i], int.Parse(basic_ratio[1]), int.Parse(grow_ratio[1]), abilityLv, abilityFavor, true);
                            break;
                    }

                    abilityValues[i] = value;

                    abilityMags[i].Text = $"{value} ({doll.AbilityGrade[i]})";
                }

                if ((doll.Type == "MG") || (doll.Type == "SG"))
                {
                    TDollDic_TDollInfo_AbilityBulletPanel.Visible = true;
                    TDollDic_TDollInfo_AbilityReloadPanel.Visible = true;

                    double ReloadTime = CalcReloadTime(doll, doll.Type, abilityValues[4]);
                    int Bullet = 0;

                    if (doll.HasMod == false)
                        Bullet = int.Parse(doll.Abilities["Bullet"]);
                    else
                        Bullet = int.Parse(doll.Abilities["Bullet"].Split(';')[modIndex]);

                    TDollDic_TDollInfo_AbilityBulletMag.Text = Bullet.ToString();
                    TDollDic_TDollInfo_AbilityReloadMag.Text = $"{ReloadTime} 초";
                }
                else
                {
                    TDollDic_TDollInfo_AbilityBulletPanel.Visible = false;
                    TDollDic_TDollInfo_AbilityReloadPanel.Visible = false;
                }

                if (doll.Type == "SG")
                {
                    TDollDic_TDollInfo_AbilityAMPanel.Visible = true;

                    string[] basic_ratio = doll.Abilities["Armor"].Split(';');
                    int value = 0;

                    switch (modIndex)
                    {
                        case 0:
                            value = das.CalcAbility("Armor", int.Parse(basic_ratio[0]), int.Parse(grow_ratio[0]), abilityLv, abilityFavor, false);
                            break;
                        case 1:
                        case 2:
                        case 3:
                            value = das.CalcAbility("Armor", int.Parse(basic_ratio[1]), int.Parse(grow_ratio[1]), abilityLv, abilityFavor, true);
                            break;
                    }

                    abilityValues[5] = value;
                    TDollDic_TDollInfo_AbilityAMMag.Text = $"{value} ({doll.AbilityGrade[6]})";
                }
                else
                {
                    abilityValues[5] = 0;
                    TDollDic_TDollInfo_AbilityAMPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("인형 불러오기 오류", "능력치 정보를 불러오는 동안 오류가 발생했습니다.");
            }
        }

        private void LoadCostumeVoiceList()
        {
            try
            {
                TDollDic_TDollInfo_CostumeVoiceSelector.Items.Clear();

                List<string> vcList = new List<string>()
                {
                    "기본"
                };

                if (doll.CostumeVoices != null)
                    for (int i = 0; i < (doll.CostumeVoices.Length / doll.CostumeVoices.Rank); ++i)
                        vcList.Add(doll.Costumes[int.Parse(doll.CostumeVoices[i, 0])]);

                vcList.TrimExcess();

                TDollDic_TDollInfo_CostumeVoiceSelector.Items.AddRange(vcList.ToArray());
                TDollDic_TDollInfo_CostumeVoiceSelector.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("인형 불러오기 오류", "코스튬 보이스 목록을 불러오는 동안 오류가 발생했습니다.");
            }
        }

        private double CalcReloadTime(Doll doll, string type, int AttackSpeed)
        {
            double result = 0;

            switch (type)
            {
                case "MG":
                    int tAS = AttackSpeed;
                    if (tAS == 0) result = 0;
                    else result = 4 + (200 / tAS);
                    break;
                case "SG":
                    int tB = int.Parse(doll.Abilities["Bullet"]);
                    result = 1.5 + (0.5 * tB);
                    break;
            }

            return result;
        }

        private async Task PlayVoice(string voiceName)
        {
            try
            {
                string server = "";
                string target = "";

                switch (vCostumeIndex)
                {
                    case 0:
                        server = Path.Combine(ETC.server, "Data", "Voice", "Doll", doll.krName, $"{doll.krName}_{voiceName}_JP.wav");
                        target = Path.Combine(ETC.cachePath, "Voices", "Doll", $"{doll.DicNumber}_{voiceName}_JP.gfdcache");
                        break;
                    default:
                        server = Path.Combine(ETC.server, "Data", "Voice", "Doll", $"{doll.krName}_{vCostumeIndex - 1}", $"{doll.krName}_{vCostumeIndex - 1}_{voiceName}_JP.wav");
                        target = Path.Combine(ETC.cachePath, "Voices", "Doll", $"{doll.DicNumber}_{vCostumeIndex - 1}_{voiceName}_JP.gfdcache");
                        break;
                }

                await Task.Delay(100);

                if (!File.Exists(target))
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadProgressChanged += (object s, DownloadProgressChangedEventArgs args) =>
                            {

                            };
                            await wc.DownloadFileTaskAsync(server, target);
                        }
                    }
                    catch (Exception ex)
                    {
                        ETC.LogError(ex);
                    }

                soundPlayer.SoundLocation = target;
                soundPlayer.Play();
            }
            catch (WebException ex)
            {
                ETC.LogError(ex);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }



        // Event Functions

        private void TDollDic_TDollFilter_CheckedChanged(object sender, EventArgs e)
        {
            _ = ListDoll(TDollDic_SearchTextBox.Text);
        }

        private void TDollDic_SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _ = ListDoll((sender as TextBox).Text);
        }

        private void TDollDic_TDollProductTimeButton_Click(object sender, EventArgs e)
        {
            ProductTimeTable ptt = new ProductTimeTable(0);
            ptt.Show();
        }

        private void TDollDic_TDollInfo_FullImageView_Click(object sender, EventArgs e)
        {
            TDollImageViewer iv = new TDollImageViewer(doll, modIndex);
            iv.Show();
        }

        private void TDollDic_TDollListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            try
            {
                ListView lv = sender as ListView;

                switch (lv.Sorting)
                {
                    case SortOrder.None:
                    case SortOrder.Descending:
                    default:
                        lv.Sorting = SortOrder.Ascending;
                        break;
                    case SortOrder.Ascending:
                        lv.Sorting = SortOrder.Descending;
                        break;
                }

                lv.ListViewItemSorter = new ETC.Sorter
                {
                    Order = lv.Sorting,
                    ColumnIndex = e.Column
                };

                lv.Sort();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }

        private void TDollDic_TDollListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            try
            {
                if (e.IsSelected)
                {
                    doll = new Doll(ETC.FindDataRow(ETC.dollList, "DicNumber", int.Parse(e.Item.Text)));
                    vCostumeIndex = 0;

                    _ = LoadTDollInfo();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("인형 불러오기 오류", "인형 정보를 불러오는 동안 오류가 발생했습니다.");
            }
        }

        private void TDollDic_TDollInfo_AbilityLevelSelector_ValueChanged(object sender, EventArgs e)
        {
            abilityLv = Convert.ToInt32((sender as NumericUpDown).Value);

            _ = LoadAbility();
        }

        private void TDollDic_TDollInfo_AbilityFavorSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((sender as ComboBox).SelectedIndex)
            {
                case 0:
                    abilityFavor = 5;
                    break;
                default:
                case 1:
                    abilityFavor = 50;
                    break;
                case 2:
                    abilityFavor = 115;
                    break;
                case 3:
                    abilityFavor = 165;
                    break;
                case 4:
                    abilityFavor = 195;
                    break;
            }

            _ = LoadAbility();
        }

        private async void TDollDic_TDollInfo_ModSelector_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked == false)
                return;

            modIndex = Convert.ToInt32((sender as RadioButton).Tag);

            if (!isDollLoad)
            {
                await LoadDollImages(false);
                await LoadAbility();
                LoadFormationBuff();

                switch (modIndex)
                {
                    default:
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                }
            }
        }

        private void TDollDic_TDollInfo_GunDataButton_Click(object sender, EventArgs e)
        {
            string url = Path.Combine(ETC.server, "Data", "Text", "Gun", "ModelData", $"{doll.DicNumber}.txt");

            TextViewer tv = new TextViewer($"총기 제원 - {doll.Name}", url, true);
            tv.Show();
        }

        private void TDollDic_TDollInfo_ModStoryButton_Click(object sender, EventArgs e)
        {
            TextViewer tv = new TextViewer(doll);
            tv.Show();
        }

        private void TDollDic_TDollInfo_FullImageView_LoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TDollDic_TDollInfo_FullImageLoadProcessBar.Visible = true;
            TDollDic_TDollInfo_FullImageLoadProcessBar.Style = ProgressBarStyle.Continuous;
            TDollDic_TDollInfo_FullImageLoadProcessBar.Value = e.ProgressPercentage;
        }

        private void TDollDic_TDollInfo_FullImageView_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            TDollDic_TDollInfo_FullImageLoadProcessBar.Visible = false;
        }

        private void TDollDic_TDollInfo_CostumeVoiceSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                vCostumeIndex = (sender as ComboBox).SelectedIndex;

                dollVoiceList.Clear();
                TDollDic_TDollInfo_VoiceSelector.Items.Clear();

                switch (vCostumeIndex)
                {
                    case 0:
                        dollVoiceList.AddRange(doll.Voices);
                        break;
                    default:
                        dollVoiceList.AddRange(doll.CostumeVoices[vCostumeIndex - 1, 1].Split(';'));
                        break;
                }
                dollVoiceList.TrimExcess();

                TDollDic_TDollInfo_VoiceSelector.Items.AddRange(dollVoiceList.ToArray());
                TDollDic_TDollInfo_VoiceSelector.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("인형 불러오기 오류", "보이스 목록을 불러오는 중 오류가 발생했습니다.");
            }
        }

        private void TDollDic_TDollInfo_VoicePlayButton_Click(object sender, EventArgs e)
        {
            int vIndex = TDollDic_TDollInfo_VoiceSelector.SelectedIndex;

            _ = PlayVoice(dollVoiceList[vIndex]);
        }

        private void TDollDic_TDollInfo_SkillButton_Click(object sender, EventArgs e)
        {
            SkillViewer sv;

            switch ((string)(sender as Button).Tag)
            {
                default:
                case "Normal":
                    sv = new SkillViewer(doll, false);
                    break;
                case "Mod":
                    sv = new SkillViewer(doll, true);
                    break;
            }

            sv.Show();
        }
    }
}
