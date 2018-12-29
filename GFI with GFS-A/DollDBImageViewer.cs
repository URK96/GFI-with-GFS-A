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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UK.CO.Senab.Photoview;

namespace GFI_with_GFS_A
{
    [Activity(Label = "DollDBImageViewer", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBImageViewer : AppCompatActivity
    {
        private DataRow DollInfoDR = null;
        private Doll doll;

        private CoordinatorLayout SnackbarLayout;
        private Spinner CostumeList;
        private ProgressBar LoadProgressBar;
        private Button RefreshCacheButton;
        private ToggleButton ChangeStateButton;
        private ToggleButton CensoredOption;
        private PhotoView DollImageView;
        private TextView ImageStatus;

        private List<string> Costumes;

        private bool IsDamage = false;
        private int CostumeIndex = 0;
        private int ModIndex = 0;
        private bool HasCensored = false;
        private bool EnableCensored = true;
        private string[] CensorType;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

                // Create your application here
                SetContentView(Resource.Layout.DollDB_ImageViewer);

                string[] temp = Intent.GetStringExtra("Data").Split(';');

                ModIndex = int.Parse(temp[1]);

                doll = new Doll(ETC.FindDataRow(ETC.DollList, "DicNumber", int.Parse(temp[0])));

                HasCensored = doll.HasCensored;
                if (HasCensored == true) CensorType = doll.CensorType;

                DollImageView = FindViewById<PhotoView>(Resource.Id.DollDBImageViewerImageView);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBImageViewerSnackbarLayout);
                CostumeList = FindViewById<Spinner>(Resource.Id.DollDBImageViewerCostumeList);
                CostumeList.ItemSelected += CostumeList_ItemSelected;
                LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.DollDBImageViewerLoadProgress);
                LoadProgressBar.Visibility = ViewStates.Visible;
                RefreshCacheButton = FindViewById<Button>(Resource.Id.DollDBImageViewerRefreshImageCacheButton);
                RefreshCacheButton.Click += delegate { LoadImage(CostumeIndex, true); };
                ChangeStateButton = FindViewById<ToggleButton>(Resource.Id.DollDBImageViewerChangeStateButton);
                ChangeStateButton.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) =>
                {
                    IsDamage = e.IsChecked;
                    CensoredOption.Enabled = CheckCensorType();
                    if (CensoredOption.Enabled == true) CensoredOption.Checked = true;
                    LoadImage(CostumeIndex, false);
                };
                CensoredOption = FindViewById<ToggleButton>(Resource.Id.DollDBImageViewerCensoredOption);
                if (HasCensored == true) CensoredOption.Checked = true;
                else CensoredOption.Checked = false;
                CensoredOption.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) =>
                {
                    EnableCensored = e.IsChecked;
                    LoadImage(CostumeIndex, false);
                };
                ImageStatus = FindViewById<TextView>(Resource.Id.DollDBImageViewerImageStatus);

                if (ETC.sharedPreferences.GetBoolean("ImageCensoredUnlock", false) == true)
                    CensoredOption.Visibility = ViewStates.Visible;
                else
                    CensoredOption.Visibility = ViewStates.Gone;

                LoadCostumeList();
                LoadImage(0, false);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void CostumeList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            IsDamage = false;
            CostumeIndex = e.Position;
            ChangeStateButton.Checked = false;
            CensoredOption.Enabled = CheckCensorType();
            if (CensoredOption.Enabled == true) CensoredOption.Checked = true;

            LoadImage(CostumeIndex, false);
        }

        private void LoadCostumeList()
        {
            try
            {
                Costumes = new List<string>()
                {
                    Resources.GetString(Resource.String.DollDBImageViewer_DefaultCostume)
                };

                if (doll.Costumes != null) Costumes.AddRange(doll.Costumes);

                Costumes.TrimExcess();

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

        private bool CheckCensorType()
        {
            string censor_type = "";

            switch (CostumeIndex)
            {
                case 0:
                    if (IsDamage == true) censor_type = "D";
                    else censor_type = "N";
                    break;
                default:
                    censor_type = $"C{CostumeIndex}";
                    if (IsDamage == true) censor_type += "D";
                    break;
            }

            foreach (string type in CensorType)
            {
                if (type == censor_type) return true;
            }

            return false;
        }

        private async void LoadImage(int CostumeIndex, bool IsRefresh)
        {
            try
            {
                LoadProgressBar.Visibility = ViewStates.Visible;

                await Task.Delay(100);

                string ImageName = doll.DicNumber.ToString();
                if (CostumeIndex >= 1) ImageName += $"_{CostumeIndex + 1}";
                else if ((CostumeIndex == 0) && (ModIndex == 3)) ImageName += "_M";
                if (IsDamage == true) ImageName += "_D";

                if ((HasCensored == true) && (EnableCensored == true) && (ModIndex != 3))
                    if (CheckCensorType() == true) ImageName += "_C";

                string ImagePath = Path.Combine(ETC.CachePath, "Doll", "Normal", $"{ImageName}.gfdcache");
                string URL = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal", $"{ImageName}.png");

                if ((File.Exists(ImagePath) == false) || (IsRefresh == true))
                {
                    using (WebClient wc = new WebClient())
                    {
                        await Task.Run(async () => { await wc.DownloadFileTaskAsync(URL, ImagePath); });
                    }
                }

                await Task.Delay(100);

                DollImageView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(ImagePath));

                string DamageText = "";

                switch (IsDamage)
                {
                    case true:
                        DamageText = Resources.GetString(Resource.String.DollDBImageViewer_ImageStatusDamage);
                        break;
                    case false:
                        DamageText = Resources.GetString(Resource.String.DollDBImageViewer_ImageStatusNormal);
                        break;
                }

                ChangeStateButton.Text = DamageText;
                ImageStatus.Text = $"{doll.Name} - {Costumes[CostumeIndex]} - {DamageText}";
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

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }
}