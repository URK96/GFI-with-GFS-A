using Android.Content;
using Android.Support.V7.App;

namespace GFI_with_GFS_A
{
    public partial class StoryActivity : AppCompatActivity
    {
        private void ListStoryItem_Main()
        {
            Item_List.Clear();

            int Title_res = 0;
            int Caption_res = 0;
            int TopTitle_res = 0;

            switch (SubMain_Index)
            {
                case 0:
                    Title_res = Resource.Array.Story_Main_Main_Prologue;
                    Caption_res = Resource.Array.Story_Main_Main_Prologue_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Prologue_TopTitle;
                    break;
                case 1:
                    Title_res = Resource.Array.Story_Main_Main_Area_0;
                    Caption_res = Resource.Array.Story_Main_Main_Area_0_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_0_TopTitle;
                    break;
                case 2:
                    Title_res = Resource.Array.Story_Main_Main_Area_1;
                    Caption_res = Resource.Array.Story_Main_Main_Area_1_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_1_TopTitle;
                    break;
                case 3:
                    Title_res = Resource.Array.Story_Main_Main_Area_2;
                    Caption_res = Resource.Array.Story_Main_Main_Area_2_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_2_TopTitle;
                    break;
                case 4:
                    Title_res = Resource.Array.Story_Main_Main_Area_3;
                    Caption_res = Resource.Array.Story_Main_Main_Area_3_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_3_TopTitle;
                    break;
                case 5:
                    Title_res = Resource.Array.Story_Main_Main_Area_4;
                    Caption_res = Resource.Array.Story_Main_Main_Area_4_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_4_TopTitle;
                    break;
                case 6:
                    Title_res = Resource.Array.Story_Main_Main_Area_5;
                    Caption_res = Resource.Array.Story_Main_Main_Area_5_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_5_TopTitle;
                    break;
                case 7:
                    Title_res = Resource.Array.Story_Main_Main_Area_6;
                    Caption_res = Resource.Array.Story_Main_Main_Area_6_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_6_TopTitle;
                    break;
                case 8:
                    Title_res = Resource.Array.Story_Main_Main_Area_7;
                    Caption_res = Resource.Array.Story_Main_Main_Area_7_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_7_TopTitle;
                    break;
                case 9:
                    Title_res = Resource.Array.Story_Main_Main_Area_8;
                    Caption_res = Resource.Array.Story_Main_Main_Area_8_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_8_TopTitle;
                    break;
                case 10:
                    Title_res = Resource.Array.Story_Main_Main_Area_9;
                    Caption_res = Resource.Array.Story_Main_Main_Area_9_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_9_TopTitle;
                    break;
                case 11:
                    Title_res = Resource.Array.Story_Main_Main_Area_10;
                    Caption_res = Resource.Array.Story_Main_Main_Area_10_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_10_TopTitle;
                    break;
                case 12:
                    Title_res = Resource.Array.Story_Main_Main_Area_11;
                    Caption_res = Resource.Array.Story_Main_Main_Area_11_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Area_11_TopTitle;
                    break;
                case 13:
                    Title_res = Resource.Array.Story_Main_Main_Event_Cube;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Cube_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_Cube_TopTitle;
                    break;
                case 14:
                    Title_res = Resource.Array.Story_Main_Main_Event_Hypothermia_1;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Hypothermia_1_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_Hypothermia_1_TopTitle;
                    break;
                case 15:
                    Title_res = Resource.Array.Story_Main_Main_Event_Hypothermia_2;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Hypothermia_2_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_Hypothermia_2_TopTitle;
                    break;
                case 16:
                    Title_res = Resource.Array.Story_Main_Main_Event_Hypothermia_3;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Hypothermia_3_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_Hypothermia_3_TopTitle;
                    break;
                case 17:
                    Title_res = Resource.Array.Story_Main_Main_Event_Hypothermia_Hidden;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Hypothermia_Hidden_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_Hypothermia_Hidden_TopTitle;
                    break;
                case 18:
                    Title_res = Resource.Array.Story_Main_Main_Event_CubePlus;
                    Caption_res = Resource.Array.Story_Main_Main_Event_CubePlus_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_CubePlus_Toptitle;
                    break;
                case 19:
                    Title_res = Resource.Array.Story_Main_Main_Event_GuiltyGear;
                    Caption_res = Resource.Array.Story_Main_Main_Event_GuiltyGear_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_GuiltyGear_Toptitle;
                    break;
                case 20:
                    Title_res = Resource.Array.Story_Main_Main_Event_DeepDive_1;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DeepDive_1_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_DeepDive_1_TopTitle;
                    break;
                case 21:
                    Title_res = Resource.Array.Story_Main_Main_Event_DeepDive_2;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DeepDive_2_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_DeepDive_2_TopTitle;
                    break;
                case 22:
                    Title_res = Resource.Array.Story_Main_Main_Event_DeepDive_3;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DeepDive_3_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_DeepDive_3_TopTitle;
                    break;
                case 23:
                    Title_res = Resource.Array.Story_Main_Main_Event_DeepDive_Hidden;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DeepDive_Hidden_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_DeepDive_Hidden_TopTitle;
                    break;
                case 24:
                    Title_res = Resource.Array.Story_Main_Main_Event_Singularity;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Singularity_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_Singularity_TopTitle;
                    break;
                case 25:
                    Title_res = Resource.Array.Story_Main_Main_Event_DJMAX_1;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DJMAX_1_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_DJMAX_1_TopTitle;
                    break;
                case 26:
                    Title_res = Resource.Array.Story_Main_Main_Event_DJMAX_2;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DJMAX_2_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_DJMAX_2_TopTitle;
                    break;
                case 27:
                    Title_res = Resource.Array.Story_Main_Main_Event_ContinuumTurbulence;
                    Caption_res = Resource.Array.Story_Main_Main_Event_ContinuumTurbulence_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_ContinuumTurbulence_TopTitle;
                    break;
                case 28:
                    Title_res = Resource.Array.Story_Main_Main_Event_Isomer;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Isomer_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_Isomer_TopTitle;
                    break;
                case 29:
                    Title_res = Resource.Array.Story_Main_Main_Event_VA;
                    Caption_res = Resource.Array.Story_Main_Main_Event_VA_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Main_Event_VA_TopTitle;
                    break;
            }

            Item_List.AddRange(Resources.GetStringArray(Title_res));
            Caption_List.AddRange(Resources.GetStringArray(Caption_res));
            TopTitle_List.AddRange(Resources.GetStringArray(TopTitle_res));
            Item_List.TrimExcess();
            Caption_List.TrimExcess();
            TopTitle_List.TrimExcess();

            adapter = new StoryListAdapter(Item_List.ToArray(), TopTitle_List.ToArray(), Caption_List.ToArray());
            adapter.ItemClick += Adapter_ItemClick;
        }

