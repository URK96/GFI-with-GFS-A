using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "Hidden Setting", Theme = "@style/GFS.Setting", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class HiddenSettingActivity : Activity
    {
        private CoordinatorLayout SnackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // Create your application here
                SetContentView(Resource.Layout.SettingMainLayout);

                SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.SettingSnackbarLayout);

                Fragment SettingFragment = new HiddenSettingFragment();
                FragmentTransaction ft = FragmentManager.BeginTransaction();
                ft.Add(Resource.Id.SettingFragmentContainer, SettingFragment);
                ft.Commit();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
        }
    }

    public class HiddenSettingFragment : PreferenceFragment
    {
        private ISharedPreferencesEditor SaveSetting;

        private CoordinatorLayout SnackbarLayout;

        private Dialog dialog;
        private ProgressDialog p_dialog = null;
        private ProgressBar totalProgressBar = null;
        private ProgressBar nowProgressBar = null;
        private TextView totalProgress = null;
        private TextView nowProgress = null;

        private int total = 0;
        private int now = 0;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SnackbarLayout = Activity.FindViewById<CoordinatorLayout>(Resource.Id.SettingSnackbarLayout);
            SnackbarLayout.BringToFront();

            AddPreferencesFromResource(Resource.Xml.HiddenSetting);
            InitPreferences();
        }

        private void InitPreferences()
        {
            SaveSetting = ETC.sharedPreferences.Edit();

            ListPreference MainActionbarIcon = (ListPreference)FindPreference("MainActionbarIcon");
            MainActionbarIcon.SetEntries(new string[] { "RFB 1", "Dictionary", "K5", "RFB 2" });
            MainActionbarIcon.SetEntryValues(new string[] { Resource.Drawable.AppIcon_Old.ToString(), Resource.Drawable.AppIcon_Old2.ToString(), Resource.Drawable.AppIcon.ToString(), Resource.Drawable.AppIcon2.ToString() });
            MainActionbarIcon.PreferenceChange += delegate
            {
                SaveSetting.PutString("MainActionbarIcon", MainActionbarIcon.Value);
                SaveSetting.Commit();
            };
        }
    }
}