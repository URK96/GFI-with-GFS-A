using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Name = "com.gfl.dic.StoryActivity", Label = "Story", Theme = "@style/GFS.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public partial class StoryActivity : AppCompatActivity
    {
        enum Category { Main, SubMain, Item }
        enum Top { Main, Sub }

        private Category CategoryType = Category.Main;
        private Top TopType = Top.Main;

        private ArrayAdapter Category_Adapter;

        private RecyclerView MainRecyclerView;
        private Button PreviousButton;
        private RecyclerView.LayoutManager MainRecyclerManager;

        private string[] Main_List;
        private List<string> SubMain_List = new List<string>();
        private List<string> Item_List = new List<string>();

        private List<string> Caption_List = new List<string>();

        private StoryListAdapter adapter;

        private int SubMain_Index = 0;
        private int Item_Index = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.StoryMainLayout);

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.StoryMainToolbar));

            MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.StoryRecyclerView);
            MainRecyclerManager = new LinearLayoutManager(this);
            MainRecyclerView.SetLayoutManager(MainRecyclerManager);

            PreviousButton = FindViewById<Button>(Resource.Id.StoryMainPreviousButton);
            PreviousButton.Click += delegate { ChangeAdapter(true); };

            InitializeList();

            adapter = new StoryListAdapter(Main_List, Caption_List.ToArray());
            adapter.ItemClick += Adapter_ItemClick;
            MainRecyclerView.SetAdapter(adapter);
        }

        private void Adapter_ItemClick(object sender, int position)
        {
            switch (CategoryType)
            {
                case Category.Main:
                    if (position == 0) TopType = Top.Main;
                    else TopType = Top.Sub;
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

            Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Caption));
            Caption_List.TrimExcess();
        }

        private void ChangeAdapter(bool IsPrevious)
        {
            adapter = null;
            SubMain_List.Clear();
            Caption_List.Clear();

            if (IsPrevious == true)
            {
                switch (CategoryType)
                {
                    case Category.SubMain:
                        Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Caption));
                        adapter = new StoryListAdapter(Main_List, Caption_List.ToArray());
                        CategoryType = Category.Main;
                        PreviousButton.Enabled = false;
                        break;
                    case Category.Item:
                        if (TopType == Top.Main)
                        {
                            SubMain_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main));
                            Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Main_Caption));
                            SubMain_List.TrimExcess();
                            Caption_List.TrimExcess();
                            adapter = new StoryListAdapter(SubMain_List.ToArray(), Caption_List.ToArray());
                        }
                        else if (TopType == Top.Sub)
                        {
                            SubMain_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub));
                            Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_Caption));
                            SubMain_List.TrimExcess();
                            Caption_List.TrimExcess();
                            adapter = new StoryListAdapter(SubMain_List.ToArray(), Caption_List.ToArray());
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
                            SubMain_List.TrimExcess();
                            Caption_List.TrimExcess();
                            adapter = new StoryListAdapter(SubMain_List.ToArray(), Caption_List.ToArray());
                        }
                        else if (TopType == Top.Sub)
                        {
                            SubMain_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub));
                            Caption_List.AddRange(Resources.GetStringArray(Resource.Array.Story_Main_Sub_Caption));
                            SubMain_List.TrimExcess();
                            Caption_List.TrimExcess();
                            adapter = new StoryListAdapter(SubMain_List.ToArray(), Caption_List.ToArray());
                        }
                        CategoryType = Category.SubMain;
                        break;
                    case Category.SubMain:
                        if (TopType == Top.Main) ListStoryItem_Main();
                        //else if (TopType == Top.Sub) ListStoryItem_Sub();
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
        public TextView Title { get; private set; }
        public TextView Caption { get; private set; }

        public StoryListViewHolder(View view, Action<int> listener) : base (view)
        {
            Title = view.FindViewById<TextView>(Resource.Id.StoryListViewTitleText);
            Caption = view.FindViewById<TextView>(Resource.Id.StoryListViewCaptionText);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    public class StoryListAdapter : RecyclerView.Adapter
    {
        private string[] TitleList;
        private string[] CaptionList;

        public event EventHandler<int> ItemClick;

        public StoryListAdapter(string[] title, string[] caption)
        {
            TitleList = title;
            CaptionList = caption;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            StoryListViewHolder vh = holder as StoryListViewHolder;

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