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
    [Activity(Label = "@string/Acticity_EnemyMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EnemyDBMainActivity : BaseAppCompatActivity
    {
        delegate Task DownloadProgress();

        private enum SortType { Name }
        private enum SortOrder { Ascending, Descending }
        private SortType sortType = SortType.Name;
        private SortOrder sortOrder = SortOrder.Ascending;

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

        private bool[] hasApplyFilter = { false, false };
        private bool[] filterEnemyType = { false, false };
        private bool[] filterEnemyAffiliation = { false, false, false, false, false, false };
        private bool canRefresh = false;

        private Android.Support.V7.Widget.Toolbar toolbar;
        private Android.Support.V7.Widget.SearchView searchView;
        private RecyclerView mEnemyRecyclerView;
        private RecyclerView.LayoutManager mainLayoutManager;
        private CoordinatorLayout snackbarLayout;

        string searchViewText = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Toolbar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.EnemyDBListLayout);

                canRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.EnemyDBMainToolbar);
                searchView = FindViewById<Android.Support.V7.Widget.SearchView>(Resource.Id.EnemyDBSearchView);
                searchView.QueryTextChange += (sender, e) =>
                {
                    searchViewText = e.NewText;
                    _ = ListEnemy(searchViewText);
                };
                mEnemyRecyclerView = FindViewById<RecyclerView>(Resource.Id.EnemyDBRecyclerView);
                mainLayoutManager = new LinearLayoutManager(this);
                mEnemyRecyclerView.SetLayoutManager(mainLayoutManager);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EnemyDBSnackbarLayout);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetTitle(Resource.String.EnemyDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                InitProcess();

                _ = ListEnemy();

                /*if ((ETC.locale.Language == "ko") && ETC.sharedPreferences.GetBoolean("Help_DBList", true))
                {
                    ETC.RunHelpActivity(this, "DBList");
                }*/
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.EnemyDBMenu, menu);

            var cacheItem = menu?.FindItem(Resource.Id.RefreshEnemyCropImageCache);
            _ = canRefresh ? cacheItem.SetVisible(true) : cacheItem.SetVisible(false);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.EnemyDBMainFilter:
                    InitFilterBox();
                    break;
                case Resource.Id.EnemyDBMainSort:
                    InitSortBox();
                    break;
                case Resource.Id.RefreshEnemyCropImageCache:
                    downloadList.Clear();

                    foreach (Enemy enemy in rootList)
                    {
                        downloadList.Add(enemy.CodeName);
                    }

                    downloadList.TrimExcess();

                    ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(EnemyCropImageDownloadProcess));
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void InitProcess()
        {
            CreateListObject();

            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false))
            {
                if (CheckEnemyCropImage())
                {
                    ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(EnemyCropImageDownloadProcess));
                }
            }     
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.enemyList.Rows)
                {
                    bool isCreate = false;

                    foreach (Enemy enemy in rootList)
                    {
                        if (enemy.CodeName == (string)dr["CodeName"])
                        {
                            isCreate = true;
                            break;
                        }
                    }

                    if (!isCreate)
                    {
                        rootList.Add(new Enemy(dr));
                    }
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
                string filePath = Path.Combine(ETC.cachePath, "Enemy", "Normal_Crop", $"{enemy.CodeName}.gfdcache");

                if (!File.Exists(filePath))
                {
                    downloadList.Add(enemy.CodeName);
                }
            }

            downloadList.TrimExcess();

            return !(downloadList.Count == 0);
        }

        private void ShowDownloadCheckMessage(int title, int message, DownloadProgress method)
        {
            using (Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBG))
            {
                ad.SetTitle(title);
                ad.SetMessage(message);
                ad.SetCancelable(true);
                ad.SetPositiveButton(Resource.String.AlertDialog_Download, delegate { method(); });
                ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

                ad.Show();
            }
        }

        private async Task EnemyCropImageDownloadProcess()
        {
            Dialog dialog;
            ProgressBar totalProgressBar;
            ProgressBar nowProgressBar;
            TextView totalProgress;
            TextView nowProgress;

            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            int pNow = 0;
            int pTotal = 0;

            using (Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGDownload))
            {
                pd.SetTitle(Resource.String.DBList_DownloadCropImageTitle);
                pd.SetCancelable(false);
                pd.SetView(v);

                dialog = pd.Create();
                dialog.Show();
            }

            try
            {
                totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
                totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
                nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
                nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

                pTotal = downloadList.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = pNow;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (sender, e) =>
                    {
                        nowProgressBar.Progress = e.ProgressPercentage;
                        nowProgress.Text = $"{e.ProgressPercentage}%";
                    };
                    wc.DownloadFileCompleted += (sender, e) =>
                    {
                        pNow += 1;

                        totalProgressBar.Progress = Convert.ToInt32(pNow / Convert.ToDouble(pTotal) * 100);
                        totalProgress.Text = $"{totalProgressBar.Progress}%";
                    };

                    for (int i = 0; i < pTotal; ++i)
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "Enemy", "Normal_Crop", $"{downloadList[i]}.png");
                        string target = Path.Combine(ETC.cachePath, "Enemy", "Normal_Crop", $"{downloadList[i]}.gfdcache");

                        await wc.DownloadFileTaskAsync(url, target).ConfigureAwait(false);
                    }
                }

                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                _ = ListEnemy(searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_DownloadCropImageFail, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
            finally
            {
                dialog.Dismiss();
            }
        }

        private void InitSortBox()
        {
            string[] sortTypeList =
            {
                Resources.GetString(Resource.String.Sort_SortMethod_Name)
            };

            try
            {
                View v = LayoutInflater.Inflate(Resource.Layout.CommonSorterLayout, null);

                var adapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, sortTypeList);
                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                var sortSpinner = v.FindViewById<Spinner>(Resource.Id.CommonSortSpinner);

                sortSpinner.Adapter = adapter;
                sortSpinner.SetSelection((int)sortType);

                switch (sortOrder)
                {
                    default:
                    case SortOrder.Ascending:
                        v.FindViewById<RadioButton>(Resource.Id.CommonSortOrderAscending).Checked = true;
                        break;
                    case SortOrder.Descending:
                        v.FindViewById<RadioButton>(Resource.Id.CommonSortOrderDescending).Checked = true;
                        break;
                }

                using (var FilterBox = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGVertical))
                {
                    FilterBox.SetTitle(Resource.String.DBList_SortBoxTitle);
                    FilterBox.SetView(v);
                    FilterBox.SetPositiveButton(Resource.String.AlertDialog_Set, delegate { ApplySort(v); });
                    FilterBox.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                    FilterBox.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate { ResetSort(); });

                    FilterBox.Show();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.SortBox_InitError, Snackbar.LengthLong);
            }
        }

        private void ApplySort(View view)
        {
            try
            {
                sortType = (SortType)view.FindViewById<Spinner>(Resource.Id.CommonSortSpinner).SelectedItemPosition;

                if (view.FindViewById<RadioButton>(Resource.Id.CommonSortOrderAscending).Checked)
                {
                    sortOrder = SortOrder.Ascending;
                }
                else if (view.FindViewById<RadioButton>(Resource.Id.CommonSortOrderDescending).Checked)
                {
                    sortOrder = SortOrder.Descending;
                }
                else
                {
                    sortOrder = SortOrder.Ascending;
                }

                _ = ListEnemy(searchViewText ?? "");
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void ResetSort()
        {
            try
            {
                sortType = SortType.Name;
                sortOrder = SortOrder.Ascending;

                _ = ListEnemy(searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private void InitFilterBox()
        {
            var inflater = LayoutInflater;

            try
            {
                View v = inflater.Inflate(Resource.Layout.EnemyFilterLayout, null);

                for (int i = 0; i < enemyTypeFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(enemyTypeFilters[i]).Checked = filterEnemyType[i];
                }
                for (int i = 0; i < enemyAffiliationFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(enemyAffiliationFilters[i]).Checked = filterEnemyAffiliation[i];
                }

                using (var FilterBox = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGVertical))
                {
                    FilterBox.SetTitle(Resource.String.DBList_FilterBoxTitle);
                    FilterBox.SetView(v);
                    FilterBox.SetPositiveButton(Resource.String.AlertDialog_Set, delegate { ApplyFilter(v); });
                    FilterBox.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                    FilterBox.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate { ResetFilter(); });

                    FilterBox.Show();
                }
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
                {
                    filterEnemyType[i] = view.FindViewById<CheckBox>(enemyTypeFilters[i]).Checked;
                }
                for (int i = 0; i < enemyAffiliationFilters.Length; ++i)
                {
                    filterEnemyAffiliation[i] = view.FindViewById<CheckBox>(enemyAffiliationFilters[i]).Checked;
                }

                CheckApplyFilter();

                _ = ListEnemy(searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void CheckApplyFilter()
        {
            for (int i = 0; i < filterEnemyType.Length; ++i)
            {
                hasApplyFilter[0] = filterEnemyType[i];

                if (hasApplyFilter[0])
                {
                    break;
                }
            }
            for (int i = 0; i < filterEnemyAffiliation.Length; ++i)
            {
                hasApplyFilter[1] = filterEnemyAffiliation[i];

                if (hasApplyFilter[1])
                {
                    break;
                }
            }
        }

        private void ResetFilter()
        {
            try
            {
                for (int i = 0; i < enemyTypeFilters.Length; ++i)
                {
                    filterEnemyType[i] = false;
                }
                for (int i = 0; i < enemyAffiliationFilters.Length; ++i)
                {
                    filterEnemyAffiliation[i] = false;
                }

                for (int i = 0; i < hasApplyFilter.Length; ++i)
                {
                    hasApplyFilter[i] = false;
                }

                _ = ListEnemy(searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async Task ListEnemy(string searchText = "")
        {
            subList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < rootList.Count; ++i)
                {
                    Enemy enemy = rootList[i];

                    if (CheckFilter(enemy))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        string name = enemy.Name.ToUpper();

                        if (!name.Contains(searchText))
                        {
                            continue;
                        }
                    }

                    subList.Add(enemy);
                }

                subList.Sort(SortEnemy);

                var adapter = new EnemyListAdapter(subList, this);

                if (!adapter.HasOnItemClick())
                {
                    adapter.ItemClick += Adapter_ItemClick;
                }

                await Task.Delay(100);

                RunOnUiThread(() => 
                {
                    mEnemyRecyclerView.SetAdapter(null);
                    mEnemyRecyclerView.SetAdapter(adapter); 
                });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
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

        private int SortEnemy(Enemy x, Enemy y)
        {
            if (sortOrder == SortOrder.Descending)
            {
                Enemy temp = x;
                x = y;
                y = temp;
            }

            return x.Name.CompareTo(y.Name);
        }

        private bool CheckFilter(Enemy enemy)
        {
            if (hasApplyFilter[0])
            {
                switch (enemy.IsBoss)
                {
                    case false:
                        if (!filterEnemyType[0])
                        {
                            return true;
                        }
                        break;
                    case true:
                        if (!filterEnemyType[1])
                        {
                            return true;
                        }
                        break;
                }
            }

            if (hasApplyFilter[1])
            {
                switch (enemy.Affiliation)
                {
                    case "SANGVIS FERRI":
                        if (!filterEnemyAffiliation[0])
                        {
                            return true;
                        }
                        break;
                    case "I.O.P Manufacturing Company":
                        if (!filterEnemyAffiliation[1])
                        {
                            return true;
                        }
                        break;
                    case "KCCO":
                        if (!filterEnemyAffiliation[2])
                        {
                            return true;
                        }
                        break;
                    case "Paradeus":
                        if (!filterEnemyAffiliation[3])
                        {
                            return true;
                        }
                        break;
                    case "Mind Map System":
                        if (!filterEnemyAffiliation[4])
                        {
                            return true;
                        }
                        break;
                    case "E.L.I.D.":
                        if (!filterEnemyAffiliation[5])
                        {
                            return true;
                        }
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
                string enemyType = "";

                switch (item.IsBoss)
                {
                    default:
                    case false:
                        typeIconId = Resource.Drawable.Grade_N;
                        enemyType = "NM";
                        break;
                    case true:
                        typeIconId = Resource.Drawable.Grade_S;
                        enemyType = "Boss";
                        break;
                }
                //vh.TypeIcon.SetImageResource(typeIconId);

                vh.Type.Text = enemyType;

                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false))
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;
                    string filePath = Path.Combine(ETC.cachePath, "Enemy", "Normal_Crop", $"{item.CodeName}.gfdcache");

                    if (File.Exists(filePath))
                    {
                        vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(filePath));
                    }
                }
                else
                {
                    vh.SmallImage.Visibility = ViewStates.Gone;
                }

                int affiliationIconId = 0;

                affiliationIconId = item.Affiliation switch
                {
                    "SANGVIS FERRI" => Resource.Drawable.SFLogo,
                    "Mind Map System" => Resource.Drawable.IOPLogo,
                    "KCCO" => Resource.Drawable.KCCOLogo,
                    "Paradeus" => Resource.Drawable.ParadeusLogo,
                    "E.L.I.D." => Resource.Drawable.ELIDLogo,
                    _ => Resource.Drawable.IOPLogo,
                };
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