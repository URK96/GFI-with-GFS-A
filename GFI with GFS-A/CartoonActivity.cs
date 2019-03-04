using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.CartoonActivity", Label = "Cartoon", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public partial class CartoonActivity : AppCompatActivity
    {
        bool IsCategory = true;

        private ArrayAdapter Category_Adapter;
        private Android.Support.V4.App.FragmentTransaction ft;
        private Android.Support.V4.App.Fragment CartoonScreen_F;

        internal DrawerLayout MainDrawerLayout;
        private ListView DrawerListView;

        private string[] Category_List;
        private List<string> Item_List = new List<string>();

        private int Category_Index = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ETC.SetDialogTheme();

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
            ft.Add(Resource.Id.CartoonContainer, CartoonScreen_F, "CartoonScreen");
            ft.Commit();

            LoadCategoryList();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == false)
                        MainDrawerLayout.OpenDrawer(GravityCompat.Start);
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

                    ListItems(Category_Index, ref Item_List);
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
                            switch (Category_Index)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 8:
                                    ((CartoonScreen)CartoonScreen_F).LoadProcess(Category_List[Category_Index], Category_Index, (e.Position - 1), false);
                                    break;
                                case 6:
                                case 7:
                                    ((CartoonScreen)CartoonScreen_F).LoadProcess_Web(Category_List[Category_Index], Category_Index, (e.Position - 1), false);
                                    break;
                            }
                            MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        internal void ListItems(int Category_Index, ref List<string> list)
        {
            switch (Category_Index)
            {
                case 0:
                    list.AddRange(Resources.GetStringArray(Resource.Array.kazensky_GF));
                    break;
                case 1:
                    list.AddRange(Resources.GetStringArray(Resource.Array.GF_SF));
                    break;
                case 2:
                    list.AddRange(Resources.GetStringArray(Resource.Array.GF_SF2));
                    break;
                case 3:
                    list.AddRange(Resources.GetStringArray(Resource.Array.SOPMOD_Cartoon));
                    break;
                case 4:
                    list.AddRange(Resources.GetStringArray(Resource.Array.GF_Guide));
                    break;
                case 5:
                    list.AddRange(Resources.GetStringArray(Resource.Array.GF_DailyComic));
                    break;
                case 6:
                    list.AddRange(Resources.GetStringArray(Resource.Array.mota6nako_GF));
                    break;
                case 7:
                    list.AddRange(Resources.GetStringArray(Resource.Array.ImmortalityFront_GF));
                    break;
                case 8:
                    list.AddRange(Resources.GetStringArray(Resource.Array.MMM_GF));
                    break;
            }
        }

        public override void OnBackPressed()
        {
            if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == true)
            {
                MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                return;
            }
            else
            {
                base.OnBackPressed();
                OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            }
        }
    }

    public class CartoonScreen : Android.Support.V4.App.Fragment
    {
        enum CartoonType { Image, Web }

        private View v;

        private LinearLayout CopyrightLayout;
        private FrameLayout WebViewLayout;
        private ProgressBar LoadProgress;
        private Button PreviousButton;
        private Button NextButton;
        private ImageButton RefreshButton;
        private TextView NowCartoonText;
        private RecyclerView MainRecyclerView;
        private RecyclerView.LayoutManager MainLayoutManager;

        private CartoonType C_Type = CartoonType.Image;

        private List<string> Selected_Item_List = new List<string>();
        private List<string> Selected_Item_URL_List = new List<string>();
        private List<Android.Graphics.Bitmap> Bitmap_List = new List<Android.Graphics.Bitmap>();

        private string Now_Category = "";
        private int Now_Category_Index = 0;
        private int Now_Item_Index = 0;

        private string CartoonTopPath = Path.Combine(ETC.CachePath, "Cartoon");

        private Android.Support.V7.App.AlertDialog dialog;
        private int count = 0;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.CartoonScreenLayout, container, false);

            //MainLayout = v.FindViewById<LinearLayout>(Resource.Id.CartoonScreenMainLayout);
            CopyrightLayout = v.FindViewById<LinearLayout>(Resource.Id.CartoonScreenCopyrightLayout);
            WebViewLayout = v.FindViewById<FrameLayout>(Resource.Id.CartoonScreenWebViewLayout);
            LoadProgress = v.FindViewById<ProgressBar>(Resource.Id.CartoonScreenLoadProgress);
            PreviousButton = v.FindViewById<Button>(Resource.Id.CartoonScreenPreviousButton);
            PreviousButton.Click += delegate
            {
                switch (C_Type)
                {
                    case CartoonType.Image:
                        LoadProcess(Now_Category, Now_Category_Index, Now_Item_Index - 1, false);
                        break;
                    case CartoonType.Web:
                        LoadProcess_Web(Now_Category, Now_Category_Index, Now_Item_Index - 1, false);
                        break;
                }
            };
            NextButton = v.FindViewById<Button>(Resource.Id.CartoonScreenNextButton);
            NextButton.Click += delegate 
            {
                switch (C_Type)
                {
                    case CartoonType.Image:
                        LoadProcess(Now_Category, Now_Category_Index, Now_Item_Index + 1, false);
                        break;
                    case CartoonType.Web:
                        LoadProcess_Web(Now_Category, Now_Category_Index, Now_Item_Index + 1, false);
                        break;
                }
            };
            RefreshButton = v.FindViewById<ImageButton>(Resource.Id.CartoonScreenRefreshButton);
            RefreshButton.Click += delegate 
            {
                switch (C_Type)
                {
                    case CartoonType.Image:
                        LoadProcess(Now_Category, Now_Category_Index, Now_Item_Index, true);
                        break;
                    case CartoonType.Web:
                        LoadProcess_Web(Now_Category, Now_Category_Index, Now_Item_Index, true);
                        break;
                }
            };
            NowCartoonText = v.FindViewById<TextView>(Resource.Id.CartoonScreenNowCartoonText);
            MainRecyclerView = v.FindViewById<RecyclerView>(Resource.Id.CartoonScreenMainRecyclerView);
            MainLayoutManager = new LinearLayoutManager(Activity);
            MainRecyclerView.SetLayoutManager(MainLayoutManager);

            return v;
        }

        internal async Task LoadProcess(string Category, int Category_Index, int Item_Index, bool IsRefresh)
        {
            C_Type = CartoonType.Image;
            Now_Item_Index = Item_Index;
            Now_Category_Index = Category_Index;
            Now_Category = Category;

            try
            {
                if (Item_Index == 0)
                {
                    PreviousButton.Enabled = false;
                    NextButton.Enabled = true;
                }
                else if (Item_Index == (Selected_Item_List.Count - 1))
                {
                    PreviousButton.Enabled = true;
                    NextButton.Enabled = false;
                }
                else
                {
                    PreviousButton.Enabled = true;
                    NextButton.Enabled = true;
                }

                LoadProgress.Visibility = ViewStates.Visible;
                ((CartoonActivity)Activity).MainDrawerLayout.Enabled = false;
                Selected_Item_List.Clear();

                CopyrightLayout.RemoveAllViews();

                await Task.Delay(100);

                ((CartoonActivity)Activity).ListItems(Category_Index, ref Selected_Item_List);
                Selected_Item_List.TrimExcess();

                string Category_Path = Path.Combine(CartoonTopPath, Category);
                string Item_Path = Path.Combine(Category_Path, Item_Index.ToString());

                if (IsRefresh == true) Directory.Delete(Item_Path, true);

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
                        tv1.Text = "Creator : 츠보우";
                        tv2.Text = "http://kazensky.blog.me/221115059226";
                        break;
                    case 1:
                    case 2:
                    case 3:
                        tv1.Text = "Creator : 잉여군";
                        tv2.Text = "https://twitter.com/INGUKOON";
                        break;
                    case 4:
                    case 5:
                        tv1.Text = "Creator : MADCORE";
                        tv2.Text = "https://www.pixiv.net/member.php?id=455690";
                        break;
                    case 8:
                        tv1.Text = "Creator : MMM";
                        tv2.Text = "https://www.pixiv.net/member.php?id=25683341";
                        break;
                }

                layout.Orientation = Orientation.Vertical;
                tv1.Gravity = GravityFlags.Center;
                tv2.Gravity = GravityFlags.Center;

                layout.AddView(tv1);
                layout.AddView(tv2);

                CopyrightLayout.AddView(layout);

                List<string> Files = Directory.GetFiles(Item_Path).ToList();
                Files.TrimExcess();
                Files.Sort(SortCartoonList);
                Bitmap_List.Clear();

                const int Image_Size = 500;

                foreach (string file in Files)
                {
                    Drawable drawable = Drawable.CreateFromPath(file);
                    Android.Graphics.Bitmap bitmap = ((BitmapDrawable)drawable).Bitmap;

                    int height = 0;

                    while (height < bitmap.Height)
                    {
                        int remain_height = bitmap.Height - height;

                        Android.Graphics.Bitmap bitmap_fix;

                        if (remain_height >= Image_Size)
                        {
                            bitmap_fix = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, Image_Size);
                            height += Image_Size;
                        }
                        else
                        {
                            bitmap_fix = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, remain_height);
                            height += remain_height;
                        }

                        Bitmap_List.Add(bitmap_fix);
                    }

                    await Task.Delay(10);
                }

                Bitmap_List.TrimExcess();

                MainRecyclerView.SetAdapter(new CartoonScreenAdapter(Bitmap_List.ToArray()));

                GC.Collect();

                LoadProgress.Visibility = ViewStates.Invisible;
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
            finally
            {
                ((CartoonActivity)Activity).MainDrawerLayout.Enabled = false;
                NowCartoonText.Text = Selected_Item_List[Item_Index];
                WebViewLayout.Visibility = ViewStates.Gone;
                MainRecyclerView.Visibility = ViewStates.Visible;
            }
        }

        internal async Task LoadProcess_Web(string Category, int Category_Index, int Item_Index, bool IsRefresh)
        {
            C_Type = CartoonType.Web;
            Now_Item_Index = Item_Index;
            Now_Category_Index = Category_Index;
            Now_Category = Category;

            try
            {              
                if (Item_Index == 0)
                {
                    PreviousButton.Enabled = false;
                    NextButton.Enabled = true;
                }
                else if (Item_Index == (Selected_Item_List.Count - 1))
                {
                    PreviousButton.Enabled = true;
                    NextButton.Enabled = false;
                }
                else
                {
                    PreviousButton.Enabled = true;
                    NextButton.Enabled = true;
                }

                LoadProgress.Visibility = ViewStates.Visible;
                ((CartoonActivity)Activity).MainDrawerLayout.Enabled = false;
                Selected_Item_List.Clear();
                Selected_Item_URL_List.Clear();

                WebViewLayout.RemoveAllViews();

                await Task.Delay(100);

                ((CartoonActivity)Activity).ListItems(Category_Index, ref Selected_Item_List);
                ListItemURLs(Category_Index, ref Selected_Item_URL_List);
                Selected_Item_List.TrimExcess();
                Selected_Item_URL_List.TrimExcess();

                WebView webview = new WebView(Activity);
                webview.SetWebViewClient(new WebBrowserWebClient());
                webview.Settings.BuiltInZoomControls = true;
                webview.Settings.AllowContentAccess = true;
                webview.Settings.BlockNetworkImage = false;
                webview.Settings.BlockNetworkLoads = false;
                webview.Settings.LoadsImagesAutomatically = true;
                webview.Settings.DomStorageEnabled = true;
                webview.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
                webview.Settings.SetAppCacheEnabled(true);
                webview.Settings.JavaScriptEnabled = true;

                WebViewLayout.AddView(webview);
                webview.LoadUrl(Selected_Item_URL_List[Now_Item_Index]);

                LoadProgress.Visibility = ViewStates.Invisible;
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
            finally
            {
                ((CartoonActivity)Activity).MainDrawerLayout.Enabled = false;
                NowCartoonText.Text = Selected_Item_List[Item_Index];
                MainRecyclerView.Visibility = ViewStates.Gone;
                WebViewLayout.Visibility = ViewStates.Visible;
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
            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.DialogBG_Download);
            ad.SetTitle(Resource.String.Cartoon_DownloadCartoonTitle);
            ad.SetMessage(Resource.String.Cartoon_DownloadCartoonMessage);
            ad.SetCancelable(false);
            ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);

            dialog = ad.Show();

            try
            {
                string message = Resources.GetString(Resource.String.Cartoon_DownloadCartoonMessage);
                string ServerItemPath = Path.Combine(ETC.Server, "Data", "Images", "Cartoon", "ko", Category, Selected_Item_List[Item_Index]);
                count = 1;

                while (true)
                {
                    string ContentPath = Path.Combine(ServerItemPath, $"{count}.png");
                    string ContentPath_local = Path.Combine(CartoonTopPath, Category, Item_Index.ToString(), $"{count}.gfdcache");
                    WebRequest request = WebRequest.Create(ContentPath);

                    using (WebResponse response = await request.GetResponseAsync())
                    {
                        if (response.ContentType != "image/png") break;
                    }

                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
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

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string message = Resources.GetString(Resource.String.Cartoon_DownloadCartoonMessage);
            Activity.RunOnUiThread(() => { dialog.SetMessage($"{message}{count}({e.ProgressPercentage}%)"); });
        }

        internal void ListItemURLs(int Category_Index, ref List<string> list)
        {
            switch (Category_Index)
            {
                case 6:
                    list.AddRange(Resources.GetStringArray(Resource.Array.mota6nako_GF_URL));
                    break;
                case 7:
                    list.AddRange(Resources.GetStringArray(Resource.Array.ImmortalityFront_GF_URL));
                    break;
            }
        }

        private class WebBrowserWebClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                view.LoadUrl(url);
                return false;
            }

            public override void OnLoadResource(WebView view, string url)
            {
                base.OnLoadResource(view, url);
            }

            public override void OnPageStarted(WebView view, string url, Android.Graphics.Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
            }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
            }
        }
    }

    public class CartoonScreenViewHolder : RecyclerView.ViewHolder
    {
        public ImageView CartoonImageView { get; private set; }

        public CartoonScreenViewHolder(View view) : base(view)
        {
            CartoonImageView = view.FindViewById<ImageView>(Resource.Id.CartoonScreenImageView);
        }
    }

    public class CartoonScreenAdapter : RecyclerView.Adapter
    {
        private Android.Graphics.Bitmap[] Image;

        public CartoonScreenAdapter(Android.Graphics.Bitmap[] bitmaps)
        {
            Image = bitmaps;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CartoonScreenViewHolder vh = holder as CartoonScreenViewHolder;

            vh.CartoonImageView.SetImageBitmap(Image[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.CartoonScreenListLayout, parent, false);

            CartoonScreenViewHolder vh = new CartoonScreenViewHolder(view);
            return vh;
        }

        public override int ItemCount
        {
            get { return Image.Length; }
        }
    }
}