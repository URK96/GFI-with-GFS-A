using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "@string/Activity_DollMainActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBMainActivity_Beta : BaseAppCompatActivity
    {
        delegate void DownloadProgress();

        private enum SortType { Name, Number, ProductTime, HP, FR, EV, AC, AS }
        private enum SortOrder { Ascending, Descending }
        private SortType sortType = SortType.Name;
        private SortOrder sortOrder = SortOrder.Ascending;

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
        private bool canRefresh = false;

        private string searchViewText = "";

        private Android.Support.V7.Widget.Toolbar toolbar;
        private Android.Support.V7.Widget.SearchView searchView;
        private RecyclerView mDollListView;
        private RecyclerView.LayoutManager mainLayoutManager;
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
                SetContentView(Resource.Layout.DollDBListLayout_Beta);

                canRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.bDollDBMainToolbar);
                searchView = FindViewById<Android.Support.V7.Widget.SearchView>(Resource.Id.bDollDBSearchView);
                searchView.QueryTextChange += (sender, e) =>
                {
                    searchViewText = e.NewText;
                    _ = ListDoll(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
                };
                mDollListView = FindViewById<RecyclerView>(Resource.Id.bDollDBRecyclerView);
                mainLayoutManager = new LinearLayoutManager(this);
                mDollListView.SetLayoutManager(mainLayoutManager);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.bDollDBSnackbarLayout);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetTitle(Resource.String.DollDBMainActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                InitProcess();

                _ = ListDoll(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2]);

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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.DollDBMenu, menu);

            var cacheItem = menu.FindItem(Resource.Id.RefreshDollCropImageCache);
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
                case Resource.Id.DollDBMainFilter:
                    InitFilterBox();
                    break;
                case Resource.Id.DollDBMainSort:
                    InitSortBox();
                    break;
                case Resource.Id.RefreshDollCropImageCache:
                    downloadList.Clear();

                    foreach (DataRow dr in ETC.dollList.Rows)
                    {
                        downloadList.Add((int)dr["DicNumber"]);
                    }

                    downloadList.TrimExcess();
                    ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(DollCropImageDownloadProcess));
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
                    ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(DollCropImageDownloadProcess));
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
                Doll doll = rootList[i];
                string filePath = Path.Combine(ETC.cachePath, "Doll", "Normal_Crop", $"{doll.DicNumber}.gfdcache");

                if (!File.Exists(filePath))
                {
                    downloadList.Add(doll.DicNumber);
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

        private async void DollCropImageDownloadProcess()
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
                        string url = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal_Crop", $"{downloadList[i]}.png");
                        string target = Path.Combine(ETC.cachePath, "Doll", "Normal_Crop", $"{downloadList[i]}.gfdcache");

                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                _ = ListDoll(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
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

                _ = ListDoll(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
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

                _ = ListDoll(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
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

                _ = ListDoll(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
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

                _ = ListDoll(new int[] { filterProductTime[0], filterProductTime[1] }, filterProductTime[2], searchViewText);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async Task ListDoll(int[] pTime, int pRange, string searchText = "")
        {
            subList.Clear();

            searchText = searchText.ToUpper();

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

                    if (searchText != "")
                    {
                        string name = doll.Name.ToUpper();

                        if (!name.Contains(searchText))
                        {
                            continue;
                        }
                    }

                    subList.Add(doll);
                }

                subList.Sort(SortDoll);

                var adapter = new DollListAdapter(subList, this);

                if (!adapter.HasOnItemClick())
                {
                    adapter.ItemClick += Adapter_ItemClick;
                }

                await Task.Delay(100);

                RunOnUiThread(() => { mDollListView.SetAdapter(adapter); });
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private async void Adapter_ItemClick(object sender, int position)
        {
            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.DollListMainLayout);
            ArrayAdapter adapter = sender as ArrayAdapter;
            View view = adapter.GetView(position, layout, null);

            await Task.Delay(100);
            var DollInfo = new Intent(this, typeof(DollDBDetailActivity));
            DollInfo.PutExtra("DicNum", subList[position].DicNumber);
            ActivityOptionsCompat options = ActivityOptionsCompat.MakeSceneTransitionAnimation(this, view, view.TransitionName);
            StartActivity(DollInfo, options.ToBundle()) ;
            //OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private bool CheckDollByProductTime(int[] pTime, int range, int dTime)
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
                        DollAbilitySet DAS = new DollAbilitySet(x.Type);

                        string[] xGrowRatio = x.Abilities["Grow"].Split(';');
                        string[] xBasicRatio = x.Abilities[abilities[(int)sortType - 3]].Split(';');
                        int xValue = DAS.CalcAbility(abilities[(int)sortType - 3], int.Parse(xBasicRatio[0]), int.Parse(xGrowRatio[0]), 100, 90, false);

                        string[] yGrowRatio = y.Abilities["Grow"].Split(';');
                        string[] yBasicRatio = y.Abilities[abilities[(int)sortType - 3]].Split(';');
                        int yValue = DAS.CalcAbility(abilities[(int)sortType - 3], int.Parse(yBasicRatio[0]), int.Parse(yGrowRatio[0]), 100, 90, false);

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
                        if (!filterType[0])
                        {
                            return true;
                        }
                        break;
                    case "AR":
                        if (!filterType[0])
                        {
                            return true;
                        }
                        break;
                    case "RF":
                        if (!filterType[0])
                        {
                            return true;
                        }
                        break;
                    case "MG":
                        if (!filterType[0])
                        {
                            return true;
                        }
                        break;
                    case "SG":
                        if (!filterType[0])
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

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }
}