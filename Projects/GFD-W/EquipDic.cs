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
        List<Equip> equipRootList = new List<Equip>();

        Equip equip;

        bool isEquipLoad = false;
        bool[] applyEquipFilters = { false, false };

        private async Task InitializeEquipDic()
        {
            await Task.Delay(1);

            try
            {
                EquipDic_SplitContainer.Panel1.Hide();

                _ = ListEquip(null);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("도감 초기화 오류", "장비 도감 초기화 중 오류가 발생했습니다.");
            }
        }

        private async Task ListEquip(string searchName)
        {
            await Task.Delay(10);

            try
            {
                EquipDic_EquipListView.Items.Clear();

                CheckApplyEquipFilter();

                foreach (Equip equip in equipRootList)
                {
                    if ((searchName != null) && !equip.Name.ToUpper().Contains(searchName.ToUpper()))
                        continue;

                    if (!CheckFilter(equip))
                        continue;

                    string grade = "";

                    if (equip.Grade == 0)
                        grade = "★";
                    else
                        grade = $"☆{equip.Grade}";

                    EquipDic_EquipListView.Items.Add(new ListViewItem(new string[]
                    {
                        equip.Id.ToString(),
                        grade,
                        equip.Name,
                        equip.Category,
                        equip.Type
                    }));
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("리스트 오류", "장비 리스트를 생성하는 중 오류가 발생했습니다.");
            }
        }

        private void CheckApplyEquipFilter()
        {
            for (int i = 0; i < applyEquipFilters.Length; ++i)
                applyEquipFilters[i] = false;

            var categoryCollection = EquipDic_EquipFilterGroup_CategoryGroup.Controls;
            var gradeCollection = EquipDic_EquipFilterGroup_GradeGroup.Controls;

            foreach (CheckBox c in categoryCollection)
                if (c.Checked)
                    applyEquipFilters[0] = true;

            foreach (CheckBox c in gradeCollection)
                if (c.Checked)
                    applyEquipFilters[1] = true;
        }

        private bool CheckFilter(Equip equip)
        {
            var categoryCollection = EquipDic_EquipFilterGroup_CategoryGroup.Controls;
            var gradeCollection = EquipDic_EquipFilterGroup_GradeGroup.Controls;

            if (applyEquipFilters[0])
                foreach (CheckBox c in categoryCollection)
                    if ((equip.Category == c.Text) && !c.Checked)
                        return false;

            if (applyEquipFilters[1])
                foreach (CheckBox c in gradeCollection)
                    if ((equip.Grade == int.Parse(c.Tag.ToString())) && !c.Checked)
                        return false;

            return true;
        }

        private async Task LoadEquipInfo(bool isRefresh = false)
        {
            await Task.Delay(10);
            isEquipLoad = true;

            try
            {
                EquipDic_SplitContainer.Panel1.Show();

                EquipDic_EquipInfo_DicNumber.Text = $"No. {equip.Id}";
                EquipDic_EquipInfo_Name.Text = equip.Name;
                EquipDic_EquipInfo_TypeCategory.Text = $"{equip.Category} - {equip.Type}";

                await LoadEquipImage(isRefresh);

                Bitmap gBitmap;

                switch (equip.Grade)
                {
                    default:
                    case 2:
                        gBitmap = Resources.EquipBG_2;
                        break;
                    case 3:
                        gBitmap = Resources.EquipBG_3;
                        break;
                    case 4:
                        gBitmap = Resources.EquipBG_4;
                        break;
                    case 0:
                    case 5:
                        gBitmap = Resources.EquipBG_5;
                        break;
                }

                EquipDic_EquipInfo_FullImageView.BackgroundImage = gBitmap;

                EquipDic_EquipInfo_ProductTimeLabel.Text = equip.GetProductTimeToString;

                if (equip.OnlyUse == null)
                {
                    EquipDic_EquipInfo_OnlyUseDollPanel.Visible = false;
                    EquipDic_EquipInfo_DollTypePanel.Visible = true;
                    LoadEquipDollType();
                }
                else
                {
                    EquipDic_EquipInfo_DollTypePanel.Visible = false;
                    EquipDic_EquipInfo_OnlyUseDollPanel.Visible = true;
                    LoadEquipOnlyUseDoll();
                }

                LoadAbilityEquip();

                EquipDic_EquipInfo_GainTooltip.SetToolTip(EquipDic_EquipInfo_FullImageView, equip.ProductDialog);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("장비 불러오기 오류", "장비 정보를 불러오는 중 오류가 발생했습니다.");
            }
            finally
            {
                isEquipLoad = false;
            }
        }

        private async Task LoadEquipImage(bool isRefresh)
        {
            try
            {
                string fileName = equip.Icon;

                try
                {
                    string url = Path.Combine(ETC.server, "Data", "Images", "Equipments", $"{fileName}.png");
                    string target = Path.Combine(ETC.cachePath, "Equip", "Normal", $"{fileName}.gfdcache");

                    if (!File.Exists(target) || isRefresh)
                        using (WebClient wc = new WebClient())
                            await wc.DownloadFileTaskAsync(url, target);

                    EquipDic_EquipInfo_FullImageView.ImageLocation = target;
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("장비 불러오기 오류", "이미지를 불러오는 동안 오류가 발생했습니다.");            }
        }

        private void LoadEquipDollType()
        {
            PictureBox[] typeControls =
            {
                EquipDic_EquipInfo_DollTypeHG,
                EquipDic_EquipInfo_DollTypeSMG,
                EquipDic_EquipInfo_DollTypeAR,
                EquipDic_EquipInfo_DollTypeRF,
                EquipDic_EquipInfo_DollTypeMG,
                EquipDic_EquipInfo_DollTypeSG
            };
            Bitmap[] typeN =
            {
                Resources.HG_N,
                Resources.SMG_N,
                Resources.AR_N,
                Resources.RF_N,
                Resources.MG_N,
                Resources.SG_N
            };
            Bitmap[] type =
            {
                Resources.HG,
                Resources.SMG,
                Resources.AR,
                Resources.RF,
                Resources.MG,
                Resources.SG
            };
            Bitmap[] typeR =
            {
                Resources.HG_R,
                Resources.SMG_R,
                Resources.AR_R,
                Resources.RF_R,
                Resources.MG_R,
                Resources.SG_R
            };

            Bitmap icon;
            string tooltip = "";

            for (int i = 0; i < typeControls.Length; ++i)
            {
                string[] temp = equip.DollType[i].Split(',');

                switch (temp[1])
                {
                    default:
                    case "N":
                        icon = typeN[i];
                        tooltip = $"{temp[0]} 사용불가";
                        break;
                    case "U":
                        icon = type[i];
                        tooltip = $"{temp[0]} 사용가능";
                        break;
                    case "F":
                        icon = typeR[i];
                        tooltip = $"{temp[0]} 사용권장";
                        break;
                }

                typeControls[i].Image = icon;
                EquipDic_EquipInfo_DollTypeToolTip.SetToolTip(typeControls[i], tooltip);
            }
        }

        private void LoadEquipOnlyUseDoll()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < equip.OnlyUse.Length; ++i)
            {
                sb.Append(equip.OnlyUse[i]);

                if (i < (equip.OnlyUse.Length - 1))
                    sb.Append("   ");
            }

            EquipDic_EquipInfo_OnlyUseDollName.Text = sb.ToString();
        }

        private void LoadAbilityEquip()
        {
            Label[] abilityControls =
            {
                EquipDic_EquipInfo_Ability1,
                EquipDic_EquipInfo_Ability2,
                EquipDic_EquipInfo_Ability3,
                EquipDic_EquipInfo_Ability4
            };
            Label[] initMagControls =
            {
                EquipDic_EquipInfo_InitMag1,
                EquipDic_EquipInfo_InitMag2,
                EquipDic_EquipInfo_InitMag3,
                EquipDic_EquipInfo_InitMag4
            };
            Label[] maxMagControls =
            {
                EquipDic_EquipInfo_MaxMag1,
                EquipDic_EquipInfo_MaxMag2,
                EquipDic_EquipInfo_MaxMag3,
                EquipDic_EquipInfo_MaxMag4
            };

            try
            {
                for (int i = 0; i < equip.Abilities.Length; ++i)
                {
                    abilityControls[i].Text = equip.Abilities[i];
                    initMagControls[i].Text = equip.InitMags[i];
                    maxMagControls[i].Text = equip.MaxMags[i];
                }

                for (int i = equip.Abilities.Length; i < abilityControls.Length; ++i)
                {
                    abilityControls[i].Text = "";
                    initMagControls[i].Text = "";
                    maxMagControls[i].Text = "";
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("장비 능력치 오류", "장비 능력치를 불러오는 중 오류가 발생했습니다.");
            }
        }


        // Event Functions

        private void EquipDic_EquipFilter_CheckedChanged(object sender, EventArgs e)
        {
            _ = ListEquip(EquipDic_SearchTextBox.Text);
        }

        private void EquipDic_SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _ = ListEquip((sender as TextBox).Text);
        }

        private void EquipDic_EquipProductTimeButton_Click(object sender, EventArgs e)
        {
            ProductTimeTable ptt = new ProductTimeTable(1);
            ptt.Show();
        }

        private void EquipDic_EquipListView_ColumnClick(object sender, ColumnClickEventArgs e)
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

        private void EquipDic_EquipListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            try
            {
                if (e.IsSelected)
                {
                    equip = new Equip(ETC.FindDataRow(ETC.equipmentList, "Id", int.Parse(e.Item.Text)));

                    _ = LoadEquipInfo();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("장비 불러오기 오류", "장비 정보를 불러오는 동안 오류가 발생했습니다.");
            }
        }

    }
}