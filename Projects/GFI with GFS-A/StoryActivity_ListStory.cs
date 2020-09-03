using Android.Content;
using Android.Icu.Text;

using static GFI_with_GFS_A.Resource.Array;

namespace GFI_with_GFS_A
{
    public partial class StoryActivity : BaseAppCompatActivity
    {
        private void ListMainStoryItem(out (int, int, int) stringIds)
        {
            switch (subMainIndex)
            {
                default:
                    stringIds = (0, 0, 0);
                    break;
                case 0:
                    stringIds = (Story_Main_Main_Prologue_TopTitle, Story_Main_Main_Prologue, Story_Main_Main_Prologue_Caption);
                    break;
                case 1:
                    stringIds = (Story_Main_Main_Area_0_TopTitle, Story_Main_Main_Area_0, Story_Main_Main_Area_0_Caption);
                    break;
                case 2:
                    stringIds = (Story_Main_Main_Area_1_TopTitle, Story_Main_Main_Area_1, Story_Main_Main_Area_1_Caption);
                    break;
                case 3:
                    stringIds = (Story_Main_Main_Area_2_TopTitle, Story_Main_Main_Area_2, Story_Main_Main_Area_2_Caption);
                    break;
                case 4:
                    stringIds = (Story_Main_Main_Area_3_TopTitle, Story_Main_Main_Area_3, Story_Main_Main_Area_3_Caption);
                    break;
                case 5:
                    stringIds = (Story_Main_Main_Area_4_TopTitle, Story_Main_Main_Area_4, Story_Main_Main_Area_4_Caption);
                    break;
                case 6:
                    stringIds = (Story_Main_Main_Area_5_TopTitle, Story_Main_Main_Area_5, Story_Main_Main_Area_5_Caption);
                    break;
                case 7:
                    stringIds = (Story_Main_Main_Area_6_TopTitle, Story_Main_Main_Area_6, Story_Main_Main_Area_6_Caption);
                    break;
                case 8:
                    stringIds = (Story_Main_Main_Area_7_TopTitle, Story_Main_Main_Area_7, Story_Main_Main_Area_7_Caption);
                    break;
                case 9:
                    stringIds = (Story_Main_Main_Area_8_TopTitle, Story_Main_Main_Area_8, Story_Main_Main_Area_8_Caption);
                    break;
                case 10:
                    stringIds = (Story_Main_Main_Area_9_TopTitle, Story_Main_Main_Area_9, Story_Main_Main_Area_9_Caption);
                    break;
                case 11:
                    stringIds = (Story_Main_Main_Area_10_TopTitle, Story_Main_Main_Area_10, Story_Main_Main_Area_10_Caption);
                    break;
                case 12:
                    stringIds = (Story_Main_Main_Area_11_TopTitle, Story_Main_Main_Area_11, Story_Main_Main_Area_11_Caption);
                    break;
                case 13:
                    stringIds = (Story_Main_Main_Area_12_TopTitle, Story_Main_Main_Area_12, Story_Main_Main_Area_12_Caption);
                    break;
                case 14:
                    stringIds = (Story_Main_Main_Area_13_TopTitle, Story_Main_Main_Area_13, Story_Main_Main_Area_13_Caption);
                    break;
                case 15:
                    stringIds = (Story_Main_Main_Event_Cube_TopTitle, Story_Main_Main_Event_Cube, Story_Main_Main_Event_Cube_Caption);
                    break;
                case 16:
                    stringIds = (Story_Main_Main_Event_Hypothermia_1_TopTitle, Story_Main_Main_Event_Hypothermia_1, Story_Main_Main_Event_Hypothermia_1_Caption);
                    break;
                case 17:
                    stringIds = (Story_Main_Main_Event_Hypothermia_2_TopTitle, Story_Main_Main_Event_Hypothermia_2, Story_Main_Main_Event_Hypothermia_2_Caption);
                    break;
                case 18:
                    stringIds = (Story_Main_Main_Event_Hypothermia_3_TopTitle, Story_Main_Main_Event_Hypothermia_3, Story_Main_Main_Event_Hypothermia_3_Caption);
                    break;
                case 19:
                    stringIds = (Story_Main_Main_Event_Hypothermia_Hidden_TopTitle, Story_Main_Main_Event_Hypothermia_Hidden, Story_Main_Main_Event_Hypothermia_Hidden_Caption);
                    break;
                case 20:
                    stringIds = (Story_Main_Main_Event_CubePlus_Toptitle, Story_Main_Main_Event_CubePlus, Story_Main_Main_Event_CubePlus_Caption);
                    break;
                case 21:
                    stringIds = (Story_Main_Main_Event_GuiltyGear_Toptitle, Story_Main_Main_Event_GuiltyGear, Story_Main_Main_Event_GuiltyGear_Caption);
                    break;
                case 22:
                    stringIds = (Story_Main_Main_Event_DeepDive_1_TopTitle, Story_Main_Main_Event_DeepDive_1, Story_Main_Main_Event_DeepDive_1_Caption);
                    break;
                case 23:
                    stringIds = (Story_Main_Main_Event_DeepDive_2_TopTitle, Story_Main_Main_Event_DeepDive_2, Story_Main_Main_Event_DeepDive_2_Caption);
                    break;
                case 24:
                    stringIds = (Story_Main_Main_Event_DeepDive_3_TopTitle, Story_Main_Main_Event_DeepDive_3, Story_Main_Main_Event_DeepDive_3_Caption);
                    break;
                case 25:
                    stringIds = (Story_Main_Main_Event_DeepDive_Hidden_TopTitle, Story_Main_Main_Event_DeepDive_Hidden, Story_Main_Main_Event_DeepDive_Hidden_Caption);
                    break;
                case 26:
                    stringIds = (Story_Main_Main_Event_Singularity_TopTitle, Story_Main_Main_Event_Singularity, Story_Main_Main_Event_Singularity_Caption);
                    break;
                case 27:
                    stringIds = (Story_Main_Main_Event_DJMAX_1_TopTitle, Story_Main_Main_Event_DJMAX_1, Story_Main_Main_Event_DJMAX_1_Caption);
                    break;
                case 28:
                    stringIds = (Story_Main_Main_Event_DJMAX_2_TopTitle, Story_Main_Main_Event_DJMAX_2, Story_Main_Main_Event_DJMAX_2_Caption);
                    break;
                case 29:
                    stringIds = (Story_Main_Main_Event_ContinuumTurbulence_TopTitle, Story_Main_Main_Event_ContinuumTurbulence, Story_Main_Main_Event_ContinuumTurbulence_Caption);
                    break;
                case 30:
                    stringIds = (Story_Main_Main_Event_Isomer_TopTitle, Story_Main_Main_Event_Isomer, Story_Main_Main_Event_Isomer_Caption);
                    break;
                case 31:
                    stringIds = (Story_Main_Main_Event_VA_TopTitle, Story_Main_Main_Event_VA, Story_Main_Main_Event_VA_Caption);
                    break;
                case 32:
                    stringIds = (Story_Main_Main_Event_ShatteredConnexion_TopTitle, Story_Main_Main_Event_ShatteredConnexion, Story_Main_Main_Event_ShatteredConnexion_Caption);
                    break;
                case 33:
                    stringIds = (Story_Main_Main_Event_2019Halloween_TopTitle, Story_Main_Main_Event_2019Halloween, Story_Main_Main_Event_2019Halloween_Caption);
                    break;
                case 34:
                    stringIds = (Story_Main_Main_Event_2019Christmas_TopTitle, Story_Main_Main_Event_2019Christmas, Story_Main_Main_Event_2019Christmas_Caption);
                    break;
                case 35:
                    stringIds = (Story_Main_Main_Event_PolarizedLight_TopTitle, Story_Main_Main_Event_PolarizedLight, Story_Main_Main_Event_PolarizedLight_Caption);
                    break;
                case 36:
                    stringIds = (Story_Main_Main_Event_2020Spring_TopTitle, Story_Main_Main_Event_2020Spring, Story_Main_Main_Event_2020Spring_Caption);
                    break;
                case 37:
                    stringIds = (Story_Main_Main_Event_2020Summer_TopTitle, Story_Main_Main_Event_2020Summer, Story_Main_Main_Event_2020Summer_Caption);
                    break;
                case 38:
                    stringIds = (Story_Main_Main_Event_DreamDrama_TopTitle, Story_Main_Main_Event_DreamDrama, Story_Main_Main_Event_DreamDrama_Caption);
                    break;
            }
        }

