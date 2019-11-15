using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;

namespace GFI_with_GFS_A
{
    [Activity(Label = "@string/Activity_ExternLibraryCopyrightActivity", Theme = "@style/GFS")]
    public class ExternLibraryCopyright : BaseAppCompatActivity
    {
        private RecyclerView mainRecyclerView;
        private RecyclerView.LayoutManager mainLayoutManager;

        string[] name;
        string[] explain;
        string[] license;
        string[] licenseType;

        private ExternLibraryAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ExternLibraryCopyrightLayout);

            mainRecyclerView = FindViewById<RecyclerView>(Resource.Id.ExternLibraryRecyclerView);
            mainLayoutManager = new LinearLayoutManager(this);
            mainRecyclerView.SetLayoutManager(mainLayoutManager);

            InitializeList();

            adapter = new ExternLibraryAdapter(name, explain, license);
            adapter.ItemClick += Adapter_ItemClick;
            mainRecyclerView.SetAdapter(adapter);
        }

        private void Adapter_ItemClick(object sender, int position)
        {
            var intent = new Intent(this, typeof(LicenseViewer));
            intent.PutExtra("Type", licenseType[position]);
            StartActivity(intent);
        }

        private void InitializeList()
        {
            name = Resources.GetStringArray(Resource.Array.ExternLibrary_Name);
            explain = Resources.GetStringArray(Resource.Array.ExternLibrary_Explain);
            license = Resources.GetStringArray(Resource.Array.ExternLibrary_License);
            licenseType = Resources.GetStringArray(Resource.Array.ExternLibrary_LicenseType);
        }
    }

    public class ExternLibraryViewHolder : RecyclerView.ViewHolder
    {
        public TextView name { get; private set; }
        public TextView explain { get; private set; }
        public TextView license { get; private set; }

        public ExternLibraryViewHolder(View view, Action<int> listener) : base(view)
        {
            name = view?.FindViewById<TextView>(Resource.Id.ExternLibraryListViewTitleText);
            explain = view?.FindViewById<TextView>(Resource.Id.ExternLibraryListViewExplainText);
            license = view?.FindViewById<TextView>(Resource.Id.ExternLibraryListViewLicenseText);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    public class ExternLibraryAdapter : RecyclerView.Adapter
    {
        private string[] nameList;
        private string[] explainList;
        private string[] licenseList;

        public event EventHandler<int> ItemClick;

        public ExternLibraryAdapter(string[] name, string[] explain, string[] license)
        {
            nameList = name;
            explainList = explain;
            licenseList = license;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ExternLibraryViewHolder vh = holder as ExternLibraryViewHolder;

            vh.name.Text = nameList[position];
            vh.explain.Text = explainList[position];
            vh.license.Text = licenseList[position];
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.ExternLibraryListViewLayout, parent, false);

            ExternLibraryViewHolder vh = new ExternLibraryViewHolder(view, OnClick);

            return vh;
        }

        public override int ItemCount
        {
            get { return nameList.Length; }
        }

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        public bool HasOnItemClick()
        {
            return ItemClick != null;
        }
    }
}