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
    public partial class TDollImageViewer : Form
    {
        Doll doll;

        List<string> costumeList;

        int costumeIndex = 0;
        int modIndex = 0;

        bool isDamage = false;
        bool enableCensored = true;
        public TDollImageViewer(Doll doll, int modIndex)
        {
            InitializeComponent();

            this.doll = doll;
            this.modIndex = modIndex;

            InitializeList();
        }

        private void InitializeList()
        {
            try
            {
                costumeList = new List<string>()
                {
                    "기본"
                };

                if (doll.Costumes != null)
                    costumeList.AddRange(doll.Costumes);

                costumeList.TrimExcess();

                TDollDic_ImageViewer_CostumeSelector.DataSource = costumeList;

                _ = LoadImage();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("이미지 뷰어 초기화 오류", "이미지 뷰어를 초기화 하는 중 오류가 발생했습니다.");
            }
        }

        private bool CheckCensorType()
        {
            if (doll.CensorType == null)
                return false;

            string censor_type;

            switch (costumeIndex)
            {
                case 0:
                    censor_type = isDamage ? "D" : "N";
                    break;
                default:
                    censor_type = $"C{costumeIndex}";

                    if (isDamage == true)
                        censor_type += "D";
                    break;
            }

            foreach (string type in doll.CensorType)
            {
                if (type == censor_type)
                    return true;
            }

            return false;
        }

        private async Task LoadImage(bool IsRefresh = false)
        {
            try
            {
                string ImageName = doll.DicNumber.ToString();

                if (costumeIndex >= 1)
                    ImageName += $"_{costumeIndex + 1}";
                else if 
                    ((costumeIndex == 0) && (modIndex == 3)) ImageName += "_M";

                if (isDamage == true)
                    ImageName += "_D";

                if ((doll.HasCensored == true) && (enableCensored == true) && (modIndex != 3))
                    if (CheckCensorType() == true)
                        ImageName += "_C";

                string ImagePath = Path.Combine(ETC.cachePath, "Doll", "Normal", $"{ImageName}.gfdcache");
                string URL = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal", $"{ImageName}.png");

                if ((File.Exists(ImagePath) == false) || (IsRefresh == true))
                    using (WebClient wc = new WebClient())
                        await Task.Run(async () => { await wc.DownloadFileTaskAsync(URL, ImagePath); });

                await Task.Delay(100);

                TDollDic_ImageViewer_ImageView.ImageLocation = ImagePath;

                string damageText = isDamage ? "중상" : "정상";

                TDollDic_ImageViewer_DamageToggle.Text = damageText;
                TDollDic_ImageViewer_ImageStatus.Text = $"{doll.Name} - {costumeList[costumeIndex]} - {damageText}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("이미지 로드 오류", "이미지를 로드하는 중 오류가 발생했습니다.");
            }
        }

        private void TDollDic_ImageViewer_CostumeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            costumeIndex = (sender as ComboBox).SelectedIndex;

            if (isDamage)
                TDollDic_ImageViewer_DamageToggle.Checked = false;

            _ = LoadImage();
        }

        private void TDollDic_ImageViewer_RefreshButton_Click(object sender, EventArgs e)
        {
            _ = LoadImage(true);
        }

        private void TDollDic_ImageViewer_DamageToggle_CheckedChanged(object sender, EventArgs e)
        {
            isDamage = !isDamage;

            _ = LoadImage();
        }
    }
}
