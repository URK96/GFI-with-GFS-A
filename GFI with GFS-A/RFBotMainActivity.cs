using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "라플봇", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class RFBotMainActivity : AppCompatActivity
    {
        private TextView StatusText;
        private EditText InputText;
        private Button InputButton;

        private CoordinatorLayout SnackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.RFBotLayout);

            StatusText = FindViewById<TextView>(Resource.Id.RFBotStatusText);
            InputText = FindViewById<EditText>(Resource.Id.RFBotInputText);
            InputButton = FindViewById<Button>(Resource.Id.RFBotInputButton);
            InputButton.Click += InputButton_Click;
            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.RFBotSnackbarLayout);
        }

        private async void InputButton_Click(object sender, EventArgs e)
        {
            try
            {
                InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);
                imm.HideSoftInputFromWindow(InputText.WindowToken, 0);

                if (InputText.Text == "connect to Zina OS")
                {
                    StartOS();
                }
                else if (InputText.Text == "Hidden Hint")
                {
                    InputText.Text = "";
                    StatusText.Text = "";

                    string temp_text1 = "## Hidden OS Connect Code Hint ##\n\n";

                    for (int i = 0; i < temp_text1.Length; ++i)
                    {
                        StatusText.Text += temp_text1[i];
                        await Task.Delay(10);
                    }

                    await Task.Delay(1000);

                    temp_text1 = "이번 코드는 Operating System, 즉 운영체제 이름입니다.\n\n";

                    for (int i = 0; i < temp_text1.Length; ++i)
                    {
                        StatusText.Text += temp_text1[i];
                        await Task.Delay(10);
                    }

                    await Task.Delay(1000);

                    temp_text1 = "- I.O.P에서 생산한 GRIFON 측 인형들에게 탑재되는 OS 이름 -\n\n";

                    for (int i = 0; i < temp_text1.Length; ++i)
                    {
                        StatusText.Text += temp_text1[i];
                        await Task.Delay(10);
                    }

                    await Task.Delay(1000);

                    temp_text1 = "* 코드 입력 방법 *\n\n";

                    for (int i = 0; i < temp_text1.Length; ++i)
                    {
                        StatusText.Text += temp_text1[i];
                        await Task.Delay(10);
                    }

                    await Task.Delay(1000);

                    temp_text1 = "connect to ____ OS";

                    for (int i = 0; i < temp_text1.Length; ++i)
                    {
                        StatusText.Text += temp_text1[i];
                        await Task.Delay(500);
                    }
                }
                else throw new Exception();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.RFBot_TestMode, Snackbar.LengthShort, Android.Graphics.Color.Coral);
            }
            finally
            {
                InputText.Text = "";
            }
        }

        private async Task StartOS()
        {
            try
            {
                Random R = new Random(DateTime.Now.Second);

                StatusText.Text = "";
                await AnimateText("Connecting 127.0.0.1", R.Next(3, 6));

                await Task.Delay(1000);

                EnterZinaOS();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }

        private void EnterZinaOS()
        {
            StartActivity(typeof(ZinaOS));
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }

        private async Task AnimateText(string message, int loopCount)
        {
            StatusText.Text += message;

            for (int i = 0; i < loopCount; ++i)
            {
                StatusText.Text += '.';
                await Task.Delay(500);
            }

            StatusText.Text += "\n";
        }

        private async Task AnimateText(string message, int loopCount, string aftermessage)
        {
            StatusText.Text += message;

            for (int i = 0; i < loopCount; ++i)
            {
                StatusText.Text += '.';
                await Task.Delay(500);
            }

            StatusText.Text += aftermessage + "\n";
        }
    }
}