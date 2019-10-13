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
using System.Net;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.EventListActivity", Label = "@string/Activity_EventListActivity", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EventListActivity : BaseAppCompatActivity
    {
        enum EventPeriodType { Now, Scheduled, Over }

        private LinearLayout EventListSubLayout;
        private LinearLayout EventListSubLayout2;

        private int EventCount = 0;
        private int NowEventCount = 0;
        private int ScheduledEventCount = 0;
        private string[] EventURLs;
        private string[] EventPeriods;
        private string EventFilePath = Path.Combine(ETC.cachePath, "Event", "EventVer.txt");
       

        private CoordinatorLayout SnackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme == true) SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.EventListLayout);

            ETC.SetDialogTheme();

            SetTitle(Resource.String.EventListActivity_Title);

            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EventListSnackbarLayout);

            EventListSubLayout = FindViewById<LinearLayout>(Resource.Id.EventListButtonSubLayout);
            EventListSubLayout2 = FindViewById<LinearLayout>(Resource.Id.EventListButtonSubLayout2);

            _ = InitLoad();
        }

        private async Task InitLoad()
        {
            await Task.Delay(100);

            try
            {
                if (await CheckEventVersion() == true)
                    await UpdateEvent();

                TextView period1 = FindViewById<TextView>(Resource.Id.EventPeriodText1);
                Button button1 = FindViewById<Button>(Resource.Id.EventButton1);
                TextView period2 = FindViewById<TextView>(Resource.Id.EventPeriodText2);
                Button button2 = FindViewById<Button>(Resource.Id.EventButton2);

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

                NowEventCount = 0;
                ScheduledEventCount = 0;

                EventListSubLayout.RemoveAllViews();
                EventListSubLayout2.RemoveAllViews();

                for (int i = 0; i < EventCount; ++i)
                {
                    EventPeriodType type;
                    string[] EventPeriod = EventPeriods[i].Split(' ');

                    type = CheckEventPeriod(EventPeriod);

                    if (type == EventPeriodType.Over)
                        continue;

                    string text_period = EventPeriods[i];
                    Android.Graphics.Drawables.Drawable event_image = Android.Graphics.Drawables.Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "Event", "Images", "Event_" + (i + 1) + ".png"));

                    if (type == EventPeriodType.Now)
                    {
                        NowEventCount += 1;

                        if (NowEventCount == 1)
                        {
                            period1.Text = text_period;
                            button1.Background = event_image;
                            button1.Tag = i;
                            button1.Click += EventButton_Click;
                        }
                        else
                        {
                            TextView period = new TextView(this);
                            Button button = new Button(this);

                            period.LayoutParameters = period1.LayoutParameters;
                            button.LayoutParameters = button1.LayoutParameters;

                            period.Text = text_period;
                            period.Gravity = GravityFlags.Center;

                            button.Background = event_image;
                            button.Tag = i;
                            button.Click += EventButton_Click;

                            EventListSubLayout.AddView(period);
                            EventListSubLayout.AddView(button);
                        }
                    }
                    else if (type == EventPeriodType.Scheduled)
                    {
                        ScheduledEventCount += 1;

                        if (ScheduledEventCount == 1)
                        {
                            period2.Text = text_period;
                            button2.Background = event_image;
                            button2.Tag = i;
                            button2.Click += EventButton_Click;
                        }
                        else
                        {
                            TextView period = new TextView(this);
                            Button button = new Button(this);

                            period.LayoutParameters = period2.LayoutParameters;
                            button.LayoutParameters = button2.LayoutParameters;

                            period.Text = text_period;
                            period.Gravity = GravityFlags.Center;

                            button.Background = event_image;
                            button.Tag = i;
                            button.Click += EventButton_Click;

                            EventListSubLayout2.AddView(period);
                            EventListSubLayout2.AddView(button);
                        }
                    }
                }

                if (NowEventCount == 0)
                {
                    period1.Text = Resources.GetString(Resource.String.EventList_NoEvent);
                    button1.Visibility = ViewStates.Gone;
                }

                if (ScheduledEventCount == 0)
                {
                    period2.Text = Resources.GetString(Resource.String.EventList_NoEvent);
                    button2.Visibility = ViewStates.Gone;
                }

                /*TextView period1 = FindViewById<TextView>(Resource.Id.EventPeriodText1);
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
                }*/

                FindViewById<LinearLayout>(Resource.Id.EventListButtonLayout).Animate().Alpha(1.0f).SetDuration(500).Start();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.InitLoad_Error, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private static async Task<bool> CheckEventVersion()
        {
            await ETC.CheckServerNetwork();

            if (ETC.isServerDown == true)
                return false;

            string LocalEventVerPath = Path.Combine(ETC.cachePath, "Event", "EventVer.txt");
            string ServerEventVerPath = Path.Combine(ETC.server, "EventVer.txt");
            string TempEventVerPath = Path.Combine(ETC.tempPath, "EventVer.txt");

            bool HasEventUpdate = false;

            if (File.Exists(LocalEventVerPath) == false)
                HasEventUpdate = true;
            else
            {
                using (WebClient wc = new WebClient())
                    await wc.DownloadFileTaskAsync(ServerEventVerPath, TempEventVerPath);

                await Task.Delay(1);

                using (StreamReader sr1 = new StreamReader(new FileStream(LocalEventVerPath, FileMode.Open, FileAccess.Read)))
                using (StreamReader sr2 = new StreamReader(new FileStream(TempEventVerPath, FileMode.Open, FileAccess.Read)))
                {
                    int localVer = int.Parse(sr1.ReadToEnd().Split(';')[1]);
                    int serverVer = int.Parse(sr2.ReadToEnd().Split(';')[1]);

                    if (localVer < serverVer)
                        HasEventUpdate = true;
                }
            }

            return HasEventUpdate;
        }

        private EventPeriodType CheckEventPeriod(string[] period)
        {
            int start_year = int.Parse(period[0].Split('/')[0]);
            int start_month = int.Parse(period[0].Split('/')[1]);
            int start_day = int.Parse(period[0].Split('/')[2]);
            int start_hour = int.Parse(period[1].Split(':')[0]);
            int start_minute = int.Parse(period[1].Split(':')[1]);

            int end_year = int.Parse(period[3].Split('/')[0]);
            int end_month = int.Parse(period[3].Split('/')[1]);
            int end_day = int.Parse(period[3].Split('/')[2]);
            int end_hour = int.Parse(period[4].Split(':')[0]);
            int end_minute = int.Parse(period[4].Split(':')[1]);

            DateTime now = DateTime.Now;
            DateTime start = new DateTime(start_year, start_month, start_day, start_hour, start_minute, 0);
            DateTime end = new DateTime(end_year, end_month, end_day, end_hour, end_minute, 0);

            if (DateTime.Compare(now, start) < 0) return EventPeriodType.Scheduled;
            else if (DateTime.Compare(now, end) > 0) return EventPeriodType.Over;
            else return EventPeriodType.Now;
        }

        private void EventButton_Click(object sender, EventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                var intent = new Intent(this, typeof(WebBrowserActivity));
                intent.PutExtra("url", EventURLs[Convert.ToInt32(bt.Tag)]);
                StartActivity(intent);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async Task UpdateEvent()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            ProgressBar totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
            TextView totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
            ProgressBar nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
            TextView nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

            Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGDownload);
            pd.SetTitle(Resource.String.UpdateEventDialog_Title);
            pd.SetMessage(Resources.GetString(Resource.String.UpdateEventDialog_Message));
            pd.SetView(v);
            pd.SetCancelable(false);

            Dialog dialog = pd.Create();
            dialog.Show();

            await Task.Delay(100);

            try
            {
                nowProgressBar.Indeterminate = true;
                totalProgressBar.Indeterminate = true;

                if (Directory.Exists(ETC.tempPath) == false)
                    Directory.CreateDirectory(ETC.tempPath);
                if (Directory.Exists(Path.Combine(ETC.cachePath, "Event", "Images")) == false)
                    Directory.CreateDirectory(Path.Combine(ETC.cachePath, "Event", "Images"));

                using (WebClient wc = new WebClient())
                {
                    string url = Path.Combine(ETC.server, "EventVer.txt");
                    string target = Path.Combine(ETC.tempPath, "EventVer.txt");
                    await wc.DownloadFileTaskAsync(url, target);
                    await Task.Delay(500);

                    nowProgressBar.Indeterminate = false;
                    totalProgressBar.Indeterminate = false;
                    totalProgressBar.Progress = 0;

                    wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                    {
                        nowProgressBar.Progress = e.ProgressPercentage;
                        nowProgress.Text = e.BytesReceived > 2048 ? $"{e.BytesReceived / 1024}KB" : $"{e.BytesReceived}B";
                    };
                    wc.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                    {
                        totalProgressBar.Progress += 1;
                        totalProgress.Text = $"{totalProgressBar.Progress} / {totalProgressBar.Max}";
                    };

                    int totalCount = 0;

                    using (StreamReader sr = new StreamReader(new FileStream(Path.Combine(ETC.tempPath, "EventVer.txt"), FileMode.Open, FileAccess.Read)))
                        totalCount += int.Parse(sr.ReadToEnd().Split(';')[2]);

                    totalProgressBar.Max = totalCount;

                    for (int i = 1; i <= totalCount; ++i)
                    {
                        string url2 = Path.Combine(ETC.server, "Data", "Images", "Events", "Event_" + i + ".png");
                        string target2 = Path.Combine(ETC.cachePath, "Event", "Images", "Event_" + i + ".png");
                        await wc.DownloadFileTaskAsync(url2, target2);
                        await Task.Delay(100);
                    }

                    await Task.Delay(500);

                    RunOnUiThread(() => { pd.SetMessage(Resources.GetString(Resource.String.UpdateEventDialog_RefreshVersionMessage)); });

                    string oldVersion = Path.Combine(ETC.cachePath, "Event", "EventVer.txt");
                    string newVersion = Path.Combine(ETC.tempPath, "EventVer.txt");
                    File.Copy(newVersion, oldVersion, true);

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
            finally
            {
                dialog.Dismiss();
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }
    }
}