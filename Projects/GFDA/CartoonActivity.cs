using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;

using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace GFDA
{
    [Activity(Name = "com.gfl.dic.CartoonActivity", Label = "Cartoon", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public partial class CartoonActivity : BaseAppCompatActivity
    {
        bool isCategory = true;

        private ArrayAdapter categoryAdapter;
        private AndroidX.Fragment.App.FragmentTransaction ft;
        private AndroidX.Fragment.App.Fragment cartoonScreenF;

        internal TextView toolbarCartoonTitle;
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
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.CartoonMainLayout);


            // Find View & Connect Event

            toolbarCartoonTitle = FindViewById<TextView>(Resource.Id.CartoonToolbarCartoonTitle);
            mainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.CartoonMainDrawerLayout);
            mainDrawerLayout.DrawerOpened += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuOpen); };
            mainDrawerLayout.DrawerClosed += delegate { SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu); };
            drawerListView = FindViewById<ListView>(Resource.Id.CartoonMainNavigationListView);
            drawerListView.ItemClick += DrawerListView_ItemClick;


            // Set ActionBar

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.CartoonMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.Menu);


            // Set Fragment

            cartoonScreenF = new CartoonScreen();

            ft = SupportFragmentManager.BeginTransaction();
            ft.Add(Resource.Id.CartoonContainer, cartoonScreenF, "CartoonScreen");
            ft.Commit();

            LoadCategoryList();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.CartoonMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (mainDrawerLayout.IsDrawerOpen(GravityCompat.Start))
                    {
                        mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    }
                    else
                    {
                        mainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    }
                    break;
                case Resource.Id.CartoonSkipPrevious:
                    (cartoonScreenF as CartoonScreen).SkipCartoon(-1);
                    break;
                case Resource.Id.CartoonSkipNext:
                    (cartoonScreenF as CartoonScreen).SkipCartoon(1);
                    break;
                case Resource.Id.RefreshCartoonImageCache:
                    (cartoonScreenF as CartoonScreen).RefreshCartoon();
                    break;
                case Resource.Id.CartoonExit:
                    mainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
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

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                if (isCategory)
                {
                    categoryIndex = e.Position;

                    itemList.Clear();
                    itemList.Add("...");

                    ListItems(categoryIndex, itemList);
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
                                case 6:
                                case 9:
                                case 11:
                                    _ = (cartoonScreenF as CartoonScreen).LoadProcess(categoryList[categoryIndex], categoryIndex, e.Position - 1, false);
                                    break;
                                case 7:
                                case 8:
                                case 10:
                                    _ = (cartoonScreenF as CartoonScreen).LoadProcessWeb(categoryList[categoryIndex], categoryIndex, e.Position - 1, false);
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

        internal void ListItems(int categoryIndex, List<string> list)
        {
            list.AddRange(categoryIndex switch
            {
                0 => Resources.GetStringArray(Resource.Array.kazensky_GF),
                1 => Resources.GetStringArray(Resource.Array.GF_SF),
                2 => Resources.GetStringArray(Resource.Array.GF_SF2),
                3 => Resources.GetStringArray(Resource.Array.GF_SF2_After),
                4 => Resources.GetStringArray(Resource.Array.GF_INGUKOON_Frontline),
                5 => Resources.GetStringArray(Resource.Array.GF_Guide),
                6 => Resources.GetStringArray(Resource.Array.GF_DailyComic),
                7 => Resources.GetStringArray(Resource.Array.mota6nako_GF),
                8 => Resources.GetStringArray(Resource.Array.ImmortalityFront_GF),
                9 => Resources.GetStringArray(Resource.Array.MMM_GF),
                10 => Resources.GetStringArray(Resource.Array.Geo_GF),
                11 => Resources.GetStringArray(Resource.Array.senlong_GF),
                _ => null
            });
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

    public class CartoonScreen : AndroidX.Fragment.App.Fragment
    {
        enum CartoonType { Image, Web }

        private View v;

        private TextView toolbarTitle;
        private LinearLayout copyrightLayout;
        private FrameLayout webViewLayout;
        private ProgressBar loadProgress;
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

            toolbarTitle = (Activity as CartoonActivity).toolbarCartoonTitle;
            copyrightLayout = v.FindViewById<LinearLayout>(Resource.Id.CartoonScreenCopyrightLayout);
            webViewLayout = v.FindViewById<FrameLayout>(Resource.Id.CartoonScreenWebViewLayout);
            loadProgress = v.FindViewById<ProgressBar>(Resource.Id.CartoonScreenLoadProgress);
            mainRecyclerView = v.FindViewById<RecyclerView>(Resource.Id.CartoonScreenMainRecyclerView);
            mainLayoutManager = new LinearLayoutManager(Activity);
            mainRecyclerView.SetLayoutManager(mainLayoutManager);

            return v;
        }

        internal void SkipCartoon(int mag)
        {
            // mag = 1 : Skip Next
            // mag = -1 : Skip Previous

            if ((nowItemIndex == 0) && (mag == -1))
            {
                Toast.MakeText(Activity, " ", ToastLength.Short);
            }
            else if ((nowItemIndex == (selectedItemList.Count - 1)) && (mag == 1))
            {
                Toast.MakeText(Activity, " ", ToastLength.Short);
            }
            else
            {
                _ = cartoonType switch
                {
                    CartoonType.Web => LoadProcessWeb(nowCategory, nowCategoryIndex, nowItemIndex + mag, false),
                    _ => LoadProcess(nowCategory, nowCategoryIndex, nowItemIndex + mag, false),
                };
            } 
        }

        internal void RefreshCartoon()
        {
            _ = cartoonType switch
            {
                CartoonType.Web => LoadProcessWeb(nowCategory, nowCategoryIndex, nowItemIndex, true),
                _ => LoadProcess(nowCategory, nowCategoryIndex, nowItemIndex, true),
            };
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
                loadProgress.Visibility = ViewStates.Visible;
                (Activity as CartoonActivity).mainDrawerLayout.Enabled = false;
                selectedItemList.Clear();

                copyrightLayout.RemoveAllViews();

                await Task.Delay(100);

                (Activity as CartoonActivity).ListItems(categoryIndex, selectedItemList);
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

                var layout = new LinearLayout(Activity);
                var tv1 = new TextView(Activity);
                var tv2 = new TextView(Activity)
                {
                    AutoLinkMask = Android.Text.Util.MatchOptions.WebUrls
                };

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
                    case 11:
                        tv1.Text = "Creator : 센롱";
                        tv2.Text = "https://www.pixiv.net/users/21946119";
                        break;
                }

                layout.Orientation = Orientation.Vertical;
                tv1.Gravity = GravityFlags.Center;
                tv2.Gravity = GravityFlags.Center;

                layout.AddView(tv1);
                layout.AddView(tv2);

                copyrightLayout.AddView(layout);

                var Files = Directory.GetFiles(itemPath).ToList();
                Files.TrimExcess();
                Files.Sort(SortCartoonList);
                bitmapList.Clear();

                const int imageSize = 500;

                foreach (string file in Files)
                {
                    var drawable = Drawable.CreateFromPath(file);
                    var bitmap = (drawable as BitmapDrawable).Bitmap;

                    int height = 0;

                    while (height < bitmap.Height)
                    {
                        int remainHeight = bitmap.Height - height;
                        var bitmapFix = (remainHeight >= imageSize) ? Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, imageSize) :
                            Android.Graphics.Bitmap.CreateBitmap(bitmap, 0, height, bitmap.Width, remainHeight);

                        height += (remainHeight >= imageSize) ? imageSize : remainHeight;

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
                (Activity as CartoonActivity).mainDrawerLayout.Enabled = false;
                toolbarTitle.Text = selectedItemList[itemIndex];
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
                loadProgress.Visibility = ViewStates.Visible;
                (Activity as CartoonActivity).mainDrawerLayout.Enabled = false;
                selectedItemList.Clear();
                selectedItemURLList.Clear();

                webViewLayout.RemoveAllViews();
                copyrightLayout.RemoveAllViews();

                await Task.Delay(100);

                (Activity as CartoonActivity).ListItems(categoryIndex, selectedItemList);
                ListItemURLs(categoryIndex, ref selectedItemURLList);
                selectedItemList.TrimExcess();
                selectedItemURLList.TrimExcess();

                var webview = new WebView(Activity);
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
                (Activity as CartoonActivity).mainDrawerLayout.Enabled = false;
                toolbarTitle.Text = selectedItemList[itemIndex];
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
            var ad = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity, ETC.dialogBGDownload);
            ad.SetTitle(Resource.String.Cartoon_DownloadCartoonTitle);
            ad.SetMessage(Resource.String.Cartoon_DownloadCartoonMessage);
            ad.SetCancelable(false);
            ad.SetView(Resource.Layout.SpinnerProgressDialogLayout);

            var dialog = ad.Show();

            try
            {
                string serverItemPath = Path.Combine(ETC.server, "Data", "Images", "Cartoon", "ko", category, selectedItemList[itemIndex]);

                count = 1;

                while (true)
                {
                    string contentPath = Path.Combine(serverItemPath, $"{count}.png");
                    string localContentPath = Path.Combine(cartoonTopPath, category, itemIndex.ToString(), $"{count}.gfdcache");

                    Uri.TryCreate(contentPath, UriKind.RelativeOrAbsolute, out Uri uri);
                    var request = WebRequest.Create(uri);

                    using (var response = await request.GetResponseAsync().ConfigureAwait(false))
                    {
                        if (response.ContentType != "image/png")
                        {
                            break;
                        }
                    }

                    using (var wc = new WebClient())
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
        private Android.Graphics.Bitmap[] image;

        public CartoonScreenAdapter(Android.Graphics.Bitmap[] bitmaps)
        {
            image = bitmaps;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as CartoonScreenViewHolder;

            vh?.CartoonImageView.SetImageBitmap(image[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.CartoonScreenListLayout, parent, false);
            var vh = new CartoonScreenViewHolder(view);

            return vh;
        }

        public override int ItemCount
        {
            get { return image.Length; }
        }
    }
}