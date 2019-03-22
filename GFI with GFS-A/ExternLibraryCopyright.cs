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
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Xamarin.Essentials;

namespace GFI_with_GFS_A
{
    [Activity(Label = "Extern Library Copyright", Theme = "@style/GFS")]
    public class ExternLibraryCopyright : AppCompatActivity
    {
        private RecyclerView MainRecyclerView;
        private RecyclerView.LayoutManager MainLayoutManager;

        string[] Names;
        string[] Explains;
        string[] Licenses;
        string[] URLs;

        private ExternLibraryAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ExternLibraryCopyrightLayout);

            MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.ExternLibraryRecyclerView);
            MainLayoutManager = new LinearLayoutManager(this);
            MainRecyclerView.SetLayoutManager(MainLayoutManager);

            InitializeList();

            adapter = new ExternLibraryAdapter(Names, Explains, Licenses);
            adapter.ItemClick += Adapter_ItemClick;
            MainRecyclerView.SetAdapter(adapter);
        }

        private void Adapter_ItemClick(object sender, int e)
        {
            Browser.OpenAsync(URLs[e], BrowserLaunchMode.SystemPreferred);
        }

        private void InitializeList()
        {
            Names = Resources.GetStringArray(Resource.Array.ExternLibrary_Name);
            Explains = Resources.GetStringArray(Resource.Array.ExternLibrary_Explain);
            Licenses = Resources.GetStringArray(Resource.Array.ExternLibrary_License);
            URLs = Resources.GetStringArray(Resource.Array.ExternLibrary_URL);
        }
    }

    public class ExternLibraryViewHolder : RecyclerView.ViewHolder
    {
        public TextView Name { get; private set; }
        public TextView Explain { get; private set; }
        public TextView License { get; private set; }

        public ExternLibraryViewHolder(View view, Action<int> listener) : base(view)
        {
            Name = view.FindViewById<TextView>(Resource.Id.ExternLibraryListViewTitleText);
            Explain = view.FindViewById<TextView>(Resource.Id.ExternLibraryListViewExplainText);
            License = view.FindViewById<TextView>(Resource.Id.ExternLibraryListViewLicenseText);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    public class ExternLibraryAdapter : RecyclerView.Adapter
    {
        private string[] NameList;
        private string[] ExplainList;
        private string[] LicenseList;

        public event EventHandler<int> ItemClick;

        public ExternLibraryAdapter(string[] name, string[] explain, string[] license)
        {
            NameList = name;
            ExplainList = explain;
            LicenseList = license;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ExternLibraryViewHolder vh = holder as ExternLibraryViewHolder;

            vh.Name.Text = NameList[position];
            vh.Explain.Text = ExplainList[position];
            vh.License.Text = LicenseList[position];
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ExternLibraryListViewLayout, parent, false);

            ExternLibraryViewHolder vh = new ExternLibraryViewHolder(view, OnClick);
            return vh;
        }

        public override int ItemCount
        {
            get { return NameList.Length; }
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