using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Support.V4.View;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Android.Graphics.Drawables;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.CartoonActivity", Label = "Cartoon", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public partial class CartoonActivity : AppCompatActivity
    {
        bool IsCategory = true;

        private ArrayAdapter Category_Adapter;
        private Android.Support.V4.App.FragmentTransaction ft;
        private Android.Support.V4.App.Fragment CartoonScreen_F;

        private DrawerLayout MainDrawerLayout;
        private ListView DrawerListView;

        private string[] Category_List;
        private List<string> Item_List = new List<string>();

        private int Category_Index = 0;
        private int Item_Index = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

            // Create your application here
            SetContentView(Resource.Layout.CartoonMainLayout);

            MainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.CartoonMainDrawerLayout);
            MainDrawerLayout.DrawerOpened += delegate
            {
                if (ETC.UseLightTheme == true) SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen_WhiteTheme);
                else SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen);
            };
            MainDrawerLayout.DrawerClosed += delegate
            {
                if (ETC.UseLightTheme == true) SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
                else SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);
            };

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.CartoonMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            if (ETC.UseLightTheme == true) SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
            else SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);

            DrawerListView = FindViewById<ListView>(Resource.Id.CartoonMainNavigationListView);
            DrawerListView.ItemClick += DrawerListView_ItemClick;

            CartoonScreen_F = new CartoonScreen();

            ft = SupportFragmentManager.BeginTransaction();
            ft.Add(Resource.Id.CartoonMainLayout, CartoonScreen_F, "CartoonScreen");

            ft.Commit();

            LoadCategoryList();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == false) MainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    else MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void LoadCategoryList()
        {
            try
            {
                Category_List = Resources.GetStringArray(Resource.Array.Cartoon_Category);

                Category_Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, Category_List);
                DrawerListView.Adapter = Category_Adapter;

                MainDrawerLayout.OpenDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                if (IsCategory == true)
                {
                    Category_Index = e.Position;

                    Item_List.Clear();
                    Item_List.Add("...");

                    switch (Category_Index)
                    {
                        case 0:
                            Item_List.AddRange(Resources.GetStringArray(Resource.Array.kazensky_GF));
                            break;
                    }
                    Item_List.TrimExcess();

                    var Item_Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, Item_List);

                    DrawerListView.Adapter = null;
                    DrawerListView.Adapter = Item_Adapter;

                    IsCategory = false;
                }
                else
                {
                    switch (e.Position)
                    {
                        case 0:
                            DrawerListView.Adapter = Category_Adapter;
                            IsCategory = true;
                            break;
                        default:
                            ((CartoonScreen)CartoonScreen_F).LoadProcess(Category_List[Category_Index], Category_Index, (e.Position - 1));
                            break;
                    }

                    MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }
    }

    public class CartoonScreen : Android.Support.V4.App.Fragment
    {
        private View v;

        private LinearLayout MainLayout;
        private List<ImageView> ImageViewList = new List<ImageView>();
        private ProgressBar LoadProgress;

        private List<string> Selected_Item_List = new List<string>();

        private string CartoonTopPath = Path.Combine(ETC.CachePath, "Cartoon");

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.CartoonScreenLayout, container, false);

            MainLayout = v.FindViewById<LinearLayout>(Resource.Id.CartoonScreenMainLayout);
            LoadProgress = v.FindViewById<ProgressBar>(Resource.Id.CartoonScreenLoadProgress);

            return v;
        }

        internal async Task LoadProcess(string Category, int Category_Index, int Item_Index)
        {
            try
            {
                LoadProgress.Visibility = ViewStates.Visible;

                ImageViewList.Clear();
                MainLayout.RemoveAllViews();

                await Task.Delay(100);

                Selected_Item_List.AddRange(Resources.GetStringArray(Resource.Array.kazensky_GF));

                string Category_Path = Path.Combine(CartoonTopPath, Category);
                string Item_Path = Path.Combine(Category_Path, Item_Index.ToString());

                if (Directory.Exists(Category_Path) == false) Directory.CreateDirectory(Category_Path);
                if (Directory.Exists(Item_Path) == false)
                {
                    Directory.CreateDirectory(Item_Path);
                    await DownloadCartoon(Category, Item_Index);
                }
                else
                {
                    if (Directory.GetFiles(Item_Path).Length == 0) await DownloadCartoon(Category, Item_Index);
                }

                LinearLayout layout = new LinearLayout(Activity);
                TextView tv1 = new TextView(Activity);
                TextView tv2 = new TextView(Activity);
                tv2.AutoLinkMask = Android.Text.Util.MatchOptions.WebUrls;

                switch (Category_Index)
                {
                    case 0:
                        tv1.Text = "Creator : 츠보우 (kazensky)";
                        tv2.Text = "http://kazensky.blog.me/221115059226";
                        break;
                }

                /*LinearLayout.LayoutParams p1 = (LinearLayout.LayoutParams)tv1.LayoutParameters;
                p1.SetMargins(0, 10, 0, 10);
                tv1.LayoutParameters = p1;

                LinearLayout.LayoutParams p2 = (LinearLayout.LayoutParams)tv2.LayoutParameters;
                p2.SetMargins(0, 10, 0, 10);
                tv2.LayoutParameters = p2;

                LinearLayout.LayoutParams p3 = (LinearLayout.LayoutParams)layout.LayoutParameters;
                p3.SetMargins(0, 10, 0, 10);
                layout.LayoutParameters = p3;*/

                layout.Orientation = Orientation.Vertical;
                tv1.Gravity = GravityFlags.Center;
                tv2.Gravity = GravityFlags.Center;

                layout.AddView(tv1);
                layout.AddView(tv2);

                MainLayout.AddView(layout);


                List<string> Files = Directory.GetFiles(Item_Path).ToList();
                Files.TrimExcess();
                Files.Sort(SortCartoonList);

                foreach (string file in Files)
                {
                    ImageView iv = new ImageView(Activity);
                    iv.SetImageDrawable(await Drawable.CreateFromPathAsync(file));
                    iv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ((BitmapDrawable)iv.Drawable).Bitmap.Height);
                    iv.SetScaleType(ImageView.ScaleType.FitXy);

                    ImageViewList.Add(iv);
                    MainLayout.AddView(iv);

                    await Task.Delay(500);
                }

                LoadProgress.Visibility = ViewStates.Invisible;
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }

        private int SortCartoonList(string x, string y)
        {
            int a = int.Parse(Path.GetFileNameWithoutExtension(x));
            int b = int.Parse(Path.GetFileNameWithoutExtension(y));

            return a.CompareTo(b);
        }

        private async Task DownloadCartoon(string Category, int Item_Index)
        {
            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG);
            ad.SetTitle(Resource.String.Cartoon_DownloadCartoonTitle);
            ad.SetMessage(Resource.String.Cartoon_DownloadCartoonMessage);
            ad.SetCancelable(false);
            ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);

            var dialog = ad.Show();

            try
            {
                string ServerItemPath = Path.Combine(ETC.Server, "Data", "Images", "Cartoon", "ko", Category, Selected_Item_List[Item_Index]);
                int count = 1;

                while (true)
                {
                    string ContentPath = Path.Combine(ServerItemPath, $"{count}.png");
                    string ContentPath_local = Path.Combine(CartoonTopPath, Category, Item_Index.ToString(), $"{count}.png");
                    WebRequest request = WebRequest.Create(ContentPath);

                    using (WebResponse response = await request.GetResponseAsync())
                    {
                        if (response.ContentType != "image/png") break;
                    }

                    using (WebClient wc = new WebClient())
                    {
                        await wc.DownloadFileTaskAsync(ContentPath, ContentPath_local);
                    }

                    count += 1;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
            finally
            {
                dialog.Dismiss();
            }
        }
    }
}