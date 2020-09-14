using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;

using Google.Android.Material.Navigation;

using System;

namespace GFDA
{
    [Activity(Name = "com.gfl.dic.CalcActivity", Label = "계산기", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CalcMainActivity : BaseAppCompatActivity
    {
        AndroidX.Fragment.App.Fragment expItemCalcF;
        AndroidX.Fragment.App.Fragment coreCalcF;
        AndroidX.Fragment.App.Fragment coalitionGradeF;
        AndroidX.Fragment.App.Fragment skillTrainingCalcF;
        AndroidX.Fragment.App.Fragment fstGradeUpF;
        AndroidX.Fragment.App.Fragment areaExpCalcF;

        AndroidX.Fragment.App.FragmentTransaction ft = null;

        DrawerLayout mainDrawerLayout;
        NavigationView mainNavigationView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_NoActionBar_Light);
            }

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

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.Calc_Toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.Title = Resources.GetString(Resource.String.TitleName_ExpItemCalc);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);


            // Set Fragment

            ft = SupportFragmentManager.BeginTransaction();

            expItemCalcF = new ExpItemCalc();
            coreCalcF = new DummyCore();
            coalitionGradeF = new CoalitionGrade();
            skillTrainingCalcF = new SkillTraining();
            fstGradeUpF = new FSTGradeUp();
            areaExpCalcF = new AreaExpCalc();

            ft.Add(Resource.Id.CalcFragmentContainer, expItemCalcF, "ExpItemCalc");
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
                        ft.Replace(Resource.Id.CalcFragmentContainer, expItemCalcF, "ExpItemCalc");
                        title = Resources.GetString(Resource.String.TitleName_ExpItemCalc);
                        break;
                    case Resource.Id.CalcNavigation_Core:
                        ft.Replace(Resource.Id.CalcFragmentContainer, coreCalcF, "CoreCalc");
                        title = Resources.GetString(Resource.String.TitleName_CoreCalc);
                        break;
                    case Resource.Id.CalcNavigation_CoalitionGrade:
                        ft.Replace(Resource.Id.CalcFragmentContainer, coalitionGradeF, "CoalitionGradeCalc");
                        title = Resources.GetString(Resource.String.TitleName_CoalitionGrade);
                        break;
                    case Resource.Id.CalcNavigation_SkillTraining:
                        ft.Replace(Resource.Id.CalcFragmentContainer, skillTrainingCalcF, "SkillTraining");
                        title = Resources.GetString(Resource.String.TitleName_SkillTrainingCalc);
                        break;
                    case Resource.Id.CalcNavigation_FSTGradeUp:
                        ft.Replace(Resource.Id.CalcFragmentContainer, fstGradeUpF, "FSTGradeUp");
                        title = Resources.GetString(Resource.String.TitleName_FSTGradeUpCalc);
                        break;
                    case Resource.Id.CalcNavigation_AreaExp:
                        ft.Replace(Resource.Id.CalcFragmentContainer, areaExpCalcF, "AreaExpCalc");
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