        private void ListSubStoryItem(out (int, int, int) stringIds)
        {
            switch (subMainIndex)
            {
                default:
                    stringIds = (0, 0, 0);
                    break;
                case 0:
                    stringIds = (Story_Main_Sub_2016MessyHalloween_TopTitle, Story_Main_Sub_2016MessyHalloween, Story_Main_Sub_2016MessyHalloween_Caption);
                    break;
                case 1:
                    stringIds = (Story_Main_Sub_2016TacticalChristmas_TopTitle, Story_Main_Sub_2016TacticalChristmas, Story_Main_Sub_2016TacticalChristmas_Caption);
                    break;
                case 2:
                    stringIds = (Story_Main_Sub_2017Anniversary_TopTitle, Story_Main_Sub_2017Anniversary, Story_Main_Sub_2017Anniversary_Caption);
                    break;
                case 3:
                    stringIds = (Story_Main_Sub_2017OperaPrinces_TopTitle, Story_Main_Sub_2017OperaPrinces, Story_Main_Sub_2017OperaPrinces_Caption);
                    break;
                case 4:
                    stringIds = (Story_Main_Sub_2018LunaNewYear_TopTitle, Story_Main_Sub_2018LunaNewYear, Story_Main_Sub_2018LunaNewYear_Caption);
                    break;
                case 5:
                    stringIds = (Story_Main_Sub_2018SweetWedding_TopTitle, Story_Main_Sub_2018SweetWedding, Story_Main_Sub_2018SweetWedding_Caption);
                    break;
                case 6:
                    stringIds = (Story_Main_Sub_2018Anniversary_TopTitle, Story_Main_Sub_2018Anniversary, Story_Main_Sub_2018Anniversary_Caption);
                    break;
                case 7:
                    stringIds = (Story_Main_Sub_2018MaidTraining_TopTitle, Story_Main_Sub_2018MaidTraining, Story_Main_Sub_2018MaidTraining_Caption);
                    break;
                case 8:
                    stringIds = (Story_Main_Sub_2018BeachParty_TopTitle, Story_Main_Sub_2018BeachParty, Story_Main_Sub_2018BeachParty_Caption);
                    break;
                case 9:
                    stringIds = (Story_Main_Sub_2018RiseoftheWitches_TopTitle, Story_Main_Sub_2018RiseoftheWitches, Story_Main_Sub_2018RiseoftheWitches_Caption);
                    break;
                case 10:
                    stringIds = (Story_Main_Sub_2018AnotherChristmas_TopTitle, Story_Main_Sub_2018AnotherChristmas, Story_Main_Sub_2018AnotherChristmas_Caption);
                    break;
                case 11:
                    stringIds = (Story_Main_Sub_2019LunaNewYear_TopTitle, Story_Main_Sub_2019LunaNewYear, Story_Main_Sub_2019LunaNewYear_Caption);
                    break;
                case 12:
                    stringIds = (Story_Main_Sub_2019GunRose_TopTitle, Story_Main_Sub_2019GunRose, Story_Main_Sub_2019GunRose_Caption);
                    break;
                case 13:
                    stringIds = (Story_Main_Sub_2019Anniversary_TopTitle, Story_Main_Sub_2019Anniversary, Story_Main_Sub_2019Anniversary_Caption);
                    break;
            }
        }

        private void ListMemoryStoryItem(out (int, int, int) stringIds)
        {
            switch (subMainIndex)
            {
                default:
                    stringIds = (0, 0, 0);
                    break;
                case 0:
                    stringIds = (Story_Main_Memory_C121_TopTitle, Story_Main_Memory_C121, Story_Main_Memory_C121_Caption);
                    break;
                case 1:
                    stringIds = (Story_Main_Memory_C122_TopTitle, Story_Main_Memory_C122, Story_Main_Memory_C122_Caption);
                    break;
            }
        }

        private void RunReader()
        {
            string top = "";
            string category = "";

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