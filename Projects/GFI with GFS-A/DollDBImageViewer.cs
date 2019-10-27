using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using ImageViews.Photo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "DollDBImageViewer", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBImageViewer : BaseAppCompatActivity
    {
        private Doll doll;

        private CoordinatorLayout snackbarLayout;
        private Spinner costumeList;
        private ProgressBar loadProgressBar;
        private Button refreshCacheButton;
        private ToggleButton changeStateButton;
        private ToggleButton censoredOption;
        private PhotoView dollImageView;
        private TextView imageStatus;

        private List<string> costumes;

        private bool isDamage = false;
        private int costumeIndex = 0;
        private int modIndex = 0;
        private bool enableCensored = true;
        private string[] censorType;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_NoActionBar_Light);
                }

                // Create your application here
                SetContentView(Resource.Layout.DollDB_ImageViewer);

                string[] temp = Intent.GetStringExtra("Data").Split(';');

                modIndex = int.Parse(temp[1]);
                doll = new Doll(ETC.FindDataRow(ETC.dollList, "DicNumber", int.Parse(temp[0])));
                censorType = doll.HasCensored ? doll.CensorType : null;

                dollImageView = FindViewById<PhotoView>(Resource.Id.DollDBImageViewerImageView);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DollDBImageViewerSnackbarLayout);
                costumeList = FindViewById<Spinner>(Resource.Id.DollDBImageViewerCostumeList);
                costumeList.ItemSelected += (sender , e) =>
                {
                    isDamage = false;
                    costumeIndex = e.Position;
                    changeStateButton.Checked = false;
                    censoredOption.Checked = censoredOption.Enabled = CheckCensorType();

                    LoadImage(costumeIndex, false);
                };
                loadProgressBar = FindViewById<ProgressBar>(Resource.Id.DollDBImageViewerLoadProgress);
                loadProgressBar.Visibility = ViewStates.Visible;
                refreshCacheButton = FindViewById<Button>(Resource.Id.DollDBImageViewerRefreshImageCacheButton);
                refreshCacheButton.Click += delegate { LoadImage(costumeIndex, true); };
                changeStateButton = FindViewById<ToggleButton>(Resource.Id.DollDBImageViewerChangeStateButton);
                changeStateButton.CheckedChange += (sender, e) =>
                {
                    isDamage = e.IsChecked;
                    censoredOption.Checked = censoredOption.Enabled = CheckCensorType();

                    LoadImage(costumeIndex, false);
                };
                censoredOption = FindViewById<ToggleButton>(Resource.Id.DollDBImageViewerCensoredOption);
                censoredOption.Checked = doll.HasCensored;
                censoredOption.CheckedChange += (sender, e) =>
                {
                    enableCensored = e.IsChecked;

                    LoadImage(costumeIndex, false);
                };
                imageStatus = FindViewById<TextView>(Resource.Id.DollDBImageViewerImageStatus);

                censoredOption.Visibility = ETC.sharedPreferences.GetBoolean("ImageCensoredUnlock", false) ? ViewStates.Visible : ViewStates.Gone;

                LoadCostumeList();
                LoadImage(0, false);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.Activity_OnCreateError, ToastLength.Short).Show();
            }
        }

        private void LoadCostumeList()
        {
            try
            {
                costumes = new List<string>()
                {
                    Resources.GetString(Resource.String.DollDBImageViewer_DefaultCostume)
                };

                if (doll.Costumes != null)
                {
                    costumes.AddRange(doll.Costumes);
                }

                costumes.TrimExcess();

                var CostumeListAdapter = new ArrayAdapter(this, Resource.Layout.SpinnerListLayout, costumes);
                CostumeListAdapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                costumeList.Adapter = CostumeListAdapter;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Initialize_List_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        private bool CheckCensorType()
        {
            if (censorType == null)
            {
                return false;
            }

            string cType;

            switch (costumeIndex)
            {
                case 0:
                    cType = isDamage ? "D" : "N";
                    break;
                default:
                    cType = $"C{costumeIndex}";
                    cType += isDamage ? "D" : "";
                    break;
            }

            foreach (string type in censorType)
            {
                if (type == cType)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task LoadImage(int costumeIndex, bool isRefresh = false)
        {
            await Task.Delay(100);

            try
            {
                loadProgressBar.Visibility = ViewStates.Visible;

                string imageName = doll.DicNumber.ToString();

                if (costumeIndex >= 1)
                {
                    imageName += $"_{costumeIndex + 1}";
                }
                else if ((costumeIndex == 0) && (modIndex == 3))
                {
                    imageName += "_M";
                }

                imageName += isDamage ? "_D" : "";

                if (doll.HasCensored && enableCensored && (modIndex != 3))
                {
                    imageName += CheckCensorType() ? "_C" : "";
                }

                string imagePath = Path.Combine(ETC.cachePath, "Doll", "Normal", $"{imageName}.gfdcache");
                string url = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal", $"{imageName}.png");

                if (!File.Exists(imagePath) || isRefresh)
                {
                    using (WebClient wc = new WebClient())
                    {
                        await Task.Run(async () => { await wc.DownloadFileTaskAsync(url, imagePath); });
                    }
                }

                await Task.Delay(100);

                dollImageView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(imagePath));

                string damageText = isDamage ? Resources.GetString(Resource.String.DollDBImageViewer_ImageStatusDamage) : Resources.GetString(Resource.String.DollDBImageViewer_ImageStatusNormal);

                changeStateButton.Text = damageText;
                imageStatus.Text = $"{doll.Name} - {costumes[costumeIndex]} - {damageText}";
            }
            catch (WebException ex) when (ex.Message.Contains("System.IO"))
            {
                ETC.LogError(ex, this);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ImageLoad_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
            finally
            {
                loadProgressBar.Visibility = ViewStates.Invisible;
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