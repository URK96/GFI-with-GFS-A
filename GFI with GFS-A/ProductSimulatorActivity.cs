using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Label = "ProductSimulatorActivity", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ProductSimulatorActivity : FragmentActivity
    {
        enum ProductCategory { Doll, Equip }
        enum ProductType { Normal, Advance }
        enum AdvanceType { _1, _2, _3 }

        private ProductCategory Category;
        private ProductType Type;
        private AdvanceType Adv_Type;

        private NumberPicker[] ManPower_NPs = new NumberPicker[4];
        private NumberPicker[] Ammo_NPs = new NumberPicker[4];
        private NumberPicker[] Food_NPs = new NumberPicker[4];
        private NumberPicker[] Parts_NPs = new NumberPicker[4];

        private readonly int[] ManPower_NP_Ids =
        {
            Resource.Id.PSManPowerNumberPicker1,
            Resource.Id.PSManPowerNumberPicker2,
            Resource.Id.PSManPowerNumberPicker3,
            Resource.Id.PSManPowerNumberPicker4
        };

        private readonly int[] Ammo_NP_Ids =
        {
            Resource.Id.PSAmmoNumberPicker1,
            Resource.Id.PSAmmoNumberPicker2,
            Resource.Id.PSAmmoNumberPicker3,
            Resource.Id.PSAmmoNumberPicker4
        };

        private readonly int[] Food_NP_Ids =
        {
            Resource.Id.PSFoodNumberPicker1,
            Resource.Id.PSFoodNumberPicker2,
            Resource.Id.PSFoodNumberPicker3,
            Resource.Id.PSFoodNumberPicker4
        };

        private readonly int[] Parts_NP_Ids =
        {
            Resource.Id.PSPartsNumberPicker1,
            Resource.Id.PSPartsNumberPicker2,
            Resource.Id.PSPartsNumberPicker3,
            Resource.Id.PSPartsNumberPicker4
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.ProductSimulatorLayout);

            string[] InfoData = Intent.GetStringExtra("Info").Split('/');

            for (int i = 0; i < ManPower_NPs.Length; ++i) ManPower_NPs[i] = FindViewById<NumberPicker>(ManPower_NP_Ids[i]);
            for (int i = 0; i < Ammo_NPs.Length; ++i) Ammo_NPs[i] = FindViewById<NumberPicker>(Ammo_NP_Ids[i]);
            for (int i = 0; i < Food_NPs.Length; ++i) Food_NPs[i] = FindViewById<NumberPicker>(Food_NP_Ids[i]);
            for (int i = 0; i < Parts_NPs.Length; ++i) Parts_NPs[i] = FindViewById<NumberPicker>(Parts_NP_Ids[i]);

            switch (InfoData[1])
            {
                default:
                case "Normal":
                    Type = ProductType.Normal;
                    ManPower_NPs[3].Visibility = ViewStates.Gone;
                    Ammo_NPs[3].Visibility = ViewStates.Gone;
                    Food_NPs[3].Visibility = ViewStates.Gone;
                    Parts_NPs[3].Visibility = ViewStates.Gone;
                    break;
                case "Advance":
                    Type = ProductType.Advance;
                    break;
            }

            switch (InfoData[0])
            {
                default:
                case "Doll":
                    Category = ProductCategory.Doll;
                    break;
                case "Equip":
                    Category = ProductCategory.Equip;
                    break;
            }

            //InitNumberPickerRange();
        }

        private void InitNumberPickerRange()
        {
            if (Category == ProductCategory.Doll)
            {
                if (Type == ProductType.Normal)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        int value = 0;
                        int min = 0;
                        int max = 9;

                        if (i == 2) min = 3;
                    }
                }
            }
        }
    }
}