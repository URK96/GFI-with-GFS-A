using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;

namespace GFDA
{
    public class ExpItemCalc : AndroidX.Fragment.App.Fragment
    {
        private View v;

        // T-Doll Require Exp Between Level
        private readonly int[] levelExp = { 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500, 5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000, 21000, 23100, 25300, 27600, 30000, 32500, 35100, 37900, 41000, 44400, 48600, 53200, 58200, 63600, 69400, 75700, 82400, 89600, 97300, 105500, 114300, 123600, 133500, 144000, 155100, 166900, 179400, 192500, 206400, 221000, 236400, 252500, 269400, 287100, 305700, 325200, 345600, 366900, 389200, 412500, 436800, 462100, 488400, 515800, 544300, 573900, 604700, 636700, 669900, 704300, 749400, 796200, 844800, 895200, 947400, 1001400, 1057300, 1115200, 1175000, 1236800, 1300700, 1366700, 1434800, 1505100, 1577700, 1652500, 1729600, 1809100, 1891000, 1975300, 2087900, 2204000, 2323500, 2446600, 2573300, 2703700, 2837800, 2975700, 3117500, 3263200, 3363200, 3483200, 3623200, 3783200, 3963200, 4163200, 4383200, 4623200, 4903200, 5263200, 5743200, 6383200, 7283200, 8483200, 10083200, 12283200, 15283200, 19283200, 24283200, 30283200 };
        
        // Fairy Require Exp Between Level
        private readonly int[] levelExpFairy = { 0, 300, 900, 1800, 3000, 4500, 6300, 8400, 10800, 13500, 16500, 19800, 23400, 27300, 31500, 36000, 40800, 45900, 51400, 57400, 63900, 71000, 79000, 88000, 98000, 109000, 121200, 134600, 149300, 165300, 182800, 201700, 222200, 244400, 268300, 294000, 321600, 351100, 382700, 416400, 452300, 490500, 531000, 574000, 619500, 667700, 718600, 772300, 828900, 888500, 951200, 1017100, 1086300, 1158900, 1234900, 1314500, 1397800, 1484800, 1575700, 1670600, 1769600, 1872700, 1980100, 2091900, 2208200, 2329100, 2454700, 2585100, 2720400, 2860800, 3006300, 3157100, 3313200, 3474800, 3642000, 3814900, 3993600, 4178300, 4369000, 4565900, 4769100, 4978700, 5194800, 5417600, 5647200, 5883700, 6127200, 6377800, 6635700, 6901000, 7173800, 7454200, 7742400, 8038500, 8342600, 8654900, 8975500, 9304500, 9642000, 9999000 };

        // FST Require Exp Between Level
        private readonly int[] levelExpFST = { 0, 500, 1400, 2700, 4500, 6700, 9400, 12600, 16200, 20200, 24700, 29700, 35100, 40900, 47200, 54000, 61200, 68800, 77100, 86100, 95900, 106500, 118500, 132000, 147000, 163500, 181800, 201900, 223900, 247900, 274200, 302500, 333300, 366600, 402400, 441000, 482400, 526600, 574000, 624600, 678400, 735700, 796500, 861000, 929200, 1001500, 1077900, 1158400, 1243300, 1332700, 1426800, 1525600, 1629400, 1738300, 1852300, 1971800, 2096700, 2227200, 2363500, 2505900, 2654400, 2809000, 2970100, 3137800, 3312300, 3493800, 3682300, 3877800, 4080800, 4291400, 4509600, 4735800, 4970000, 5212500, 5463300, 5722800, 5990800, 6267800, 6553800, 6849300, 7154000, 7468500, 7792500, 8127000, 8471000, 8826000, 9191000, 9567000, 9954000, 10352000, 10761000, 11182000, 11614000, 12058000, 12514000, 12983000, 13464000, 13957000, 14463000, 15000000 };

        // T-Doll Require Exp Between Level
        private readonly int[] consumeCountFST = { 1, 1, 3, 5, 5, 7, 9, 9, 11, 13, 15 };

        private Spinner expTypeList;
        private ToggleButton applyMODModeSwitch;
        private ToggleButton applyVowSwitch;
        private NumberPicker startLevel;
        private NumberPicker targetLevel;
        private NumberPicker trainerLevel;
        private EditText nowExpEditText;
        private TextView resultExpItem;
        private TextView resultRemainExp;
        private TextView resultTime;
        private TextView resultBattery;

        private bool isVow = false;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_ExpItem, container, false);

            // Find View & Connect Event

