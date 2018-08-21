using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;

namespace GFI_with_GFS_A
{
    [Activity(Label = "계산기", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class CalcMainActivity : AppCompatActivity
    {
        Fragment ExpItemCalc_F;
        Fragment CoreCalc_F;
        Fragment SkillTrainingCalc_F;

        FragmentTransaction ft = null;

        DrawerLayout MainDrawerLayout;
        NavigationView MainNavigationView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

            // Create your application here
            SetContentView(Resource.Layout.CalcMainLayout);

            MainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.Calc_MainDrawerLayout);
            MainNavigationView = FindViewById<NavigationView>(Resource.Id.Calc_NavigationView);
            MainNavigationView.NavigationItemSelected += MainNavigationView_NavigationItemSelected;

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Calc_Toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            if (ETC.UseLightTheme == true) SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.CalcIcon_WhiteTheme);
            else SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.CalcIcon);
            SupportActionBar.Title = Resources.GetString(Resource.String.TitleName_ExpItemCalc);

            ft = FragmentManager.BeginTransaction();

            ExpItemCalc_F = new ExpItemCalc();
            CoreCalc_F = new Core();
            SkillTrainingCalc_F = new SkillTraining();

            ft.Add(Resource.Id.CalcFragmentContainer, ExpItemCalc_F, "ExpItemCalc");

