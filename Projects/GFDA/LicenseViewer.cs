using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

using System;
using System.IO;
using System.Threading.Tasks;

namespace GFDA
{
    [Activity(Label = "Open Source License", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LicenseViewer : BaseAppCompatActivity
    {
        private AndroidX.AppCompat.Widget.Toolbar toolbar;
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

            toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.LicenseViewerMainToolbar);

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
            await Task.Delay(100);

            try
            {
                string assetName = licenseType switch
                {
                    "MIT" => "MIT_License.txt",
                    "Microsoft_License" => "MICROSOFT_SOFTWARE_LICENSE_TERMS.txt",
                    "Arcana_Studio_License" => "Arcana_Studio_License.txt",
                    "MIT_NewtonSoft" => "NewtonSoft_MIT_License.txt",
                    "Syncfusion_License" => "ESSENTIAL STUDIO SOFTWARE LICENSE.txt",
                    "MIT_.NET Foundation and Contributor" => "DotNET_Foundation_and_Contributors_MIT_License.txt",
                    "MIT_.NET Foundation Contributor" => "DotNET_Foundation_Contributors_MIT_License.txt",
                    "MIT_Xamarin" => "Xamarin_MIT_License.txt",
                    "MIT_Microsoft Corporation" => "Xamarin.Essentials_MIT_License.txt",
                    "MIT_jzeferino" => "jzeferino_MIT_License.txt",
                    "Apache-2.0" => "Apache2.0.txt",
                    "MIT_.NET Foundation Contributor_Android Software Development Kit License Agreement" => "Android Software Development Kit License Agreement_with_MIT.txt",
                    "MIT_.NET Foundation Contributor_Apache-2.0" => "Apache2.0_with_MIT.txt",
                    _ => "Unknown.txt",
                };

                try
                {
                    using (var sr = new StreamReader(Assets.Open(assetName)))
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