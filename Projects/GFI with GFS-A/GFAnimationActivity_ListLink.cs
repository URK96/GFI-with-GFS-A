using System.Collections.Generic;

namespace GFI_with_GFS_A
{
    public partial class GFAnimationActivity : BaseAppCompatActivity
    {
        private void ListLink(int position, Dictionary<string, string> linkDic)
        {
            string langCN = Resources.GetString(Resource.String.GFAnimation_Language_Chinese);
            string langJP = Resources.GetString(Resource.String.GFAnimation_Language_Japanese);
            string langKR = Resources.GetString(Resource.String.GFAnimation_Language_Korean);

            linkDic.Clear();

            switch (position)
            {
                // Healing Total Episode
                case 0:
                    linkDic.Add(langJP, "https://youtu.be/O10fTQ-z1lA");
                    break;
                case 1:
                    linkDic.Add(langCN, "https://youtu.be/4um2ipxUWRM");
                    linkDic.Add(langJP, "https://youtu.be/QW8jhMSZMuU");
                    linkDic.Add(langKR, "https://youtu.be/gqjX7hf4AAk");
                    break;
                case 2:
                    linkDic.Add(langCN, "https://youtu.be/AvkRH5A5vYw");
                    linkDic.Add(langJP, "https://youtu.be/ywTscnvs7Qk");
                    linkDic.Add(langKR, "https://youtu.be/NTUefb0tS-Y");
                    break;
                case 3:
                    linkDic.Add(langCN, "https://youtu.be/Wag-1xX7ChM");
                    linkDic.Add(langJP, "https://youtu.be/wwV-fnZBM_U");
                    linkDic.Add(langKR, "https://youtu.be/odGYPY4QCoY");
                    break;
                case 4:
                    linkDic.Add(langCN, "https://youtu.be/UKdvfpnoTkE");
                    linkDic.Add(langJP, "https://youtu.be/mWSN86FavZo");
                    linkDic.Add(langKR, "https://youtu.be/bNrEtf6ndNc");
                    break;
                case 5:
                    linkDic.Add(langCN, "https://youtu.be/0YEiEMaigFQ");
                    linkDic.Add(langJP, "https://youtu.be/5rIlQi-eXCE");
                    linkDic.Add(langKR, "https://youtu.be/7j5RMsgDflk");
                    break;
                case 6:
                    linkDic.Add(langCN, "https://youtu.be/QRU575czX0w");
                    linkDic.Add(langJP, "https://youtu.be/AlPl0c-HtQ0");
                    linkDic.Add(langKR, "https://youtu.be/Uq3MXl0GMmA");
                    break;
                case 7:
                    linkDic.Add(langCN, "https://youtu.be/S3E6WKv9URI");
                    linkDic.Add(langJP, "https://youtu.be/cA0nwi_T8bg");
                    linkDic.Add(langKR, "https://youtu.be/xYQkuaUQ_s4");
                    break;
                case 8:
                    linkDic.Add(langCN, "https://youtu.be/5V4cC8vtqk4");
                    linkDic.Add(langJP, "https://youtu.be/T12i2xhiqvo");
                    linkDic.Add(langKR, "https://youtu.be/Evh1bduF9oY");
                    break;
                case 9:
                    linkDic.Add(langCN, "https://youtu.be/_HwhgdMw8AM");
                    linkDic.Add(langJP, "https://youtu.be/bsx_o0hJXus");
                    linkDic.Add(langKR, "https://youtu.be/nSxfjgQrTwk");
                    break;
                case 10:
                    linkDic.Add(langCN, "https://youtu.be/GpEUaaJ8pk8");
                    linkDic.Add(langJP, "https://youtu.be/zjQAFJYIK_Y");
                    linkDic.Add(langKR, "https://youtu.be/aEB8F-9Icr4");
                    break;
                case 11:
                    linkDic.Add(langCN, "https://youtu.be/ZLuy-hVS-SM");
                    linkDic.Add(langJP, "https://youtu.be/5x20KQlIftU");
                    break;
                case 12:
                    linkDic.Add(langCN, "https://youtu.be/gyLAlAN2MQs");
                    linkDic.Add(langJP, "https://youtu.be/Jlg5NUiOjgs");
                    break;
                // Interview
                case 13:
                    linkDic.Add(langCN, "https://youtu.be/rBZrAih0aP8");
                    break;
                // Mad Total Episode
                case 14:
                    linkDic.Add(langJP, "https://youtu.be/Ge-R-l-7n7A");
                    break;
                case 15:
                    linkDic.Add(langCN, "https://youtu.be/xYYwrL0PKJo");
                    linkDic.Add(langJP, "https://youtu.be/NEoVuqCTyOE");
                    break;
                case 16:
                    linkDic.Add(langCN, "https://youtu.be/Lr4UbbTngO8");
                    linkDic.Add(langJP, "https://youtu.be/AciMnWGEa4E");
                    break;
                case 17:
                    linkDic.Add(langCN, "https://youtu.be/X3kjskdfYZo");
                    linkDic.Add(langJP, "https://youtu.be/xdxF0oKlyM0");
                    break;
                case 18:
                    linkDic.Add(langCN, "https://youtu.be/zMJZgaYmZK0");
                    linkDic.Add(langJP, "https://youtu.be/uq_N_mo9h2Q");
                    break;
                case 19:
                    linkDic.Add(langCN, "https://youtu.be/RIqcwlQvOac");
                    linkDic.Add(langJP, "https://youtu.be/Obc9ll4Ki3Y");
                    break;
                case 20:
                    linkDic.Add(langCN, "https://youtu.be/Ln3MOULLBbo");
                    linkDic.Add(langJP, "https://youtu.be/24NGiIDc_Jo");
                    break;
                case 21:
                    linkDic.Add(langCN, "https://youtu.be/QPCQ7Hdsazk");
                    linkDic.Add(langJP, "https://youtu.be/Paf3rkkJbqc");
                    break;
                case 22:
                    linkDic.Add(langCN, "https://youtu.be/OII_Sjky-j8");
                    linkDic.Add(langJP, "https://youtu.be/LrZLbGpsU_g");
                    break;
                case 23:
                    linkDic.Add(langCN, "https://youtu.be/h8ctC9pJJY0");
                    linkDic.Add(langJP, "https://youtu.be/_pP6ES95H5E");
                    break;
                case 24:
                    linkDic.Add(langCN, "https://youtu.be/Irlwh2E9OJU");
                    linkDic.Add(langJP, "https://youtu.be/WG7K1yLFnB4");
                    break;
                case 25:
                    linkDic.Add(langCN, "https://youtu.be/tGq3arGs4kA");
                    linkDic.Add(langJP, "https://youtu.be/crmt_eNbXX0");
                    break;
                case 26:
                    linkDic.Add(langCN, "https://youtu.be/BtccwUu56B4");
                    linkDic.Add(langJP, "https://youtu.be/hViLBmIwrZA");
                    break;
            }
        }
    }
}