        private void ListStoryItem_Sub()
        {
            Item_List.Clear();

            int Title_res = 0;
            int Caption_res = 0;
            int TopTitle_res = 0;

            switch (SubMain_Index)
            {
                case 0:
                    Title_res = Resource.Array.Story_Main_Sub_2016MessyHalloween;
                    Caption_res = Resource.Array.Story_Main_Sub_2016MessyHalloween_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2016MessyHalloween_TopTitle;
                    break;
                case 1:
                    Title_res = Resource.Array.Story_Main_Sub_2016TacticalChristmas;
                    Caption_res = Resource.Array.Story_Main_Sub_2016TacticalChristmas_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2016TacticalChristmas_TopTitle;
                    break;
                case 2:
                    Title_res = Resource.Array.Story_Main_Sub_2017Anniversary;
                    Caption_res = Resource.Array.Story_Main_Sub_2017Anniversary_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2017Anniversary_TopTitle;
                    break;
                case 3:
                    Title_res = Resource.Array.Story_Main_Sub_2017OperaPrinces;
                    Caption_res = Resource.Array.Story_Main_Sub_2017OperaPrinces_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2017OperaPrinces_TopTitle;
                    break;
                case 4:
                    Title_res = Resource.Array.Story_Main_Sub_2018LunaNewYear;
                    Caption_res = Resource.Array.Story_Main_Sub_2018LunaNewYear_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2018LunaNewYear_TopTitle;
                    break;
                case 5:
                    Title_res = Resource.Array.Story_Main_Sub_2018SweetWedding;
                    Caption_res = Resource.Array.Story_Main_Sub_2018SweetWedding_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2018SweetWedding_TopTitle;
                    break;
                case 6:
                    Title_res = Resource.Array.Story_Main_Sub_2018Anniversary;
                    Caption_res = Resource.Array.Story_Main_Sub_2018Anniversary_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2018Anniversary_TopTitle;
                    break;
                case 7:
                    Title_res = Resource.Array.Story_Main_Sub_2018MaidTraining;
                    Caption_res = Resource.Array.Story_Main_Sub_2018MaidTraining_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2018MaidTraining_TopTitle;
                    break;
                case 8:
                    Title_res = Resource.Array.Story_Main_Sub_2018BeachParty;
                    Caption_res = Resource.Array.Story_Main_Sub_2018BeachParty_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2018BeachParty_TopTitle;
                    break;
                case 9:
                    Title_res = Resource.Array.Story_Main_Sub_2018RiseoftheWitches;
                    Caption_res = Resource.Array.Story_Main_Sub_2018RiseoftheWitches_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2018RiseoftheWitches_TopTitle;
                    break;
                case 10:
                    Title_res = Resource.Array.Story_Main_Sub_2018AnotherChristmas;
                    Caption_res = Resource.Array.Story_Main_Sub_2018AnotherChristmas_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2018AnotherChristmas_TopTitle;
                    break;
                case 11:
                    Title_res = Resource.Array.Story_Main_Sub_2019LunaNewYear;
                    Caption_res = Resource.Array.Story_Main_Sub_2019LunaNewYear_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2019LunaNewYear_TopTitle;
                    break;
                case 12:
                    Title_res = Resource.Array.Story_Main_Sub_2019GunRose;
                    Caption_res = Resource.Array.Story_Main_Sub_2019GunRose_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2019GunRose_TopTitle;
                    break;
                case 13:
                    Title_res = Resource.Array.Story_Main_Sub_2019Anniversary;
                    Caption_res = Resource.Array.Story_Main_Sub_2019Anniversary_Caption;
                    TopTitle_res = Resource.Array.Story_Main_Sub_2019Anniversary_TopTitle;
                    break;
            }

            Item_List.AddRange(Resources.GetStringArray(Title_res));
            Caption_List.AddRange(Resources.GetStringArray(Caption_res));
            TopTitle_List.AddRange(Resources.GetStringArray(TopTitle_res));
            Item_List.TrimExcess();
            Caption_List.TrimExcess();
            TopTitle_List.TrimExcess();

            adapter = new StoryListAdapter(Item_List.ToArray(), TopTitle_List.ToArray(), Caption_List.ToArray());
            adapter.ItemClick += Adapter_ItemClick;
        }


