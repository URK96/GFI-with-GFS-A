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
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "인형 목록", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBMainActivity : AppCompatActivity
    {
        delegate void DownloadProgress();

        private List<Doll> RootList = new List<Doll>();
        private List<Doll> SubList = new List<Doll>();
        private List<int> Download_List = new List<int>();

        int[] GradeFilters = { Resource.Id.DollFilterGrade2, Resource.Id.DollFilterGrade3, Resource.Id.DollFilterGrade4, Resource.Id.DollFilterGrade5, Resource.Id.DollFilterGradeExtra };
        int[] TypeFilters = { Resource.Id.DollFilterTypeHG, Resource.Id.DollFilterTypeSMG, Resource.Id.DollFilterTypeAR, Resource.Id.DollFilterTypeRF, Resource.Id.DollFilterTypeMG, Resource.Id.DollFilterTypeSG};
        int[] ProductTimeFilters = { Resource.Id.DollFilterProductHour, Resource.Id.DollFilterProductMinute, Resource.Id.DollFilterProductNearRange };
        int ModFilter = Resource.Id.DollFilterOnlyMod;

        int p_now = 0;
        int p_total = 0;

        private bool[] HasApplyFilter = { false, false, false, false };
        private int[] Filter_ProductTime = { 0, 0, 0 };
        private bool[] Filter_Grade = { false, false, false, false, false };
        private bool[] Filter_Type = { false, false, false, false, false, false };
        private bool Filter_Mod = false;
        private bool CanRefresh = false;

        private enum LineUp { Name, Number, ProductTime }
        private LineUp LineUpStyle = LineUp.Name;

        private RecyclerView mDollListView;
        private RecyclerView.LayoutManager MainRecyclerManager;
        private CoordinatorLayout SnackbarLayout;

        private TextView LineUp_Name;
        private TextView LineUp_Time;
        private TextView LineUp_Num;

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
                SetContentView(Resource.Layout.DollDBListLayout);

                SetTitle(Resource.String.DollDBMainActivity_Title);

                CanRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                mDollListView = FindViewById<RecyclerView>(Resource.Id.DollDBRecyclerView);
                MainRecyclerManager = new LinearLayoutManager(this);
                mDollListView.SetLayoutManager(MainRecyclerManager);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBSnackbarLayout);

                LineUp_Name = FindViewById<TextView>(Resource.Id.DollDBLineUp_Name);
                LineUp_Name.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                LineUp_Name.Click += LineUp_Text_Click;
                LineUp_Time = FindViewById<TextView>(Resource.Id.DollDBLineUp_Time);
                LineUp_Time.Click += LineUp_Text_Click;
                LineUp_Num = FindViewById<TextView>(Resource.Id.DollDBLineUp_Number);
                LineUp_Num.Click += LineUp_Text_Click;

                SearchText = FindViewById<EditText>(Resource.Id.DollSearchText);

                InitializeView();

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<LinearLayout>(Resource.Id.DollSearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.DollSearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.DollSearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                }

                InitProcess();

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);

                if ((ETC.Language.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true) == true)) ETC.RunHelpActivity(this, "DBList");
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void MDollListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
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
                        if (CanRefresh == true) refresh_fab.Show();
                        filter_fab.Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeStatusError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void InitializeView()
        {
            refresh_fab = FindViewById<FloatingActionButton>(Resource.Id.DollRefreshCacheFAB);
            if (CanRefresh == false) refresh_fab.Hide();
            else
            {
                if (refresh_fab.HasOnClickListeners == false) refresh_fab.Click += delegate 
                {
                    Download_List.Clear();
                    foreach (DataRow dr in ETC.DollList.Rows) Download_List.Add((int)dr["DicNumber"]);
                    Download_List.TrimExcess();
                    ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(DollCropImageDownloadProcess));
                };

                refresh_fab.LongClick += MainFAB_LongClick;
            }

            filter_fab = FindViewById<FloatingActionButton>(Resource.Id.DollFilterFAB);
            if (filter_fab.HasOnClickListeners == false) filter_fab.Click += Filter_Fab_Click;
            filter_fab.LongClick += MainFAB_LongClick;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.DollSearchResetButton);
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
                    case Resource.Id.DollRefreshCacheFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_CacheRefresh);
                        break;
                    case Resource.Id.DollFilterFAB:
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

        private void LineUp_Text_Click(object sender, EventArgs e)
        {
            try
            {
                TextView tv = sender as TextView;

                switch (tv.Id)
                {
                    case Resource.Id.DollDBLineUp_Number:
                        LineUpStyle = LineUp.Number;
                        LineUp_Name.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Time.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Num.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                        break;
                    case Resource.Id.DollDBLineUp_Time:
                        LineUpStyle = LineUp.ProductTime;
                        LineUp_Num.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Name.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Time.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                        break;
                    case Resource.Id.DollDBLineUp_Name:
                    default:
                        LineUpStyle = LineUp.Name;
                        LineUp_Num.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Time.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Name.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                        break;
                }

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.LineUp_Error, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void InitProcess()
        {
            CreateListObject();

            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
            {
                if (CheckCropImage() == true)
                    ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(DollCropImageDownloadProcess));
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.DollList.Rows)
                    RootList.Add(new Doll(dr));

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
                Doll doll = RootList[i];
                string FilePath = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", $"{doll.DicNumber}.gfdcache");
                if (File.Exists(FilePath) == false) Download_List.Add(doll.DicNumber);
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

        private async void DollCropImageDownloadProcess()
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

                p_now = 0;
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
                        string url = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", $"{Download_List[i]}.png");
                        string target = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", $"{Download_List[i]}.gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
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
            ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
        }

        private void SearchResetButton_Click(object sender, EventArgs e)
        {
            SearchText.Text = "";
        }

        private void Filter_Fab_Click(object sender, EventArgs e)
        {
            InitFilterBox();   
        }

        private void InitFilterBox()
        {
            var inflater = LayoutInflater;

            try
            {
                View v = inflater.Inflate(Resource.Layout.DollFilterLayout, null);

                v.FindViewById<NumberPicker>(Resource.Id.DollFilterProductHour).MaxValue = 12;
                v.FindViewById<NumberPicker>(Resource.Id.DollFilterProductMinute).MaxValue = 59;
                v.FindViewById<NumberPicker>(Resource.Id.DollFilterProductNearRange).MaxValue = 20;

                for (int i = 0; i < GradeFilters.Length; ++i) v.FindViewById<CheckBox>(GradeFilters[i]).Checked = Filter_Grade[i];
                for (int i = 0; i < TypeFilters.Length; ++i) v.FindViewById<CheckBox>(TypeFilters[i]).Checked = Filter_Type[i];
                for (int i = 0; i < ProductTimeFilters.Length; ++i) v.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value = Filter_ProductTime[i];
                v.FindViewById<CheckBox>(ModFilter).Checked = Filter_Mod;

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
                for (int i = 0; i < GradeFilters.Length; ++i) Filter_Grade[i] = view.FindViewById<CheckBox>(GradeFilters[i]).Checked;
                for (int i = 0; i < TypeFilters.Length; ++i) Filter_Type[i] = view.FindViewById<CheckBox>(TypeFilters[i]).Checked;
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = view.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value;
                Filter_Mod = view.FindViewById<CheckBox>(ModFilter).Checked;

                CheckApplyFilter();

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
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
            for (int i = 0; i < TypeFilters.Length; ++i)
                if (Filter_Type[i] == true)
                {
                    HasApplyFilter[2] = true;
                    break;
                }
                else HasApplyFilter[2] = false;

            if (Filter_Mod == true) HasApplyFilter[3] = true;
            else HasApplyFilter[3] = false;
        }

        private void ResetFilter(View view)
        {
            try
            {
                for (int i = 0; i < GradeFilters.Length; ++i) Filter_Grade[i] = false;
                for (int i = 0; i < TypeFilters.Length; ++i) Filter_Type[i] = false;
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = 0;
                Filter_Mod = false;

                for (int i = 0; i < HasApplyFilter.Length; ++i)
                    HasApplyFilter[i] = false;

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListDoll(string searchText, int[] p_time, int p_range)
        {
            SubList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < RootList.Count; ++i)
                {
                    Doll doll = RootList[i];

                    if ((p_time[0] + p_time[1]) != 0)
                        if (CheckDollByProductTime(p_time, p_range, doll.ProductTime) == false) continue;

                    if (CheckFilter(doll) == true) continue;

                    if (searchText != "")
                    {
                        string name = doll.Name.ToUpper();

                        if (name.Contains(searchText) == false) continue;
                    }

                    SubList.Add(doll);
                }

                SubList.Sort(SortDoll);

                var adapter = new DollListAdapter(SubList, this);

                if (adapter.HasOnItemClick() == false) adapter.ItemClick += Adapter_ItemClick;

                await Task.Delay(100);

                RunOnUiThread(() => { mDollListView.SetAdapter(adapter); });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private async void Adapter_ItemClick(object sender, int position)
        {
            await Task.Delay(100);
            var DollInfo = new Intent(this, typeof(DollDBDetailActivity));
            DollInfo.PutExtra("DicNum", SubList[position].DicNumber);
            StartActivity(DollInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private bool CheckDollByProductTime(int[] p_time, int range, int d_time)
        {
            int p_time_minute = (p_time[0] * 60) + p_time[1];

            for (int i = p_time_minute - range; i <= (p_time_minute + range); ++i)
                if (d_time == i) return true;

            return false;
        }

        private int SortDoll(Doll x, Doll y)
        {
            switch (LineUpStyle)
            {
                case LineUp.Number:
                    return x.DicNumber.CompareTo(y.DicNumber);
                case LineUp.ProductTime:
                    int x_time = x.ProductTime;
                    int y_time = y.ProductTime;

                    if ((x_time == 0) && (y_time !=0)) return 1;
                    else if ((y_time == 0) && (x_time != 0)) return -1;
                    else if (x_time == y_time) return x.Name.CompareTo(y.Name);
                    else return x_time.CompareTo(y_time);
                case LineUp.Name:
                default:
                    return x.Name.CompareTo(y.Name);
            }
        }

        private bool CheckFilter(Doll doll)
        {
            if (HasApplyFilter[1] == true)
            {
                switch (doll.Grade)
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
                switch (doll.Type)
                {
                    case "HG":
                        if (Filter_Type[0] == false) return true;
                        break;
                    case "SMG":
                        if (Filter_Type[1] == false) return true;
                        break;
                    case "AR":
                        if (Filter_Type[2] == false) return true;
                        break;
                    case "RF":
                        if (Filter_Type[3] == false) return true;
                        break;
                    case "MG":
                        if (Filter_Type[4] == false) return true;
                        break;
                    case "SG":
                        if (Filter_Type[5] == false) return true;
                        break;
                }
            }

            if (doll.HasMod == false)
                if (Filter_Mod == true) return true;

            return false;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }

    class DollListViewHolder : RecyclerView.ViewHolder
    {
        public TextView DicNumber { get; private set; }
        public TextView Type { get; private set; }
        public ImageView Grade { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView Name { get; private set; }
        public TextView ProductTime { get; private set; }

        public DollListViewHolder(View view, Action<int> listener) : base(view)
        {
            DicNumber = view.FindViewById<TextView>(Resource.Id.DollListNumber);
            Type = view.FindViewById<TextView>(Resource.Id.DollListType);
            Grade = view.FindViewById<ImageView>(Resource.Id.DollListGrade);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.DollListSmallImage);
            Name = view.FindViewById<TextView>(Resource.Id.DollListName);
            ProductTime = view.FindViewById<TextView>(Resource.Id.DollListProductTime);

            view.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }

    class DollListAdapter : RecyclerView.Adapter
    {
        List<Doll> items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public DollListAdapter(List<Doll> items, Activity context)
        {
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.DollListLayout, parent, false);

            DollListViewHolder vh = new DollListViewHolder(view, OnClick);
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
            DollListViewHolder vh = holder as DollListViewHolder;

            var item = items[position];

            try
            {
                vh.DicNumber.Text = $"No. {item.DicNumber}";

                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;
                    if (File.Exists(Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", $"{item.DicNumber}.gfdcache")) == true)
                        vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", $"{item.DicNumber}.gfdcache")));
                }
                else vh.SmallImage.Visibility = ViewStates.Gone;

                vh.Grade.SetImageResource(item.GradeIconId);
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