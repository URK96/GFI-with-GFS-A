using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Label = "DollDBProductPercentTableActivity", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DollDBProductPercentTableActivity : FragmentActivity
    {
        enum ShowMode { Normal, Advance }
        private ShowMode Show_Mode = ShowMode.Normal;

        private int DollDicNum;

        private LinearLayout TableMainLayout;

        private ProgressBar InitLoadProgressBar;
        private Button ChangeShowMode;
        private Button ChangeListMode;

        private int[,] Normal_Data;
        private int[,] Advance_Data;

        private readonly int[] TopViewIds =
        {
            Resource.Id.ProductPercentTable_TableTop_Manpower,
            Resource.Id.ProductPercentTable_TableTop_Ammo,
            Resource.Id.ProductPercentTable_TableTop_MRE,
            Resource.Id.ProductPercentTable_TableTop_Parts,
            Resource.Id.ProductPercentTable_TableTop_ProductType,
            Resource.Id.ProductPercentTable_TableTop_ProductCategory,
            Resource.Id.ProductPercentTable_TableTop_HitCount,
            Resource.Id.ProductPercentTable_TableTop_TotalCount,
            Resource.Id.ProductPercentTable_TableTop_Percentage
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.ProductPercentTableLayout);

            DollDicNum = Convert.ToInt32(Intent.GetStringExtra("DollNum"));

            InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.ProductPercentTableInitLoadProgress);
            ChangeShowMode = FindViewById<Button>(Resource.Id.ProductPercentTableChangeShowModeButton);
            ChangeListMode = FindViewById<Button>(Resource.Id.ProductPercentTableChangeListModeButton);

            TableMainLayout = FindViewById<LinearLayout>(Resource.Id.ProductPercentTable_MainTableLayout);

            ProcessData();
        }

        private async Task LoadList()
        {
            int[,] list;

            switch (Show_Mode)
            {
                case ShowMode.Normal:
                default:
                    list = new int[(Normal_Data.Length / 8), 8];
                    break;
                case ShowMode.Advance:
                    list = new int[(Advance_Data.Length / 8), 8];
                    break;
            }

            list = Normal_Data;


            InitLoadProgressBar.Indeterminate = false;
            InitLoadProgressBar.Max = list.Length;
            InitLoadProgressBar.Progress = 0;
            await Task.Delay(100);

            for (int i = 0; i < (list.Length / 8); ++i)
            {
                LinearLayout layout = new LinearLayout(this);

                layout.Orientation = Orientation.Horizontal;
                layout.LayoutParameters = TableMainLayout.LayoutParameters;
                layout.SetGravity(GravityFlags.Center);

                for (int j = 0; j < TopViewIds.Length; ++j)
                {
                    TextView tv = new TextView(this);
                    tv.LayoutParameters = FindViewById<TextView>(TopViewIds[j]).LayoutParameters;

                    if (j == 8)
                    {
                        double value = (((double)list[i, 6] / list[i, 7]) * 100);
                        tv.Text = (Math.Round(value, 3)) + "%";
                    }
                    else tv.Text = list[i, j].ToString();
                    tv.Gravity = GravityFlags.Center;
                    layout.AddView(tv);
                    InitLoadProgressBar.Progress += 1;
                }
                await Task.Delay(1);
                TableMainLayout.AddView(layout);
            }

            await Task.Delay(500);

            InitLoadProgressBar.Visibility = ViewStates.Invisible;
        }

        private async Task ProcessData()
        {
            try
            {
                InitLoadProgressBar.Visibility = ViewStates.Visible;
                InitLoadProgressBar.BringToFront();

                string url = "https://ipick.baka.pw:444/stats/tdoll/id/" + DollDicNum;
                string temp_data;

                using (WebClient wc = new WebClient())
                {
                    temp_data = await wc.DownloadStringTaskAsync(url);
                }

                StringBuilder sb = new StringBuilder(temp_data);

                sb.Replace("\"formula\":", " ");

                ArrayList remove_list = new ArrayList();

                for (int i = 0; i < sb.Length; ++i)
                {
                    if ((sb[i] == '[') || (sb[i] == ']') || (sb[i] == '{') || (sb[i] == '}') || (char.IsWhiteSpace(sb[i]) == true))
                    {
                        remove_list.Add(i);
                    }
                }

                int count = 0;

                foreach (int index in remove_list)
                {
                    if ((index - count) >= 0) sb.Remove((index - count), 1);
                    else sb.Remove(0, 1);
                    count += 1;
                }

                await Task.Delay(100);

                temp_data = sb.ToString();

                string[] data = temp_data.Split(',');

                int normal_count = 0;
                ArrayList normal_row = new ArrayList();
                int advance_count = 0;
                ArrayList advance_row = new ArrayList();

                for (int i = 0; i < (data.Length / 8); ++i)
                {
                    int type = int.Parse(data[((i * 8) + 5)].Split(':')[1]);

                    switch (type)
                    {
                        case 0:
                        default:
                            normal_count += 1;
                            normal_row.Add(i);
                            break;
                        case 2:
                            advance_count += 1;
                            advance_row.Add(i);
                            break;
                    }
                }

                normal_row.TrimToSize();
                advance_row.TrimToSize();

                Normal_Data = new int[normal_count, 8];
                Advance_Data = new int[advance_count, 8];

                for (int i = 0; i < (Normal_Data.Length / 8); ++i)
                {
                    int row = (int)normal_row[i];

                    for (int j = 0; j < (TopViewIds.Length - 1); j++) Normal_Data[i,j] = int.Parse(data[((row * 8) + j)].Split(':')[1]);
                }

                if (advance_count != 0)
                {
                    for (int i = 0; i < (Advance_Data.Length / 8); ++i)
                    {
                        int row = (int)advance_row[i];

                        for (int j = 0; j < (TopViewIds.Length - 1); j++) Advance_Data[i, j] = int.Parse(data[((row * 8) + j)].Split(':')[1]);
                    }
                }

                await Task.Delay(500);

                LoadList();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }
    }
}