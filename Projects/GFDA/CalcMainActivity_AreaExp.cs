using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace GFDA
{
    public class AreaExpCalc : AndroidX.Fragment.App.Fragment
    {
        private View v;

        // T-Doll Require Exp Between Level
        private readonly int[] LevelExp = { 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500, 5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000, 21000, 23100, 25300, 27600, 30000, 32500, 35100, 37900, 41000, 44400, 48600, 53200, 58200, 63600, 69400, 75700, 82400, 89600, 97300, 105500, 114300, 123600, 133500, 144000, 155100, 166900, 179400, 192500, 206400, 221000, 236400, 252500, 269400, 287100, 305700, 325200, 345600, 366900, 389200, 412500, 436800, 462100, 488400, 515800, 544300, 573900, 604700, 636700, 669900, 704300, 749400, 796200, 844800, 895200, 947400, 1001400, 1057300, 1115200, 1175000, 1236800, 1300700, 1366700, 1434800, 1505100, 1577700, 1652500, 1729600, 1809100, 1891000, 1975300, 2087900, 2204000, 2323500, 2446600, 2573300, 2703700, 2837800, 2975700, 3117500, 3263200, 3363200, 3483200, 3623200, 3783200, 3963200, 4163200, 4383200, 4623200, 4903200, 5263200, 5743200, 6383200, 7283200, 8483200, 10083200, 12283200, 15283200, 19283200, 24283200, 30283200 };

        private Spinner areaList;
        private ToggleButton applyVow;
        private ToggleButton applyExpEvent;
        private ToggleButton applyAutoAddDummy;
        private ToggleButton lastEnemy;
        private NumberPicker nowLevel;
        private NumberPicker targetLevel;
        private NumberPicker dollDummy;
        private NumberPicker warCount;
        private EditText nowExpEditText;
        private EditText commanderCostumeBonusEditText;
        private TextView resultNormal;
        private TextView resultLeader;
        private TextView resultMVP;
        private TextView resultLeaderMVP;

        private DataRow AreaDR;

        private bool isVow = false;
        private bool isExpEvent = false;
        private bool isAutoAddDummy = false;
        private bool hasLastEnemy = false;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater?.Inflate(Resource.Layout.Calc_AreaExpCalc, container, false);

            // Find View & Connect Event

            areaList = v.FindViewById<Spinner>(Resource.Id.CalcAreaSelector);
            areaList.ItemSelected += AreaList_ItemSelected;
            nowLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcAreaExpNowLevel);
            nowLevel.ValueChanged += LevelSelector_ValueChanged;
            targetLevel = v.FindViewById<NumberPicker>(Resource.Id.CalcAreaExpTargetLevel);
            targetLevel.ValueChanged += LevelSelector_ValueChanged;
            dollDummy = v.FindViewById<NumberPicker>(Resource.Id.CalcAreaExpDollDummy);
            dollDummy.ValueChanged += LevelSelector_ValueChanged;
            warCount = v.FindViewById<NumberPicker>(Resource.Id.CalcAreaExpWarCount);
            warCount.ValueChanged += LevelSelector_ValueChanged;
            nowExpEditText = v.FindViewById<EditText>(Resource.Id.CalcAreaExpNowExp);
            nowExpEditText.TextChanged += delegate { CalcCount(nowLevel.Value, targetLevel.Value, dollDummy.Value, warCount.Value); };
            commanderCostumeBonusEditText = v.FindViewById<EditText>(Resource.Id.CalcAreaExpCommanderCostumeBonus);
            commanderCostumeBonusEditText.TextChanged += delegate { CalcCount(nowLevel.Value, targetLevel.Value, dollDummy.Value, warCount.Value); };
            applyVow = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpVowSelector);
            applyVow.CheckedChange += delegate
            {
                isVow = applyVow.Checked;
                CalcCount(nowLevel.Value, targetLevel.Value, dollDummy.Value, warCount.Value);
            };
            applyExpEvent = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpExpEventSelector);
            applyExpEvent.CheckedChange += delegate
            {
                isExpEvent = applyExpEvent.Checked;
                CalcCount(nowLevel.Value, targetLevel.Value, dollDummy.Value, warCount.Value);
            };
            applyAutoAddDummy = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpAutoAddDummySelector);
            applyAutoAddDummy.CheckedChange += delegate
            {
                isAutoAddDummy = applyAutoAddDummy.Checked;
                CalcCount(nowLevel.Value, targetLevel.Value, dollDummy.Value, warCount.Value);
            };
            lastEnemy = v.FindViewById<ToggleButton>(Resource.Id.CalcAreaExpLastEnemySelector);
            lastEnemy.CheckedChange += delegate
            {
                hasLastEnemy = lastEnemy.Checked;
                CalcCount(nowLevel.Value, targetLevel.Value, dollDummy.Value, warCount.Value);
            };
            resultNormal = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_Normal);
            resultLeader = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_Leader);
            resultMVP = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_MVP);
            resultLeaderMVP = v.FindViewById<TextView>(Resource.Id.CalcAreaExpResult_Leader_MVP);

            // Set Option Icon Size

            DisplayMetrics dm = Context.Resources.DisplayMetrics;
            int width = dm.WidthPixels / 2;

            applyVow.LayoutParameters.Width = width;
            applyVow.LayoutParameters.Height = width;
            applyExpEvent.LayoutParameters.Width = width;
            applyExpEvent.LayoutParameters.Height = width;
            applyAutoAddDummy.LayoutParameters.Width = width;
            applyAutoAddDummy.LayoutParameters.Height = width;
            lastEnemy.LayoutParameters.Width = width;
            lastEnemy.LayoutParameters.Height = width;

            _ = InitializeProcess();

            return v;
        }

        private void AreaList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                AreaDR = ETC.freeOPList.Rows[e.Position];
                CalcCount(nowLevel.Value, targetLevel.Value, dollDummy.Value, warCount.Value);
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

            foreach (DataRow dr in ETC.freeOPList.Rows)
            {
                areaList.Add((string)dr["Location"]);
            }

            var adapter = new ArrayAdapter(Activity, Resource.Layout.SpinnerListLayout, areaList);
            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);
            this.areaList.Adapter = adapter;

            // Set Init Value

            resultNormal.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultNormalResultText)} => 0";
            resultLeader.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderResultText)} => 0";
            resultMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultMVPResultText)} => 0";
            resultLeaderMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderMVPResultText)} => 0";

            applyVow.Checked = false;

            nowLevel.MinValue = 1;
            nowLevel.MaxValue = 120;
            nowLevel.Value = 1;
            targetLevel.MinValue = 1;
            targetLevel.MaxValue = 120;
            targetLevel.Value = 1;
            dollDummy.MinValue = 1;
            dollDummy.MaxValue = 1;
            dollDummy.Value = 1;
            warCount.MinValue = 1;
            warCount.MaxValue = 20;
            warCount.Value = 1;
        }

        private void LevelSelector_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            NumberPicker np = sender as NumberPicker;

            try
            {
                switch (np.Id)
                {
                    case Resource.Id.CalcAreaExpNowLevel:
                        targetLevel.MinValue = e.NewVal;
                        nowExpEditText.Text = LevelExp[np.Value - 1].ToString();

                        if (e.NewVal < e.OldVal)
                        {
                            if (e.NewVal < 10)
                                dollDummy.MaxValue = 1;
                            else if (e.NewVal < 30)
                                dollDummy.MaxValue = 2;
                            else if (e.NewVal < 70)
                                dollDummy.MaxValue = 3;
                            else if (e.NewVal < 90)
                                dollDummy.MaxValue = 4;
                        }
                        else
                        {
                            if (e.NewVal >= 90)
                                dollDummy.MaxValue = 5;
                            else if (e.NewVal >= 70)
                                dollDummy.MaxValue = 4;
                            else if (e.NewVal >= 30)
                                dollDummy.MaxValue = 3;
                            else if (e.NewVal >= 10)
                                dollDummy.MaxValue = 2;
                        }
                        break;
                }

                CalcCount(nowLevel.Value, targetLevel.Value, dollDummy.Value, warCount.Value);
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
            int[] resultCount = { 0, 0, 0, 0 };

            try
            {
                if (!int.TryParse(nowExpEditText.Text, out int nowExp) || 
                    !int.TryParse(commanderCostumeBonusEditText.Text, out int commanderCostumeBonus))
                {
                    resultNormal.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultNormalResultText)} => NaN";
                    resultLeader.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderResultText)} => NaN";
                    resultMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultMVPResultText)} => NaN";
                    resultLeaderMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderMVPResultText)} => NaN";
                    return;
                }

                int costumeBonus = commanderCostumeBonus + 0;

                resultCount[0] = CalcTotalCount(start, target, nowExp, dummy, warCount, costumeBonus,  false, false);
                resultCount[1] = CalcTotalCount(start, target, nowExp, dummy, warCount, costumeBonus, true, false);
                resultCount[2] = CalcTotalCount(start, target, nowExp, dummy, warCount, costumeBonus, false, true);
                resultCount[3] = CalcTotalCount(start, target, nowExp, dummy, warCount, costumeBonus, true, true);

                resultNormal.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultNormalResultText)} => {resultCount[0]}";
                resultLeader.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderResultText)} => {resultCount[1]}";
                resultMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultMVPResultText)} => {resultCount[2]}";
                resultLeaderMVP.Text = $"{Resources.GetString(Resource.String.Calc_AreaExp_DefaultLeaderMVPResultText)} => {resultCount[3]}";
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