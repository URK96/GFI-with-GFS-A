using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Preference;

using Google.Android.Material.Snackbar;

using System;

using Xamarin.Essentials;

namespace GFDA
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

                FragmentManager.BeginTransaction().Replace(Resource.Id.SettingFragmentContainer, new HiddenSettingFragment(), null).Commit();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
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

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SnackbarLayout = Activity.FindViewById<CoordinatorLayout>(Resource.Id.SettingSnackbarLayout);
            SnackbarLayout.BringToFront();

            AddPreferencesFromResource(Resource.Xml.HiddenSetting);
            InitPreferences();
        }

        private void InitPreferences()
        {
            //SaveSetting = ETC.sharedPreferences.Edit();

            ListPreference MainActionbarIcon = (ListPreference)FindPreference("MainActionbarIcon");
            MainActionbarIcon.SetEntries(new string[] 
            {
                "RFB 1",
                "Dictionary",
                "K5",
                "RFB 2"
            });
            MainActionbarIcon.SetEntryValues(new string[] 
            {
                Resource.Drawable.AppIcon_Old.ToString(),
                Resource.Drawable.AppIcon_Old2.ToString(),
                Resource.Drawable.AppIcon.ToString(),
                Resource.Drawable.AppIcon2.ToString()
            });
            /*MainActionbarIcon.PreferenceChange += delegate
            {
                SaveSetting.PutString("MainActionbarIcon", MainActionbarIcon.Value);
                SaveSetting.Commit();
            };*/

            SwitchPreference UnlockCensored = (SwitchPreference)FindPreference("UnlockCensored");
            UnlockCensored.Checked = Preferences.Get("DollImageCensoredUnlock", false);
            UnlockCensored.PreferenceChange += delegate { Preferences.Set("DollImageCensoredUnlock", UnlockCensored.Checked); };
        }
    }
}