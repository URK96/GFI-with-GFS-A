using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Snackbar;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GFDA
{
    [Activity(Label = "@string/Activity_CoalitionMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CoalitionDBMainActivity : DBMainActivity
    {
        delegate void DownloadProgress();

        private List<Coalition> rootList = new List<Coalition>();
        private List<Coalition> subList = new List<Coalition>();
        private List<int> downloadList = new List<int>();

        readonly int[] gradeFilters = { Resource.Id.CoalitionFilterGrade1, Resource.Id.CoalitionFilterGrade2, Resource.Id.CoalitionFilterGrade3 };
        readonly int[] typeFilters = { Resource.Id.CoalitionFilterTypeLArmor, Resource.Id.CoalitionFilterTypeHArmor, Resource.Id.CoalitionFilterTypeMachine, Resource.Id.CoalitionFilterTypeDoll, Resource.Id.CoalitionFilterTypeRemote, Resource.Id.CoalitionFilterTypeCloseIn };

        private bool[] hasApplyFilter = { false, false };
        private bool[] filterGrade = { false, false, false };
        private bool[] filterType = { false, false, false, false, false, false };
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Toolbar_Light);
                }

                ListItem = new ListProcess(ListCoalition);

                adapter = new CoalitionListAdapter(subList, filterType, this);
                (adapter as CoalitionListAdapter).ItemClick += Adapter_ItemClick;

                recyclerView.SetAdapter(adapter);

                SupportActionBar.SetTitle(Resource.String.CoalitionDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                InitProcess();

                _ = ListCoalition();

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

                    foreach (DataRow dr in ETC.coalitionList.Rows)
                    {
                        downloadList.Add((int)dr["DicNumber"]);
                    }

                    downloadList.TrimExcess();
                    ShowDownloadCheckMessage(downloadList,
                        Path.Combine(ETC.server, "Data", "Images", "Coalition", "Normal_Crop"),
                        Path.Combine(ETC.cachePath, "Coalition", "Normal_Crop"));
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
                    ShowDownloadCheckMessage(downloadList,
                        Path.Combine(ETC.server, "Data", "Images", "Coalition", "Normal_Crop"),
                        Path.Combine(ETC.cachePath, "Coalition", "Normal_Crop"));
                }
            }
        }

        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.coalitionList.Rows)
                {
                    rootList.Add(new Coalition(dr));
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
                var coalition = rootList[i];
                string filePath = Path.Combine(ETC.cachePath, "Coalition", "Normal_Crop", $"{coalition.DicNumber}.gfdcache");

                if (!File.Exists(filePath))
                {
                    downloadList.Add(coalition.DicNumber);
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
                var v = LayoutInflater.Inflate(Resource.Layout.CommonSorterLayout, null);

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

                _ = ListCoalition();
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

                _ = ListCoalition();
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
                View v = LayoutInflater.Inflate(Resource.Layout.CoalitionFilterLayout, null);

                for (int i = 0; i < gradeFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(gradeFilters[i]).Checked = filterGrade[i];
                }
                for (int i = 0; i < typeFilters.Length; ++i)
                {
                    v.FindViewById<CheckBox>(typeFilters[i]).Checked = filterType[i];
                }

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

                CheckApplyFilter();

                _ = ListCoalition();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void CheckApplyFilter()
        {
            for (int i = 0; i < gradeFilters.Length; ++i)
            {
                hasApplyFilter[0] = filterGrade[i];

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
                for (int i = 0; i < gradeFilters.Length; ++i)
                {
                    filterGrade[i] = false;
                }
                for (int i = 0; i < typeFilters.Length; ++i)
                {
                    filterType[i] = false;
                }

                for (int i = 0; i < hasApplyFilter.Length; ++i)
                {
                    hasApplyFilter[i] = false;
                }

                _ = ListCoalition();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async Task ListCoalition()
        {
            subList.Clear();

            string searchText = searchViewText.ToUpper();

            try
            {
                for (int i = 0; i < rootList.Count; ++i)
                {
                    var coalition = rootList[i];

                    if (CheckFilter(coalition))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        if (!coalition.Name.ToUpper().Contains(searchText))
                        {
                            continue;
                        }
                    }

                    subList.Add(coalition);
                }

                subList.Sort(SortCoalition);

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

            var coalitionInfo = new Intent(this, typeof(CoalitionDBDetailActivity));
            coalitionInfo.PutExtra("DicNum", subList[position].DicNumber);
            StartActivity(coalitionInfo) ;
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private int SortCoalition(Coalition x, Coalition y)
        {
            if (sortOrder == SortOrder.Descending)
            {
                Coalition temp = x;
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
            }
        }

        private bool CheckFilter(Coalition coalition)
        {
            if (hasApplyFilter[0])
            {
                switch (coalition.Grade)
                {
                    case 1:
                        if (!filterGrade[0])
                        {
                            return true;
                        }
                        break;
                    case 2:
                        if (!filterGrade[1])
                        {
                            return true;
                        }
                        break;
                    case 3:
                        if (!filterGrade[2])
                        {
                            return true;
                        }
                        break;
                }
            }

            if (hasApplyFilter[1])
            {
                string[] typeIndex = { "LightArmor", "HeavyArmor", "Machine", "Doll", "Remote", "CloseIn" };
                bool isPass = false;

                for (int i = 0; i < filterType.Length; ++i)
                {
                    if (filterType[i] && coalition.TypeCode.Contains(typeIndex[i]))
                    {
                        isPass = true;

                        break;
                    }
                }

                if (!isPass)
                {
                    return true;
                }
            }

            return false;
        }
    }

    class CoalitionListViewHolder : RecyclerView.ViewHolder
    {
        public TextView DicNumber { get; private set; }
        public TextView Affiliation { get; private set; }
        public TextView[] Types { get; private set; }
        public ImageView Grade { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView Name { get; private set; }

        public CoalitionListViewHolder(View view, Action<int> listener) : base(view)
        {
            Types = new TextView[3];

            DicNumber = view.FindViewById<TextView>(Resource.Id.CoalitionListDicNumber);
            Affiliation = view.FindViewById<TextView>(Resource.Id.CoalitionListAffiliation);
            Types[0] = view.FindViewById<TextView>(Resource.Id.CoalitionListType1);
            Types[1] = view.FindViewById<TextView>(Resource.Id.CoalitionListType2);
            Types[2] = view.FindViewById<TextView>(Resource.Id.CoalitionListType3);
            Grade = view.FindViewById<ImageView>(Resource.Id.CoalitionListGrade);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.CoalitionListSmallImage);
            Name = view.FindViewById<TextView>(Resource.Id.CoalitionListName);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    class CoalitionListAdapter : RecyclerView.Adapter
    {
        List<Coalition> items;
        Activity context;

        bool[] filterType;

        public event EventHandler<int> ItemClick;

        public CoalitionListAdapter(List<Coalition> items, bool[] filterType, Activity context)
        {
            this.items = items;
            this.filterType = filterType;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.CoalitionListLayout, parent, false);
            var vh = new CoalitionListViewHolder(view, OnClick);

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
            var vh = holder as CoalitionListViewHolder;
            var item = items[position];

            try
            {
                vh.Affiliation.Text = item.Affiliation;
                vh.DicNumber.Text = $"No. {item.DicNumber}";

                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    vh.SmallImage.Visibility = ViewStates.Visible;

                    if (File.Exists(Path.Combine(ETC.cachePath, "Coalition", "Normal_Crop", $"{item.DicNumber}.gfdcache")))
                    {
                        vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(Path.Combine(ETC.cachePath, "Coalition", "Normal_Crop", $"{item.DicNumber}.gfdcache")));
                    }
                }
                else
                {
                    vh.SmallImage.Visibility = ViewStates.Gone;
                }

                vh.Grade.SetImageResource(item.GradeIconId);
                vh.Name.Text = item.Name;

                for (int i = 0; i < item.TypeCode.Length; ++i)
                {
                    bool isSelected = false;
                    string s = "";

                    switch (item.TypeCode[i])
                    {
                        case "LightArmor":
                            isSelected = filterType[0];
                            s = context.Resources.GetString(Resource.String.Common_LightArmor);
                            break;
                        case "HeavyArmor":
                            isSelected = filterType[1];
                            s = context.Resources.GetString(Resource.String.Common_HeavyArmor);
                            break;
                        case "Machine":
                            isSelected = filterType[2];
                            s = context.Resources.GetString(Resource.String.Common_Machine);
                            break;
                        case "Doll":
                            isSelected = filterType[3];
                            s = context.Resources.GetString(Resource.String.Common_Doll);
                            break;
                        case "Remote":
                            isSelected = filterType[4];
                            s = context.Resources.GetString(Resource.String.Common_Remote);
                            break;
                        case "CloseIn":
                            isSelected = filterType[5];
                            s = context.Resources.GetString(Resource.String.Common_CloseIn);
                            break;
                    }

                    vh.Types[i].Text = s;
                    vh.Types[i].SetBackgroundResource(isSelected ? Resource.Drawable.typeTextViewBGSelect : Resource.Drawable.typeTextViewBG);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, context);
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}