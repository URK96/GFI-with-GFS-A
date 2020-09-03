using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using AndroidX.Transitions;

using System;
using System.Collections.Generic;

using static GFI_with_GFS_A.Resource.Array;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.StoryActivity", Label = "@string/Activity_StoryActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public partial class StoryActivity : BaseAppCompatActivity
    {
        enum Category { Main, SubMain, Item }
        enum Top { Main, Sub, Memory }

        private Category categoryType = Category.Main;
        private Top topType = Top.Main;

        private RecyclerView mainRecyclerView;

        private List<string> titleList = new List<string>();
        private List<string> itemList = new List<string>();
        private List<string> topTitleList = new List<string>();
        private List<string> captionList = new List<string>();

        private StoryListAdapter adapter;

        private int subMainIndex;
        private int itemIndex;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.StoryMainLayout);

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.StoryMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.Activity_StoryActivity);

            mainRecyclerView = FindViewById<RecyclerView>(Resource.Id.StoryRecyclerView);
            mainRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            InitializeList();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.StoryToolbarMenu, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item?.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
                case Resource.Id.StoryExit:
                    base.OnBackPressed();
                    OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void Adapter_ItemClick(object sender, int position)
        {
            switch (categoryType)
            {
                case Category.Main when position is 0:
                    topType = Top.Main;
                    break;
                case Category.Main when position is 1:
                    topType = Top.Main;
                    break;
                case Category.Main when position is 2:
                    topType = Top.Memory;
                    break;
                case Category.SubMain:
                    subMainIndex = position;
                    break;
                case Category.Item:
                    itemIndex = position;
                    break;
            }

            ChangeAdapter(false);
        }

        private void InitializeList()
        {
            topTitleList.AddRange(Resources.GetStringArray(Story_Main_TopTitle));
            titleList.AddRange(Resources.GetStringArray(Story_Main));
            captionList.AddRange(Resources.GetStringArray(Story_Main_Caption));

            adapter = new StoryListAdapter(titleList, topTitleList, captionList);
            adapter.itemClick += Adapter_ItemClick;

            mainRecyclerView.SetAdapter(adapter);
        }

        private void ChangeAdapter(bool isPrevious)
        {
            (int topTitleId, int titleId, int captionId) menuStringIds = (0, 0, 0);

            TransitionManager.BeginDelayedTransition(mainRecyclerView);

            if (!(!isPrevious && (categoryType == Category.Item)))
            {
                titleList.Clear();
                captionList.Clear();
                topTitleList.Clear();
            }

            if (isPrevious)
            {
                switch (categoryType)
                {
                    // SubMain -> Main
                    case Category.SubMain:
                        menuStringIds = (Story_Main_TopTitle, Story_Main, Story_Main_Caption);
                        categoryType = Category.Main;
                        break;
                    // Item -> SubMain
                    case Category.Item when topType is Top.Main:
                        menuStringIds = (Story_Main_Main_TopTitle, Story_Main_Main, Story_Main_Main_Caption);
                        categoryType = Category.SubMain;
                        break;
                    case Category.Item when topType is Top.Sub:
                        menuStringIds = (Story_Main_Sub_TopTitle, Story_Main_Sub, Story_Main_Sub_Caption);
                        categoryType = Category.SubMain;
                        break;
                    case Category.Item when topType is Top.Memory:
                        menuStringIds = (Story_Main_Memory_TopTitle, Story_Main_Memory, Story_Main_Memory_Caption);
                        categoryType = Category.SubMain;
                        break;
                }
            }
            else
            {
                switch (categoryType)
                {
                    // Main -> SubMain
                    case Category.Main when topType is Top.Main:
                        menuStringIds = (Story_Main_Main_TopTitle, Story_Main_Main, Story_Main_Main_Caption);
                        categoryType = Category.SubMain;
                        break;
                    case Category.Main when topType is Top.Sub:
                        menuStringIds = (Story_Main_Sub_TopTitle, Story_Main_Sub, Story_Main_Sub_Caption);
                        categoryType = Category.SubMain;
                        break;
                    case Category.Main when topType is Top.Memory:
                        menuStringIds = (Story_Main_Memory_TopTitle, Story_Main_Memory, Story_Main_Memory_Caption);
                        categoryType = Category.SubMain;
                        break;
                    // SubMain -> Item
                    case Category.SubMain when topType is Top.Main:
                        ListMainStoryItem(out menuStringIds);
                        categoryType = Category.Item;
                        break;
                    case Category.SubMain when topType is Top.Sub:
                        ListSubStoryItem(out menuStringIds);
                        categoryType = Category.Item;
                        break;
                    case Category.SubMain when topType is Top.Memory:
                        ListMemoryStoryItem(out menuStringIds);
                        categoryType = Category.Item;
                        break;
                    // Item -> Resder
                    case Category.Item:
                        RunReader();
                        return;
                }
            }

            topTitleList.AddRange(Resources.GetStringArray(menuStringIds.topTitleId));
            titleList.AddRange(Resources.GetStringArray(menuStringIds.titleId));
            captionList.AddRange(Resources.GetStringArray(menuStringIds.captionId));

            adapter.NotifyDataSetChanged();
        }

        public override void OnBackPressed()
        {
            switch (categoryType)
            {
                case Category.Item:
                case Category.SubMain:
                    ChangeAdapter(true);
                    break;
                case Category.Main:
                    base.OnBackPressed();
                    OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                    break;
            }
        }
    }

    public class StoryListViewHolder : RecyclerView.ViewHolder
    {
        public TextView TopTitle { get; private set; }
        public TextView Title { get; private set; }
        public TextView Caption { get; private set; }

        public StoryListViewHolder(View view, Action<int> listener) : base (view)
        {
            TopTitle = view?.FindViewById<TextView>(Resource.Id.StoryListViewTopTitleText);
            Title = view?.FindViewById<TextView>(Resource.Id.StoryListViewTitleText);
            Caption = view?.FindViewById<TextView>(Resource.Id.StoryListViewCaptionText);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    public class StoryListAdapter : RecyclerView.Adapter
    {
        private List<string> topTitleList;
        private List<string> titleList;
        private List<string> captionList;

        public event EventHandler<int> itemClick;

        public StoryListAdapter(List<string> title, List<string> toptitle, List<string> caption)
        {
            titleList = title;
            topTitleList = toptitle;
            captionList = caption;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as StoryListViewHolder;

            vh.TopTitle.Text = topTitleList[position];
            vh.Title.Text = titleList[position];
            vh.Caption.Text = captionList[position];
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.StoryListViewLayout, parent, false);
            var vh = new StoryListViewHolder(view, OnClick);

            return vh;
        }

        public override int ItemCount
        {
            get { return titleList.Count; }
        }

        void OnClick(int position)
        {
            itemClick?.Invoke(this, position);
        }

        public bool HasOnItemClick()
        {
            return (itemClick != null);
        }
    }
}