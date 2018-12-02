using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UptimeSharp;
using System.Data;
using System.Net;

namespace GFI_with_GFS_A
{
    public class Doll
    {
        public string Name { get; private set; }
        public string krName { get; private set; }
        public string NickName { get; private set; }
        public string CodeName { get; private set; }
        public int DicNumber { get; private set; }
        public string RealModel { get; private set; }
        public string Country { get; private set; }
        public string Illustrator { get; private set; }
        public string VoiceActor { get; private set; }
        public int Grade { get; private set; }
        public int ModGrade { get; private set; }
        public int GradeIconId { get; private set; }
        public int ProductTime { get; private set; }
        public string ProductDialog { get; private set; }
        public string Type { get; private set; }
        public bool HasMod { get; private set; }
        public bool HasVoice { get; private set; }
        public string[] Costumes { get; private set; }
        public int[] BuffFormation { get; private set; }
        public int[] ModBuffFormation { get; private set; }
        public string[] BuffInfo { get; private set; }
        public string[] ModBuffInfo { get; private set; }
        public string BuffType { get; private set; }
        public string SkillName { get; private set; }
        public string SkillExplain { get; private set; }
        public string[] SkillEffect { get; private set; }
        public string[] SkillMag { get; private set; }
        public string[] SkillMagAfterMod { get; private set; }
        public string ModSkillName { get; private set; }
        public string ModSkillExplain { get; private set; }
        public string[] ModSkillEffect { get; private set; }
        public string[] ModSkillMag { get; private set; }
        public string[] Voices { get; private set; }
        public string[,] CostumeVoices { get; private set; }
        public bool HasCensored { get; private set; }
        public string[] CensorType { get; private set; }

        public Dictionary<string, string> Abilities { get; private set; }
        public string[] AbilityGrade { get; private set; }

        public string GetDicNumberString { get { return string.Format("No. {0}", DicNumber); } }
        public string GetProductTimeToString { get { return ETC.CalcTime(ProductTime); } }

        internal Doll(DataRow dr)
        {
            if (ETC.Language.Language == "ko")
            {
                Name = (string)dr["Name"];

                if (ETC.IsDBNullOrBlank(dr, "ProductDialog") == true) ProductDialog = "";
                else ProductDialog = (string)dr["ProductDialog"];
            }
            else
            {
                if (ETC.IsDBNullOrBlank(dr, "Name_EN") == true) Name = (string)dr["Name"];
                else Name = (string)dr["Name_EN"];

                if (ETC.IsDBNullOrBlank(dr, "ProductDialog") == true) ProductDialog = "";
                else ProductDialog = (string)dr["ProductDialog"];
            }

            krName = (string)dr["Name"];
            NickName = (string)dr["NickName"];
            CodeName = (string)dr["CodeName"];
            DicNumber = (int)dr["DicNumber"];
            RealModel = (string)dr["Model"];
            Country = (string)dr["Country"];
            Grade = (int)dr["Grade"];
            ProductTime = (int)dr["ProductTime"];
            Type = (string)dr["Type"];
            HasMod = (bool)dr["HasMod"];
            HasVoice = (bool)dr["HasVoice"];
            Illustrator = (string)dr["Illustrator"];
            if (ETC.IsDBNullOrBlank(dr, "VoiceActor") == true) VoiceActor = "";
            else VoiceActor = (string)dr["VoiceActor"];
            HasCensored = (bool)dr["HasCensor"];
            if (HasCensored == true) CensorType = ((string)dr["CensorType"]).Split(';');

            if (ETC.IsDBNullOrBlank(dr, "Costume") == true) Costumes = null;
            else Costumes = ((string)dr["Costume"]).Split(';');

            string[] BuffFormation_Data = ((string)dr["EffectFormation"]).Split(',');
            BuffFormation = new int[9];
            for (int i = 0; i < BuffFormation_Data.Length; ++i) BuffFormation[i] = int.Parse(BuffFormation_Data[i]);

            BuffInfo = ((string)dr["Effect"]).Split(';');
            BuffType = (string)dr["EffectType"];

            SkillName = (string)dr["Skill"];
            SkillExplain = (string)dr["SkillExplain"];
            SkillEffect = ((string)dr["SkillEffect"]).Split(';');
            SkillMag = ((string)dr["SkillMag"]).Split(',');

            if (HasMod == true)
            {
                ModGrade = (int)dr["ModGrade"];
                string[] ModBuffFormation_Data = ((string)dr["ModEffectFormation"]).Split(',');
                ModBuffFormation = new int[9];
                for (int i = 0; i < ModBuffFormation_Data.Length; ++i) ModBuffFormation[i] = int.Parse(ModBuffFormation_Data[i]);
                ModBuffInfo = ((string)dr["ModEffect"]).Split(';');
                ModSkillName = (string)dr["ModSkill"];
                ModSkillExplain = (string)dr["ModSkillExplain"];
                ModSkillEffect = ((string)dr["ModSkillEffect"]).Split(';');
                ModSkillMag = ((string)dr["ModSkillMag"]).Split(',');
                SkillMagAfterMod = ((string)dr["SkillMagAfterMod"]).Split(',');
            }

            if (HasVoice == true)
            {
                Voices = ((string)dr["Voices"]).Split(';');

                if (ETC.IsDBNullOrBlank(dr, "CostumeVoices") == false)
                {
                    string[] temp = ((string)dr["CostumeVoices"]).Split('/');

                    CostumeVoices = new string[temp.Length, 2];

                    for (int i = 0; i < temp.Length; ++i)
                    {
                        CostumeVoices[i, 0] = temp[i].Split(':')[0];
                        CostumeVoices[i, 1] = temp[i].Split(':')[1];
                    }
                }
            }
            else
            {
                Voices = null;
                CostumeVoices = null;
            }

            //CropImagePath = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", $"{DicNumber}.gfdcache");
            //ModCropImagePath = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", $"{DicNumber}_M.gfdcache");
            //FullImagePath = Path.Combine(ETC.CachePath, "Doll", "Normal", $"{doll.DicNumber}.gfdcache");

            switch (Grade)
            {
                case 0:
                    GradeIconId = Resource.Drawable.Grade_0;
                    break;
                case 2:
                    GradeIconId = Resource.Drawable.Grade_2;
                    break;
                case 3:
                    GradeIconId = Resource.Drawable.Grade_3;
                    break;
                case 4:
                    GradeIconId = Resource.Drawable.Grade_4;
                    break;
                case 5:
                    GradeIconId = Resource.Drawable.Grade_5;
                    break;
            }


            SetAbility(ref dr);
        }

