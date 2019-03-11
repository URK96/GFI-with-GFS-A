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
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "화력소대 목록", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FSTDBMainActivity : AppCompatActivity
    {
        delegate void DownloadProgress();

        private List<FST> RootList = new List<FST>();
        private List<FST> SubList = new List<FST>();
        private List<string> Download_List = new List<string>();

        int[] TypeFilters = { Resource.Id.FSTFilterTypeATW, Resource.Id.FSTFilterTypeAGL, Resource.Id.FSTFilterTypeMTR};

        int p_now = 0;
        int p_total = 0;

        private bool[] HasApplyFilter = { false };
        private bool[] Filter_Type = { false, false, false };

        private enum LineUp { Name, Number }
        private LineUp LineUpStyle = LineUp.Name;

        private RecyclerView mFSTListView;
        private RecyclerView.LayoutManager MainRecyclerManager;
        private CoordinatorLayout SnackbarLayout;

        private EditText SearchText;

        private Dialog dialog;
        private ProgressBar totalProgressBar;
        private ProgressBar nowProgressBar;
        private TextView totalProgress;
        private TextView nowProgress;
        private FloatingActionButton filter_fab;
        private FloatingActionButton array_fab;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.FSTDBListLayout);

                SetTitle(Resource.String.FSTDBMainActivity_Title);

                mFSTListView = FindViewById<RecyclerView>(Resource.Id.FSTDBRecyclerView);
                MainRecyclerManager = new LinearLayoutManager(this);
                mFSTListView.SetLayoutManager(MainRecyclerManager);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FSTDBSnackbarLayout);

                SearchText = FindViewById<EditText>(Resource.Id.FSTSearchText);

                InitializeView();

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<LinearLayout>(Resource.Id.FSTSearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.FSTSearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.FSTSearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                }

                InitProcess();

                ListFST(SearchText.Text);

                if ((ETC.Language.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true) == true)) ETC.RunHelpActivity(this, "DBList");
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        /*private void MFSTListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            try
            {
                switch (e.ScrollState)
                {
                    case ScrollState.TouchScroll:
                        filter_fab.Hide();
                        array_fab.Hide();
                        break;
                    case ScrollState.Idle:
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
        }*/

        private async void Adapter_ItemClick(object sender, int position)
        {
            await Task.Delay(100);
            var FSTInfo = new Intent(this, typeof(FSTDBDetailActivity));
            FSTInfo.PutExtra("Keyword", SubList[position].CodeName);
            StartActivity(FSTInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        /*private void MFSTListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            DataRow dr = mFSTList[e.Position].FSTDR;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0} : {1}\n\n", Resources.GetString(Resource.String.Common_NickName), (string)dr["NickName"]);

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this);
            ad.SetTitle((string)dr["Name"]);
            ad.SetCancelable(true);
            ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });
            ad.SetMessage(sb.ToString());
            ad.Show();
        }*/

        private void InitializeView()
        {
            filter_fab = FindViewById<FloatingActionButton>(Resource.Id.FSTFilterFAB);
            if (filter_fab.HasOnClickListeners == false) filter_fab.Click += Filter_Fab_Click;

            array_fab = FindViewById<FloatingActionButton>(Resource.Id.FSTArrayFAB);
            if (array_fab.HasOnClickListeners == false) array_fab.Click += Array_fab_Click;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.FSTSearchResetButton);
            if (SearchResetButton.HasOnClickListeners == false) SearchResetButton.Click += SearchResetButton_Click;

            SearchText.TextChanged += SearchText_TextChanged;
        }

        private void Array_fab_Click(object sender, EventArgs e)
        {
            try
            {
                switch (LineUpStyle)
                {
                    default:
                    case LineUp.Name:
                        LineUpStyle = LineUp.Number;
                        array_fab.SetImageResource(Resource.Drawable.LineUp_DicNum_Icon);
                        break;
                    case LineUp.Number:
                        LineUpStyle = LineUp.Name;
                        array_fab.SetImageResource(Resource.Drawable.LineUp_ProductTime_Icon);
                        break;
                }

                ListFST(SearchText.Text);
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
                if (CheckFSTCropImage() == true)
                    ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(FSTCropImageDownloadProcess));
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.FSTList.Rows)
                    RootList.Add(new FST(dr, true));

                RootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthShort);
            }
        }

        private bool CheckFSTCropImage()
        {
            Download_List.Clear();

            for (int i = 0; i < RootList.Count; ++i)
            {
                FST fst = RootList[i];
                string FilePath = System.IO.Path.Combine(ETC.CachePath, "FST", "Normal_Icon", $"{fst.CodeName}.gfdcache");
                if (System.IO.File.Exists(FilePath) == false)
                    Download_List.Add(fst.CodeName);
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

        private async void FSTCropImageDownloadProcess()
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
                        string url = System.IO.Path.Combine(ETC.Server, "Data", "Images", "FST", "Normal_Icon", $"{Download_List[i]}.png");
                        string target = System.IO.Path.Combine(ETC.CachePath, "FST", "Normal_Icon", $"{Download_List[i]}.gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListFST(SearchText.Text);
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
            ListFST(SearchText.Text);
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
                View v = inflater.Inflate(Resource.Layout.FSTFilterLayout, null);

                for (int i = 0; i < TypeFilters.Length; ++i) v.FindViewById<CheckBox>(TypeFilters[i]).Checked = Filter_Type[i];

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

                CheckApplyFilter();

                ListFST(SearchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void CheckApplyFilter()
        {
            for (int i = 0; i < TypeFilters.Length; ++i)
                if (Filter_Type[i] == true)
                {
                    HasApplyFilter[0] = true;
                    break;
                }
        }

        private void ResetFilter(View view)
        {
            try
            {
                for (int i = 0; i < TypeFilters.Length; ++i) Filter_Type[i] = true;

                for (int i = 0; i < HasApplyFilter.Length; ++i)
                    HasApplyFilter[i] = false;

                ListFST(SearchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListFST(string searchText)
        {
            //ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_Listing, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);

            SubList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < RootList.Count; ++i)
                {
                    FST fst = RootList[i];

                    if (CheckFilter(fst) == true) continue;

                    if (searchText != "")
                    {
                        string name = fst.Name.ToUpper();
                        if (name.Contains(searchText) == false) continue;
                    }

                    SubList.Add(fst);
                }

                SubList.Sort(SortFST);

                var adapter = new FSTListAdapter(SubList, this);

                if (adapter.HasOnItemClick() == false) adapter.ItemClick += Adapter_ItemClick;

                await Task.Delay(100);

                RunOnUiThread(() => { mFSTListView.SetAdapter(adapter); });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private int SortFST(FST x, FST y)
        {
            switch (LineUpStyle)
            {
                case LineUp.Number:
                    return x.DicNumber.CompareTo(y.DicNumber);
                case LineUp.Name:
                default:
                    return x.Name.CompareTo(y.Name);
            }
        }

        private bool CheckFilter(FST fst)
        {
            if (HasApplyFilter[0] == true)
            {
                switch (fst.Type)
                {
                    case "ATW":
                        if (Filter_Type[0] == false) return true;
                        break;
                    case "AGL":
                        if (Filter_Type[1] == false) return true;
                        break;
                    case "MTR":
                        if (Filter_Type[2] == false) return true;
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

    class FSTListViewHolder : RecyclerView.ViewHolder
    {
        public TextView Type { get; private set; }
        public ImageView TypeIcon { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView Name { get; private set; }
        public TextView RealModel { get; private set; }

        public FSTListViewHolder(View view, Action<int> listener) : base(view)
        {
            Type = view.FindViewById<TextView>(Resource.Id.FSTListType);
            TypeIcon = view.FindViewById<ImageView>(Resource.Id.FSTListTypeIcon);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.FSTListSmallImage);
            Name = view.FindViewById<TextView>(Resource.Id.FSTListName);
            RealModel = view.FindViewById<TextView>(Resource.Id.FSTListRealModel);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    class FSTListAdapter : RecyclerView.Adapter
    {
        List<FST> items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public FSTListAdapter(List<FST> items, Activity context)
        {
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.FSTListLayout, parent, false);

            FSTListViewHolder vh = new FSTListViewHolder(view, OnClick);
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
            FSTListViewHolder vh = holder as FSTListViewHolder;

            var item = items[position];

            try
            {
                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;
                    string FilePath = System.IO.Path.Combine(ETC.CachePath, "FST", "Normal_Icon", $"{item.CodeName}.gfdcache");
                    if (System.IO.File.Exists(FilePath) == true)
                        vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                }
                else vh.SmallImage.Visibility = ViewStates.Gone;

                vh.Type.Text = item.Type;
                //vh.Grade.SetImageResource(item.GradeIconId);
                vh.Name.Text = item.Name;
                vh.RealModel.Text = item.RealModel;
            }
            catch (Exception ex)
            {
                ETC.LogError(context, ex.ToString());
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}