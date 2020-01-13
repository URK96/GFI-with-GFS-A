using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Com.Wang.Avi;

namespace GFI_with_GFS_A
{
    [Activity(Label = "ProductPercentTableActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ProductPercentTableActivity : BaseAppCompatActivity
    {
        enum Category { Doll, Equip, Fairy }
        enum ProductTypeMode { Normal, Advance }
        enum ListMode { TotalCount, Percentage }

        private Category productCategory = Category.Doll;
        private ProductTypeMode productTypeMode = ProductTypeMode.Normal;
        private ListMode listMode = ListMode.Percentage;

        private int id;
        private string topURL = "";
        private string listM = "";
        private string typeM = "";
        private bool isLoading = false;

        private AVLoadingIndicatorView loadingIndicatorView;
        private TextView loadingIndicatorExplain;
        private TextView nameToolbarTextView;
        private TextView modeToolbarTextView;
        private LinearLayout tableMainLayout;

        private double[,] normalData;
        private double[,] advanceData;

        private readonly int[] topViewIds =
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

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.ProductPercentTableLayout);

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.ProductPercentTableMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            loadingIndicatorView = FindViewById<AVLoadingIndicatorView>(Resource.Id.ProductPercentTableLoadingIndicatorView);
            loadingIndicatorView.SetIndicator("BallClipRotateMultipleIndicator");
            loadingIndicatorExplain = FindViewById<TextView>(Resource.Id.ProductPercentTableLoadingIndicatorExplainText);
            nameToolbarTextView = FindViewById<TextView>(Resource.Id.ProductPercentTableToolbarName);
            modeToolbarTextView = FindViewById<TextView>(Resource.Id.ProductPercentTableToolbarMode);

            tableMainLayout = FindViewById<LinearLayout>(Resource.Id.ProductPercentTable_MainTableLayout);

            string[] data = Intent.GetStringArrayExtra("Info");

            id = int.Parse(data[1]);

            switch (data[0])
            {
                case "Doll":
                    productCategory = Category.Doll;
                    topURL = "http://db.baka.pw:9999/stats/tdoll/id/";
                    nameToolbarTextView.Text = $"Doll No.{id}";
                    break;
                case "Equip":
                    productCategory = Category.Equip;
                    topURL = "http://db.baka.pw:9999/stats/equip/id/";
                    nameToolbarTextView.Text = $"Equip No.{id}";
                    break;
                case "Fairy":
                    productCategory = Category.Fairy;
                    topURL = "http://db.baka.pw:9999/stats/fairy/id/";
                    nameToolbarTextView.Text = $"Fairy No.{id}";
                    break;
            }

            typeM = Resources.GetString(Resource.String.ProductPercentTable_ProductType_Normal);
            listM = Resources.GetString(Resource.String.ProductPercentTable_List_Percentage);

            modeToolbarTextView.Text = $"{typeM} - {listM}";

            _ = ProcessData();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ProductPercentTableToolbarMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.ProductPercentTableChangeListMode:
                    ChangeShowMode(0);
                    break;
                case Resource.Id.ProductPercentTableChangeProductTypeMode:
                    ChangeShowMode(1);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode">Type Mode (0 : List, 1 : Product Type)</param>
        private void ChangeShowMode(int mode)
        {
            try
            {
                if (isLoading)
                {
                    return;
                }

                switch (mode)
                {
                    case 0 when (listMode == ListMode.Percentage):
                        listMode = ListMode.TotalCount;
                        listM = Resources.GetString(Resource.String.ProductPercentTable_List_TotalCount);
                        break;
                    case 0 when (listMode == ListMode.TotalCount):
                        listMode = ListMode.Percentage;
                        listM = Resources.GetString(Resource.String.ProductPercentTable_List_Percentage);
                        break;
                    case 1 when (productTypeMode == ProductTypeMode.Normal):
                        productTypeMode = ProductTypeMode.Advance;
                        typeM = Resources.GetString(Resource.String.ProductPercentTable_ProductType_Advance);
                        break;
                    case 1 when (productTypeMode == ProductTypeMode.Advance):
                        productTypeMode = ProductTypeMode.Normal;
                        typeM = Resources.GetString(Resource.String.ProductPercentTable_ProductType_Normal);
                        break;
                }

                modeToolbarTextView.Text = $"{typeM} - {listM}";

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
                isLoading = true;

                tableMainLayout.RemoveAllViews();

                switch (productTypeMode)
                {
                    case ProductTypeMode.Normal:
                    default:
                        list = new double[(normalData.Length / 8), 8];
                        list = normalData;
                        break;
                    case ProductTypeMode.Advance:
                        list = new double[(advanceData.Length / 8), 8];
                        list = advanceData;
                        break;
                }

                list = await SortList(list);

                if (list == null)
                {
                    throw new Exception();
                }

                await Task.Delay(100);

                for (int i = 0; i < (list.Length / topViewIds.Length); ++i)
                {
                    LinearLayout layout = new LinearLayout(this)
                    {
                        Orientation = Orientation.Horizontal,
                        LayoutParameters = tableMainLayout.LayoutParameters
                    };
                    layout.SetGravity(GravityFlags.Center);

                    for (int j = 0; j < topViewIds.Length; ++j)
                    {
                        TextView tv = new TextView(this)
                        {
                            LayoutParameters = FindViewById<TextView>(topViewIds[j]).LayoutParameters,
                            Gravity = GravityFlags.Center,
                            Text = (j == 8) ? $"{list[i, j]}%" : list[i, j].ToString()
                        };

                        /*if (j == 8)
                        {
                            tv.Text = string.Format("{0}%", list[i, j]);
                        }
                        else
                        {
                            tv.Text = list[i, j].ToString();
                        }*/

                        layout.AddView(tv);
                    }

                    await Task.Delay(1);
                    tableMainLayout.AddView(layout);
                }

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task<double[,]> SortList(double[,] list)
        {
            await Task.Delay(10);

            try
            {
                double[] temp = new double[topViewIds.Length];
                int index = 0;

                index = listMode switch
                {
                    ListMode.TotalCount => topViewIds.Length - 2,
                    _ => topViewIds.Length - 1,
                };

                for (int i = 0; i < (list.Length / topViewIds.Length); ++i)
                {
                    int maxIndex = i;

                    for (int j = i; j < (list.Length / topViewIds.Length); ++j)
                    {
                        if (list[maxIndex, index] < list[j, index])
                        {
                            maxIndex = j;
                        }
                    }

                    if (maxIndex != i)
                    {
                        for (int k = 0; k < temp.Length; ++k)
                        {
                            temp[k] = list[i, k];
                            list[i, k] = list[maxIndex, k];
                            list[maxIndex, k] = temp[k];
                        }
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                return null;
            }
        }

        private async Task ProcessData()
        {
            try
            {
                isLoading = true;

                loadingIndicatorExplain.SetText(Resource.String.ProductPercentTable_LoadingText_ReceiveServerData);

                string url = $"{topURL}{id}";
                string tempData;

                using (WebClient wc = new WebClient())
                {
                    tempData = await wc.DownloadStringTaskAsync(url);
                }

                loadingIndicatorExplain.SetText(Resource.String.ProductPercentTable_LoadingText_DataProcess);

                StringBuilder sb = new StringBuilder(tempData);

                sb.Replace("\"formula\":", " ");

                List<int> removeList = new List<int>();

                for (int i = 0; i < sb.Length; ++i)
                {
                    if ((sb[i] == '[') || (sb[i] == ']') || (sb[i] == '{') || (sb[i] == '}') || (char.IsWhiteSpace(sb[i])))
                    {
                        removeList.Add(i);
                    }
                }

                removeList.TrimExcess();

                int count = 0;

                foreach (int index in removeList)
                {
                    if ((index - count) >= 0)
                    {
                        sb.Remove((index - count), 1);
                    }
                    else
                    {
                        sb.Remove(0, 1);
                    }

                    count += 1;
                }

                await Task.Delay(100);

                tempData = sb.ToString();

                loadingIndicatorExplain.SetText(Resource.String.ProductPercentTable_LoadingText_DataListing);

                string[] data = tempData.Split(',');

                int normalCount = 0;
                List<int> normalRow = new List<int>();
                int advanceCount = 0;
                List<int> advanceRow = new List<int>();

                for (int i = 0; i < (data.Length / 8); ++i)
                {
                    int type = int.Parse(data[((i * 8) + 5)].Split(':')[1]);

                    switch (type)
                    {
                        case 0:
                        default:
                            normalCount += 1;
                            normalRow.Add(i);
                            break;
                        case 2:
                            advanceCount += 1;
                            advanceRow.Add(i);
                            break;
                    }
                }

                normalRow.TrimExcess();
                advanceRow.TrimExcess();

                normalData = new double[normalCount, 9];
                advanceData = new double[advanceCount, 9];

                for (int i = 0; i < (normalData.Length / topViewIds.Length); ++i)
                {
                    int row = normalRow[i];

                    if (i == 197)
                    {
                        await Task.Delay(1);
                    }

                    for (int j = 0; j < topViewIds.Length; j++)
                    {
                        normalData[i, j] = (j == 8) ? Math.Round(((normalData[i, 6] / normalData[i, 7]) * 100), 4) :
                            double.Parse(data[((row * 8) + j)].Split(':')[1]);

                        /*if (j == 8) normalData[i, j] = Math.Round(((normalData[i, 6] / normalData[i, 7]) * 100), 4);
                        else normalData[i, j] = double.Parse(data[((row * 8) + j)].Split(':')[1]);*/
                    }
                }

                if (advanceCount != 0)
                {
                    for (int i = 0; i < (advanceData.Length / topViewIds.Length); ++i)
                    {
                        int row = advanceRow[i];

                        for (int j = 0; j < topViewIds.Length; j++)
                        {
                            advanceData[i, j] = (j == 8) ? Math.Round(((advanceData[i, 6] / advanceData[i, 7]) * 100), 4) :
                                int.Parse(data[((row * 8) + j)].Split(':')[1]);
                            
                            /*if (j == 8) advanceData[i, j] = Math.Round(((advanceData[i, 6] / advanceData[i, 7]) * 100), 4);
                            else advanceData[i, j] = int.Parse(data[((row * 8) + j)].Split(':')[1]);*/
                        }
                    }
                }

                await Task.Delay(500);

                _ = LoadList();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
            finally
            {
                loadingIndicatorView.SmoothToHide();
                loadingIndicatorExplain.Visibility = ViewStates.Gone;
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }
}