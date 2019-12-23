using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;

using System;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.CalcActivity", Label = "계산기", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CalcMainActivity : BaseAppCompatActivity
    {
        Android.Support.V4.App.Fragment ExpItemCalc_F;
        Android.Support.V4.App.Fragment CoreCalc_F;
        Android.Support.V4.App.Fragment SkillTrainingCalc_F;
        Android.Support.V4.App.Fragment FSTGradeUp_F;
        Android.Support.V4.App.Fragment AreaExpCalc_F;

        Android.Support.V4.App.FragmentTransaction ft = null;

        DrawerLayout mainDrawerLayout;
        NavigationView mainNavigationView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
                SetTheme(Resource.Style.GFS_NoActionBar_Light);

            // Create your application here
            SetContentView(Resource.Layout.CalcMainLayout);

            ETC.LoadDBSync(ETC.skillTrainingList, "SkillTraining.gfs", true);
            ETC.LoadDBSync(ETC.freeOPList, "FreeOP.gfs", true);


            // Set Main Drawer

            mainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.Calc_MainDrawerLayout);
            mainDrawerLayout.DrawerOpened += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen); };
            mainDrawerLayout.DrawerClosed += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu); };
            mainNavigationView = FindViewById<NavigationView>(Resource.Id.Calc_NavigationView);
            mainNavigationView.NavigationItemSelected += MainNavigationView_NavigationItemSelected;


            // Set ActionBar

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Calc_Toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.Title = Resources.GetString(Resource.String.TitleName_ExpItemCalc);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);


            // Set Fragment

            ft = SupportFragmentManager.BeginTransaction();

            ExpItemCalc_F = new ExpItemCalc();
            CoreCalc_F = new Core();
            SkillTrainingCalc_F = new SkillTraining();
            FSTGradeUp_F = new FSTGradeUp();
            AreaExpCalc_F = new AreaExpCalc();

            ft.Add(Resource.Id.CalcFragmentContainer, ExpItemCalc_F, "ExpItemCalc");
            ft.Commit();
        }

        private void MainNavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            try
            {
                string title = "";

                ft = SupportFragmentManager.BeginTransaction();

                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.CalcNavigation_ExpItem:
                        ft.Replace(Resource.Id.CalcFragmentContainer, ExpItemCalc_F, "ExpItemCalc");
                        title = Resources.GetString(Resource.String.TitleName_ExpItemCalc);
                        break;
                    case Resource.Id.CalcNavigation_Core:
                        ft.Replace(Resource.Id.CalcFragmentContainer, CoreCalc_F, "CoreCalc");
                        title = Resources.GetString(Resource.String.TitleName_CoreCalc);
                        break;
                    case Resource.Id.CalcNavigation_SkillTraining:
                        ft.Replace(Resource.Id.CalcFragmentContainer, SkillTrainingCalc_F, "SkillTraining");
                        title = Resources.GetString(Resource.String.TitleName_SkillTrainingCalc);
                        break;
                    case Resource.Id.CalcNavigation_FSTGradeUp:
                        ft.Replace(Resource.Id.CalcFragmentContainer, FSTGradeUp_F, "FSTGradeUp");
                        title = Resources.GetString(Resource.String.TitleName_FSTGradeUpCalc);
                        break;
                    case Resource.Id.CalcNavigation_AreaExp:
                        ft.Replace(Resource.Id.CalcFragmentContainer, AreaExpCalc_F, "AreaExpCalc");
                        title = Resources.GetString(Resource.String.TitleName_AreaExpCalc);
                        break;
                }

                ft.Commit();

                mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                SupportActionBar.Title = title;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, Resource.String.ChangeMode_Error, ToastLength.Short).Show();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.CalcToolbarMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
                    {
                        mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    }
                    else
                    {
                        mainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    }
                    break;
                case Resource.Id.CalcExit:
                    mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                mainDrawerLayout.CloseDrawer(GravityCompat.Start);

                return;
            }
            else
            {
                GC.Collect();

                base.OnBackPressed();
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
        }
    }
}