        private void SetAbility(ref DataRow dr)
        {
            Abilities = new Dictionary<string, string>();

            Abilities.Add("HP", (string)dr["HP"]);
            Abilities.Add("FireRate", (string)dr["FireRate"]);
            Abilities.Add("Evasion", (string)dr["Evasion"]);
            Abilities.Add("Accuracy", (string)dr["Accuracy"]);
            Abilities.Add("AttackSpeed", (string)dr["AttackSpeed"]);
            Abilities.Add("MoveSpeed", (string)dr["MoveSpeed"]);
            Abilities.Add("Critical", (string)dr["Critical"]);
            if (ETC.IsDBNullOrBlank(dr, "Bullet") == true) Abilities.Add("Bullet", "0");
            else Abilities.Add("Bullet", (string)dr["Bullet"]);
            if (ETC.IsDBNullOrBlank(dr, "Armor") == true) Abilities.Add("Armor", "0");
            else Abilities.Add("Armor", (string)dr["Armor"]);

            AbilityGrade = ((string)dr["AbilityGrade"]).Split(';');
        }
    }

    public class Equip
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Grade { get; set; }
        public string GradeColor { get; set; }
        public int ProductTime { get; set; }
        public string Category { get; set; }
        public string Icon { get; set; }
        public string[] OnlyUse { get; set; }
        public string[] DollType { get; set; }
        public string[] SpecialDoll { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        public string ImagePath { get; set; }

        public string GetIdString { get { return string.Format("No. {0}", Id); } }
        public string GetProductTimeToString { get { return ETC.CalcTime(ProductTime); } }

        internal Equip(DataRow dr)
        {
            Name = (string)dr["Name"];
            Id = (int)dr["Id"];
            Grade = (int)dr["Grade"];
            ProductTime = (int)dr["ProductTime"];
            Category = (string)dr["Category"];
            Icon = (string)dr["Icon"];
            if (dr["Note"] == DBNull.Value) Note = "";
            else Note = (string)dr["Note"];
            Type = (string)dr["Type"];

            ImagePath = Path.Combine(ETC.CachePath, "Equip", "Normal", string.Format("{0}.gfdcache", Icon));

            if (dr["OnlyUse"] == DBNull.Value) OnlyUse = null;
            else OnlyUse = ((string)dr["OnlyUse"]).Split(';');
            if (dr["DollType"] == DBNull.Value) DollType = null;
            else DollType = ((string)dr["DollType"]).Split(';');
            if (dr["SpecialDoll"] == DBNull.Value) SpecialDoll = null;
            else SpecialDoll = ((string)dr["SpecialDoll"]).Split(';');
        }
    }

    public class Fairy
    {
        public string Name { get; set; }
        public int DicNumber { get; set; }
        public string Type { get; set; }
        public int ProductTime { get; set; }
        public string Note { get; set; }
        public string CropImagePath { get; set; }

        public string GetDicNumberString { get { return string.Format("No. {0}", DicNumber); } }
        public string GetProductTimeToString { get { return ETC.CalcTime(ProductTime); } }

        internal Fairy(DataRow dr)
        {
            Name = (string)dr["Name"];
            DicNumber = (int)dr["DicNumber"];
            Type = (string)dr["Type"];
            ProductTime = (int)dr["ProductTime"];

            if (dr["Note"] == DBNull.Value) Note = "";
            else Note = (string)dr["Note"];

            CropImagePath = Path.Combine(ETC.CachePath, "Fairy", "Normal_Crop", string.Format("{0}.gfdcache", DicNumber));
        }
    }
}