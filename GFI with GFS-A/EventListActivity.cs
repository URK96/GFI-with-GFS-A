using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Support.Design.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Label = "진행 중인 이벤트", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EventListActivity : AppCompatActivity
    {
        private LinearLayout EventListSubLayout;

        private int EventCount = 0;
        private string[] EventURLs;
        private string[] EventPeriods;
        private string EventFilePath = Path.Combine(ETC.CachePath, "Event", "EventVer.txt");
       

        private CoordinatorLayout SnackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.EventListLayout);

            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EventListSnackbarLayout);

            EventListSubLayout = FindViewById<LinearLayout>(Resource.Id.EventListButtonSubLayout);

            InitLoad();
        }

        private async Task InitLoad()
        {
            await Task.Delay(100);

            try
            {
                string[] temp;

                using (StreamReader sr = new StreamReader(new FileStream(EventFilePath, FileMode.Open, FileAccess.Read)))
                {
                    temp = sr.ReadToEnd().Split(';');
                }

                EventCount = int.Parse(temp[2]);
                EventURLs = new string[EventCount];
                EventURLs = temp[4].Split(',');
                EventPeriods = new string[EventCount];
                EventPeriods = temp[5].Split(',');

                TextView period1 = FindViewById<TextView>(Resource.Id.EventPeriodText1);
                Button button1 = FindViewById<Button>(Resource.Id.EventButton1);

                period1.Text = EventPeriods[0];
                button1.Background = Android.Graphics.Drawables.Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Event", "Images", "Event_1.png"));
                button1.Tag = 1;
                button1.Click += EventButton_Click;

                for (int i = 1; i < EventCount; ++i)
                {
                    TextView period = new TextView(this);
                    Button button = new Button(this);

                    period.LayoutParameters = period1.LayoutParameters;
                    button.LayoutParameters = button1.LayoutParameters;

                    period.Text = EventPeriods[i];
                    period.Gravity = GravityFlags.Center;

                    string filename = "Event_" + (i + 1) + ".png";
                    button.Background = Android.Graphics.Drawables.Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Event", "Images", filename));
                    button.Tag = i + 1;
                    button.Click += EventButton_Click;

                    EventListSubLayout.AddView(period);
                    EventListSubLayout.AddView(button);
                }

                FindViewById<LinearLayout>(Resource.Id.EventListButtonLayout).Animate().Alpha(1.0f).SetDuration(500).Start();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, "초기 로딩 실패", Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void EventButton_Click(object sender, EventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (ETC.HasEvent == true)
                {
                    var intent = new Intent(this, typeof(WebBrowserActivity));
                    intent.PutExtra("url", EventURLs[Convert.ToInt32(bt.Tag) - 1]);
                    StartActivity(intent);
                    OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }
    }
}