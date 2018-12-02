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
using System.Text;
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
        private Button ChangeStateButton;
        private PhotoView DollImageView;
        private TextView ImageStatus;

        private List<string> Costumes;

        private bool IsDamage = false;
        private int CostumeIndex = 0;
        private int ModIndex = 0;
        private bool HasCensored = false;
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

                if (ETC.sharedPreferences.GetBoolean("DollImageCensoredUnlock", false) == false)
                {
                    HasCensored = doll.HasCensored;
                    if (HasCensored == true) CensorType = doll.CensorType;
                }

                DollImageView = FindViewById<PhotoView>(Resource.Id.DollDBImageViewerImageView);
                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBImageViewerSnackbarLayout);
                CostumeList = FindViewById<Spinner>(Resource.Id.DollDBImageViewerCostumeList);
                CostumeList.ItemSelected += CostumeList_ItemSelected;
                LoadProgressBar = FindViewById<ProgressBar>(Resource.Id.DollDBImageViewerLoadProgress);
                LoadProgressBar.Visibility = ViewStates.Visible;
                RefreshCacheButton = FindViewById<Button>(Resource.Id.DollDBImageViewerRefreshImageCacheButton);
                RefreshCacheButton.Click += delegate { LoadImage(CostumeIndex, IsDamage, true); };
                ChangeStateButton = FindViewById<Button>(Resource.Id.DollDBImageViewerChangeStateButton);
                ChangeStateButton.Click += ChangeStateButton_Click;
                ImageStatus = FindViewById<TextView>(Resource.Id.DollDBImageViewerImageStatus);

                LoadCostumeList();
                LoadImage(0, IsDamage, false);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void ChangeStateButton_Click(object sender, EventArgs e)
        {
            IsDamage = !IsDamage;

            LoadImage(CostumeIndex, IsDamage, false);
        }

        private void CostumeList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            IsDamage = false;
            CostumeIndex = e.Position;

            LoadImage(CostumeIndex, IsDamage, false);
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

        private async void LoadImage(int CostumeIndex, bool Damage, bool IsRefresh)
        {
            try
            {
                LoadProgressBar.Visibility = ViewStates.Visible;

                await Task.Delay(100);

                string ImageName = doll.DicNumber.ToString();
                if (CostumeIndex >= 1) ImageName += $"_{CostumeIndex + 1}";
                else if ((CostumeIndex == 0) && (ModIndex == 3)) ImageName += "_M";
                if (Damage == true) ImageName += "_D";

                if ((HasCensored == true) && (ModIndex != 3))
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