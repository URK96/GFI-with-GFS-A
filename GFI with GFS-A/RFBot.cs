using Android.App;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    internal class RFBot
    {
        enum DataRowType { Doll, Equip, Fairy, Enemy }

        private string Command { get; set; }
        private Activity activity { get; set; }

        internal RFBot()
        {
            Command = "NULL";
            activity = null;
        }

        internal RFBot(Activity context)
        {
            activity = context;
        }

        internal string InputCommand(string command)
        {
            Command = command;

            return AnalysisCommand();
        }

        private string AnalysisCommand()
        {
            string result = "";

            string[] Command_Split = Command.Split(' ');

            if (int.TryParse(Command_Split[0], out int DicNumber) == true)
            {
                if (Command_Split.Length >= 2)
                {
                    if (Command_Split[1] == ETC.Resources.GetString(Resource.String.RFBotMain_Command_Detail)) OpenDollDetailActivity(DicNumber);
                }
                else
                {
                    DataRow dr = ETC.FindDataRow(ETC.DollList, "DicNumber", DicNumber);

                    if (dr == null) result = "RFBot Error : No T-Doll in match number";
                    else result = CombineDollInfo(dr);
                }
            }
            else if (Command_Split[0] == ETC.Resources.GetString(Resource.String.RFBotMain_Command_DollProduct))
            {
                if ((int.TryParse(Command_Split[1], out int Input_ProductTime) == false) || (Command_Split[1].Length > 4) || (Command_Split[1].Length < 3)) result = "입력한 제조시간 형식이 잘못되었습니다.";
                else
                {
                    string DollInfo = "";
                    int ProductTime = (Input_ProductTime / 100 * 60) + (Input_ProductTime % 100);

                    List<DataRow> DollDRs = new List<DataRow>();

                    foreach (DataRow dr in ETC.DollList.Rows)
                    {
                        if ((int)dr["ProductTime"] == ProductTime) DollDRs.Add(dr);
                    }

                    DollDRs.TrimExcess();

                    if (DollDRs.Count == 0) return ETC.Resources.GetString(Resource.String.RFBotMain_Reply_DollProductTimeNotMatch);

                    StringBuilder sb = new StringBuilder();

                    foreach (DataRow dr in DollDRs)
                    {
                        if (dr != null) DollInfo = CombineDollInfo(dr);
                        else return ETC.Resources.GetString(Resource.String.RFBotMain_Reply_DollProductTimeNotMatch);

                        sb.AppendLine(DollInfo);
                        sb.Append("\n\n");
                    }

                    result = sb.ToString();
                }
            }
            else if (Command_Split[0] == ETC.Resources.GetString(Resource.String.RFBotMain_Command_EquipProduct))
            {
                if ((int.TryParse(Command_Split[1], out int Input_ProductTime) == false) || (Command_Split[1].Length > 4) || (Command_Split[1].Length < 3)) result = "입력한 제조시간 형식이 잘못되었습니다.";
                else
                {
                    int ProductTime = ((Input_ProductTime / 100) * 60) + (Input_ProductTime % 100);
                    List<DataRow> InfoDRs = new List<DataRow>();
                    List<DataRowType> InfoTypes = new List<DataRowType>();

                    foreach (DataRow dr in ETC.EquipmentList.Rows)
                    {
                        if ((int)dr["ProductTime"] == ProductTime)
                        {
                            InfoDRs.Add(dr);
                            InfoTypes.Add(DataRowType.Equip);
                        }
                    }
                    foreach (DataRow dr in ETC.FairyList.Rows)
                    {
                        if ((int)dr["ProductTime"] == ProductTime)
                        {
                            InfoDRs.Add(dr);
                            InfoTypes.Add(DataRowType.Fairy);
                        }
                    }

                    InfoDRs.TrimExcess();
                    InfoTypes.TrimExcess();

                    if (InfoDRs.Count == 0) return ETC.Resources.GetString(Resource.String.RFBotMain_Reply_EquipProductTimeNotMatch);

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < InfoDRs.Count; ++i)
                    {
                        string Info = "";

                        switch (InfoTypes[i])
                        {
                            case DataRowType.Equip:
                                Info = CombineEquipInfo(InfoDRs[i]);
                                break;
                            case DataRowType.Fairy:
                                Info = CombineFairyInfo(InfoDRs[i]);
                                break;
                        }

                        sb.AppendLine(Info);
                        sb.Append("\n\n");
                    }

                    result = sb.ToString();
                }
            }
            else if (Command_Split[0] == ETC.Resources.GetString(Resource.String.Common_TDoll))
            {
                List<DataRow> DollDRs = new List<DataRow>();

                CheckName(ETC.DollList, ref DollDRs, Command_Split[1]);

                if (DollDRs.Count >= 2)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(ETC.Resources.GetString(Resource.String.RFBotMain_MultipleResult));
                    sb.Append("\n\n");

                    foreach (DataRow dr in DollDRs) sb.AppendLine((string)dr["Name"]);

                    result = sb.ToString();
                }
                else
                {
                    DataRow DollDR = DollDRs[0];

                    if (Command_Split.Length >= 3)
                    {
                        if (Command_Split[2] == ETC.Resources.GetString(Resource.String.RFBotMain_Command_Detail)) OpenDollDetailActivity((int)DollDR["DicNumber"]);
                    }
                    else
                    {
                        if (DollDR == null) result = "RFBot Error : No T-Doll in match number";
                        else result = CombineDollInfo(DollDR);
                    }
                }
            }
            else if (Command_Split[0] == ETC.Resources.GetString(Resource.String.Common_Equipment))
            {
                List<DataRow> EquipDRs = new List<DataRow>();

                CheckName(ETC.EquipmentList, ref EquipDRs, Command_Split[1]);

                if (EquipDRs.Count >= 2)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(ETC.Resources.GetString(Resource.String.RFBotMain_MultipleResult));
                    sb.Append("\n\n");

                    foreach (DataRow dr in EquipDRs) sb.AppendLine((string)dr["Name"]);

                    result = sb.ToString();
                }
                else
                {
                    DataRow EquipDR = EquipDRs[0];

                    if (Command_Split.Length >= 3)
                    {
                        if (Command_Split[2] == ETC.Resources.GetString(Resource.String.RFBotMain_Command_Detail)) OpenEquipDetailActivity((string)EquipDR["Name"]);
                    }
                    else
                    {
                        if (EquipDR == null) result = "RFBot Error : No Equipment in match number";
                        else result = CombineEquipInfo(EquipDR);
                    }
                }
            }
            else if (Command_Split[0] == ETC.Resources.GetString(Resource.String.Common_Fairy))
            {
                List<DataRow> FairyDRs = new List<DataRow>();

                CheckName(ETC.FairyList, ref FairyDRs, Command_Split[1]);

                if (FairyDRs.Count >= 2)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(ETC.Resources.GetString(Resource.String.RFBotMain_MultipleResult));
                    sb.Append("\n\n");

                    foreach (DataRow dr in FairyDRs) sb.AppendLine((string)dr["Name"]);

                    result = sb.ToString();
                }
                else
                {
                    DataRow FairyDR = FairyDRs[0];

                    if (Command_Split.Length >= 3)
                    {
                        if (Command_Split[2] == ETC.Resources.GetString(Resource.String.RFBotMain_Command_Detail)) OpenEquipDetailActivity((string)FairyDR["Name"]);
                    }
                    else
                    {
                        if (FairyDR == null) result = "RFBot Error : No Fairy in match number";
                        else result = CombineFairyInfo(FairyDR);
                    }
                }
            }
            else result = ETC.Resources.GetString(Resource.String.RFBotMain_Reply_CommandNotMatch);

            return result;
        }

        private void CheckName(DataTable TargetTable, ref List<DataRow> List, string Name)
        {
            List.Clear();

            foreach (DataRow dr in TargetTable.Rows)
            {
                string index_name = ((string)dr["Name"]).Replace(" ", "").ToLower();

                if (index_name == Name.ToLower())
                {
                    List.Clear();
                    List.Add(dr);
                    break;
                }

                if (index_name.Contains(Name.ToLower()) == true) List.Add(dr);
            }

            List.TrimExcess();
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