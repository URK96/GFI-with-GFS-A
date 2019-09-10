using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    [Activity(Label = "ProductPercentTableActivity", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ProductPercentTableActivity : FragmentActivity
    {
        enum Category { Doll, Equip, Fairy }
        enum ShowMode { Normal, Advance }
        enum ListMode { TotalCount, Percentage }

        private Category ProductCategory = Category.Doll;
        private ShowMode Show_Mode = ShowMode.Normal;
        private ListMode List_Mode = ListMode.Percentage;

        private int Id;
        private string TopURL = "";

        private LinearLayout TableMainLayout;

        private ProgressBar InitLoadProgressBar;
        private Button ChangeShowMode;
        private Button ChangeListMode;

        private double[,] Normal_Data;
        private double[,] Advance_Data;

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

            if (ETC.UseLightTheme)
                SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.ProductPercentTableLayout);

            InitLoadProgressBar = FindViewById<ProgressBar>(Resource.Id.ProductPercentTableInitLoadProgress);
            ChangeShowMode = FindViewById<Button>(Resource.Id.ProductPercentTableChangeShowModeButton);
            ChangeShowMode.Click += ChangeModeButton_Click;
            ChangeListMode = FindViewById<Button>(Resource.Id.ProductPercentTableChangeListModeButton);
            ChangeListMode.Click += ChangeModeButton_Click;

            TableMainLayout = FindViewById<LinearLayout>(Resource.Id.ProductPercentTable_MainTableLayout);

            string[] data = Intent.GetStringArrayExtra("Info");

            Id = int.Parse(data[1]);

            switch (data[0])
            {
                case "Doll":
                    ProductCategory = Category.Doll;
                    TopURL = "http://db.baka.pw:9999/stats/tdoll/id/";
                    break;
                case "Equip":
                    ProductCategory = Category.Equip;
                    TopURL = "http://db.baka.pw:9999/stats/equip/id/";
                    break;
                case "Fairy":
                    ProductCategory = Category.Fairy;
                    TopURL = "http://db.baka.pw:9999/stats/fairy/id/";
                    Show_Mode = ShowMode.Advance;
                    ChangeShowMode.Text = Resources.GetString(Resource.String.Common_AdvanceProduct);
                    break;
            }
            
            _ = ProcessData();
        }

        private void ChangeModeButton_Click(object sender, EventArgs e)
        {
            Button bt = sender as Button;

            try
            {
                switch (bt.Id)
                {
                    case Resource.Id.ProductPercentTableChangeShowModeButton:
                        if (Show_Mode == ShowMode.Normal)
                        {
                            Show_Mode = ShowMode.Advance;
                            bt.Text = Resources.GetString(Resource.String.Common_AdvanceProduct);
                        }
                        else if (Show_Mode == ShowMode.Advance)
                        {
                            Show_Mode = ShowMode.Normal;
                            bt.Text = Resources.GetString(Resource.String.Common_NormalProduct);
                        }
                        else
                        {
                            Show_Mode = ShowMode.Normal;
                            bt.Text = Resources.GetString(Resource.String.Common_NormalProduct);
                        }
                        break;
                    case Resource.Id.ProductPercentTableChangeListModeButton:
                        if (List_Mode == ListMode.Percentage)
                        {
                            List_Mode = ListMode.TotalCount;
                            bt.Text = Resources.GetString(Resource.String.ProductPercentTable_List_TotalCount);
                        }
                        else if (List_Mode == ListMode.TotalCount)
                        {
                            List_Mode = ListMode.Percentage;
                            bt.Text = Resources.GetString(Resource.String.ProductPercentTable_List_Percentage);
                        }
                        else
                        {
                            List_Mode = ListMode.Percentage;
                            bt.Text = Resources.GetString(Resource.String.ProductPercentTable_List_Percentage);
                        }
                        break;
                }

                _ = LoadList();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private async Task LoadList()
        {
            double[,] list;

            try
            {
                TableMainLayout.RemoveAllViews();

                ChangeShowMode.Enabled = false;
                ChangeListMode.Enabled = false;

                if (InitLoadProgressBar.Visibility != ViewStates.Visible) InitLoadProgressBar.Visibility = ViewStates.Visible;

                switch (Show_Mode)
                {
                    case ShowMode.Normal:
                    default:
                        list = new double[(Normal_Data.Length / 8), 8];
                        list = Normal_Data;
                        break;
                    case ShowMode.Advance:
                        list = new double[(Advance_Data.Length / 8), 8];
                        list = Advance_Data;
                        break;
                }

                list = await SortList(list);

                if (list == null) throw new Exception();

                InitLoadProgressBar.Indeterminate = false;
                InitLoadProgressBar.Max = list.Length / TopViewIds.Length;
                InitLoadProgressBar.Progress = 0;

                await Task.Delay(100);

                for (int i = 0; i < (list.Length / TopViewIds.Length); ++i)
                {
                    LinearLayout layout = new LinearLayout(this)
                    {
                        Orientation = Orientation.Horizontal,
                        LayoutParameters = TableMainLayout.LayoutParameters
                    };
                    layout.SetGravity(GravityFlags.Center);

                    for (int j = 0; j < TopViewIds.Length; ++j)
                    {
                        TextView tv = new TextView(this)
                        {
                            LayoutParameters = FindViewById<TextView>(TopViewIds[j]).LayoutParameters
                        };

                        if (j == 8) tv.Text = string.Format("{0}%", list[i, j]);
                        else tv.Text = list[i, j].ToString();

                        tv.Gravity = GravityFlags.Center;
                        layout.AddView(tv);
                    }

                    InitLoadProgressBar.Progress += 1;
                    await Task.Delay(1);
                    TableMainLayout.AddView(layout);
                }

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
            finally
            {
                InitLoadProgressBar.Visibility = ViewStates.Invisible;
                InitLoadProgressBar.Indeterminate = true;
                ChangeListMode.Enabled = true;
                ChangeShowMode.Enabled = true;
            }
        }

        private async Task<double[,]> SortList(double[,] list)
        {
            await Task.Delay(100);
            try
            {
                double[] temp = new double[TopViewIds.Length];
                int index = 0;

                switch (List_Mode)
                {
                    case ListMode.Percentage:
                    default:
                        index = TopViewIds.Length - 1;
                        break;
                    case ListMode.TotalCount:
                        index = TopViewIds.Length - 2;
                        break;
                }

                for (int i = 0; i < (list.Length / TopViewIds.Length); ++i)
                {
                    int max_index = i;

                    for (int j = i; j < (list.Length / TopViewIds.Length); ++j)
                    {
                        if (list[max_index, index] < list[j, index]) max_index = j;
                    }

                    if (max_index != i)
                    {
                        for (int k = 0; k < temp.Length; ++k)
                        {
                            temp[k] = list[i, k];
                            list[i, k] = list[max_index, k];
                            list[max_index, k] = temp[k];
                        }
                    }
                }

                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task ProcessData()
        {
            try
            {
                InitLoadProgressBar.Visibility = ViewStates.Visible;
                InitLoadProgressBar.BringToFront();

                string url = string.Format("{0}{1}", TopURL, Id);
                string temp_data;

                using (WebClient wc = new WebClient())
                {
                    temp_data = await wc.DownloadStringTaskAsync(url);
                }

                StringBuilder sb = new StringBuilder(temp_data);

                sb.Replace("\"formula\":", " ");

                List<int> remove_list = new List<int>();

                for (int i = 0; i < sb.Length; ++i)
                {
                    if ((sb[i] == '[') || (sb[i] == ']') || (sb[i] == '{') || (sb[i] == '}') || (char.IsWhiteSpace(sb[i]) == true))
                    {
                        remove_list.Add(i);
                    }
                }

                remove_list.TrimExcess();

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
                List<int> normal_row = new List<int>();
                int advance_count = 0;
                List<int> advance_row = new List<int>();

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

                normal_row.TrimExcess();
                advance_row.TrimExcess();

                Normal_Data = new double[normal_count, 9];
                Advance_Data = new double[advance_count, 9];

                for (int i = 0; i < (Normal_Data.Length / TopViewIds.Length); ++i)
                {
                    int row = normal_row[i];

                    if (i == 197)
                    {
                        await Task.Delay(1);
                    }

                    for (int j = 0; j < TopViewIds.Length; j++)
                    {
                        if (j == 8) Normal_Data[i, j] = Math.Round(((Normal_Data[i, 6] / Normal_Data[i, 7]) * 100), 4);
                        else Normal_Data[i, j] = double.Parse(data[((row * 8) + j)].Split(':')[1]);
                    }
                }

                if (advance_count != 0)
                {
                    for (int i = 0; i < (Advance_Data.Length / TopViewIds.Length); ++i)
                    {
                        int row = advance_row[i];

                        for (int j = 0; j < TopViewIds.Length; j++)
                        {
                            if (j == 8) Advance_Data[i, j] = Math.Round(((Advance_Data[i, 6] / Advance_Data[i, 7]) * 100), 4);
                            else Advance_Data[i, j] = int.Parse(data[((row * 8) + j)].Split(':')[1]);
                        }
                    }
                }

                await Task.Delay(500);

                _ = LoadList();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                InitLoadProgressBar.Visibility = ViewStates.Invisible;
            }
        }
    }
}