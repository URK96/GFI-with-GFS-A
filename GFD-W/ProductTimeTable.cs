using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GFD_W
{
    public partial class ProductTimeTable : Form
    {
        // 0 = Doll, 1 = Equip, 2 = Fairy
        public ProductTimeTable(int type)
        {
            InitializeComponent();

            switch (type)
            {
                case 0:
                    ListTable(ref ETC.dollList);
                    break;
                case 1:
                    ListTable(ref ETC.equipmentList);
                    break;
                case 2:
                    ListTable(ref ETC.fairyList);
                    break;
                default:
                    if (MessageBox.Show("잘못된 데이터 지정입니다.", "데이터 지정 오류", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                        Close();
                    break;
            }
        }

        private void ListTable(ref DataTable dt)
        {
            int[] time = new int[2];

            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int t = (int)dr["ProductTime"];

                    if (t == 0)
                        continue;

                    time[0] = t / 60;
                    time[1] = t - time[0] * 60;

                    ProductTimeTable_ListView.Items.Add(new ListViewItem(new string[]
                        {
                            "",
                            $"{time[0]} : {time[1]}",
                            (string)dr["Name"]
                        }));
                }

                ProductTimeTable_ListView.ListViewItemSorter = new ETC.Sorter
                {
                    Order = ProductTimeTable_ListView.Sorting,
                    ColumnIndex = 1
                };

                ProductTimeTable_ListView.Sort();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);

                if (MessageBox.Show("데이터 구성 중 오류가 발생했습니다.", "데이터 구성 오류", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                    Close();
            }
        }
    }
}
