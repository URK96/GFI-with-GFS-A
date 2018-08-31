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
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "철혈 목록", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class EnemyDBMainActivity : AppCompatActivity
    {
        delegate void DownloadProgress();

        private List<EnemyListBasicInfo> mEnemyList = new List<EnemyListBasicInfo>();

        int[] EnemyTypeFilters = { Resource.Id.EnemyFilterNormalEnemy, Resource.Id.EnemyFilterBossEnemy };

        int p_now = 0;
        int p_total = 0;

        private bool[] Filter_EnemyType = { true, true };
        private bool CanRefresh = false;

        private ListView mEnemyListView = null;
        private CoordinatorLayout SnackbarLayout = null;

        private EditText SearchText = null;

        private Dialog dialog = null;
        private ProgressBar totalProgressBar = null;
        private ProgressBar nowProgressBar = null;
        private TextView totalProgress = null;
        private TextView nowProgress = null;
        private FloatingActionButton refresh_fab = null;
        private FloatingActionButton filter_fab = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

                // Create your application here
                SetContentView(Resource.Layout.EnemyDBListLayout);

                SetTitle(Resource.String.EnemyDBMainActivity_Title);

                CanRefresh = ETC.sharedPreferences.GetBoolean("DBListImageShow", false);

                mEnemyListView = FindViewById<ListView>(Resource.Id.EnemyDBListView);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.EnemyDBSnackbarLayout);

                SearchText = FindViewById<EditText>(Resource.Id.EnemySearchText);

                InitializeView();

                if (ETC.UseLightTheme == true)
                {
                    FindViewById<LinearLayout>(Resource.Id.EnemySearchLayout).SetBackgroundColor(Android.Graphics.Color.LightGray);
                    FindViewById<ImageButton>(Resource.Id.EnemySearchResetButton).SetBackgroundResource(Resource.Drawable.SearchIcon_WhiteTheme);
                    FindViewById<View>(Resource.Id.EnemySearchSeperateBar).SetBackgroundColor(Android.Graphics.Color.DarkGreen);
                    mEnemyListView.Divider = new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Gray);
                }

                InitProcess();

                ListEnemy(SearchText.Text);

                mEnemyListView.FastScrollEnabled = true;
                mEnemyListView.FastScrollAlwaysVisible = false;
                mEnemyListView.ItemClick += MEnemyListView_ItemClick;
                //mEnemyListView.ItemLongClick += MDollListView_ItemLongClick;
                mEnemyListView.ScrollStateChanged += MEnemyListView_ScrollStateChanged;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void MEnemyListView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            try
            {
                switch (e.ScrollState)
                {
                    case ScrollState.TouchScroll:
                        if (CanRefresh == true) refresh_fab.Hide();
                        filter_fab.Hide();
                        break;
                    case ScrollState.Idle:
                        if (CanRefresh == true) refresh_fab.Show();
                        filter_fab.Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.FAB_ChangeStatusError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void MEnemyListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string EnemyCodeName = (string)(mEnemyList[e.Position].EnemyDR)["CodeName"];
            var EnemyInfo = new Intent(this, typeof(EnemyDBDetailActivity));
            EnemyInfo.PutExtra("Keyword", EnemyCodeName);
            StartActivity(EnemyInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private void InitializeView()
        {
            refresh_fab = FindViewById<FloatingActionButton>(Resource.Id.EnemyRefreshCacheFAB);
            if (CanRefresh == false) refresh_fab.Hide();
            else
            {
                if (refresh_fab.HasOnClickListeners == false) refresh_fab.Click += delegate { ShowDownloadCheckMessage(Resource.String.DBList_RefreshCropImageTitle, Resource.String.DBList_RefreshCropImageMessage, new DownloadProgress(EnemyCropImageDownloadProcess)); };
            }

            filter_fab = FindViewById<FloatingActionButton>(Resource.Id.EnemyFilterFAB);
            if (filter_fab.HasOnClickListeners == false) filter_fab.Click += Filter_Fab_Click;

            ImageButton SearchResetButton = FindViewById<ImageButton>(Resource.Id.EnemySearchResetButton);
            if (SearchResetButton.HasOnClickListeners == false) SearchResetButton.Click += SearchResetButton_Click;

            SearchText.TextChanged += SearchText_TextChanged;
        }

        private void InitProcess()
        {
            if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
            {
                if (CheckEnemyCropImage() == false) ShowDownloadCheckMessage(Resource.String.DBList_DownloadCropImageCheckTitle, Resource.String.DBList_DownloadCropImageCheckMessage, new DownloadProgress(EnemyCropImageDownloadProcess));
            }
        }

        private bool CheckEnemyCropImage()
        {
            for (int i = 0; i < ETC.EnemyList.Rows.Count; ++i)
            {
                DataRow dr = ETC.EnemyList.Rows[i];
                string EnemyCodeName = (string)dr["CodeName"];
                string FilePath = System.IO.Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", EnemyCodeName + ".gfdcache");
                if (System.IO.File.Exists(FilePath) == false)
                {
                    Toast.MakeText(this, EnemyCodeName.ToString(), ToastLength.Short).Show();
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

        private async void EnemyCropImageDownloadProcess()
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

                List<string> CodeName_List = new List<string>();

                for (int i = 0; i < ETC.EnemyList.Rows.Count; ++i)
                {
                    string code_name = ((string)ETC.EnemyList.Rows[i]["CodeName"]);

                    if (CodeName_List.Contains(code_name) == true) continue;

                    CodeName_List.Add(code_name);
                }

                CodeName_List.TrimExcess();

                p_total = 0;
                p_total = CodeName_List.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;

                using (TimeOutWebClient wc = new TimeOutWebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    for (int i = 0; i < p_total; ++i)
                    {
                        string filename = (string)CodeName_List[i];
                        string url = System.IO.Path.Combine(ETC.Server, "Data", "Images", "Enemy", "Normal_Crop", filename + ".png");
                        string target = System.IO.Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", filename + ".gfdcache");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_DownloadCropImageComplete, Snackbar.LengthLong, Android.Graphics.Color.DarkOliveGreen);

                await Task.Delay(500);

                ListEnemy(SearchText.Text);
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
            ListEnemy(SearchText.Text);
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
                View v = inflater.Inflate(Resource.Layout.EnemyFilterLayout, null);

                for (int i = 0; i < EnemyTypeFilters.Length; ++i) v.FindViewById<CheckBox>(EnemyTypeFilters[i]).Checked = Filter_EnemyType[i];

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
                for (int i = 0; i < EnemyTypeFilters.Length; ++i) Filter_EnemyType[i] = view.FindViewById<CheckBox>(EnemyTypeFilters[i]).Checked;

                ListEnemy(SearchText.Text);
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
                for (int i = 0; i < EnemyTypeFilters.Length; ++i) Filter_EnemyType[i] = true;

                ListEnemy(SearchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_FilterBoxResetFail, Snackbar.LengthLong);
            }
        }

        private async void ListEnemy(string searchText)
        {
            //ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_Listing, Snackbar.LengthShort, Android.Graphics.Color.DarkViolet);

            mEnemyList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < ETC.EnemyList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.EnemyList.Rows[i];

                    bool IsSkip = false;

                    foreach (EnemyListBasicInfo item in mEnemyList)
                    {
                        if ((string)item.EnemyDR["CodeName"] == (string)dr["CodeName"]) IsSkip = true;
                    }

                    if (IsSkip == true) continue;

                    if (CheckFilter(dr) == true) continue;
                    if (searchText != "")
                    {
                        string name = ((string)dr["Name"]).ToUpper();
                        if (name.Contains(searchText) == false) continue;
                    }

                    EnemyListBasicInfo info = new EnemyListBasicInfo()
                    {
                        Id = i,
                        EnemyDR = dr
                    };
                    mEnemyList.Add(info);
                }

                mEnemyList.Sort(SortEnemy);

                var adapter = new EnemyListAdapter(this, mEnemyList);

                await Task.Delay(100);

                RunOnUiThread(() => { mEnemyListView.Adapter = adapter; });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBList_ListingFail, Snackbar.LengthLong);
            }
        }

        private int SortEnemy(EnemyListBasicInfo x, EnemyListBasicInfo y)
        {
            string x_name = (string)x.EnemyDR["Name"];
            string y_name = (string)y.EnemyDR["Name"];
            return x_name.CompareTo(y_name);
        }

        private bool CheckFilter(DataRow dr)
        {
            bool enemy_type = (bool)dr["IsBoss"];

            switch (enemy_type)
            {
                case false:
                    if (Filter_EnemyType[0] == false) return true;
                    break;
                case true:
                    if (Filter_EnemyType[1] == false) return true;
                    break;
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

    class EnemyListBasicInfo
    {
        public int Id { set; get; }
        public DataRow EnemyDR { set; get; }
    }

    class EnemyListAdapter : BaseAdapter<EnemyListBasicInfo>
    {
        List<EnemyListBasicInfo> mitems;
        Activity mcontext;
        int count = 0;

        public EnemyListAdapter(Activity context, List<EnemyListBasicInfo> items) :base()
        {
            mcontext = context;
            mitems = items;
        }

        public override EnemyListBasicInfo this[int position]
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
                if (view == null) view = mcontext.LayoutInflater.Inflate(Resource.Layout.EnemyListLayout, null);

                count += 1;
                if (count == 50)
                {
                    GC.Collect();
                    count = 0;
                }

                string code_name = (string)item.EnemyDR["CodeName"];

                ImageView EnemySmallImage = view.FindViewById<ImageView>(Resource.Id.EnemyListSmallImage);
                if (ETC.sharedPreferences.GetBoolean("DBListImageShow", false) == true)
                {
                    EnemySmallImage.Visibility = ViewStates.Visible;
                    string FilePath = System.IO.Path.Combine(ETC.CachePath, "Enemy", "Normal_Crop", code_name + ".gfdcache");
                    if (System.IO.File.Exists(FilePath) == true) EnemySmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));
                }
                else EnemySmallImage.Visibility = ViewStates.Gone;

                ImageView EnemyGradeIcon = view.FindViewById<ImageView>(Resource.Id.EnemyListGrade);
                int GradeIconId = 0;
                string enemy_type = "";

                switch ((bool)item.EnemyDR["IsBoss"])
                {
                    case false:
                        GradeIconId = Resource.Drawable.Grade_N;
                        enemy_type = "NM";
                        break;
                    case true:
                        GradeIconId = Resource.Drawable.Grade_S;
                        enemy_type = "Boss";
                        break;
                    default:
                        GradeIconId = Resource.Drawable.Grade_2;
                        enemy_type = "NM";
                        break;
                }
                EnemyGradeIcon.SetImageResource(GradeIconId);

                TextView EnemyType = view.FindViewById<TextView>(Resource.Id.EnemyListType);
                EnemyType.Text = enemy_type;

                TextView EnemyName = view.FindViewById<TextView>(Resource.Id.EnemyListName);
                EnemyName.Text = (string)item.EnemyDR["Name"];

                TextView EnemyCodeName = view.FindViewById<TextView>(Resource.Id.EnemyListCodeName);
                EnemyCodeName.Text = code_name;
            }
            catch (OutOfMemoryException)
            {
                GC.Collect();
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