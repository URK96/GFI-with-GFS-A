using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GFD_W
{
    public partial class TextViewer : Form
    {
        string category;
        string storyName;

        int index = 1;
        int count = 1;
        
        Doll doll;

        ///<summary>
        /// 일반 텍스트 뷰어
        ///</summary>
        public TextViewer(string title, string s, bool isURL = false, bool enableControl = false)
        {
            InitializeComponent();

            Text = title;

            if (!enableControl)
                TextViewer_ControlPanel.Visible = false;

            if (isURL)
                _ = LoadText(s);
            else
                TextViewer_TextField.Text = s;
        }

        ///<summary>
        /// MOD 스토리 뷰어
        ///</summary>
        public TextViewer(Doll doll)
        {
            InitializeComponent();

            category = "ModStory";
            this.doll = doll;
            count = 4;

            TextViewer_PreviousButton.Enabled = false;

            _ = LoadStory(doll.DicNumber.ToString());
        }

        ///<summary>
        /// 일반 스토리 뷰어
        ///</summary>
        public TextViewer(string category, string storyName, int index, int count)
        {
            InitializeComponent();

            this.category = category;
            this.storyName = storyName;
            this.index = index;
            this.count = count;

            if (index <= 1)
                TextViewer_PreviousButton.Enabled = false;
            if (index >= count)
                TextViewer_NextButton.Enabled = false;

            _ = LoadStory();
        }

        private async Task LoadText(string url)
        {
            string text;

            try
            {
                using (WebClient wc = new WebClient())
                    text = await wc.DownloadStringTaskAsync(url);

                TextViewer_TextField.Text = text;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("텍스트 뷰어 오류", "네트워크에서 텍스트를 불러오는 중 오류가 발생했습니다.");
            }
        }

        private async Task LoadStory(string fileName = "", bool isRefresh = false)
        {
            try
            {
                string file;

                if (category == "ModStory")
                {
                    file = Path.Combine(ETC.cachePath, "Story", category, $"{fileName}_{index}.gfdcache");
                    TextViewer_NowStatus.Text = $"개조 이야기 - {doll.Name} {index}화";
                }
                else
                {
                    file = Path.Combine(ETC.cachePath, "Story", category, $"{index}.gfdcache");
                    TextViewer_NowStatus.Text = $"{category} - {storyName}";
                }

                if ((File.Exists(file) == false) || (isRefresh == true))
                    await DownloadStory(category, Path.GetFileNameWithoutExtension(file));

                

                using (StreamReader sr = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
                    TextViewer_TextField.Text = await sr.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("텍스트 뷰어 오류", "스토리 데이터를 불러오는 중 오류가 발생했습니다.");
            }
        }

        private async Task DownloadStory(string category, string fileName)
        {
            try
            {
                string server = "";
                string target = "";

                if (category == "ModStory")
                {
                    server = Path.Combine(ETC.server, "Data", "Text", "Story", "ko", "Sub", category, $"{fileName}.txt");
                    target = Path.Combine(ETC.cachePath, "Story", category, $"{fileName}.gfdcache");
                }
                else
                {
                    server = Path.Combine(ETC.server, "Data", "Text", "Story", "ko", "Main", category, $"{fileName}.txt");
                    target = Path.Combine(ETC.cachePath, "Story", category, $"{fileName}.gfdcache");
                }

                DirectoryInfo di = new DirectoryInfo(Path.Combine(ETC.cachePath, "Story", category));
                if (di.Exists == false)
                    di.Create();

                using (WebClient wc = new WebClient())
                    await wc.DownloadFileTaskAsync(server, target);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
                ETC.ShowErrorMessage("텍스트 뷰어 오류", "스토리 데이터를 다운로드 하는 중 오류가 발생했습니다.");
            }
        }

        private void TextViewer_ControlButton_Click(object sender, EventArgs e)
        {
            try
            {
                switch ((string)(sender as Button).Tag)
                {
                    case "P":
                        if (index > 1)
                            index -= 1;
                        break;
                    default:
                    case "N":
                        if (index < count)
                            index += 1;
                        break;
                }

                TextViewer_PreviousButton.Enabled = true;
                TextViewer_NextButton.Enabled = true;

                if (index <= 1)
                    TextViewer_PreviousButton.Enabled = false;
                if (index >= count)
                    TextViewer_NextButton.Enabled = false;

                if (category == "ModStory")
                    _ = LoadStory(doll.DicNumber.ToString());
                else
                    _ = LoadStory();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }
    }
}
