using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "Open Source License", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LicenseViewer : BaseAppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;
        private TextView licenseView;

        string licenseType = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.LicenseViewerLayout);

            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.LicenseViewerMainToolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Open Source License";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            licenseType = Intent.GetStringExtra("Type");

            licenseView = FindViewById<TextView>(Resource.Id.LicenseViewerTextView);

            _ = ReadLicense();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async Task ReadLicense()
        {
            string assetName = "";

            try
            {
                switch (licenseType)
                {
                    case "MIT":
                        assetName = "MIT_License.txt";
                        break;
                    case "Microsoft_License":
                        assetName = "MICROSOFT_SOFTWARE_LICENSE_TERMS.txt";
                        break;
                    case "Arcana_Studio_License":
                        assetName = "Arcana_Studio_License.txt";
                        break;
                    case "MIT_NewtonSoft":
                        assetName = "NewtonSoft_MIT_License.txt";
                        break;
                    case "Syncfusion_License":
                        assetName = "ESSENTIAL STUDIO SOFTWARE LICENSE.txt";
                        break;
                    case "MIT_.NET Foundation and Contributor":
                        assetName = "DotNET_Foundation_and_Contributors_MIT_License.txt";
                        break;
                    case "MIT_.NET Foundation Contributor":
                        assetName = "DotNET_Foundation_Contributors_MIT_License.txt";
                        break;
                    case "MIT_Xamarin":
                        assetName = "Xamarin_MIT_License.txt";
                        break;
                    case "MIT_Microsoft Corporation":
                        assetName = "Xamarin.Essentials_MIT_License.txt";
                        break;
                    case "Apache-2.0":
                        assetName = "Apache2.0.txt";
                        break;
                    case "Unknown":
                        assetName = "Unknown.txt";
                        break;
                }

                try
                {
                    using (StreamReader sr = new StreamReader(Assets.Open(assetName)))
                    {
                        licenseView.Text = sr.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                    Toast.MakeText(this, "Cannot Read License Raw Data", ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }
    }
}