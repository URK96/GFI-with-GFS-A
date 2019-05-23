using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;

namespace GFI_with_GFS_A
{
    public class ExpItemCalc : Android.Support.V4.App.Fragment
    {
        private View v;

        private readonly int[] LevelExp = { 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500, 5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000, 21000, 23100, 25300, 27600, 30000, 32500, 35100, 37900, 41000, 44400, 48600, 53200, 58200, 63600, 69400, 75700, 82400, 89600, 97300, 105500, 114300, 123600, 133500, 144000, 155100, 166900, 179400, 192500, 206400, 221000, 236400, 252500, 269400, 287100, 305700, 325200, 345600, 366900, 389200, 412500, 436800, 462100, 488400, 515800, 544300, 573900, 604700, 636700, 669900, 704300, 749400, 796200, 844800, 895200, 947400, 1001400, 1057300, 1115200, 1175000, 1236800, 1300700, 1366700, 1434800, 1505100, 1577700, 1652500, 1729600, 1809100, 1891000, 1975300, 2087900, 2204000, 2323500, 2446600, 2573300, 2703700, 2837800, 2975700, 3117500, 3263200, 3363200, 3483200, 3623200, 3783200, 3963200, 4163200, 4383200, 4623200, 4903200, 5263200, 5743200, 6383200, 7283200, 8483200, 10083200, 12283200, 15283200, 19283200, 24283200, 30283200 };
        private readonly int[] LevelExp_Fairy = { 0, 300, 900, 1800, 3000, 4500, 6300, 8400, 10800, 13500, 16500, 19800, 23400, 27300, 31500, 36000, 40800, 45900, 51400, 57400, 63900, 71000, 79000, 88000, 98000, 109000, 121200, 134600, 149300, 165300, 182800, 201700, 222200, 244400, 268300, 294000, 321600, 351100, 382700, 416400, 452300, 490500, 531000, 574000, 619500, 667700, 718600, 772300, 828900, 888500, 951200, 1017100, 1086300, 1158900, 1234900, 1314500, 1397800, 1484800, 1575700, 1670600, 1769600, 1872700, 1980100, 2091900, 2208200, 2329100, 2454700, 2585100, 2720400, 2860800, 3006300, 3157100, 3313200, 3474800, 3642000, 3814900, 3993600, 4178300, 4369000, 4565900, 4769100, 4978700, 5194800, 5417600, 5647200, 5883700, 6127200, 6377800, 6635700, 6901000, 7173800, 7454200, 7742400, 8038500, 8342600, 8654900, 8975500, 9304500, 9642000, 9999000 };
        private readonly int[] LevelExp_FST = { 0, 500, 1400, 2700, 4500, 6700, 9400, 12600, 16200, 20200, 24700, 29700, 35100, 40900, 47200, 54000, 61200, 68800, 77100, 86100, 95900, 106500, 118500, 132000, 147000, 163500, 181800, 201900, 223900, 247900, 274200, 302500, 333300, 366600, 402400, 441000, 482400, 526600, 574000, 624600, 678400, 735700, 796500, 861000, 929200, 1001500, 1077900, 1158400, 1243300, 1332700, 1426800, 1525600, 1629400, 1738300, 1852300, 1971800, 2096700, 2227200, 2363500, 2505900, 2654400, 2809000, 2970100, 3137800, 3312300, 3493800, 3682300, 3877800, 4080800, 4291400, 4509600, 4735800, 4970000, 5212500, 5463300, 5722800, 5990800, 6267800, 6553800, 6849300, 7154000, 7468500, 7792500, 8127000, 8471000, 8826000, 9191000, 9567000, 9954000, 10352000, 10761000, 11182000, 11614000, 12058000, 12514000, 12983000, 13464000, 13957000, 14463000, 15000000 };
        private readonly int[] ConsumeCount_FST = { 1, 1, 3, 5, 5, 7, 9, 9, 11, 13, 15 };

        private Spinner ExpTypeList;
        private ToggleButton ApplyMODModeSwitch;
        private ToggleButton ApplyVowSwitch;
        private NumberPicker StartLevel;
        private NumberPicker TargetLevel;
        private NumberPicker TrainerLevel;
        private EditText NowExp;
        private TextView Result_ExpItem;
        private TextView Result_RemainExp;
        private TextView Result_Time;
        private TextView Result_Battery;

        private bool IsVow = false;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_ExpItem, container, false);

            ExpTypeList = v.FindViewById<Spinner>(Resource.Id.CalcReportType);
            ExpTypeList.ItemSelected += ExpTypeList_ItemSelected;

