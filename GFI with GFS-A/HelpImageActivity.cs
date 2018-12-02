using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;

namespace GFI_with_GFS_A
{
    [Activity(Label = "HelpImageActivity", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class HelpImageActivity : FragmentActivity
    {
        private ImageView HelpImageView;

        private string ImageType = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.HelpImageLayout);

            ImageType = Intent.GetStringExtra("Type");

            HelpImageView = FindViewById<ImageView>(Resource.Id.HelpImageView);
            HelpImageView.Click += delegate
            {
                Finish();
                OverridePendingTransition(Resource.Animation.Fadein, Resource.Animation.Fadeout);
            };

            SetImage();
        }

        private void SetImage()
        {
            int ImageId = 0;

            switch (ImageType)
            {
                case "DBList":
                    ImageId = Resource.Drawable.Help_DBList;
                    break;
                case "DollDBDetail":
                    ImageId = Resource.Drawable.Help_DollDBDetail;
                    break;
                case "EquipDBDetail":
                    ImageId = Resource.Drawable.Help_EquipDBDetail;
                    break;
                case "FairyDBDetail":
                    ImageId = Resource.Drawable.Help_FairyDBDetail;
                    break;
                case "Calc":
                    break;
                case "OSTPlayer":
                    break;
            }

            HelpImageView.SetImageResource(ImageId);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Fadein, Resource.Animation.Fadeout);
        }
    }
}