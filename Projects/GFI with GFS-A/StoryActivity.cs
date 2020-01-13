using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.StoryActivity", Label = "@string/Activity_StoryActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public partial class StoryActivity : BaseAppCompatActivity
    {
        enum Category { Main, SubMain, Item }
        enum Top { Main, Sub }

        private Category categoryType = Category.Main;
        private Top topType = Top.Main;

        private RecyclerView mainRecyclerView;
        private RecyclerView.LayoutManager mainRecyclerManager;

        private string[] mainList;
        private List<string> subMainList = new List<string>();
        private List<string> itemList = new List<string>();
        private List<string> topTitleList = new List<string>();
        private List<string> captionList = new List<string>();

        private StoryListAdapter adapter;

        private int subMainIndex = 0;
        private int itemIndex = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.StoryMainLayout);

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.StoryMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.Activity_StoryActivity);

            mainRecyclerView = FindViewById<RecyclerView>(Resource.Id.StoryRecyclerView);
            mainRecyclerManager = new LinearLayoutManager(this);
            mainRecyclerView.SetLayoutManager(mainRecyclerManager);

            InitializeList();

            adapter = new StoryListAdapter(mainList, topTitleList.ToArray(), captionList.ToArray());
            adapter.itemClick += Adapter_ItemClick;
            mainRecyclerView.SetAdapter(adapter);
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
                case Category.Main:
                    if (position == 0)
                    {
                        topType = Top.Main;
                    }
                    else if (position == 1)
                    {
                        topType = Top.Sub;
                    }
                    else
                    {
                        return;
                    }
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
            mainList = Resources.GetStringArray(Resource.Array.Story_Main);

            topTitleList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_TopTitle));
            topTitleList.TrimExcess();
            captionList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Caption));
            captionList.TrimExcess();
        }

        private void ChangeAdapter(bool IsPrevious)
        {
            adapter = null;
            subMainList.Clear();
            captionList.Clear();
            topTitleList.Clear();

            if (IsPrevious)
            {
                switch (categoryType)
                {
                    case Category.SubMain:
                        captionList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Caption));
                        topTitleList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_TopTitle));
                        captionList.TrimExcess();
                        topTitleList.TrimExcess();
                        categoryType = Category.Main;
                        adapter = new StoryListAdapter(mainList, topTitleList.ToArray(), captionList.ToArray());
                        break;
                    case Category.Item:
                        if (topType == Top.Main)
                        {
                            subMainList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main));
                            captionList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main_Caption));
                            topTitleList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main_TopTitle));
                            subMainList.TrimExcess();
                            captionList.TrimExcess();
                            topTitleList.TrimExcess();
                            adapter = new StoryListAdapter(subMainList.ToArray(), topTitleList.ToArray(), captionList.ToArray());
                        }
                        else if (topType == Top.Sub)
                        {
                            subMainList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub));
                            captionList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_Caption));
                            topTitleList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_TopTitle));
                            subMainList.TrimExcess();
                            captionList.TrimExcess();
                            topTitleList.TrimExcess();
                            adapter = new StoryListAdapter(subMainList.ToArray(), topTitleList.ToArray(), captionList.ToArray());
                        }
                        categoryType = Category.SubMain;
                        break;
                }
            }
            else
            {
                switch (categoryType)
                {
                    case Category.Main:
                        if (topType == Top.Main)
                        {
                            subMainList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main));
                            captionList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main_Caption));
                            topTitleList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main_TopTitle));
                            subMainList.TrimExcess();
                            captionList.TrimExcess();
                            topTitleList.TrimExcess();
                            adapter = new StoryListAdapter(subMainList.ToArray(), topTitleList.ToArray(), captionList.ToArray());
                        }
                        else if (topType == Top.Sub)
                        {
                            subMainList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub));
                            captionList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_Caption));
                            topTitleList.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_TopTitle));
                            subMainList.TrimExcess();
                            captionList.TrimExcess();
                            topTitleList.TrimExcess();
                            adapter = new StoryListAdapter(subMainList.ToArray(), topTitleList.ToArray(), captionList.ToArray());
                        }
                        categoryType = Category.SubMain;
                        break;
                    case Category.SubMain:
                        if (topType == Top.Main)
                        {
                            ListStoryItem_Main();
                        }
                        else if (topType == Top.Sub)
                        {
                            ListStoryItem_Sub();
                        }
                        categoryType = Category.Item;
                        break;
                    case Category.Item:
                        RunReader();
                        return;
                }
            }

            if (!adapter.HasOnItemClick())
            {
                adapter.itemClick += Adapter_ItemClick;
            }

            mainRecyclerView.SetAdapter(adapter);
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
        private string[] titleList;
        private string[] topTitleList;
        private string[] captionList;

        public event EventHandler<int> itemClick;

        public StoryListAdapter(string[] title, string[] toptitle, string[] caption)
        {
            titleList = title;
            topTitleList = toptitle;
            captionList = caption;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            StoryListViewHolder vh = holder as StoryListViewHolder;

            vh.TopTitle.Text = topTitleList[position];
            vh.Title.Text = titleList[position];
            vh.Caption.Text = captionList[position];
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.StoryListViewLayout, parent, false);
            StoryListViewHolder vh = new StoryListViewHolder(view, OnClick);

            return vh;
        }

        public override int ItemCount
        {
            get { return titleList.Length; }
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