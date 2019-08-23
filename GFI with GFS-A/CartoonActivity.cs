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
        bool isCategory = true;

        private ArrayAdapter categoryAdapter;
        private Android.Support.V4.App.FragmentTransaction ft;
        private Android.Support.V4.App.Fragment CartoonScreen_F;

        internal DrawerLayout MainDrawerLayout;
        private ListView DrawerListView;

        private string[] categoryList;
        private List<string> itemList = new List<string>();

        private int categoryIndex = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ETC.SetDialogTheme();

            if (ETC.UseLightTheme)
                SetTheme(Resource.Style.GFS_NoActionBar_Light);

            // Create your application here
            SetContentView(Resource.Layout.CartoonMainLayout);

            // Find View & Connect Event

            MainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.CartoonMainDrawerLayout);
            MainDrawerLayout.DrawerOpened += delegate
            {
                if (ETC.UseLightTheme)
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen_WhiteTheme);
                else
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen);
            };
            MainDrawerLayout.DrawerClosed += delegate
            {
                if (ETC.UseLightTheme)
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
                else
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);
            };
            DrawerListView = FindViewById<ListView>(Resource.Id.CartoonMainNavigationListView);
            DrawerListView.ItemClick += DrawerListView_ItemClick;

            // Set ActionBar

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.CartoonMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            if (ETC.UseLightTheme)
                SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
            else
                SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);

            // Set Fragment

            CartoonScreen_F = new CartoonScreen();

            ft = SupportFragmentManager.BeginTransaction();
            ft.Add(Resource.Id.CartoonContainer, CartoonScreen_F, "CartoonScreen");
            ft.Commit();

            LoadCategoryList();
        }

        private void LoadCategoryList()
        {
            try
            {
                categoryList = Resources.GetStringArray(Resource.Array.Cartoon_Category);

                categoryAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, categoryList);
                DrawerListView.Adapter = categoryAdapter;

                MainDrawerLayout.OpenDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail to initialize category list", ToastLength.Short).Show();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (!MainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
                        MainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    else
                        MainDrawerLayout.CloseDrawer(GravityCompat.Start);

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                if (isCategory)
                {
                    categoryIndex = e.Position;

                    itemList.Clear();
                    itemList.Add("...");

                    ListItems(categoryIndex, ref itemList);
                    itemList.TrimExcess();

                    var itemAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, itemList);
                    DrawerListView.Adapter = itemAdapter;

                    isCategory = false;
                }
                else
                {
                    switch (e.Position)
                    {
                        case 0:
                            DrawerListView.Adapter = categoryAdapter;
                            isCategory = true;
                            break;
                        default:
                            switch (categoryIndex)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 8:
                                    _ = ((CartoonScreen)CartoonScreen_F).LoadProcess(categoryList[categoryIndex], categoryIndex, e.Position - 1, false);
                                    break;
                                case 6:
                                case 7:
                                case 9:
                                    _ = ((CartoonScreen)CartoonScreen_F).LoadProcess_Web(categoryList[categoryIndex], categoryIndex, e.Position - 1, false);
                                    break;
                            }
                            MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail to process list item", ToastLength.Short).Show();
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
                    list.AddRange(Resources.GetStringArray(Resource.Array.GF_SF2_After));
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
                case 9:
                    list.AddRange(Resources.GetStringArray(Resource.Array.Geo_GF));
                    break;
            }
        }

        public override void OnBackPressed()
        {
            if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                MainDrawerLayout.CloseDrawer(GravityCompat.Start);

                return;
            }
            else
            {
                GC.Collect();

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

        private CartoonType cartoonType = CartoonType.Image;

        private List<string> selectedItemList = new List<string>();
        private List<string> selectedItemURLList = new List<string>();
        private List<Android.Graphics.Bitmap> bitmapList = new List<Android.Graphics.Bitmap>();

        private string nowCategory = "";
        private int nowCategoryIndex = 0;
        private int nowItemIndex = 0;

        private string cartoonTopPath = Path.Combine(ETC.CachePath, "Cartoon");

        private Android.Support.V7.App.AlertDialog dialog;
        private int count = 0;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.CartoonScreenLayout, container, false);

            // Find View & Connect Event

            CopyrightLayout = v.FindViewById<LinearLayout>(Resource.Id.CartoonScreenCopyrightLayout);
            WebViewLayout = v.FindViewById<FrameLayout>(Resource.Id.CartoonScreenWebViewLayout);
            LoadProgress = v.FindViewById<ProgressBar>(Resource.Id.CartoonScreenLoadProgress);
            PreviousButton = v.FindViewById<Button>(Resource.Id.CartoonScreenPreviousButton);
            PreviousButton.Click += delegate
            {
                switch (cartoonType)
                {
                    default:
                    case CartoonType.Image:
                        _ = LoadProcess(nowCategory, nowCategoryIndex, nowItemIndex - 1, false);
                        break;
                    case CartoonType.Web:
                        _ = LoadProcess_Web(nowCategory, nowCategoryIndex, nowItemIndex - 1, false);
                        break;
                }
            };
            NextButton = v.FindViewById<Button>(Resource.Id.CartoonScreenNextButton);
            NextButton.Click += delegate 
            {
                switch (cartoonType)
                {
                    default:
                    case CartoonType.Image:
                        _ = LoadProcess(nowCategory, nowCategoryIndex, nowItemIndex + 1, false);
                        break;
                    case CartoonType.Web:
                        _ = LoadProcess_Web(nowCategory, nowCategoryIndex, nowItemIndex + 1, false);
                        break;
                }
            };
            RefreshButton = v.FindViewById<ImageButton>(Resource.Id.CartoonScreenRefreshButton);
            RefreshButton.Click += delegate 
            {
                switch (cartoonType)
                {
                    default:
                    case CartoonType.Image:
                        _ = LoadProcess(nowCategory, nowCategoryIndex, nowItemIndex, true);
                        break;
                    case CartoonType.Web:
                        _ = LoadProcess_Web(nowCategory, nowCategoryIndex, nowItemIndex, true);
                        break;
                }
            };
            NowCartoonText = v.FindViewById<TextView>(Resource.Id.CartoonScreenNowCartoonText);
            MainRecyclerView = v.FindViewById<RecyclerView>(Resource.Id.CartoonScreenMainRecyclerView);
            MainLayoutManager = new LinearLayoutManager(Activity);
            MainRecyclerView.SetLayoutManager(MainLayoutManager);

            return v;
        }

        internal async Task LoadProcess(string category, int categoryIndex, int itemIndex, bool isRefresh)
        {
            cartoonType = CartoonType.Image;
            nowItemIndex = itemIndex;
            nowCategoryIndex = categoryIndex;
            nowCategory = category;

            await Task.Delay(100);

            try
            {
                if (itemIndex == 0)
                {
                    PreviousButton.Enabled = false;
                    NextButton.Enabled = true;
                }
                else if (itemIndex == (selectedItemList.Count - 1))
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
                selectedItemList.Clear();

                CopyrightLayout.RemoveAllViews();

                await Task.Delay(100);

                ((CartoonActivity)Activity).ListItems(categoryIndex, ref selectedItemList);
                selectedItemList.TrimExcess();

                string categoryPath = Path.Combine(cartoonTopPath, category);
                string itemPath = Path.Combine(categoryPath, itemIndex.ToString());

                if (isRefresh)
                    Directory.Delete(itemPath, true);

                if (!Directory.Exists(categoryPath))
                    Directory.CreateDirectory(categoryPath);

                if (!Directory.Exists(itemPath))
                {
                    Directory.CreateDirectory(itemPath);
                    await DownloadCartoon(category, itemIndex);
                }
                else
                    if (Directory.GetFiles(itemPath).Length == 0)
                        await DownloadCartoon(category, itemIndex);

                LinearLayout layout = new LinearLayout(Activity);
                TextView tv1 = new TextView(Activity);
                TextView tv2 = new TextView(Activity);
                tv2.AutoLinkMask = Android.Text.Util.MatchOptions.WebUrls;

                switch (categoryIndex)
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

                List<string> Files = Directory.GetFiles(itemPath).ToList();
                Files.TrimExcess();
                Files.Sort(SortCartoonList);
                bitmapList.Clear();

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

                        bitmapList.Add(bitmap_fix);
                    }

                    await Task.Delay(10);
                }

                bitmapList.TrimExcess();

                MainRecyclerView.SetAdapter(new CartoonScreenAdapter(bitmapList.ToArray()));

                GC.Collect();

                LoadProgress.Visibility = ViewStates.Invisible;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
            finally
            {
                ((CartoonActivity)Activity).MainDrawerLayout.Enabled = false;
                NowCartoonText.Text = selectedItemList[itemIndex];
                WebViewLayout.Visibility = ViewStates.Gone;
                MainRecyclerView.Visibility = ViewStates.Visible;
            }
        }

        internal async Task LoadProcess_Web(string Category, int Category_Index, int Item_Index, bool IsRefresh)
        {
            cartoonType = CartoonType.Web;
            nowItemIndex = Item_Index;
            nowCategoryIndex = Category_Index;
            nowCategory = Category;

            try
            {              
                if (Item_Index == 0)
                {
                    PreviousButton.Enabled = false;
                    NextButton.Enabled = true;
                }
                else if (Item_Index == (selectedItemList.Count - 1))
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
                selectedItemList.Clear();
                selectedItemURLList.Clear();

                WebViewLayout.RemoveAllViews();
                CopyrightLayout.RemoveAllViews();

                await Task.Delay(100);

                ((CartoonActivity)Activity).ListItems(Category_Index, ref selectedItemList);
                ListItemURLs(Category_Index, ref selectedItemURLList);
                selectedItemList.TrimExcess();
                selectedItemURLList.TrimExcess();

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
                webview.LoadUrl(selectedItemURLList[nowItemIndex]);

                LoadProgress.Visibility = ViewStates.Invisible;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
            finally
            {
                ((CartoonActivity)Activity).MainDrawerLayout.Enabled = false;
                NowCartoonText.Text = selectedItemList[Item_Index];
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
                string ServerItemPath = Path.Combine(ETC.Server, "Data", "Images", "Cartoon", "ko", Category, selectedItemList[Item_Index]);
                count = 1;

                while (true)
                {
                    string ContentPath = Path.Combine(ServerItemPath, $"{count}.png");
                    string ContentPath_local = Path.Combine(cartoonTopPath, Category, Item_Index.ToString(), $"{count}.gfdcache");
                    WebRequest request = WebRequest.Create(ContentPath);

                    using (WebResponse response = await request.GetResponseAsync())
                        if (response.ContentType != "image/png")
                            break;

                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                        {
                            string message = Resources.GetString(Resource.String.Cartoon_DownloadCartoonMessage);
                            Activity.RunOnUiThread(() => { ad.SetMessage($"{message}{count}({e.BytesReceived / 1024}KB)"); });
                        };
                        await wc.DownloadFileTaskAsync(ContentPath, ContentPath_local);
                    }

                    count += 1;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
            finally
            {
                dialog.Dismiss();
            }
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
                case 9:
                    list.AddRange(Resources.GetStringArray(Resource.Array.Geo_GF_URL));
                    break;
            }
        }

        private class WebBrowserWebClient : WebViewClient
        {
            [Obsolete]
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