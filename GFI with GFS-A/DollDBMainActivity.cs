using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "인형 목록", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBMainActivity : AppCompatActivity
    {
        delegate void DownloadProgress();

        private List<DollListBasicInfo> mDollList = new List<DollListBasicInfo>();

        int[] GradeFilters = { Resource.Id.DollFilterGrade2, Resource.Id.DollFilterGrade3, Resource.Id.DollFilterGrade4, Resource.Id.DollFilterGrade5, Resource.Id.DollFilterGradeExtra };
        int[] TypeFilters = { Resource.Id.DollFilterTypeHG, Resource.Id.DollFilterTypeSMG, Resource.Id.DollFilterTypeAR, Resource.Id.DollFilterTypeRF, Resource.Id.DollFilterTypeMG, Resource.Id.DollFilterTypeSG};
        int[] ProductTimeFilters = { Resource.Id.DollFilterProductHour, Resource.Id.DollFilterProductMinute, Resource.Id.DollFilterProductNearRange };
        int[] ModFilters = { Resource.Id.DollFilterHasModYes, Resource.Id.DollFilterHasModNo };

        int p_now = 0;
        int p_total = 0;

        private bool[] Filter_Grade = { true, true, true, true, true };
        private bool[] Filter_Type = { true, true, true, true, true, true };
        private int[] Filter_ProductTime = { 0, 0, 0 };
        private bool[] Filter_Mod = { true, true };

        private enum LineUp { Name, Number, ProductTime }
        private LineUp LineUpStyle = LineUp.Name;

        private ListView mDollListView = null;
        private CoordinatorLayout SnackbarLayout = null;

        private EditText SearchText = null;

        private Dialog dialog = null;
        private ProgressBar totalProgressBar = null;
        private ProgressBar nowProgressBar = null;
        private TextView totalProgress = null;
        private TextView nowProgress = null;
        private FloatingActionButton filter_fab = null;
        private FloatingActionButton array_fab = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.DollDBListLayout);

                SetTitle(Resource.String.DollDBMainActivity_Title);

                mDollListView = FindViewById<ListView>(Resource.Id.DollDBListView);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBSnackbarLayout);

                SearchText = FindViewById<EditText>(Resource.Id.DollSearchText);

                InitializeView();

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<LinearLayout>(Resource.Id.DollSearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.DollSearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.DollSearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                    mDollListView.Divider = new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Gray);
                }

                InitProcess();

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);

                mDollListView.FastScrollEnabled = true;
                mDollListView.FastScrollAlwaysVisible = false;
                mDollListView.ItemClick += MDollListView_ItemClick;
                mDollListView.ItemLongClick += MDollListView_ItemLongClick;
                mDollListView.ScrollStateChanged += MDollListView_ScrollStateChanged;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void MDollListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            try
            {
                switch (e.ScrollState)
                {
                    case ScrollState.TouchScroll:
                        filter_fab.Hide();
                        array_fab.Hide();
                        break;
                    case ScrollState.Idle:
                        filter_fab.Show();
                        array_fab.Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeStatusError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void MDollListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string DollName = (string)(mDollList[e.Position].DollDR)["Name"];
            var DollInfo = new Intent(this, typeof(DollDBDetailActivity));
            DollInfo.PutExtra("Keyword", DollName);
            StartActivity(DollInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }


        private void MDollListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            DataRow dr = mDollList[e.Position].DollDR;

            string[] Buffs = ((string)dr["Effect"]).Split(';')[0].Split(',');
            StringBuilder buff_sb = new StringBuilder();

            for (int i = 0; i < Buffs.Length; ++i)
            {
                string temp = "";
                
                switch (Buffs[i])
                {
                    case "FR":
                        temp = string.Format("{0}↑", Resources.GetString(Resource.String.Common_FR));
                        break;
                    case "EV":
                        temp = string.Format("{0}↑", Resources.GetString(Resource.String.Common_EV));
                        break;
                    case "AC":
                        temp = string.Format("{0}↑", Resources.GetString(Resource.String.Common_AC));
                        break;
                    case "AS":
                        temp = string.Format("{0}↑", Resources.GetString(Resource.String.Common_AS));
                        break;
                    case "CR":
                        temp = string.Format("{0}↑", Resources.GetString(Resource.String.Common_CR));
                        break;
                    case "CL":
                        temp = string.Format("{0}↓", Resources.GetString(Resource.String.Common_CL));
                        break;
                    case "AM":
                        temp = string.Format("{0}↑", Resources.GetString(Resource.String.Common_AM));
                        break;
                }

                buff_sb.Append(temp);
                if (i < (Buffs.Length - 1)) buff_sb.Append(" | ");
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0} : {1}\n\n", Resources.GetString(Resource.String.Common_NickName), (string)dr["NickName"]);
            sb.AppendFormat("{0} : {1}\n\n", Resources.GetString(Resource.String.Common_Buff), buff_sb.ToString());
            sb.AppendFormat("{0} : {1}", Resources.GetString(Resource.String.Common_Skill),(string)dr["Skill"]);

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this);
            ad.SetTitle("<No. " + (int)dr["DicNumber"] + "> " + (string)dr["Name"]);
            ad.SetCancelable(true);
            ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });
            ad.SetMessage(sb.ToString());
            ad.Show();
        }

        private void InitializeView()
        {
            filter_fab = FindViewById<FloatingActionButton>(Resource.Id.DollFilterFAB);
            if (filter_fab.HasOnClickListeners == false) filter_fab.Click += Filter_Fab_Click;

            array_fab = FindViewById<FloatingActionButton>(Resource.Id.DollArrayFAB);
            if (array_fab.HasOnClickListeners == false) array_fab.Click += Array_fab_Click;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.DollSearchResetButton);
            if (SearchResetButton.HasOnClickListeners == false) SearchResetButton.Click += SearchResetButton_Click;

            SearchText.TextChanged += SearchText_TextChanged;
        }

        private void Array_fab_Click(object sender, EventArgs e)
        {
            try
            {
                switch (LineUpStyle)
                {
                    case LineUp.Name:
                        LineUpStyle = LineUp.Number;
                        array_fab.SetImageResource(Resource.Drawable.LineUp_DicNum_Icon);
                        break;
                    case LineUp.Number:
                        LineUpStyle = LineUp.ProductTime;
                        array_fab.SetImageResource(Resource.Drawable.LineUp_ProductTime_Icon);
                        break;
                    case LineUp.ProductTime:
                    default:
                        LineUpStyle = LineUp.Name;
                        array_fab.SetImageResource(Resource.Drawable.LineUp_Name_Icon);
                        break;
                }

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.LineUp_Error, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void InitProcess()
        {
            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
            {
                if (CheckDollCropImage() == false) ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(DollCropImageDownloadProcess));
            }
        }

        private bool CheckDollCropImage()
        {
            for (int i = 0; i < ETC.DollList.Rows.Count; ++i)
            {
                DataRow dr = ETC.DollList.Rows[i];
                int DollNum = (int)dr["DicNumber"];
                string FilePath = System.IO.Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", DollNum + ".gfdcache");
                if (System.IO.File.Exists(FilePath) == false)
                {
                    //Toast.MakeText(this, DollNum.ToString(), ToastLength.Short).Show();
                    return false;
                }
            }

            return true;
        }

        private void ShowDownloadCheckMessage(int title, int message, DownloadProgress method)
        {
            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG);
            ad.SetTitle(title);
            ad.SetMessage(message);
            ad.SetCancelable(true);
            ad.SetPositiveButton(Resource.String.AlertDialog_Download, delegate { method(); });
            ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

            ad.Show();
        }

        private async void DollCropImageDownloadProcess()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Download);
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

                p_now = 0;
                p_total = 0;
                p_total = ETC.DollList.Rows.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    for (int i = 0; i < p_total; ++i)
                    {
                        DataRow dr = ETC.DollList.Rows[i];
                        int num = (int)dr["DicNumber"];
                        string filename = num.ToString();
                        string url = System.IO.Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", filename + ".png");
                        string target = System.IO.Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", filename + ".gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageFail, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
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
            p_now += 1;

            totalProgressBar.Progress = Convert.ToInt32((p_now / Convert.ToDouble(p_total)) * 100);
            totalProgress.Text = string.Format("{0}%", totalProgressBar.Progress);
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            nowProgressBar.Progress = e.ProgressPercentage;
            nowProgress.Text = string.Format("{0}%", e.ProgressPercentage);
        }

        private void SearchText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
        }

        private void SearchResetButton_Click(object sender, EventArgs e)
        {
            SearchText.Text = "";
        }

        private void Filter_Fab_Click(object sender, EventArgs e)
        {
            InitFilterBox();   
        }

        private void InitFilterBox()
        {
            var inflater = LayoutInflater;

            try
            {
                View v = inflater.Inflate(Resource.Layout.DollFilterLayout, null);

                v.FindViewById<NumberPicker>(Resource.Id.DollFilterProductHour).MaxValue = 12;
                v.FindViewById<NumberPicker>(Resource.Id.DollFilterProductMinute).MaxValue = 59;
                v.FindViewById<NumberPicker>(Resource.Id.DollFilterProductNearRange).MaxValue = 20;

                for (int i = 0; i < GradeFilters.Length; ++i) v.FindViewById<CheckBox>(GradeFilters[i]).Checked = Filter_Grade[i];
                for (int i = 0; i < TypeFilters.Length; ++i) v.FindViewById<CheckBox>(TypeFilters[i]).Checked = Filter_Type[i];
                for (int i = 0; i < ProductTimeFilters.Length; ++i) v.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value = Filter_ProductTime[i];
                for (int i = 0; i < ModFilters.Length; ++i) v.FindViewById<CheckBox>(ModFilters[i]).Checked = Filter_Mod[i];

                Android.Support.V7.App.AlertDialog.Builder FilterBox = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Vertical);
                FilterBox.SetTitle(Resource.String.DBList_FilterBoxTitle);
                FilterBox.SetView(v);
                FilterBox.SetPositiveButton(Resource.String.AlertDialog_Set, delegate { ApplyFilter(v); });
                FilterBox.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });
                FilterBox.SetNeutralButton(Resource.String.AlertDialog_Reset, delegate { ResetFilter(v); });

                FilterBox.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FilterBox_InitError, Snackbar.LengthLong);
            }
        }

        private void ApplyFilter(View view)
        {
            try
            {
                for (int i = 0; i < GradeFilters.Length; ++i) Filter_Grade[i] = view.FindViewById<CheckBox>(GradeFilters[i]).Checked;
                for (int i = 0; i < TypeFilters.Length; ++i) Filter_Type[i] = view.FindViewById<CheckBox>(TypeFilters[i]).Checked;
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = view.FindViewById<NumberPicker>(ProductTimeFilters[i]).Value;
                for (int i = 0; i < ModFilters.Length; ++i) Filter_Mod[i] = view.FindViewById<CheckBox>(ModFilters[i]).Checked;

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxApplyFail, Snackbar.LengthLong);
            }
        }

        private void ResetFilter(View view)
        {
            try
            {
                for (int i = 0; i < GradeFilters.Length; ++i) Filter_Grade[i] = true;
                for (int i = 0; i < TypeFilters.Length; ++i) Filter_Type[i] = true;
                for (int i = 0; i < ProductTimeFilters.Length; ++i) Filter_ProductTime[i] = 0;
                for (int i = 0; i < ModFilters.Length; ++i) Filter_Mod[i] = true;

                ListDoll(SearchText.Text, new int[] { Filter_ProductTime[0], Filter_ProductTime[1] }, Filter_ProductTime[2]);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListDoll(string searchText, int[] p_time, int p_range)
        {
            //ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_Listing, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);

            mDollList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < ETC.DollList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.DollList.Rows[i];

                    if ((p_time[0] + p_time[1]) != 0)
                    {
                        if (CheckDollByProductTime(p_time, p_range, (int)dr["ProductTime"]) == false) continue;
                    }

                    if (CheckFilter(dr) == true) continue;
                    if (searchText != "")
                    {
                        string name = ((string)dr["Name"]).ToUpper();
                        if (name.Contains(searchText) == false) continue;
                    }

                    DollListBasicInfo info = new DollListBasicInfo()
                    {
                        Id = i,
                        DollDR = dr
                    };
                    mDollList.Add(info);
                }

                mDollList.Sort(SortDoll);

                var adapter = new DollListAdapter(this, mDollList);

                await Task.Delay(100);

                RunOnUiThread(() => { mDollListView.Adapter = adapter; });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private bool CheckDollByProductTime(int[] p_time, int range, int d_time)
        {
            int p_time_minute = (p_time[0] * 60) + p_time[1];

            for (int i = (p_time_minute - range); i <= (p_time_minute + range); ++i)
            {
                if (d_time == i) return true;
            }

            return false;
        }

        private int SortDoll(DollListBasicInfo x, DollListBasicInfo y)
        {
            switch (LineUpStyle)
            {
                case LineUp.Number:
                    int x_num = (int)x.DollDR["DicNumber"];
                    int y_num = (int)y.DollDR["DicNumber"];
                    return x_num.CompareTo(y_num);
                case LineUp.ProductTime:
                    int x_time = (int)x.DollDR["ProductTime"];
                    int y_time = (int)y.DollDR["ProductTime"];
                    if ((x_time == 0) && (y_time !=0)) return 1;
                    else if ((y_time == 0) && (x_time != 0)) return -1;
                    else if (x_time == y_time)
                    {
                        string x_name_t = (string)x.DollDR["Name"];
                        string y_name_t = (string)y.DollDR["Name"];
                        return x_name_t.CompareTo(y_name_t);
                    }
                    else return x_time.CompareTo(y_time);
                case LineUp.Name:
                default:
                    string x_name = (string)x.DollDR["Name"];
                    string y_name = (string)y.DollDR["Name"];
                    return x_name.CompareTo(y_name);
            }
        }

        private bool CheckFilter(DataRow dr)
        {
            int grade = (int)dr["Grade"];
            string type = (string)dr["Type"];
            bool HasMOD = (bool)dr["HasMod"];

            switch (grade)
            {
                case 2:
                    if (Filter_Grade[0] == false) return true;
                    break;
                case 3:
                    if (Filter_Grade[1] == false) return true;
                    break;
                case 4:
                    if (Filter_Grade[2] == false) return true;
                    break;
                case 5:
                    if (Filter_Grade[3] == false) return true;
                    break;
                case 0:
                    if (Filter_Grade[4] == false) return true;
                    break;
            }

            switch (type)
            {
                case "HG":
                    if (Filter_Type[0] == false) return true;
                    break;
                case "SMG":
                    if (Filter_Type[1] == false) return true;
                    break;
                case "AR":
                    if (Filter_Type[2] == false) return true;
                    break;
                case "RF":
                    if (Filter_Type[3] == false) return true;
                    break;
                case "MG":
                    if (Filter_Type[4] == false) return true;
                    break;
                case "SG":
                    if (Filter_Type[5] == false) return true;
                    break;
            }

            switch (HasMOD)
            {
                case true:
                    if (Filter_Mod[0] == false) return true;
                    break;
                case false:
                    if (Filter_Mod[1] == false) return true;
                    break;
            }

            return false;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            try
            {
                MenuInflater.Inflate(Resource.Menu.DollDBMenu, menu);

                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true) menu.FindItem(Resource.Id.RefreshDollCropImageCache).SetVisible(true);
                else menu.FindItem(Resource.Id.RefreshDollCropImageCache).SetVisible(false);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_LoadFail, ToastLength.Short).Show();
            }

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.RefreshDollCropImageCache:
                    ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(DollCropImageDownloadProcess));
                    break;
            }

            return true;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }

    class DollListBasicInfo
    {
        public int Id { set; get; }
        public DataRow DollDR { set; get; }
    }

    class DollListAdapter : BaseAdapter<DollListBasicInfo>
    {
        List<DollListBasicInfo> mitems;
        Activity mcontext;
        int count = 0;

        public DollListAdapter(Activity context, List<DollListBasicInfo> items) : base()
        {
            mcontext = context;
            mitems = items;
        }

        public override DollListBasicInfo this[int position]
        {
            get { return mitems[position]; }
        }

        public override int Count
        {
            get { return mitems.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return mitems[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = mitems[position];
            var view = convertView;

            try
            {
                if (view == null) view = mcontext.LayoutInflater.Inflate(Resource.Layout.DollListLayout, null);

                count += 1;
                if (count == 50)
                {
                    GC.Collect(0, GCCollectionMode.Optimized, false, true);
                    count = 0;
                }

                int D_Num = (int)item.DollDR["DicNumber"];

                ImageView DollSmallImage = view.FindViewById<ImageView>(Resource.Id.DollListSmallImage);
                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    DollSmallImage.Visibility = ViewStates.Visible;
                    string FilePath = System.IO.Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", D_Num + ".gfdcache");
                    if (System.IO.File.Exists(FilePath) == true) DollSmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                }
                else DollSmallImage.Visibility = ViewStates.Gone;

                ImageView DollGradeIcon = view.FindViewById<ImageView>(Resource.Id.DollListGrade);
                int GradeIconId = 0;
                switch ((int)item.DollDR["Grade"])
                {
                    case 2:
                        GradeIconId = Resource.Drawable.Grade_2;
                        break;
                    case 3:
                        GradeIconId = Resource.Drawable.Grade_3;
                        break;
                    case 4:
                        GradeIconId = Resource.Drawable.Grade_4;
                        break;
                    case 5:
                        GradeIconId = Resource.Drawable.Grade_5;
                        break;
                    case 0:
                        GradeIconId = Resource.Drawable.Grade_0;
                        break;
                    default:
                        GradeIconId = Resource.Drawable.Grade_2;
                        break;
                }
                DollGradeIcon.SetImageResource(GradeIconId);

                TextView DollType = view.FindViewById<TextView>(Resource.Id.DollListType);
                DollType.Text = (string)item.DollDR["Type"];

                TextView DollName = view.FindViewById<TextView>(Resource.Id.DollListName);
                DollName.Text = (string)item.DollDR["Name"];

                TextView DollProductTime = view.FindViewById<TextView>(Resource.Id.DollListProductTime);
                DollProductTime.Text = ETC.CalcTime((int)item.DollDR["ProductTime"]);
            }
            catch (OutOfMemoryException)
            {
                GC.Collect(0, GCCollectionMode.Forced, false, true);
                GetView(position, convertView, parent);
            }
            catch (Exception ex)
            {
                ETC.LogError(mcontext, ex.ToString());
                Toast.MakeText(mcontext, "Error Create View", ToastLength.Short).Show();
            }

            return view;
        }
    }
}