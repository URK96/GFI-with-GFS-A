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
    [Activity(Name = "com.gfl.dic.EventListActivity", Label = "@string/Activity_EventListActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EventListActivity : BaseAppCompatActivity
    {
        enum EventPeriodType { Now, Scheduled, Over }

        private LinearLayout eventListSubLayout;
        private LinearLayout eventListSubLayout2;

        private int eventCount = 0;
        private int nowEventCount = 0;
        private int scheduledEventCount = 0;
        private string[] eventURLs;
        private string[] eventPeriods;
        private readonly string eventFilePath = Path.Combine(ETC.cachePath, "Event", "EventVer.txt");

        private CoordinatorLayout snackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.EventListLayout);

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.EventListMainToolbar));
            SupportActionBar.SetTitle(Resource.String.EventListActivity_Title);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            ETC.SetDialogTheme();

            snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EventListSnackbarLayout);

            eventListSubLayout = FindViewById<LinearLayout>(Resource.Id.EventListButtonSubLayout);
            eventListSubLayout2 = FindViewById<LinearLayout>(Resource.Id.EventListButtonSubLayout2);

            _ = InitLoad();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            try
            {
                MenuInflater.Inflate(Resource.Menu.EventListMenu, menu);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Cannot create option menu", ToastLength.Short).Show();
            }

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.RefreshOldGFDImageCache:
                    _ = UpdateEvent();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async Task InitLoad()
        {
            await Task.Delay(100);

            try
            {
                if (await CheckEventVersion())
                {
                    await UpdateEvent();
                }

                TextView period1 = FindViewById<TextView>(Resource.Id.EventPeriodText1);
                Button button1 = FindViewById<Button>(Resource.Id.EventButton1);
                TextView period2 = FindViewById<TextView>(Resource.Id.EventPeriodText2);
                Button button2 = FindViewById<Button>(Resource.Id.EventButton2);

                string[] temp;

                using (StreamReader sr = new StreamReader(new FileStream(eventFilePath, FileMode.Open, FileAccess.Read)))
                {
                    temp = sr.ReadToEnd().Split(';');
                }

                eventCount = int.Parse(temp[2]);
                eventURLs = new string[eventCount];
                eventURLs = temp[4].Split(',');
                eventPeriods = new string[eventCount];
                eventPeriods = temp[5].Split(',');

                nowEventCount = 0;
                scheduledEventCount = 0;

                eventListSubLayout.RemoveAllViews();
                eventListSubLayout2.RemoveAllViews();

                for (int i = 0; i < eventCount; ++i)
                {
                    EventPeriodType type;
                    string[] EventPeriod = eventPeriods[i].Split(' ');

                    type = CheckEventPeriod(EventPeriod);

                    if (type == EventPeriodType.Over)
                    {
                        continue;
                    }

                    string textPeriod = eventPeriods[i];
                    Android.Graphics.Drawables.Drawable eventImage = Android.Graphics.Drawables.Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "Event", "Images", "Event_" + (i + 1) + ".png"));

                    if (type == EventPeriodType.Now)
                    {
                        nowEventCount += 1;

                        if (nowEventCount == 1)
                        {
                            period1.Text = textPeriod;
                            button1.Background = eventImage;
                            button1.Tag = i;
                            button1.Click += EventButton_Click;
                        }
                        else
                        {
                            TextView period = new TextView(this);
                            Button button = new Button(this);

                            period.LayoutParameters = period1.LayoutParameters;
                            button.LayoutParameters = button1.LayoutParameters;

                            period.Text = textPeriod;
                            period.Gravity = GravityFlags.Center;

                            button.Background = eventImage;
                            button.Tag = i;
                            button.Click += EventButton_Click;

                            eventListSubLayout.AddView(period);
                            eventListSubLayout.AddView(button);
                        }
                    }
                    else if (type == EventPeriodType.Scheduled)
                    {
                        scheduledEventCount += 1;

                        if (scheduledEventCount == 1)
                        {
                            period2.Text = textPeriod;
                            button2.Background = eventImage;
                            button2.Tag = i;
                            button2.Click += EventButton_Click;
                        }
                        else
                        {
                            TextView period = new TextView(this);
                            Button button = new Button(this);

                            period.LayoutParameters = period2.LayoutParameters;
                            button.LayoutParameters = button2.LayoutParameters;

                            period.Text = textPeriod;
                            period.Gravity = GravityFlags.Center;

                            button.Background = eventImage;
                            button.Tag = i;
                            button.Click += EventButton_Click;

                            eventListSubLayout2.AddView(period);
                            eventListSubLayout2.AddView(button);
                        }
                    }
                }

                if (nowEventCount == 0)
                {
                    period1.Text = Resources.GetString(Resource.String.EventList_NoEvent);
                    button1.Visibility = ViewStates.Gone;
                }

                if (scheduledEventCount == 0)
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
                ETC.ShowSnackbar(snackbarLayout, Resource.String.InitLoad_Error, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private static async Task<bool> CheckEventVersion()
        {
            await ETC.CheckServerNetwork();

            if (ETC.isServerDown)
            {
                return false;
            }

            string localEventVerPath = Path.Combine(ETC.cachePath, "Event", "EventVer.txt");
            string serverEventVerPath = Path.Combine(ETC.server, "EventVer.txt");
            string tempEventVerPath = Path.Combine(ETC.tempPath, "EventVer.txt");

            bool hasEventUpdate = false;

            if (!File.Exists(localEventVerPath))
            {
                hasEventUpdate = true;
            }
            else
            {
                using (WebClient wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(serverEventVerPath, tempEventVerPath);
                }

                await Task.Delay(1);

                using (StreamReader sr1 = new StreamReader(new FileStream(localEventVerPath, FileMode.Open, FileAccess.Read)))
                using (StreamReader sr2 = new StreamReader(new FileStream(tempEventVerPath, FileMode.Open, FileAccess.Read)))
                {
                    int localVer = int.Parse(sr1.ReadToEnd().Split(';')[1]);
                    int serverVer = int.Parse(sr2.ReadToEnd().Split(';')[1]);

                    hasEventUpdate = localVer < serverVer;
                }
            }

            return hasEventUpdate;
        }

        private EventPeriodType CheckEventPeriod(string[] period)
        {
            int startYear = int.Parse(period[0].Split('/')[0]);
            int startMonth = int.Parse(period[0].Split('/')[1]);
            int startDay = int.Parse(period[0].Split('/')[2]);
            int startHour = int.Parse(period[1].Split(':')[0]);
            int startMinute = int.Parse(period[1].Split(':')[1]);

            int endYear = int.Parse(period[3].Split('/')[0]);
            int endMonth = int.Parse(period[3].Split('/')[1]);
            int endDay = int.Parse(period[3].Split('/')[2]);
            int endHour = int.Parse(period[4].Split(':')[0]);
            int endMinute = int.Parse(period[4].Split(':')[1]);

            DateTime now = DateTime.Now;
            DateTime start = new DateTime(startYear, startMonth, startDay, startHour, startMinute, 0);
            DateTime end = new DateTime(endYear, endMonth, endDay, endHour, endMinute, 0);

            if (DateTime.Compare(now, start) < 0)
            {
                return EventPeriodType.Scheduled;
            }
            else if (DateTime.Compare(now, end) > 0)
            {
                return EventPeriodType.Over;
            }
            else
            {
                return EventPeriodType.Now;
            }
        }

        private void EventButton_Click(object sender, EventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                var intent = new Intent(this, typeof(WebBrowserActivity));
                intent.PutExtra("url", eventURLs[Convert.ToInt32(bt.Tag)]);
                StartActivity(intent);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
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

                if (!Directory.Exists(ETC.tempPath))
                {
                    Directory.CreateDirectory(ETC.tempPath);
                }
                if (!Directory.Exists(Path.Combine(ETC.cachePath, "Event", "Images")))
                {
                    Directory.CreateDirectory(Path.Combine(ETC.cachePath, "Event", "Images"));
                }

                using (WebClient wc = new WebClient())
                {
                    string url = Path.Combine(ETC.server, "EventVer.txt");
                    string target = Path.Combine(ETC.tempPath, "EventVer.txt");

                    await wc.DownloadFileTaskAsync(url, target);
                    await Task.Delay(500);

                    nowProgressBar.Indeterminate = false;
                    totalProgressBar.Indeterminate = false;
                    totalProgressBar.Progress = 0;

                    wc.DownloadProgressChanged += (sender, e) =>
                    {
                        nowProgressBar.Progress = e.ProgressPercentage;
                        nowProgress.Text = e.BytesReceived > 2048 ? $"{e.BytesReceived / 1024}KB" : $"{e.BytesReceived}B";
                    };
                    wc.DownloadFileCompleted += (sender, e) =>
                    {
                        totalProgressBar.Progress += 1;
                        totalProgress.Text = $"{totalProgressBar.Progress} / {totalProgressBar.Max}";
                    };

                    int totalCount = 0;

                    using (StreamReader sr = new StreamReader(new FileStream(Path.Combine(ETC.tempPath, "EventVer.txt"), FileMode.Open, FileAccess.Read)))
                    {
                        totalCount += int.Parse(sr.ReadToEnd().Split(';')[2]);
                    }

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

                    ETC.CopyFile(newVersion, oldVersion);

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