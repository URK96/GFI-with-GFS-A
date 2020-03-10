using Android.Content;
using Android.Support.V7.App;

namespace GFI_with_GFS_A
{
    public partial class StoryActivity : BaseAppCompatActivity
    {
        private void ListStoryItem_Main()
        {
            itemList.Clear();

            int titleRes = 0;
            int captionRes = 0;
            int topTitleRes = 0;

            switch (subMainIndex)
            {
                case 0:
                    titleRes = Resource.Array.Story_Main_Main_Prologue;
                    captionRes = Resource.Array.Story_Main_Main_Prologue_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Prologue_TopTitle;
                    break;
                case 1:
                    titleRes = Resource.Array.Story_Main_Main_Area_0;
                    captionRes = Resource.Array.Story_Main_Main_Area_0_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_0_TopTitle;
                    break;
                case 2:
                    titleRes = Resource.Array.Story_Main_Main_Area_1;
                    captionRes = Resource.Array.Story_Main_Main_Area_1_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_1_TopTitle;
                    break;
                case 3:
                    titleRes = Resource.Array.Story_Main_Main_Area_2;
                    captionRes = Resource.Array.Story_Main_Main_Area_2_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_2_TopTitle;
                    break;
                case 4:
                    titleRes = Resource.Array.Story_Main_Main_Area_3;
                    captionRes = Resource.Array.Story_Main_Main_Area_3_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_3_TopTitle;
                    break;
                case 5:
                    titleRes = Resource.Array.Story_Main_Main_Area_4;
                    captionRes = Resource.Array.Story_Main_Main_Area_4_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_4_TopTitle;
                    break;
                case 6:
                    titleRes = Resource.Array.Story_Main_Main_Area_5;
                    captionRes = Resource.Array.Story_Main_Main_Area_5_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_5_TopTitle;
                    break;
                case 7:
                    titleRes = Resource.Array.Story_Main_Main_Area_6;
                    captionRes = Resource.Array.Story_Main_Main_Area_6_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_6_TopTitle;
                    break;
                case 8:
                    titleRes = Resource.Array.Story_Main_Main_Area_7;
                    captionRes = Resource.Array.Story_Main_Main_Area_7_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_7_TopTitle;
                    break;
                case 9:
                    titleRes = Resource.Array.Story_Main_Main_Area_8;
                    captionRes = Resource.Array.Story_Main_Main_Area_8_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_8_TopTitle;
                    break;
                case 10:
                    titleRes = Resource.Array.Story_Main_Main_Area_9;
                    captionRes = Resource.Array.Story_Main_Main_Area_9_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_9_TopTitle;
                    break;
                case 11:
                    titleRes = Resource.Array.Story_Main_Main_Area_10;
                    captionRes = Resource.Array.Story_Main_Main_Area_10_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_10_TopTitle;
                    break;
                case 12:
                    titleRes = Resource.Array.Story_Main_Main_Area_11;
                    captionRes = Resource.Array.Story_Main_Main_Area_11_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_11_TopTitle;
                    break;
                case 13:
                    titleRes = Resource.Array.Story_Main_Main_Area_12;
                    captionRes = Resource.Array.Story_Main_Main_Area_12_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Area_12_TopTitle;
                    break;
                case 14:
                    titleRes = Resource.Array.Story_Main_Main_Event_Cube;
                    captionRes = Resource.Array.Story_Main_Main_Event_Cube_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_Cube_TopTitle;
                    break;
                case 15:
                    titleRes = Resource.Array.Story_Main_Main_Event_Hypothermia_1;
                    captionRes = Resource.Array.Story_Main_Main_Event_Hypothermia_1_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_Hypothermia_1_TopTitle;
                    break;
                case 16:
                    titleRes = Resource.Array.Story_Main_Main_Event_Hypothermia_2;
                    captionRes = Resource.Array.Story_Main_Main_Event_Hypothermia_2_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_Hypothermia_2_TopTitle;
                    break;
                case 17:
                    titleRes = Resource.Array.Story_Main_Main_Event_Hypothermia_3;
                    captionRes = Resource.Array.Story_Main_Main_Event_Hypothermia_3_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_Hypothermia_3_TopTitle;
                    break;
                case 18:
                    titleRes = Resource.Array.Story_Main_Main_Event_Hypothermia_Hidden;
                    captionRes = Resource.Array.Story_Main_Main_Event_Hypothermia_Hidden_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_Hypothermia_Hidden_TopTitle;
                    break;
                case 19:
                    titleRes = Resource.Array.Story_Main_Main_Event_CubePlus;
                    captionRes = Resource.Array.Story_Main_Main_Event_CubePlus_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_CubePlus_Toptitle;
                    break;
                case 20:
                    titleRes = Resource.Array.Story_Main_Main_Event_GuiltyGear;
                    captionRes = Resource.Array.Story_Main_Main_Event_GuiltyGear_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_GuiltyGear_Toptitle;
                    break;
                case 21:
                    titleRes = Resource.Array.Story_Main_Main_Event_DeepDive_1;
                    captionRes = Resource.Array.Story_Main_Main_Event_DeepDive_1_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_DeepDive_1_TopTitle;
                    break;
                case 22:
                    titleRes = Resource.Array.Story_Main_Main_Event_DeepDive_2;
                    captionRes = Resource.Array.Story_Main_Main_Event_DeepDive_2_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_DeepDive_2_TopTitle;
                    break;
                case 23:
                    titleRes = Resource.Array.Story_Main_Main_Event_DeepDive_3;
                    captionRes = Resource.Array.Story_Main_Main_Event_DeepDive_3_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_DeepDive_3_TopTitle;
                    break;
                case 24:
                    titleRes = Resource.Array.Story_Main_Main_Event_DeepDive_Hidden;
                    captionRes = Resource.Array.Story_Main_Main_Event_DeepDive_Hidden_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_DeepDive_Hidden_TopTitle;
                    break;
                case 25:
                    titleRes = Resource.Array.Story_Main_Main_Event_Singularity;
                    captionRes = Resource.Array.Story_Main_Main_Event_Singularity_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_Singularity_TopTitle;
                    break;
                case 26:
                    titleRes = Resource.Array.Story_Main_Main_Event_DJMAX_1;
                    captionRes = Resource.Array.Story_Main_Main_Event_DJMAX_1_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_DJMAX_1_TopTitle;
                    break;
                case 27:
                    titleRes = Resource.Array.Story_Main_Main_Event_DJMAX_2;
                    captionRes = Resource.Array.Story_Main_Main_Event_DJMAX_2_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_DJMAX_2_TopTitle;
                    break;
                case 28:
                    titleRes = Resource.Array.Story_Main_Main_Event_ContinuumTurbulence;
                    captionRes = Resource.Array.Story_Main_Main_Event_ContinuumTurbulence_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_ContinuumTurbulence_TopTitle;
                    break;
                case 29:
                    titleRes = Resource.Array.Story_Main_Main_Event_Isomer;
                    captionRes = Resource.Array.Story_Main_Main_Event_Isomer_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_Isomer_TopTitle;
                    break;
                case 30:
                    titleRes = Resource.Array.Story_Main_Main_Event_VA;
                    captionRes = Resource.Array.Story_Main_Main_Event_VA_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_VA_TopTitle;
                    break;
                case 31:
                    titleRes = Resource.Array.Story_Main_Main_Event_ShatteredConnexion;
                    captionRes = Resource.Array.Story_Main_Main_Event_ShatteredConnexion_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_ShatteredConnexion_TopTitle;
                    break;
                case 32:
                    titleRes = Resource.Array.Story_Main_Main_Event_2019Halloween;
                    captionRes = Resource.Array.Story_Main_Main_Event_2019Halloween_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_2019Halloween_TopTitle;
                    break;
                case 33:
                    titleRes = Resource.Array.Story_Main_Main_Event_2019Christmas;
                    captionRes = Resource.Array.Story_Main_Main_Event_2019Christmas_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_2019Christmas_TopTitle;
                    break;
                case 34:
                    titleRes = Resource.Array.Story_Main_Main_Event_PolarizedLight;
                    captionRes = Resource.Array.Story_Main_Main_Event_PolarizedLight_Caption;
                    topTitleRes = Resource.Array.Story_Main_Main_Event_PolarizedLight_TopTitle;
                    break;
            }

            itemList.AddRange(Resources.GetStringArray(titleRes));
            captionList.AddRange(Resources.GetStringArray(captionRes));
            topTitleList.AddRange(Resources.GetStringArray(topTitleRes));

            itemList.TrimExcess();
            captionList.TrimExcess();
            topTitleList.TrimExcess();

            adapter = new StoryListAdapter(itemList.ToArray(), topTitleList.ToArray(), captionList.ToArray());
            adapter.itemClick += Adapter_ItemClick;
        }

