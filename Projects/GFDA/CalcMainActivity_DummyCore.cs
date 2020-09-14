using Android.OS;
using Android.Views;
using Android.Widget;
using System;

namespace GFDA
{
    public class DummyCore : AndroidX.Fragment.App.Fragment
    {
        private View v;

        // T-Doll Require Core by Link Dummy
        private readonly int[] LevelLink_DollCount = { 0, 1, 1, 2, 3 };
        // T-Doll Require Core by Grade
        private readonly int[] GradeLinkCore = { 1, 3, 9, 15 };

        private NumberPicker dollGradeSelector;
        private NumberPicker dollDummyStart;
        private NumberPicker dollDummyTarget;
        private TextView resultText;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_Core, container, false);

            // Find View & Connect Event

            dollGradeSelector = v.FindViewById<NumberPicker>(Resource.Id.CalcCoreDollGrade);
            dollGradeSelector.ValueChanged += delegate (object sender, NumberPicker.ValueChangeEventArgs e) { CalcCore(e.NewVal, dollDummyStart.Value, dollDummyTarget.Value); };
            dollDummyStart = v.FindViewById<NumberPicker>(Resource.Id.CalcCoreStartDollCount);
            dollDummyStart.ValueChanged += DollDummySelector_ValueChanged;
            dollDummyTarget = v.FindViewById<NumberPicker>(Resource.Id.CalcCoreEndDollCount);
            dollDummyTarget.ValueChanged += DollDummySelector_ValueChanged;
            resultText = v.FindViewById<TextView>(Resource.Id.CalcCoreResult);

            InitializeProcess();

            return v;
        }

        private void InitializeProcess()
        {
            // Set Init Value

            resultText.Text = $"0 {Resources.GetString(Resource.String.Calc_Core_DefaultCoreCountResultText)}";

            dollGradeSelector.MinValue = 2;
            dollGradeSelector.MaxValue = 5;
            dollGradeSelector.Value = 2;
            dollDummyStart.MinValue = 1;
            dollDummyStart.MaxValue = 1;
            dollDummyStart.Value = 1;
            dollDummyTarget.MinValue = 1;
            dollDummyTarget.MaxValue = 5;
            dollDummyTarget.Value = 1;
        }

        private void DollDummySelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            var np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcCoreStartDollCount:
                        dollDummyTarget.MinValue = e.NewVal;
                        break;
                    case Resource.Id.CalcCoreEndDollCount:
                        dollDummyStart.MaxValue = e.NewVal;
                        break;
                }

                CalcCore(dollGradeSelector.Value, dollDummyStart.Value, dollDummyTarget.Value);
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
                {
                    requireDollCount += LevelLink_DollCount[start + i];
                }

                int resultCore = requireDollCount * GradeLinkCore[grade - 2];

                resultText.Text = $"{resultCore} {Resources.GetString(Resource.String.Calc_Core_DefaultCoreCountResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }
    }
}