            expTypeList = v.FindViewById<Spinner>(Resource.Id.CalcReportType);
            expTypeList.ItemSelected += ExpTypeList_ItemSelected;
            applyMODModeSwitch = v.FindViewById<ToggleButton>(Resource.Id.CalcReportModSelector);
            applyMODModeSwitch.CheckedChange += ApplyMODModeCheckBox_CheckedChange;
            applyVowSwitch = v.FindViewById<ToggleButton>(Resource.Id.CalcReportVowSelector);
            applyVowSwitch.CheckedChange += delegate
            {
                isVow = applyVowSwitch.Checked;
                CalcReport(targetLevel.Value);
            };
            startLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcReportStartLevel);
            startLevel.ValueChanged += LevelSelector_ValueChanged;
            targetLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcReportEndLevel);
            targetLevel.ValueChanged += LevelSelector_ValueChanged;
            trainerLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcReportTrainerLevel);
            trainerLevel.ValueChanged += LevelSelector_ValueChanged;
            nowExpEditText = v.FindViewById<EditText>(Resource.Id.CalcReportNowExp);
            nowExpEditText.TextChanged += delegate 
            {
                switch (expTypeList.SelectedItemPosition)
                {
                    case 0:
                        CalcReport(targetLevel.Value);
                        break;
                    case 1:
                        CalcReportFairy(targetLevel.Value);
                        break;
                    case 2:
                        CalcReportFST(targetLevel.Value, trainerLevel.Value);
                        break;
                }
            };
            resultExpItem = v.FindViewById<TextView>(Resource.Id.CalcReportResultExpItem);
            resultRemainExp = v.FindViewById<TextView>(Resource.Id.CalcReportResultRemainExp);
            resultTime = v.FindViewById<TextView>(Resource.Id.CalcReportResultTime);
            resultBattery = v.FindViewById<TextView>(Resource.Id.CalcReportResultBattery);

            // Set Option Icon Size

            var dm = Context.Resources.DisplayMetrics;
            int width = dm.WidthPixels / 2;

            applyMODModeSwitch.LayoutParameters.Width = width;
            applyMODModeSwitch.LayoutParameters.Height = width;
            applyVowSwitch.LayoutParameters.Width = width;
            applyVowSwitch.LayoutParameters.Height = width;

            InitializeProcess();

            return v;
        }

        
        private void ExpTypeList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                switch (e.Position)
                {
                    case 0:
                        applyMODModeSwitch.Visibility = ViewStates.Visible;
                        applyVowSwitch.Visibility = ViewStates.Visible;
                        resultTime.Visibility = ViewStates.Gone;
                        v.FindViewById<LinearLayout>(Resource.Id.CalcReportTrainerLevelSettingLayout).Visibility = ViewStates.Gone;
                        break;
                    case 1:
                        applyMODModeSwitch.Checked = false;
                        applyMODModeSwitch.Visibility = ViewStates.Gone;
                        applyVowSwitch.Visibility = ViewStates.Gone;
                        resultTime.Visibility = ViewStates.Gone;
                        v.FindViewById<LinearLayout>(Resource.Id.CalcReportTrainerLevelSettingLayout).Visibility = ViewStates.Gone;
                        break;
                    case 2:
                        applyMODModeSwitch.Checked = false;
                        applyMODModeSwitch.Visibility = ViewStates.Gone;
                        applyVowSwitch.Visibility = ViewStates.Gone;
                        resultTime.Visibility = ViewStates.Visible;
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
            // Set List Adapter

            string[] TypeList =
            {
                Resources.GetString(Resource.String.Common_TDoll),
                Resources.GetString(Resource.String.Common_Fairy),
                Resources.GetString(Resource.String.Common_FST)
            };

            var adapter = new ArrayAdapter(Activity, Resource.Layout.SpinnerListLayout, TypeList);
            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
            expTypeList.Adapter = adapter;

            // Set Init Value

            InitializeString();

            startLevel.MinValue = 1;
            startLevel.MinValue = 1;
            startLevel.Value = 1;
            targetLevel.MinValue = 1;
            targetLevel.MaxValue = 100;
            targetLevel.Value = 1;
            trainerLevel.MinValue = 0;
            trainerLevel.MaxValue = 10;
            trainerLevel.Value = 0;
            nowExpEditText.Text = "0";
        }

        private void InitializeString()
        {
            resultExpItem.Text = $"0 {Resources.GetString(Resource.String.Calc_ExpItem_DefaultItemResultText)}";
            resultRemainExp.Text = $"0 {Resources.GetString(Resource.String.Calc_ExpItem_DefaultRemainExpResultText)}";
            resultBattery.Text = $"0 {Resources.GetString(Resource.String.Calc_ExpItem_DefaultBatteryResultText)}";
            resultTime.Text = $"0 {Resources.GetString(Resource.String.Calc_ExpItem_DefaultTimeResultText)}";
        }

        private void LevelSelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcReportStartLevel when expTypeList.SelectedItemPosition == 0:
                        targetLevel.MinValue = e.NewVal;
                        nowExpEditText.Text = levelExp[np.Value - 1].ToString();
                        break;
                    case Resource.Id.CalcReportStartLevel when expTypeList.SelectedItemPosition == 1:
                        targetLevel.MinValue = e.NewVal;
                        nowExpEditText.Text = levelExpFairy[np.Value - 1].ToString();
                        break;
                    case Resource.Id.CalcReportStartLevel when expTypeList.SelectedItemPosition == 2:
                        targetLevel.MinValue = e.NewVal;
                        nowExpEditText.Text = levelExpFST[np.Value - 1].ToString();
                        break;
                    case Resource.Id.CalcReportEndLevel:
                        startLevel.MaxValue = e.NewVal;
                        break;
                }

                switch (expTypeList.SelectedItemPosition)
                {
                    case 0:
                        CalcReport(targetLevel.Value);
                        break;
                    case 1:
                        CalcReportFairy(targetLevel.Value);
                        break;
                    case 2:
                        CalcReportFST(targetLevel.Value, trainerLevel.Value);
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
            var tb = sender as ToggleButton;

            try
            {
                if (tb.Checked)
                {
                    startLevel.MaxValue = 100;
                    startLevel.MinValue = 100;
                    startLevel.Value = startLevel.MinValue;
                    targetLevel.MinValue = 100;
                    targetLevel.MaxValue = 120;
                    targetLevel.Value = targetLevel.MinValue;
                }
                else
                {
                    startLevel.MaxValue = 1;
                    startLevel.MinValue = 1;
                    startLevel.Value = startLevel.MinValue;
                    targetLevel.MinValue = 1;
                    targetLevel.MaxValue = 100;
                    targetLevel.Value = targetLevel.MinValue;
                }

                nowExpEditText.Text = levelExp[startLevel.Value - 1].ToString();

                CalcReport(targetLevel.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        /// <summary>
        /// Calculate report count for T-Doll
        /// </summary>
        /// <param name="target">Set target level</param>
        private void CalcReport(int target)
        {
            const int expItem = 3000;

            try
            {
                int nowExp = !string.IsNullOrWhiteSpace(nowExpEditText.Text) ? int.Parse(nowExpEditText.Text) : 0;
                int requireExp = levelExp[target - 1] - nowExp;
                int requireExpItem = isVow ? Convert.ToInt32(Math.Ceiling(requireExp / Convert.ToDouble(expItem * 2))) :
                    Convert.ToInt32(Math.Ceiling(requireExp / Convert.ToDouble(expItem)));
                int surplusExp = isVow ? (expItem * requireExpItem * 2) - requireExp : (expItem * requireExpItem) - requireExp;

                resultExpItem.Text = $"{requireExpItem} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultItemResultText)}";
                resultRemainExp.Text = $"{surplusExp} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultRemainExpResultText)}";
                resultBattery.Text = $"{requireExpItem * 3} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultBatteryResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// Calculate report count for Fairy
        /// </summary>
        /// <param name="target">Set target level</param>
        private void CalcReportFairy(int target)
        {
            const int expItem = 3000;

            try
            {
                int nowExp = !string.IsNullOrWhiteSpace(nowExpEditText.Text) ? int.Parse(nowExpEditText.Text) : 0;
                int requireExp = levelExpFairy[target - 1] - nowExp;
                int requireExpItem = Convert.ToInt32(Math.Ceiling(requireExp / Convert.ToDouble(expItem)));
                int surplusExp = (expItem * requireExpItem) - requireExp;

                resultExpItem.Text = $"{requireExpItem} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultItemResultText)}";
                resultRemainExp.Text = $"{surplusExp} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultRemainExpResultText)}";
                resultBattery.Text = $"{requireExpItem * 3} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultBatteryResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// Calculate report count for Fairy
        /// </summary>
        /// <param name="target">Set target level</param>
        /// <param name="trainer">Set FST trainer level</param>
        private void CalcReportFST(int target, int trainer)
        {
            const int expItem = 3000;

            try
            {
                int nowExp = !string.IsNullOrWhiteSpace(nowExpEditText.Text) ? int.Parse(nowExpEditText.Text) : 0;
                int requireExp = levelExpFST[target - 1] - nowExp;
                int requireExpItem = Convert.ToInt32(Math.Ceiling(requireExp / (double)expItem));
                int surplusExp = (expItem * requireExpItem) - requireExp;
                int requireTime = Convert.ToInt32(Math.Ceiling(requireExpItem / (double)consumeCountFST[trainer]));

                resultExpItem.Text = $"{requireExpItem} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultItemResultText)}";
                resultRemainExp.Text = $"{surplusExp} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultRemainExpResultText)}";
                resultBattery.Text = $"{requireExpItem * 5} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultBatteryResultText)}";
                resultTime.Text = $"{requireTime} {Resources.GetString(Resource.String.Calc_ExpItem_DefaultTimeResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Short).Show();
            }
        }
    }
}