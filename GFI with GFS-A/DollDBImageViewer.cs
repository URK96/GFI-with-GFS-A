using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "DollDBImageViewer", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBImageViewer : AppCompatActivity
    {
        private ScaleGestureDetector mScaleGestureDetector;
        private static float mScaleFactor = 1.0f;
        private bool IsScaleMode = false;
        private bool ChangeModeTemp = false;

        private DataRow DollInfoDR = null;
        private int DollDicNum;
        private string DollName;

        private CoordinatorLayout SnackbarLayout;
        private Spinner CostumeList;
        private ProgressBar LoadProgressBar;
        private Button RefreshCacheButton;
        private Button ChangeStateButton;
        private static ImageView DollImageView;
        private TextView ImageStatus;
        private FloatingActionButton ExitFAB;

        private ArrayList Costumes;

        private bool IsDamage = false;
        private int CostumeIndex = 0;
        private int ModIndex = 0;
        private bool HasCensored = false;
        private string[] CensorType;
        private bool IsRefresh = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.DollDB_ImageViewer);

                string[] temp = Intent.GetStringExtra("Data").Split(';');

                DollInfoDR = ETC.FindDataRow(ETC.DollList, "Name", temp[0]);
                DollDicNum = (int)DollInfoDR["DicNumber"];
                ModIndex = int.Parse(temp[1]);
                DollName = (string)DollInfoDR["Name"];
                HasCensored = (bool)DollInfoDR["HasCensor"];
                if (HasCensored == true) CensorType = ((string)DollInfoDR["CensorType"]).Split(';');

                DollImageView = FindViewById<ImageView>(Resource.Id.DollDBImageViewerImageView);
                DollImageView.Click += DollImageView_Click;
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBImageViewerSnackbarLayout);
                CostumeList = FindViewById<Spinner>(Resource.Id.DollDBImageViewerCostumeList);
                CostumeList.ItemSelected += CostumeList_ItemSelected;
                LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.DollDBImageViewerLoadProgress);
                LoadProgressBar.Visibility = ViewStates.Visible;
                RefreshCacheButton = FindViewById<Button>(Resource.Id.DollDBImageViewerRefreshImageCacheButton);
                RefreshCacheButton.Click += delegate
                {
                    IsRefresh = true;
                    LoadImage(CostumeIndex, IsDamage);
                };
                ChangeStateButton = FindViewById<Button>(Resource.Id.DollDBImageViewerChangeStateButton);
                ChangeStateButton.Click += ChangeStateButton_Click;
                ImageStatus = FindViewById<TextView>(Resource.Id.DollDBImageViewerImageStatus);
                ExitFAB = FindViewById<FloatingActionButton>(Resource.Id.DollDBImageViewerExitModeFAB);
                ExitFAB.Click += ExitFAB_Click;

                mScaleGestureDetector = new ScaleGestureDetector(this, new ImageScaleListener());

                LoadCostumeList();
                LoadImage(0, IsDamage);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void DollImageView_Click(object sender, EventArgs e)
        {
            if (IsScaleMode == false)
            {
                IsScaleMode = true;

                CostumeList.Animate().Alpha(0.0f).SetDuration(500).Start();
                FindViewById<LinearLayout>(Resource.Id.DollDBImageViewerButtonLayout).Animate().Alpha(0.0f).SetDuration(500).Start();
                ImageStatus.Animate().Alpha(0.0f).SetDuration(500).Start();

                ExitFAB.Animate().Alpha(1.0f).SetDuration(500).WithStartAction(new Java.Lang.Runnable(delegate { ExitFAB.Visibility = ViewStates.Visible; })).Start();
            }
        }

        private void ExitFAB_Click(object sender, EventArgs e)
        {
            if (IsScaleMode == true)
            {
                IsScaleMode = false;

                CostumeList.Animate().Alpha(1.0f).SetDuration(500).Start();
                FindViewById<LinearLayout>(Resource.Id.DollDBImageViewerButtonLayout).Animate().Alpha(1.0f).SetDuration(500).Start();
                ImageStatus.Animate().Alpha(1.0f).SetDuration(500).Start();

                ExitFAB.Animate().Alpha(0.0f).SetDuration(500).WithEndAction(new Java.Lang.Runnable(delegate { ExitFAB.Visibility = ViewStates.Gone; })).Start();

                mScaleFactor = 1.0f;
                DollImageView.Animate().ScaleX(1.0f).ScaleY(1.0f).SetDuration(500).SetStartDelay(300).Start();
            }
        }

        private void ChangeStateButton_Click(object sender, EventArgs e)
        {
            IsDamage = !IsDamage;

            LoadImage(CostumeIndex, IsDamage);
        }

        private void CostumeList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            IsDamage = false;
            CostumeIndex = e.Position;

            LoadImage(CostumeIndex, IsDamage);
        }

        private void LoadCostumeList()
        {
            try
            {
                Costumes = new ArrayList()
                {
                    "기본 코스튬"
                };

                if (DollInfoDR["Costume"] != DBNull.Value)
                {
                    if (System.String.IsNullOrEmpty((string)DollInfoDR["Costume"]) == false)
                    {
                        string[] costume_list = ((string)DollInfoDR["Costume"]).Split(';');

                        foreach (string s in costume_list) Costumes.Add(s);
                    }
                }

                Costumes.TrimToSize();

                var CostumeListAdapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, Costumes);
                CostumeListAdapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                CostumeList.Adapter = CostumeListAdapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private async void LoadImage(int CostumeIndex, bool Damage)
        {
            try
            {
                LoadProgressBar.Visibility = ViewStates.Visible;

                await Task.Delay(100);

                string ImageName = DollDicNum.ToString();
                if (CostumeIndex >= 1) ImageName += "_" + (CostumeIndex + 1);
                else if ((CostumeIndex == 0) && (ModIndex == 3)) ImageName += "_M";
                if (Damage == true) ImageName += "_D";

                if ((HasCensored == true) && (ETC.sharedPreferences.GetBoolean("UseCensorImage", true)) && (ModIndex != 3))
                {
                    string censor_type = "";

                    switch (CostumeIndex)
                    {
                        case 0:
                            if (Damage == true) censor_type = "D";
                            else censor_type = "N";
                            break;
                        default:
                            censor_type = "C" + CostumeIndex;
                            if (Damage == true) censor_type += "D";
                            break;
                    }

                    foreach (string type in CensorType)
                    {
                        if (type == censor_type) ImageName += "_C";
                    }
                }

                string ImagePath = Path.Combine(ETC.CachePath, "Doll", "Normal", ImageName + ".gfdcache");

                if ((File.Exists(ImagePath) == false) || (IsRefresh == true))
                {
                    using (WebClient wc = new WebClient())
                    {
                        await Task.Run(async () => { await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", ImageName + ".png"), ImagePath); });
                    }
                }

                await Task.Delay(500);

                DollImageView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(ImagePath));

                StringBuilder sb = new StringBuilder();
                sb.Append(DollName);
                sb.Append(" - ");
                sb.Append((string)Costumes[CostumeIndex]);
                sb.Append(" - ");
                if (IsDamage == true)
                {
                    sb.Append("중상");
                    ChangeStateButton.Text = "중상";
                }
                else
                {
                    sb.Append("정상");
                    ChangeStateButton.Text = "정상";
                }

                ImageStatus.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                LoadProgressBar.Visibility = ViewStates.Invisible;
                IsRefresh = false;
            }
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            if (IsScaleMode == true) mScaleGestureDetector.OnTouchEvent(ev);

            return base.DispatchTouchEvent(ev);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }

        private class ImageScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            public override bool OnScale(ScaleGestureDetector detector)
            {
                mScaleFactor *= detector.ScaleFactor;
                mScaleFactor = Math.Max(1.0f, Math.Min(mScaleFactor, 3.0f));

                DollImageView.ScaleX = mScaleFactor;
                DollImageView.ScaleY = mScaleFactor;

                return true;
            }
        }
    }
}