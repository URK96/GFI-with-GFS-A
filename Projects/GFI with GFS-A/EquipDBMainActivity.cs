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
using Xamarin.Essentials;

namespace GFI_with_GFS_A
{
    [Activity(Label = "@string/Activity_EquipMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EquipDBMainActivity : BaseAppCompatActivity
    {
        delegate void DownloadProgress();

        private enum SortType { Name, ProductTime }
        private enum SortOrder { Ascending, Descending }
        private SortType sortType = SortType.Name;
        private SortOrder sortOrder = SortOrder.Ascending;

        private List<Equip> rootList = new List<Equip>();
        private List<Equip> subList = new List<Equip>();
        private List<string> downloadList = new List<string>();

        int[] gradeFilters = { Resource.Id.EquipFilterGrade2, Resource.Id.EquipFilterGrade3, Resource.Id.EquipFilterGrade4, Resource.Id.EquipFilterGrade5, Resource.Id.EquipFilterGradeExtra };
        int[] categoryFilters = { Resource.Id.EquipFilterCategoryAttach, Resource.Id.EquipFilterCategoryBullet, Resource.Id.EquipFilterCategoryDoll };
        int[] productTimeFilters = { Resource.Id.EquipFilterProductHour, Resource.Id.EquipFilterProductMinute, Resource.Id.EquipFilterProductNearRange };

        private bool[] hasApplyFilter = { false, false, false };
        private int[] filterProductTime = { 0, 0, 0 };
        private bool[] filterGrade = { false, false, false, false, false };
        private bool[] filterCategory = { false, false, false };
        private bool canRefresh = false;

        private string searchViewText = "";

        private Android.Support.V7.Widget.Toolbar toolbar;
        private Android.Support.V7.Widget.SearchView searchView;
        private RecyclerView mEquipListView;
        private RecyclerView.LayoutManager mainRecyclerManager;
        private CoordinatorLayout snackbarLayout;

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
                SetContentView(Resource.Layout.EquipDBListLayout);

                canRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.EquipDBMainToolbar);
                searchView = FindViewById<Android.Support.V7.Widget.SearchView>(Resource.Id.EquipDBSearchView);
                searchView.QueryTextChange += (sender, e) =>
                {
                    searchViewText = e.NewText;
                    _ = ListEquip(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
                };
                mEquipListView = FindViewById<RecyclerView>(Resource.Id.EquipDBRecyclerView);
                mainRecyclerManager = new LinearLayoutManager(this);
                mEquipListView.SetLayoutManager(mainRecyclerManager);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EquipDBSnackbarLayout);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetTitle(Resource.String.EquipDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                InitProcess();

                _ = ListEquip(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2]);

                /*if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true)))
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
            MenuInflater.Inflate(Resource.Menu.EquipDBMenu, menu);

            if (canRefresh)
            {
                menu?.FindItem(Resource.Id.RefreshEquipCropImageCache).SetVisible(true);
            }

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.EquipDBMainFilter:
                    InitFilterBox();
                    break;
                case Resource.Id.EquipDBMainSort:
                    InitSortBox();
                    break;
                case Resource.Id.RefreshEquipCropImageCache:
                    downloadList.Clear();

                    string iconName = "";

                    foreach (Equip equip in rootList)
                    {
                        iconName = equip.Icon;

                        if (!downloadList.Contains(iconName))
                        {
                            downloadList.Add(iconName);
                        }
                    }

                    downloadList.TrimExcess();
                    ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(EquipCropImageDownloadProcess));
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void InitProcess()
        {
            CreateListObject();

            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false))
            {
                if (CheckCropImage())
                {
                    ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(EquipCropImageDownloadProcess));
                }
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.equipmentList.Rows)
                {
                    rootList.Add(new Equip(dr));
                }

                rootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthShort);
            }
        }

        private bool CheckCropImage()
        {
            downloadList.Clear();

            for (int i = 0; i < rootList.Count; ++i)
            {
                Equip equip = rootList[i];
                string filePath = Path.Combine(ETC.cachePath, "Equip", "Normal", $"{equip.Icon}.gfdcache");

                if (!File.Exists(filePath))
                {
                    if (!downloadList.Contains(equip.Icon))
                    {
                        downloadList.Add(equip.Icon);
                    }
                }
            }

            downloadList.TrimExcess();

            return !(downloadList.Count == 0);
        }

        private void ShowDownloadCheckMessage(int title, int message, DownloadProgress method)
        {
            var ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBG);
            ad.SetTitle(title);
            ad.SetMessage(message);
            ad.SetCancelable(true);
            ad.SetPositiveButton(Resource.String.AlertDialog_Download, delegate { method(); });
            ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

            ad.Show();
        }

        private async void EquipCropImageDownloadProcess()
        {
            Dialog dialog;
            ProgressBar totalProgressBar;
            ProgressBar nowProgressBar;
            TextView totalProgress;
            TextView nowProgress;

            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            int pNow = 0;
            int pTotal = 0;

            var pd = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGDownload);
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

                pTotal = downloadList.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = pNow;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (sender, e) =>
                    {
                        nowProgressBar.Progress = e.ProgressPercentage;
                        MainThread.BeginInvokeOnMainThread(() => { nowProgress.Text = $"{e.ProgressPercentage}%"; });
                    };
                    wc.DownloadFileCompleted += (sender, e) =>
                    {
                        pNow += 1;

                        totalProgressBar.Progress = Convert.ToInt32((pNow / Convert.ToDouble(pTotal)) * 100);
                        MainThread.BeginInvokeOnMainThread(() => { totalProgress.Text = $"{totalProgressBar.Progress}%"; });
                    };

                    for (int i = 0; i < pTotal; ++i)
                    {
                        string url = Path.Combine(ETC.server, "Data", "Images", "Equipments", $"{downloadList[i]}.png");
                        string target = Path.Combine(ETC.cachePath, "Equip", "Normal", $"{downloadList[i]}.gfdcache");

                        await wc.DownloadFileTaskAsync(url, target).ConfigureAwait(false);
                    }
                }

                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                _ = ListEquip(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
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
                Resources.GetString(Resource.String.Sort_SortMethod_ProductTime)
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

                var filterBox = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGVertical);
                filterBox.SetTitle(Resource.String.DBList_SortBoxTitle);
                filterBox.SetView(v);
                filterBox.SetPositiveButton(Resource.String.AlertDialog_Set, delegate { ApplySort(v); });
                filterBox.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                filterBox.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate { ResetSort(); });

                filterBox.Show();
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

                _ = ListEquip(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
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

                _ = ListEquip(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private void InitFilterBox()
        {
            try
            {
                View v = LayoutInflater.Inflate(Resource.Layout.EquipFilterLayout, null);

                v.FindViewById<NumberPicker>(Resource.Id.EquipFilterProductHour).MaxValue = 0;
                v.FindViewById<NumberPicker>(Resource.Id.EquipFilterProductMinute).MaxValue = 59;
                v.FindViewById<NumberPicker>(Resource.Id.EquipFilterProductNearRange).MaxValue = 10;

                for (int i = 0; i < productTimeFilters.Length; ++i)
                {
                    v.FindViewById<NumberPicker>(productTimeFilters[i]).Value = filterProductTime[i];
                }
                for (int i = 0; i < gradeFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(gradeFilters[i]).Checked = filterGrade[i];
                }
                for (int i = 0; i < categoryFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(categoryFilters[i]).Checked = filterCategory[i];
                }

                var filterBox = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.dialogBGVertical);
                filterBox.SetTitle(Resource.String.DBList_FilterBoxTitle);
                filterBox.SetView(v);
                filterBox.SetPositiveButton(Resource.String.AlertDialog_Set, delegate { ApplyFilter(v); });
                filterBox.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                filterBox.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate { ResetFilter(); });

                filterBox.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.FilterBox_InitError, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
            }
        }

        private void ApplyFilter(View view)
        {
            try
            {
                for (int i = 0; i < productTimeFilters.Length; ++i)
                {
                    filterProductTime[i] = view.FindViewById<NumberPicker>(productTimeFilters[i]).Value;
                }
                for (int i = 0; i < gradeFilters.Length; ++i)
                {
                    filterGrade[i] = view.FindViewById<CheckBox>(gradeFilters[i]).Checked;
                }
                for (int i = 0; i < categoryFilters.Length; ++i)
                {
                    filterCategory[i] = view.FindViewById<CheckBox>(categoryFilters[i]).Checked;
                }

                CheckApplyFilter();

                _ = ListEquip(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
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
            for (int i = 0; i < gradeFilters.Length; ++i)
            {
                hasApplyFilter[1] = filterGrade[i];

                if (hasApplyFilter[1])
                {
                    break;
                }
            }
            for (int i = 0; i < categoryFilters.Length; ++i)
            {
                hasApplyFilter[2] = filterCategory[i];

                if (hasApplyFilter[2])
                {
                    break;
                }
            }
        }

        private void ResetFilter()
        {
            try
            {
                for (int i = 0; i < productTimeFilters.Length; ++i)
                {
                    filterProductTime[i] = 0;
                }
                for (int i = 0; i < gradeFilters.Length; ++i)
                {
                    filterGrade[i] = false;
                }
                for (int i = 0; i < categoryFilters.Length; ++i)
                {
                    filterCategory[i] = false;
                }

                for (int i = 0; i < hasApplyFilter.Length; ++i)
                {
                    hasApplyFilter[i] = false;
                }

                _ = ListEquip(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async Task ListEquip(int[] pTime, int pRange, string searchText = "")
        {
            subList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < rootList.Count; ++i)
                {
                    Equip equip = rootList[i];

                    if ((pTime[0] + pTime[1]) != 0)
                    {
                        if (!CheckEquipByProductTime(pTime, pRange, equip.ProductTime))
                        {
                            continue;
                        }
                    }

                    if (CheckFilter(equip))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        string name = equip.Name.ToUpper();

                        if (!name.Contains(searchText))
                        {
                            continue;
                        }
                    }

                    subList.Add(equip);
                }

                subList.Sort(SortEquip);

                var adapter = new EquipListAdapter(subList, this);

                if (!adapter.HasOnItemClick())
                {
                    adapter.ItemClick += Adapter_ItemClick;
                }

                await Task.Delay(100);

                RunOnUiThread(() => { mEquipListView.SetAdapter(adapter); });
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
            var EquipInfo = new Intent(this, typeof(EquipDBDetailActivity));
            EquipInfo.PutExtra("Id", subList[position].Id);
            StartActivity(EquipInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private static bool CheckEquipByProductTime(int[] pTime, int range, int dTime)
        {
            int pTimeM = (pTime[0] * 60) + pTime[1];

            for (int i = (pTimeM - range); i <= (pTimeM + range); ++i)
            {
                if (dTime == i)
                {
                    return true;
                }
            }

            return false;
        }

        private int SortEquip(Equip x, Equip y)
        {
            if (sortOrder == SortOrder.Descending)
            {
                Equip temp = x;
                x = y;
                y = temp;
            }

            switch (sortType)
            {
                default:
                case SortType.Name:
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
            }
        }

        private bool CheckFilter(Equip equip)
        {
            if (hasApplyFilter[1])
            {
                switch (equip.Grade)
                {
                    case 2:
                        if (!filterGrade[0])
                        {
                            return true;
                        }
                        break;
                    case 3:
                        if (!filterGrade[1])
                        {
                            return true;
                        }
                        break;
                    case 4:
                        if (!filterGrade[2])
                        {
                            return true;
                        }
                        break;
                    case 5:
                        if (!filterGrade[3])
                        {
                            return true;
                        }
                        break;
                    case 0:
                        if (!filterGrade[4])
                        {
                            return true;
                        }
                        break;
                }
            }

            if (hasApplyFilter[2])
            {
                switch (equip.Category)
                {
                    case string s when s == Resources.GetString(Resource.String.Common_Accessories):
                        if (!filterCategory[0])
                        {
                            return true;
                        }
                        break;
                    case string s when s == Resources.GetString(Resource.String.Common_Magazine):
                        if (!filterCategory[1])
                        {
                            return true;
                        }
                        break;
                    case string s when s == Resources.GetString(Resource.String.Common_TDoll):
                        if (!filterCategory[2])
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
                    string FilePath = System.IO.Path.Combine(ETC.cachePath, "Equip", "Normal", $"{item.Icon}.gfdcache");
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
                ETC.LogError(ex, context);
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}