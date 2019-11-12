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
using System.IO;

namespace GFI_with_GFS_A
{
    [Activity(Label = "@string/Activity_FairyMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FairyDBMainActivity : BaseAppCompatActivity
    {
        delegate void DownloadProgress();

        private enum SortType { Name, Number, ProductTime }
        private enum SortOrder { Ascending, Descending }
        private SortType sortType = SortType.Name;
        private SortOrder sortOrder = SortOrder.Ascending;

        private List<Fairy> rootList = new List<Fairy>();
        private List<Fairy> subList = new List<Fairy>();
        private List<int> downloadList = new List<int>();

        int[] typeFilters = { Resource.Id.FairyFilterTypeCombat, Resource.Id.FairyFilterTypeStrategy };
        int[] productTimeFilters = { Resource.Id.FairyFilterProductHour, Resource.Id.FairyFilterProductMinute };

        private bool[] hasApplyFilter = { false, false };
        private int[] filterProductTime = { 0, 0 };
        private bool[] filterType = { true, true };
        private bool canRefresh = false;

        private string searchViewText = "";

        private Android.Support.V7.Widget.Toolbar toolbar;
        private Android.Support.V7.Widget.SearchView searchView;
        private RecyclerView mFairyListView;
        private RecyclerView.LayoutManager mainLayoutManager;
        private CoordinatorLayout snackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.FairyDBListLayout);

                SetTitle(Resource.String.FairyDBMainActivity_Title);

                canRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.FairyDBMainToolbar);
                searchView = FindViewById<Android.Support.V7.Widget.SearchView>(Resource.Id.FairyDBSearchView);
                searchView.QueryTextChange += (sender, e) =>
                {
                    searchViewText = e.NewText;
                    _ = ListFairy(new int[] { filterProductTime[0], filterProductTime[1] }, searchViewText);
                };
                mFairyListView = FindViewById<RecyclerView>(Resource.Id.FairyDBRecyclerView);
                mainLayoutManager = new LinearLayoutManager(this);
                mFairyListView.SetLayoutManager(mainLayoutManager);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FairyDBSnackbarLayout);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetTitle(Resource.String.FairyDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                InitProcess();

                _ = ListFairy(new int[] { filterProductTime[0], filterProductTime[1] });

                /*if ((ETC.locale.Language == "ko") && ETC.sharedPreferences.GetBoolean("Help_DBList", true))
                {
                    ETC.RunHelpActivity(this, "DBList");
                }*/
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.FairyDBMenu, menu);

            var cacheItem = menu.FindItem(Resource.Id.RefreshFairyCropImageCache);
            _ = canRefresh ? cacheItem.SetVisible(true) : cacheItem.SetVisible(false);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.FairyDBMainFilter:
                    InitFilterBox();
                    break;
                case Resource.Id.FairyDBMainSort:
                    InitSortBox();
                    break;
                case Resource.Id.RefreshEquipCropImageCache:
                    downloadList.Clear();

                    for (int i = 0; i < rootList.Count; ++i)
                    {
                        string FilePath = Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop", $"{rootList[i].DicNumber}.gfdcache");

                        if (!File.Exists(FilePath))
                        {
                            downloadList.Add(rootList[i].DicNumber);
                        }
                    }

