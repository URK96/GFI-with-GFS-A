using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System;

namespace GFI_with_GFS_A
{
    [Activity(Label = "", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ProductSimulatorCategorySelectActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.ProductSimulatorSelectLayout);

            SetTitle(Resource.String.ProductSimulatorCategorySelectActivity_Title);

            Button Doll_NormalProduct = FindViewById<Button>(Resource.Id.ProductSimulatorCategoryDollNormalButton);
            Doll_NormalProduct.Click += ProductCategoryButton_Click;
            Button Doll_AdvanceProduct = FindViewById<Button>(Resource.Id.ProductSimulatorCategoryDollAdvanceButton);
            Doll_AdvanceProduct.Click += ProductCategoryButton_Click;
            Button Equip_NormalProduct = FindViewById<Button>(Resource.Id.ProductSimulatorCategoryEquipNormalButton);
            Equip_NormalProduct.Click += ProductCategoryButton_Click;
            Button Equip_AdvanceProduct = FindViewById<Button>(Resource.Id.ProductSimulatorCategoryEquipAdvanceButton);
            Equip_AdvanceProduct.Click += ProductCategoryButton_Click;
        }

        private void ProductCategoryButton_Click(object sender, EventArgs e)
        {
            string ProductType = "";

            Button bt = sender as Button;

            switch (bt.Id)
            {
                case Resource.Id.ProductSimulatorCategoryDollNormalButton:
                    ProductType = "Doll/Normal";
                    break;
                case Resource.Id.ProductSimulatorCategoryDollAdvanceButton:
                    ProductType = "Doll/Advance";
                    break;
                case Resource.Id.ProductSimulatorCategoryEquipNormalButton:
                    return;
                    /*ProductType = "Equip/Normal";
                    break;*/
                case Resource.Id.ProductSimulatorCategoryEquipAdvanceButton:
                    return;
                    /*ProductType = "Equip/Advance";
                    break;*/
                default:
                    return;
            }

            Intent ProductInfo = new Intent(this, typeof(ProductSimulatorActivity));
            ProductInfo.PutExtra("Info", ProductType);
            StartActivity(ProductInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }
    }
}