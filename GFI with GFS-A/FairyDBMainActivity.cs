using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using Android.Text;
using Android.Content;

namespace GFI_with_GFS_A
{
    [Activity(Label = "요정 목록", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FairyDBMainActivity : AppCompatActivity
    {
        delegate void DownloadProgress();

        private List<FairyListBasicInfo> mFairyList = new List<FairyListBasicInfo>();
        private List<int> Download_List = new List<int>();

        int[] TypeFilters = { Resource.Id.FairyFilterTypeCombat, Resource.Id.FairyFilterTypeStrategy };
        int[] ProductTimeFilters = { Resource.Id.FairyFilterProductHour, Resource.Id.FairyFilterProductMinute };

        int p_now = 0;
        int p_total = 0;

        private enum LineUp { Name, ProductTime }
        private LineUp LineUpStyle = LineUp.Name;

        private bool[] Filter_Type = { true, true };
        private int[] Filter_ProductTime = { 0, 0 };
        private bool CanRefresh = false;

        private ListView mFairyListView = null;
        private CoordinatorLayout SnackbarLayout = null;

        private EditText SearchText = null;

        private Dialog dialog = null;
        private ProgressBar totalProgressBar = null;
        private ProgressBar nowProgressBar = null;
        private TextView totalProgress = null;
        private TextView nowProgress = null;
        private FloatingActionButton refresh_fab = null;
        private FloatingActionButton filter_fab = null;
        private FloatingActionButton array_fab = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.FairyDBListLayout);

                SetTitle(Resource.String.FairyDBMainActivity_Title);

                CanRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                mFairyListView = FindViewById<ListView>(Resource.Id.FairyDBListView);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FairyDBSnackbarLayout);

                SearchText = FindViewById<EditText>(Resource.Id.FairySearchText);

                InitializeView();

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<LinearLayout>(Resource.Id.FairySearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.FairySearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.FairySearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                    mFairyListView.Divider = new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.LightGray);
                }

