﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private AdvanceType Adv_Type = AdvanceType._1;

        private CoordinatorLayout SnackbarLayout = null;

        private NumberPicker[] ManPower_NPs = new NumberPicker[4];
        private NumberPicker[] Ammo_NPs = new NumberPicker[4];
        private NumberPicker[] Food_NPs = new NumberPicker[4];
        private NumberPicker[] Parts_NPs = new NumberPicker[4];

        private RadioButton[] Adv_Type_RBs = new RadioButton[3];

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

        private readonly int[] Adv_Type_Ids =
        {
            Resource.Id.PSAdvanceProductType1,
            Resource.Id.PSAdvanceProductType2,
            Resource.Id.PSAdvanceProductType3
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.ProductSimulatorLayout);

            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.ProductSimulatorSnackbarLayout);

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

            if (Type == ProductType.Advance)
            {
                for (int i = 0; i < Adv_Type_RBs.Length; ++i)
                {
                    Adv_Type_RBs[i] = FindViewById<RadioButton>(Adv_Type_Ids[i]);
                    Adv_Type_RBs[i].CheckedChange += Adv_Type_RBs_CheckedChange;
                }
                Adv_Type_RBs[0].Checked = true;
                Adv_Type_RBs[1].Checked = false;
                Adv_Type_RBs[2].Checked = false;
            }
            else FindViewById<LinearLayout>(Resource.Id.PSAdvanceProductTypeLayout).Visibility = ViewStates.Gone;

            InitNumberPickerRange();

            if ((Category != ProductCategory.Doll) || (Type != ProductType.Advance))
            {
                ManPower_NPs[0].ValueChanged += ResourceValueNP_ValueChanged;
                Ammo_NPs[0].ValueChanged += ResourceValueNP_ValueChanged;
                Food_NPs[0].ValueChanged += ResourceValueNP_ValueChanged;
                Parts_NPs[0].ValueChanged += ResourceValueNP_ValueChanged;
            }

            FindViewById<Button>(Resource.Id.PSProductStart).Click += ProductStartButton_Click;
        }

        private void Adv_Type_RBs_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            try
            {
                if (e.IsChecked == false) return;
                int test = 1;
                switch (rb.Id)
                {
                    case Resource.Id.PSAdvanceProductType1:
                        Adv_Type = AdvanceType._1;
                        test = 1;
                        break;
                    case Resource.Id.PSAdvanceProductType2:
                        Adv_Type = AdvanceType._2;
                        test = 2;
                        break;
                    case Resource.Id.PSAdvanceProductType3:
                        Adv_Type = AdvanceType._3;
                        test = 3;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void ProductStartButton_Click(object sender, EventArgs e)
        {
            try
            {
                switch (Category)
                {
                    case ProductCategory.Doll:
                        ListProductAvailable();
                        break;
                    case ProductCategory.Equip:
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ProductSimulatorActivity_ProductStartError, Snackbar.LengthShort);
            }
        }

        private void ResourceValueNP_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            try
            {
                NumberPicker np = sender as NumberPicker;
                bool IsZero = false;
                int minValue = 0;

                if (e.NewVal == 0) IsZero = true;

                if (IsZero == false) minValue = 0;
                else if (Category == ProductCategory.Doll)
                {
                    switch (Type)
                    {
                        case ProductType.Normal:
                            minValue = 3;
                            break;
                        case ProductType.Advance:
                            return;
                    }
                }
                else if (Category == ProductCategory.Equip)
                {
                    switch (Type)
                    {
                        case ProductType.Normal:
                            minValue = 1;
                            break;
                        case ProductType.Advance:
                            minValue = 5;
                            break;
                    }
                }

                switch (np.Id)
                {
                    case Resource.Id.PSManPowerNumberPicker1:
                        ManPower_NPs[1].MinValue = minValue;
                        break;
                    case Resource.Id.PSAmmoNumberPicker1:
                        Ammo_NPs[1].MinValue = minValue;
                        break;
                    case Resource.Id.PSFoodNumberPicker1:
                        Food_NPs[1].MinValue = minValue;
                        break;
                    case Resource.Id.PSPartsNumberPicker1:
                        Parts_NPs[1].MinValue = minValue;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void InitNumberPickerRange()
        {
            try
            {
                int init_min = 0;
                int init_max = 9;
                int init_value = 0;

                for (int i = 0; i < 4; ++i)
                {
                    ManPower_NPs[i].MinValue = init_min;
                    ManPower_NPs[i].MaxValue = init_max;
                    ManPower_NPs[i].Value = init_value;

                    Ammo_NPs[i].MinValue = init_min;
                    Ammo_NPs[i].MaxValue = init_max;
                    Ammo_NPs[i].Value = init_value;

                    Food_NPs[i].MinValue = init_min;
                    Food_NPs[i].MaxValue = init_max;
                    Food_NPs[i].Value = init_value;

                    Parts_NPs[i].MinValue = init_min;
                    Parts_NPs[i].MaxValue = init_max;
                    Parts_NPs[i].Value = init_value;
                }

                if (Category == ProductCategory.Doll)
                {
                    switch (Type)
                    {
                        case ProductType.Normal:
                            ManPower_NPs[1].MinValue = 3;
                            Ammo_NPs[1].MinValue = 3;
                            Food_NPs[1].MinValue = 3;
                            Parts_NPs[1].MinValue = 3;
                            break;
                        case ProductType.Advance:
                            ManPower_NPs[0].MinValue = 1;
                            Ammo_NPs[0].MinValue = 1;
                            Food_NPs[0].MinValue = 1;
                            Parts_NPs[0].MinValue = 1;
                            break;
                    }
                }
                else if (Category == ProductCategory.Equip)
                {
                    switch (Type)
                    {
                        case ProductType.Normal:
                            ManPower_NPs[1].MinValue = 1;
                            Ammo_NPs[1].MinValue = 1;
                            Food_NPs[1].MinValue = 1;
                            Parts_NPs[1].MinValue = 1;

                            ManPower_NPs[0].MaxValue = 3;
                            Ammo_NPs[0].MaxValue = 3;
                            Food_NPs[0].MaxValue = 3;
                            Parts_NPs[0].MaxValue = 3;
                            break;
                        case ProductType.Advance:
                            ManPower_NPs[1].MinValue = 5;
                            Ammo_NPs[1].MinValue = 5;
                            Food_NPs[1].MinValue = 5;
                            Parts_NPs[1].MinValue = 5;

                            ManPower_NPs[0].MaxValue = 5;
                            Ammo_NPs[0].MaxValue = 5;
                            Food_NPs[0].MaxValue = 5;
                            Parts_NPs[0].MaxValue = 5;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ProductSimulatorActivity_InitRangeSetError, Snackbar.LengthShort);
            }
        }

        private int CalcResource(string ResourceType)
        {
            try
            {
                int[] values = null;
                int result = 0;

                if (Type == ProductType.Normal) values = new int[3];
                else values = new int[4];

                switch (ResourceType)
                {
                    case "ManPower":
                        for (int i = 0; i < values.Length; ++i) values[i] = ManPower_NPs[i].Value;
                        break;
                    case "Ammo":
                        for (int i = 0; i < values.Length; ++i) values[i] = Ammo_NPs[i].Value;
                        break;
                    case "Food":
                        for (int i = 0; i < values.Length; ++i) values[i] = Food_NPs[i].Value;
                        break;
                    case "Parts":
                        for (int i = 0; i < values.Length; ++i) values[i] = Parts_NPs[i].Value;
                        break;
                }

                for (int i = 0; i < values.Length; ++i) result += (values[i] * Convert.ToInt32(Math.Pow(10, (values.Length - (i + 1)))));

                return result;
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ProductSimulatorActivity_CalcResourceError, Snackbar.LengthShort);
                return 0;
            }
        }

        private void ShowResultScreen(DataRow dr)
        {
            try
            {
                if (dr == null)
                {
                    ETC.ShowSnackbar(SnackbarLayout, "Result Null", Snackbar.LengthShort);
                    return;
                }

                StringBuilder sb = new StringBuilder();
                
                switch (Category)
                {
                    case ProductCategory.Doll:
                        sb.Append("Doll");
                        break;
                    case ProductCategory.Equip:
                        if ((int)dr["ProductTime"] <= 60) sb.Append("Equip");
                        else sb.Append("Fairy");
                        break;
                }

                sb.Append(";");
                sb.Append((string)dr["Name"]);

                Intent ResultInfo = new Intent(this, typeof(ProductResultActivity));
                ResultInfo.PutExtra("ResultData", sb.ToString());
                StartActivity(ResultInfo);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ProductSimulatorActivity_StartResultError, Snackbar.LengthShort);
            }
        }

        private void ListProductAvailable()
        {
            try
            {
                List<string> AvailableType = new List<string>();
                List<DataRow> AvailableDoll = new List<DataRow>();

                int pManPower = CalcResource("ManPower");
                int pAmmo = CalcResource("Ammo");
                int pFood = CalcResource("Food");
                int pParts = CalcResource("Parts");

                /*if (ETC.IsShowCensorImage == true)
                {
                    if ((pManPower == 666) && (pAmmo == 666) && (pFood == 666) && (pParts == 666)) File.Create(Path.Combine("System", "Extra", "Censor.extra"));
                }*/

                AvailableType.Add("SMG");
                switch (Type)
                {
                    case ProductType.Normal:
                        if ((pManPower + pAmmo + pFood + pParts) <= 920) AvailableType.Add("HG");
                        if ((pManPower + pAmmo + pFood + pParts) >= 800) AvailableType.Add("AR");
                        if ((pManPower >= 300) && (pFood >= 300)) AvailableType.Add("RF");
                        if ((pManPower >= 400) && (pAmmo >= 600) && (pParts > 300)) AvailableType.Add("MG");
                        break;
                    case ProductType.Advance:
                        if ((pManPower + pAmmo + pFood + pParts) >= 800) AvailableType.Add("AR");
                        if ((pManPower >= 3000) && (pFood >= 3000)) AvailableType.Add("RF");
                        if ((pManPower >= 4000) && (pAmmo >= 6000) && (pParts >= 3000)) AvailableType.Add("MG");
                        if ((pManPower >= 4000) && (pFood >= 6000) && (pParts >= 3000)) AvailableType.Add("SG");
                        break;
                }

                AvailableType.TrimExcess();

                for (int i = 0; i < ETC.DollList.Rows.Count; ++i)
                {
                    DataRow dr = ETC.DollList.Rows[i];
                    string d_name = (string)dr["Name"];

                    if (AvailableType.Contains((string)dr["Type"]) == false) continue;

                    string[] drop = ((string)dr["DropEvent"]).Split(',');
                    if ((Type == ProductType.Normal) && (drop[0] == "중형제조")) continue;
                    if ((int)dr["ProductTime"] == 0) continue;

                    switch ((string)dr["Type"])
                    {
                        case "SMG":
                            string[] list_smg = { "G36C", "벡터", "79식", "수오미", "SR-3MP", "C-MS", "UMP9", "UMP45", "PP-90", "시프카", "PP-19-01", "스텐 Mk.II" };

                            if ((list_smg.Contains(d_name) == true) && (((Type == ProductType.Normal) && (pManPower >= 400) && (pAmmo >= 400) && (pFood >= 30) && (pParts >= 30)) || ((Type == ProductType.Advance) && (pManPower >= 4000) && (pAmmo >= 4000) && (pFood >= 1000) && (pParts >= 1000)))) AvailableDoll.Add(dr);
                            else if (list_smg.Contains(d_name) == false) AvailableDoll.Add(dr);
                            else continue;
                            break;
                        case "HG":
                            if (Type == ProductType.Normal)
                            {
                                string[] list_hg = { "M950A", "웰로드 Mk.II", "컨텐더", "스테츠킨", "P7", "Spitfire", "K5" };

                                if ((list_hg.Contains(d_name) == true) && (pManPower >= 130) && (pAmmo >= 130) && (pFood >= 130) && (pParts >= 30)) AvailableDoll.Add(dr);
                                else if (list_hg.Contains(d_name) == false) AvailableDoll.Add(dr);
                                else continue;
                            }
                            break;
                        case "AR":
                            string[] list_ar = { "G41", "FAL", "95식", "97식", "RFB", "T91", "K2", "MDR", "Zas M21", "AN-94", "AK-12", "TAR-21", "G36", "리베롤" };

                            if ((list_ar.Contains(d_name) == true) && (((Type == ProductType.Normal) && (pManPower >= 30) && (pAmmo >= 400) && (pFood >= 400) && (pParts >= 30)) || ((Type == ProductType.Advance) && (pManPower >= 1000) && (pAmmo >= 4000) && (pFood >= 4000) && (pParts >= 1000)))) AvailableDoll.Add(dr);
                            else if (list_ar.Contains(d_name) == false) AvailableDoll.Add(dr);
                            else continue;
                            break;
                        case "RF":
                            string[] list_rf = { "Kar98k", "리엔필드", "M99", "IWS2000", "카르카노 M1938", "SVD", "T-5000", "한양조88식" };

                            if ((list_rf.Contains(d_name) == true) && (((Type == ProductType.Normal) && (pManPower >= 400) && (pAmmo >= 30) && (pFood >= 400) && (pParts >= 30)) || ((Type == ProductType.Advance) && (pManPower >= 4000) && (pAmmo >= 1000) && (pFood >= 4000) && (pParts >= 1000)))) AvailableDoll.Add(dr);
                            else if (list_rf.Contains(d_name) == false) AvailableDoll.Add(dr);
                            else continue;
                            break;
                        case "MG":
                            string[] list_mg = { "네게브", "MG4", "PKP", "PK" };

                            if ((list_mg.Contains(d_name) == true) && (((Type == ProductType.Normal) && (pManPower >= 600) && (pAmmo >= 600) && (pFood >= 100) && (pParts >= 400)) || ((Type == ProductType.Advance) && (pManPower >= 6000) && (pAmmo >= 6000) && (pFood >= 1000) && (pParts >= 4000)))) AvailableDoll.Add(dr);
                            else if (list_mg.Contains(d_name) == false) AvailableDoll.Contains(dr);
                            else continue;
                            break;
                        case "SG":
                            if (Type == ProductType.Advance)
                            {
                                string[] list_sg = { "Saiga-12", "S.A.T.8", "M37", "Super-Shorty", "RMB", "M1897" };

                                if ((list_sg.Contains(d_name) == true) && ((pManPower >= 6000) && (pAmmo >= 1000) && (pFood >= 6000) && (pParts >= 4000))) AvailableDoll.Add(dr);
                                else if (list_sg.Contains(d_name) == false) AvailableDoll.Add(dr);
                                else continue;
                            }
                            break;
                    }
                }

                AvailableDoll.TrimExcess();
                ProductProcess_Doll(AvailableDoll, pManPower, pAmmo, pFood, pParts);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ProductSimulatorActivity_GroupDollError, Snackbar.LengthShort);
            }
        }

        private void ProductProcess_Doll(List<DataRow> AvailableDoll, int num1, int num2, int num3, int num4)
        {
            try
            {
                Random R = new Random(DateTime.Now.Millisecond);
                int seed_num1 = (num1 + num2 + num3 + num4) / (AvailableDoll.Count / 10);
                int seed_num2 = num1 + num2 + num3 + num4;

                int[] tP = { 60, 27, 10, 3 };
                int[] tAP1 = { 40, 45, 15 };
                int[] tAP2 = { 20, 60, 20 };
                int[] tAP3 = { 0, 75, 25 };
                int ConfirmGrade = 0;

                int mag = 1;
                int[] P = null;

                switch (Type)
                {
                    case ProductType.Normal:
                        P = new int[4];
                        for (int k = 0; k < tP.Length; ++k) P[k] = tP[k] * mag;
                        break;
                    case ProductType.Advance:
                        P = new int[3];
                        switch (Adv_Type)
                        {
                            case AdvanceType._1:
                                for (int k = 0; k < tAP1.Length; ++k) P[k] = tAP1[k] * mag;
                                break;
                            case AdvanceType._2:
                                for (int k = 0; k < tAP2.Length; ++k) P[k] = tAP2[k] * mag;
                                break;
                            case AdvanceType._3:
                                for (int k = 0; k < tAP3.Length; ++k) P[k] = tAP3[k] * mag;
                                break;
                        }
                        break;
                }

                int num = R.Next(seed_num1, seed_num2 * mag) % (100 * mag);

                switch (Type)
                {
                    case ProductType.Normal:
                        if ((num >= 0) && (num < P[0])) ConfirmGrade = 2;
                        else if ((num >= P[0]) && (num < (P[0] + P[1]))) ConfirmGrade = 3;
                        else if ((num >= (P[0] + P[1])) && (num >= (P[0] + P[1] + P[2]))) ConfirmGrade = 4;
                        else ConfirmGrade = 5;
                        break;
                    case ProductType.Advance:
                        if ((num >= 0) && (num < P[0])) ConfirmGrade = 3;
                        else if ((num >= P[0]) && (num < (P[0] + P[1]))) ConfirmGrade = 4;
                        else ConfirmGrade = 5;
                        break;
                }


                if (ConfirmGrade == 0)
                {
                    if (Type == ProductType.Normal) ConfirmGrade = 2;
                    else if ((Type == ProductType.Advance) && (Adv_Type == AdvanceType._3)) ConfirmGrade = 4;
                    else ConfirmGrade = 3;
                }

                List<DataRow> FinalDoll = new List<DataRow>();
                int tArrange = 0;

                foreach (DataRow dr in AvailableDoll)
                {
                    if ((int)dr["Grade"] == ConfirmGrade)
                    {
                        FinalDoll.Add(dr);

                        int tcount = 0;
                        if (((Type == ProductType.Normal) && (dr["ProductionPercent"] == DBNull.Value)) ||  ((Type == ProductType.Advance) && (dr["AdvanceProductionPercent"] == DBNull.Value))) tcount = 100;
                        else
                        {
                            if (Type == ProductType.Normal) tcount += (int)dr["ProductionPercent"];
                            else tcount += (int)dr["AdvanceProductionPercent"];
                        }

                        tArrange += tcount;
                    }
                }

                FinalDoll.TrimExcess();

                int fnum = R.Next(1, tArrange);
                int count = 0;
                DataRow ResultDoll = null;

                foreach (DataRow dr in FinalDoll)
                {
                    int count2 = count;

                    if (((Type == ProductType.Normal) && (dr["ProductionPercent"] == DBNull.Value)) || ((Type == ProductType.Advance) && (dr["AdvanceProductionPercent"] == DBNull.Value))) count2 += 100;
                    else
                    {
                        if (Type == ProductType.Normal) count2 += (int)dr["ProductionPercent"];
                        else count2 += (int)dr["AdvanceProductionPercent"];
                    }

                    if ((fnum > count) && (fnum <= count2))
                    {
                        ResultDoll = dr;
                        break;
                    }

                    count = count2;
                }

                ShowResultScreen(ResultDoll);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.ProductSimulatorActivity_ProductDollError, Snackbar.LengthShort);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }
    }
}