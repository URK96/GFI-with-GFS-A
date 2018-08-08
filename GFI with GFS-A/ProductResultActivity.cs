using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using Android.Animation;
using System.IO;
using System.Net;

namespace GFI_with_GFS_A
{
    [Activity(Label = "ProductResultActivity", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ProductResultActivity : FragmentActivity
    {
        enum ResultType { Doll, Equip, Fairy }

        private ResultType Result_Type = ResultType.Doll;

        private DataRow ResultDR = null;
        private string ResultName = "";
        private int ResultTime = 0;
        private int ResultGrade = 0;

        private ProgressBar[] PBs = new ProgressBar[5];

        private CoordinatorLayout SnackbarLayout;

        private int[] PB_Ids =
        {
            Resource.Id.PSResultPB1,
            Resource.Id.PSResultPB2,
            Resource.Id.PSResultPB3,
            Resource.Id.PSResultPB4,
            Resource.Id.PSResultPB5
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.ProductResultLayout);

            string[] data = Intent.GetStringExtra("ResultData").Split(';');
            ResultName = data[1];

            switch (data[0])
            {
                case "Doll":
                    Result_Type = ResultType.Doll;
                    ResultDR = ETC.FindDataRow(ETC.DollList, "Name", ResultName);
                    break;
                case "Equip":
                    Result_Type = ResultType.Equip;
                    ResultDR = ETC.FindDataRow(ETC.EquipmentList, "Name", ResultName);
                    break;
                case "Fairy":
                    Result_Type = ResultType.Fairy;
                    ResultDR = ETC.FindDataRow(ETC.FairyList, "Name", ResultName);
                    break;
            }

            ResultTime = (int)ResultDR["ProductTime"];
            if (Result_Type == ResultType.Fairy) ResultGrade = 0;
            else ResultGrade = (int)ResultDR["Grade"];

            for (int i = 0; i < PBs.Length; ++i) PBs[i] = FindViewById<ProgressBar>(PB_Ids[i]);

            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.PSResultSnackbarLayout);

            Task.Run(async () =>
            {
                string FileName = "";

                switch (Result_Type)
                {
                    case ResultType.Doll:
                        FileName = ((int)ResultDR["DicNumber"]).ToString();
                        break;
                    case ResultType.Equip:
                        FileName = (string)ResultDR["Icon"];
                        break;
                    case ResultType.Fairy:
                        FileName = (string)ResultDR["Name"] + "_1";
                        break;
                }

                if (File.Exists(Path.Combine(ETC.CachePath, "Doll", "Normal", FileName + ".gfdcache")) == false)
                {
                    using (WebClient wc = new WebClient())
                    {
                        await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", FileName + ".png"), Path.Combine(ETC.CachePath, "Doll", "Normal", FileName + ".gfdcache"));
                    }
                }
            });

            ResultAnimationProcess();
        }

        private async Task ResultAnimationProcess()
        {
            await Task.Delay(500);

            await ProductTimeAnimation();
            await GradeImageAnimation();
            await ResultImageAnimation();
        }

        private async Task ProductTimeAnimation()
        {
            TextView ProductTimeView = FindViewById<TextView>(Resource.Id.PSResultProductTime);

            try
            {            
                int delay = 1;
                int MaxValue = 0;

                if (ResultGrade == 0)
                {

                }
                else
                {
                    int count = 0;
                    Random R = new Random(ResultTime);
                    int ran_num = R.Next() % 10;
                    int IsUp = 0;

                    MaxValue = ResultTime / ResultGrade;

                    if (ResultGrade == 2) IsUp = 0;
                    else if (ResultGrade == 5)
                    {
                        //MaxValue = ResultTime / 4;
                        IsUp = 1;
                    }
                    else
                    {
                        //MaxValue = ResultTime / ResultGrade;
                        IsUp = R.Next() % 2;
                    }

                    foreach (ProgressBar pb in PBs)
                    {
                        pb.Max = MaxValue;
                        //pb.Min = 0;
                        pb.Progress = 0;
                    }

                    for (int i = 0; i <= (ResultTime - 1); ++i)
                    {
                        ProductTimeView.Text = ETC.CalcTime(i);

                        if ((IsUp == 0) && (count == (ResultGrade))) PBs[ResultGrade].Progress = MaxValue / 2;
                        else if ((IsUp == 1) && (count == (ResultGrade - 1))) PBs[ResultGrade - 1].Progress = MaxValue / 2;
                        else PBs[count].Progress += 1;

                        if (PBs[count].Progress == MaxValue) count += 1;
                        if (i == (ResultTime - (ran_num))) delay = 100;
                        if (i == (ResultTime - 2)) delay = 500;

                        await Task.Delay(delay);
                    }

                    await Task.Delay(500);

                    ProductTimeView.Text = ETC.CalcTime(ResultTime) + " " + IsUp + " " + ResultGrade;
                    switch (IsUp)
                    {
                        case 0:
                            PBs[ResultGrade].Progress = 0;
                            break;
                        case 1:
                            PBs[ResultGrade - 1].Progress = MaxValue;
                            break;
                    }

                    foreach (ProgressBar pb in PBs) pb.ProgressTintList = PBs[ResultGrade - 1].ProgressTintList;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "시간 애니메이션 오류", Snackbar.LengthShort);
            }
        }

        private async Task GradeImageAnimation()
        {
            ImageView GradeImage = FindViewById<ImageView>(Resource.Id.PSResultGradeImageView);

            try
            {
                int resId = 0;

                switch (ResultGrade)
                {
                    case 2:
                        resId = Resource.Drawable.PSGrade_2;
                        break;
                    case 3:
                        resId = Resource.Drawable.PSGrade_3;
                        break;
                    case 4:
                        resId = Resource.Drawable.PSGrade_4;
                        break;
                    case 5:
                        resId = Resource.Drawable.PSGrade_5;
                        break;
                    default:
                        resId = Resource.Drawable.PSGrade_0;
                        break;
                }

                GradeImage.SetImageResource(resId);

                GradeImage.Visibility = ViewStates.Visible;
                GradeImage.Animate().Alpha(1.0f).SetDuration(500).Start();
                FindViewById<LinearLayout>(Resource.Id.PSResultGradeProgressLayout).Animate().Alpha(0.0f).SetDuration(500).Start();
                await Task.Delay(500);

                ValueAnimator animator = ValueAnimator.OfInt(GradeImage.Height, 100);
                animator.SetDuration(1000);
                animator.Update += (object sender, ValueAnimator.AnimatorUpdateEventArgs e) =>
                {
                    int value = (int)animator.AnimatedValue;
                    ViewGroup.LayoutParams layoutParams = GradeImage.LayoutParameters;
                    layoutParams.Height = value;
                    GradeImage.LayoutParameters = layoutParams;

                };
                animator.Start();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "등급 애니메이션 오류", Snackbar.LengthShort);
            }
        }

        private async Task ResultImageAnimation()
        {
            ImageView ResultImage = FindViewById<ImageView>(Resource.Id.PSResultImageView);
            TextView ProductTimeView = FindViewById<TextView>(Resource.Id.PSResultProductTime);

            try
            {
                await Task.Delay(100);

                string FileName = "";

                switch (Result_Type)
                {
                    case ResultType.Doll:
                        FileName = ((int)ResultDR["DicNumber"]).ToString();
                        break;
                    case ResultType.Equip:
                        FileName = (string)ResultDR["Icon"];
                        break;
                    case ResultType.Fairy:
                        FileName = (string)ResultDR["Name"] + "_1";
                        break;
                }

                ResultImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Normal", FileName + ".gfdcache")));

                ResultImage.Visibility = ViewStates.Visible;
                ResultImage.Animate().Alpha(1.0f).SetDuration(1000).Start();
                ProductTimeView.Animate().Alpha(0.0f).SetDuration(500).Start();
                ProductTimeView.Text = ResultName;
                ProductTimeView.Animate().Alpha(1.0f).SetDuration(500).Start();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "결과 애니메이션 오류", Snackbar.LengthShort);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            GC.Collect();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }
    }
}