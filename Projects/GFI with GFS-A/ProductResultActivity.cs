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
    [Activity(Label = "ProductResultActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ProductResultActivity : BaseAppCompatActivity
    {
        private string[] types;
        private DataRow[] drs;

        private RecyclerView mainRecyclerView;
        private ProductResultAdapter adapter;

        private CoordinatorLayout snackbarLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.ProductResultLayout);

            types = Intent.GetStringArrayExtra("ResultType");
            string[] names = Intent.GetStringArrayExtra("ResultInfo");

            drs = new DataRow[types.Length];

            for (int i = 0; i < types.Length; ++i)
            {
                DataRow ResultDR = null;

                switch (types[i])
                {
                    case "Doll":
                        ResultDR = ETC.FindDataRow(ETC.dollList, "Name", names[i]);
                        break;
                    case "Equip":
                        ResultDR = ETC.FindDataRow(ETC.equipmentList, "Name", names[i]);
                        break;
                    case "Fairy":
                        ResultDR = ETC.FindDataRow(ETC.fairyList, "Name", names[i]);
                        break;
                }

                drs[i] = ResultDR;
            }

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.PSResultMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.ProductSimulatorResultActivity_Title);

            snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.PSResultSnackbarLayout);
            mainRecyclerView = FindViewById<RecyclerView>(Resource.Id.PSResultRecyclerView);
            mainRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            adapter = new ProductResultAdapter(types, drs, this);

            if (!adapter.HasOnItemClick())
            {
                adapter.ItemClick += Adapter_ItemClick;
            }

            mainRecyclerView.SetAdapter(adapter);
        }

        private void Adapter_ItemClick(object sender, int e)
        {
            try
            {
                Intent ResultInfo = null;

                switch (types[e])
                {
                    case "Doll":
                        ResultInfo = new Intent(this, typeof(DollDBDetailActivity));
                        ResultInfo.PutExtra("DicNum", (int)drs[e]["DicNumber"]);
                        break;
                    case "Equip":
                        ResultInfo = new Intent(this, typeof(EquipDBDetailActivity));
                        ResultInfo.PutExtra("Keyword", (string)drs[e]["Name"]);
                        break;
                    case "Fairy":
                        ResultInfo = new Intent(this, typeof(FairyDBDetailActivity));
                        ResultInfo.PutExtra("Keyword", (string)drs[e]["Name"]);
                        break;
                }

                StartActivity(ResultInfo);
                OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.DBDetail_LoadDetailFail, Snackbar.LengthShort);
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
        public CardView CardView { get; private set; }
        public TextView DicNumber { get; private set; }
        public TextView Type { get; private set; }
        public ImageView Grade { get; private set; }
        public ImageView SmallImage { get; private set; }
        public TextView Name { get; private set; }
        public TextView ProductTime { get; private set; }

        public ProductResultViewHolder(View view, Action<int> listener) : base(view)
        {
            CardView = view.FindViewById<CardView>(Resource.Id.PSResultListMainCardView);
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
        readonly string[] types;
        readonly DataRow[] items;
        readonly Activity context;

        public event EventHandler<int> ItemClick;

        public ProductResultAdapter(string[] types, DataRow[] items, Activity context)
        {
            this.types = types;
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.ProductResultListLayout, parent, false);
            var vh = new ProductResultViewHolder(view, OnClick);

            return vh;
        }

        public override int ItemCount
        {
            get { return items.Length; }
        }

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        public bool HasOnItemClick()
        {
            return (ItemClick != null);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as ProductResultViewHolder;

            string type = types[position];
            var item = items[position];

            try
            {
                int dNum = (int)item["DicNumber"];

                vh.DicNumber.Text = $"No. {(int)item["DicNumber"]}";

                string URL = "";
                string FilePath = "";

                switch (type)
                {
                    case "Doll":
                        URL = Path.Combine(ETC.server, "Data", "Images", "Guns", "Normal_Crop", $"{(int)item["DicNumber"]}.png");
                        FilePath = Path.Combine(ETC.cachePath, "Doll", "Normal_Crop", $"{(int)item["DicNumber"]}.gfdcache");
                        break;
                    case "Equip":
                        URL = Path.Combine(ETC.server, "Data", "Images", "Equipments", $"{(string)item["Icon"]}.png");
                        FilePath = Path.Combine(ETC.cachePath, "Equip", "Normal", $"{(string)item["Icon"]}.gfdcache");
                        break;
                    case "Fairy":
                        URL = Path.Combine(ETC.server, "Data", "Images", "Fairy", $"{(int)item["Name"]}_1.png");
                        FilePath = Path.Combine(ETC.cachePath, "Fairy", "Normal_Crop", $"{(int)item["DicNumber"]}.gfdcache");
                        break;
                }

                if (!File.Exists(FilePath))
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
                        default:
                        case 2:
                            GradeIconId = Resource.Drawable.Grade_2;
                            break;
                        case 3:
                            GradeIconId = Resource.Drawable.Grade_3;
                            vh.CardView.SetCardBackgroundColor(Android.Graphics.Color.ParseColor("#8055CCEE"));
                            break;
                        case 4:
                            GradeIconId = Resource.Drawable.Grade_4;
                            vh.CardView.SetCardBackgroundColor(Android.Graphics.Color.ParseColor("#80AACC22"));
                            break;
                        case 5:
                            GradeIconId = Resource.Drawable.Grade_5;
                            vh.CardView.SetCardBackgroundColor(Android.Graphics.Color.ParseColor("#80FFBB22"));
                            break;
                        case 0:
                            GradeIconId = Resource.Drawable.Grade_0;
                            vh.CardView.SetCardBackgroundColor(Android.Graphics.Color.ParseColor("#80C040B0"));
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
                        if (ETC.locale.Language == "ko") name = (string)item["Name"];
                        else
                        {
                            if (item["Name_EN"] == DBNull.Value)
                            {
                                name = (string)item["Name"];
                            }
                            else if (string.IsNullOrWhiteSpace((string)item["Name_EN"]))
                            {
                                name = (string)item["Name"];
                            }
                            else
                            {
                                name = (string)item["Name_EN"];
                            }
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