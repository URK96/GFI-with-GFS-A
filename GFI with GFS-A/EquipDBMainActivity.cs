using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "장비 목록", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EquipDBMainActivity : AppCompatActivity
    {
        delegate void DownloadProgress();

        private List<EquipListBasicInfo> mEquipList = new List<EquipListBasicInfo>();

        int[] GradeFilters = { Resource.Id.EquipFilterGrade2, Resource.Id.EquipFilterGrade3, Resource.Id.EquipFilterGrade4, Resource.Id.EquipFilterGrade5, Resource.Id.EquipFilterGradeExtra };
        int[] CategoryFilters = { Resource.Id.EquipFilterCategoryAttach, Resource.Id.EquipFilterCategoryBullet, Resource.Id.EquipFilterCategoryDoll };
        int[] ProductTimeFilters = { Resource.Id.EquipFilterProductHour, Resource.Id.EquipFilterProductMinute, Resource.Id.EquipFilterProductNearRange };

        int p_now = 0;
        int p_total = 0;

        private enum LineUp { Name, ProductTime }
        private LineUp LineUpStyle = LineUp.Name;

        private bool[] Filter_Grade = { true, true, true, true, true };
        private bool[] Filter_Category = { true, true, true};
        private int[] Filter_ProductTime = { 0, 0, 0 };
        private bool CanRefresh = false;

        private ListView mEquipListView = null;
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
                SetContentView(Resource.Layout.EquipDBListLayout);

                SetTitle(Resource.String.EquipDBMainActivity_Title);

                CanRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                mEquipListView = FindViewById<ListView>(Resource.Id.EquipDBListView);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EquipDBSnackbarLayout);

                SearchText = FindViewById<EditText>(Resource.Id.EquipSearchText);

                InitializeView();

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<LinearLayout>(Resource.Id.EquipSearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.EquipSearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.EquipSearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                    mEquipListView.Divider = new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.LightGray);
                }

                InitProcess();

                ListEquip(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);

                mEquipListView.FastScrollEnabled = true;
                mEquipListView.FastScrollAlwaysVisible = false;
                mEquipListView.ItemClick += MEquipListView_ItemClick;
                mEquipListView.ScrollStateChanged += MEquipListView_ScrollStateChanged;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void MEquipListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
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
                        if (CanRefresh == true) refresh_fab.Hide();
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

        private void MEquipListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string EquipName = (string)(mEquipList[e.Position].EquipDR)["Name"];
            var EquipInfo = new Intent(this, typeof(EquipDBDetailActivity));
            EquipInfo.PutExtra("Keyword", EquipName);
            StartActivity(EquipInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void InitializeView()
        {
            refresh_fab = FindViewById<FloatingActionButton>(Resource.Id.EquipRefreshCacheFAB);
            if (CanRefresh == false) refresh_fab.Hide();
            else
            {
                if (refresh_fab.HasOnClickListeners == false) refresh_fab.Click += delegate { ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(EquipCropImageDownloadProcess)); };
            }

            filter_fab = FindViewById<FloatingActionButton>(Resource.Id.EquipFilterFAB);
            if (filter_fab.HasOnClickListeners == false) filter_fab.Click += Filter_fab_Click;

            array_fab = FindViewById<FloatingActionButton>(Resource.Id.EquipArrayFAB);
            if (array_fab.HasOnClickListeners == false) array_fab.Click += Array_fab_Click;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.EquipSearchResetButton);
            if (SearchResetButton.HasOnClickListeners == false) SearchResetButton.Click += SearchResetButton_Click;

            SearchText.TextChanged += SearchText_TextChanged;
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

                ListEquip(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.LineUp_Error, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void InitProcess()
        {
            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
            {
                if (CheckEquipCropImage() == false) ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(EquipCropImageDownloadProcess));
            }
        }

        private bool CheckEquipCropImage()
        {
            for (int i = 0; i < ETC.EquipmentList.Rows.Count; ++i)
            {
                DataRow dr = ETC.EquipmentList.Rows[i];
                string IconName = (string)dr["Icon"];
                string FilePath = System.IO.Path.Combine(ETC.CachePath, "Equip", "Normal", IconName + ".gfdcache");
                if (System.IO.File.Exists(FilePath) == false)
                {
                    Toast.MakeText(this, IconName, ToastLength.Short).Show();
                    return false;
                }
            }

            return true;
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

        private async void EquipCropImageDownloadProcess()
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

                List<string> IconList = new List<string>();

                foreach (DataRow dr in ETC.EquipmentList.Rows)
                {
                    IconList.TrimExcess();
                    bool IsExist = false;
                    string icon = (string)dr["Icon"];
                    foreach (string s in IconList)
                    {
                        if (s == icon)
                        {
                            IsExist = true;
                            break;
                        }
                    }

                    if (IsExist == true) continue;

                    IconList.Add(icon);
                }

                IconList.TrimExcess();

                p_total = 0;
                p_total = IconList.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    for (int i = 0; i < p_total; ++i)
                    {
                        string filename = (string)IconList[i];
                        string url = System.IO.Path.Combine(ETC.Server, "Data", "Images", "Equipments", filename + ".png");
                        string target = System.IO.Path.Combine(ETC.CachePath, "Equip", "Normal", filename + ".gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListEquip(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
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

        private void SearchText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            ListEquip(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
        }

        private void SearchResetButton_Click(object sender, EventArgs e)
        {
            SearchText.Text = "";
        }

        private void Filter_fab_Click(object sender, EventArgs e)
        {
            InitFilterBox();
        }

        private int SortEquipName(EquipListBasicInfo x, EquipListBasicInfo y)
        {
            switch (LineUpStyle)
            {
                case LineUp.ProductTime:
                    int x_time = (int)x.EquipDR["ProductTime"];
                    int y_time = (int)y.EquipDR["ProductTime"];
                    if ((x_time == 0) && (y_time != 0)) return 1;
                    else if ((y_time == 0) && (x_time != 0)) return -1;
                    else if (x_time == y_time)
                    {
                        string x_name_t = (string)x.EquipDR["Name"];
                        string y_name_t = (string)y.EquipDR["Name"];
                        return x_name_t.CompareTo(y_name_t);
                    }
                    else return x_time.CompareTo(y_time);
                case LineUp.Name:
                default:
                    string x_name = (string)x.EquipDR["Name"];
                    string y_name = (string)y.EquipDR["Name"];
                    return x_name.CompareTo(y_name);
            }
        }

        private void InitFilterBox()
        {
            var inflater = LayoutInflater;

            try
            {
                View v = inflater.Inflate(Resource.Layout.EquipFilterLayout, null);

                v.FindViewById<NumberPicker>(Resource.Id.EquipFilterProductHour).MaxValue = 0;
                v.FindViewById<NumberPicker>(Resource.Id.EquipFilterProductMinute).MaxValue = 59;
                v.FindViewById<NumberPicker>(Resource.Id.EquipFilterProductNearRange).MaxValue = 10;

                for (int i = 0; i < GradeFilters.Length; ++i) v.FindViewById<CheckBox>(GradeFilters[i]).Checked = Filter_Grade[i];
                for (int i = 0; i < CategoryFilters.Length; ++i) v.FindViewById<CheckBox>(CategoryFilters[i]).Checked = Filter_Category[i];
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
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FilterBox_InitError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void ApplyFilter(View view)
        {
            try
            {
                for (int i = 0; i < GradeFilters.Length; ++i) Filter_Grade[i] = view.FindViewById<CheckBox>(GradeFilters[i]).Checked;
                for (int i = 0; i < CategoryFilters.Length; ++i) Filter_Category[i] = view.FindViewById<CheckBox>(CategoryFilters[i]).Checked;
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = view.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value;

                ListEquip(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
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
                for (int i = 0; i < GradeFilters.Length; ++i) Filter_Grade[i] = true;
                for (int i = 0; i < CategoryFilters.Length; ++i) Filter_Category[i] = true;
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = 0;

                ListEquip(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListEquip(string searchText, int[] p_time, int p_range)
        {
            //ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_Listing, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);

            mEquipList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < ETC.EquipmentList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.EquipmentList.Rows[i];

                    if ((p_time[0] + p_time[1]) != 0)
                    {
                        if (CheckEquipByProductTime(p_time, p_range, (int)dr["ProductTime"]) == false) continue;
                    }

                    if (CheckFilter(dr) == true) continue;
                    if (searchText != "")
                    {
                        string name = ((string)dr["Name"]).ToUpper();
                        if (name.Contains(searchText) == false) continue;
                    }

                    EquipListBasicInfo info = new EquipListBasicInfo()
                    {
                        Id = i,
                        EquipDR = dr
                    };
                    mEquipList.Add(info);
                }

                mEquipList.Sort(SortEquipName);

                var adapter = new EquipListAdapter(this, mEquipList);

                await Task.Delay(100);

                RunOnUiThread(() => { mEquipListView.Adapter = adapter; });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private bool CheckEquipByProductTime(int[] p_time, int range, int d_time)
        {
            int p_time_minute = (p_time[0] * 60) + p_time[1];

            for (int i = (p_time_minute - range); i <= (p_time_minute + range); ++i)
            {
                if (d_time == i) return true;
            }

            return false;
        }

        private bool CheckFilter(DataRow dr)
        {
            int grade = (int)dr["Grade"];
            string category = (string)dr["Category"];

            switch (grade)
            {
                case 2:
                    if (Filter_Grade[0] == false) return true;
                    break;
                case 3:
                    if (Filter_Grade[1] == false) return true;
                    break;
                case 4:
                    if (Filter_Grade[2] == false) return true;
                    break;
                case 5:
                    if (Filter_Grade[3] == false) return true;
                    break;
                case 0:
                    if (Filter_Grade[4] == false) return true;
                    break;
            }

            switch (category)
            {
                case string s when s == Resources.GetString(Resource.String.Common_Accessories):
                    if (Filter_Category[0] == false) return true;
                    break;
                case string s when s == Resources.GetString(Resource.String.Common_Magazine):
                    if (Filter_Category[1] == false) return true;
                    break;
                case string s when s == Resources.GetString(Resource.String.Common_TDoll):
                    if (Filter_Category[2] == false) return true;
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

    class EquipListBasicInfo
    {
        public int Id { set; get; }
        public DataRow EquipDR { set; get; }
    }

    class EquipListAdapter : BaseAdapter<EquipListBasicInfo>
    {
        List<EquipListBasicInfo> mitems;
        Activity mcontext;
        int count = 0;

        public EquipListAdapter(Activity context, List<EquipListBasicInfo> items) : base()
        {
            mcontext = context;
            mitems = items;
        }

        public override EquipListBasicInfo this[int position]
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
                if (view == null) view = mcontext.LayoutInflater.Inflate(Resource.Layout.EquipListLayout, null);

                count += 1;
                if (count == 50)
                {
                    GC.Collect();
                    count = 0;
                }

                string IconName = (string)item.EquipDR["Icon"];

                ImageView EquipSmallImage = view.FindViewById<ImageView>(Resource.Id.EquipListSmallImage);
                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    EquipSmallImage.Visibility = ViewStates.Visible;
                    string FilePath = System.IO.Path.Combine(ETC.CachePath, "Equip", "Normal", IconName + ".gfdcache");
                    if (System.IO.File.Exists(FilePath) == true) EquipSmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                }
                else EquipSmallImage.Visibility = ViewStates.Gone;

                ImageView EquipGradeIcon = view.FindViewById<ImageView>(Resource.Id.EquipListGrade);
                int GradeIconId = 0;
                switch ((int)item.EquipDR["Grade"])
                {
                    case 2:
                        GradeIconId = Resource.Drawable.Grade_2;
                        break;
                    case 3:
                        GradeIconId = Resource.Drawable.Grade_3;
                        break;
                    case 4:
                        GradeIconId = Resource.Drawable.Grade_4;
                        break;
                    case 5:
                        GradeIconId = Resource.Drawable.Grade_5;
                        break;
                    case 0:
                        GradeIconId = Resource.Drawable.Grade_0;
                        break;
                    default:
                        GradeIconId = Resource.Drawable.Grade_2;
                        break;
                }
                EquipGradeIcon.SetImageResource(GradeIconId);

                TextView EquipCategory = view.FindViewById<TextView>(Resource.Id.EquipListCategory);
                EquipCategory.Text = (string)item.EquipDR["Category"];

                TextView EquipName = view.FindViewById<TextView>(Resource.Id.EquipListName);
                EquipName.Text = (string)item.EquipDR["Name"];

                TextView EquipProductTime = view.FindViewById<TextView>(Resource.Id.EquipListProductTime);
                EquipProductTime.Text = ETC.CalcTime((int)item.EquipDR["ProductTime"]);
            }
            catch (Exception ex)
            {
                ETC.LogError(mcontext, ex.ToString());
                Toast.MakeText(mcontext, "Error Create View", ToastLength.Short).Show();
            }

            return view;
        }
    }
}