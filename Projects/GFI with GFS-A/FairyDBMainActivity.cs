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
using Android.Support.V7.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Label = "@string/Activity_FairyMainActivity", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FairyDBMainActivity : BaseAppCompatActivity
    {
        delegate void DownloadProgress();

        private List<Fairy> RootList = new List<Fairy>();
        private List<Fairy> SubList = new List<Fairy>();
        private List<int> Download_List = new List<int>();

        int[] TypeFilters = { Resource.Id.FairyFilterTypeCombat, Resource.Id.FairyFilterTypeStrategy };
        int[] ProductTimeFilters = { Resource.Id.FairyFilterProductHour, Resource.Id.FairyFilterProductMinute };

        int p_now = 0;
        int p_total = 0;

        private enum LineUp { Name, ProductTime }
        private LineUp LineUpStyle = LineUp.Name;

        private bool[] HasApplyFilter = { false, false };
        private int[] Filter_ProductTime = { 0, 0 };
        private bool[] Filter_Type = { true, true };
        private bool CanRefresh = false;

        private RecyclerView mFairyListView;
        private RecyclerView.LayoutManager MainLayoutManager;
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

                if (ETC.useLightTheme)
                    SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.FairyDBListLayout);

                SetTitle(Resource.String.FairyDBMainActivity_Title);

                CanRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                mFairyListView = FindViewById<RecyclerView>(Resource.Id.FairyDBRecyclerView);
                MainLayoutManager = new LinearLayoutManager(this);
                mFairyListView.SetLayoutManager(MainLayoutManager);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FairyDBSnackbarLayout);

                LineUp_Name = FindViewById<TextView>(Resource.Id.FairyDBLineUp_Name);
                LineUp_Name.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                LineUp_Name.Click += LineUp_Text_Click;
                LineUp_Time = FindViewById<TextView>(Resource.Id.FairyDBLineUp_Time);
                LineUp_Time.Click += LineUp_Text_Click;

                SearchText = FindViewById<EditText>(Resource.Id.FairySearchText);

                InitializeView();

                if (ETC.useLightTheme)
                {
                    FindViewById<LinearLayout>(Resource.Id.FairySearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.FairySearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.FairySearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                }

                InitProcess();

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });

                if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true)))
                    ETC.RunHelpActivity(this, "DBList");
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
                    case Resource.Id.FairyDBLineUp_Time:
                        LineUpStyle = LineUp.ProductTime;
                        LineUp_Name.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Time.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                        break;
                    case Resource.Id.FairyDBLineUp_Name:
                    default:
                        LineUpStyle = LineUp.Name;
                        LineUp_Time.SetBackgroundColor(Android.Graphics.Color.Transparent);
                        LineUp_Name.SetBackgroundColor(Android.Graphics.Color.ParseColor("#54A716"));
                        break;
                }

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.LineUp_Error, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
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
                        break;
                    case ScrollState.Idle:
                        if (CanRefresh == true) refresh_fab.Show();
                        filter_fab.Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeStatusError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private async void Adapter_ItemClick(object sender, int position)
        {
            await Task.Delay(100);
            var FairyInfo = new Intent(this, typeof(FairyDBDetailActivity));
            FairyInfo.PutExtra("DicNum", SubList[position].DicNumber);
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
                }

                Toast.MakeText(this, tip, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
            CreateListObject();

            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
            {
                if (CheckFairyCropImage() == true)
                    ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(FairyCropImageDownloadProcess));
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.fairyList.Rows)
                    RootList.Add(new Fairy(dr));

                RootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthShort);
            }
        }

        private bool CheckFairyCropImage()
        {
            Download_List.Clear();

            for (int i = 0; i < RootList.Count; ++i)
            {
                string FilePath = System.IO.Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop", $"{RootList[i].DicNumber}.gfdcache");
                if (System.IO.File.Exists(FilePath) == false) Download_List.Add(RootList[i].DicNumber);
            }

            Download_List.TrimExcess();

            if (Download_List.Count == 0) return false;
            else return true;
        }

        private void ShowDownloadCheckMessage(int title, int message, DownloadProgress method)
        {
            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBG);
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

            Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGDownload);
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
                        string url = System.IO.Path.Combine(ETC.server, "Data", "Images", "Fairy", "Normal_Crop", $"{filename}.png");
                        string target = System.IO.Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop", $"{filename}.gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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

                Android.Support.V7.App.AlertDialog.Builder FilterBox = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGVertical);
                FilterBox.SetTitle(Resource.String.DBList_FilterBoxTitle);
                FilterBox.SetView(v);
                FilterBox.SetPositiveButton(Resource.String.AlertDialog_Set, delegate { ApplyFilter(v); });
                FilterBox.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                FilterBox.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate { ResetFilter(); });

                FilterBox.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FilterBox_InitError, Snackbar.LengthLong);
            }
        }

        private void ApplyFilter(View view)
        {
            try
            {
                for (int i = 0; i < TypeFilters.Length; ++i)
                    Filter_Type[i] = view.FindViewById<CheckBox>(TypeFilters[i]).Checked;
                for (int i = 0; i < ProductTimeFilters.Length; ++i)
                    Filter_ProductTime[i] = view.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value;

                CheckApplyFilter();

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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
            for (int i = 0; i < TypeFilters.Length; ++i)
                if (Filter_Type[i])
                {
                    HasApplyFilter[1] = true;
                    break;
                }
                else HasApplyFilter[1] = false;
        }

        private void ResetFilter()
        {
            try
            {
                for (int i = 0; i < TypeFilters.Length; ++i)
                    Filter_Type[i] = true;
                for (int i = 0; i < ProductTimeFilters.Length; ++i)
                    Filter_ProductTime[i] = 0;

                for (int i = 0; i < HasApplyFilter.Length; ++i)
                    HasApplyFilter[i] = false;

                ListFairy(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListFairy(string searchText, int[] p_time)
        {
            //ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_Listing, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);

            SubList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < RootList.Count; ++i)
                {
                    Fairy fairy = RootList[i];

                    if ((p_time[0] + p_time[1]) != 0)
                        if (fairy.ProductTime != ((p_time[0] * 60) + p_time[1])) continue;

                    if (CheckFilter(fairy) == true) continue;

                    if (searchText != "")
                    {
                        string name = fairy.Name.ToUpper();

                        if (name.Contains(searchText) == false) continue;
                    }

                    SubList.Add(fairy);
                }

                SubList.Sort(SortFairyName);

                var adapter = new FairyListAdapter(SubList, this);

                if (adapter.HasOnItemClick() == false) adapter.ItemClick += Adapter_ItemClick;

                await Task.Delay(100);

                RunOnUiThread(() => { mFairyListView.SetAdapter(adapter); });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private int SortFairyName(Fairy x, Fairy y)
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

        private bool CheckFilter(Fairy fairy)
        {
            if (HasApplyFilter[1] == true)
            {
                switch (fairy.Type)
                {
                    case "전투":
                        if (Filter_Type[0] == false) return true;
                        break;
                    case "책략":
                        if (Filter_Type[1] == false) return true;
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

    class FairyListViewHolder : RecyclerView.ViewHolder
    {
        public TextView DicNumber { get; private set; }
        public ImageView TypeIcon { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView Name { get; private set; }
        public TextView ProductTime { get; private set; }

        public FairyListViewHolder(View view, Action<int> listener) : base(view)
        {
            //DicNumber = view.FindViewById<TextView>(Resource.Id.FairyListNumber);
            TypeIcon = view.FindViewById<ImageView>(Resource.Id.FairyListType);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.FairyListSmallImage);
            Name = view.FindViewById<TextView>(Resource.Id.FairyListName);
            ProductTime = view.FindViewById<TextView>(Resource.Id.FairyListProductTime);

            view.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }

    class FairyListAdapter : RecyclerView.Adapter
    {
        List<Fairy> items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public FairyListAdapter(List<Fairy> items, Activity context)
        {
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.FairyListLayout, parent, false);

            FairyListViewHolder vh = new FairyListViewHolder(view, OnClick);
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
            FairyListViewHolder vh = holder as FairyListViewHolder;

            var item = items[position];

            try
            {
                int TypeIconId = 0;
                switch (item.Type)
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
                vh.TypeIcon.SetImageResource(TypeIconId);

                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;
                    string FilePath = System.IO.Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop", $"{item.DicNumber}.gfdcache");
                    if (System.IO.File.Exists(FilePath) == true)
                        vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                }
                else vh.SmallImage.Visibility = ViewStates.Gone;

                vh.Name.Text = item.Name;
                vh.ProductTime.Text = ETC.CalcTime(item.ProductTime);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, context);
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}