                    downloadList.TrimExcess();
                    ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(FairyCropImageDownloadProcess));
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void InitProcess()
        {
            CreateListObject();

            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false))
            {
                if (CheckFairyCropImage())
                {
                    ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(FairyCropImageDownloadProcess));
                }
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.fairyList.Rows)
                {
                    rootList.Add(new Fairy(dr));
                }

                rootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthShort);
            }
        }

        private bool CheckFairyCropImage()
        {
            downloadList.Clear();

            for (int i = 0; i < rootList.Count; ++i)
            {
                string FilePath = Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop", $"{rootList[i].DicNumber}.gfdcache");

                if (!File.Exists(FilePath))
                {
                    downloadList.Add(rootList[i].DicNumber);
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

        private async void FairyCropImageDownloadProcess()
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

                        totalProgressBar.Progress = Convert.ToInt32((pNow / Convert.ToDouble(pTotal)) * 100);
                        totalProgress.Text = $"{totalProgressBar.Progress}%";
                    };

                    for (int i = 0; i < pTotal; ++i)
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "Fairy", "Normal_Crop", $"{downloadList[i]}.png");
                        string target = Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop", $"{downloadList[i]}.gfdcache");
                       
                        await wc.DownloadFileTaskAsync(url, target).ConfigureAwait(false);
                    }
                }

                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                _ = ListFairy( new int[] { filterProductTime[0], filterProductTime[1] }, searchViewText);
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
                Resources.GetString(Resource.String.Sort_SortMethod_Name),
                Resources.GetString(Resource.String.Sort_SortMethod_Number),
                Resources.GetString(Resource.String.Sort_SortMethod_ProductTime),
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

                using (Android.Support.V7.App.AlertDialog.Builder FilterBox = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGVertical))
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

                _ = ListFairy(new int[] { filterProductTime[0], filterProductTime[1] }, searchViewText);
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

                _ = ListFairy(new int[] { filterProductTime[0], filterProductTime[1] }, searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void Adapter_ItemClick(object sender, int position)
        {
            await Task.Delay(100);
            var FairyInfo = new Intent(this, typeof(FairyDBDetailActivity));
            FairyInfo.PutExtra("DicNum", subList[position].DicNumber);
            StartActivity(FairyInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void InitFilterBox()
        {
            try
            {
                View v = LayoutInflater.Inflate(Resource.Layout.FairyFilterLayout, null);

                v.FindViewById<NumberPicker>(Resource.Id.FairyFilterProductHour).MaxValue = 12;
                v.FindViewById<NumberPicker>(Resource.Id.FairyFilterProductMinute).MaxValue = 59;

                for (int i = 0; i < typeFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(typeFilters[i]).Checked = filterType[i];
                }
                for (int i = 0; i < productTimeFilters.Length; ++i)
                {
                    v.FindViewById<NumberPicker>(productTimeFilters[i]).Value = filterProductTime[i];
                }

                using (Android.Support.V7.App.AlertDialog.Builder FilterBox = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGVertical))
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
                for (int i = 0; i < typeFilters.Length; ++i)
                {
                    filterType[i] = view.FindViewById<CheckBox>(typeFilters[i]).Checked;
                }
                for (int i = 0; i < productTimeFilters.Length; ++i)
                {
                    filterProductTime[i] = view.FindViewById<NumberPicker>(productTimeFilters[i]).Value;
                }

                CheckApplyFilter();

                _ = ListFairy(new int[] { filterProductTime[0], filterProductTime[1] }, searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void CheckApplyFilter()
        {
            for (int i = 0; i < productTimeFilters.Length; ++i)
            {
                hasApplyFilter[0] = filterProductTime[i] != 0;

                if (hasApplyFilter[0])
                {
                    break;
                }
            }
            for (int i = 0; i < typeFilters.Length; ++i)
            {
                hasApplyFilter[1] = filterType[i];

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
                for (int i = 0; i < typeFilters.Length; ++i)
                {
                    filterType[i] = true;
                }
                for (int i = 0; i < productTimeFilters.Length; ++i)
                {
                    filterProductTime[i] = 0;
                }

                for (int i = 0; i < hasApplyFilter.Length; ++i)
                {
                    hasApplyFilter[i] = false;
                }

                _ = ListFairy(new int[] { filterProductTime[0], filterProductTime[1] }, searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async Task ListFairy(int[] pTime, string searchText = "")
        {
            subList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < rootList.Count; ++i)
                {
                    Fairy fairy = rootList[i];

                    if ((pTime[0] + pTime[1]) != 0)
                    {
                        if (fairy.ProductTime != ((pTime[0] * 60) + pTime[1]))
                        {
                            continue;
                        }
                    }

                    if (CheckFilter(fairy))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        string name = fairy.Name.ToUpper();

                        if (!name.Contains(searchText))
                        {
                            continue;
                        }
                    }

                    subList.Add(fairy);
                }

                subList.Sort(SortFairyName);

                var adapter = new FairyListAdapter(subList, this);

                if (!adapter.HasOnItemClick())
                {
                    adapter.ItemClick += Adapter_ItemClick;
                }

                await Task.Delay(100);

                RunOnUiThread(() => { mFairyListView.SetAdapter(adapter); });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private bool CheckFilter(Fairy fairy)
        {
            if (hasApplyFilter[1])
            {
                switch (fairy.Type)
                {
                    case "전투":
                        if (!filterType[0])
                        {
                            return true;
                        }
                        break;
                    case "책략":
                        if (!filterType[1])
                        {
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        private int SortFairyName(Fairy x, Fairy y)
        {
            if (sortOrder == SortOrder.Descending)
            {
                Fairy temp = x;
                x = y;
                y = temp;
            }

            switch (sortType)
            {
                case SortType.Name:
                default:
                    return x.Name.CompareTo(y.Name);
                case SortType.ProductTime:
                    int xTime = x.ProductTime;
                    int yTime = y.ProductTime;

                    if ((xTime == 0) && (yTime != 0))
                    {
                        return 1;
                    }
                    else if ((yTime == 0) && (xTime != 0))
                    {
                        return -1;
                    }
                    else if (xTime == yTime)
                    {
                        return x.Name.CompareTo(y.Name);
                    }
                    else
                    {
                        return xTime.CompareTo(yTime);
                    }
                case SortType.Number:
                    return x.DicNumber.CompareTo(y.DicNumber);
            }
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