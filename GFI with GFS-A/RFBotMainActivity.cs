using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "라플봇", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class RFBotMainActivity : AppCompatActivity
    {
        enum DataRowType { Doll, Equip, Fairy, Enemy }

        private TextView StatusText;
        private EditText InputText;
        private Button InputButton;

        private CoordinatorLayout SnackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.RFBotLayout);

            StatusText = FindViewById<TextView>(Resource.Id.RFBotStatusText);
            InputText = FindViewById<EditText>(Resource.Id.RFBotInputText);
            InputButton = FindViewById<Button>(Resource.Id.RFBotInputButton);
            InputButton.Click += InputButton_Click;
            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.RFBotSnackbarLayout);
        }

        private async void InputButton_Click(object sender, EventArgs e)
        {
            int Input_DollNum = 0;

            try
            {
                InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);
                imm.HideSoftInputFromWindow(InputText.WindowToken, 0);

                InputText.Enabled = false;
                string[] Command = InputText.Text.Split(' ');

                if (InputText.Text == "connect to Zina OS")
                {
                    StartOS();
                }
                else if (Command[0] == "Hidden")
                {
                    InputText.Text = "";

                    await AnimateText("## Hidden OS Connect Code Hint ##\n\n", 10, true);

                    await Task.Delay(1000);
                }
                else if (int.TryParse(Command[0], out Input_DollNum) == true)
                {
                    DataRow dr = ETC.FindDataRow(ETC.DollList, "DicNumber", Input_DollNum);

                    if (Command.Length >= 2)
                    {
                        if (Command[1] == "상세정보") OpenDollDetailActivity((string)dr["Name"]);
                    }
                    else
                    {
                        if (dr == null) await AnimateText("입력한 도감번호에 해당하는 인형 정보가 없습니다.", 10, true);
                        else
                        {
                            string info = CombineDollInfo(dr);
                            await AnimateText(info, 10, true);
                        }
                    }
                }
                else if (Command[0] == "인형제조")
                {
                    int Input_ProductTime = 0;
                    if ((int.TryParse(Command[1], out Input_ProductTime) == false) || (Command[1].Length > 4) || (Command[1].Length < 3)) await AnimateText("입력한 제조시간 형식이 잘못되었습니다.", 10, true);
                    else
                    {
                        string DollInfo = "";
                        int ProductTime = ((Input_ProductTime / 100) * 60) + (Input_ProductTime % 100);

                        DataRow DollDR = ETC.FindDataRow(ETC.DollList, "ProductTime", ProductTime);

                        if (DollDR != null) DollInfo = CombineDollInfo(DollDR);

                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(DollInfo);

                        await AnimateText(sb.ToString(), 10, true);
                    }
                }
                else if (Command[0] == "장비제조")
                {
                    int Input_ProductTime = 0;
                    if ((int.TryParse(Command[1], out Input_ProductTime) == false) || (Command[1].Length > 4) || (Command[1].Length < 3)) await AnimateText("입력한 제조시간 형식이 잘못되었습니다.", 10, true);
                    else
                    {
                        DataRow InfoDR = null;
                        string Info = "";
                        int ProductTime = ((Input_ProductTime / 100) * 60) + (Input_ProductTime % 100);
                        DataRowType Type = DataRowType.Equip;
                        bool IsDetail = false;

                        if (Command.Length >= 3) IsDetail = (Command[2] == "상세정보") ? true : false;
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

                                await AnimateText(Info, 10, true);
                            }
                        }
                        else await AnimateText("해당 제조시간에 맞는 장비/요정이 없습니다.", 10, true);
                    }
                }
                else
                {
                    await AnimateText("잘못된 명령어를 입력하셨습니다.", 10, true);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.RFBot_DisunderstandInput, Snackbar.LengthShort, Android.Graphics.Color.Coral);
            }
            finally
            {
                InputText.Text = "";
                InputText.Enabled = true;
            }
        }

        private string CombineDollInfo(DataRow DollDR)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                int Grade = (int)DollDR["Grade"];

                sb.Append("* 인형 요약 정보 *\n\n");
                sb.AppendFormat(" # 이름 : {0}\n\n", (string)DollDR["Name"]);
                sb.AppendFormat(" # 별명 : {0}\n\n", (string)DollDR["NickName"]);
                sb.AppendFormat(" # 등급 : {0}\n\n", (Grade == 0) ? "Extra" : Grade.ToString());
                sb.AppendFormat(" # 도감 번호 : {0}\n\n", (int)DollDR["DicNumber"]);
                sb.AppendFormat(" # 총기 종류 : {0}\n\n", (string)DollDR["Type"]);
                sb.AppendFormat(" # 제조 시간 : {0}\n\n", ETC.CalcTime((int)DollDR["ProductTime"]));
                sb.AppendFormat(" # 스킬명 : {0}\n\n", (string)DollDR["Skill"]);
                sb.AppendFormat(" # 스킬설명 : {0}\n\n", (string)DollDR["SkillExplain"]);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                return "인형 데이터 참조 오류";
            }
        }

        private string CombineEquipInfo(DataRow EquipDR)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                int Grade = (int)EquipDR["Grade"];

                sb.Append("* 장비 요약 정보 *\n\n");
                sb.AppendFormat(" # 이름 : {0}\n\n", (string)EquipDR["Name"]);
                sb.AppendFormat(" # 등급 : {0}\n\n", (Grade == 0) ? "Extra" : Grade.ToString());
                sb.AppendFormat(" # 장비 분류 : {0}\n\n", (string)EquipDR["Category"]);
                sb.AppendFormat(" # 장비 종류 : {0}\n\n", (string)EquipDR["Type"]);
                sb.AppendFormat(" # 제조 시간 : {0}\n\n", ETC.CalcTime((int)EquipDR["ProductTime"]));

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                return "장비 데이터 참조 오류";
            }
        }

        private string CombineFairyInfo(DataRow FairyDR)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                sb.Append("* 요정 요약 정보 *\n\n");
                sb.AppendFormat(" # 이름 : {0}\n\n", (string)FairyDR["Name"]);
                sb.AppendFormat(" # 도감 번호 : {0}\n\n", (int)FairyDR["DicNumber"]);
                sb.AppendFormat(" # 요정 종류 : {0}\n\n", (string)FairyDR["Type"]);
                sb.AppendFormat(" # 제조 시간 : {0}\n\n", ETC.CalcTime((int)FairyDR["ProductTime"]));
                sb.AppendFormat(" # 스킬명 : {0}\n\n", (string)FairyDR["SkillName"]);
                sb.AppendFormat(" # 스킬설명 : {0}\n\n", (string)FairyDR["SkillExplain"]);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                return "요정 데이터 참조 오류";
            }
        }

        private void OpenDollDetailActivity(string DollName)
        {
            var DollInfo = new Intent(this, typeof(DollDBDetailActivity));
            DollInfo.PutExtra("Keyword", DollName);
            StartActivity(DollInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void OpenEquipDetailActivity(string EquipName)
        {
            var EquipInfo = new Intent(this, typeof(EquipDBDetailActivity));
            EquipInfo.PutExtra("Keyword", EquipName);
            StartActivity(EquipInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void OpenFairyDetailActivity(string FairyName)
        {
            var FairyInfo = new Intent(this, typeof(FairyDBDetailActivity));
            FairyInfo.PutExtra("Keyword", FairyName);
            StartActivity(FairyInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private async Task StartOS()
        {
            try
            {
                Random R = new Random(DateTime.Now.Second);

                StatusText.Text = "";
                await AnimateText("Connecting 127.0.0.1", ".", R.Next(3, 6), 500);

                await Task.Delay(1000);

                EnterZinaOS();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }

        private void EnterZinaOS()
        {
            StartActivity(typeof(ZinaOS));
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }

        private async Task AnimateText(string message, int delay, bool BeforeClear)
        {
            if (BeforeClear == true) StatusText.Text = "";

            for (int i = 0; i < message.Length; ++i)
            {
                StatusText.Text += message[i];
                await Task.Delay(delay);
            }
        }

        private async Task AnimateText(string message, string loopMessage, int loopCount, int delay)
        {
            StatusText.Text += message;

            for (int i = 0; i < loopCount; ++i)
            {
                StatusText.Text += loopMessage;
                await Task.Delay(delay);
            }

            StatusText.Text += "\n";
        }

        private async Task AnimateText(string message, int loopCount, int delay, string aftermessage)
        {
            StatusText.Text += message;

            for (int i = 0; i < loopCount; ++i)
            {
                StatusText.Text += '.';
                await Task.Delay(delay);
            }

            StatusText.Text += aftermessage + "\n";
        }
    }
}