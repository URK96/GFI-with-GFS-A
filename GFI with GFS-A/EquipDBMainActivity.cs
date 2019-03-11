using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
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

        private List<Equip> RootList = new List<Equip>();
        private List<Equip> SubList = new List<Equip>();
        private List<string> Download_List = new List<string>();

        int[] GradeFilters = { Resource.Id.EquipFilterGrade2, Resource.Id.EquipFilterGrade3, Resource.Id.EquipFilterGrade4, Resource.Id.EquipFilterGrade5, Resource.Id.EquipFilterGradeExtra };
        int[] CategoryFilters = { Resource.Id.EquipFilterCategoryAttach, Resource.Id.EquipFilterCategoryBullet, Resource.Id.EquipFilterCategoryDoll };
        int[] ProductTimeFilters = { Resource.Id.EquipFilterProductHour, Resource.Id.EquipFilterProductMinute, Resource.Id.EquipFilterProductNearRange };

        int p_now = 0;
        int p_total = 0;

        private bool[] HasApplyFilter = { false, false, false };
        private int[] Filter_ProductTime = { 0, 0, 0 };
        private bool[] Filter_Grade = { false, false, false, false, false };
        private bool[] Filter_Category = { false, false, false };
        private bool CanRefresh = false;

        private enum LineUp { Name, ProductTime }
        private LineUp LineUpStyle = LineUp.Name;

        private RecyclerView mEquipListView;
        private RecyclerView.LayoutManager MainRecyclerManager;
        private CoordinatorLayout SnackbarLayout;

        private TextView LineUp_Name;
        private TextView LineUp_Time;

        private EditText SearchText;

        private Dialog dialog;
        private ProgressBar totalProgressBar;
        private ProgressBar nowProgressBar;
        private TextView totalProgress;
        private TextView nowProgress;
        private FloatingActionButton refresh_fab;
        private FloatingActionButton filter_fab;

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

                mEquipListView = FindViewById<RecyclerView>(Resource.Id.EquipDBRecyclerView);
                MainRecyclerManager = new LinearLayoutManager(this);
                mEquipListView.SetLayoutManager(MainRecyclerManager);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EquipDBSnackbarLayout);

                LineUp_Name = FindViewById<TextView>(Resource.Id.EquipDBLineUp_Name);
                LineUp_Name.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                LineUp_Name.Click += LineUp_Text_Click;
                LineUp_Time = FindViewById<TextView>(Resource.Id.EquipDBLineUp_Time);
                LineUp_Time.Click += LineUp_Text_Click;

                SearchText = FindViewById<EditText>(Resource.Id.EquipSearchText);

                InitializeView();

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<LinearLayout>(Resource.Id.EquipSearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.EquipSearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.EquipSearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                }

                InitProcess();

                ListEquip(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);

                if ((ETC.Language.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true) == true)) ETC.RunHelpActivity(this, "DBList");
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void LineUp_Text_Click(object sender, EventArgs e)
        {
            try
            {
                TextView tv = sender as TextView;

                switch (tv.Id)
                {
                    case Resource.Id.EquipDBLineUp_Time:
                        LineUpStyle = LineUp.ProductTime;
                        LineUp_Name.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Time.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                        break;
                    case Resource.Id.EquipDBLineUp_Name:
                    default:
                        LineUpStyle = LineUp.Name;
                        LineUp_Time.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Name.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
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

        /*private void MEquipListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            try
            {
                switch (e.ScrollState)
                {
                    case ScrollState.TouchScroll:
                        if (CanRefresh == true) refresh_fab.Hide();
                        filter_fab.Hide();
                        break;
                    case ScrollState.Idle:
                        if (CanRefresh == true) refresh_fab.Hide();
                        filter_fab.Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeStatusError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }*/

        private async void Adapter_ItemClick(object sender, int position)
        {
            await Task.Delay(100);
            var EquipInfo = new Intent(this, typeof(EquipDBDetailActivity));
            EquipInfo.PutExtra("Id", SubList[position].Id);
            StartActivity(EquipInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void InitializeView()
        {
            refresh_fab = FindViewById<FloatingActionButton>(Resource.Id.EquipRefreshCacheFAB);
            if (CanRefresh == false) refresh_fab.Hide();
            else
            {
                if (refresh_fab.HasOnClickListeners == false) refresh_fab.Click += delegate 
                {
                    Download_List.Clear();
                    foreach (Equip equip in RootList)
                    {
                        string item = equip.Icon;
                        if (Download_List.Contains(item) == false) Download_List.Add(item);
                    }
                    ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(EquipCropImageDownloadProcess));
                };

                refresh_fab.LongClick += MainFAB_LongClick;
            }

            filter_fab = FindViewById<FloatingActionButton>(Resource.Id.EquipFilterFAB);
            if (filter_fab.HasOnClickListeners == false) filter_fab.Click += Filter_fab_Click;
            filter_fab.LongClick += MainFAB_LongClick;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.EquipSearchResetButton);
            if (SearchResetButton.HasOnClickListeners == false) SearchResetButton.Click += SearchResetButton_Click;

            SearchText.TextChanged += SearchText_TextChanged;
        }

        private void MainFAB_LongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                string tip = "";

                switch (fab.Id)
                {
                    case Resource.Id.EquipRefreshCacheFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_CacheRefresh);
                        break;
                    case Resource.Id.EquipFilterFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_Filter);
                        break;
                }

                Toast.MakeText(this, tip, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void InitProcess()
        {
            CreateListObject();

            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
            {
                if (CheckCropImage() == true)
                    ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(EquipCropImageDownloadProcess));
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.EquipmentList.Rows)
                    RootList.Add(new Equip(dr));

                RootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthShort);
            }
        }

        private bool CheckCropImage()
        {
            Download_List.Clear();

            for (int i = 0; i < RootList.Count; ++i)
            {
                Equip equip = RootList[i];
                string FilePath = System.IO.Path.Combine(ETC.CachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache");
                if (System.IO.File.Exists(FilePath) == false)
                    if (Download_List.Contains(equip.Icon) == false)
                        Download_List.Add(equip.Icon);
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
                        string url = System.IO.Path.Combine(ETC.Server, "Data", "Images", "Equipments", $"{Download_List[i]}.png");
                        string target = System.IO.Path.Combine(ETC.CachePath, "Equip", "Normal", $"{Download_List[i]}.gfdcache");
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
            totalProgress.Text = $"{totalProgressBar.Progress}%";
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            nowProgressBar.Progress = e.ProgressPercentage;
            nowProgress.Text = $"{e.ProgressPercentage}%";
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

        private int SortEquip(Equip x, Equip y)
        {
            switch (LineUpStyle)
            {
                case LineUp.ProductTime:
                    int x_time = x.ProductTime;
                    int y_time = y.ProductTime;

                    if ((x_time == 0) && (y_time != 0)) return 1;
                    else if ((y_time == 0) && (x_time != 0)) return -1;
                    else if (x_time == y_time) return x.Name.CompareTo(y.Name);
                    else return x_time.CompareTo(y_time);
                case LineUp.Name:
                default:
                    return x.Name.CompareTo(y.Name);
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

                for (int i = 0; i < ProductTimeFilters.Length; ++i) v.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value = Filter_ProductTime[i];
                for (int i = 0; i < GradeFilters.Length; ++i) v.FindViewById<CheckBox>(GradeFilters[i]).Checked = Filter_Grade[i];
                for (int i = 0; i < CategoryFilters.Length; ++i) v.FindViewById<CheckBox>(CategoryFilters[i]).Checked = Filter_Category[i];

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
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = view.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value;
                for (int i = 0; i < GradeFilters.Length; ++i) Filter_Grade[i] = view.FindViewById<CheckBox>(GradeFilters[i]).Checked;
                for (int i = 0; i < CategoryFilters.Length; ++i) Filter_Category[i] = view.FindViewById<CheckBox>(CategoryFilters[i]).Checked;

                CheckApplyFilter();

                ListEquip(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void CheckApplyFilter()
        {
            for (int i = 0; i < ProductTimeFilters.Length; ++i)
                if (Filter_ProductTime[i] != 0)
                {
                    HasApplyFilter[0] = true;
                    break;
                }
                else HasApplyFilter[0] = false;
            for (int i = 0; i < GradeFilters.Length; ++i)
                if (Filter_Grade[i] == true)
                {
                    HasApplyFilter[1] = true;
                    break;
                }
                else HasApplyFilter[1] = false;
            for (int i = 0; i < CategoryFilters.Length; ++i)
                if (Filter_Category[i] == true)
                {
                    HasApplyFilter[2] = true;
                    break;
                }
                else HasApplyFilter[2] = false;
        }

        private void ResetFilter(View view)
        {
            try
            {
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = 0;
                for (int i = 0; i < GradeFilters.Length; ++i) Filter_Grade[i] = false;
                for (int i = 0; i < CategoryFilters.Length; ++i) Filter_Category[i] = false;

                for (int i = 0; i < HasApplyFilter.Length; ++i)
                    HasApplyFilter[i] = false;

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

            SubList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < RootList.Count; ++i)
                {
                    Equip equip = RootList[i];

                    if ((p_time[0] + p_time[1]) != 0)
                        if (CheckEquipByProductTime(p_time, p_range, equip.ProductTime) == false) continue;

                    if (CheckFilter(equip) == true) continue;

                    if (searchText != "")
                    {
                        string name = equip.Name.ToUpper();

                        if (name.Contains(searchText) == false) continue;
                    }

                    SubList.Add(equip);
                }

                SubList.Sort(SortEquip);

                var adapter = new EquipListAdapter(SubList, this);

                if (adapter.HasOnItemClick() == false) adapter.ItemClick += Adapter_ItemClick;

                await Task.Delay(100);

                RunOnUiThread(() => { mEquipListView.SetAdapter(adapter); });
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

            for (int i = p_time_minute - range; i <= (p_time_minute + range); ++i)
                if (d_time == i) return true;

            return false;
        }

        private bool CheckFilter(Equip equip)
        {
            if (HasApplyFilter[1] == true)
            {
                switch (equip.Grade)
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
            }

            if (HasApplyFilter[2] == true)
            {
                switch (equip.Category)
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

    class EquipListViewHolder : RecyclerView.ViewHolder
    {
        public TextView Category { get; private set; }
        public TextView Type { get; private set; }
        public ImageView Grade { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView Name { get; private set; }
        public TextView ProductTime { get; private set; }

        public EquipListViewHolder(View view, Action<int> listener) : base(view)
        {
            Category = view.FindViewById<TextView>(Resource.Id.EquipListCategory);
            Type = view.FindViewById<TextView>(Resource.Id.EquipListType);
            Grade = view.FindViewById<ImageView>(Resource.Id.EquipListGrade);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.EquipListSmallImage);
            Name = view.FindViewById<TextView>(Resource.Id.EquipListName);
            ProductTime = view.FindViewById<TextView>(Resource.Id.EquipListProductTime);

            view.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }

    class EquipListAdapter : RecyclerView.Adapter
    {
        List<Equip> items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public EquipListAdapter(List<Equip> items, Activity context)
        {
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.EquipListLayout, parent, false);

            EquipListViewHolder vh = new EquipListViewHolder(view, OnClick);
            return vh;
        }

        public override int ItemCount
        {
            get { return items.Count; }
        }

        void OnClick(int position)
        {
            if (ItemClick != null)
            {
                ItemClick(this, position);
            }
        }

        public bool HasOnItemClick()
        {
            if (ItemClick == null) return false;
            else return true;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            EquipListViewHolder vh = holder as EquipListViewHolder;

            var item = items[position];

            try
            {
                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;
                    string FilePath = System.IO.Path.Combine(ETC.CachePath, "Equip", "Normal", $"{item.Icon}.gfdcache");
                    if (System.IO.File.Exists(FilePath) == true)
                        vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                }
                else vh.SmallImage.Visibility = ViewStates.Gone;

                vh.Grade.SetImageResource(item.GradeIconId);

                vh.Category.Text = item.Category;
                vh.Type.Text = item.Type;
                vh.Name.Text = item.Name;

                vh.ProductTime.Text = ETC.CalcTime(item.ProductTime);
            }
            catch (Exception ex)
            {
                ETC.LogError(context, ex.ToString());
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}