using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.Media;
using Android.Support.V7.Widget;

namespace GFI_with_GFS_A
{
    [Activity(Label = "ProductResultActivity", Theme = "@style/GFS", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ProductResultActivity : FragmentActivity
    {
        private string[] Types;
        private DataRow[] DRs;

        private RecyclerView MainRecyclerView;
        private RecyclerView.LayoutManager MainLayoutManager;
        private ProductResultAdapter adapter;

        private MediaPlayer ResultEffect;

        private CoordinatorLayout SnackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.UseLightTheme == true) SetTheme(Resource.Style.GFS_Light);

            // Create your application here
            SetContentView(Resource.Layout.ProductResultLayout);

            Types = Intent.GetStringArrayExtra("ResultType");
            string[] names = Intent.GetStringArrayExtra("ResultInfo");

            DRs = new DataRow[Types.Length];

            for (int i = 0; i < Types.Length; ++i)
            {
                DataRow ResultDR = null;

                switch (Types[i])
                {
                    case "Doll":
                        ResultDR = ETC.FindDataRow(ETC.DollList, "Name", names[i]);
                        break;
                    case "Equip":
                        ResultDR = ETC.FindDataRow(ETC.EquipmentList, "Name", names[i]);
                        break;
                    case "Fairy":
                        ResultDR = ETC.FindDataRow(ETC.FairyList, "Name", names[i]);
                        break;
                }

                DRs[i] = ResultDR;
            }

            SnackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.PSResultSnackbarLayout);
            MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.PSResultRecyclerView);
            MainLayoutManager = new LinearLayoutManager(this);
            MainRecyclerView.SetLayoutManager(MainLayoutManager);

            adapter = new ProductResultAdapter(Types, DRs, this);
            if (adapter.HasOnItemClick() == false) adapter.ItemClick += Adapter_ItemClick;
            MainRecyclerView.SetAdapter(adapter);
        }

        private void Adapter_ItemClick(object sender, int e)
        {
            try
            {
                Intent ResultInfo = null;

                switch (Types[e])
                {
                    case "Doll":
                        ResultInfo = new Intent(this, typeof(DollDBDetailActivity));
                        break;
                    case "Equip":
                        ResultInfo = new Intent(this, typeof(EquipDBDetailActivity));
                        break;
                    case "Fairy":
                        ResultInfo = new Intent(this, typeof(FairyDBDetailActivity));
                        break;
                }

                ResultInfo.PutExtra("Keyword", (string)DRs[e]["Name"]);
                StartActivity(ResultInfo);
                OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(SnackbarLayout, Resource.String.DBDetail_LoadDetailFail, Snackbar.LengthShort);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            GC.Collect();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }
    }

    public class ProductResultViewHolder : RecyclerView.ViewHolder
    {
        public TextView DicNumber { get; private set; }
        public TextView Type { get; private set; }
        public ImageView Grade { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView Name { get; private set; }
        public TextView ProductTime { get; private set; }

        public ProductResultViewHolder(View view, Action<int> listener) : base(view)
        {
            DicNumber = view.FindViewById<TextView>(Resource.Id.PSResultListNumber);
            Type = view.FindViewById<TextView>(Resource.Id.PSResultListType);
            Grade = view.FindViewById<ImageView>(Resource.Id.PSResultListGrade);
            SmallImage = view.FindViewById<ImageView>(Resource.Id.PSResultListSmallImage);
            Name = view.FindViewById<TextView>(Resource.Id.PSResultListName);
            ProductTime = view.FindViewById<TextView>(Resource.Id.PSResultListProductTime);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    public class ProductResultAdapter : RecyclerView.Adapter
    {
        string[] types;
        DataRow[] items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public ProductResultAdapter(string[] types, DataRow[] items, Activity context)
        {
            this.types = types;
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ProductResultListLayout, parent, false);

            ProductResultViewHolder vh = new ProductResultViewHolder(view, OnClick);
            return vh;
        }

        public override int ItemCount
        {
            get { return items.Length; }
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

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ProductResultViewHolder vh = holder as ProductResultViewHolder;

            string type = types[position];
            DataRow item = items[position];

            try
            {
                int D_Num = (int)item["DicNumber"];

                vh.DicNumber.Text = $"No. {(int)item["DicNumber"]}";


                string URL = "";
                string FilePath = "";

                switch (type)
                {
                    case "Doll":
                        URL = Path.Combine(ETC.Server, "Data", "Images", "Guns", "Normal_Crop", $"{(int)item["DicNumber"]}.png");
                        FilePath = Path.Combine(ETC.CachePath, "Doll", "Normal_Crop", $"{(int)item["DicNumber"]}.gfdcache");
                        break;
                    case "Equip":
                        URL = Path.Combine(ETC.Server, "Data", "Images", "Equipments", $"{(string)item["Icon"]}.png");
                        FilePath = Path.Combine(ETC.CachePath, "Equip", "Normal", $"{(string)item["Icon"]}.gfdcache");
                        break;
                    case "Fairy":
                        URL = Path.Combine(ETC.Server, "Data", "Images", "Fairy", $"{(int)item["Name"]}_1.png");
                        FilePath = Path.Combine(ETC.CachePath, "Fairy", "Normal_Crop", $"{(int)item["DicNumber"]}.gfdcache");
                        break;
                }

                if (File.Exists(FilePath) == false)
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(URL, FilePath);
                    }
                }
                vh.SmallImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(FilePath));

                int GradeIconId = 0;
                if ((type == "Doll") || (type == "Equip"))
                {
                    switch ((int)item["Grade"])
                    {
                        case 2:
                            GradeIconId = Resource.Drawable.Grade_2;
                            break;
                        case 3:
                            GradeIconId = Resource.Drawable.Grade_3;
                            break;
                        case 4:
                            GradeIconId = Resource.Drawable.Grade_4;
                            break;
                        case 5:
                            GradeIconId = Resource.Drawable.Grade_5;
                            break;
                        case 0:
                            GradeIconId = Resource.Drawable.Grade_0;
                            break;
                        default:
                            GradeIconId = Resource.Drawable.Grade_2;
                            break;
                    }

                    vh.Type.Text = (string)item["Type"];
                }
                else if (type == "Fairy")
                {
                    switch ((string)item["Type"])
                    {
                        case "전투":
                            GradeIconId = Resource.Drawable.Fairy_Combat;
                            break;
                        case "책략":
                            GradeIconId = Resource.Drawable.Fairy_Strategy;
                            break;
                    }

                    vh.Type.Visibility = ViewStates.Gone;
                }
                vh.Grade.SetImageResource(GradeIconId);

                string name = "";

                switch (type)
                {
                    case "Doll":
                        if (ETC.Language.Language == "ko") name = (string)item["Name"];
                        else
                        {
                            if (item["Name_EN"] == DBNull.Value) name = (string)item["Name"];
                            else if (string.IsNullOrWhiteSpace((string)item["Name_EN"])) name = (string)item["Name"];
                            else name = (string)item["Name_EN"];
                        }
                        break;
                    case "Equip":
                    case "Fairy":
                        name = (string)item["Name"];
                        break;
                }
                vh.Name.Text = name;

                vh.ProductTime.Text = ETC.CalcTime((int)item["ProductTime"]);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, context);
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}