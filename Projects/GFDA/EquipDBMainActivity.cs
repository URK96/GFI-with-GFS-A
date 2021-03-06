﻿using Android.App;
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
    [Activity(Label = "@string/Activity_EquipMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EquipDBMainActivity : DBMainActivity
    {
        delegate void DownloadProgress();

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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                ListItem = new ListProcess(ListEquip);

                adapter = new EquipListAdapter(subList, this);
                (adapter as EquipListAdapter).ItemClick += Adapter_ItemClick;

                recyclerView.SetAdapter(adapter);

                SupportActionBar.SetTitle(Resource.String.EquipDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                InitProcess();

                _ = ListEquip();

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
                    ShowDownloadCheckMessage(downloadList,
                        Path.Combine(ETC.server, "Data", "Images", "Equipments"),
                        Path.Combine(ETC.cachePath, "Equip", "Normal"));
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void InitProcess()
        {
            CreateListObject();

            if (Preferences.Get("DBListImageShow", false))
            {
                if (CheckCropImage())
                {
                    ShowDownloadCheckMessage(downloadList,
                        Path.Combine(ETC.server, "Data", "Images", "Equipments"),
                        Path.Combine(ETC.cachePath, "Equip", "Normal"));
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

                _ = ListEquip();
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

                _ = ListEquip();
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

                _ = ListEquip();
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

                _ = ListEquip();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async Task ListEquip()
        {
            subList.Clear();

            string searchText = searchViewText.ToUpper();

            int[] pTime = { filterProductTime[0], filterProductTime[1] };
            int pRange = filterProductTime[2];

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
            var equipInfo = new Intent(this, typeof(EquipDBDetailActivity));
            equipInfo.PutExtra("Id", subList[position].Id);
            StartActivity(equipInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private static bool CheckEquipByProductTime(int[] pTime, int range, int dTime)
        {
            int pTimeM = (pTime[0] * 60) + pTime[1];
            int minTime = pTimeM - range;
            int maxTime = pTimeM + range;

            return ((minTime <= dTime) && (dTime <= maxTime));
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
                if (Preferences.Get("DBListImageShow", false))
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