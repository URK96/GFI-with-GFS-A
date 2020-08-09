using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "@string/Activity_FairyMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FairyDBMainActivity : DBMainActivity
    {
        delegate void DownloadProgress();

        private List<Fairy> rootList = new List<Fairy>();
        private List<Fairy> subList = new List<Fairy>();
        private List<int> downloadList = new List<int>();

        int[] typeFilters = { Resource.Id.FairyFilterTypeCombat, Resource.Id.FairyFilterTypeStrategy };
        int[] productTimeFilters = { Resource.Id.FairyFilterProductHour, Resource.Id.FairyFilterProductMinute };

        private bool[] hasApplyFilter = { false, false };
        private int[] filterProductTime = { 0, 0 };
        private bool[] filterType = { false, false };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                ListItem = new ListProcess(ListFairy);

                adapter = new FairyListAdapter(subList, this);
                (adapter as FairyListAdapter).ItemClick += Adapter_ItemClick;

                recyclerView.SetAdapter(adapter);

                SupportActionBar.SetTitle(Resource.String.FairyDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                InitProcess();

                _ = ListFairy();

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

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.DBMainFilter:
                    InitFilterBox();
                    break;
                case Resource.Id.DBMainSort:
                    InitSortBox();
                    break;
                case Resource.Id.RefreshCropImageCache:
                    downloadList.Clear();

                    string filePath = "";

                    for (int i = 0; i < rootList.Count; ++i)
                    {
                        filePath = Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop", $"{rootList[i].DicNumber}.gfdcache");

                        if (!File.Exists(filePath))
                        {
                            downloadList.Add(rootList[i].DicNumber);
                        }
                    }

                    downloadList.TrimExcess();
                    ShowDownloadCheckMessage(downloadList,
                        Path.Combine(ETC.server, "Data", "Images", "Fairy", "Normal_Crop"),
                        Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop"));
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
                    ShowDownloadCheckMessage(downloadList,
                        Path.Combine(ETC.server, "Data", "Images", "Fairy", "Normal_Crop"),
                        Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop"));
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

                _ = ListFairy();
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

                _ = ListFairy();
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

                _ = ListFairy();
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

                _ = ListFairy();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async Task ListFairy()
        {
            subList.Clear();

            string searchText = searchViewText.ToUpper();

            int[] pTime = { filterProductTime[0], filterProductTime[1] };

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

                await Task.Delay(100);

                RefreshAdapter();
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