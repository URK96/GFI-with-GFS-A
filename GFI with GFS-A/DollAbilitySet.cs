using System;
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
        Dictionary<string, double[]> NormalGrow_Basic;
        Dictionary<string, double[]> NormalGrow_Grow;
        Dictionary<string, double[]> ModGrow_Basic;
        Dictionary<string, double[]> ModGrow_Grow;

        Dictionary<string, double> Attributes;

        string[] Attribute_Types =
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
            NormalGrow_Basic = new Dictionary<string, double[]>();
            NormalGrow_Grow = new Dictionary<string, double[]>();
            ModGrow_Basic = new Dictionary<string, double[]>();
            ModGrow_Grow = new Dictionary<string, double[]>();
            Attributes = new Dictionary<string, double>();

            NormalGrow_Basic.Add("HP", new double[] { 55, 0.555 });
            NormalGrow_Basic.Add("FireRate", new double[] { 16, 0 });
            NormalGrow_Basic.Add("Accuracy", new double[] { 5, 0 });
            NormalGrow_Basic.Add("Evasion", new double[] { 5, 0 });
            NormalGrow_Basic.Add("AttackSpeed", new double[] { 45, 0 });
            NormalGrow_Basic.Add("Armor", new double[] { 2, 0.161 });

            NormalGrow_Grow.Add("FireRate", new double[] { 0.242, 0 });
            NormalGrow_Grow.Add("Accuracy", new double[] { 0.303, 0 });
            NormalGrow_Grow.Add("Evasion", new double[] { 0.303, 0 });
            NormalGrow_Grow.Add("AttackSpeed", new double[] { 0.181, 0 });

            ModGrow_Basic.Add("HP", new double[] { 55, 0.555 });
            ModGrow_Basic.Add("FireRate", new double[] { 16, 0 });
            ModGrow_Basic.Add("Accuracy", new double[] { 5, 0 });
            ModGrow_Basic.Add("Evasion", new double[] { 5, 0 });
            ModGrow_Basic.Add("AttackSpeed", new double[] { 45, 0 });
            ModGrow_Basic.Add("Armor", new double[] { 2, 0.161 });

            ModGrow_Grow.Add("FireRate", new double[] { 0.06, 18.018 });
            ModGrow_Grow.Add("Accuracy", new double[] { 0.075, 22.572 });
            ModGrow_Grow.Add("Evasion", new double[] { 0.075, 22.572 });
            ModGrow_Grow.Add("AttackSpeed", new double[] { 0.022, 15.741 });

            double[] Attribute_Values = null;

            switch(type)
            {
                case "HG":
                    Attribute_Values = new double[] { 0.6, 0.6, 1.2, 1.8, 0.8, 0 };
                    break;
                case "SMG":
                    Attribute_Values = new double[] { 1.6, 0.6, 0.3, 1.6, 1.2, 0 };
                    break;
                case "AR":
                    Attribute_Values = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 0 };
                    break;
                case "RF":
                    Attribute_Values = new double[] { 0.8, 2.4, 1.6, 0.8, 0.5, 0 };
                    break;
                case "MG":
                    Attribute_Values = new double[] { 1.5, 1.8, 0.6, 0.6, 1.6, 0 };
                    break;
                case "SG":
                    Attribute_Values = new double[] { 2.0, 0.7, 0.3, 0.3, 0.4, 1 };
                    break;
            }

            for (int i = 0; i < Attribute_Types.Length; ++i)
                Attributes.Add(Attribute_Types[i], Attribute_Values[i]);
        }

        internal int CalcAbility(string ability, int ability_value, int grow_ratio, int level, int favor, bool IsMod = false)
        {
            double favor_ratio = 0;
            double result = 0;

            try
            {
                if (favor < 10) favor_ratio = -0.05;
                else if (favor < 90) favor_ratio = 0;
                else if (favor < 140) favor_ratio = 0.05;
                else if (favor < 190) favor_ratio = 0.1;
                else if (favor < 200) favor_ratio = 0.15;
                else favor_ratio = 0;

                double[] basic = null;
                double[] grow = null;
                double type_ratio = Attributes[ability];

                switch (IsMod)
                {
                    case false:
                        basic = NormalGrow_Basic[ability];
                        if ((ability != "HP") && (ability != "Armor"))
                            grow = NormalGrow_Grow[ability];
                        break;
                    case true:
                        basic = ModGrow_Basic[ability];
                        if ((ability != "HP") && (ability != "Armor"))
                            grow = ModGrow_Grow[ability];
                        break;
                }

                result = Math.Ceiling((basic[0] + (level - 1) * basic[1]) * type_ratio * ability_value / 100);

                if ((ability != "HP") && (ability != "Armor"))
                    result += Math.Ceiling((grow[1] + (level - 1) * grow[0]) * type_ratio * ability_value * grow_ratio / 100 / 100);

                if ((ability == "FireRate") || (ability == "Accuracy") || (ability == "Evasion"))
                    result += Math.Sign(favor_ratio) * Math.Ceiling(Math.Abs(result * favor_ratio));
            }
            catch (Exception ex)
            {
                ETC.LogError(ex);
            }

            return Convert.ToInt32(result);
        }
    }
}