        private void RunReader()
        {
            string Top = "";
            string Category = "";

            switch (TopType)
            {
                case StoryActivity.Top.Main:
                    Top = "Main";
                    break;
                case StoryActivity.Top.Sub:
                    Top = "Sub";
                    break;
            }

            if (TopType == StoryActivity.Top.Main)
            {
                switch (SubMain_Index)
                {
                    case 0:
                        Category = "Prologue";
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
                        Category = $"Area_{SubMain_Index - 1}";
                        break;
                    case 13:
                        Category = "Cube";
                        break;
                    case 14:
                        Category = "Hypothermia_1";
                        break;
                    case 15:
                        Category = "Hypothermia_2";
                        break;
                    case 16:
                        Category = "Hypothermia_3";
                        break;
                    case 17:
                        Category = "Hypothermia_Hidden";
                        break;
                    case 18:
                        Category = "CubePlus";
                        break;
                    case 19:
                        Category = "GuiltyGear";
                        break;
                    case 20:
                        Category = "DeepDive_1";
                        break;
                    case 21:
                        Category = "DeepDive_2";
                        break;
                    case 22:
                        Category = "DeepDive_3";
                        break;
                    case 23:
                        Category = "DeepDive_Hidden";
                        break;
                    case 24:
                        Category = "Singularity";
                        break;
                    case 25:
                        Category = "DJMAX_1";
                        break;
                    case 26:
                        Category = "DJMAX_2";
                        break;
                    case 27:
                        Category = "ContinuumTurbulence";
                        break;
                    case 28:
                        Category = "Isomer";
                        break;
                    case 29:
                        Category = "VA";
                        break;
                }
            }
            else if (TopType == StoryActivity.Top.Sub)
            {
                switch (SubMain_Index)
                {
                    case 0:
                        Category = "2016MessyHalloween";
                        break;
                    case 1:
                        Category = "2016TacticalChristmas";
                        break;
                    case 2:
                        Category = "2017Anniversary";
                        break;
                    case 3:
                        Category = "2017OperaPrinces";
                        break;
                    case 4:
                        Category = "2018LunaNewYear";
                        break;
                    case 5:
                        Category = "2018SweetWedding";
                        break;
                    case 6:
                        Category = "2018Anniversary";
                        break;
                    case 7:
                        Category = "2018MaidTraining";
                        break;
                    case 8:
                        Category = "2018BeachParty";
                        break;
                    case 9:
                        Category = "2018RiseoftheWitches";
                        break;
                    case 10:
                        Category = "2018AnotherChristmas";
                        break;
                    case 11:
                        Category = "2019LunaNewYear";
                        break;
                    case 12:
                        Category = "2019GunRose";
                        break;
                    case 13:
                        Category = "2019Anniversary";
                        break;
                }
            }

            var intent = new Intent(this, typeof(StoryReaderActivity));
            intent.PutExtra("Info", new string[] { Top, Category, Item_Index.ToString(), Item_List.Count.ToString() });
            intent.PutExtra("List", Item_List.ToArray());
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }
    }
}