                InitProcess();

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });

                mFairyListView.FastScrollEnabled = true;
                mFairyListView.FastScrollAlwaysVisible = false;
                mFairyListView.ItemClick += MFairyListView_ItemClick;
                mFairyListView.ScrollStateChanged += MFairyListView_ScrollStateChanged;

                if ((ETC.Language.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true) == true)) ETC.RunHelpActivity(this, "DBList");
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void MFairyListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            try
            {
                switch (e.ScrollState)
                {
                    case ScrollState.TouchScroll:
                        if (CanRefresh == true) refresh_fab.Hide();
                        filter_fab.Hide();
                        array_fab.Hide();
                        break;
                    case ScrollState.Idle:
                        if (CanRefresh == true) refresh_fab.Show();
                        filter_fab.Show();
                        array_fab.Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeStatusError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void MFairyListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string FairyDicNum = mFairyList[e.Position].FairyDR["DicNumber"].ToString();
            var FairyInfo = new Intent(this, typeof(FairyDBDetailActivity));
            FairyInfo.PutExtra("DicNum", FairyDicNum);
            StartActivity(FairyInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void InitializeView()
        {
            refresh_fab = FindViewById<FloatingActionButton>(Resource.Id.FairyRefreshCacheFAB);
            if (CanRefresh == false) refresh_fab.Hide();
            else
            {
                if (refresh_fab.HasOnClickListeners == false) refresh_fab.Click += delegate { ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(FairyCropImageDownloadProcess)); };

                refresh_fab.LongClick += MainFAB_fab_LongClick;
            }

            filter_fab = FindViewById<FloatingActionButton>(Resource.Id.FairyFilterFAB);
            if (filter_fab.HasOnClickListeners == false) filter_fab.Click += Filter_Fab_Click;
            filter_fab.LongClick += MainFAB_fab_LongClick;

            array_fab = FindViewById<FloatingActionButton>(Resource.Id.FairyArrayFAB);
            if (array_fab.HasOnClickListeners == false) array_fab.Click += Array_fab_Click;
            array_fab.LongClick += MainFAB_fab_LongClick;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.FairySearchResetButton);
            if (SearchResetButton.HasOnClickListeners == false) SearchResetButton.Click += SearchResetButton_Click;

            SearchText.TextChanged += SearchText_TextChanged;
        }

        private void MainFAB_fab_LongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                string tip = "";

                switch (fab.Id)
                {
                    case Resource.Id.FairyRefreshCacheFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_CacheRefresh);
                        break;
                    case Resource.Id.FairyFilterFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_Filter);
                        break;
                    case Resource.Id.FairyArrayFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_LineUp);
                        break;
                }

                Toast.MakeText(this, tip, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void Array_fab_Click(object sender, EventArgs e)
        {
            try
            {
                switch (LineUpStyle)
                {
                    case LineUp.Name:
                        LineUpStyle = LineUp.ProductTime;
                        array_fab.SetImageResource(Resource.Drawable.LineUp_ProductTime_Icon);
                        break;
                    case LineUp.ProductTime:
                    default:
                        LineUpStyle = LineUp.Name;
                        array_fab.SetImageResource(Resource.Drawable.LineUp_Name_Icon);
                        break;
                }

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.LineUp_Error, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });
        }

        private void SearchResetButton_Click(object sender, EventArgs e)
        {
            SearchText.Text = "";
        }

        private void Filter_Fab_Click(object sender, EventArgs e)
        {
            InitFilterBox();
        }

        private void InitProcess()
        {
            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
            {
                if (CheckFairyCropImage() == true) ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(FairyCropImageDownloadProcess));
            }
        }

        private bool CheckFairyCropImage()
        {
            Download_List.Clear();

            for (int i = 0; i < ETC.FairyList.Rows.Count; ++i)
            {
                DataRow dr = ETC.FairyList.Rows[i];
                int FairyDicNumber = (int)dr["DicNumber"];
                string FilePath = System.IO.Path.Combine(ETC.CachePath, "Fairy", "Normal_Crop", FairyDicNumber + ".gfdcache");
                if (System.IO.File.Exists(FilePath) == false) Download_List.Add(FairyDicNumber);
            }

            Download_List.TrimExcess();

            if (Download_List.Count == 0) return false;
            else return true;
        }

        private void ShowDownloadCheckMessage(int title, int message, DownloadProgress method)
        {
            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG);
            ad.SetTitle(title);
            ad.SetMessage(message);
            ad.SetCancelable(true);
            ad.SetPositiveButton(Resource.String.AlertDialog_Download, delegate { method(); });
            ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

            ad.Show();
        }

        private async void FairyCropImageDownloadProcess()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Download);
            pd.SetTitle(Resource.String.DBList_DownloadCropImageTitle);
            pd.SetCancelable(false);
            pd.SetView(v);

            dialog = pd.Create();
            dialog.Show();

            try
            {
                totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
                totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
                nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
                nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

                p_total = 0;
                p_total = Download_List.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    for (int i = 0; i < p_total; ++i)
                    {
                        int filename = Download_List[i];
                        string url = System.IO.Path.Combine(ETC.Server, "Data", "Images", "Fairy", "Normal_Crop", filename + ".png");
                        string target = System.IO.Path.Combine(ETC.CachePath, "Fairy", "Normal_Crop", filename + ".gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageFail, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
            finally
            {
                dialog.Dismiss();
                dialog = null;
                totalProgressBar = null;
                totalProgress = null;
                nowProgressBar = null;
                nowProgress = null;
            }
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            p_now += 1;

            totalProgressBar.Progress = Convert.ToInt32((p_now / Convert.ToDouble(p_total)) * 100);
            totalProgress.Text = string.Format("{0}%", totalProgressBar.Progress);
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            nowProgressBar.Progress = e.ProgressPercentage;
            nowProgress.Text = string.Format("{0}%", e.ProgressPercentage);
        }

        private void InitFilterBox()
        {
            var inflater = LayoutInflater;

            try
            {
                View v = inflater.Inflate(Resource.Layout.FairyFilterLayout, null);

                v.FindViewById<NumberPicker>(Resource.Id.FairyFilterProductHour).MaxValue = 12;
                v.FindViewById<NumberPicker>(Resource.Id.FairyFilterProductMinute).MaxValue = 59;

                for (int i = 0; i < TypeFilters.Length; ++i) v.FindViewById<CheckBox>(TypeFilters[i]).Checked = Filter_Type[i];
                for (int i = 0; i < ProductTimeFilters.Length; ++i) v.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value = Filter_ProductTime[i];

                Android.Support.V7.App.AlertDialog.Builder FilterBox = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Vertical);
                FilterBox.SetTitle(Resource.String.DBList_FilterBoxTitle);
                FilterBox.SetView(v);
                FilterBox.SetPositiveButton(Resource.String.AlertDialog_Set, delegate { ApplyFilter(v); });
                FilterBox.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                FilterBox.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate { ResetFilter(v); });

                FilterBox.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FilterBox_InitError, Snackbar.LengthLong);
            }
        }

        private void ApplyFilter(View view)
        {
            try
            {
                for (int i = 0; i < TypeFilters.Length; ++i) Filter_Type[i] = view.FindViewById<CheckBox>(TypeFilters[i]).Checked;
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = view.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value;

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void ResetFilter(View view)
        {
            try
            {
                for (int i = 0; i < TypeFilters.Length; ++i) Filter_Type[i] = true;
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = 0;

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListFairy(string searchText, int[] p_time)
        {
            //ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_Listing, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);

            mFairyList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < ETC.FairyList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.FairyList.Rows[i];

                    if ((p_time[0] + p_time[1]) != 0)
                    {
                        if ((int)dr["ProductTime"] != ((p_time[0] * 60) + p_time[1])) continue;
                    }

                    if (CheckFilter(dr) == true) continue;
                    if (searchText != "")
                    {
                        string name = ((string)dr["Name"]).ToUpper();
                        if (name.Contains(searchText) == false) continue;
                    }

                    FairyListBasicInfo info = new FairyListBasicInfo()
                    {
                        Id = i,
                        FairyDR = dr
                    };
                    mFairyList.Add(info);
                }

                mFairyList.Sort(SortFairyName);

                var adapter = new FairyListAdapter(this, mFairyList);

                await Task.Delay(100);

                RunOnUiThread(() => { mFairyListView.Adapter = adapter; });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private int SortFairyName(FairyListBasicInfo x, FairyListBasicInfo y)
        {
            switch (LineUpStyle)
            {
                case LineUp.ProductTime:
                    int x_time = (int)x.FairyDR["ProductTime"];
                    int y_time = (int)y.FairyDR["ProductTime"];
                    if ((x_time == 0) && (y_time != 0)) return 1;
                    else if ((y_time == 0) && (x_time != 0)) return -1;
                    else if (x_time == y_time)
                    {
                        string x_name_t = (string)x.FairyDR["Name"];
                        string y_name_t = (string)y.FairyDR["Name"];
                        return x_name_t.CompareTo(y_name_t);
                    }
                    else return x_time.CompareTo(y_time);
                case LineUp.Name:
                default:
                    string x_name = (string)x.FairyDR["Name"];
                    string y_name = (string)y.FairyDR["Name"];
                    return x_name.CompareTo(y_name);
            }
        }

        private bool CheckFilter(DataRow dr)
        {
            string type = (string)dr["Type"];

            switch (type)
            {
                case string s when s == Resources.GetString(Resource.String.Common_FairyType_Combat):
                    if (Filter_Type[0] == false) return true;
                    break;
                case string s when s == Resources.GetString(Resource.String.Common_FairyType_Strategy):
                    if (Filter_Type[1] == false) return true;
                    break;
            }

            return false;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }

    class FairyListBasicInfo
    {
        public int Id { set; get; }
        public DataRow FairyDR { set; get; }
    }

    class FairyListAdapter : BaseAdapter<FairyListBasicInfo>
    {
        List<FairyListBasicInfo> mitems;
        Activity mcontext;
        int count = 0;

        public FairyListAdapter(Activity context, List<FairyListBasicInfo> items) : base()
        {
            mcontext = context;
            mitems = items;
        }

        public override FairyListBasicInfo this[int position]
        {
            get { return mitems[position]; }
        }

        public override int Count
        {
            get { return mitems.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return mitems[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = mitems[position];
            var view = convertView;

            try
            {
                if (view == null) view = mcontext.LayoutInflater.Inflate(Resource.Layout.FairyListLayout, null);

                count += 1;
                if (count == 50)
                {
                    GC.Collect();
                    count = 0;
                }

                ImageView FairyTypeIcon = view.FindViewById<ImageView>(Resource.Id.FairyListType);
                int TypeIconId = 0;
                switch ((string)item.FairyDR["Type"])
                {
                    case string s when s == ETC.Resources.GetString(Resource.String.Common_FairyType_Combat):
                        TypeIconId = Resource.Drawable.Fairy_Combat;
                        break;
                    case string s when s == ETC.Resources.GetString(Resource.String.Common_FairyType_Strategy):
                        TypeIconId = Resource.Drawable.Fairy_Strategy;
                        break;
                    default:
                        TypeIconId = Resource.Drawable.Fairy_Combat;
                        break;
                }
                FairyTypeIcon.SetImageResource(TypeIconId);

                int F_DicNumber = (int)item.FairyDR["DicNumber"];

                ImageView FairySmallImage = view.FindViewById<ImageView>(Resource.Id.FairyListSmallImage);
                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    FairySmallImage.Visibility = ViewStates.Visible;
                    string FilePath = System.IO.Path.Combine(ETC.CachePath, "Fairy", "Normal_Crop", F_DicNumber + ".gfdcache");
                    if (System.IO.File.Exists(FilePath) == true) FairySmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                }
                else FairySmallImage.Visibility = ViewStates.Gone;

                TextView FairyName = view.FindViewById<TextView>(Resource.Id.FairyListName);
                FairyName.Text = (string)item.FairyDR["Name"];

                TextView FairyProductTime = view.FindViewById<TextView>(Resource.Id.FairyListProductTime);

                FairyProductTime.Text = ETC.CalcTime((int)item.FairyDR["ProductTime"]);
            }
            catch (Exception ex)
            {
                ETC.LogError(mcontext, ex 
                    .ToString());
                Toast.MakeText(mcontext, "Error Create View", ToastLength.Short).Show();
            }

            return view;
        }
    }
}