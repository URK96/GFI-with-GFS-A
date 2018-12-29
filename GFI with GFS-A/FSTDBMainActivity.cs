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
    [Activity(Label = "화력소대 목록", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FSTDBMainActivity : AppCompatActivity
    {
        delegate void DownloadProgress();

        private List<FSTListBasicInfo> mFSTList = new List<FSTListBasicInfo>();

        int[] TypeFilters = { Resource.Id.FSTFilterTypeATW, Resource.Id.FSTFilterTypeAGL, Resource.Id.FSTFilterTypeMTR};

        int p_now = 0;
        int p_total = 0;

        private bool[] Filter_Type = { true, true, true, true, true, true };

        private enum LineUp { Name, Number }
        private LineUp LineUpStyle = LineUp.Name;

        private ListView mFSTListView = null;
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
                SetContentView(Resource.Layout.FSTDBListLayout);

                SetTitle(Resource.String.FSTDBMainActivity_Title);

                mFSTListView = FindViewById<ListView>(Resource.Id.FSTDBListView);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.FSTDBSnackbarLayout);

                SearchText = FindViewById<EditText>(Resource.Id.FSTSearchText);

                InitializeView();

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<LinearLayout>(Resource.Id.FSTSearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.FSTSearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.FSTSearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                    mFSTListView.Divider = new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Gray);
                }

                InitProcess();

                ListFST(SearchText.Text);

                mFSTListView.FastScrollEnabled = true;
                mFSTListView.FastScrollAlwaysVisible = false;
                mFSTListView.ItemClick += MFSTListView_ItemClick;
                mFSTListView.ItemLongClick += MFSTListView_ItemLongClick;
                mFSTListView.ScrollStateChanged += MFSTListView_ScrollStateChanged;

                if ((ETC.Language.Language == "ko") && (ETC.sharedPreferences.GetBoolean("Help_DBList", true) == true)) ETC.RunHelpActivity(this, "DBList");
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void MFSTListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
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

        private void MFSTListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string FSTName = (string)(mFSTList[e.Position].FSTDR)["Name"];
            var FSTInfo = new Intent(this, typeof(FSTDBDetailActivity));
            FSTInfo.PutExtra("Keyword", FSTName);
            StartActivity(FSTInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void MFSTListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            DataRow dr = mFSTList[e.Position].FSTDR;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0} : {1}\n\n", Resources.GetString(Resource.String.Common_NickName), (string)dr["NickName"]);

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this);
            ad.SetTitle((string)dr["Name"]);
            ad.SetCancelable(true);
            ad.SetPositiveButton(Resource.String.AlertDialog_Confirm, delegate { });
            ad.SetMessage(sb.ToString());
            ad.Show();
        }

        private void InitializeView()
        {
            filter_fab = FindViewById<FloatingActionButton>(Resource.Id.FSTFilterFAB);
            if (filter_fab.HasOnClickListeners == false) filter_fab.Click += Filter_Fab_Click;

            array_fab = FindViewById<FloatingActionButton>(Resource.Id.FSTArrayFAB);
            if (array_fab.HasOnClickListeners == false) array_fab.Click += Array_fab_Click;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.FSTSearchResetButton);
            if (SearchResetButton.HasOnClickListeners == false) SearchResetButton.Click += SearchResetButton_Click;

            SearchText.TextChanged += SearchText_TextChanged;
        }

        private void Array_fab_Click(object sender, EventArgs e)
        {
            try
            {
                switch (LineUpStyle)
                {
                    default:
                    case LineUp.Name:
                        LineUpStyle = LineUp.Number;
                        array_fab.SetImageResource(Resource.Drawable.LineUp_DicNum_Icon);
                        break;
                    case LineUp.Number:
                        LineUpStyle = LineUp.Name;
                        array_fab.SetImageResource(Resource.Drawable.LineUp_ProductTime_Icon);
                        break;
                }

                ListFST(SearchText.Text);
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
                if (CheckFSTCropImage() == false) ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(FSTCropImageDownloadProcess));
            }
        }

        private bool CheckFSTCropImage()
        {
            for (int i = 0; i < ETC.FSTList.Rows.Count; ++i)
            {
                DataRow dr = ETC.FSTList.Rows[i];
                string FileName = (string)dr["Name"];
                string FilePath = System.IO.Path.Combine(ETC.CachePath, "FST", "Normal_Crop", FileName + ".gfdcache");
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

        private async void FSTCropImageDownloadProcess()
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
                p_total = ETC.FSTList.Rows.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    for (int i = 0; i < p_total; ++i)
                    {
                        DataRow dr = ETC.FSTList.Rows[i];
                        string filename = (string)dr["Name"];
                        string url = System.IO.Path.Combine(ETC.Server, "Data", "Images", "FST", "Normal_Crop", filename + ".png");
                        string target = System.IO.Path.Combine(ETC.CachePath, "FST", "Normal_Crop", filename + ".gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListFST(SearchText.Text);
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
            ListFST(SearchText.Text);
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
                View v = inflater.Inflate(Resource.Layout.FSTFilterLayout, null);

                for (int i = 0; i < TypeFilters.Length; ++i) v.FindViewById<CheckBox>(TypeFilters[i]).Checked = Filter_Type[i];

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
                for (int i = 0; i < TypeFilters.Length; ++i) Filter_Type[i] = view.FindViewById<CheckBox>(TypeFilters[i]).Checked;

                ListFST(SearchText.Text);
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
                for (int i = 0; i < TypeFilters.Length; ++i) Filter_Type[i] = true;

                ListFST(SearchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListFST(string searchText)
        {
            //ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_Listing, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);

            mFSTList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < ETC.FSTList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.FSTList.Rows[i];

                    if (CheckFilter(dr) == true) continue;
                    if (searchText != "")
                    {
                        string name = ((string)dr["Name"]).ToUpper();
                        if (name.Contains(searchText) == false) continue;
                    }

                    FSTListBasicInfo info = new FSTListBasicInfo()
                    {
                        Id = i,
                        FSTDR = dr
                    };
                    mFSTList.Add(info);
                }

                mFSTList.Sort(SortFST);

                var adapter = new FSTListAdapter(this, mFSTList);

                await Task.Delay(100);

                RunOnUiThread(() => { mFSTListView.Adapter = adapter; });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private int SortFST(FSTListBasicInfo x, FSTListBasicInfo y)
        {
            switch (LineUpStyle)
            {
                case LineUp.Number:
                    int x_num = (int)x.FSTDR["DicNumber"];
                    int y_num = (int)y.FSTDR["DicNumber"];
                    return x_num.CompareTo(y_num);
                case LineUp.Name:
                default:
                    string x_name = (string)x.FSTDR["Name"];
                    string y_name = (string)y.FSTDR["Name"];
                    return x_name.CompareTo(y_name);
            }
        }

        private bool CheckFilter(DataRow dr)
        {
            string type = (string)dr["Type"];

            switch (type)
            {
                case "ATW":
                    if (Filter_Type[0] == false) return true;
                    break;
                case "AGL":
                    if (Filter_Type[1] == false) return true;
                    break;
                case "MTR":
                    if (Filter_Type[2] == false) return true;
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
                    ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(FSTCropImageDownloadProcess));
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

    class FSTListBasicInfo
    {
        public int Id { set; get; }
        public DataRow FSTDR { set; get; }
    }

    class FSTListAdapter : BaseAdapter<FSTListBasicInfo>
    {
        List<FSTListBasicInfo> mitems;
        Activity mcontext;
        int count = 0;

        public FSTListAdapter(Activity context, List<FSTListBasicInfo> items) : base()
        {
            mcontext = context;
            mitems = items;
        }

        public override FSTListBasicInfo this[int position]
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
                if (view == null) view = mcontext.LayoutInflater.Inflate(Resource.Layout.FSTListLayout, null);

                count += 1;
                if (count == 50)
                {
                    GC.Collect(0, GCCollectionMode.Optimized, false, true);
                    count = 0;
                }

                string FST_name = (string)item.FSTDR["Name"];

                ImageView FSTSmallImage = view.FindViewById<ImageView>(Resource.Id.FSTListSmallImage);
                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    //FSTSmallImage.Visibility = ViewStates.Visible;
                    string FilePath = System.IO.Path.Combine(ETC.CachePath, "FST", "Normal_Crop", FST_name + ".gfdcache");
                    //if (System.IO.File.Exists(FilePath) == true) FSTSmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                    if (System.IO.File.Exists(FilePath) == true)
                    {
                        Android.Graphics.Drawables.Drawable drawable = Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath);
                        drawable.Alpha = 40;
                        view.FindViewById<LinearLayout>(Resource.Id.FSTListMainLayout).Background = drawable;
                    }
                }
                else FSTSmallImage.Visibility = ViewStates.Gone;

                /*ImageView DollGradeIcon = view.FindViewById<ImageView>(Resource.Id.DollListGrade);
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
                DollGradeIcon.SetImageResource(GradeIconId);*/

                TextView FSTType = view.FindViewById<TextView>(Resource.Id.FSTListType);
                FSTType.Text = (string)item.FSTDR["Type"];

                TextView FSTName = view.FindViewById<TextView>(Resource.Id.FSTListName);
                FSTName.Text = FST_name;

                TextView FSTRealModel = view.FindViewById<TextView>(Resource.Id.FSTListRealModel);
                FSTRealModel.Text = (string)item.FSTDR["Model"];
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