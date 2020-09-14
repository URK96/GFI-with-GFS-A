using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;

using Google.Android.Material.Snackbar;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GFDA
{
    [Activity(Label = "ProductSimulatorActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ProductSimulatorActivity : BaseAppCompatActivity
    {
        enum ProductCategory { Doll, Equip }
        enum ProductType { Normal, Advance }
        enum AdvanceType { _1, _2, _3 }

        private ProductCategory category;
        private ProductType type;
        private AdvanceType typeAdvance = AdvanceType._1;

        private int pCount = 0;

        private CoordinatorLayout snackbarLayout;

        private NumberPicker[] manPowerNPs = new NumberPicker[4];
        private NumberPicker[] ammoNPs = new NumberPicker[4];
        private NumberPicker[] foodNPs = new NumberPicker[4];
        private NumberPicker[] partsNPs = new NumberPicker[4];
        private NumberPicker productCount;

        private RadioButton[] advanceTypeRBs = new RadioButton[3];

        private readonly int[] manPowerNPIds =
        {
            Resource.Id.PSManPowerNumberPicker1,
            Resource.Id.PSManPowerNumberPicker2,
            Resource.Id.PSManPowerNumberPicker3,
            Resource.Id.PSManPowerNumberPicker4
        };

        private readonly int[] ammoNPIds =
        {
            Resource.Id.PSAmmoNumberPicker1,
            Resource.Id.PSAmmoNumberPicker2,
            Resource.Id.PSAmmoNumberPicker3,
            Resource.Id.PSAmmoNumberPicker4
        };

        private readonly int[] foodNPIds =
        {
            Resource.Id.PSFoodNumberPicker1,
            Resource.Id.PSFoodNumberPicker2,
            Resource.Id.PSFoodNumberPicker3,
            Resource.Id.PSFoodNumberPicker4
        };

        private readonly int[] partsNPIds =
        {
            Resource.Id.PSPartsNumberPicker1,
            Resource.Id.PSPartsNumberPicker2,
            Resource.Id.PSPartsNumberPicker3,
            Resource.Id.PSPartsNumberPicker4
        };

        private readonly int[] advTypeIds =
        {
            Resource.Id.PSAdvanceProductType1,
            Resource.Id.PSAdvanceProductType2,
            Resource.Id.PSAdvanceProductType3
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.ProductSimulatorLayout);

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.ProductSimulatorMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.ProductSimulatorSnackbarLayout);

            string[] infoData = Intent.GetStringExtra("Info").Split('/'); // index 0 : Product Category, index 1 : Product Type

            switch (infoData[0])
            {
                default:
                case "Doll":
                    category = ProductCategory.Doll;
                    ETC.LoadDBSync(ETC.dollList, Path.Combine(ETC.dbPath, "Doll.gfs"), true);
                    break;
                case "Equip":
                    category = ProductCategory.Equip;
                    ETC.LoadDBSync(ETC.equipmentList, Path.Combine(ETC.dbPath, "Equipment.gfs"), true);
                    ETC.LoadDBSync(ETC.fairyList, Path.Combine(ETC.dbPath, "Fairy.gfs"), true);
                    break;
            }

            InitNumberPickerRange();

            switch (infoData[1])
            {
                default:
                case "Normal":
                    type = ProductType.Normal;
                    manPowerNPs[3].Visibility = ViewStates.Gone;
                    ammoNPs[3].Visibility = ViewStates.Gone;
                    foodNPs[3].Visibility = ViewStates.Gone;
                    partsNPs[3].Visibility = ViewStates.Gone;
                    break;
                case "Advance":
                    type = ProductType.Advance;
                    break;
            }

            switch (category)
            {
                case ProductCategory.Doll when type == ProductType.Normal:
                    FindViewById<TextView>(Resource.Id.ProductSimulatorToolbarType).SetText(Resource.String.ProductSimulatorCategoryActivity_DollNormalProduct);
                    break;
                case ProductCategory.Doll when type == ProductType.Advance:
                    FindViewById<TextView>(Resource.Id.ProductSimulatorToolbarType).SetText(Resource.String.ProductSimulatorCategoryActivity_DollAdvancedProduct);
                    break;
                case ProductCategory.Equip when type == ProductType.Normal:
                    FindViewById<TextView>(Resource.Id.ProductSimulatorToolbarType).SetText(Resource.String.ProductSimulatorCategoryActivity_EquipNormalProduct);
                    break;
                case ProductCategory.Equip when type == ProductType.Advance:
                    FindViewById<TextView>(Resource.Id.ProductSimulatorToolbarType).SetText(Resource.String.ProductSimulatorCategoryActivity_EquipAdvancedProduct);
                    break;
            }

            if (type == ProductType.Advance)
            {
                for (int i = 0; i < advanceTypeRBs.Length; ++i)
                {
                    advanceTypeRBs[i] = FindViewById<RadioButton>(advTypeIds[i]);
                    advanceTypeRBs[i].CheckedChange += AdvanceTypeRBs_CheckedChange;
                }

                advanceTypeRBs[0].Checked = true;
                advanceTypeRBs[1].Checked = false;
                advanceTypeRBs[2].Checked = false;
            }
            else
            {
                FindViewById<LinearLayout>(Resource.Id.PSAdvanceProductTypeLayout).Visibility = ViewStates.Gone;
            }

            if ((category != ProductCategory.Doll) || (type != ProductType.Advance))
            {
                manPowerNPs[0].ValueChanged += ResourceValueNP_ValueChanged;
                ammoNPs[0].ValueChanged += ResourceValueNP_ValueChanged;
                foodNPs[0].ValueChanged += ResourceValueNP_ValueChanged;
                partsNPs[0].ValueChanged += ResourceValueNP_ValueChanged;
            }

            productCount = FindViewById<NumberPicker>(Resource.Id.PSCountNumberPicker);
            productCount.MinValue = 1;
            productCount.MaxValue = 10;
            productCount.Value = 1;
            FindViewById<Button>(Resource.Id.PSProductStart).Click += ProductStartButton_Click;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void AdvanceTypeRBs_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                if (!e.IsChecked)
                {
                    return;
                }

                switch ((sender as RadioButton).Id)
                {
                    case Resource.Id.PSAdvanceProductType1:
                        typeAdvance = AdvanceType._1;
                        break;
                    case Resource.Id.PSAdvanceProductType2:
                        typeAdvance = AdvanceType._2;
                        break;
                    case Resource.Id.PSAdvanceProductType3:
                        typeAdvance = AdvanceType._3;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private void ProductStartButton_Click(object sender, EventArgs e)
        {
            try
            {
                pCount = productCount.Value;

                switch (category)
                {
                    case ProductCategory.Doll:
                        ListProductAvailableDoll();
                        break;
                    case ProductCategory.Equip:
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ProductSimulatorActivity_ProductStartError, Snackbar.LengthShort);
            }
        }

        private void ResourceValueNP_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            try
            {
                NumberPicker np = sender as NumberPicker;
                bool IsZero = (e.NewVal == 0);
                int minValue = 0;

                if (!IsZero)
                {
                    minValue = 0;
                }
                else if (category == ProductCategory.Doll)
                {
                    switch (type)
                    {
                        case ProductType.Normal:
                            minValue = 3;
                            break;
                        case ProductType.Advance:
                            return;
                    }
                }
                else if (category == ProductCategory.Equip)
                {
                    switch (type)
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
                        manPowerNPs[1].MinValue = minValue;
                        break;
                    case Resource.Id.PSAmmoNumberPicker1:
                        ammoNPs[1].MinValue = minValue;
                        break;
                    case Resource.Id.PSFoodNumberPicker1:
                        foodNPs[1].MinValue = minValue;
                        break;
                    case Resource.Id.PSPartsNumberPicker1:
                        partsNPs[1].MinValue = minValue;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private void InitNumberPickerRange()
        {
            try
            {
                for (int i = 0; i < manPowerNPs.Length; ++i)
                {
                    manPowerNPs[i] = FindViewById<NumberPicker>(manPowerNPIds[i]);
                }
                for (int i = 0; i < ammoNPs.Length; ++i)
                {
                    ammoNPs[i] = FindViewById<NumberPicker>(ammoNPIds[i]);
                }
                for (int i = 0; i < foodNPs.Length; ++i)
                {
                    foodNPs[i] = FindViewById<NumberPicker>(foodNPIds[i]);
                }
                for (int i = 0; i < partsNPs.Length; ++i)
                {
                    partsNPs[i] = FindViewById<NumberPicker>(partsNPIds[i]);
                }

                int initMin = 0;
                int initMax = 9;
                int initValue = 0;

                for (int i = 0; i < 4; ++i)
                {
                    manPowerNPs[i].MinValue = initMin;
                    manPowerNPs[i].MaxValue = initMax;
                    manPowerNPs[i].Value = initValue;

                    ammoNPs[i].MinValue = initMin;
                    ammoNPs[i].MaxValue = initMax;
                    ammoNPs[i].Value = initValue;

                    foodNPs[i].MinValue = initMin;
                    foodNPs[i].MaxValue = initMax;
                    foodNPs[i].Value = initValue;

                    partsNPs[i].MinValue = initMin;
                    partsNPs[i].MaxValue = initMax;
                    partsNPs[i].Value = initValue;
                }

                if (category == ProductCategory.Doll)
                {
                    switch (type)
                    {
                        case ProductType.Normal:
                            manPowerNPs[1].MinValue = 3;
                            ammoNPs[1].MinValue = 3;
                            foodNPs[1].MinValue = 3;
                            partsNPs[1].MinValue = 3;
                            break;
                        case ProductType.Advance:
                            manPowerNPs[0].MinValue = 1;
                            ammoNPs[0].MinValue = 1;
                            foodNPs[0].MinValue = 1;
                            partsNPs[0].MinValue = 1;
                            break;
                    }
                }
                else if (category == ProductCategory.Equip)
                {
                    switch (type)
                    {
                        case ProductType.Normal:
                            manPowerNPs[1].MinValue = 1;
                            ammoNPs[1].MinValue = 1;
                            foodNPs[1].MinValue = 1;
                            partsNPs[1].MinValue = 1;

                            manPowerNPs[0].MaxValue = 3;
                            ammoNPs[0].MaxValue = 3;
                            foodNPs[0].MaxValue = 3;
                            partsNPs[0].MaxValue = 3;
                            break;
                        case ProductType.Advance:
                            manPowerNPs[1].MinValue = 5;
                            ammoNPs[1].MinValue = 5;
                            foodNPs[1].MinValue = 5;
                            partsNPs[1].MinValue = 5;

                            manPowerNPs[0].MaxValue = 5;
                            ammoNPs[0].MaxValue = 5;
                            foodNPs[0].MaxValue = 5;
                            partsNPs[0].MaxValue = 5;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ProductSimulatorActivity_InitRangeSetError, Snackbar.LengthShort);
            }
        }

        private int CalcResource(string resourceType)
        {
            try
            {
                int[] values = (type == ProductType.Normal) ? new int[3] : new int[4];
                int result = 0;

                switch (resourceType)
                {
                    case "ManPower":
                        for (int i = 0; i < values.Length; ++i)
                        {
                            values[i] = manPowerNPs[i].Value;
                        }
                        break;
                    case "Ammo":
                        for (int i = 0; i < values.Length; ++i)
                        {
                            values[i] = ammoNPs[i].Value;
                        }
                        break;
                    case "Food":
                        for (int i = 0; i < values.Length; ++i)
                        {
                            values[i] = foodNPs[i].Value;
                        }
                        break;
                    case "Parts":
                        for (int i = 0; i < values.Length; ++i)
                        {
                            values[i] = partsNPs[i].Value;
                        }
                        break;
                }

                for (int i = 0; i < values.Length; ++i)
                {
                    result += (values[i] * Convert.ToInt32(Math.Pow(10, (values.Length - (i + 1)))));
                }

                return result;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ProductSimulatorActivity_CalcResourceError, Snackbar.LengthShort);

                return 0;
            }
        }

        private void ShowResultScreen(int[] resultDicnumbers)
        {
            try
            {
                if (resultDicnumbers.Length == 0)
                {
                    ETC.ShowSnackbar(snackbarLayout, "Result Empty", Snackbar.LengthShort);

                    return;
                }

                string[] type = new string[resultDicnumbers.Length];
                string[] resultNames = new string[resultDicnumbers.Length];

                for (int i = 0; i < resultDicnumbers.Length; ++i)
                {
                    DataRow dr = null;

                    switch (category)
                    {
                        case ProductCategory.Doll:
                            type[i] = "Doll";
                            dr = ETC.FindDataRow(ETC.dollList, "DicNumber", resultDicnumbers[i]);
                            break;
                        case ProductCategory.Equip:
                            if ((int)dr["ProductTime"] <= 60)
                            {
                                type[i] = "Equip";
                                dr = ETC.FindDataRow(ETC.equipmentList, "DicNumber", resultDicnumbers[i]);
                            }
                            else
                            {
                                type[i] = "Fairy";
                                dr = ETC.FindDataRow(ETC.fairyList, "DicNumber", resultDicnumbers[i]);
                            }
                            break;
                    }

                    resultNames[i] = (string)dr["Name"];
                }

                Intent resultInfo = new Intent(this, typeof(ProductResultActivity));
                resultInfo.PutExtra("ResultType", type);
                resultInfo.PutExtra("ResultInfo", resultNames);
                StartActivity(resultInfo);
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ProductSimulatorActivity_StartResultError, Snackbar.LengthShort);
            }
        }

        // Create list of production available T-Doll by resources
        private void ListProductAvailableDoll()
        {
            try
            {
                List<string> availableType = new List<string>();
                List<Doll> availableDoll = new List<Doll>();

                int pManPower = CalcResource("ManPower");
                int pAmmo = CalcResource("Ammo");
                int pFood = CalcResource("Food");
                int pParts = CalcResource("Parts");

                if ((pManPower == 666) && (pAmmo == 666) && (pFood == 666) && (pParts == 666))
                {
                    if (!ETC.sharedPreferences.GetBoolean("ImageCensoredUnlock", false))
                    {
                        var editor = ETC.sharedPreferences.Edit();
                        editor.PutBoolean("ImageCensoredUnlock", true);
                        editor.Commit();

                        Toast.MakeText(this, "Image Censor Option unlock", ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(this, "Image Censor Option already unlock", ToastLength.Short).Show();
                    }
                }

                availableType.Add("SMG");

                switch (type)
                {
                    case ProductType.Normal:
                        if ((pManPower + pAmmo + pFood + pParts) <= 920)
                        {
                            availableType.Add("HG");
                        }
                        if ((pManPower + pAmmo + pFood + pParts) >= 800)
                        {
                            availableType.Add("AR");
                        }
                        if ((pManPower >= 300) && (pFood >= 300))
                        {
                            availableType.Add("RF");
                        }
                        if ((pManPower >= 400) && (pAmmo >= 600) && (pParts > 300))
                        {
                            availableType.Add("MG");
                        }
                        break;
                    case ProductType.Advance:
                        if ((pManPower + pAmmo + pFood + pParts) >= 800)
                        {
                            availableType.Add("AR");
                        }
                        if ((pManPower >= 3000) && (pFood >= 3000))
                        {
                            availableType.Add("RF");
                        }
                        if ((pManPower >= 4000) && (pAmmo >= 6000) && (pParts >= 3000))
                        {
                            availableType.Add("MG");
                        }
                        if ((pManPower >= 4000) && (pFood >= 6000) && (pParts >= 3000))
                        {
                            availableType.Add("SG");
                        }
                        break;
                }

                availableType.TrimExcess();

                for (int i = 0; i < ETC.dollList.Rows.Count; ++i)
                {
                    Doll doll = new Doll(ETC.dollList.Rows[i], true);

                    if (!availableType.Contains(doll.Type) ||
                        ((type == ProductType.Normal) && (doll.DropEvent[0] == "중형제조")) ||
                        (doll.ProductTime == 0))
                    {
                        continue;
                    }

                    switch (doll.Type)
                    {
                        case "SMG":
                            string[] listSMG = { "G36C", "벡터", "79식", "수오미", "SR-3MP", "C-MS", "UMP9", "UMP45", "PP-90", "시프카", "PP-19-01", "스텐 Mk.II" };

                            if (listSMG.Contains(doll.Name) &&
                                (((type == ProductType.Normal) && (pManPower >= 400) && (pAmmo >= 400) && (pFood >= 30) && (pParts >= 30)) ||
                                ((type == ProductType.Advance) && (pManPower >= 4000) && (pAmmo >= 4000) && (pFood >= 1000) && (pParts >= 1000))))
                            {
                                availableDoll.Add(doll);
                            }
                            else if (!listSMG.Contains(doll.Name))
                            {
                                availableDoll.Add(doll);
                            }
                            else
                            {
                                continue;
                            }
                            break;
                        case "HG":
                            if (type == ProductType.Normal)
                            {
                                string[] listHG = { "M950A", "웰로드 Mk.II", "컨텐더", "스테츠킨", "P7", "Spitfire", "K5" };

                                if (listHG.Contains(doll.Name) &&
                                    (pManPower >= 130) && (pAmmo >= 130) && (pFood >= 130) && (pParts >= 30))
                                {
                                    availableDoll.Add(doll);
                                }
                                else if (!listHG.Contains(doll.Name))
                                {
                                    availableDoll.Add(doll);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            break;
                        case "AR":
                            string[] listAR = { "G41", "FAL", "95식", "97식", "RFB", "T91", "K2", "MDR", "Zas M21", "AN-94", "AK-12", "TAR-21", "G36", "리베롤" };

                            if (listAR.Contains(doll.Name)
                                && (((type == ProductType.Normal) && (pManPower >= 30) && (pAmmo >= 400) && (pFood >= 400) && (pParts >= 30)) ||
                                ((type == ProductType.Advance) && (pManPower >= 1000) && (pAmmo >= 4000) && (pFood >= 4000) && (pParts >= 1000))))
                            {
                                availableDoll.Add(doll);
                            }
                            else if (!listAR.Contains(doll.Name))
                            {
                                availableDoll.Add(doll);
                            }
                            else
                            {
                                continue;
                            }
                            break;
                        case "RF":
                            string[] listRF = { "Kar98k", "리엔필드", "M99", "IWS2000", "카르카노 M1938", "SVD", "T-5000", "한양조88식" };

                            if (listRF.Contains(doll.Name) &&
                                (((type == ProductType.Normal) && (pManPower >= 400) && (pAmmo >= 30) && (pFood >= 400) && (pParts >= 30)) ||
                                ((type == ProductType.Advance) && (pManPower >= 4000) && (pAmmo >= 1000) && (pFood >= 4000) && (pParts >= 1000))))
                            {
                                availableDoll.Add(doll);
                            }
                            else if (!listRF.Contains(doll.Name))
                            {
                                availableDoll.Add(doll);
                            }
                            else
                            {
                                continue;
                            }
                            break;
                        case "MG":
                            string[] listMG = { "네게브", "MG4", "PKP", "PK" };

                            if (listMG.Contains(doll.Name) &&
                                (((type == ProductType.Normal) && (pManPower >= 600) && (pAmmo >= 600) && (pFood >= 100) && (pParts >= 400)) ||
                                ((type == ProductType.Advance) && (pManPower >= 6000) && (pAmmo >= 6000) && (pFood >= 1000) && (pParts >= 4000))))
                            {
                                availableDoll.Add(doll);
                            }
                            else if (!listMG.Contains(doll.Name))
                            {
                                availableDoll.Add(doll);
                            }
                            else
                            {
                                continue;
                            }
                            break;
                        case "SG":
                            if (type == ProductType.Advance)
                            {
                                string[] listSG = { "Saiga-12", "S.A.T.8", "M37", "Super-Shorty", "RMB", "M1897" };

                                if (listSG.Contains(doll.Name) &&
                                    (pManPower >= 6000) && (pAmmo >= 1000) && (pFood >= 6000) && (pParts >= 4000))
                                {
                                    availableDoll.Add(doll);
                                }
                                else if (!listSG.Contains(doll.Name))
                                {
                                    availableDoll.Add(doll);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            break;
                    }
                }

                availableDoll.TrimExcess();
                availableDoll.ShuffleList();

                ProductProcessDoll(availableDoll, pManPower, pAmmo, pFood, pParts, pCount);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ProductSimulatorActivity_GroupDollError, Snackbar.LengthShort);
            }
        }


        private void ProductProcessDoll(List<Doll> availableDoll, int num1, int num2, int num3, int num4, int loopCount)
        {
            if (availableDoll.Count == 0)
            {
                Toast.MakeText(this, "Empty Doll List", ToastLength.Short).Show();

                return;
            }

            try
            {
                int[] resultsDicNumber = new int[loopCount];
                
                int[] tP = { 60, 27, 10, 3 };
                int[] tAP1 = { 40, 45, 15 };
                int[] tAP2 = { 20, 60, 20 };
                int[] tAP3 = { 0, 75, 25 };
                int[] P = (type == ProductType.Normal) ? new int[4] : new int[3];

                int confirmGrade = 0;
                int mag = 10;
                int seedNum = (num1 + num2 + num3 + num4) / availableDoll.Count;
                
                for (int i = 0; i < loopCount; ++i)
                {
                    switch (type)
                    {
                        case ProductType.Normal:
                            for (int k = 0; k < tP.Length; ++k)
                            {
                                P[k] = tP[k] * mag;
                            }
                            break;
                        case ProductType.Advance:
                            switch (typeAdvance)
                            {
                                case AdvanceType._1:
                                    for (int k = 0; k < tAP1.Length; ++k)
                                    {
                                        P[k] = tAP1[k] * mag;
                                    }
                                    break;
                                case AdvanceType._2:
                                    for (int k = 0; k < tAP2.Length; ++k)
                                    {
                                        P[k] = tAP2[k] * mag;
                                    }
                                    break;
                                case AdvanceType._3:
                                    for (int k = 0; k < tAP3.Length; ++k)
                                    {
                                        P[k] = tAP3[k] * mag;
                                    }
                                    break;
                            }
                            break;
                    }

                    if (seedNum == 0)
                    {
                        seedNum = 1;
                    }

                    int num = ETC.CreateRandomNum(seedNum) % (100 * mag);

                    switch (type)
                    {
                        case ProductType.Normal:
                            if ((num >= 0) && (num < P[0]))
                            {
                                confirmGrade = 2;
                            }
                            else if ((num >= P[0]) && (num < (P[0] + P[1])))
                            {
                                confirmGrade = 3;
                            }
                            else if ((num >= (P[0] + P[1])) && (num >= (P[0] + P[1] + P[2])))
                            {
                                confirmGrade = 4;
                            }
                            else
                            {
                                confirmGrade = 5;
                            }
                            break;
                        case ProductType.Advance:
                            if ((num >= 0) && (num < P[0]))
                            {
                                confirmGrade = 3;
                            }
                            else if ((num >= P[0]) && (num < (P[0] + P[1])))
                            {
                                confirmGrade = 4;
                            }
                            else
                            {
                                confirmGrade = 5;
                            }
                            break;
                    }

                    if (confirmGrade == 0)
                    {
                        if (type == ProductType.Normal)
                        {
                            confirmGrade = 2;
                        }
                        else if ((type == ProductType.Advance) && (typeAdvance == AdvanceType._3))
                        {
                            confirmGrade = 4;
                        }
                        else
                        {
                            confirmGrade = 3;
                        }
                    }

                    var finalDoll = new List<Doll>();

                    foreach (var doll in availableDoll)
                    {
                        if (doll.Grade == confirmGrade)
                        {
                            finalDoll.Add(doll);
                        }
                    }

                    finalDoll.TrimExcess();
                    finalDoll.ShuffleList();

                    int fnum = ETC.CreateRandomNum() % finalDoll.Count;

                    resultsDicNumber[i] = finalDoll[fnum].DicNumber;
                }

                ShowResultScreen(resultsDicNumber);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.ProductSimulatorActivity_ProductDollError, Snackbar.LengthShort);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }
    }
}