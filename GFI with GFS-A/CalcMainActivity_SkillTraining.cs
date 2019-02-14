using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;

namespace GFI_with_GFS_A
{
    public class SkillTraining : Android.Support.V4.App.Fragment
    {
        private View v;

        private Spinner TrainingTypeList;
        private NumberPicker TrainingStartLevel;
        private NumberPicker TrainingTargetLevel;
        private TextView Result_BasicChip;
        private TextView Result_AdvanceChip;
        private TextView Result_MasterChip;
        private TextView Result_Time;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_SkillTraining, container, false);

            TrainingTypeList = v.FindViewById<Spinner>(Resource.Id.CalcSkillTrainingType);
            TrainingTypeList.ItemSelected += delegate { CalcSkillTraining(TrainingStartLevel.Value, TrainingTargetLevel.Value); };
            TrainingStartLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcSkillTrainingStartLevel);
            TrainingStartLevel.ValueChanged += TrainingLevelSelector_ValueChanged;
            TrainingTargetLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcSkillTrainingEndLevel);
            TrainingTargetLevel.ValueChanged += TrainingLevelSelector_ValueChanged;
            Result_BasicChip = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultBasicSkillChip);
            Result_AdvanceChip = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultAdvanceSkillChip);
            Result_MasterChip = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultMasterSkillChip);
            Result_Time = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultTime);

            Result_BasicChip.Text = $"0 {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultBasicSkillChipResultText)}";
            Result_AdvanceChip.Text = $"0 {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultAdvanceSkillChipResultText)}";
            Result_MasterChip.Text = $"0 {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultMasterSkillChipResultText)}";
            Result_Time.Text = $"0 {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultTimeResultText)}";

            InitializeProcess();

            return v;
        }

        private void InitializeProcess()
        {
            List<string> list = new List<string>(3);

            foreach (DataRow dr in ETC.SkillTrainingList.Rows) list.Add((string)dr["Type"]);
            list.TrimExcess();

            var adapter = new ArrayAdapter(Activity, Resource.Layout.SpinnerListLayout, list);
            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
            TrainingTypeList.Adapter = adapter;

            TrainingStartLevel.MinValue = 1;
            TrainingStartLevel.MaxValue = 1;
            TrainingStartLevel.Value = 1;
            TrainingTargetLevel.MinValue = 1;
            TrainingTargetLevel.MaxValue = 10;
            TrainingTargetLevel.Value = 1;
        }

        private void TrainingLevelSelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcSkillTrainingResultTime:
                        TrainingTargetLevel.MinValue = e.NewVal;
                        break;
                    case Resource.Id.CalcSkillTrainingEndLevel:
                        TrainingStartLevel.MaxValue = e.NewVal;
                        break;
                }

                CalcSkillTraining(TrainingStartLevel.Value, TrainingTargetLevel.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }

        private void CalcSkillTraining(int start, int target)
        {
            try
            {
                DataRow dr = ETC.SkillTrainingList.Rows[TrainingTypeList.SelectedItemPosition];

                string[] ItemConsume = ((string)dr["Consumption"]).Split(';');
                string[] Time = ((string)dr["Time"]).Split(';');
                string[] ItemType = ((string)dr["DataType"]).Split(';');

                int[] ItemCount = { 0, 0, 0 };
                int TimeCount = 0;

                for (int i = start; i < target; ++i)
                {
                    int count = int.Parse(ItemConsume[i - 1]);

                    switch (ItemType[i - 1])
                    {
                        case "B":
                            ItemCount[0] += count;
                            break;
                        case "A":
                            ItemCount[1] += count;
                            break;
                        case "M":
                            ItemCount[2] += count;
                            break;
                    }

                    TimeCount += int.Parse(Time[i - 1]);
                }

                Result_BasicChip.Text = $"{ItemCount[0]} {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultBasicSkillChipResultText)}";
                Result_AdvanceChip.Text = $"{ItemCount[1]} {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultAdvanceSkillChipResultText)}";
                Result_MasterChip.Text = $"{ItemCount[2]} {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultMasterSkillChipResultText)}";
                Result_Time.Text = $"{TimeCount} {Resources.GetString(Resource.String.Calc_SkillTraining_DefaultTimeResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }
    }
}