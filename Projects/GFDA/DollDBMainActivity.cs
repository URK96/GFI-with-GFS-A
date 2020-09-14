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
    [Activity(Label = "@string/Activity_DollMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBMainActivity : DBMainActivity
    {
        delegate void DownloadProgress();

        private List<Doll> rootList = new List<Doll>();
        private List<Doll> subList = new List<Doll>();
        private List<int> downloadList = new List<int>();

        readonly int[] gradeFilters = { Resource.Id.DollFilterGrade2, Resource.Id.DollFilterGrade3, Resource.Id.DollFilterGrade4, Resource.Id.DollFilterGrade5, Resource.Id.DollFilterGradeExtra };
        readonly int[] typeFilters = { Resource.Id.DollFilterTypeHG, Resource.Id.DollFilterTypeSMG, Resource.Id.DollFilterTypeAR, Resource.Id.DollFilterTypeRF, Resource.Id.DollFilterTypeMG, Resource.Id.DollFilterTypeSG};
        readonly int[] productTimeFilters = { Resource.Id.DollFilterProductHour, Resource.Id.DollFilterProductMinute, Resource.Id.DollFilterProductNearRange };
        readonly int modFilter = Resource.Id.DollFilterOnlyMod;

        private bool[] hasApplyFilter = { false, false, false, false };
        private int[] filterProductTime = { 0, 0, 0 };
        private bool[] filterGrade = { false, false, false, false, false };
        private bool[] filterType = { false, false, false, false, false, false };
        private bool filterMod = false;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                ListItem = new ListProcess(ListDoll);

                adapter = new DollListAdapter(subList, this);
                (adapter as DollListAdapter).ItemClick += Adapter_ItemClick;

                recyclerView.SetAdapter(adapter);

                SupportActionBar.SetTitle(Resource.String.DollDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                InitProcess();

                _ = ListDoll();

                /*if ((ETC.locale.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true)))
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

                    foreach (DataRow dr in ETC.dollList.Rows)
                    {
                        downloadList.Add((int)dr["DicNumber"]);
                    }

                    downloadList.TrimExcess();
                    ShowDownloadCheckMessage(downloadList,
                        Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal_Crop"),
                        Path.Combine(ETC.cachePath, "Doll", "Normal_Crop"));
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
                        Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal_Crop"), 
                        Path.Combine(ETC.cachePath, "Doll", "Normal_Crop"));
                }
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.dollList.Rows)
                {
                    rootList.Add(new Doll(dr));
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
                var doll = rootList[i];
                string filePath = Path.Combine(ETC.cachePath, "Doll", "Normal_Crop", $"{doll.DicNumber}.gfdcache");

                if (!File.Exists(filePath))
                {
                    downloadList.Add(doll.DicNumber);
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
                Resources.GetString(Resource.String.Sort_SortMethod_HP),
                Resources.GetString(Resource.String.Sort_SortMethod_FR),
                Resources.GetString(Resource.String.Sort_SortMethod_EV),
                Resources.GetString(Resource.String.Sort_SortMethod_AC),
                Resources.GetString(Resource.String.Sort_SortMethod_AS),
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

                _ = ListDoll();
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

                _ = ListDoll();
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
                View v = LayoutInflater.Inflate(Resource.Layout.DollFilterLayout, null);

                v.FindViewById<NumberPicker>(Resource.Id.DollFilterProductHour).MaxValue = 12;
                v.FindViewById<NumberPicker>(Resource.Id.DollFilterProductMinute).MaxValue = 59;
                v.FindViewById<NumberPicker>(Resource.Id.DollFilterProductNearRange).MaxValue = 20;

                for (int i = 0; i < gradeFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(gradeFilters[i]).Checked = filterGrade[i];
                }
                for (int i = 0; i < typeFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(typeFilters[i]).Checked = filterType[i];
                }
                for (int i = 0; i < productTimeFilters.Length; ++i)
                {
                    v.FindViewById<NumberPicker>(productTimeFilters[i]).Value = filterProductTime[i];
                }
                v.FindViewById<CheckBox>(modFilter).Checked = filterMod;

                var FilterBox = new AndroidX.AppCompat.App.AlertDialog.Builder(this, ETC.dialogBGVertical);
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
                for (int i = 0; i < gradeFilters.Length; ++i)
                {
                    filterGrade[i] = view.FindViewById<CheckBox>(gradeFilters[i]).Checked;
                }
                for (int i = 0; i < typeFilters.Length; ++i)
                {
                    filterType[i] = view.FindViewById<CheckBox>(typeFilters[i]).Checked;
                }
                for (int i = 0; i < productTimeFilters.Length; ++i)
                {
                    filterProductTime[i] = view.FindViewById<NumberPicker>(productTimeFilters[i]).Value;
                }
                filterMod = view.FindViewById<CheckBox>(modFilter).Checked;

                CheckApplyFilter();

                _ = ListDoll();
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
            for (int i = 0; i < typeFilters.Length; ++i)
            {
                hasApplyFilter[2] = filterType[i];

                if (hasApplyFilter[2])
                {
                    break;
                }
            }
            hasApplyFilter[3] = filterMod;
        }

        private void ResetFilter()
        {
            try
            {
                for (int i = 0; i < gradeFilters.Length; ++i)
                {
                    filterGrade[i] = false;
                }
                for (int i = 0; i < typeFilters.Length; ++i)
                {
                    filterType[i] = false;
                }
                for (int i = 0; i < productTimeFilters.Length; ++i)
                {
                    filterProductTime[i] = 0;
                }
                filterMod = false;

                for (int i = 0; i < hasApplyFilter.Length; ++i)
                {
                    hasApplyFilter[i] = false;
                }

                _ = ListDoll();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async Task ListDoll()
        {
            subList.Clear();

            string searchText = searchViewText.ToUpper();

            int[] pTime = { filterProductTime[0], filterProductTime[1] };
            int pRange = filterProductTime[2];

            try
            {
                for (int i = 0; i < rootList.Count; ++i)
                {
                    Doll doll = rootList[i];

                    if ((pTime[0] + pTime[1]) != 0)
                    {
                        if (!CheckDollByProductTime(pTime, pRange, doll.ProductTime))
                        {
                            continue;
                        }
                    }

                    if (CheckFilter(doll))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        if (!doll.Name.ToUpper().Contains(searchText))
                        {
                            continue;
                        }
                    }

                    subList.Add(doll);
                }

                subList.Sort(SortDoll);

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

            var dollInfo = new Intent(this, typeof(DollDBDetailActivity));
            dollInfo.PutExtra("DicNum", subList[position].DicNumber);
            StartActivity(dollInfo) ;
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private bool CheckDollByProductTime(int[] pTime, int range, int dTime)
        {
            int pTimeM = (pTime[0] * 60) + pTime[1];
            int minTime = pTimeM - range;
            int maxTime = pTimeM + range;

            return ((minTime <= dTime) && (dTime <= maxTime));
        }

        private int SortDoll(Doll x, Doll y)
        {
            if (sortOrder == SortOrder.Descending)
            {
                Doll temp = x;
                x = y;
                y = temp;
            }

            switch (sortType)
            {
                default:
                case SortType.Name:
                    return x.Name.CompareTo(y.Name);
                case SortType.Number:
                    return x.DicNumber.CompareTo(y.DicNumber);
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
                case SortType.HP:
                case SortType.FR:
                case SortType.EV:
                case SortType.AC:
                case SortType.AS:
                    if (x.Type != y.Type)
                    {
                        return x.Type.CompareTo(y.Type);
                    }
                    else
                    {
                        string[] abilities = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed" };
                        DollAbilitySet das = new DollAbilitySet(x.Type);

                        int xGrowRatio = int.Parse(x.Abilities["Grow"].Split(';')[0]);
                        int xBasicRatio = int.Parse(x.Abilities[abilities[(int)sortType - 3]].Split(';')[0]);
                        int xValue = das.CalcAbility(abilities[(int)sortType - 3], xBasicRatio, xGrowRatio, 100, 90, false);

                        int yGrowRatio = int.Parse(y.Abilities["Grow"].Split(';')[0]);
                        int yBasicRatio = int.Parse(y.Abilities[abilities[(int)sortType - 3]].Split(';')[0]);
                        int yValue = das.CalcAbility(abilities[(int)sortType - 3], yBasicRatio, yGrowRatio, 100, 90, false);

                        return xValue.CompareTo(yValue);
                    }
            }
        }

        private bool CheckFilter(Doll doll)
        {
            if (hasApplyFilter[1])
            {
                switch (doll.Grade)
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
                switch (doll.Type)
                {
                    case "HG":
                        if (!filterType[0])
                        {
                            return true;
                        }
                        break;
                    case "SMG":
                        if (!filterType[1])
                        {
                            return true;
                        }
                        break;
                    case "AR":
                        if (!filterType[2])
                        {
                            return true;
                        }
                        break;
                    case "RF":
                        if (!filterType[3])
                        {
                            return true;
                        }
                        break;
                    case "MG":
                        if (!filterType[4])
                        {
                            return true;
                        }
                        break;
                    case "SG":
                        if (!filterType[5])
                        {
                            return true;
                        }
                        break;
                }
            }

            if (!doll.HasMod && filterMod)
            {
                return true;
            }

            return false;
        }
    }

    class DollListViewHolder : RecyclerView.ViewHolder
    {
        public TextView DicNumber { get; private set; }
        public TextView Type { get; private set; }
        public ImageView Grade { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView Name { get; private set; }
        public TextView ProductTime { get; private set; }

        public DollListViewHolder(View view, Action<int> listener) : base(view)
        {
            DicNumber = view.FindViewById<TextView>(Resource.Id.DollListNumber);
            Type = view.FindViewById<TextView>(Resource.Id.DollListType);
            Grade = view.FindViewById<ImageView>(Resource.Id.DollListGrade);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.DollListSmallImage);
            Name = view.FindViewById<TextView>(Resource.Id.DollListName);
            ProductTime = view.FindViewById<TextView>(Resource.Id.DollListProductTime);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    class DollListAdapter : RecyclerView.Adapter
    {
        List<Doll> items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public DollListAdapter(List<Doll> items, Activity context)
        {
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.DollListLayout, parent, false);
            var vh = new DollListViewHolder(view, OnClick);

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
            return !(ItemClick == null);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as DollListViewHolder;
            var item = items[position];

            try
            {
                vh.DicNumber.Text = $"No. {item.DicNumber}";

                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;

                    if (File.Exists(Path.Combine(ETC.cachePath, "Doll", "Normal_Crop", $"{item.DicNumber}.gfdcache")))
                    {
                        vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "Doll", "Normal_Crop", $"{item.DicNumber}.gfdcache")));
                    }
                }
                else
                {
                    vh.SmallImage.Visibility = ViewStates.Gone;
                }

                vh.Grade.SetImageResource(item.GradeIconId);
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