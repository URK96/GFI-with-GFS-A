using Android.OS;
using Android.Views;
using Android.Widget;
using System;

namespace GFI_with_GFS_A
{
    public class CoalitionGrade : AndroidX.Fragment.App.Fragment
    {
        private View v;

        private readonly int[,] requireDisk =
        {
            { 50, 70, 100, 200, 0 },
            { 0, 100, 150, 300, 0 },
            { 0, 0, 300, 450, 0 }
        };

        private NumberPicker bornGradeSelector;
        private NumberPicker gradeStart;
        private NumberPicker gradeTarget;
        private TextView result;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater?.Inflate(Resource.Layout.Calc_CoalitionGrade, container, false);

            // Find View & Connect Event

            bornGradeSelector = v.FindViewById<NumberPicker>(Resource.Id.CalcCoalitionGradeBornGrade);
            gradeStart = v.FindViewById<NumberPicker>(Resource.Id.CalcCoalitionGradeStartGrade);
            gradeTarget = v.FindViewById<NumberPicker>(Resource.Id.CalcCoalitionGradeEndGrade);
            result = v.FindViewById<TextView>(Resource.Id.CalcCoalitionGradeResult);

            bornGradeSelector.ValueChanged += DollDummySelector_ValueChanged;
            gradeStart.ValueChanged += DollDummySelector_ValueChanged;
            gradeTarget.ValueChanged += DollDummySelector_ValueChanged;

            InitializeProcess();

            return v;
        }

        private void InitializeProcess()
        {
            // Set Init Value

            result.Text = $"0 {Resources.GetString(Resource.String.Calc_CoalitionGrade_DefaultDiskCountResultText)}";

            bornGradeSelector.MinValue = 1;
            bornGradeSelector.MaxValue = 3;
            bornGradeSelector.Value = 1;
            gradeStart.MinValue = 1;
            gradeStart.MaxValue = 5;
            gradeStart.Value = 1;
            gradeTarget.MinValue = 1;
            gradeTarget.MaxValue = 5;
            gradeTarget.Value = 1;
        }

        private void DollDummySelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcCoalitionGradeBornGrade:
                        gradeStart.MinValue = e.NewVal;
                        gradeTarget.MinValue = gradeStart.Value;
                        break;
                    case Resource.Id.CalcCoalitionGradeStartGrade:
                        gradeTarget.MinValue = e.NewVal;
                        break;
                    case Resource.Id.CalcCoalitionGradeEndGrade:
                        gradeStart.MaxValue = e.NewVal;
                        break;
                }

                CalcDisk(bornGradeSelector.Value, gradeStart.Value, gradeTarget.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        private void CalcDisk(int grade, int start, int target)
        {
            try
            {
                int count = 0;
                int gradeIndex = grade - 1;

                for (int i = (start - 1); i < (target - 1); ++i)
                {
                    count += requireDisk[gradeIndex, i];
                }

                result.Text = $"{count} {Resources.GetString(Resource.String.Calc_CoalitionGrade_DefaultDiskCountResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }
    }
}