            ft.Commit();
        }

        private void MainNavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            try
            {
                string title = "";
                ft = FragmentManager.BeginTransaction();

                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.CalcNavigation_ExpItem:
                        ft.Replace(Resource.Id.CalcFragmentContainer, ExpItemCalc_F, "ExpItemCalc");
                        title = Resources.GetString(Resource.String.TitleName_ExpItemCalc);
                        break;
                    case Resource.Id.CalcNavigation_Core:
                        ft.Replace (Resource.Id.CalcFragmentContainer, CoreCalc_F, "CoreCalc");
                        title = Resources.GetString(Resource.String.TitleName_CoreCalc);
                        break;
                    case Resource.Id.CalcNavigation_SkillTraining:
                        ft.Replace(Resource.Id.CalcFragmentContainer, SkillTrainingCalc_F, "SkillTraining");
                        title = Resources.GetString(Resource.String.TitleName_SkillTrainingCalc);
                        break;
                }

                ft.Commit();

                MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                SupportActionBar.Title = title;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.ChangeMode_Error, ToastLength.Short).Show();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == false) MainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    else MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }

    public class ExpItemCalc : Fragment
    {
        private View v;

        private readonly int[] LevelExp = { 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500, 5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000, 21000, 23100, 25300, 27600, 30000, 32500, 35100, 37900, 41000, 44400, 48600, 53200, 58200, 63600, 69400, 75700, 82400, 89600, 97300, 105500, 114300, 123600, 133500, 144000, 155100, 166900, 179400, 192500, 206400, 221000, 236400, 252500, 269400, 287100, 305700, 325200, 345600, 366900, 389200, 412500, 436800, 462100, 488400, 515800, 544300, 573900, 604700, 636700, 669900, 704300, 749400, 796200, 844800, 895200, 947400, 1001400, 1057300, 1115200, 1175000, 1236800, 1300700, 1366700, 1434800, 1505100, 1577700, 1652500, 1729600, 1809100, 1891000, 1975300, 2087900, 2204000, 2323500, 2446600, 2573300, 2703700, 2837800, 2975700, 3117500, 3263200, 3363200, 3483200, 3623200, 3783200, 3963200, 4163200, 4383200, 4623200, 4903200, 5263200, 5743200, 6383200, 7283200, 8483200, 10083200, 12283200, 15283200, 19283200, 24283200, 30283200 };
        private readonly int[] LevelExp_Fairy = { 0, 300, 600, 900, 1200, 1500, 1800, 2100, 2400, 2700, 3000, 3300, 3600, 3900, 4200, 4500, 4800, 5100, 5500, 6000, 6500, 7100, 8000, 9000, 10000, 11000, 12200, 13400, 14700, 16000, 17500, 18900, 20500, 22200, 23900, 25700, 27600, 29500, 31600, 33700, 35900, 38200, 40500, 43000, 45500, 48200, 50900, 53700, 56600, 59600, 62700, 65900, 69200, 72600, 76000, 79600, 83300, 87000, 90900, 94900, 99000, 103100, 107400, 111800, 116300, 120900, 125600, 130400, 135300, 140400, 145500, 150800, 156100, 161600, 167200, 172900, 178700, 184700, 190700, 196900, 202300, 209600, 216100, 222800, 229600, 236500, 243500, 250600, 257900, 265300, 272800, 280400, 288200, 296100, 304100, 312300, 320600, 329000, 337500, 357000 };

        private CheckBox ApplyMODModeCheckBox;
        private CheckBox ApplyVowCheckBox;
        private NumberPicker StartLevel;
        private NumberPicker TargetLevel;
        private EditText NowExp;
        private TextView Result;

        private bool IsVow = false;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_ExpItem, container, false);

            ApplyMODModeCheckBox = v.FindViewById<CheckBox>(Resource.Id.CalcReportModSelector);
            ApplyMODModeCheckBox.CheckedChange += ApplyMODModeCheckBox_CheckedChange;
            ApplyVowCheckBox = v.FindViewById<CheckBox>(Resource.Id.CalcReportVowSelector);
            ApplyVowCheckBox.CheckedChange += delegate
            {
                IsVow = ApplyVowCheckBox.Checked;
                CalcReport(StartLevel.Value, TargetLevel.Value);
            };
            StartLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcReportStartLevel);
            StartLevel.ValueChanged += LevelSelector_ValueChanged;
            TargetLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcReportEndLevel);
            TargetLevel.ValueChanged += LevelSelector_ValueChanged;
            NowExp = v.FindViewById<EditText>(Resource.Id.CalcReportNowExp);
            NowExp.TextChanged += delegate { CalcReport(StartLevel.Value, TargetLevel.Value); };
            Result = v.FindViewById<TextView>(Resource.Id.CalcReportResult);

            InitializeProcess();

            return v;
        }

        private void InitializeProcess()
        {
            StartLevel.MinValue = 1;
            StartLevel.MinValue = 1;
            StartLevel.Value = 1;
            TargetLevel.MinValue = 1;
            TargetLevel.MaxValue = 100;
            TargetLevel.Value = 1;
            NowExp.Text = "0";
        }

        private void LevelSelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcReportStartLevel:
                        TargetLevel.MinValue = e.NewVal;
                        NowExp.Text = LevelExp[np.Value - 1].ToString();
                        break;
                    case Resource.Id.CalcReportEndLevel:
                        StartLevel.MaxValue = e.NewVal;
                        break;
                }

                CalcReport(StartLevel.Value, TargetLevel.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }

        private void ApplyMODModeCheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            try
            {
                switch (cb.Checked)
                {
                    case true:
                        StartLevel.MaxValue = 100;
                        StartLevel.MinValue = 100;
                        StartLevel.Value = StartLevel.MinValue;
                        TargetLevel.MinValue = 100;
                        TargetLevel.MaxValue = 120;
                        TargetLevel.Value = TargetLevel.MinValue;
                        break;
                    case false:
                        StartLevel.MaxValue = 1;
                        StartLevel.MinValue = 1;
                        StartLevel.Value = StartLevel.MinValue;
                        TargetLevel.MinValue = 1;
                        TargetLevel.MaxValue = 100;
                        TargetLevel.Value = TargetLevel.MinValue;
                        break;
                }

                NowExp.Text = LevelExp[StartLevel.Value - 1].ToString();

                CalcReport(StartLevel.Value, TargetLevel.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }

        private void CalcReport(int start, int target)
        {
            const int ExpItem = 3000;

            try
            {
                int now_exp = 0;
                if (string.IsNullOrWhiteSpace(NowExp.Text) == false) now_exp = int.Parse(NowExp.Text);
                else now_exp = 0;

                int RequireExp = LevelExp[target - 1] - now_exp;
                int RequireExpItem = 0;
                int SurplusExp = 0;

                switch (IsVow)
                {
                    case false:
                        RequireExpItem = Convert.ToInt32(Math.Ceiling(RequireExp / Convert.ToDouble(ExpItem)));
                        SurplusExp = (ExpItem * RequireExpItem) - RequireExp;
                        break;
                    case true:
                        RequireExpItem = Convert.ToInt32(Math.Ceiling(RequireExp / Convert.ToDouble(ExpItem * 2)));
                        SurplusExp = (ExpItem * RequireExpItem * 2) - RequireExp;
                        break;
                }

                Result.Text = string.Format("{0}{1} / {2} Exp", RequireExpItem, Resources.GetString(Resource.String.ExpItemCalc_ItemCount), SurplusExp);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Short).Show();
            }
        }
    }

    public class Core : Fragment
    {
        private View v;

        private readonly int[] LevelLink_DollCount = { 0, 1, 1, 2, 3 };
        private readonly int[] GradeLinkCore = { 1, 3, 9, 15 };

        private NumberPicker DollGradeSelector;
        private NumberPicker DollDummyStart;
        private NumberPicker DollDummyTarget;
        private TextView Result;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_Core, container, false);

            DollGradeSelector = v.FindViewById<NumberPicker>(Resource.Id.CalcCoreDollGrade);
            DollGradeSelector.ValueChanged += delegate (object sender, NumberPicker.ValueChangeEventArgs e) { CalcCore(e.NewVal, DollDummyStart.Value, DollDummyTarget.Value); };
            DollDummyStart = v.FindViewById<NumberPicker>(Resource.Id.CalcCoreStartDollCount);
            DollDummyStart.ValueChanged += DollDummySelector_ValueChanged;
            DollDummyTarget = v.FindViewById<NumberPicker>(Resource.Id.CalcCoreEndDollCount);
            DollDummyTarget.ValueChanged += DollDummySelector_ValueChanged;
            Result = v.FindViewById<TextView>(Resource.Id.CalcCoreResult);

            InitializeProcess();

            return v;
        }

        private void InitializeProcess()
        {
            DollGradeSelector.MinValue = 2;
            DollGradeSelector.MaxValue = 5;
            DollGradeSelector.Value = 2;
            DollDummyStart.MinValue = 1;
            DollDummyStart.MaxValue = 1;
            DollDummyStart.Value = 1;
            DollDummyTarget.MinValue = 1;
            DollDummyTarget.MaxValue = 5;
            DollDummyTarget.Value = 1;
        }

        private void DollDummySelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcCoreStartDollCount:
                        DollDummyTarget.MinValue = e.NewVal;
                        break;
                    case Resource.Id.CalcCoreEndDollCount:
                        DollDummyStart.MaxValue = e.NewVal;
                        break;
                }

                CalcCore(DollGradeSelector.Value, DollDummyStart.Value, DollDummyTarget.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }

        private void CalcCore(int grade, int start, int target)
        {
            try
            {
                int RequireDollCount = 0;

                for (int i = 0; i < (target - start); ++i)
                {
                    RequireDollCount += LevelLink_DollCount[start + i];
                }

                int ResultCore = RequireDollCount * GradeLinkCore[grade - 2];

                Result.Text = string.Format("{0}{1}", ResultCore, Resources.GetString(Resource.String.CoreCalc_ItemCount));
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }
    }

    public class SkillTraining : Fragment
    {
        private View v;

        private Spinner TrainingTypeList;
        private NumberPicker TrainingStartLevel;
        private NumberPicker TrainingTargetLevel;
        private TextView Result_Chip;
        private TextView Result_Time;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_SkillTraining, container, false);

            TrainingTypeList = v.FindViewById<Spinner>(Resource.Id.CalcSkillTrainingType);
            TrainingTypeList.ItemSelected += delegate { CalcSkillTraining(TrainingStartLevel.Value, TrainingTargetLevel.Value); };
            TrainingStartLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcSkillTrainingStartLevel);
            TrainingStartLevel.ValueChanged += TrainingLevelSelector_ValueChanged;
            TrainingTargetLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcSkillTrainingEndLevel);
            TrainingTargetLevel.ValueChanged += TrainingLevelSelector_ValueChanged;
            Result_Chip = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultSkillChip);
            Result_Time = v.FindViewById<TextView>(Resource.Id.CalcSkillTrainingResultTime);

            InitializeProcess();

            return v;
        }

        private void InitializeProcess()
        {
            List<string> list = new List<string>(3);

            foreach (DataRow dr in ETC.SkillTrainingList.Rows) list.Add((string)dr["Type"]);
            list.TrimExcess();

            var adapter = new ArrayAdapter(Activity, Resource.Layout.SpinnerListLayout, list);
            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
            TrainingTypeList.Adapter = adapter;

            TrainingStartLevel.MinValue = 1;
            TrainingStartLevel.MaxValue = 1;
            TrainingStartLevel.Value = 1;
            TrainingTargetLevel.MinValue = 1;
            TrainingTargetLevel.MaxValue = 10;
            TrainingTargetLevel.Value = 1;
        }

        private void TrainingLevelSelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcSkillTrainingResultTime:
                        TrainingTargetLevel.MinValue = e.NewVal;
                        break;
                    case Resource.Id.CalcSkillTrainingEndLevel:
                        TrainingStartLevel.MaxValue = e.NewVal;
                        break;
                }

                CalcSkillTraining(TrainingStartLevel.Value, TrainingTargetLevel.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }

        private void CalcSkillTraining(int start, int target)
        {
            try
            {
                DataRow dr = ETC.SkillTrainingList.Rows[TrainingTypeList.SelectedItemPosition];

                string[] ItemConsume = ((string)dr["Consumption"]).Split(';');
                string[] Time = ((string)dr["Time"]).Split(';');
                string[] ItemType = ((string)dr["DataType"]).Split(';');

                int[] ItemCount = { 0, 0, 0 };
                int TimeCount = 0;

                for (int i = start; i < target; ++i)
                {
                    int count = Int32.Parse(ItemConsume[i - 1]);

                    switch (ItemType[i - 1])
                    {
                        case "B":
                            ItemCount[0] += count;
                            break;
                        case "A":
                            ItemCount[1] += count;
                            break;
                        case "M":
                            ItemCount[2] += count;
                            break;
                    }

                    TimeCount += int.Parse(Time[i - 1]);
                }

                Result_Chip.Text = string.Format("{0} / {1} / {2}",ItemCount[0], ItemCount[1], ItemCount[2]);
                Result_Time.Text = string.Format("{0} {1}", TimeCount, Resources.GetString(Resource.String.Time_Hour));
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                Toast.MakeText(Activity, Resource.String.InternalCalc_Error, ToastLength.Long).Show();
            }
        }
    }
}