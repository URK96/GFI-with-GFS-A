using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.StoryActivity", Label = "@string/Activity_StoryActivity", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public partial class StoryActivity : AppCompatActivity
    {
        enum Category { Main, SubMain, Item }
        enum Top { Main, Sub }

        private Category CategoryType = Category.Main;
        private Top TopType = Top.Main;

        private RecyclerView MainRecyclerView;
        private Button PreviousButton;
        private RecyclerView.LayoutManager MainRecyclerManager;

        private string[] Main_List;
        private List<string> SubMain_List = new List<string>();
        private List<string> Item_List = new List<string>();
        private List<string> TopTitle_List = new List<string>();
        private List<string> Caption_List = new List<string>();

        private StoryListAdapter adapter;

        private int SubMain_Index = 0;
        private int Item_Index = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_NoActionBar_Light);

            // Create your application here
            SetContentView(Resource.Layout.StoryMainLayout);

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.StoryMainToolbar));

            MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.StoryRecyclerView);
            MainRecyclerManager = new LinearLayoutManager(this);
            MainRecyclerView.SetLayoutManager(MainRecyclerManager);

            PreviousButton = FindViewById<Button>(Resource.Id.StoryMainPreviousButton);
            PreviousButton.Click += delegate { ChangeAdapter(true); };

            InitializeList();

            adapter = new StoryListAdapter(Main_List, TopTitle_List.ToArray(), Caption_List.ToArray());
            adapter.ItemClick += Adapter_ItemClick;
            MainRecyclerView.SetAdapter(adapter);
        }

        private void Adapter_ItemClick(object sender, int position)
        {
            switch (CategoryType)
            {
                case Category.Main:
                    if (position == 0)
                        TopType = Top.Main;
                    else return;
                    break;
                case Category.SubMain:
                    SubMain_Index = position;
                    break;
                case Category.Item:
                    Item_Index = position;
                    break;
            }

            ChangeAdapter(false);
        }

        private void InitializeList()
        {
            Main_List = Resources.GetStringArray(Resource.Array.Story_Main);

            TopTitle_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_TopTitle));
            TopTitle_List.TrimExcess();
            Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Caption));
            Caption_List.TrimExcess();
        }

        private void ChangeAdapter(bool IsPrevious)
        {
            adapter = null;
            SubMain_List.Clear();
            Caption_List.Clear();
            TopTitle_List.Clear();

            if (IsPrevious)
            {
                switch (CategoryType)
                {
                    case Category.SubMain:
                        Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Caption));
                        TopTitle_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_TopTitle));
                        Caption_List.TrimExcess();
                        TopTitle_List.TrimExcess();
                        CategoryType = Category.Main;
                        PreviousButton.Enabled = false;
                        adapter = new StoryListAdapter(Main_List, TopTitle_List.ToArray(), Caption_List.ToArray());
                        break;
                    case Category.Item:
                        if (TopType == Top.Main)
                        {
                            SubMain_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main));
                            Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main_Caption));
                            TopTitle_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main_TopTitle));
                            SubMain_List.TrimExcess();
                            Caption_List.TrimExcess();
                            TopTitle_List.TrimExcess();
                            adapter = new StoryListAdapter(SubMain_List.ToArray(), TopTitle_List.ToArray(), Caption_List.ToArray());
                        }
                        else if (TopType == Top.Sub)
                        {
                            SubMain_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub));
                            Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_Caption));
                            TopTitle_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_TopTitle));
                            SubMain_List.TrimExcess();
                            Caption_List.TrimExcess();
                            TopTitle_List.TrimExcess();
                            adapter = new StoryListAdapter(SubMain_List.ToArray(), TopTitle_List.ToArray(), Caption_List.ToArray());
                        }
                        CategoryType = Category.SubMain;
                        break;
                }
            }
            else
            {
                PreviousButton.Enabled = true;

                switch (CategoryType)
                {
                    case Category.Main:
                        if (TopType == Top.Main)
                        {
                            SubMain_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main));
                            Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main_Caption));
                            TopTitle_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main_TopTitle));
                            SubMain_List.TrimExcess();
                            Caption_List.TrimExcess();
                            TopTitle_List.TrimExcess();
                            adapter = new StoryListAdapter(SubMain_List.ToArray(), TopTitle_List.ToArray(), Caption_List.ToArray());
                        }
                        else if (TopType == Top.Sub)
                        {
                            SubMain_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub));
                            Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_Caption));
                            TopTitle_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_TopTitle));
                            SubMain_List.TrimExcess();
                            Caption_List.TrimExcess();
                            TopTitle_List.TrimExcess();
                            adapter = new StoryListAdapter(SubMain_List.ToArray(), TopTitle_List.ToArray(), Caption_List.ToArray());
                        }
                        CategoryType = Category.SubMain;
                        break;
                    case Category.SubMain:
                        if (TopType == Top.Main)
                            ListStoryItem_Main();
                        else if (TopType == Top.Sub)
                            ListStoryItem_Sub();

                        CategoryType = Category.Item;
                        break;
                    case Category.Item:
                        RunReader();
                        return;
                }
            }

            if (adapter.HasOnItemClick() == false) adapter.ItemClick += Adapter_ItemClick;
            MainRecyclerView.SetAdapter(adapter);
        }

        public override void OnBackPressed()
        {
            switch (CategoryType)
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
            TopTitle = view.FindViewById<TextView>(Resource.Id.StoryListViewTopTitleText);
            Title = view.FindViewById<TextView>(Resource.Id.StoryListViewTitleText);
            Caption = view.FindViewById<TextView>(Resource.Id.StoryListViewCaptionText);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    public class StoryListAdapter : RecyclerView.Adapter
    {
        private string[] TitleList;
        private string[] TopTitleList;
        private string[] CaptionList;

        public event EventHandler<int> ItemClick;

        public StoryListAdapter(string[] title, string[] toptitle, string[] caption)
        {
            TitleList = title;
            TopTitleList = toptitle;
            CaptionList = caption;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            StoryListViewHolder vh = holder as StoryListViewHolder;

            vh.TopTitle.Text = TopTitleList[position];
            vh.Title.Text = TitleList[position];
            vh.Caption.Text = CaptionList[position];
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.StoryListViewLayout, parent, false);

            StoryListViewHolder vh = new StoryListViewHolder(view, OnClick);
            return vh;
        }

        public override int ItemCount
        {
            get { return TitleList.Length; }
        }

        void OnClick(int position)
        {
            if (ItemClick != null)
            {
                ItemClick(this, position);
            }
        }

        public bool HasOnItemClick()
        {
            if (ItemClick == null) return false;
            else return true;
        }
    }
}