﻿using GFD_W.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace GFD_W
{
    public partial class Doll
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
        public string[] BuffType { get; private set; }
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
        public string[] DropEvent { get; private set; }

        public Dictionary<string, string> Abilities { get; private set; }
        public string[] AbilityGrade { get; private set; }

        public string GetDicNumberString { get { return string.Format("No. {0}", DicNumber); } }
        public string GetProductTimeToString { get { return ETC.CalcTime(ProductTime); } }

        internal Doll(DataRow dr, bool basic_info = false)
        {
            Name = (string)dr["Name"];
            ProductDialog = ETC.IsDBNullOrBlank(dr, "ProductDialog") ? "" : (string)dr["ProductDialog"];

            krName = (string)dr["Name"];
            NickName = (string)dr["NickName"];
            CodeName = (string)dr["CodeName"];
            DicNumber = (int)dr["DicNumber"];
            RealModel = (string)dr["Model"];
            Country = SetCountry(ref dr);
            Grade = (int)dr["Grade"];
            ProductTime = (int)dr["ProductTime"];
            Type = (string)dr["Type"];
            DropEvent = ((string)dr["DropEvent"]).Split(',');
            HasMod = (bool)dr["HasMod"];
            HasVoice = (bool)dr["HasVoice"];
            Illustrator = ETC.IsDBNullOrBlank(dr, "Illustrator") ? "" : (string)dr["Illustrator"];
            HasCensored = (bool)dr["HasCensor"];
            CensorType = HasCensored ? ((string)dr["CensorType"]).Split(';') : null;
            Costumes = ETC.IsDBNullOrBlank(dr, "Costume") == true ? null : ((string)dr["Costume"]).Split(';');

            string[] BuffFormation_Data = ((string)dr["EffectFormation"]).Split(',');
            BuffFormation = new int[9];

            for (int i = 0; i < BuffFormation_Data.Length; ++i)
                BuffFormation[i] = int.Parse(BuffFormation_Data[i]);

            BuffInfo = ((string)dr["Effect"]).Split(';');
            BuffType = ((string)dr["EffectType"]).Split(',');

            SkillName = (string)dr["Skill"];
            SkillExplain = (string)dr["SkillExplain"];
            SkillEffect = ((string)dr["SkillEffect"]).Split(';');
            SkillMag = ((string)dr["SkillMag"]).Split(',');

            if (HasMod == true)
            {
                ModGrade = (int)dr["ModGrade"];
                string[] ModBuffFormation_Data = ((string)dr["ModEffectFormation"]).Split(',');
                ModBuffFormation = new int[9];

                for (int i = 0; i < ModBuffFormation_Data.Length; ++i)
                    ModBuffFormation[i] = int.Parse(ModBuffFormation_Data[i]);

                ModBuffInfo = ((string)dr["ModEffect"]).Split(';');
                ModSkillName = (string)dr["ModSkill"];
                ModSkillExplain = (string)dr["ModSkillExplain"];
                ModSkillEffect = ((string)dr["ModSkillEffect"]).Split(';');
                ModSkillMag = ((string)dr["ModSkillMag"]).Split(',');
                SkillMagAfterMod = ((string)dr["SkillMagAfterMod"]).Split(',');
            }

            if (basic_info == false)
            {
                if (HasVoice == true)
                {
                    Voices = ((string)dr["Voices"]).Split(';');
                    VoiceActor = ETC.IsDBNullOrBlank(dr, "VoiceActor") ? "" : (string)dr["VoiceActor"];

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

                SetAbility(ref dr);
            }
        }

        private void SetAbility(ref DataRow dr)
        {
            Abilities = new Dictionary<string, string>();

            string[] ability_name = { "HP", "FireRate", "Evasion", "Accuracy", "AttackSpeed", "MoveSpeed", "Critical" };

            for (int i = 0; i < ability_name.Length; ++i)
                Abilities.Add(ability_name[i], (string)dr[ability_name[i]]);

            Abilities.Add("Grow", (string)dr["Grow"]);

            if (ETC.IsDBNullOrBlank(dr, "Bullet") == true)
                Abilities.Add("Bullet", "0");
            else
                Abilities.Add("Bullet", (string)dr["Bullet"]);

            if (ETC.IsDBNullOrBlank(dr, "Armor") == true)
                Abilities.Add("Armor", "0");
            else
                Abilities.Add("Armor", (string)dr["Armor"]);

            AbilityGrade = ((string)dr["AbilityGrade"]).Split(';');
        }
    }

    public class Equip
    {
        public string Name { get; private set; }
        public int Id { get; private set; }
        public int Grade { get; private set; }
        public int ProductTime { get; private set; }
        public string Category { get; private set; }
        public string Type { get; private set; }
        public string Icon { get; private set; }
        public string[] OnlyUse { get; private set; }
        public string[] DollType { get; private set; }
        public string[] SpecialDoll { get; private set; }
        public string Note { get; private set; }
        public string ImagePath { get; private set; }

        public string[] Abilities { get; private set; }
        public string[] InitMags { get; private set; }
        public string[] MaxMags { get; private set; }
        public bool CanUpgrade { get; private set; }
        public string ProductDialog { get; private set; }

        public string GetIdString { get { return string.Format("No. {0}", Id); } }
        public string GetProductTimeToString { get { return ETC.CalcTime(ProductTime); } }

        internal Equip(DataRow dr, bool basic_info = false)
        {
            Name = (string)dr["Name"];
            Id = (int)dr["Id"];
            Grade = (int)dr["Grade"];
            ProductTime = (int)dr["ProductTime"];
            Category = (string)dr["Category"];
            Icon = (string)dr["Icon"];
            Note = ETC.IsDBNullOrBlank(dr, "Note") ? "" : (string)dr["Note"];
            Type = (string)dr["Type"];
            ProductDialog = ETC.IsDBNullOrBlank(dr, "ProductDialog") ? "" : (string)dr["ProductDialog"];

            ImagePath = Path.Combine(ETC.cachePath, "Equip", "Normal", string.Format("{0}.gfdcache", Icon));

            OnlyUse = ETC.IsDBNullOrBlank(dr, "OnlyUse") ? null : ((string)dr["OnlyUse"]).Split(';');
            DollType = ETC.IsDBNullOrBlank(dr, "DollType") ? null : ((string)dr["DollType"]).Split(';');
            SpecialDoll = ETC.IsDBNullOrBlank(dr, "SpecialDoll") ? null : ((string)dr["SpecialDoll"]).Split(';');

            SetAbility(ref dr);
        }

        private void SetAbility(ref DataRow dr)
        {
            Abilities = ((string)dr["Ability"]).Split(';');
            InitMags = ((string)dr["InitialMagnification"]).Split(';');
            MaxMags = ((string)dr["MaxMagnification"]).Split(';');
            CanUpgrade = MaxMags[0] == "강화불가";
        }
    }

    public class Fairy
    {
        public string Name { get; private set; }
        public int DicNumber { get; private set; }
        public string Type { get; private set; }
        public int ProductTime { get; private set; }
        public string Note { get; private set; }
        public string ProductDialog { get; private set; }
        public string SkillName { get; private set; }
        public string SkillExplain { get; private set; }
        public string[] SkillEffect { get; private set; }
        public string[] SkillRate { get; private set; }
        public int OrderConsume { get; private set; }
        public int CoolDown { get; private set; }

        public Dictionary<string, string> Abilities;

        public string GetDicNumberString { get { return string.Format("No. {0}", DicNumber); } }
        public string GetProductTimeToString { get { return ETC.CalcTime(ProductTime); } }

        internal Fairy(DataRow dr, bool basic_info = false)
        {
            Name = (string)dr["Name"];
            DicNumber = (int)dr["DicNumber"];
            Type = (string)dr["Type"];
            ProductTime = (int)dr["ProductTime"];
            Note = ETC.IsDBNullOrBlank(dr, "Note") ? "" : (string)dr["Note"];
            ProductDialog = ETC.IsDBNullOrBlank(dr, "ProductDialog") ? "" : (string)dr["ProductDialog"];

            SkillName = (string)dr["SkillName"];
            SkillExplain = (string)dr["SkillExplain"];
            SkillEffect = ((string)dr["SkillEffect"]).Split(';');
            SkillRate = ((string)dr["SkillRate"]).Split(';');

            OrderConsume = (int)dr["OrderConsume"];
            CoolDown = (int)dr["CoolDown"];

            SetAbility(ref dr);
        }

        private void SetAbility(ref DataRow dr)
        {
            Abilities = new Dictionary<string, string>();

            string[] ability_name = { "FireRate", "Accuracy", "Evasion", "Armor", "Critical" };

            for (int i = 0; i < ability_name.Length; ++i)
                Abilities.Add(ability_name[i], (string)dr[ability_name[i]]);
        }
    }

    public class Enemy
    {
        public string Name { get; private set; }
        public string CodeName { get; private set; }
        public string[] Types { get; private set; }
        public bool IsBoss { get; private set; }
        public bool HasVoice { get; private set; }
        public string VoiceActor { get; private set; }
        public string[] Voices { get; private set; }

        public Dictionary<string, int>[] Abilities;

        public Enemy(DataRow dr, bool basic_info = false)
        {
            Name = (string)dr["Name"];
            CodeName = (string)dr["CodeName"];

            List<DataRow> drs = new List<DataRow>();

            foreach (DataRow t_dr in ETC.enemyList.Rows)
                if ((string)t_dr["CodeName"] == CodeName)
                    drs.Add(t_dr);

            drs.TrimExcess();

            Types = new string[drs.Count];
            for (int i = 0; i < Types.Length; ++i)
                Types[i] = (string)drs[i]["Type"];

            IsBoss = (bool)dr["IsBoss"];
            HasVoice = (bool)dr["HasVoice"];
            if (HasVoice == true)
            {
                VoiceActor = (string)dr["VoiceActor"];
                Voices = ((string)dr["Voices"]).Split(';');
            }

            SetAbility(ref drs);
        }

        private void SetAbility(ref List<DataRow> drs)
        {
            Abilities = new Dictionary<string, int>[Types.Length];

            string[] ability_name = { "HP", "FireRate", "Accuracy", "Evasion", "AttackSpeed", "MoveSpeed", "Penetration", "Armor", "Range" };

            for (int i = 0; i < Abilities.Length; ++i)
            {
                Abilities[i] = new Dictionary<string, int>();

                for (int k = 0; k < ability_name.Length; ++k)
                    Abilities[i].Add(ability_name[k], (int)drs[i][ability_name[k]]);
            }
        }
    }

    public class FST
    {
        public string Name { get; private set; }
        public string CodeName { get; private set; }
        public string NickName { get; private set; }
        public int DicNumber { get; private set; }
        public string RealModel { get; private set; }
        public string Country { get; private set; }
        public string Type { get; private set; }
        public string Illustrator { get; private set; }
        public string VoiceActor { get; private set; }
        public int ForceSize { get; private set; }
        public string Distance { get; private set; }
        public Dictionary<string, int>[] VersionUpPlus { get; private set; }
        public string ChipsetType { get; private set; }
        public int[,,] ChipsetCircuit { get; private set; }
        public Dictionary<string, int>[] GradeRestriction { get; private set; }
        public int[] ChipsetBonusCount { get; private set; }
        public Dictionary<string, int>[] ChipsetBonusMag { get; private set; }
        public string[] SkillName { get; private set; }
        public string[] SkillExplain { get; private set; }
        public string[][] SkillEffect { get; private set; }
        public string[][] SkillMag { get; private set; }

        public Dictionary<string, int> Abilities;

        public string[] AbilityList = { "Kill", "Crush", "Accuracy", "Reload" };
        public int CircuitCount = 5;
        public int CircuitLength = 8;
        public int CircuitHeight = 8;

        public FST(DataRow dr, bool basic_info = false)
        {
            Name = (string)dr["Name"];
            CodeName = (string)dr["CodeName"];
            NickName = "";
            DicNumber = 0;
            RealModel = (string)dr["Model"];
            Country = (string)dr["Country"];
            Type = (string)dr["Type"];
            Illustrator = "";
            VoiceActor = "";
            ForceSize = (int)dr["ForceSize"];
            Distance = (string)dr["Distance"];

            if (basic_info == false)
            {
                InitializeVersionUpPlus(ref dr);
                InitializeChipsetCircuit(ref dr);
                InitializeGradeRestriction(ref dr);
                InitializeChipsetBonus(ref dr);
                InitializeSkills(ref dr);
                InitializeAbilities(ref dr);
            }
        }

        private void InitializeVersionUpPlus(ref DataRow dr)
        {
            VersionUpPlus = new Dictionary<string, int>[10];
            string[] list = ((string)dr["VersionUp"]).Split(';');

            for (int i = 0; i < VersionUpPlus.Length; ++i)
            {
                VersionUpPlus[i] = new Dictionary<string, int>();
                string[] ability_list = list[i].Split(',');

                for (int k = 0; k < ability_list.Length; ++k)
                    VersionUpPlus[i].Add(AbilityList[k], int.Parse(ability_list[k]));
            }
        }

        private void InitializeChipsetCircuit(ref DataRow dr)
        {
            ChipsetType = (string)dr["CircuitType"];

            ChipsetCircuit = new int[CircuitCount, CircuitHeight, CircuitLength];

            for (int i = 0; i < CircuitCount; ++i)
                for (int k = 0; k < CircuitHeight; ++k)
                    for (int x = 0; x < CircuitLength; ++x)
                        ChipsetCircuit[i, k, x] = 0;

            for (int i = 0; i < CircuitCount; ++i)
            {
                string[] circuit_rows = ((string)dr[$"Circuit{i + 1}"]).Split(';');

                for (int k = 0; k < circuit_rows.Length; ++k)
                {
                    string[] row_index = circuit_rows[k].Split(',');

                    for (int j = 0; j < row_index.Length; ++j)
                    {
                        int num = int.Parse(row_index[j]);

                        if (num != 0)
                        {
                            if (num < 10)
                                ChipsetCircuit[i, k, num - 1] = 1;
                            else
                            {
                                int start = num / 10;
                                int end = num % 10;

                                for (int x = start - 1; x < end; ++x)
                                    ChipsetCircuit[i, k, x] = 1;
                            }
                        }
                    }
                }
            }
        }

        private void InitializeGradeRestriction(ref DataRow dr)
        {
            GradeRestriction = new Dictionary<string, int>[5];
            string[] list = ((string)dr["CeilingRestriction"]).Split(';');

            for (int i = 0; i < GradeRestriction.Length; ++i)
            {
                GradeRestriction[i] = new Dictionary<string, int>();
                string[] ability_list = list[i].Split(',');

                for (int k = 0; k < ability_list.Length; ++k)
                    GradeRestriction[i].Add(AbilityList[k], int.Parse(ability_list[k]));
            }
        }

        private void InitializeChipsetBonus(ref DataRow dr)
        {
            string[] count_list = ((string)dr["ChipsetBonusCount"]).Split(',');
            ChipsetBonusCount = new int[count_list.Length];

            for (int i = 0; i < count_list.Length; ++i)
                ChipsetBonusCount[i] = int.Parse(count_list[i]);

            string[] mag_list = ((string)dr["ChipsetBonusMag"]).Split(';');
            ChipsetBonusMag = new Dictionary<string, int>[mag_list.Length];

            for (int i = 0; i < mag_list.Length; ++i)
            {
                ChipsetBonusMag[i] = new Dictionary<string, int>();
                string[] ability_values = mag_list[i].Split(',');

                for (int k = 0; k < ability_values.Length; ++k)
                    ChipsetBonusMag[i].Add(AbilityList[k], int.Parse(ability_values[k]));
            }
        }

        private void InitializeSkills(ref DataRow dr)
        {
            SkillName = new string[3];
            SkillExplain = new string[3];
            SkillEffect = new string[3][];
            SkillMag = new string[3][];

            for (int i = 0; i < 3; ++i)
            {
                SkillName[i] = (string)dr[$"SkillName{i + 1}"];
                SkillExplain[i] = (string)dr[$"SkillExplain{i + 1}"];

                string[] effect_list = ((string)dr[$"SkillEffect{i + 1}"]).Split(';');
                string[] mag_list = ((string)dr[$"SkillMag{i + 1}"]).Split(',');

                SkillEffect[i] = new string[effect_list.Length];
                SkillMag[i] = new string[mag_list.Length];

                for (int k = 0; k < effect_list.Length; ++k)
                {
                    SkillEffect[i][k] = effect_list[k];
                    SkillMag[i][k] = mag_list[k];
                }
            }
        }

        private void InitializeAbilities(ref DataRow dr)
        {
            Abilities = new Dictionary<string, int>();

            foreach (string s in AbilityList)
            {
                string[] list = ((string)dr[s]).Split('/');

                Abilities.Add($"{s}_Min", int.Parse(list[0]));
                Abilities.Add($"{s}_Max", int.Parse(list[1]));
            }
        }
    }
}