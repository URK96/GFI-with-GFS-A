using Android.OS;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Data;

namespace GFDA
{
    public class SkillTraining : AndroidX.Fragment.App.Fragment
    {
        private View v;

        private Spinner trainingTypeList;
        private NumberPicker trainingStartLevel;
        private NumberPicker trainingTargetLevel;
        private TextView resultBasicChip;
        private TextView resultAdvanceChip;
        private TextView resultMasterChip;
        private TextView resultSkillItemText;
        private TextView resultTime;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_SkillTraining, container, false);

            // Find View & Connect Event

            trainingTypeList = v.FindViewById<Spinner>(Resource.Id.CalcSkillTrainingType);
            trainingStartLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcSkillTrainingStartLevel);
            trainingTargetLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcSkillTrainingEndLevel);
            resultBasicChip = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultBasicSkillChip);
            resultAdvanceChip = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultAdvanceSkillChip);
            resultMasterChip = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultMasterSkillChip);
            resultSkillItemText = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultSkillItemText);
            resultTime = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultTime);

            trainingTypeList.ItemSelected += TrainingTypeList_ItemSelected;
            trainingStartLevel.ValueChanged += TrainingLevelSelector_ValueChanged;
            trainingTargetLevel.ValueChanged += TrainingLevelSelector_ValueChanged;

            InitializeProcess();

            return v;
        }

        private void InitializeProcess()
        {
            // Set List Adapter

            var list = new List<string>(5);

            foreach (DataRow dr in ETC.skillTrainingList.Rows)
            {
                list.Add((string)dr[(ETC.locale.Language == "ko") ? "Type" : "Type_EN"]);
            }

            list.TrimExcess();

            var adapter = new ArrayAdapter(Activity, Resource.Layout.SpinnerListLayout, list);
            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

            trainingTypeList.Adapter = adapter;

            // Set Init Value

            resultBasicChip.Text = $"0 {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultBasicSkillChipResultText)}";
            resultAdvanceChip.Text = $"0 {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultAdvanceSkillChipResultText)}";
            resultMasterChip.Text = $"0 {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultMasterSkillChipResultText)}";
            resultTime.Text = $"0 {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultTimeResultText)}";

            trainingStartLevel.MinValue = 1;
            trainingStartLevel.MaxValue = 1;
            trainingStartLevel.Value = 1;
            trainingTargetLevel.MinValue = 1;
            trainingTargetLevel.MaxValue = 10;
            trainingTargetLevel.Value = 1;
        }

        private void TrainingTypeList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                trainingStartLevel.MinValue = 1;
                trainingStartLevel.MaxValue = 1;
                trainingStartLevel.Value = 1;
                trainingTargetLevel.MinValue = 1;
                trainingTargetLevel.Value = 1;

                switch (e.Position)
                {
                    default:
                    case 0:
                    case 1:
                    case 2:
                        trainingTargetLevel.MaxValue = 10;
                        resultAdvanceChip.Visibility = ViewStates.Visible;
                        break;
                    case 3:
                        trainingTargetLevel.MaxValue = 10;
                        resultAdvanceChip.Visibility = ViewStates.Gone;
                        break;
                    case 4:
                        trainingTargetLevel.MaxValue = 5;
                        resultAdvanceChip.Visibility = ViewStates.Gone;
                        break;
                }

                CalcSkillTraining(trainingStartLevel.Value, trainingTargetLevel.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        private void TrainingLevelSelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            var np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcSkillTrainingResultTime:
                        trainingTargetLevel.MinValue = e.NewVal;
                        break;
                    case Resource.Id.CalcSkillTrainingEndLevel:
                        trainingStartLevel.MaxValue = e.NewVal;
                        break;
                }

                CalcSkillTraining(trainingStartLevel.Value, trainingTargetLevel.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        /// <summary>
        /// Calculate require chips of T-Doll & Fairy skill
        /// </summary>
        /// <param name="start">Set now skill level</param>
        /// <param name="target">Set target skill level</param>
        private void CalcSkillTraining(int start, int target)
        {
            try
            {
                var dr = ETC.skillTrainingList.Rows[trainingTypeList.SelectedItemPosition];

                string[] itemConsume = ((string)dr["Consumption"]).Split(';');
                string[] time = ((string)dr["Time"]).Split(';');
                string[] itemType = ((string)dr["DataType"]).Split(';');

                int[] itemCount = { 0, 0, 0 };
                int timeCount = 0;

                for (int i = start; i < target; ++i)
                {
                    int count = int.Parse(itemConsume[i - 1]);

                    switch (itemType[i - 1])
                    {
                        case "B":
                            itemCount[0] += count;
                            break;
                        case "A":
                            itemCount[1] += count;
                            break;
                        case "M":
                            itemCount[2] += count;
                            break;
                    }

                    timeCount += int.Parse(time[i - 1]);
                }

                switch (trainingTypeList.SelectedItemPosition)
                {
                    case 0:
                    case 1:
                    case 2:
                        resultSkillItemText.SetText(Resource.String.Calc_SkillTraining_DefaultSkillChipResultText);
                        break;
                    case 3:
                    case 4:
                        resultSkillItemText.SetText(Resource.String.Calc_skillTraining_DefaultSkillCodeResultText);
                        break;
                }

                resultBasicChip.Text = $"{itemCount[0]} {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultBasicSkillChipResultText)}";
                resultAdvanceChip.Text = $"{itemCount[1]} {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultAdvanceSkillChipResultText)}";
                resultMasterChip.Text = $"{itemCount[2]} {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultMasterSkillChipResultText)}";
                resultTime.Text = $"{timeCount} {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultTimeResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }
    }
}