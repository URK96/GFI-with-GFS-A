using Android.App;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UptimeSharp;

namespace GFI_with_GFS_A
{
    internal class RFBot
    {
        enum DataRowType { Doll, Equip, Fairy, Enemy }

        private string Command { get; set; }
        private Activity activity { get; set; }
        private bool IsNotification = false;

        internal RFBot(bool IsNotify)
        {
            Command = "NULL";
            activity = null;
            IsNotification = IsNotify;
        }

        internal RFBot(bool IsNotify, Activity context)
        {
            IsNotification = IsNotify;
            activity = context;
        }

        private string AnalysisCommand()
        {
            string result = "";

            string[] Command_Split = Command.Split(' ');

            if (int.TryParse(Command_Split[0], out int DicNumber) == true)
            {
                if (Command.Length >= 2)
                {
                    if (Command_Split[1] == "상세정보") OpenDollDetailActivity(DicNumber);
                }
                else
                {
                    DataRow dr = ETC.FindDataRow(ETC.DollList, "DicNumber", DicNumber);

                    if (dr == null) result = "입력한 도감번호에 해당하는 인형 정보가 없습니다.";
                    else result = CombineDollInfo(dr);
                }
            }
            else if (Command_Split[0] == ETC.Resources.GetString(Resource.String.RFBotMain_Command_DollProduct))
            {
                if ((int.TryParse(Command_Split[1], out int Input_ProductTime) == false) || (Command_Split[1].Length > 4) || (Command_Split[1].Length < 3)) result = "입력한 제조시간 형식이 잘못되었습니다.";
                else
                {
                    string DollInfo = "";
                    int ProductTime = ((Input_ProductTime / 100) * 60) + (Input_ProductTime % 100);

                    DataRow DollDR = ETC.FindDataRow(ETC.DollList, "ProductTime", ProductTime);

                    if (DollDR != null) DollInfo = CombineDollInfo(DollDR);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(DollInfo);

                    result = sb.ToString();
                }
            }
            else if (Command_Split[0] == ETC.Resources.GetString(Resource.String.RFBotMain_Command_EquipProduct))
            {
                if ((int.TryParse(Command_Split[1], out int Input_ProductTime) == false) || (Command_Split[1].Length > 4) || (Command_Split[1].Length < 3)) result = "입력한 제조시간 형식이 잘못되었습니다.";
                else
                {
                    DataRow InfoDR = null;
                    string Info = "";
                    int ProductTime = ((Input_ProductTime / 100) * 60) + (Input_ProductTime % 100);
                    DataRowType Type = DataRowType.Equip;
                    bool IsDetail = false;

                    if (Command.Length >= 3) IsDetail = (Command_Split[2] == ETC.Resources.GetString(Resource.String.RFBotMain_Command_Detail)) ? true : false;
                    else IsDetail = false;

                    InfoDR = ETC.FindDataRow(ETC.EquipmentList, "ProductTime", ProductTime);
                    if (InfoDR == null)
                    {
                        InfoDR = ETC.FindDataRow(ETC.FairyList, "ProductTime", ProductTime);
                        Type = DataRowType.Fairy;
                    }

                    if (InfoDR != null)
                    {
                        if (IsDetail == true)
                        {
                            switch (Type)
                            {
                                case DataRowType.Equip:
                                    OpenEquipDetailActivity((string)InfoDR["Name"]);
                                    break;
                                case DataRowType.Fairy:
                                    OpenFairyDetailActivity((string)InfoDR["Name"]);
                                    break;
                            }
                        }
                        else
                        {
                            switch (Type)
                            {
                                case DataRowType.Equip:
                                    Info = CombineEquipInfo(InfoDR);
                                    break;
                                case DataRowType.Fairy:
                                    Info = CombineFairyInfo(InfoDR);
                                    break;
                            }

                            result = Info;
                        }
                    }
                    else result = ETC.Resources.GetString(Resource.String.RFBotMain_Reply_EquipProductTimeNotMatch);
                }
            }
            else result = ETC.Resources.GetString(Resource.String.RFBotMain_Reply_CommandNotMatch);

            return result;
        }

        private void OpenDollDetailActivity(int DollDicNum)
        {
            if (activity == null) return;

            var DollInfo = new Intent(activity, typeof(DollDBDetailActivity));
            DollInfo.PutExtra("Keyword", DollDicNum);
            activity.StartActivity(DollInfo);
            activity.OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void OpenEquipDetailActivity(string EquipName)
        {
            if (activity == null) return;

            var EquipInfo = new Intent(activity, typeof(EquipDBDetailActivity));
            EquipInfo.PutExtra("Keyword", EquipName);
            activity.StartActivity(EquipInfo);
            activity.OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void OpenFairyDetailActivity(string FairyName)
        {
            if (activity == null) return;

            var FairyInfo = new Intent(activity, typeof(FairyDBDetailActivity));
            FairyInfo.PutExtra("Keyword", FairyName);
            activity.StartActivity(FairyInfo);
            activity.OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private string CombineDollInfo(DataRow DollDR)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                int Grade = (int)DollDR["Grade"];

                sb.AppendFormat("* {0} *\n\n", ETC.Resources.GetString(Resource.String.Common_SkillExplain));
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_Name), (string)DollDR["Name"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_NickName), (string)DollDR["NickName"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_Grade), (Grade == 0) ? "Extra" : Grade.ToString());
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_DicNumber), (int)DollDR["DicNumber"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_GunType), (string)DollDR["Type"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_ProductTime), ETC.CalcTime((int)DollDR["ProductTime"]));
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_SkillName), (string)DollDR["Skill"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_SkillExplain), (string)DollDR["SkillExplain"]);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
                return ETC.Resources.GetString(Resource.String.RFBotMain_DollInfo_ReferDollInfoError);
            }
        }

        private string CombineEquipInfo(DataRow EquipDR)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                int Grade = (int)EquipDR["Grade"];

                sb.AppendFormat("* {0} *\n\n", ETC.Resources.GetString(Resource.String.RFBotMain_EquipInfo_Title));
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_Name), (string)EquipDR["Name"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_Grade), (Grade == 0) ? "Extra" : Grade.ToString());
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_EquipCategory), (string)EquipDR["Category"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_EquipType), (string)EquipDR["Type"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_ProductTime), ETC.CalcTime((int)EquipDR["ProductTime"]));

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
                return ETC.Resources.GetString(Resource.String.RFBotMain_EquipInfo_ReferEquipInfoError);
            }
        }

        private string CombineFairyInfo(DataRow FairyDR)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                sb.AppendFormat("* {0} *\n\n", ETC.Resources.GetString(Resource.String.RFBotMain_FairyInfo_Title));
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_Name), (string)FairyDR["Name"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_DicNumber), (int)FairyDR["DicNumber"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_FairyType), (string)FairyDR["Type"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_ProductTime), ETC.CalcTime((int)FairyDR["ProductTime"]));
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_SkillName), (string)FairyDR["SkillName"]);
                sb.AppendFormat(" # {0} : {1}\n\n", ETC.Resources.GetString(Resource.String.Common_SkillExplain), (string)FairyDR["SkillExplain"]);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
                return ETC.Resources.GetString(Resource.String.RFBotMain_FairyInfo_ReferFairyInfoError);
            }
        }
    }
}