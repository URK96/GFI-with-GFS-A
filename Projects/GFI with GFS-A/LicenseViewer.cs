using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "Open Source License", Theme = "@style/GFS")]
    public class LicenseViewer : BaseAppCompatActivity
    {
        TextView licenseView;

        string licenseType = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            LinearLayout mainLayout = new LinearLayout(this);
            mainLayout.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

            ScrollView scrollView = new ScrollView(this);
            scrollView.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

            licenseView = new TextView(this);

            scrollView.AddView(licenseView);
            mainLayout.AddView(scrollView);

            // Create your application here
            SetContentView(mainLayout);

            licenseType = Intent.GetStringExtra("Type");

            _ = ReadLicense();
        }

        private async Task ReadLicense()
        {
            string assetName = "";

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
                case "Unknown":
                    assetName = "Unknown.txt";
                    break;
            }

            await Task.Run(() =>
            {
                try
                {
                    using (StreamReader sr = new StreamReader(Assets.Open(assetName)))
                        licenseView.Text = sr.ReadToEnd();
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, this);
                    Toast.MakeText(this, "Cannot Read License Raw Data", ToastLength.Short).Show();
                }
            });
        }
    }
}