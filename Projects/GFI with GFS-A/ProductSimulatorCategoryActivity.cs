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
    [Activity(Label = "", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ProductSimulatorCategoryActivity : BaseAppCompatActivity
    {
        private RecyclerView recyclerView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.ProductSimulatorCategoryLayout);

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.ProductSimulatorCategoryMainToolbar));
            SupportActionBar.SetTitle(Resource.String.ProductSimulatorCategoryActivity_Title);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.ProductSimulatorCategoryRecyclerView);

            recyclerView.SetLayoutManager(new LinearLayoutManager(this));

            var adapter = new ProductSimulatorCategoryAdapter();
            adapter.ItemClick += Adapter_ItemClick;

            recyclerView.SetAdapter(adapter);
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
            var productInfo = new Intent(this, typeof(ProductSimulatorActivity));

            switch (e)
            {
                case 0:
                    productInfo.PutExtra("Info", "Doll/Normal");
                    break;
                case 1:
                    productInfo.PutExtra("Info", "Doll/Advance");
                    break;
                case 2:
                    productInfo.PutExtra("Info", "Equip/Normal");
                    break;
                case 3:
                    productInfo.PutExtra("Info", "Equip/Advance");
                    break;
                default:
                    return;
            }

            StartActivity(productInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }

        private class ProductSimulatorCategoryViewHolder : RecyclerView.ViewHolder
        {
            public TextView Title { get; private set; }
            public TextView Caption { get; private set; }

            public ProductSimulatorCategoryViewHolder(View view, Action<int> listener) : base(view)
            {
                Title = view?.FindViewById<TextView>(Resource.Id.ProductSimulatorCategoryTitleText);
                Caption = view?.FindViewById<TextView>(Resource.Id.ProductSimulatorCategoryCaptionText);

                view.Click += (sender, e) => listener(LayoutPosition);
            }
        }

        private class ProductSimulatorCategoryAdapter : RecyclerView.Adapter
        {
            private string[] titleList;
            private string[] captionList;

            public event EventHandler<int> ItemClick;

            public ProductSimulatorCategoryAdapter()
            {
                titleList = new string[]
                {
                    ETC.Resources.GetString(Resource.String.ProductSimulatorCategoryActivity_DollNormalProduct),
                    ETC.Resources.GetString(Resource.String.ProductSimulatorCategoryActivity_DollAdvancedProduct)
                    //ETC.Resources.GetString(Resource.String.ProductSimulatorCategoryActivity_EquipNormalProduct),
                    //ETC.Resources.GetString(Resource.String.ProductSimulatorCategoryActivity_EquipAdvancedProduct)
                };
                captionList = new string[]
                {
                    ETC.Resources.GetString(Resource.String.ProductSimulatorCategoryActivity_DollNormalProduct_Caption),
                    ETC.Resources.GetString(Resource.String.ProductSimulatorCategoryActivity_DollAdvancedProduct_Caption)
                    //ETC.Resources.GetString(Resource.String.ProductSimulatorCategoryActivity_EquipNormalProduct_Caption),
                    //ETC.Resources.GetString(Resource.String.ProductSimulatorCategoryActivity_EquipAdvancedProduct_Caption)
                };
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var vh = holder as ProductSimulatorCategoryViewHolder;

                vh.Title.Text = titleList[position];
                vh.Caption.Text = captionList[position];
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.ProductSimulatorCategoryListLayout, parent, false);
                var vh = new ProductSimulatorCategoryViewHolder(view, OnClick);

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