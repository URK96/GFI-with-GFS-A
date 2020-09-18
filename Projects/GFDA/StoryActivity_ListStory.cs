using Android.Content;
using Android.Icu.Text;

using System.Runtime.Remoting.Messaging;

using static GFDA.Resource.Array;

namespace GFDA
{
    public partial class StoryActivity : BaseAppCompatActivity
    {
        private void ListMainStoryItem(out (int, int, int) stringIds)
        {
            stringIds = subMainIndex switch
            {
                0 => (Story_Main_Main_Prologue_TopTitle, Story_Main_Main_Prologue, Story_Main_Main_Prologue_Caption),
                1 => (Story_Main_Main_Area_0_TopTitle, Story_Main_Main_Area_0, Story_Main_Main_Area_0_Caption),
                2 => (Story_Main_Main_Area_1_TopTitle, Story_Main_Main_Area_1, Story_Main_Main_Area_1_Caption),
                3 => (Story_Main_Main_Area_2_TopTitle, Story_Main_Main_Area_2, Story_Main_Main_Area_2_Caption),
                4 => (Story_Main_Main_Area_3_TopTitle, Story_Main_Main_Area_3, Story_Main_Main_Area_3_Caption),
                5 => (Story_Main_Main_Area_4_TopTitle, Story_Main_Main_Area_4, Story_Main_Main_Area_4_Caption),
                6 => (Story_Main_Main_Area_5_TopTitle, Story_Main_Main_Area_5, Story_Main_Main_Area_5_Caption),
                7 => (Story_Main_Main_Area_6_TopTitle, Story_Main_Main_Area_6, Story_Main_Main_Area_6_Caption),
                8 => (Story_Main_Main_Area_7_TopTitle, Story_Main_Main_Area_7, Story_Main_Main_Area_7_Caption),
                9 => (Story_Main_Main_Area_8_TopTitle, Story_Main_Main_Area_8, Story_Main_Main_Area_8_Caption),
                10 => (Story_Main_Main_Area_9_TopTitle, Story_Main_Main_Area_9, Story_Main_Main_Area_9_Caption),
                11 => (Story_Main_Main_Area_10_TopTitle, Story_Main_Main_Area_10, Story_Main_Main_Area_10_Caption),
                12 => (Story_Main_Main_Area_11_TopTitle, Story_Main_Main_Area_11, Story_Main_Main_Area_11_Caption),
                13 => (Story_Main_Main_Area_12_TopTitle, Story_Main_Main_Area_12, Story_Main_Main_Area_12_Caption),
                14 => (Story_Main_Main_Area_13_TopTitle, Story_Main_Main_Area_13, Story_Main_Main_Area_13_Caption),
                15 => (Story_Main_Main_Event_Cube_TopTitle, Story_Main_Main_Event_Cube, Story_Main_Main_Event_Cube_Caption),
                16 => (Story_Main_Main_Event_Hypothermia_1_TopTitle, Story_Main_Main_Event_Hypothermia_1, Story_Main_Main_Event_Hypothermia_1_Caption),
                17 => (Story_Main_Main_Event_Hypothermia_2_TopTitle, Story_Main_Main_Event_Hypothermia_2, Story_Main_Main_Event_Hypothermia_2_Caption),
                18 => (Story_Main_Main_Event_Hypothermia_3_TopTitle, Story_Main_Main_Event_Hypothermia_3, Story_Main_Main_Event_Hypothermia_3_Caption),
                19 => (Story_Main_Main_Event_Hypothermia_Hidden_TopTitle, Story_Main_Main_Event_Hypothermia_Hidden, Story_Main_Main_Event_Hypothermia_Hidden_Caption),
                20 => (Story_Main_Main_Event_CubePlus_Toptitle, Story_Main_Main_Event_CubePlus, Story_Main_Main_Event_CubePlus_Caption),
                21 => (Story_Main_Main_Event_GuiltyGear_Toptitle, Story_Main_Main_Event_GuiltyGear, Story_Main_Main_Event_GuiltyGear_Caption),
                22 => (Story_Main_Main_Event_DeepDive_1_TopTitle, Story_Main_Main_Event_DeepDive_1, Story_Main_Main_Event_DeepDive_1_Caption),
                23 => (Story_Main_Main_Event_DeepDive_2_TopTitle, Story_Main_Main_Event_DeepDive_2, Story_Main_Main_Event_DeepDive_2_Caption),
                24 => (Story_Main_Main_Event_DeepDive_3_TopTitle, Story_Main_Main_Event_DeepDive_3, Story_Main_Main_Event_DeepDive_3_Caption),
                25 => (Story_Main_Main_Event_DeepDive_Hidden_TopTitle, Story_Main_Main_Event_DeepDive_Hidden, Story_Main_Main_Event_DeepDive_Hidden_Caption),
                26 => (Story_Main_Main_Event_Singularity_TopTitle, Story_Main_Main_Event_Singularity, Story_Main_Main_Event_Singularity_Caption),
                27 => (Story_Main_Main_Event_DJMAX_1_TopTitle, Story_Main_Main_Event_DJMAX_1, Story_Main_Main_Event_DJMAX_1_Caption),
                28 => (Story_Main_Main_Event_DJMAX_2_TopTitle, Story_Main_Main_Event_DJMAX_2, Story_Main_Main_Event_DJMAX_2_Caption),
                29 => (Story_Main_Main_Event_ContinuumTurbulence_TopTitle, Story_Main_Main_Event_ContinuumTurbulence, Story_Main_Main_Event_ContinuumTurbulence_Caption),
                30 => (Story_Main_Main_Event_Isomer_TopTitle, Story_Main_Main_Event_Isomer, Story_Main_Main_Event_Isomer_Caption),
                31 => (Story_Main_Main_Event_VA_TopTitle, Story_Main_Main_Event_VA, Story_Main_Main_Event_VA_Caption),
                32 => (Story_Main_Main_Event_ShatteredConnexion_TopTitle, Story_Main_Main_Event_ShatteredConnexion, Story_Main_Main_Event_ShatteredConnexion_Caption),
                33 => (Story_Main_Main_Event_2019Halloween_TopTitle, Story_Main_Main_Event_2019Halloween, Story_Main_Main_Event_2019Halloween_Caption),
                34 => (Story_Main_Main_Event_2019Christmas_TopTitle, Story_Main_Main_Event_2019Christmas, Story_Main_Main_Event_2019Christmas_Caption),
                35 => (Story_Main_Main_Event_PolarizedLight_TopTitle, Story_Main_Main_Event_PolarizedLight, Story_Main_Main_Event_PolarizedLight_Caption),
                36 => (Story_Main_Main_Event_2020Spring_TopTitle, Story_Main_Main_Event_2020Spring, Story_Main_Main_Event_2020Spring_Caption),
                37 => (Story_Main_Main_Event_2020Summer_TopTitle, Story_Main_Main_Event_2020Summer, Story_Main_Main_Event_2020Summer_Caption),
                38 => (Story_Main_Main_Event_DreamDrama_TopTitle, Story_Main_Main_Event_DreamDrama, Story_Main_Main_Event_DreamDrama_Caption),
                _ => (0, 0, 0),
            };
        }

