using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using AndroidX.Preference;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using XCheckBox = AndroidX.AppCompat.Widget.AppCompatCheckBox;
using XTextView = AndroidX.AppCompat.Widget.AppCompatTextView;
using System.Threading;

namespace GFDA
{
    public partial class MainSettingFragment : PreferenceFragmentCompat
    {
        enum CacheCategory { Dic, OldGFD, GuideBook, Story, PV, OST, Cartoon }

        private const string REGEX_MATCH_INDEX = "name";
        private const string PARENT_DIRECTORY = "Parent Directory";

        private Dictionary<CacheCategory, bool> categoryCheck;

        private View cacheSelectView;

        private double freeSize = 0;
        private int expectSize = 0;

        private void SelectCache()
        {
            string freeText = Resources.GetString(Resource.String.SelectCache_FreeSpace);
            string requireText = Resources.GetString(Resource.String.SelectCache_RequireSpace);

            if (categoryCheck == null)
            {
                categoryCheck = new Dictionary<CacheCategory, bool>();
            }

            categoryCheck.Clear();

            cacheSelectView = LayoutInflater.Inflate(Resource.Layout.CacheSelectLayout, null);

            cacheSelectView.FindViewById<XTextView>(Resource.Id.CacheSelectNowFreeSpace).Text = $"{freeText} : {freeSize}MB";
            cacheSelectView.FindViewById<XTextView>(Resource.Id.CacheSelectRequireSpace).Text = $"{requireText} : {expectSize}MB";
            cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryDic).CheckedChange += CacheSelectCategory_CheckedChange;
            cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryOldGFD).CheckedChange += CacheSelectCategory_CheckedChange;
            cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryGuideBook).CheckedChange += CacheSelectCategory_CheckedChange;
            cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryStory).CheckedChange += CacheSelectCategory_CheckedChange;
            cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryPV).CheckedChange += CacheSelectCategory_CheckedChange;
            cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryOST).CheckedChange += CacheSelectCategory_CheckedChange;
            //cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryCartoon).CheckedChange += CacheSelectCategory_CheckedChange;

            var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBG);
            ad.SetTitle(Resource.String.SelectCache_Dialog_Title);
            ad.SetCancelable(true);
            ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
            ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, async delegate 
            {
                categoryCheck.Add(CacheCategory.Dic, cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryDic).Checked);
                categoryCheck.Add(CacheCategory.OldGFD, cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryOldGFD).Checked);
                categoryCheck.Add(CacheCategory.GuideBook, cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryGuideBook).Checked);
                categoryCheck.Add(CacheCategory.Story, cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryStory).Checked);
                categoryCheck.Add(CacheCategory.PV, cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryPV).Checked);
                categoryCheck.Add(CacheCategory.OST, cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryOST).Checked);
                //categoryCheck.Add(CacheCategory.Cartoon, cacheSelectView.FindViewById<XCheckBox>(Resource.Id.CacheSelectCategoryCartoon).Checked);

                await DownloadAllCache();
            });
            ad.SetView(cacheSelectView);

            var dialog = ad.Show();
        }

        private void CacheSelectCategory_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            const int DIC_EXPECT_SIZE = 1500;
            const int OLDGFD_EXPECT_SIZE = 10;
            const int GUIDEBOOK_EXPECT_SIZE = 10;
            const int STORY_EXPECT_SIZE = 15;
            const int PV_EXPECT_SIZE = 350;
            const int OST_EXPECT_SIZE = 1000;
            const int CARTOON_EXPECT_SIZE = 500;

            string requireText = Resources.GetString(Resource.String.SelectCache_RequireSpace);

            expectSize += (sender as XCheckBox).Id switch
            {
                Resource.Id.CacheSelectCategoryDic => e.IsChecked ? DIC_EXPECT_SIZE : -DIC_EXPECT_SIZE,
                Resource.Id.CacheSelectCategoryOldGFD => e.IsChecked ? OLDGFD_EXPECT_SIZE : -OLDGFD_EXPECT_SIZE,
                Resource.Id.CacheSelectCategoryGuideBook => e.IsChecked ? GUIDEBOOK_EXPECT_SIZE : -GUIDEBOOK_EXPECT_SIZE,
                Resource.Id.CacheSelectCategoryStory => e.IsChecked ? STORY_EXPECT_SIZE : -STORY_EXPECT_SIZE,
                Resource.Id.CacheSelectCategoryPV => e.IsChecked ? PV_EXPECT_SIZE : -PV_EXPECT_SIZE,
                Resource.Id.CacheSelectCategoryOST => e.IsChecked ? OST_EXPECT_SIZE : -OST_EXPECT_SIZE,
                //Resource.Id.CacheSelectCategoryCartoon => e.IsChecked ? CARTOON_EXPECT_SIZE : -CARTOON_EXPECT_SIZE,
                _ => 0
            };

            cacheSelectView.FindViewById<XTextView>(Resource.Id.CacheSelectRequireSpace).Text = $"{requireText} : {expectSize}MB";
        }

        private async Task DownloadAllCache()
        {
            var downloadURLs = new List<(string source, string target)>();

            var v = LayoutInflater.Inflate(Resource.Layout.SpinnerProgressDialogLayout, null);

            var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBG);
            var pd = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBGDownload);
            pd.SetTitle(Resource.String.SettingActivity_DownloadAllCache_DialogTitle);
            pd.SetMessage(Resource.String.SettingActivity_DownloadAllCache_DialogMessage);
            pd.SetCancelable(false);
            pd.SetView(v);

            var dialog = pd.Show();

            try
            {
                var statusText = v.FindViewById<TextView>(Resource.Id.SpinnerProgressStatusMessage);

                statusText.Text = "Ready download list...";

                await Task.Delay(100);

                var listThread = new Thread(new ThreadStart(delegate
                {
                    if (categoryCheck[CacheCategory.Dic])
                    {
                        ListDoll(downloadURLs);
                        ListEquip(downloadURLs);
                        ListFairy(downloadURLs);
                        ListEnemy(downloadURLs);
                        ListFST(downloadURLs);
                        ListCoalition(downloadURLs);
                        ListSkill(downloadURLs);
                    }
                    if (categoryCheck[CacheCategory.OldGFD])
                    {
                        ListOldGFD(downloadURLs);
                    }
                    if (categoryCheck[CacheCategory.GuideBook])
                    {
                        ListGuideBook(downloadURLs);
                    }
                    if (categoryCheck[CacheCategory.Story])
                    {
                        ListStory(downloadURLs);
                    }
                    if (categoryCheck[CacheCategory.PV])
                    {
                        ListPV(downloadURLs);
                    }
                    if (categoryCheck[CacheCategory.OST])
                    {
                        ListMusic(downloadURLs);
                    }
                    /*if (categoryCheck[CacheCategory.Cartoon])
                    {
                        ListCartoon(downloadURLs);
                    }*/
                }));

                listThread.Start();
                listThread.Join();

                using (var wc = new WebClient())
                {
                    var now = 0;
                    var total = downloadURLs.Count;

                    wc.DownloadFileCompleted += (sender, e) => { statusText.Text = $"{++now}/{total}"; };

                    foreach (var (source, target) in downloadURLs)
                    {
                        try
                        {
                            await wc.DownloadFileTaskAsync(source, target);
                        }
                        catch
                        {
                            statusText.Text += " Retry";
                            await wc.DownloadFileTaskAsync(source, target);
                        }
                    }
                }

                Toast.MakeText(Activity, Resource.String.SettingActivity_DownloadAllCache_CompleteDialogMessage, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(Activity, Resource.String.SettingActivity_DownloadAllCache_FailDialogMessage, ToastLength.Short).Show();
            }
            finally
            {
                dialog.Dismiss();
            }
        }

        private MatchCollection GetServerFiles(string url)
        {
            string regexText = $"<a href=\".*\">(?<{REGEX_MATCH_INDEX}>.*)</a>";

            try
            {
                var request = WebRequest.Create(url);

                using var response = request.GetResponse();
                using var reader = new StreamReader(response.GetResponseStream());

                var html = reader.ReadToEnd();

                var regex = new Regex(regexText);

                return regex.Matches(html);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string GetMatchValue(Match match) => match.Groups[REGEX_MATCH_INDEX].Value;

        private bool CheckParentDir(Match match) => GetMatchValue(match).Equals(PARENT_DIRECTORY);

        private void AddList(List<(string, string)> list, string workPath, string targetPath, string extension = "gfdcache", string beforeFileName = "")
        {
            foreach (Match match in GetServerFiles(workPath))
            {
                if (CheckParentDir(match))
                {
                    continue;
                }

                string fileName = GetMatchValue(match);

                string source = Path.Combine(workPath, fileName);
                string target = Path.Combine(targetPath, $"{beforeFileName}{Path.GetFileNameWithoutExtension(fileName)}.{extension}");

                list.Add((source, target));
            }
        }

        private void ListDoll(List<(string, string)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Images", "Guns");
            string targetRootPath = Path.Combine(ETC.cachePath, "Doll");

            // Doll Full Images
            string serverWorkPath = Path.Combine(serverRootPath, "Normal");
            string targetPath = Path.Combine(targetRootPath, "Normal");

            AddList(list, serverWorkPath, targetPath);

            // Doll Crop Images
            serverWorkPath = Path.Combine(serverRootPath, "Normal_Crop");
            targetPath = Path.Combine(targetRootPath, "Normal_Crop");

            AddList(list, serverWorkPath, targetPath);

            // Doll Guide Images
            serverWorkPath = Path.Combine(serverRootPath, "GuideImage");
            targetPath = Path.Combine(targetRootPath, "GuideImage");

            AddList(list, serverWorkPath, targetPath, "png");
        }

        private void ListEquip(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Images", "Equipments");
            string targetRootPath = Path.Combine(ETC.cachePath, "Equip");

            // Equip Images
            string serverWorkPath = Path.Combine(serverRootPath);
            string targetPath = Path.Combine(targetRootPath, "Normal");

            AddList(list, serverWorkPath, targetPath);
        }

        private void ListFairy(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Images", "Fairy");
            string targetRootPath = Path.Combine(ETC.cachePath, "Fairy");

            // Fairy Normal Images
            string serverWorkPath = Path.Combine(serverRootPath, "Normal");
            string targetPath = Path.Combine(targetRootPath, "Normal");

            AddList(list, serverWorkPath, targetPath);

            // Fairy Crop Images
            serverWorkPath = Path.Combine(serverRootPath, "Normal_Crop");
            targetPath = Path.Combine(targetRootPath, "Normal_Crop");

            AddList(list, serverWorkPath, targetPath);
        }

        private void ListEnemy(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Images", "Enemy");
            string targetRootPath = Path.Combine(ETC.cachePath, "Enemy");

            // Enemy Normal Images
            string serverWorkPath = Path.Combine(serverRootPath, "Normal");
            string targetPath = Path.Combine(targetRootPath, "Normal");

            AddList(list, serverWorkPath, targetPath);

            // Enemy Crop Images
            serverWorkPath = Path.Combine(serverRootPath, "Normal_Crop");
            targetPath = Path.Combine(targetRootPath, "Normal_Crop");

            AddList(list, serverWorkPath, targetPath);
        }

        private void ListFST(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Images", "FST");
            string targetRootPath = Path.Combine(ETC.cachePath, "FST");

            // FST Normal Images
            string serverWorkPath = Path.Combine(serverRootPath, "Normal");
            string targetPath = Path.Combine(targetRootPath, "Normal");

            AddList(list, serverWorkPath, targetPath);

            // FST Crop Images
            serverWorkPath = Path.Combine(serverRootPath, "Normal_Crop");
            targetPath = Path.Combine(targetRootPath, "Normal_Crop");

            AddList(list, serverWorkPath, targetPath);

            // FST Icon Images
            serverWorkPath = Path.Combine(serverRootPath, "Normal_Icon");
            targetPath = Path.Combine(targetRootPath, "Normal_Icon");

            AddList(list, serverWorkPath, targetPath);
        }

        private void ListCoalition(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Images", "Coalition");
            string targetRootPath = Path.Combine(ETC.cachePath, "Coalition");

            // Coalition Normal Images
            string serverWorkPath = Path.Combine(serverRootPath, "Normal");
            string targetPath = Path.Combine(targetRootPath, "Normal");

            AddList(list, serverWorkPath, targetPath);

            // Coalition Crop Images
            serverWorkPath = Path.Combine(serverRootPath, "Normal_Crop");
            targetPath = Path.Combine(targetRootPath, "Normal_Crop");

            AddList(list, serverWorkPath, targetPath);

            // Coalition BG Images
            serverWorkPath = Path.Combine(serverRootPath, "BG");
            targetPath = Path.Combine(targetRootPath, "BG");

            AddList(list, serverWorkPath, targetPath);
        }

        private void ListSkill(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Images", "SkillIcons");
            string targetRootPath = Path.Combine(ETC.cachePath, "Skill");

            // Skill Icon Images
            string serverWorkPath = Path.Combine(serverRootPath);
            string targetPath = Path.Combine(targetRootPath);

            AddList(list, serverWorkPath, targetPath);
        }

        private void ListOldGFD(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Images", "OldGFD", "Images");
            string targetRootPath = Path.Combine(ETC.cachePath, "OldGFD", "Images");
            string lang;

            if (ETC.locale.Language == "ko")
            {
                lang = "ko";
            }
            else
            {
                lang = "en";
            }

            // OldGFD Images
            string serverWorkPath = Path.Combine(serverRootPath, lang);
            string targetPath = Path.Combine(targetRootPath);

            AddList(list, serverWorkPath, targetPath, "gfdcache", $"{lang}_");

            // OldGFD Version
            serverWorkPath = Path.Combine(ETC.server, "OldGFDVer.txt");
            targetPath = Path.Combine(ETC.systemPath, "OldGFDVer.txt");

            list.Add((serverWorkPath, targetPath));
        }

        private void ListGuideBook(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "PDF", "ShortGuideBook", "Image");
            string targetRootPath = Path.Combine(ETC.cachePath, "GuideBook", "Images");
            string lang;

            if (ETC.locale.Language == "ko")
            {
                lang = "ko";
            }
            else
            {
                lang = "en";
            }

            // GuideBook Images
            string serverWorkPath = Path.Combine(serverRootPath, lang);
            string targetPath = Path.Combine(targetRootPath);

            AddList(list, serverWorkPath, targetPath, "gfdcache", $"{lang}_");

            // GuideBook Version
            serverWorkPath = Path.Combine(ETC.server, "ShortGuideVer.txt");
            targetPath = Path.Combine(ETC.systemPath, "ShortGuideVer.txt");

            list.Add((serverWorkPath, targetPath));
        }

        private void ListStory(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Text", "Story", "ko");
            string targetRootPath = Path.Combine(ETC.cachePath, "Story");

            foreach (Match match in GetServerFiles(serverRootPath))
            {
                if (CheckParentDir(match))
                {
                    continue;
                }

                string top = GetMatchValue(match);
                string topPath = Path.Combine(serverRootPath, top);

                foreach (Match match2 in GetServerFiles(topPath))
                {
                    if (CheckParentDir(match2))
                    {
                        continue;
                    }

                    string category = GetMatchValue(match2);
                    string serverWorkPath = Path.Combine(topPath, category);
                    string targetPath = Path.Combine(targetRootPath, category);

                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    AddList(list, serverWorkPath, targetPath);
                }
            }
        }

        private void ListPV(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Video", "PV");
            string targetRootPath = Path.Combine(ETC.cachePath, "Video", "PV");

            // PV Videos
            string serverWorkPath = Path.Combine(serverRootPath);
            string targetPath = Path.Combine(targetRootPath);

            AddList(list, serverWorkPath, targetPath, "mp4");
        }

        private void ListMusic(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Music", "OST");
            string targetRootPath = Path.Combine(ETC.cachePath, "OST");

            foreach (Match match in GetServerFiles(serverRootPath))
            {
                if (CheckParentDir(match))
                {
                    continue;
                }

                string category = GetMatchValue(match);
                string serverWorkPath = Path.Combine(serverRootPath, category);
                string targetPath = Path.Combine(targetRootPath);

                AddList(list, serverWorkPath, targetPath, "mp3");
            }
        }

        /*private void ListCartoon(List<(string source, string target)> list)
        {
            string serverRootPath = Path.Combine(ETC.server, "Data", "Images", "Cartoon", "ko");
            string targetRootPath = Path.Combine(ETC.cachePath, "Cartoon");

            foreach (Match match in GetServerFiles(serverRootPath))
            {
                if (CheckParentDir(match))
                {
                    continue;
                }

                string top = GetMatchValue(match);
                string topPath = Path.Combine(serverRootPath, top);

                foreach (Match match2 in GetServerFiles(topPath))
                {
                    if (CheckParentDir(match2))
                    {
                        continue;
                    }

                    string category = GetMatchValue(match2);
                    string serverWorkPath = Path.Combine(topPath, category);
                    string targetPath = Path.Combine(targetRootPath, category);

                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    AddList(list, serverWorkPath, targetPath);
                }
            }
        }*/
    }
}