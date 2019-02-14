using Android.OS;
using Android.Views;
using Android.Widget;
using System;

namespace GFI_with_GFS_A
{
    public class FSTGradeUp : Android.Support.V4.App.Fragment
    {
        private View v;

        private readonly int[] FragmentCount = { 0, 5, 15, 30, 50, 80 };
        private readonly int[] DataPatchCount = { 0, 5, 13, 23, 38, 58, 83, 113, 143, 173, 203 };

        private RatingBar NowGrade1;
        private RatingBar NowGrade2;
        private RatingBar TargetGrade1;
        private RatingBar TargetGrade2;
        private TextView Result_Fragment;
        private TextView Result_DataPatch;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_FSTGradeUp, container, false);

            NowGrade1 = v.FindViewById<RatingBar>(Resource.Id.Calc_FSTGradeUp_NowGradeControl1);
            NowGrade1.RatingBarChange += NormalGrade_RatingBarChange;
            NowGrade2 = v.FindViewById<RatingBar>(Resource.Id.Calc_FSTGradeUp_NowGradeControl2);
            NowGrade2.RatingBarChange += VersionUpGrade_RatingBarChange;
            TargetGrade1 = v.FindViewById<RatingBar>(Resource.Id.Calc_FSTGradeUp_TargetGradeControl1);
            TargetGrade1.RatingBarChange += NormalGrade_RatingBarChange;
            TargetGrade2 = v.FindViewById<RatingBar>(Resource.Id.Calc_FSTGradeUp_TargetGradeControl2);
            TargetGrade2.RatingBarChange += VersionUpGrade_RatingBarChange;
            Result_Fragment = v.FindViewById<TextView>(Resource.Id.Calc_FSTGradeUp_ResultFragment);
            Result_DataPatch = v.FindViewById<TextView>(Resource.Id.Calc_FSTGradeUp_ResultDataPatch);

            Result_Fragment.Text = $"0 {Resources.GetString(Resource.String.Calc_FSTGradeUp_DefaultFragmentResultText)}";
            Result_DataPatch.Text = $"0 {Resources.GetString(Resource.String.Calc_FSTGradeUp_DefaultDataPatchText)}";

            InitializeProcess();

            return v;
        }

        private void NormalGrade_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            float now = NowGrade1.Rating;
            float target = TargetGrade1.Rating;

            if (now <= target) CalcFragment(Convert.ToInt32(now), Convert.ToInt32(target));
            else TargetGrade1.Rating = now;
        }

        private void VersionUpGrade_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            float now = NowGrade2.Rating;
            float target = TargetGrade2.Rating;

            if (now <= target) CalcDataPatch(Convert.ToInt32(now * 2), Convert.ToInt32(target * 2));
            else TargetGrade2.Rating = now;
        }

        private void InitializeProcess()
        {
            NowGrade1.Rating = 0;
            NowGrade2.Rating = 0;
            TargetGrade1.Rating = 0;
            TargetGrade2.Rating = 0;
        }

        private void CalcFragment(int start, int target)
        {
            try
            {
                int RequireFragment = FragmentCount[target] - FragmentCount[start];

                Result_Fragment.Text = $"{RequireFragment} {Resources.GetString(Resource.String.Calc_FSTGradeUp_DefaultFragmentResultText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }

        private void CalcDataPatch(int start, int target)
        {
            try
            {
                int RequireDataPatch = DataPatchCount[target] - DataPatchCount[start];

                Result_DataPatch.Text = $"{RequireDataPatch} {Resources.GetString(Resource.String.Calc_FSTGradeUp_DefaultDataPatchText)}";
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }
    }
}