        private void ListSubStoryItem(out (int, int, int) stringIds)
        {
            stringIds = subMainIndex switch
            {
                0 => (Story_Main_Sub_2016MessyHalloween_TopTitle, Story_Main_Sub_2016MessyHalloween, Story_Main_Sub_2016MessyHalloween_Caption),
                1 => (Story_Main_Sub_2016TacticalChristmas_TopTitle, Story_Main_Sub_2016TacticalChristmas, Story_Main_Sub_2016TacticalChristmas_Caption),
                2 => (Story_Main_Sub_2017Anniversary_TopTitle, Story_Main_Sub_2017Anniversary, Story_Main_Sub_2017Anniversary_Caption),
                3 => (Story_Main_Sub_2017OperaPrinces_TopTitle, Story_Main_Sub_2017OperaPrinces, Story_Main_Sub_2017OperaPrinces_Caption),
                4 => (Story_Main_Sub_2018LunaNewYear_TopTitle, Story_Main_Sub_2018LunaNewYear, Story_Main_Sub_2018LunaNewYear_Caption),
                5 => (Story_Main_Sub_2018SweetWedding_TopTitle, Story_Main_Sub_2018SweetWedding, Story_Main_Sub_2018SweetWedding_Caption),
                6 => (Story_Main_Sub_2018Anniversary_TopTitle, Story_Main_Sub_2018Anniversary, Story_Main_Sub_2018Anniversary_Caption),
                7 => (Story_Main_Sub_2018MaidTraining_TopTitle, Story_Main_Sub_2018MaidTraining, Story_Main_Sub_2018MaidTraining_Caption),
                8 => (Story_Main_Sub_2018BeachParty_TopTitle, Story_Main_Sub_2018BeachParty, Story_Main_Sub_2018BeachParty_Caption),
                9 => (Story_Main_Sub_2018RiseoftheWitches_TopTitle, Story_Main_Sub_2018RiseoftheWitches, Story_Main_Sub_2018RiseoftheWitches_Caption),
                10 => (Story_Main_Sub_2018AnotherChristmas_TopTitle, Story_Main_Sub_2018AnotherChristmas, Story_Main_Sub_2018AnotherChristmas_Caption),
                11 => (Story_Main_Sub_2019LunaNewYear_TopTitle, Story_Main_Sub_2019LunaNewYear, Story_Main_Sub_2019LunaNewYear_Caption),
                12 => (Story_Main_Sub_2019GunRose_TopTitle, Story_Main_Sub_2019GunRose, Story_Main_Sub_2019GunRose_Caption),
                13 => (Story_Main_Sub_2019Anniversary_TopTitle, Story_Main_Sub_2019Anniversary, Story_Main_Sub_2019Anniversary_Caption),
                14 => (Story_Main_Sub_2019Summer_TopTitle, Story_Main_Sub_2019Summer, Story_Main_Sub_2019Summer_Caption),
                15 => (Story_Main_Sub_2019Halloween_TopTitle, Story_Main_Sub_2019Halloween, Story_Main_Sub_2019Halloween_Caption),
                16 => (Story_Main_Sub_2019Christmas_TopTitle, Story_Main_Sub_2019Christmas, Story_Main_Sub_2019Christmas_Caption),
                17 => (Story_Main_Sub_2020NewYear_TopTitle, Story_Main_Sub_2020NewYear, Story_Main_Sub_2020NewYear_Caption),
                _ => (0, 0, 0),
            };
        }

