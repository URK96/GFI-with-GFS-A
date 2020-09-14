using Android.OS;
using Android.Views;
using Android.Widget;
using System;

namespace GFDA
{
    public class FSTGradeUp : AndroidX.Fragment.App.Fragment
    {
        private View v;

        // FST Require Fragment by Grade
        private readonly int[] fragmentCount = { 0, 5, 15, 30, 50, 80 };
        // FST Require Data Patch by Version
        private readonly int[] dataPatchCount = { 0, 5, 13, 23, 38, 58, 83, 113, 143, 173, 203 };

        private RatingBar nowGrade1;
        private RatingBar nowGrade2;
        private RatingBar targetGrade1;
        private RatingBar targetGrade2;
        private TextView resultFragment;
        private TextView resultDataPatch;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_FSTGradeUp, container, false);

            // Find View & Connect Event

            nowGrade1 = v.FindViewById<RatingBar>(Resource.Id.Calc_FSTGradeUp_NowGradeControl1);
            nowGrade1.RatingBarChange += NormalGrade_RatingBarChange;
            nowGrade2 = v.FindViewById<RatingBar>(Resource.Id.Calc_FSTGradeUp_NowGradeControl2);
            nowGrade2.RatingBarChange += VersionUpGrade_RatingBarChange;
            targetGrade1 = v.FindViewById<RatingBar>(Resource.Id.Calc_FSTGradeUp_TargetGradeControl1);
            targetGrade1.RatingBarChange += NormalGrade_RatingBarChange;
            targetGrade2 = v.FindViewById<RatingBar>(Resource.Id.Calc_FSTGradeUp_TargetGradeControl2);
            targetGrade2.RatingBarChange += VersionUpGrade_RatingBarChange;
            resultFragment = v.FindViewById<TextView>(Resource.Id.Calc_FSTGradeUp_ResultFragment);
            resultDataPatch = v.FindViewById<TextView>(Resource.Id.Calc_FSTGradeUp_ResultDataPatch);

            InitializeProcess();

            return v;
        }

        private void InitializeProcess()
        {
            resultFragment.Text = $"0 {Resources.GetString(Resource.String.Calc_FSTGradeUp_DefaultFragmentResultText)}";
            resultDataPatch.Text = $"0 {Resources.GetString(Resource.String.Calc_FSTGradeUp_DefaultDataPatchText)}";

            nowGrade1.Rating = 0;
            nowGrade2.Rating = 0;
            targetGrade1.Rating = 0;
            targetGrade2.Rating = 0;
        }

        private void NormalGrade_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            float now = nowGrade1.Rating;
            float target = targetGrade1.Rating;

            if (now <= target)
            {
                CalcFragment(Convert.ToInt32(now), Convert.ToInt32(target));
            }
            else
            {
                targetGrade1.Rating = now;
            }
        }

        private void VersionUpGrade_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            float now = nowGrade2.Rating;
            float target = targetGrade2.Rating;

            if (now <= target)
            {
                CalcDataPatch(Convert.ToInt32(now * 2), Convert.ToInt32(target * 2));
            }
            else
            {
                targetGrade2.Rating = now;
            }
        }

        /// <summary>
        /// Calculate require fragment
        /// </summary>
        /// <param name="start">Set FST now grade</param>
        /// <param name="target">Set FST target grade</param>
        private void CalcFragment(int start, int target)
        {
            try
            {
                int requireFragment = fragmentCount[target] - fragmentCount[start];

                resultFragment.Text = $"{requireFragment} {Resources.GetString(Resource.String.Calc_FSTGradeUp_DefaultFragmentResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }

        /// <summary>
        /// Calculate require data patch
        /// </summary>
        /// <param name="start">Set FST now version</param>
        /// <param name="target">Set FST target version</param>
        private void CalcDataPatch(int start, int target)
        {
            try
            {
                int requireDataPatch = dataPatchCount[target] - dataPatchCount[start];

                resultDataPatch.Text = $"{requireDataPatch} {Resources.GetString(Resource.String.Calc_FSTGradeUp_DefaultDataPatchText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }
    }
}