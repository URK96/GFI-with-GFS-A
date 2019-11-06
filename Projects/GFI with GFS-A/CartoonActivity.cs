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
    public partial class CartoonActivity : BaseAppCompatActivity
    {
        bool isCategory = true;

        private ArrayAdapter categoryAdapter;
        private Android.Support.V4.App.FragmentTransaction ft;
        private Android.Support.V4.App.Fragment cartoonScreenF;

        internal DrawerLayout mainDrawerLayout;
        private ListView drawerListView;

        private string[] categoryList;
        private List<string> itemList = new List<string>();

        private int categoryIndex = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ETC.SetDialogTheme();

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_NoActionBar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.CartoonMainLayout);

            // Find View & Connect Event

            mainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.CartoonMainDrawerLayout);
            mainDrawerLayout.DrawerOpened += delegate
            {
                if (ETC.useLightTheme)
                {
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen_WhiteTheme);
                }
                else
                {
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen);
                }
            };
            mainDrawerLayout.DrawerClosed += delegate
            {
                if (ETC.useLightTheme)
                {
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
                }
                else
                {
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);
                }
            };
            drawerListView = FindViewById<ListView>(Resource.Id.CartoonMainNavigationListView);
            drawerListView.ItemClick += DrawerListView_ItemClick;

            // Set ActionBar

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.CartoonMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            if (ETC.useLightTheme)
            {
                SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu_WhiteTheme);
            }
            else
            {
                SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);
            }

            // Set Fragment

            cartoonScreenF = new CartoonScreen();

            ft = SupportFragmentManager.BeginTransaction();
            ft.Add(Resource.Id.CartoonContainer, cartoonScreenF, "CartoonScreen");
            ft.Commit();

            LoadCategoryList();
        }

        private void LoadCategoryList()
        {
            try
            {
                categoryList = Resources.GetStringArray(Resource.Array.Cartoon_Category);

                categoryAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, categoryList);
                drawerListView.Adapter = categoryAdapter;

                mainDrawerLayout.OpenDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                Toast.MakeText(this, "Fail to initialize category list", ToastLength.Short).Show();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (!mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
                    {
                        mainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    }
                    else
                    {
                        mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    }

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
                    drawerListView.Adapter = itemAdapter;

                    isCategory = false;
                }
                else
                {
                    switch (e.Position)
                    {
                        case 0:
                            drawerListView.Adapter = categoryAdapter;
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
                                    _ = ((CartoonScreen)cartoonScreenF).LoadProcess(categoryList[categoryIndex], categoryIndex, e.Position - 1, false);
                                    break;
                                case 6:
                                case 7:
                                case 9:
                                    _ = ((CartoonScreen)cartoonScreenF).LoadProcessWeb(categoryList[categoryIndex], categoryIndex, e.Position - 1, false);
                                    break;
                            }
                            mainDrawerLayout.CloseDrawer(GravityCompat.Start);
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

        internal void ListItems(int categoryIndex, ref List<string> list)
        {
            switch (categoryIndex)
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
                    list.AddRange(Resources.GetStringArray(Resource.Array.GF_INGUKOON_Frontline));
                    break;
                case 5:
                    list.AddRange(Resources.GetStringArray(Resource.Array.GF_Guide));
                    break;
                case 6:
                    list.AddRange(Resources.GetStringArray(Resource.Array.GF_DailyComic));
                    break;
                case 7:
                    list.AddRange(Resources.GetStringArray(Resource.Array.mota6nako_GF));
                    break;
                case 8:
                    list.AddRange(Resources.GetStringArray(Resource.Array.ImmortalityFront_GF));
                    break;
                case 9:
                    list.AddRange(Resources.GetStringArray(Resource.Array.MMM_GF));
                    break;
                case 10:
                    list.AddRange(Resources.GetStringArray(Resource.Array.Geo_GF));
                    break;
            }
        }

        public override void OnBackPressed()
        {
            if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                mainDrawerLayout.CloseDrawer(GravityCompat.Start);

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

        private LinearLayout copyrightLayout;
        private FrameLayout webViewLayout;
        private ProgressBar loadProgress;
        private Button previousButton;
        private Button nextButton;
        private ImageButton refreshButton;
        private TextView nowCartoonText;
        private RecyclerView mainRecyclerView;
        private RecyclerView.LayoutManager mainLayoutManager;

        private CartoonType cartoonType = CartoonType.Image;

        private List<string> selectedItemList = new List<string>();
        private List<string> selectedItemURLList = new List<string>();
        private List<Android.Graphics.Bitmap> bitmapList = new List<Android.Graphics.Bitmap>();

        private string nowCategory = "";
        private int nowCategoryIndex = 0;
        private int nowItemIndex = 0;
        private int count = 0;

        private readonly string cartoonTopPath = Path.Combine(ETC.cachePath, "Cartoon");

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater?.Inflate(Resource.Layout.CartoonScreenLayout, container, false);

            // Find View & Connect Event

            copyrightLayout = v.FindViewById<LinearLayout>(Resource.Id.CartoonScreenCopyrightLayout);
            webViewLayout = v.FindViewById<FrameLayout>(Resource.Id.CartoonScreenWebViewLayout);
            loadProgress = v.FindViewById<ProgressBar>(Resource.Id.CartoonScreenLoadProgress);
            previousButton = v.FindViewById<Button>(Resource.Id.CartoonScreenPreviousButton);
            previousButton.Click += delegate
            {
                switch (cartoonType)
                {
                    default:
                    case CartoonType.Image:
                        _ = LoadProcess(nowCategory, nowCategoryIndex, nowItemIndex - 1, false);
                        break;
                    case CartoonType.Web:
                        _ = LoadProcessWeb(nowCategory, nowCategoryIndex, nowItemIndex - 1, false);
                        break;
                }
            };
            nextButton = v.FindViewById<Button>(Resource.Id.CartoonScreenNextButton);
            nextButton.Click += delegate 
            {
                switch (cartoonType)
                {
                    default:
                    case CartoonType.Image:
                        _ = LoadProcess(nowCategory, nowCategoryIndex, nowItemIndex + 1, false);
                        break;
                    case CartoonType.Web:
                        _ = LoadProcessWeb(nowCategory, nowCategoryIndex, nowItemIndex + 1, false);
                        break;
                }
            };
            refreshButton = v.FindViewById<ImageButton>(Resource.Id.CartoonScreenRefreshButton);
            refreshButton.Click += delegate 
            {
                switch (cartoonType)
                {
                    default:
                    case CartoonType.Image:
                        _ = LoadProcess(nowCategory, nowCategoryIndex, nowItemIndex, true);
                        break;
                    case CartoonType.Web:
                        _ = LoadProcessWeb(nowCategory, nowCategoryIndex, nowItemIndex, true);
                        break;
                }
            };
            nowCartoonText = v.FindViewById<TextView>(Resource.Id.CartoonScreenNowCartoonText);
            mainRecyclerView = v.FindViewById<RecyclerView>(Resource.Id.CartoonScreenMainRecyclerView);
            mainLayoutManager = new LinearLayoutManager(Activity);
            mainRecyclerView.SetLayoutManager(mainLayoutManager);

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
                    previousButton.Enabled = false;
                    nextButton.Enabled = true;
                }
                else if (itemIndex == (selectedItemList.Count - 1))
                {
                    previousButton.Enabled = true;
                    nextButton.Enabled = false;
                }
                else
                {
                    previousButton.Enabled = true;
                    nextButton.Enabled = true;
                }

                loadProgress.Visibility = ViewStates.Visible;
                ((CartoonActivity)Activity).mainDrawerLayout.Enabled = false;
                selectedItemList.Clear();

                copyrightLayout.RemoveAllViews();

                await Task.Delay(100);

                ((CartoonActivity)Activity).ListItems(categoryIndex, ref selectedItemList);
                selectedItemList.TrimExcess();

                string categoryPath = Path.Combine(cartoonTopPath, category);
                string itemPath = Path.Combine(categoryPath, itemIndex.ToString());

                if (isRefresh)
                {
                    Directory.Delete(itemPath, true);
                }

                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                }

                if (!Directory.Exists(itemPath))
                {
                    Directory.CreateDirectory(itemPath);
                    await DownloadCartoon(category, itemIndex);
                }
                else
                {
                    if (Directory.GetFiles(itemPath).Length == 0)
                    {
                        await DownloadCartoon(category, itemIndex);
                    }
                }

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
                    case 4:
                        tv1.Text = "Creator : 잉여군";
                        tv2.Text = "https://twitter.com/INGUKOON";
                        break;
                    case 5:
                    case 6:
                        tv1.Text = "Creator : MADCORE";
                        tv2.Text = "https://www.pixiv.net/member.php?id=455690";
                        break;
                    case 9:
                        tv1.Text = "Creator : MMM";
                        tv2.Text = "https://www.pixiv.net/member.php?id=25683341";
                        break;
                }

                layout.Orientation = Orientation.Vertical;
                tv1.Gravity = GravityFlags.Center;
                tv2.Gravity = GravityFlags.Center;

                layout.AddView(tv1);
                layout.AddView(tv2);

                copyrightLayout.AddView(layout);

                List<string> Files = Directory.GetFiles(itemPath).ToList();
                Files.TrimExcess();
                Files.Sort(SortCartoonList);
                bitmapList.Clear();

                const int imageSize = 500;

                foreach (string file in Files)
                {
                    Drawable drawable = Drawable.CreateFromPath(file);
                    Android.Graphics.Bitmap bitmap = ((BitmapDrawable)drawable).Bitmap;

                    int height = 0;

                    while (height < bitmap.Height)
                    {
                        int remainHeight = bitmap.Height - height;

                        Android.Graphics.Bitmap bitmapFix;

                        if (remainHeight >= imageSize)
                        {
                            bitmapFix = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, imageSize);
                            height += imageSize;
                        }
                        else
                        {
                            bitmapFix = Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, remainHeight);
                            height += remainHeight;
                        }

                        bitmapList.Add(bitmapFix);
                    }

                    await Task.Delay(10);
                }

                bitmapList.TrimExcess();

                mainRecyclerView.SetAdapter(new CartoonScreenAdapter(bitmapList.ToArray()));

                GC.Collect();

                loadProgress.Visibility = ViewStates.Invisible;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
            finally
            {
                ((CartoonActivity)Activity).mainDrawerLayout.Enabled = false;
                nowCartoonText.Text = selectedItemList[itemIndex];
                webViewLayout.Visibility = ViewStates.Gone;
                mainRecyclerView.Visibility = ViewStates.Visible;
            }
        }

        internal async Task LoadProcessWeb(string category, int categoryIndex, int itemIndex, bool IsRefresh = false)
        {
            cartoonType = CartoonType.Web;
            nowItemIndex = itemIndex;
            nowCategoryIndex = categoryIndex;
            nowCategory = category;

            try
            {              
                if (itemIndex == 0)
                {
                    previousButton.Enabled = false;
                    nextButton.Enabled = true;
                }
                else if (itemIndex == (selectedItemList.Count - 1))
                {
                    previousButton.Enabled = true;
                    nextButton.Enabled = false;
                }
                else
                {
                    previousButton.Enabled = true;
                    nextButton.Enabled = true;
                }

                loadProgress.Visibility = ViewStates.Visible;
                ((CartoonActivity)Activity).mainDrawerLayout.Enabled = false;
                selectedItemList.Clear();
                selectedItemURLList.Clear();

                webViewLayout.RemoveAllViews();
                copyrightLayout.RemoveAllViews();

                await Task.Delay(100);

                ((CartoonActivity)Activity).ListItems(categoryIndex, ref selectedItemList);
                ListItemURLs(categoryIndex, ref selectedItemURLList);
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

                webViewLayout.AddView(webview);
                webview.LoadUrl(selectedItemURLList[nowItemIndex]);

                loadProgress.Visibility = ViewStates.Invisible;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, Activity);
            }
            finally
            {
                ((CartoonActivity)Activity).mainDrawerLayout.Enabled = false;
                nowCartoonText.Text = selectedItemList[itemIndex];
                mainRecyclerView.Visibility = ViewStates.Gone;
                webViewLayout.Visibility = ViewStates.Visible;
            }
        }

        private int SortCartoonList(string x, string y)
        {
            int a = int.Parse(Path.GetFileNameWithoutExtension(x));
            int b = int.Parse(Path.GetFileNameWithoutExtension(y));

            return a.CompareTo(b);
        }

        private async Task DownloadCartoon(string category, int itemIndex)
        {
            Uri uri;

            Android.Support.V7.App.AlertDialog dialog;

            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(Activity, ETC.dialogBGDownload);
            ad.SetTitle(Resource.String.Cartoon_DownloadCartoonTitle);
            ad.SetMessage(Resource.String.Cartoon_DownloadCartoonMessage);
            ad.SetCancelable(false);
            ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);

            dialog = ad.Show();

            try
            {
                string ServerItemPath = Path.Combine(ETC.server, "Data", "Images", "Cartoon", "ko", category, selectedItemList[itemIndex]);

                count = 1;

                while (true)
                {
                    string contentPath = Path.Combine(ServerItemPath, $"{count}.png");
                    string localContentPath = Path.Combine(cartoonTopPath, category, itemIndex.ToString(), $"{count}.gfdcache");

                    Uri.TryCreate(contentPath, UriKind.RelativeOrAbsolute, out uri);
                    WebRequest request = WebRequest.Create(uri);

                    using (WebResponse response = await request.GetResponseAsync().ConfigureAwait(false))
                    {
                        if (response.ContentType != "image/png")
                        {
                            break;
                        }
                    }

                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += (sender, e) =>
                        {
                            string message = Resources.GetString(Resource.String.Cartoon_DownloadCartoonMessage);
                            Activity.RunOnUiThread(() => { ad.SetMessage($"{message}{count}({e.BytesReceived / 1024}KB)"); });
                        };
                        await wc.DownloadFileTaskAsync(contentPath, localContentPath);
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

        internal void ListItemURLs(int categoryIndex, ref List<string> list)
        {
            switch (categoryIndex)
            {
                case 7:
                    list.AddRange(Resources.GetStringArray(Resource.Array.mota6nako_GF_URL));
                    break;
                case 8:
                    list.AddRange(Resources.GetStringArray(Resource.Array.ImmortalityFront_GF_URL));
                    break;
                case 10:
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
            CartoonImageView = view?.FindViewById<ImageView>(Resource.Id.CartoonScreenImageView);
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

            vh?.CartoonImageView.SetImageBitmap(Image[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.CartoonScreenListLayout, parent, false);

            CartoonScreenViewHolder vh = new CartoonScreenViewHolder(view);

            return vh;
        }

        public override int ItemCount
        {
            get { return Image.Length; }
        }
    }
}