        private void ListMemoryStoryItem(out (int, int, int) stringIds)
        {
            stringIds = subMainIndex switch
            {
                0 => (Story_Main_Memory_C121_TopTitle, Story_Main_Memory_C121, Story_Main_Memory_C121_Caption),
                1 => (Story_Main_Memory_C122_TopTitle, Story_Main_Memory_C122, Story_Main_Memory_C122_Caption),
                2 => (Story_Main_Memory_C123_TopTitle, Story_Main_Memory_C123, Story_Main_Memory_C123_Caption),
                _ => (0, 0, 0),
            };
        }

        private void RunReader()
        {
            string top;
            string category;

            switch (topType)
            {
                case Top.Main:
                    top = "Main";
                    break;
                case Top.Sub:
                    top = "Sub";
                    break;
                case Top.Memory:
                    top = "Memory";
                    break;
                default:
                    return;
            }

            if (topType == Top.Main)
            {
                switch (subMainIndex)
                {
                    case 0:
                        category = "Prologue";
                        break;
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                        category = $"Area_{subMainIndex - 1}";
                        break;
                    case 15:
                        category = "Cube";
                        break;
                    case 16:
                        category = "Hypothermia_1";
                        break;
                    case 17:
                        category = "Hypothermia_2";
                        break;
                    case 18:
                        category = "Hypothermia_3";
                        break;
                    case 19:
                        category = "Hypothermia_Hidden";
                        break;
                    case 20:
                        category = "CubePlus";
                        break;
                    case 21:
                        category = "GuiltyGear";
                        break;
                    case 22:
                        category = "DeepDive_1";
                        break;
                    case 23:
                        category = "DeepDive_2";
                        break;
                    case 24:
                        category = "DeepDive_3";
                        break;
                    case 25:
                        category = "DeepDive_Hidden";
                        break;
                    case 26:
                        category = "Singularity";
                        break;
                    case 27:
                        category = "DJMAX_1";
                        break;
                    case 28:
                        category = "DJMAX_2";
                        break;
                    case 29:
                        category = "ContinuumTurbulence";
                        break;
                    case 30:
                        category = "Isomer";
                        break;
                    case 31:
                        category = "VA";
                        break;
                    case 32:
                        category = "ShatteredConnexion";
                        break;
                    case 33:
                        category = "2019Halloween";
                        break;
                    case 34:
                        category = "2019Christmas";
                        break;
                    case 35:
                        category = "PolarizedLight";
                        break;
                    case 36:
                        category = "2020Spring";
                        break;
                    case 37:
                        category = "2020Summer";
                        break;
                    case 38:
                        category = "DreamDrama";
                        break;
                    default:
                        return;
                }
            }
            else if (topType == Top.Sub)
            {
                switch (subMainIndex)
                {
                    case 0:
                        category = "2016MessyHalloween";
                        break;
                    case 1:
                        category = "2016TacticalChristmas";
                        break;
                    case 2:
                        category = "2017Anniversary";
                        break;
                    case 3:
                        category = "2017OperaPrinces";
                        break;
                    case 4:
                        category = "2018LunaNewYear";
                        break;
                    case 5:
                        category = "2018SweetWedding";
                        break;
                    case 6:
                        category = "2018Anniversary";
                        break;
                    case 7:
                        category = "2018MaidTraining";
                        break;
                    case 8:
                        category = "2018BeachParty";
                        break;
                    case 9:
                        category = "2018RiseoftheWitches";
                        break;
                    case 10:
                        category = "2018AnotherChristmas";
                        break;
                    case 11:
                        category = "2019LunaNewYear";
                        break;
                    case 12:
                        category = "2019GunRose";
                        break;
                    case 13:
                        category = "2019Anniversary";
                        break;
                    case 14:
                        category = "2019Summer";
                        break;
                    case 15:
                        category = "2019Halloween";
                        break;
                    case 16:
                        category = "2019Christmas";
                        break;
                    case 17:
                        category = "2020NewYear";
                        break;
                    default:
                        return;
                }
            }
            else if (topType == Top.Memory)
            {
                switch (subMainIndex)
                {
                    case 0:
                        category = "C121";
                        break;
                    case 1:
                        category = "C122";
                        break;
                    case 2:
                        category = "C123";
                        break;
                    default:
                        return;
                }
            }
            else
            {
                return;
            }

            var intent = new Intent(this, typeof(StoryReaderActivity));
            intent.PutExtra("Info", new string[] { top, category, itemIndex.ToString(), titleList.Count.ToString() });
            intent.PutExtra("List", titleList.ToArray());
            intent.PutExtra("TopList", topTitleList.ToArray());
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }
    }
}