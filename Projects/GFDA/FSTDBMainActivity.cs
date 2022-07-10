using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Snackbar;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFDA
{
    [Activity(Label = "@string/Activity_FSTMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FSTDBMainActivity : DBMainActivity
    {
        delegate Task DownloadProgress();

        private List<FST> rootList = new List<FST>();
        private List<FST> subList = new List<FST>();
        private List<string> downloadList = new List<string>();

        int[] typeFilters = { Resource.Id.FSTFilterTypeATW, Resource.Id.FSTFilterTypeAGL, Resource.Id.FSTFilterTypeMTR};

        private bool[] hasApplyFilter = { false };
        private bool[] filterType = { false, false, false };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                ListItem = new ListProcess(ListFST);

                adapter = new FSTListAdapter(subList, this);
                (adapter as FSTListAdapter).ItemClick += Adapter_ItemClick;

                recyclerView.SetAdapter(adapter);

                SupportActionBar.SetTitle(Resource.String.FSTDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                InitProcess();

                _ = ListFST();

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

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
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

                    foreach (DataRow dr in ETC.fstList.Rows)
                    {
                        downloadList.Add((string)dr["CodeName"]);
                    }

                    downloadList.TrimExcess();

                    ShowDownloadCheckMessage(downloadList,
                        Path.Combine(ETC.server, "Data", "Images", "FST", "Normal_Icon"),
                        Path.Combine(ETC.cachePath, "FST", "Normal_Icon"));
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void InitProcess()
        {
            CreateListObject();

            if (Preferences.Get("DBListImageShow", false))
            {
                if (CheckFSTCropImage())
                {
                    ShowDownloadCheckMessage(downloadList,
                        Path.Combine(ETC.server, "Data", "Images", "FST", "Normal_Icon"),
                        Path.Combine(ETC.cachePath, "FST", "Normal_Icon"));
                }
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.fstList.Rows)
                {
                    rootList.Add(new FST(dr, true));
                }

                rootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthShort);
            }
        }

        private bool CheckFSTCropImage()
        {
            downloadList.Clear();

            for (int i = 0; i < rootList.Count; ++i)
            {
                FST fst = rootList[i];
                string filePath = Path.Combine(ETC.cachePath, "FST", "Normal_Icon", $"{fst.CodeName}.gfdcache");

                if (!File.Exists(filePath))
                {
                    downloadList.Add(fst.CodeName);
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
                Resources.GetString(Resource.String.Sort_SortMethod_Number)
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

                var filterBox = new AndroidX.AppCompat.App.AlertDialog.Builder(this, ETC.dialogBGVertical);
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

                _ = ListFST();
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

                _ = ListFST();
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
                View v = inflater.Inflate(Resource.Layout.FSTFilterLayout, null);

                for (int i = 0; i < typeFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(typeFilters[i]).Checked = filterType[i];
                }

                var filterBox = new AndroidX.AppCompat.App.AlertDialog.Builder(this, ETC.dialogBGVertical);
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

                CheckApplyFilter();

                _ = ListFST();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void CheckApplyFilter()
        {
            for (int i = 0; i < typeFilters.Length; ++i)
            {
                hasApplyFilter[0] = filterType[i];

                if (hasApplyFilter[0])
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

                for (int i = 0; i < hasApplyFilter.Length; ++i)
                {
                    hasApplyFilter[i] = false;
                }

                _ = ListFST();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async Task ListFST()
        {
            subList.Clear();

            string searchText = searchViewText.ToUpper();

            try
            {
                for (int i = 0; i < rootList.Count; ++i)
                {
                    FST fst = rootList[i];

                    if (CheckFilter(fst))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        string name = fst.Name.ToUpper();

                        if (!name.Contains(searchText))
                        {
                            continue;
                        }
                    }

                    subList.Add(fst);
                }

                subList.Sort(SortFST);

                await Task.Delay(100);

                RefreshAdapter();
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
            var fstInfo = new Intent(this, typeof(FSTDBDetailActivity));
            fstInfo.PutExtra("Keyword", subList[position].CodeName);
            StartActivity(fstInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private int SortFST(FST x, FST y)
        {
            if (sortOrder == SortOrder.Descending)
            {
                FST temp = x;
                x = y;
                y = temp;
            }

            switch (sortType)
            {
                case SortType.Name:
                default:
                    return x.Name.CompareTo(y.Name);
                case SortType.Number:
                    return x.DicNumber.CompareTo(y.DicNumber); 
            }
        }

        private bool CheckFilter(FST fst)
        {
            if (hasApplyFilter[0])
            {
                switch (fst.Type)
                {
                    case "ATW":
                        if (!filterType[0])
                        {
                            return true;
                        }
                        break;
                    case "AGL":
                        if (!filterType[1])
                        {
                            return true;
                        }
                        break;
                    case "MTR":
                        if (!filterType[2])
                        {
                            return true;
                        }
                        break;
                }
            }

            return false;
        }
    }

    class FSTListViewHolder : RecyclerView.ViewHolder
    {
        public TextView Type { get; private set; }
        public ImageView TypeIcon { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView DicNumber { get; private set; }
        public TextView Name { get; private set; }
        public TextView RealModel { get; private set; }

        public FSTListViewHolder(View view, Action<int> listener) : base(view)
        {
            Type = view.FindViewById<TextView>(Resource.Id.FSTListType);
            TypeIcon = view.FindViewById<ImageView>(Resource.Id.FSTListTypeIcon);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.FSTListSmallImage);
            DicNumber = view.FindViewById<TextView>(Resource.Id.FSTListDicNumber);
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
                if (Preferences.Get("DBListImageShow", false))
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;
                    string filePath = Path.Combine(ETC.cachePath, "FST", "Normal_Icon", $"{item.CodeName}.gfdcache");

                    if (File.Exists(filePath))
                    {
                        vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(filePath));
                    }
                }
                else
                {
                    vh.SmallImage.Visibility = ViewStates.Gone;
                }

                vh.Type.Text = item.Type;
                vh.DicNumber.Text = $"No. {item.DicNumber}";
                vh.Name.Text = item.Name;
                vh.RealModel.Text = item.RealModel;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, context);
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }

}