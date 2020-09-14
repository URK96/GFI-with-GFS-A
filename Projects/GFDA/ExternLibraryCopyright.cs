using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.RecyclerView.Widget;

using System;

namespace GFDA
{
    [Activity(Label = "@string/Activity_ExternLibraryCopyrightActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ExternLibraryCopyright : BaseAppCompatActivity
    {
        private RecyclerView mainRecyclerView;

        string[] name;
        string[] explain;
        string[] license;
        string[] licenseType;

        private ExternLibraryAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.ExternLibraryCopyrightLayout);

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.ExternLibraryMainToolbar));
            SupportActionBar.SetTitle(Resource.String.Activity_ExternLibraryCopyrightActivity);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            mainRecyclerView = FindViewById<RecyclerView>(Resource.Id.ExternLibraryRecyclerView);
            mainRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            InitializeList();

            adapter = new ExternLibraryAdapter(name, explain, license);
            adapter.ItemClick += Adapter_ItemClick;
            mainRecyclerView.SetAdapter(adapter);
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
        public TextView Name { get; private set; }
        public TextView Explain { get; private set; }
        public TextView License { get; private set; }

        public ExternLibraryViewHolder(View view, Action<int> listener) : base(view)
        {
            Name = view?.FindViewById<TextView>(Resource.Id.ExternLibraryListViewTitleText);
            Explain = view?.FindViewById<TextView>(Resource.Id.ExternLibraryListViewExplainText);
            License = view?.FindViewById<TextView>(Resource.Id.ExternLibraryListViewLicenseText);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    public class ExternLibraryAdapter : RecyclerView.Adapter
    {
        private readonly string[] nameList;
        private readonly string[] explainList;
        private readonly string[] licenseList;

        public event EventHandler<int> ItemClick;

        public ExternLibraryAdapter(string[] name, string[] explain, string[] license)
        {
            nameList = name;
            explainList = explain;
            licenseList = license;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as ExternLibraryViewHolder;

            vh.Name.Text = nameList[position];
            vh.Explain.Text = explainList[position];
            vh.License.Text = licenseList[position];
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