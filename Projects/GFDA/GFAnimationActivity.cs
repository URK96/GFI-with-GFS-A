using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.RecyclerView.Widget;

using Google.Android.Material.BottomSheet;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFDA
{
    [Activity(Label = "GFAnimationActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public partial class GFAnimationActivity : BaseAppCompatActivity
    {
        private RecyclerView recyclerView;
        private BottomSheetDialog sheetDialog;

        private ArrayAdapter<string> dialogAdapter;
        private ListView dialogListView;

        private Dictionary<string, string> linkDic = new Dictionary<string, string>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            int bottomsheetStyle = Resource.Style.GFD_BottomSheetDialog;

            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);

                bottomsheetStyle = Resource.Style.GFD_BottomSheetDialog_Light;
            }

            // Create your application here
            SetContentView(Resource.Layout.GFAnimationLayout);

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.GFAnimationMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.Main_Other_GFAnimation);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.GFAnimationRecyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));

            sheetDialog = new BottomSheetDialog(this, bottomsheetStyle);

            var adapter = new GFAnimationListAdapter();
            adapter.ItemClick += Adapter_ItemClick;

            recyclerView.SetAdapter(adapter);

            CreateSheetDialog();
        }

        private void CreateSheetDialog()
        {
            var view = LayoutInflater.Inflate(Resource.Layout.GFAnimationDialogLayout, null);
            dialogListView = view.FindViewById<ListView>(Resource.Id.GFAnimationDialogListView);

            dialogAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, linkDic.Keys.ToArray());

            dialogListView.Adapter = dialogAdapter;
            dialogListView.ItemClick += async (sender, e) =>
            {
                var key = linkDic.Keys.ToArray()[e.Position];

                await RunLink(linkDic[key]);
            };

            sheetDialog.SetContentView(view);
            sheetDialog.SetCancelable(true);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void Adapter_ItemClick(object sender, int e)
        {
            try
            {
                ListLink(e, linkDic);

                dialogListView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, linkDic.Keys.ToArray());

                sheetDialog.Show();
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private async Task RunLink(string url)
        {
            try
            {
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Long).Show();
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }

        private class GFAnimationListViewHolder : RecyclerView.ViewHolder
        {
            public TextView TopTitle { get; private set; }
            public TextView Title { get; private set; }

            public GFAnimationListViewHolder(View view, Action<int> listener) : base(view)
            {
                TopTitle = view?.FindViewById<TextView>(Resource.Id.GFAnimationListTopTitleText);
                Title = view?.FindViewById<TextView>(Resource.Id.GFAnimationListTitleText);

                view.Click += (sender, e) => listener(LayoutPosition);
            }
        }

        private class GFAnimationListAdapter : RecyclerView.Adapter
        {
            private readonly string[] topTitleList;
            private readonly string[] titleList;
            private readonly int[] exceptionIndex = { 0, 13, 14 };
            private readonly string episodeString;

            public event EventHandler<int> ItemClick;

            public GFAnimationListAdapter()
            {
                topTitleList = ETC.Resources.GetStringArray(Resource.Array.GFAnimation_TopTitle);
                titleList = ETC.Resources.GetStringArray(Resource.Array.GFAnimation_Title);
                episodeString = ETC.Resources.GetString(Resource.String.GFAnimation_Episode);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var vh = holder as GFAnimationListViewHolder;

                vh.TopTitle.Text = topTitleList[position];
                vh.Title.Text = $"{(exceptionIndex.Contains(position) ? "" : episodeString)} {titleList[position]}";
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.GFAnimationListLayout, parent, false);
                var vh = new GFAnimationListViewHolder(view, OnClick);

                return vh;
            }

            public override int ItemCount
            {
                get { return titleList.Length; }
            }

            void OnClick(int position)
            {
                ItemClick?.Invoke(this, position);
            }

            public bool HasOnItemClick()
            {
                return (ItemClick != null);
            }
        }
    }
}