            ApplyMODModeSwitch = v.FindViewById<ToggleButton>(Resource.Id.CalcReportModSelector);
            ApplyMODModeSwitch.CheckedChange += ApplyMODModeCheckBox_CheckedChange;
            ApplyVowSwitch = v.FindViewById<ToggleButton>(Resource.Id.CalcReportVowSelector);
            ApplyVowSwitch.CheckedChange += delegate
            {
                IsVow = ApplyVowSwitch.Checked;
                CalcReport(StartLevel.Value, TargetLevel.Value);
            };
            StartLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcReportStartLevel);
            StartLevel.ValueChanged += LevelSelector_ValueChanged;
            TargetLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcReportEndLevel);
            TargetLevel.ValueChanged += LevelSelector_ValueChanged;
            TrainerLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcReportTrainerLevel);
            TrainerLevel.ValueChanged += LevelSelector_ValueChanged;
            NowExp = v.FindViewById<EditText>(Resource.Id.CalcReportNowExp);
            NowExp.TextChanged += delegate { CalcReport(StartLevel.Value, TargetLevel.Value); };
            Result_ExpItem = v.FindViewById<TextView>(Resource.Id.CalcReportResultExpItem);
            Result_RemainExp = v.FindViewById<TextView>(Resource.Id.CalcReportResultRemainExp);
            Result_Time = v.FindViewById<TextView>(Resource.Id.CalcReportResultTime);
            Result_Battery = v.FindViewById<TextView>(Resource.Id.CalcReportResultBattery);

            DisplayMetrics dm = Context.Resources.DisplayMetrics;
            int width = dm.WidthPixels / 2;

            ApplyMODModeSwitch.LayoutParameters.Width = width;
            ApplyMODModeSwitch.LayoutParameters.Height = width;
            ApplyVowSwitch.LayoutParameters.Width = width;
            ApplyVowSwitch.LayoutParameters.Height = width;

            InitializeString();
            InitializeProcess();

            return v;
        }

        private void InitializeString()
        {
            Result_ExpItem.Text = $"0 {Resources.GetString(Resource.String.Calc_ExpItem_DefaultItemResultText)}";
            Result_RemainExp.Text = $"0 {Resources.GetString(Resource.String.Calc_ExpItem_DefaultRemainExpResultText)}";
            Result_Battery.Text = $"0 {Resources.GetString(Resource.String.Calc_ExpItem_DefaultBatteryResultText)}";
            Result_Time.Text = $"0 {Resources.GetString(Resource.String.Calc_ExpItem_DefaultTimeResultText)}";
        }

        private void ExpTypeList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                switch (e.Position)
                {
                    case 0:
                        ApplyMODModeSwitch.Visibility = ViewStates.Visible;
                        ApplyVowSwitch.Visibility = ViewStates.Visible;
                        Result_Time.Visibility = ViewStates.Gone;
                        v.FindViewById<LinearLayout>(Resource.Id.CalcReportTrainerLevelSettingLayout).Visibility = ViewStates.Gone;
                        break;
                    case 1:
                        ApplyMODModeSwitch.Checked = false;
                        ApplyMODModeSwitch.Visibility = ViewStates.Gone;
                        ApplyVowSwitch.Visibility = ViewStates.Gone;
                        Result_Time.Visibility = ViewStates.Gone;
                        v.FindViewById<LinearLayout>(Resource.Id.CalcReportTrainerLevelSettingLayout).Visibility = ViewStates.Gone;
                        break;
                    case 2:
                        ApplyMODModeSwitch.Checked = false;
                        ApplyMODModeSwitch.Visibility = ViewStates.Gone;
                        ApplyVowSwitch.Visibility = ViewStates.Gone;
                        Result_Time.Visibility = ViewStates.Visible;
                        v.FindViewById<LinearLayout>(Resource.Id.CalcReportTrainerLevelSettingLayout).Visibility = ViewStates.Visible;
                        break;
                }                
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }

            InitializeString();
        }

        private void InitializeProcess()
        {
            string[] TypeList = 
            {
                Resources.GetString(Resource.String.Common_TDoll),
                Resources.GetString(Resource.String.Common_Fairy),
                Resources.GetString(Resource.String.Common_FST)
            };

            var adapter = new ArrayAdapter(Activity, Resource.Layout.SpinnerListLayout, TypeList);
            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
            ExpTypeList.Adapter = adapter;

            StartLevel.MinValue = 1;
            StartLevel.MinValue = 1;
            StartLevel.Value = 1;
            TargetLevel.MinValue = 1;
            TargetLevel.MaxValue = 100;
            TargetLevel.Value = 1;
            TrainerLevel.MinValue = 0;
            TrainerLevel.MaxValue = 10;
            TrainerLevel.Value = 0;
            NowExp.Text = "0";
        }

        private void LevelSelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcReportStartLevel when ExpTypeList.SelectedItemPosition == 0:
                        TargetLevel.MinValue = e.NewVal;
                        NowExp.Text = LevelExp[np.Value - 1].ToString();
                        break;
                    case Resource.Id.CalcReportStartLevel when ExpTypeList.SelectedItemPosition == 1:
                        TargetLevel.MinValue = e.NewVal;
                        NowExp.Text = LevelExp_Fairy[np.Value - 1].ToString();
                        break;
                    case Resource.Id.CalcReportStartLevel when ExpTypeList.SelectedItemPosition == 2:
                        TargetLevel.MinValue = e.NewVal;
                        NowExp.Text = LevelExp_FST[np.Value - 1].ToString();
                        break;
                    case Resource.Id.CalcReportEndLevel:
                        StartLevel.MaxValue = e.NewVal;
                        break;
                }

                switch (ExpTypeList.SelectedItemPosition)
                {
                    case 0:
                        CalcReport(StartLevel.Value, TargetLevel.Value);
                        break;
                    case 1:
                        CalcReport_Fairy(StartLevel.Value, TargetLevel.Value);
                        break;
                    case 2:
                        CalcReport_FST(StartLevel.Value, TargetLevel.Value, TrainerLevel.Value);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        private void ApplyMODModeCheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;

            try
            {
                switch (tb.Checked)
                {
                    case true:
                        StartLevel.MaxValue = 100;
                        StartLevel.MinValue = 100;
                        StartLevel.Value = StartLevel.MinValue;
                        TargetLevel.MinValue = 100;
                        TargetLevel.MaxValue = 120;
                        TargetLevel.Value = TargetLevel.MinValue;
                        break;
                    case false:
                        StartLevel.MaxValue = 1;
                        StartLevel.MinValue = 1;
                        StartLevel.Value = StartLevel.MinValue;
                        TargetLevel.MinValue = 1;
                        TargetLevel.MaxValue = 100;
                        TargetLevel.Value = TargetLevel.MinValue;
                        break;
                }

                NowExp.Text = LevelExp[StartLevel.Value - 1].ToString();

                CalcReport(StartLevel.Value, TargetLevel.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        private void CalcReport(int start, int target)
        {
            const int ExpItem = 3000;

            try
            {
                int now_exp = 0;
                if (string.IsNullOrWhiteSpace(NowExp.Text) == false)
                    now_exp = int.Parse(NowExp.Text);
                else
                    now_exp = 0;

                int RequireExp = LevelExp[target - 1] - now_exp;
                int RequireExpItem = 0;
                int SurplusExp = 0;

                switch (IsVow)
                {
                    case false:
                        RequireExpItem = Convert.ToInt32(Math.Ceiling(RequireExp / Convert.ToDouble(ExpItem)));
                        SurplusExp = (ExpItem * RequireExpItem) - RequireExp;
                        break;
                    case true:
                        RequireExpItem = Convert.ToInt32(Math.Ceiling(RequireExp / Convert.ToDouble(ExpItem * 2)));
                        SurplusExp = (ExpItem * RequireExpItem * 2) - RequireExp;
                        break;
                }

                Result_ExpItem.Text = $"{RequireExpItem} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultItemResultText)}";
                Result_RemainExp.Text = $"{SurplusExp} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultRemainExpResultText)}";
                Result_Battery.Text = $"{RequireExpItem * 3} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultBatteryResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Short).Show();
            }
        }

        private void CalcReport_Fairy(int start, int target)
        {
            const int ExpItem = 3000;

            try
            {
                int now_exp = 0;
                if (string.IsNullOrWhiteSpace(NowExp.Text) == false)
                    now_exp = int.Parse(NowExp.Text);
                else
                    now_exp = 0;

                int RequireExp = LevelExp_Fairy[target - 1] - now_exp;
                int RequireExpItem = 0;
                int SurplusExp = 0;

                RequireExpItem = Convert.ToInt32(Math.Ceiling(RequireExp / Convert.ToDouble(ExpItem)));
                SurplusExp = (ExpItem * RequireExpItem) - RequireExp;

                Result_ExpItem.Text = $"{RequireExpItem} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultItemResultText)}";
                Result_RemainExp.Text = $"{SurplusExp} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultRemainExpResultText)}";
                Result_Battery.Text = $"{RequireExpItem * 3} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultBatteryResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Short).Show();
            }
        }

        private void CalcReport_FST(int start, int target, int trainer)
        {
            const int ExpItem = 3000;

            try
            {
                int now_exp = 0;
                if (string.IsNullOrWhiteSpace(NowExp.Text) == false)
                    now_exp = int.Parse(NowExp.Text);
                else
                    now_exp = 0;

                int RequireExp = LevelExp_FST[target - 1] - now_exp;
                int RequireExpItem = 0;
                int SurplusExp = 0;
                int RequireTime = 0;

                RequireExpItem = Convert.ToInt32(Math.Ceiling(RequireExp / (double)ExpItem));
                SurplusExp = (ExpItem * RequireExpItem) - RequireExp;
                RequireTime = Convert.ToInt32(Math.Ceiling(RequireExpItem / (double)ConsumeCount_FST[trainer]));

                Result_ExpItem.Text = $"{RequireExpItem} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultItemResultText)}";
                Result_RemainExp.Text = $"{SurplusExp} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultRemainExpResultText)}";
                Result_Battery.Text = $"{RequireExpItem * 5} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultBatteryResultText)}";
                Result_Time.Text = $"{RequireTime} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultTimeResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Short).Show();
            }
        }
    }
}