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
        List<Fairy> fairyRootList = new List<Fairy>();

        Fairy fairy;

        bool isFairyLoad = false;
        bool[] applyFairyFilters = { false };

        private async Task InitializeFairyDic()
        {
            await Task.Delay(1);

            try
            {
                FairyDic_SplitContainer.Panel1.Hide();

                _ = ListFairy(null);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("도감 초기화 오류", "요정 도감 초기화 중 오류가 발생했습니다.");
            }
        }

        private async Task ListFairy(string searchName)
        {
            await Task.Delay(10);

            try
            {
                FairyDic_FairyListView.Items.Clear();

                CheckApplyFairyFilter();

                foreach (Fairy fairy in fairyRootList)
                {
                    if ((searchName != null) && !fairy.Name.ToUpper().Contains(searchName.ToUpper()))
                        continue;

                    if (!CheckFilter(fairy))
                        continue;

                    FairyDic_FairyListView.Items.Add(new ListViewItem(new string[]
                    {
                        fairy.DicNumber.ToString(),
                        fairy.Type,
                        fairy.Name
                    }));
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("리스트 오류", "요정 리스트를 생성하는 중 오류가 발생했습니다.");
            }
        }

        private void CheckApplyFairyFilter()
        {
            for (int i = 0; i < applyFairyFilters.Length; ++i)
                applyFairyFilters[i] = false;

            var typeCollection = FairyDic_FairyFilterGroup_TypeGroup.Controls;

            foreach (CheckBox c in typeCollection)
                if (c.Checked)
                    applyFairyFilters[0] = true;
        }

        private bool CheckFilter(Fairy fairy)
        {
            var typeCollection = FairyDic_FairyFilterGroup_TypeGroup.Controls;

            if (applyFairyFilters[0])
                foreach (CheckBox c in typeCollection)
                    if ((fairy.Type == c.Text) && !c.Checked)
                        return false;

            return true;
        }

        private async Task LoadFairyInfo(bool isRefresh = false)
        {
            await Task.Delay(10);
            isFairyLoad = true;

            try
            {
                FairyDic_SplitContainer.Panel1.Show();

                FairyDic_FairyInfo_DicNumber.Text = $"No. {fairy.DicNumber}";
                FairyDic_FairyInfo_Name.Text = fairy.Name;
                FairyDic_FairyInfo_Type.Text = fairy.Type;

                await LoadFairyImage(isRefresh);

                FairyDic_FairyInfo_ProductTimeLabel.Text = fairy.GetProductTimeToString;

                LoadAbilityFairy();

                FairyDic_FairyInfo_GainToolTip.SetToolTip(FairyDic_FairyInfo_FullImageView, fairy.ProductDialog);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("요정 불러오기 오류", "요정 정보를 불러오는 중 오류가 발생했습니다.");
            }
            finally
            {
                isFairyLoad = false;
            }
        }

        private async Task LoadFairyImage(bool isRefresh)
        {
            try
            {
                string fileName = fairy.DicNumber.ToString();

                try
                {
                    string url = Path.Combine(ETC.server, "Data", "Images", "Fairy", "Normal_Crop", $"{fileName}.png");
                    string target = Path.Combine(ETC.cachePath, "Fairy", "Normal", $"{fileName}.gfdcache");

                    if (!File.Exists(target) || isRefresh)
                        using (WebClient wc = new WebClient())
                            await wc.DownloadFileTaskAsync(url, target);

                    FairyDic_FairyInfo_FullImageView.ImageLocation = target;
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("요정 불러오기 오류", "이미지를 불러오는 동안 오류가 발생했습니다.");            }
        }

        private void LoadAbilityFairy()
        {
            Label[] abilityMags =
            {
                FairyDic_FairyInfo_AbilityFRMag,
                FairyDic_FairyInfo_AbilityACMag,
                FairyDic_FairyInfo_AbilityEVMag,
                FairyDic_FairyInfo_AbilityAMMag,
                FairyDic_FairyInfo_AbilityCRMag
            };

            try
            {
                string[] abilities = { "FireRate", "Accuracy", "Evasion", "Armor", "Critical" };

                for (int i = 0; i < abilities.Length; ++i)
                    abilityMags[i].Text = fairy.Abilities[abilities[i]];
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("요정 불러오기 오류", "능력치 정보를 불러오는 동안 오류가 발생했습니다.");
            }
        }


        // Event Functions

        private void FairyDic_FairyFilter_CheckedChanged(object sender, EventArgs e)
        {
            _ = ListFairy(FairyDic_SearchTextBox.Text);
        }

        private void FairyDic_SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _ = ListFairy((sender as TextBox).Text);
        }

        private void FairyDic_FairyProductTimeButton_Click(object sender, EventArgs e)
        {
            ProductTimeTable ptt = new ProductTimeTable(2);
            ptt.Show();
        }

        private void FairyDic_FairyListView_ColumnClick(object sender, ColumnClickEventArgs e)
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

        private void FairyDic_FairyListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            try
            {
                if (e.IsSelected)
                {
                    fairy = new Fairy(ETC.FindDataRow(ETC.fairyList, "DicNumber", int.Parse(e.Item.Text)));

                    _ = LoadFairyInfo();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("요정 불러오기 오류", "요정 정보를 불러오는 동안 오류가 발생했습니다.");
            }
        }

    }
}