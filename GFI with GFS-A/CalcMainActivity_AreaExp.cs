using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;

namespace GFI_with_GFS_A
{
    public class AreaExpCalc : Android.Support.V4.App.Fragment
    {
        private View v;

        private readonly int[] LevelExp = { 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500, 5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000, 21000, 23100, 25300, 27600, 30000, 32500, 35100, 37900, 41000, 44400, 48600, 53200, 58200, 63600, 69400, 75700, 82400, 89600, 97300, 105500, 114300, 123600, 133500, 144000, 155100, 166900, 179400, 192500, 206400, 221000, 236400, 252500, 269400, 287100, 305700, 325200, 345600, 366900, 389200, 412500, 436800, 462100, 488400, 515800, 544300, 573900, 604700, 636700, 669900, 704300, 749400, 796200, 844800, 895200, 947400, 1001400, 1057300, 1115200, 1175000, 1236800, 1300700, 1366700, 1434800, 1505100, 1577700, 1652500, 1729600, 1809100, 1891000, 1975300, 2087900, 2204000, 2323500, 2446600, 2573300, 2703700, 2837800, 2975700, 3117500, 3263200, 3363200, 3483200, 3623200, 3783200, 3963200, 4163200, 4383200, 4623200, 4903200, 5263200, 5743200, 6383200, 7283200, 8483200, 10083200, 12283200, 15283200, 19283200, 24283200, 30283200 };

        private Spinner AreaList;
        private ToggleButton ApplyVow;
        private ToggleButton ApplyExpEvent;
        private ToggleButton ApplyAutoAddDummy;
        private ToggleButton LastEnemy;
        private NumberPicker NowLevel;
        private NumberPicker TargetLevel;
        private NumberPicker DollDummy;
        private NumberPicker WarCount;
        private EditText NowExp;
        private TextView Result_Normal;
        private TextView Result_Leader;
        private TextView Result_MVP;
        private TextView Result_LeaderMVP;

        private DataRow AreaDR;

        private bool IsVow = false;
        private bool IsExpEvent = false;
        private bool IsAutoAddDummy = false;
        private bool HasLastEnemy = false;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_AreaExpCalc, container, false);

            AreaList = v.FindViewById<Spinner>(Resource.Id.CalcAreaSelector);
            AreaList.ItemSelected += AreaList_ItemSelected;

            NowLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcAreaExpNowLevel);
            NowLevel.ValueChanged += LevelSelector_ValueChanged;
            TargetLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcAreaExpTargetLevel);
            TargetLevel.ValueChanged += LevelSelector_ValueChanged;
            DollDummy = v.FindViewById<NumberPicker>(Resource.Id.CalcAreaExpDollDummy);
            DollDummy.ValueChanged += LevelSelector_ValueChanged;
            WarCount = v.FindViewById<NumberPicker>(Resource.Id.CalcAreaExpWarCount);
            WarCount.ValueChanged += LevelSelector_ValueChanged;
            NowExp = v.FindViewById<EditText>(Resource.Id.CalcAreaExpNowExp);
            NowExp.TextChanged += delegate { CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value); };
            ApplyVow = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpVowSelector);
            ApplyVow.CheckedChange += delegate
            {
                IsVow = ApplyVow.Checked;
                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            };
            ApplyExpEvent = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpExpEventSelector);
            ApplyExpEvent.CheckedChange += delegate
            {
                IsExpEvent = ApplyExpEvent.Checked;
                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            };
            ApplyAutoAddDummy = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpAutoAddDummySelector);
            ApplyAutoAddDummy.CheckedChange += delegate
            {
                IsAutoAddDummy = ApplyAutoAddDummy.Checked;
                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            };
            LastEnemy = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpLastEnemySelector);
            LastEnemy.CheckedChange += delegate
            {
                HasLastEnemy = LastEnemy.Checked;
                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            };
            Result_Normal = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_Normal);
            Result_Leader = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_Leader);
            Result_MVP = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_MVP);
            Result_LeaderMVP = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_Leader_MVP);

            DisplayMetrics dm = Context.Resources.DisplayMetrics;
            int width = dm.WidthPixels / 2;

            ApplyVow.LayoutParameters.Width = width;
            ApplyVow.LayoutParameters.Height = width;
            ApplyExpEvent.LayoutParameters.Width = width;
            ApplyExpEvent.LayoutParameters.Height = width;
            ApplyAutoAddDummy.LayoutParameters.Width = width;
            ApplyAutoAddDummy.LayoutParameters.Height = width;
            LastEnemy.LayoutParameters.Width = width;
            LastEnemy.LayoutParameters.Height = width;

            Result_Normal.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultNormalResultText)} => 0";
            Result_Leader.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderResultText)} => 0";
            Result_MVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultMVPResultText)} => 0";
            Result_LeaderMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderMVPResultText)} => 0";

            InitializeProcess();

            return v;
        }

        private void AreaList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                AreaDR = ETC.FreeOPList.Rows[e.Position];

                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }

        private void InitializeProcess()
        {
            List<string> Area_List = new List<string>();

            foreach (DataRow dr in ETC.FreeOPList.Rows) Area_List.Add((string)dr["Location"]);

            var adapter = new ArrayAdapter(Activity, Resource.Layout.SpinnerListLayout, Area_List);
            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
            AreaList.Adapter = adapter;

            ApplyVow.Checked = false;

            NowLevel.MinValue = 1;
            NowLevel.MaxValue = 120;
            NowLevel.Value = 1;
            TargetLevel.MinValue = 1;
            TargetLevel.MaxValue = 120;
            TargetLevel.Value = 1;
            DollDummy.MinValue = 1;
            DollDummy.MaxValue = 1;
            DollDummy.Value = 1;
            WarCount.MinValue = 1;
            WarCount.MaxValue = 20;
            WarCount.Value = 1;
        }

        private void LevelSelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcAreaExpNowLevel:
                        TargetLevel.MinValue = e.NewVal;
                        NowExp.Text = LevelExp[np.Value - 1].ToString();
                        if (e.NewVal < e.OldVal)
                        {
                            if (e.NewVal < 10) DollDummy.MaxValue = 1;
                            else if (e.NewVal < 30) DollDummy.MaxValue = 2;
                            else if (e.NewVal < 70) DollDummy.MaxValue = 3;
                            else if (e.NewVal < 90) DollDummy.MaxValue = 4;
                        }
                        else
                        {
                            if (e.NewVal >= 90) DollDummy.MaxValue = 5;
                            else if (e.NewVal >= 70) DollDummy.MaxValue = 4;
                            else if (e.NewVal >= 30) DollDummy.MaxValue = 3;
                            else if (e.NewVal >= 10) DollDummy.MaxValue = 2;
                        }
                        break;
                }

                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }

        private void CalcCount(int start, int target, int dummy, int war_count)
        {
            int EXP = (int)AreaDR["EXP"];
            int[] ResultCount = { 0, 0, 0, 0 };

            try
            {
                if (int.TryParse(NowExp.Text, out int now_exp) == false) return;

                ResultCount[0] = CalcTotalCount(start, target, now_exp, dummy, war_count, false, false);
                ResultCount[1] = CalcTotalCount(start, target, now_exp, dummy, war_count, true, false);
                ResultCount[2] = CalcTotalCount(start, target, now_exp, dummy, war_count, false, true);
                ResultCount[3] = CalcTotalCount(start, target, now_exp, dummy, war_count, true, true);

                Result_Normal.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultNormalResultText)} => {ResultCount[0]}";
                Result_Leader.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderResultText)} => {ResultCount[1]}";
                Result_MVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultMVPResultText)} => {ResultCount[2]}";
                Result_LeaderMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderMVPResultText)} => {ResultCount[3]}";
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }

        private int CalcTotalCount(int NowLevel, int TargetLevel, int nowExp, int Dummy, int WarCount, bool IsLeader, bool IsMVP)
        {
            try
            {
                const double LeaderRate = 1.2;
                const double MVPRate = 1.3;
                const double VowRate = 2.0;
                const double ExpEventRate = 1.5;
                int PaneltyLevel = (int)AreaDR["PaneltyLevel"];
                int targetExp = LevelExp[TargetLevel - 1];
                int EarnExp = 0;
                int LastEarnExp = 0;
                int TotalCount = 0;

                while (nowExp < targetExp)
                {
                    while ((nowExp < LevelExp[NowLevel - 1]) || (nowExp >= LevelExp[NowLevel])) NowLevel += 1;

                    if (IsAutoAddDummy == true)
                    {
                        if ((NowLevel >= 1) && (NowLevel < 10)) Dummy = 1;
                        else if ((NowLevel >= 10) && (NowLevel < 30)) Dummy = 2;
                        else if ((NowLevel >= 30) && (NowLevel < 70)) Dummy = 3;
                        else if ((NowLevel >= 70) && (NowLevel < 90)) Dummy = 4;
                        else if (NowLevel >= 90) Dummy = 5;
                        else Dummy = 1;
                    }

                    if (NowLevel >= (PaneltyLevel + 40))
                    {
                        EarnExp = 10;
                        if (HasLastEnemy == true) LastEarnExp = EarnExp * 2;
                    }
                    else
                    {
                        EarnExp = (int)AreaDR["EXP"];
                        if (HasLastEnemy == true) LastEarnExp = EarnExp * 2;

                        EarnExp = Convert.ToInt32(Math.Ceiling(EarnExp * (1 + (0.5 * (Dummy - 1)))));
                        if (HasLastEnemy == true) LastEarnExp = Convert.ToInt32(Math.Ceiling(LastEarnExp * (1 + (0.5 * (Dummy - 1)))));

                        double PaneltyRate = 1;

                        if ((NowLevel >= (PaneltyLevel + 1)) && (NowLevel < (PaneltyLevel + 10))) PaneltyRate = 0.8;
                        else if ((NowLevel >= (PaneltyLevel + 10)) && (NowLevel < (PaneltyLevel + 20))) PaneltyRate = 0.6;
                        else if ((NowLevel >= (PaneltyLevel + 20)) && (NowLevel < (PaneltyLevel + 30))) PaneltyRate = 0.4;
                        else if ((NowLevel >= (PaneltyLevel + 30)) && (NowLevel < (PaneltyLevel + 40))) PaneltyRate = 0.2;

                        EarnExp = Convert.ToInt32(Math.Ceiling(EarnExp * PaneltyRate));
                        if (HasLastEnemy == true) LastEarnExp = Convert.ToInt32(Math.Ceiling(LastEarnExp * PaneltyRate));

                        if (IsLeader == true) EarnExp = Convert.ToInt32(Math.Ceiling(EarnExp * LeaderRate));
                        if (IsMVP == true) EarnExp = Convert.ToInt32(Math.Ceiling(EarnExp * MVPRate));

                        if (HasLastEnemy == true)
                        {
                            if (IsLeader == true) LastEarnExp = Convert.ToInt32(Math.Ceiling(LastEarnExp * LeaderRate));
                            if (IsMVP == true) LastEarnExp = Convert.ToInt32(Math.Ceiling(LastEarnExp * MVPRate));
                        }
                    }

                    if (IsVow == true)
                    {
                        EarnExp = Convert.ToInt32(Math.Ceiling(EarnExp * VowRate));
                        if (HasLastEnemy == true) LastEarnExp = Convert.ToInt32(Math.Ceiling(LastEarnExp * VowRate));
                    }

                    if (IsExpEvent == true)
                    {
                        EarnExp = Convert.ToInt32(Math.Ceiling(EarnExp * ExpEventRate));
                        if (HasLastEnemy == true) LastEarnExp = Convert.ToInt32(Math.Ceiling(LastEarnExp * ExpEventRate));
                    }

                    if (HasLastEnemy == true) nowExp += (EarnExp * (WarCount - 1)) + LastEarnExp;
                    else nowExp += EarnExp * WarCount;

                    TotalCount += 1;
                }

                return TotalCount;
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }

            return -1;
        }
    }
}