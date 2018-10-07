using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.RFBotActivity", Label = "RFBot", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class RFBotMainActivity : AppCompatActivity
    {
        enum DataRowType { Doll, Equip, Fairy, Enemy }

        private TextView StatusText;
        private EditText InputText;
        private Button InputButton;

        private CoordinatorLayout SnackbarLayout;

        private RFBot Bot;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.RFBotLayout);

            SetTitle(Resource.String.RFBotMain_Title);

            ETC.Resources = Resources;

            Bot = new RFBot(this);

            StatusText = FindViewById<TextView>(Resource.Id.RFBotStatusText);
            InputText = FindViewById<EditText>(Resource.Id.RFBotInputText);
            InputButton = FindViewById<Button>(Resource.Id.RFBotInputButton);
#if DEBUG
            InputButton.Click += InputButton_Click;
#endif
            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.RFBotSnackbarLayout);
#if RELEASE
            StatusText.Text = "RFBot is Maintenance......\nAlso, you can't connect Zina OS on RFBot after this version. You can connect Zina OS in Settings.";
#endif
        }

        private async void InputButton_Click(object sender, EventArgs e)
        {
            try
            {
                InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);
                imm.HideSoftInputFromWindow(InputText.WindowToken, 0);

                InputText.Enabled = false;
                InputText.Hint = ETC.Resources.GetString(Resource.String.RFBotMain_InputTextHint);

                await AnimateText(Bot.InputCommand(InputText.Text), 10, true);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.RFBot_DisunderstandInput, Snackbar.LengthShort, Android.Graphics.Color.Coral);
            }
            finally
            {
                InputText.Text = "";
                InputText.Enabled = true;
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }

        private async Task AnimateText(string message, int delay, bool BeforeClear)
        {
            if (BeforeClear == true) StatusText.Text = "";

            for (int i = 0; i < message.Length; ++i)
            {
                StatusText.Text += message[i];
                await Task.Delay(delay);
            }
        }

        private async Task AnimateText(string message, string loopMessage, int loopCount, int delay)
        {
            StatusText.Text += message;

            for (int i = 0; i < loopCount; ++i)
            {
                StatusText.Text += loopMessage;
                await Task.Delay(delay);
            }

            StatusText.Text += "\n";
        }
    }
}