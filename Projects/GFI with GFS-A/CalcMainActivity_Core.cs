using Android.OS;
using Android.Views;
using Android.Widget;
using System;

namespace GFI_with_GFS_A
{
    public class Core : Android.Support.V4.App.Fragment
    {
        private View v;

        // T-Doll Require Core by Link Dummy
        private readonly int[] LevelLink_DollCount = { 0, 1, 1, 2, 3 };
        // T-Doll Require Core by Grade
        private readonly int[] GradeLinkCore = { 1, 3, 9, 15 };

        private NumberPicker DollGradeSelector;
        private NumberPicker DollDummyStart;
        private NumberPicker DollDummyTarget;
        private TextView Result;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_Core, container, false);

            // Find View & Connect Event

            DollGradeSelector = v.FindViewById<NumberPicker>(Resource.Id.CalcCoreDollGrade);
            DollGradeSelector.ValueChanged += delegate (object sender, NumberPicker.ValueChangeEventArgs e) { CalcCore(e.NewVal, DollDummyStart.Value, DollDummyTarget.Value); };
            DollDummyStart = v.FindViewById<NumberPicker>(Resource.Id.CalcCoreStartDollCount);
            DollDummyStart.ValueChanged += DollDummySelector_ValueChanged;
            DollDummyTarget = v.FindViewById<NumberPicker>(Resource.Id.CalcCoreEndDollCount);
            DollDummyTarget.ValueChanged += DollDummySelector_ValueChanged;
            Result = v.FindViewById<TextView>(Resource.Id.CalcCoreResult);


            InitializeProcess();

            return v;
        }

        private void InitializeProcess()
        {
            // Set Init Value

            Result.Text = $"0 {Resources.GetString(Resource.String.Calc_Core_DefaultCoreCountResultText)}";

            DollGradeSelector.MinValue = 2;
            DollGradeSelector.MaxValue = 5;
            DollGradeSelector.Value = 2;
            DollDummyStart.MinValue = 1;
            DollDummyStart.MaxValue = 1;
            DollDummyStart.Value = 1;
            DollDummyTarget.MinValue = 1;
            DollDummyTarget.MaxValue = 5;
            DollDummyTarget.Value = 1;
        }

        private void DollDummySelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcCoreStartDollCount:
                        DollDummyTarget.MinValue = e.NewVal;
                        break;
                    case Resource.Id.CalcCoreEndDollCount:
                        DollDummyStart.MaxValue = e.NewVal;
                        break;
                }

                CalcCore(DollGradeSelector.Value, DollDummyStart.Value, DollDummyTarget.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        /// <summary>
        /// Calculate require core count by T-Doll's Link
        /// </summary>
        /// <param name="grade">T-Doll's grade</param>
        /// <param name="start">Set start link count</param>
        /// <param name="target">Set target link count</param>
        private void CalcCore(int grade, int start, int target)
        {
            try
            {
                int requireDollCount = 0;

                for (int i = 0; i < (target - start); ++i)
                    requireDollCount += LevelLink_DollCount[start + i];

                int resultCore = requireDollCount * GradeLinkCore[grade - 2];

                Result.Text = $"{resultCore} {Resources.GetString(Resource.String.Calc_Core_DefaultCoreCountResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }
    }
}