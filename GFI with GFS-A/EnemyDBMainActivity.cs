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
using System.IO;

namespace GFI_with_GFS_A
{
    [Activity(Label = "철혈 목록", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EnemyDBMainActivity : AppCompatActivity
    {
        delegate void DownloadProgress();

        private List<Enemy> RootList = new List<Enemy>();
        private List<Enemy> SubList = new List<Enemy>();
        private List<string> Download_List = new List<string>();

        int[] EnemyTypeFilters = { Resource.Id.EnemyFilterNormalEnemy, Resource.Id.EnemyFilterBossEnemy };

        int p_now = 0;
        int p_total = 0;

        private bool[] HasApplyFilter = { false };
        private bool[] Filter_EnemyType = { false, false };
        private bool CanRefresh = false;

        private RecyclerView mEnemyListView;
        private RecyclerView.LayoutManager MainLayoutManager;
        private CoordinatorLayout SnackbarLayout;

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
                SetContentView(Resource.Layout.EnemyDBListLayout);

                SetTitle(Resource.String.EnemyDBMainActivity_Title);

                CanRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                mEnemyListView = FindViewById<RecyclerView>(Resource.Id.EnemyDBRecyclerView);
                MainLayoutManager = new LinearLayoutManager(this);
                mEnemyListView.SetLayoutManager(MainLayoutManager);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EnemyDBSnackbarLayout);

                SearchText = FindViewById<EditText>(Resource.Id.EnemySearchText);

                InitializeView();

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<LinearLayout>(Resource.Id.EnemySearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.EnemySearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.EnemySearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                }

                InitProcess();

                ListEnemy(SearchText.Text);

                if ((ETC.Language.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true) == true))
                    ETC.RunHelpActivity(this, "DBList");
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void MEnemyListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
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

        private async void Adapter_ItemClick(object sender, int position)
        {
            await Task.Delay(100);
            var EnemyInfo = new Intent(this, typeof(EnemyDBDetailActivity));
            EnemyInfo.PutExtra("Keyword", SubList[position].CodeName);
            StartActivity(EnemyInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void InitializeView()
        {
            refresh_fab = FindViewById<FloatingActionButton>(Resource.Id.EnemyRefreshCacheFAB);
            if (CanRefresh == false) refresh_fab.Hide();
            else
            {
                if (refresh_fab.HasOnClickListeners == false) refresh_fab.Click += delegate 
                {
                    Download_List.Clear();
                    foreach (DataRow dr in ETC.EnemyList.Rows) Download_List.Add((string)dr["CodeName"]);
                    Download_List.TrimExcess();

                    ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(EnemyCropImageDownloadProcess));
                };

                refresh_fab.LongClick += MainFAB_fab_LongClick;
            }

            filter_fab = FindViewById<FloatingActionButton>(Resource.Id.EnemyFilterFAB);
            if (filter_fab.HasOnClickListeners == false) filter_fab.Click += Filter_Fab_Click;
            filter_fab.LongClick += MainFAB_fab_LongClick;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.EnemySearchResetButton);
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
                    case Resource.Id.EnemyRefreshCacheFAB:
                        tip = Resources.GetString(Resource.String.Tooltip_DB_CacheRefresh);
                        break;
                    case Resource.Id.EnemyFilterFAB:
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
                if (CheckEnemyCropImage() == true) ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(EnemyCropImageDownloadProcess));
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.EnemyList.Rows)
                {
                    bool IsCreate = false;

                    foreach (Enemy enemy in RootList)
                        if (enemy.CodeName == (string)dr["CodeName"])
                        {
                            IsCreate = true;
                            break;
                        }

                    if (IsCreate == false)
                        RootList.Add(new Enemy(dr));
                }

                RootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthShort);
            }
        }

        private bool CheckEnemyCropImage()
        {
            Download_List.Clear();

            foreach (Enemy enemy in RootList)
            {
                string FilePath = Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", $"{enemy.CodeName}.gfdcache");
                if (File.Exists(FilePath) == false) Download_List.Add(enemy.CodeName);
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

        private async void EnemyCropImageDownloadProcess()
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
                        string url = Path.Combine(ETC.Server, "Data", "Images", "Enemy", "Normal_Crop", $"{Download_List[i]}.png");
                        string target = Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", $"{Download_List[i]}.gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListEnemy(SearchText.Text);
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
            ListEnemy(SearchText.Text);
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
                View v = inflater.Inflate(Resource.Layout.EnemyFilterLayout, null);

                for (int i = 0; i < EnemyTypeFilters.Length; ++i) v.FindViewById<CheckBox>(EnemyTypeFilters[i]).Checked = Filter_EnemyType[i];

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
                for (int i = 0; i < EnemyTypeFilters.Length; ++i)
                    Filter_EnemyType[i] = view.FindViewById<CheckBox>(EnemyTypeFilters[i]).Checked;

                CheckApplyFilter();

                ListEnemy(SearchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void CheckApplyFilter()
        {
            for (int i = 0; i < Filter_EnemyType.Length; ++i)
                if (Filter_EnemyType[i] == true)
                {
                    HasApplyFilter[0] = true;
                    break;
                }
                else HasApplyFilter[0] = false;
        }

        private void ResetFilter(View view)
        {
            try
            {
                for (int i = 0; i < EnemyTypeFilters.Length; ++i) Filter_EnemyType[i] = false;

                for (int i = 0; i < HasApplyFilter.Length; ++i) HasApplyFilter[i] = false;

                ListEnemy(SearchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListEnemy(string searchText)
        {
            SubList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < RootList.Count; ++i)
                {
                    Enemy enemy = RootList[i];

                    if (CheckFilter(enemy) == true) continue;

                    if (searchText != "")
                    {
                        string name = enemy.Name.ToUpper();

                        if (name.Contains(searchText) == false) continue;
                    }

                    SubList.Add(enemy);
                }

                SubList.Sort(SortEnemy);

                var adapter = new EnemyListAdapter(SubList, this);

                if (adapter.HasOnItemClick() == false) adapter.ItemClick += Adapter_ItemClick;

                await Task.Delay(100);

                RunOnUiThread(() => { mEnemyListView.SetAdapter(adapter); });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private int SortEnemy(Enemy x, Enemy y)
        {
            return x.Name.CompareTo(y.Name);
        }

        private bool CheckFilter(Enemy enemy)
        {
            if (HasApplyFilter[0] == true)
            {
                switch (enemy.IsBoss)
                {
                    case false:
                        if (Filter_EnemyType[0] == false) return true;
                        break;
                    case true:
                        if (Filter_EnemyType[1] == false) return true;
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

    class EnemyListViewHolder : RecyclerView.ViewHolder
    {
        public TextView Type { get; private set; }
        public ImageView TypeIcon { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView Name { get; private set; }
        public TextView CodeName { get; private set; }

        public EnemyListViewHolder(View view, Action<int> listener) : base(view)
        {
            Type = view.FindViewById<TextView>(Resource.Id.EnemyListType);
            TypeIcon = view.FindViewById<ImageView>(Resource.Id.EnemyListTypeIcon);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.EnemyListSmallImage);
            Name = view.FindViewById<TextView>(Resource.Id.EnemyListName);
            CodeName = view.FindViewById<TextView>(Resource.Id.EnemyListCodeName);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    class EnemyListAdapter : RecyclerView.Adapter
    {
        List<Enemy> items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public EnemyListAdapter(List<Enemy> items, Activity context)
        {
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.EnemyListLayout, parent, false);

            EnemyListViewHolder vh = new EnemyListViewHolder(view, OnClick);
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
            EnemyListViewHolder vh = holder as EnemyListViewHolder;

            var item = items[position];

            try
            {
                int TypeIconId = 0;
                string enemy_type = "";
                switch (item.IsBoss)
                {
                    default:
                    case false:
                        TypeIconId = Resource.Drawable.Grade_N;
                        enemy_type = "NM";
                        break;
                    case true:
                        TypeIconId = Resource.Drawable.Grade_S;
                        enemy_type = "Boss";
                        break;
                }
                vh.TypeIcon.SetImageResource(TypeIconId);

                vh.Type.Text = enemy_type;

                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;
                    string FilePath = Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", $"{item.CodeName}.gfdcache");
                    if (File.Exists(FilePath) == true) vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                }
                else vh.SmallImage.Visibility = ViewStates.Gone;

                vh.Name.Text = item.Name;
                vh.CodeName.Text = item.CodeName;
            }
            catch (Exception ex)
            {
                ETC.LogError(context, ex.ToString());
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}