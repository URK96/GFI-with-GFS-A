using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace GFI_with_GFS_A
{
    public partial class StoryActivity : AppCompatActivity
    {
        private void ListStoryItem_Main()
        {
            Item_List.Clear();

            int Title_res = 0;
            int Caption_res = 0;

            switch (SubMain_Index)
            {
                case 0:
                    Title_res = Resource.Array.Story_Main_Main_Prologue;
                    Caption_res = Resource.Array.Story_Main_Main_Prologue_Caption;
                    break;
                case 1:
                    Title_res = Resource.Array.Story_Main_Main_Area_0;
                    Caption_res = Resource.Array.Story_Main_Main_Area_0_Caption;
                    break;
                case 2:
                    Title_res = Resource.Array.Story_Main_Main_Area_1;
                    Caption_res = Resource.Array.Story_Main_Main_Area_1_Caption;
                    break;
                case 3:
                    Title_res = Resource.Array.Story_Main_Main_Area_2;
                    Caption_res = Resource.Array.Story_Main_Main_Area_2_Caption;
                    break;
                case 4:
                    Title_res = Resource.Array.Story_Main_Main_Area_3;
                    Caption_res = Resource.Array.Story_Main_Main_Area_3_Caption;
                    break;
                case 5:
                    Title_res = Resource.Array.Story_Main_Main_Area_4;
                    Caption_res = Resource.Array.Story_Main_Main_Area_4_Caption;
                    break;
                case 6:
                    Title_res = Resource.Array.Story_Main_Main_Area_5;
                    Caption_res = Resource.Array.Story_Main_Main_Area_5_Caption;
                    break;
                case 7:
                    Title_res = Resource.Array.Story_Main_Main_Area_6;
                    Caption_res = Resource.Array.Story_Main_Main_Area_6_Caption;
                    break;
                case 8:
                    Title_res = Resource.Array.Story_Main_Main_Area_7;
                    Caption_res = Resource.Array.Story_Main_Main_Area_7_Caption;
                    break;
                case 9:
                    Title_res = Resource.Array.Story_Main_Main_Area_8;
                    Caption_res = Resource.Array.Story_Main_Main_Area_8_Caption;
                    break;
                case 10:
                    Title_res = Resource.Array.Story_Main_Main_Area_9;
                    Caption_res = Resource.Array.Story_Main_Main_Area_9_Caption;
                    break;
                case 11:
                    Title_res = Resource.Array.Story_Main_Main_Area_10;
                    Caption_res = Resource.Array.Story_Main_Main_Area_10_Caption;
                    break;
                case 12:
                    Title_res = Resource.Array.Story_Main_Main_Area_11;
                    Caption_res = Resource.Array.Story_Main_Main_Area_11_Caption;
                    break;
                case 13:
                    Title_res = Resource.Array.Story_Main_Main_Event_Cube;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Cube_Caption;
                    break;
                case 14:
                    Title_res = Resource.Array.Story_Main_Main_Event_Hypothermia_1;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Hypothermia_1_Caption;
                    break;
                case 15:
                    Title_res = Resource.Array.Story_Main_Main_Event_Hypothermia_2;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Hypothermia_2_Caption;
                    break;
                case 16:
                    Title_res = Resource.Array.Story_Main_Main_Event_Hypothermia_3;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Hypothermia_3_Caption;
                    break;
                case 17:
                    Title_res = Resource.Array.Story_Main_Main_Event_Hypothermia_Hidden;
                    Caption_res = Resource.Array.Story_Main_Main_Event_Hypothermia_Hidden_Caption;
                    break;
                case 18:
                    Title_res = Resource.Array.Story_Main_Main_Event_CubePlus;
                    Caption_res = Resource.Array.Story_Main_Main_Event_CubePlus_Caption;
                    break;
                case 19:
                    Title_res = Resource.Array.Story_Main_Main_Event_GuiltyGear;
                    Caption_res = Resource.Array.Story_Main_Main_Event_GuiltyGear_Caption;
                    break;
                case 20:
                    Title_res = Resource.Array.Story_Main_Main_Event_DeepDive_1;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DeepDive_1_Caption;
                    break;
                case 21:
                    Title_res = Resource.Array.Story_Main_Main_Event_DeepDive_2;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DeepDive_2_Caption;
                    break;
                case 22:
                    Title_res = Resource.Array.Story_Main_Main_Event_DeepDive_3;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DeepDive_3_Caption;
                    break;
                case 23:
                    Title_res = Resource.Array.Story_Main_Main_Event_DeepDive_Hidden;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DeepDive_Hidden_Caption;
                    break;
                case 24:
                    break;
                case 25:
                    Title_res = Resource.Array.Story_Main_Main_Event_DJMAX_1;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DJMAX_1_Caption;
                    break;
                case 26:
                    Title_res = Resource.Array.Story_Main_Main_Event_DJMAX_2;
                    Caption_res = Resource.Array.Story_Main_Main_Event_DJMAX_2_Caption;
                    break;
                case 27:
                    break;
            }

            Item_List.AddRange(Resources.GetStringArray(Title_res));
            Caption_List.AddRange(Resources.GetStringArray(Caption_res));
            Item_List.TrimExcess();
            Caption_List.TrimExcess();

            adapter = new StoryListAdapter(Item_List.ToArray(), Caption_List.ToArray());
            adapter.ItemClick += Adapter_ItemClick;
        }
    }
}