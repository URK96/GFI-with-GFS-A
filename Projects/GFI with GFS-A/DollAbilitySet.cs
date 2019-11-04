﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace GFI_with_GFS_A
{
    internal class DollAbilitySet
    {
        Dictionary<string, double[]> normalGrowBasic;
        Dictionary<string, double[]> normalGrowGrow;
        Dictionary<string, double[]> modGrowBasic;
        Dictionary<string, double[]> modGrowGrow;

        Dictionary<string, double> attributes;

        string[] attributeTypes =
        {
            "HP",
            "FireRate",
            "Accuracy",
            "Evasion",
            "AttackSpeed",
            "Armor"
        };

        public DollAbilitySet(string type)
        {
            normalGrowBasic = new Dictionary<string, double[]>();
            normalGrowGrow = new Dictionary<string, double[]>();
            modGrowBasic = new Dictionary<string, double[]>();
            modGrowGrow = new Dictionary<string, double[]>();
            attributes = new Dictionary<string, double>();

            normalGrowBasic.Add("HP", new double[] { 55, 0.555 });
            normalGrowBasic.Add("FireRate", new double[] { 16, 0 });
            normalGrowBasic.Add("Accuracy", new double[] { 5, 0 });
            normalGrowBasic.Add("Evasion", new double[] { 5, 0 });
            normalGrowBasic.Add("AttackSpeed", new double[] { 45, 0 });
            normalGrowBasic.Add("Armor", new double[] { 2, 0.161 });

            normalGrowGrow.Add("FireRate", new double[] { 0.242, 0 });
            normalGrowGrow.Add("Accuracy", new double[] { 0.303, 0 });
            normalGrowGrow.Add("Evasion", new double[] { 0.303, 0 });
            normalGrowGrow.Add("AttackSpeed", new double[] { 0.181, 0 });

            modGrowBasic.Add("HP", new double[] { 96.283, 0.138 });
            modGrowBasic.Add("FireRate", new double[] { 16, 0 });
            modGrowBasic.Add("Accuracy", new double[] { 5, 0 });
            modGrowBasic.Add("Evasion", new double[] { 5, 0 });
            modGrowBasic.Add("AttackSpeed", new double[] { 45, 0 });
            modGrowBasic.Add("Armor", new double[] { 13.979, 0.04 });

            modGrowGrow.Add("FireRate", new double[] { 0.06, 18.018 });
            modGrowGrow.Add("Accuracy", new double[] { 0.075, 22.572 });
            modGrowGrow.Add("Evasion", new double[] { 0.075, 22.572 });
            modGrowGrow.Add("AttackSpeed", new double[] { 0.022, 15.741 });

            double[] attributeValues = null;

            switch(type)
            {
                case "HG":
                    attributeValues = new double[] { 0.6, 0.6, 1.2, 1.8, 0.8, 0 };
                    break;
                case "SMG":
                    attributeValues = new double[] { 1.6, 0.6, 0.3, 1.6, 1.2, 0 };
                    break;
                case "AR":
                    attributeValues = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 0 };
                    break;
                case "RF":
                    attributeValues = new double[] { 0.8, 2.4, 1.6, 0.8, 0.5, 0 };
                    break;
                case "MG":
                    attributeValues = new double[] { 1.5, 1.8, 0.6, 0.6, 1.6, 0 };
                    break;
                case "SG":
                    attributeValues = new double[] { 2.0, 0.7, 0.3, 0.3, 0.4, 1 };
                    break;
            }

            for (int i = 0; i < attributeTypes.Length; ++i)
            {
                attributes.Add(attributeTypes[i], attributeValues[i]);
            }
        }

        internal int CalcAbility(string ability, int abilityValue, int growRatio, int level, int favor, bool isMod = false)
        {
            double favorRatio;
            double result = 0;

            try
            {
                if (favor < 10)
                {
                    favorRatio = -0.05;
                }
                else if (favor < 90)
                {
                    favorRatio = 0;
                }
                else if (favor < 140)
                {
                    favorRatio = 0.05;
                }
                else if (favor < 190)
                {
                    favorRatio = 0.1;
                }
                else if (favor <= 200)
                {
                    favorRatio = 0.15;
                }
                else
                {
                    favorRatio = 0;
                }

                double[] basic = null;
                double[] grow = null;
                double typeRatio = attributes[ability];

                switch (isMod)
                {
                    case false:
                        basic = normalGrowBasic[ability];
                        if ((ability != "HP") && (ability != "Armor"))
                        {
                            grow = normalGrowGrow[ability];
                        }
                        break;
                    case true:
                        basic = modGrowBasic[ability];
                        if ((ability != "HP") && (ability != "Armor"))
                        {
                            grow = modGrowGrow[ability];
                        }
                        break;
                }

                result = Math.Ceiling((basic[0] + (level - 1) * basic[1]) * typeRatio * abilityValue / 100);

                if ((ability != "HP") && (ability != "Armor"))
                {
                    result += Math.Ceiling((grow[1] + (level - 1) * grow[0]) * typeRatio * abilityValue * growRatio / 100 / 100);
                }

                if ((ability == "FireRate") || (ability == "Accuracy") || (ability == "Evasion"))
                {
                    result += Math.Sign(favorRatio) * Math.Ceiling(Math.Abs(result * favorRatio));
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }

            return Convert.ToInt32(result);
        }
    }
}