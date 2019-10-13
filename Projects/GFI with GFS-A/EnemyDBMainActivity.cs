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
    [Activity(Label = "@string/Acticity_EnemyMainActivity", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EnemyDBMainActivity : BaseAppCompatActivity
    {
        delegate void DownloadProgress();

        private List<Enemy> rootList = new List<Enemy>();
        private List<Enemy> subList = new List<Enemy>();
        private List<string> downloadList = new List<string>();

        int[] enemyTypeFilters = { Resource.Id.EnemyFilterNormalEnemy, Resource.Id.EnemyFilterBossEnemy };
        int[] enemyAffiliationFilters =
        {
            Resource.Id.EnemyFilterAffiliationSF,
            Resource.Id.EnemyFilterAffiliationIOP,
            Resource.Id.EnemyFilterAffiliationKCCO,
            Resource.Id.EnemyFilterAffiliationParadeus,
            Resource.Id.EnemyFilterAffiliationMindMapSystem,
            Resource.Id.EnemyFilterAffiliationELID
        };

        int pNow = 0;
        int pTotal = 0;

        private bool[] hasApplyFilter = { false, false };
        private bool[] filter_EnemyType = { false, false };
        private bool[] filter_EnemyAffiliation = { false, false, false, false, false, false };
        private bool canRefresh = false;

        private RecyclerView mEnemyListView;
        private RecyclerView.LayoutManager mainLayoutManager;
        private CoordinatorLayout snackbarLayout;

        private EditText searchText;

        private Dialog dialog;
        private ProgressBar totalProgressBar;
        private ProgressBar nowProgressBar;
        private TextView totalProgress;
        private TextView nowProgress;
        private FloatingActionButton refreshFAB;
        private FloatingActionButton filterFAB;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                    SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.EnemyDBListLayout);

                SetTitle(Resource.String.EnemyDBMainActivity_Title);

                canRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                mEnemyListView = FindViewById<RecyclerView>(Resource.Id.EnemyDBRecyclerView);
                mainLayoutManager = new LinearLayoutManager(this);
                mEnemyListView.SetLayoutManager(mainLayoutManager);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EnemyDBSnackbarLayout);
                searchText = FindViewById<EditText>(Resource.Id.EnemySearchText);
                refreshFAB = FindViewById<FloatingActionButton>(Resource.Id.EnemyRefreshCacheFAB);
                filterFAB = FindViewById<FloatingActionButton>(Resource.Id.EnemyFilterFAB);

                InitializeView();

                if (ETC.useLightTheme)
                {
                    FindViewById<LinearLayout>(Resource.Id.EnemySearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.EnemySearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.EnemySearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                }

                InitProcess();

                ListEnemy(searchText.Text);

                if ((ETC.locale.Language == "ko") && ETC.sharedPreferences.GetBoolean("Help_DBList", true))
                    ETC.RunHelpActivity(this, "DBList");
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void MEnemyListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            try
            {
                switch (e.ScrollState)
                {
                    case ScrollState.TouchScroll:
                        if (canRefresh)
                            refreshFAB.Hide();

                        filterFAB.Hide();
                        break;
                    case ScrollState.Idle:
                        if (canRefresh)
                            refreshFAB.Show();

                        filterFAB.Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.FAB_ChangeStatusError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private async void Adapter_ItemClick(object sender, int position)
        {
            await Task.Delay(100);
            var EnemyInfo = new Intent(this, typeof(EnemyDBDetailActivity));
            EnemyInfo.PutExtra("Keyword", subList[position].CodeName);
            StartActivity(EnemyInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void InitializeView()
        {
            if (!canRefresh)
                refreshFAB.Hide();
            else
            {
                if (!refreshFAB.HasOnClickListeners)
                    refreshFAB.Click += delegate
                    {
                        downloadList.Clear();

                        foreach (DataRow dr in ETC.enemyList.Rows)
                            downloadList.Add((string)dr["CodeName"]);

                        downloadList.TrimExcess();

                        ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(EnemyCropImageDownloadProcess));
                    };

                refreshFAB.LongClick += MainFAB_fab_LongClick;
            }

            if (!filterFAB.HasOnClickListeners)
                filterFAB.Click += delegate { InitFilterBox(); };

            filterFAB.LongClick += MainFAB_fab_LongClick;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.EnemySearchResetButton);

            if (!SearchResetButton.HasOnClickListeners)
                SearchResetButton.Click += delegate { searchText.Text = ""; };

            searchText.TextChanged += delegate { ListEnemy(searchText.Text); };
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
                ETC.LogError(ex, this);
            }
        }

        private void InitProcess()
        {
            CreateListObject();

            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false))
                if (CheckEnemyCropImage())
                    ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(EnemyCropImageDownloadProcess));
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.enemyList.Rows)
                {
                    bool IsCreate = false;

                    foreach (Enemy enemy in rootList)
                        if (enemy.CodeName == (string)dr["CodeName"])
                        {
                            IsCreate = true;
                            break;
                        }

                    if (!IsCreate)
                        rootList.Add(new Enemy(dr));
                }

                rootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthShort);
            }
        }

        private bool CheckEnemyCropImage()
        {
            downloadList.Clear();

            foreach (Enemy enemy in rootList)
            {
                string FilePath = Path.Combine(ETC.cachePath, "Enemy", "Normal_Crop", $"{enemy.CodeName}.gfdcache");

                if (!File.Exists(FilePath))
                    downloadList.Add(enemy.CodeName);
            }

            downloadList.TrimExcess();

            if (downloadList.Count == 0)
                return false;
            else
                return true;
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

        private async void EnemyCropImageDownloadProcess()
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

                pTotal = 0;
                pTotal = downloadList.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    for (int i = 0; i < pTotal; ++i)
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "Enemy", "Normal_Crop", $"{downloadList[i]}.png");
                        string target = Path.Combine(ETC.cachePath, "Enemy", "Normal_Crop", $"{downloadList[i]}.gfdcache");

                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListEnemy(searchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_DownloadCropImageFail, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
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
            pNow += 1;

            totalProgressBar.Progress = Convert.ToInt32(pNow / Convert.ToDouble(pTotal) * 100);
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
                View v = inflater.Inflate(Resource.Layout.EnemyFilterLayout, null);

                for (int i = 0; i < enemyTypeFilters.Length; ++i)
                    v.FindViewById<CheckBox>(enemyTypeFilters[i]).Checked = filter_EnemyType[i];

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
                ETC.ShowSnackbar(snackbarLayout, Resource.String.FilterBox_InitError, Snackbar.LengthLong);
            }
        }

        private void ApplyFilter(View view)
        {
            try
            {
                for (int i = 0; i < enemyTypeFilters.Length; ++i)
                    filter_EnemyType[i] = view.FindViewById<CheckBox>(enemyTypeFilters[i]).Checked;

                CheckApplyFilter();

                ListEnemy(searchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void CheckApplyFilter()
        {
            for (int i = 0; i < filter_EnemyType.Length; ++i)
                if (filter_EnemyType[i])
                {
                    hasApplyFilter[0] = true;
                    break;
                }
                else
                    hasApplyFilter[0] = false;
        }

        private void ResetFilter()
        {
            try
            {
                for (int i = 0; i < enemyTypeFilters.Length; ++i)
                    filter_EnemyType[i] = false;

                for (int i = 0; i < hasApplyFilter.Length; ++i)
                    hasApplyFilter[i] = false;

                ListEnemy(searchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListEnemy(string searchText)
        {
            subList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < rootList.Count; ++i)
                {
                    Enemy enemy = rootList[i];

                    if (CheckFilter(enemy))
                        continue;

                    if (searchText != "")
                    {
                        string name = enemy.Name.ToUpper();

                        if (!name.Contains(searchText))
                            continue;
                    }

                    subList.Add(enemy);
                }

                subList.Sort(SortEnemy);

                var adapter = new EnemyListAdapter(subList, this);

                if (!adapter.HasOnItemClick())
                    adapter.ItemClick += Adapter_ItemClick;

                await Task.Delay(100);

                RunOnUiThread(() => { mEnemyListView.SetAdapter(adapter); });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private int SortEnemy(Enemy x, Enemy y)
        {
            return x.Name.CompareTo(y.Name);
        }

        private bool CheckFilter(Enemy enemy)
        {
            if (hasApplyFilter[0])
            {
                switch (enemy.IsBoss)
                {
                    case false:
                        if (!filter_EnemyType[0])
                            return true;
                        break;
                    case true:
                        if (!filter_EnemyType[1])
                            return true;
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
        public ImageView AffiliationImage { get; private set; }
        public TextView Affiliation { get; private set; }
        public TextView Name { get; private set; }
        public TextView CodeName { get; private set; }

        public EnemyListViewHolder(View view, Action<int> listener) : base(view)
        {
            Type = view.FindViewById<TextView>(Resource.Id.EnemyListType);
            //TypeIcon = view.FindViewById<ImageView>(Resource.Id.EnemyListTypeIcon);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.EnemyListSmallImage);
            AffiliationImage = view.FindViewById<ImageView>(Resource.Id.EnemyListAffiliationImage);
            Affiliation = view.FindViewById<TextView>(Resource.Id.EnemyListAffiliation);
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
            ItemClick?.Invoke(this, position);
        }

        public bool HasOnItemClick()
        {
            if (ItemClick == null)
                return false;
            else
                return true;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            EnemyListViewHolder vh = holder as EnemyListViewHolder;

            var item = items[position];

            try
            {
                int typeIconId = 0;
                string enemy_type = "";

                switch (item.IsBoss)
                {
                    default:
                    case false:
                        typeIconId = Resource.Drawable.Grade_N;
                        enemy_type = "NM";
                        break;
                    case true:
                        typeIconId = Resource.Drawable.Grade_S;
                        enemy_type = "Boss";
                        break;
                }
                //vh.TypeIcon.SetImageResource(typeIconId);

                vh.Type.Text = enemy_type;

                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false))
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;
                    string FilePath = Path.Combine(ETC.cachePath, "Enemy", "Normal_Crop", $"{item.CodeName}.gfdcache");

                    if (File.Exists(FilePath))
                        vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                }
                else vh.SmallImage.Visibility = ViewStates.Gone;

                int affiliationIconId = 0;

                switch (item.Affiliation)
                {
                    case "SANGVIS FERRI":
                        affiliationIconId = Resource.Drawable.SFLogo;
                        break;
                    default:
                    case "I.O.P Manufacturing Company":
                        affiliationIconId = Resource.Drawable.IOPLogo;
                        break;
                    case "Mind Map System":
                        affiliationIconId = Resource.Drawable.IOPLogo;
                        break;
                    case "KCCO":
                        affiliationIconId = Resource.Drawable.KCCOLogo;
                        break;
                    case "Paradeus":
                        affiliationIconId = Resource.Drawable.ParadeusLogo;
                        break;
                    case "E.L.I.D.":
                        affiliationIconId = Resource.Drawable.ELIDLogo;
                        break;
                }
                vh.AffiliationImage.SetImageResource(affiliationIconId);

                vh.Name.Text = item.Name;
                vh.CodeName.Text = item.CodeName;
                vh.Affiliation.Text = item.Affiliation;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, context);
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}