        private void ListStoryItem_Sub()
        {
            itemList.Clear();

            int titleRes = 0;
            int captionRes = 0;
            int topTitleRes = 0;

            switch (subMainIndex)
            {
                case 0:
                    titleRes = Resource.Array.Story_Main_Sub_2016MessyHalloween;
                    captionRes = Resource.Array.Story_Main_Sub_2016MessyHalloween_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2016MessyHalloween_TopTitle;
                    break;
                case 1:
                    titleRes = Resource.Array.Story_Main_Sub_2016TacticalChristmas;
                    captionRes = Resource.Array.Story_Main_Sub_2016TacticalChristmas_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2016TacticalChristmas_TopTitle;
                    break;
                case 2:
                    titleRes = Resource.Array.Story_Main_Sub_2017Anniversary;
                    captionRes = Resource.Array.Story_Main_Sub_2017Anniversary_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2017Anniversary_TopTitle;
                    break;
                case 3:
                    titleRes = Resource.Array.Story_Main_Sub_2017OperaPrinces;
                    captionRes = Resource.Array.Story_Main_Sub_2017OperaPrinces_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2017OperaPrinces_TopTitle;
                    break;
                case 4:
                    titleRes = Resource.Array.Story_Main_Sub_2018LunaNewYear;
                    captionRes = Resource.Array.Story_Main_Sub_2018LunaNewYear_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2018LunaNewYear_TopTitle;
                    break;
                case 5:
                    titleRes = Resource.Array.Story_Main_Sub_2018SweetWedding;
                    captionRes = Resource.Array.Story_Main_Sub_2018SweetWedding_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2018SweetWedding_TopTitle;
                    break;
                case 6:
                    titleRes = Resource.Array.Story_Main_Sub_2018Anniversary;
                    captionRes = Resource.Array.Story_Main_Sub_2018Anniversary_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2018Anniversary_TopTitle;
                    break;
                case 7:
                    titleRes = Resource.Array.Story_Main_Sub_2018MaidTraining;
                    captionRes = Resource.Array.Story_Main_Sub_2018MaidTraining_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2018MaidTraining_TopTitle;
                    break;
                case 8:
                    titleRes = Resource.Array.Story_Main_Sub_2018BeachParty;
                    captionRes = Resource.Array.Story_Main_Sub_2018BeachParty_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2018BeachParty_TopTitle;
                    break;
                case 9:
                    titleRes = Resource.Array.Story_Main_Sub_2018RiseoftheWitches;
                    captionRes = Resource.Array.Story_Main_Sub_2018RiseoftheWitches_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2018RiseoftheWitches_TopTitle;
                    break;
                case 10:
                    titleRes = Resource.Array.Story_Main_Sub_2018AnotherChristmas;
                    captionRes = Resource.Array.Story_Main_Sub_2018AnotherChristmas_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2018AnotherChristmas_TopTitle;
                    break;
                case 11:
                    titleRes = Resource.Array.Story_Main_Sub_2019LunaNewYear;
                    captionRes = Resource.Array.Story_Main_Sub_2019LunaNewYear_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2019LunaNewYear_TopTitle;
                    break;
                case 12:
                    titleRes = Resource.Array.Story_Main_Sub_2019GunRose;
                    captionRes = Resource.Array.Story_Main_Sub_2019GunRose_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2019GunRose_TopTitle;
                    break;
                case 13:
                    titleRes = Resource.Array.Story_Main_Sub_2019Anniversary;
                    captionRes = Resource.Array.Story_Main_Sub_2019Anniversary_Caption;
                    topTitleRes = Resource.Array.Story_Main_Sub_2019Anniversary_TopTitle;
                    break;
            }

            itemList.AddRange(Resources.GetStringArray(titleRes));
            captionList.AddRange(Resources.GetStringArray(captionRes));
            topTitleList.AddRange(Resources.GetStringArray(topTitleRes));

            itemList.TrimExcess();
            captionList.TrimExcess();
            topTitleList.TrimExcess();

            adapter = new StoryListAdapter(itemList.ToArray(), topTitleList.ToArray(), captionList.ToArray());
            adapter.itemClick += Adapter_ItemClick;
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
                        category = $"Area_{subMainIndex - 1}";
                        break;
                    case 14:
                        category = "Cube";
                        break;
                    case 15:
                        category = "Hypothermia_1";
                        break;
                    case 16:
                        category = "Hypothermia_2";
                        break;
                    case 17:
                        category = "Hypothermia_3";
                        break;
                    case 18:
                        category = "Hypothermia_Hidden";
                        break;
                    case 19:
                        category = "CubePlus";
                        break;
                    case 20:
                        category = "GuiltyGear";
                        break;
                    case 21:
                        category = "DeepDive_1";
                        break;
                    case 22:
                        category = "DeepDive_2";
                        break;
                    case 23:
                        category = "DeepDive_3";
                        break;
                    case 24:
                        category = "DeepDive_Hidden";
                        break;
                    case 25:
                        category = "Singularity";
                        break;
                    case 26:
                        category = "DJMAX_1";
                        break;
                    case 27:
                        category = "DJMAX_2";
                        break;
                    case 28:
                        category = "ContinuumTurbulence";
                        break;
                    case 29:
                        category = "Isomer";
                        break;
                    case 30:
                        category = "VA";
                        break;
                    case 31:
                        category = "ShatteredConnexion";
                        break;
                    case 32:
                        category = "2019Halloween";
                        break;
                    case 33:
                        category = "2019Christmas";
                        break;
                    case 34:
                        category = "PolarizedLight";
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

            var intent = new Intent(this, typeof(StoryReaderActivity));
            intent.PutExtra("Info", new string[] { top, category, itemIndex.ToString(), itemList.Count.ToString() });
            intent.PutExtra("List", itemList.ToArray());
            intent.PutExtra("TopList", topTitleList.ToArray());
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }
    }
}