using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    public class AreaExpCalc : Android.Support.V4.App.Fragment
    {
        private View v;

        // T-Doll Require Exp Between Level
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
        private EditText CommanderCostumeBonus;
        private TextView Result_Normal;
        private TextView Result_Leader;
        private TextView Result_MVP;
        private TextView Result_LeaderMVP;

        private DataRow AreaDR;

        private bool isVow = false;
        private bool isExpEvent = false;
        private bool isAutoAddDummy = false;
        private bool hasLastEnemy = false;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.Calc_AreaExpCalc, container, false);

            // Find View & Connect Event

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
            CommanderCostumeBonus = v.FindViewById<EditText>(Resource.Id.CalcAreaExpCommanderCostumeBonus);
            CommanderCostumeBonus.TextChanged += delegate { CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value); };
            ApplyVow = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpVowSelector);
            ApplyVow.CheckedChange += delegate
            {
                isVow = ApplyVow.Checked;
                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            };
            ApplyExpEvent = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpExpEventSelector);
            ApplyExpEvent.CheckedChange += delegate
            {
                isExpEvent = ApplyExpEvent.Checked;
                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            };
            ApplyAutoAddDummy = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpAutoAddDummySelector);
            ApplyAutoAddDummy.CheckedChange += delegate
            {
                isAutoAddDummy = ApplyAutoAddDummy.Checked;
                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            };
            LastEnemy = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpLastEnemySelector);
            LastEnemy.CheckedChange += delegate
            {
                hasLastEnemy = LastEnemy.Checked;
                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            };
            Result_Normal = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_Normal);
            Result_Leader = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_Leader);
            Result_MVP = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_MVP);
            Result_LeaderMVP = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_Leader_MVP);

            // Set Option Icon Size

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

            _ = InitializeProcess();

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
                ETC.LogError(ex, Activity);
            }
        }

        private async Task InitializeProcess()
        {
            await Task.Delay(10);

            // Make Area List & Set List Adapter

            List<string> areaList = new List<string>();

            foreach (DataRow dr in ETC.FreeOPList.Rows)
                areaList.Add((string)dr["Location"]);

            var adapter = new ArrayAdapter(Activity, Resource.Layout.SpinnerListLayout, areaList);
            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
            AreaList.Adapter = adapter;

            // Set Init Value

            Result_Normal.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultNormalResultText)} => 0";
            Result_Leader.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderResultText)} => 0";
            Result_MVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultMVPResultText)} => 0";
            Result_LeaderMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderMVPResultText)} => 0";

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
                            if (e.NewVal < 10)
                                DollDummy.MaxValue = 1;
                            else if (e.NewVal < 30)
                                DollDummy.MaxValue = 2;
                            else if (e.NewVal < 70)
                                DollDummy.MaxValue = 3;
                            else if (e.NewVal < 90)
                                DollDummy.MaxValue = 4;
                        }
                        else
                        {
                            if (e.NewVal >= 90)
                                DollDummy.MaxValue = 5;
                            else if (e.NewVal >= 70)
                                DollDummy.MaxValue = 4;
                            else if (e.NewVal >= 30)
                                DollDummy.MaxValue = 3;
                            else if (e.NewVal >= 10)
                                DollDummy.MaxValue = 2;
                        }
                        break;
                }

                CalcCount(NowLevel.Value, TargetLevel.Value, DollDummy.Value, WarCount.Value);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
        }

        /// <summary>
        /// Calculate Count of Clear Area. This method contains all case of MVP and Leader option.
        /// </summary>
        /// <param name="start">Set start level of T-Doll</param>
        /// <param name="target">Set target level of T-Doll</param>
        /// <param name="dummy">Set dummy of T-Doll</param>
        /// <param name="warCount">Set combat count in clearing area once</param>
        private void CalcCount(int start, int target, int dummy, int warCount)
        {
            int[] ResultCount = { 0, 0, 0, 0 };

            try
            {
                if (!int.TryParse(NowExp.Text, out int nowExp) || 
                    !int.TryParse(CommanderCostumeBonus.Text, out int commanderCostumeBonus))
                {
                    Result_Normal.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultNormalResultText)} => NaN";
                    Result_Leader.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderResultText)} => NaN";
                    Result_MVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultMVPResultText)} => NaN";
                    Result_LeaderMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderMVPResultText)} => NaN";
                    return;
                }

                int costumeBonus = commanderCostumeBonus + 0;

                ResultCount[0] = CalcTotalCount(start, target, nowExp, dummy, warCount, costumeBonus,  false, false);
                ResultCount[1] = CalcTotalCount(start, target, nowExp, dummy, warCount, costumeBonus, true, false);
                ResultCount[2] = CalcTotalCount(start, target, nowExp, dummy, warCount, costumeBonus, false, true);
                ResultCount[3] = CalcTotalCount(start, target, nowExp, dummy, warCount, costumeBonus, true, true);

                Result_Normal.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultNormalResultText)} => {ResultCount[0]}";
                Result_Leader.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderResultText)} => {ResultCount[1]}";
                Result_MVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultMVPResultText)} => {ResultCount[2]}";
                Result_LeaderMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderMVPResultText)} => {ResultCount[3]}";
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }
        }

        /// <summary>
        /// Calculate Count of each case
        /// </summary>
        /// <param name="nowLevel">Set now level of T-Doll</param>
        /// <param name="targetLevel">Set target level of T-Doll</param>
        /// <param name="nowExp">Set now exp of T-Doll</param>
        /// <param name="dummy">Set dummy of T-Doll</param>
        /// <param name="warCount">Set combat count in clearing area once</param>
        /// <param name="costumeBonus">Set costume bonus percentage</param>
        /// <param name="isLeader">Set leader option</param>
        /// <param name="isMVP">Set MVP option</param>
        /// <returns></returns>
        private int CalcTotalCount(int nowLevel, int targetLevel, int nowExp, int dummy, int warCount, int costumeBonus = 0, bool isLeader = false, bool isMVP = false)
        {
            try
            {
                const double leaderRate = 1.2;
                const double mvpRate = 1.3;
                const double vowRate = 2.0;
                const double expEventRate = 1.5;
                int paneltyLevel = (int)AreaDR["PaneltyLevel"];
                int targetExp = LevelExp[targetLevel - 1];
                double earnExp = 0;
                int totalCount = 0;

                while (nowExp < targetExp)
                {
                    while ((nowExp < LevelExp[nowLevel - 1]) || (nowExp >= LevelExp[nowLevel]))
                        nowLevel += 1;

                    if (isAutoAddDummy)
                    {
                        if ((nowLevel >= 1) && (nowLevel < 10))
                            dummy = 1;
                        else if ((nowLevel >= 10) && (nowLevel < 30))
                            dummy = 2;
                        else if ((nowLevel >= 30) && (nowLevel < 70))
                            dummy = 3;
                        else if ((nowLevel >= 70) && (nowLevel < 90))
                            dummy = 4;
                        else if (nowLevel >= 90)
                            dummy = 5;
                        else
                            dummy = 1;
                    }

                    if (nowLevel >= (paneltyLevel + 40))
                        earnExp = 10;
                    else
                    {
                        double PaneltyRate = 1;

                        if ((nowLevel >= (paneltyLevel + 1)) && (nowLevel < (paneltyLevel + 10)))
                            PaneltyRate = 0.8;
                        else if ((nowLevel >= (paneltyLevel + 10)) && (nowLevel < (paneltyLevel + 20)))
                            PaneltyRate = 0.6;
                        else if ((nowLevel >= (paneltyLevel + 20)) && (nowLevel < (paneltyLevel + 30)))
                            PaneltyRate = 0.4;
                        else if ((nowLevel >= (paneltyLevel + 30)) && (nowLevel < (paneltyLevel + 40)))
                            PaneltyRate = 0.2;

                        earnExp = (int)AreaDR["EXP"];

                        earnExp *= PaneltyRate;
                    }

                    earnExp *= 1 + (0.5 * (dummy - 1));

                    earnExp = isLeader ? earnExp * leaderRate : earnExp;
                    earnExp = isMVP ? earnExp * mvpRate : earnExp;
                    earnExp = isVow ? earnExp * vowRate : earnExp;
                    earnExp = isExpEvent ? earnExp * expEventRate : earnExp;
                    earnExp = costumeBonus != 0 ? earnExp * (1 + (0.01 * costumeBonus)) : earnExp;

                    nowExp += hasLastEnemy ? Convert.ToInt32(Math.Ceiling(earnExp * (warCount - 1)) + earnExp * 2) : Convert.ToInt32(Math.Ceiling(earnExp * warCount));

                    totalCount += 1;
                }

                return totalCount;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }

            return -1;
        }
    }
}