using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.Media;

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
        private MediaPlayer[] GradeEffect = new MediaPlayer[5];
        private MediaPlayer ResultEffect;

        private FloatingActionButton LinkDetailFAB;

        private CoordinatorLayout SnackbarLayout;

        private readonly int[] PB_Ids =
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

            LinkDetailFAB = FindViewById<FloatingActionButton>(Resource.Id.PSResultLinkDetailFAB);
            LinkDetailFAB.Hide();
            LinkDetailFAB.Click += LinkDetailFAB_Click;

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
                        FileName = string.Format("{0}_1", (string)ResultDR["Name"]);
                        break;
                }

                try
                {
                    if (File.Exists(Path.Combine(ETC.CachePath, "Doll", "Normal", FileName + ".gfdcache")) == false)
                    {
                        using (TimeOutWebClient wc = new TimeOutWebClient())
                        {
                            await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", FileName + ".png"), Path.Combine(ETC.CachePath, "Doll", "Normal", FileName + ".gfdcache"));
                        }
                    }
                }
                catch (Exception)
                {

                }
            });

            ResultAnimationProcess();
        }

        private void LinkDetailFAB_Click(object sender, EventArgs e)
        {
            try
            {
                Intent ResultInfo = null;
                
                switch (Result_Type)
                {
                    case ResultType.Doll:
                        ResultInfo = new Intent(this, typeof(DollDBDetailActivity));
                        break;
                    case ResultType.Equip:
                        ResultInfo = new Intent(this, typeof(EquipDBDetailActivity));
                        break;
                    case ResultType.Fairy:
                        ResultInfo = new Intent(this, typeof(FairyDBDetailActivity));
                        break;
                }

                ResultInfo.PutExtra("Keyword", ResultName);
                StartActivity(ResultInfo);
                OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBDetail_LoadDetailFail, Snackbar.LengthShort);
            }
        }

        private async Task ResultAnimationProcess()
        {
            await Task.Delay(500);

            await ProductTimeAnimation();
            await Task.Delay(500);
            await GradeImageAnimation();
            await ResultImageAnimation();

            await Task.Delay(500);
            LinkDetailFAB.Show();
        }

        private async Task ProductTimeAnimation()
        {
            TextView ProductTimeView = FindViewById<TextView>(Resource.Id.PSResultProductTime);

            try
            {
                ResultEffect = MediaPlayer.Create(this, Resource.Raw.UI_card);
                for(int i = 0; i < GradeEffect.Length; ++i) GradeEffect[i] = MediaPlayer.Create(this, Resource.Raw.UI_flash);

                int delay = 100;

                for (int i = 0; i < ResultGrade; ++i)
                {
                    GradeEffect[i].Start();

                    for (int k = 0; k <= 10; ++k)
                    {
                        PBs[i].Progress += 1;
                        await Task.Delay(delay);
                    }
                }

                ResultEffect.Start();

                if (ResultGrade == 0)
                {
                    await Task.Delay(1000);
                    foreach (ProgressBar pb in PBs) pb.ProgressTintList = PBs[0].ProgressTintList;
                }
                else foreach (ProgressBar pb in PBs) pb.ProgressTintList = PBs[ResultGrade - 1].ProgressTintList;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Animation_Error, Snackbar.LengthShort);
            }
            finally
            {
                foreach (MediaPlayer mp in GradeEffect)
                {
                    mp.Stop();
                    mp.Release();
                }
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
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Animation_Error, Snackbar.LengthShort);
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
                        FileName = string.Format("{0}_1", (string)ResultDR["Name"]);
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
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Animation_Error, Snackbar.LengthShort);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            ResultEffect.Stop();
            ResultEffect.Release();
            GC.Collect();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }
    }
}