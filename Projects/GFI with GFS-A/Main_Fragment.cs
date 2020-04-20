using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GFI_with_GFS_A
{
    public partial class Main : BaseAppCompatActivity
    {
        internal class HomeFragment : AndroidX.Fragment.App.Fragment
        {
            private TextView notificationView;

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                var view = inflater.Inflate(Resource.Layout.Main_HomeLayout, container, false);

                notificationView = view.FindViewById<TextView>(Resource.Id.Main_Home_NotificationText);

                return view;
            }

            public override async void OnResume()
            {
                base.OnResume();

                await LoadNotification();
            }

            private async Task LoadNotification()
            {
                await Task.Delay(100);

                string url = "";

                try
                {

                    if (ETC.locale.Language == "ko")
                    {
                        url = Path.Combine(ETC.server, "Android_Notification.txt");
                    }
                    else
                    {
                        url = Path.Combine(ETC.server, "Android_Notification_en.txt");
                    }

                    if (ETC.isServerDown)
                    {
                        notificationView.Text = "& Server is Maintenance &";
                    }
                    else
                    {
                        using var wc = new WebClient();
                        notificationView.Text = await wc.DownloadStringTaskAsync(url);
                    }
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, Activity);

                    notificationView.Text = "& Notification Load Error &";
                }
            }
        }

        internal class DBFragment : AndroidX.Fragment.App.Fragment
        {
            private RecyclerView recyclerView;

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                var view = inflater.Inflate(Resource.Layout.Main_RecyclerLayout, container, false);

                recyclerView = view.FindViewById<RecyclerView>(Resource.Id.MainRecyclerView);

                recyclerView.SetLayoutManager(new GridLayoutManager(Activity, 2));

                var adapter = new MainRecyclerListAdapter(Activity, 0);
                adapter.ItemClick += Adapter_ItemClick;

                recyclerView.SetAdapter(adapter);

                return view;
            }

            private async void Adapter_ItemClick(object sender, int e)
            {
                try
                {
                    switch (e)
                    {
                        case 0:
                            await Task.Run(() =>
                            {
                                if (string.IsNullOrEmpty(ETC.dollList.TableName))
                                {
                                    ETC.LoadDBSync(ETC.dollList, "Doll.gfs", false);
                                }
                            });

                            Activity.StartActivity(typeof(DollDBMainActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 1:
                            await Task.Run(() =>
                            {
                                if (string.IsNullOrEmpty(ETC.equipmentList.TableName))
                                {
                                    ETC.LoadDBSync(ETC.equipmentList, "Equipment.gfs", false);
                                }
                            });

                            Activity.StartActivity(typeof(EquipDBMainActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 2:
                            await Task.Run(() =>
                            {
                                if (string.IsNullOrEmpty(ETC.fairyList.TableName))
                                {
                                    ETC.LoadDBSync(ETC.fairyList, "Fairy.gfs", false);
                                }
                            });

                            Activity.StartActivity(typeof(FairyDBMainActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 3:
                            await Task.Run(() =>
                            {
                                if (string.IsNullOrEmpty(ETC.enemyList.TableName))
                                {
                                    ETC.LoadDBSync(ETC.enemyList, "Enemy.gfs", false);
                                }
                            });

                            Activity.StartActivity(typeof(EnemyDBMainActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 4:
                            await Task.Run(() =>
                            {
                                if (string.IsNullOrEmpty(ETC.fstList.TableName))
                                {
                                    ETC.LoadDBSync(ETC.fstList, "FST.gfs", false);
                                }
                            });

                            Activity.StartActivity(typeof(FSTDBMainActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        default:
                            //ETC.ShowSnackbar(snackbarLayout, Resource.String.AbnormalAccess, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                            Toast.MakeText(Activity, Resource.String.AbnormalAccess, ToastLength.Short).Show();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, Activity);
                    //ETC.ShowSnackbar(snackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
                    Toast.MakeText(Activity, Resource.String.MenuAccess_Fail, ToastLength.Short).Show();
                }
            }
        }

        internal class GFDv1Fragment : AndroidX.Fragment.App.Fragment
        {
            private RecyclerView recyclerView;

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                var view = inflater.Inflate(Resource.Layout.Main_RecyclerLayout, container, false);

                recyclerView = view.FindViewById<RecyclerView>(Resource.Id.MainRecyclerView);

                recyclerView.SetLayoutManager(new GridLayoutManager(Activity, 2));

                var adapter = new MainRecyclerListAdapter(Activity, 1);
                adapter.ItemClick += Adapter_ItemClick;

                recyclerView.SetAdapter(adapter);

                return view;
            }

            private async void Adapter_ItemClick(object sender, int e)
            {
                await Task.Delay(100);

                try
                {
                    var oldGFDIntent = new Intent(Activity, typeof(OldGFDViewer));
                    oldGFDIntent.PutExtra("Index", e);
                    StartActivity(oldGFDIntent);
                    Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, Activity);
                    //ETC.ShowSnackbar(snackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
                    Toast.MakeText(Activity, Resource.String.MenuAccess_Fail, ToastLength.Short).Show();
                }
            }
        }

        internal class GFUtilFragment : AndroidX.Fragment.App.Fragment
        {
            private RecyclerView recyclerView;

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                var view = inflater.Inflate(Resource.Layout.Main_RecyclerLayout, container, false);

                recyclerView = view.FindViewById<RecyclerView>(Resource.Id.MainRecyclerView);

                recyclerView.SetLayoutManager(new GridLayoutManager(Activity, 2));

                var adapter = new MainRecyclerListAdapter(Activity, 2);
                adapter.ItemClick += Adapter_ItemClick;

                recyclerView.SetAdapter(adapter);

                return view;
            }

            internal async void Adapter_ItemClick(object sender, int e)
            {
                await Task.Delay(10);

                try
                {
                    switch (e)
                    {
                        case 0:
                            var newsIntent = new Intent(Activity, typeof(WebBrowserActivity));
                            newsIntent.PutExtra("url", "http://www.girlsfrontline.co.kr/archives/category/news");
                            StartActivity(newsIntent);
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 1:
                            if (int.Parse(Build.VERSION.Release.Split('.')[0]) >= 6)
                            {
                                (Activity as Main).CheckPermission(Manifest.Permission.Internet);
                            }

                            Activity.StartActivity(typeof(EventListActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 2:
                            if (int.Parse(Build.VERSION.Release.Split('.')[0]) >= 6)
                            {
                                (Activity as Main).CheckPermission(Manifest.Permission.Internet);
                            }

                            var mdIntent = new Intent(Activity, typeof(WebBrowserActivity));
                            mdIntent.PutExtra("url", "https://tempkaridc.github.io/gf/");
                            StartActivity(mdIntent);
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 3:
                            Activity.StartActivity(typeof(CalcMainActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 4:
                            if (int.Parse(Build.VERSION.Release.Split('.')[0]) >= 6)
                            {
                                (Activity as Main).CheckPermission(Manifest.Permission.Internet);
                            }

                            var areaTipIntent = new Intent(Activity, typeof(WebBrowserActivity));
                            areaTipIntent.PutExtra("url", "https://cafe.naver.com/girlsfrontlinekr/235663");
                            StartActivity(areaTipIntent);
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 5:
                            //ETC.ShowSnackbar(SnackbarLayout, Resource.String.DevMode, Snackbar.LengthShort);
                            Activity.StartActivity(typeof(ShortGuideBookViewer));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 6:
                            Activity.StartActivity(typeof(StoryActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 7:
                            var sdIntent = new Intent(Activity, typeof(WebBrowserActivity));
                            sdIntent.PutExtra("url", "http://urk96.github.io/gfd-sd-simulator");
                            StartActivity(sdIntent);
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        default:
                            //ETC.ShowSnackbar(snackbarLayout, Resource.String.AbnormalAccess, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                            Toast.MakeText(Activity, Resource.String.AbnormalAccess, ToastLength.Short).Show();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, Activity);
                    //ETC.ShowSnackbar(snackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
                    Toast.MakeText(Activity, Resource.String.MenuAccess_Fail, ToastLength.Short).Show();
                }
            }
        }

        internal class OtherFragment : AndroidX.Fragment.App.Fragment
        {
            private RecyclerView recyclerView;

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                var view = inflater.Inflate(Resource.Layout.Main_RecyclerLayout, container, false);

                recyclerView = view.FindViewById<RecyclerView>(Resource.Id.MainRecyclerView);

                recyclerView.SetLayoutManager(new GridLayoutManager(Activity, 2));

                var adapter = new MainRecyclerListAdapter(Activity, 3);
                adapter.ItemClick += Adapter_ItemClick;

                recyclerView.SetAdapter(adapter);

                return view;
            }

            internal async void Adapter_ItemClick(object sender, int e)
            {
                await Task.Delay(10);

                try
                {
                    switch (e)
                    {
                        case 0:
                            if (ETC.dbVersion != 0)
                            {
                                Activity.StartActivity(typeof(ProductSimulatorCategoryActivity));
                                Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            }
                            else
                            {
                                //ETC.ShowSnackbar(snackbarLayout, Resource.String.NoDBFiles, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                                Toast.MakeText(Activity, Resource.String.NoDBFiles, ToastLength.Short).Show();
                            }
                            break;
                        case 1:
                            Activity.StartActivity(typeof(CartoonActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 2:
                            //ETC.ShowSnackbar(snackbarLayout, Resource.String.DevMode, Snackbar.LengthShort);
                            Activity.StartActivity(typeof(GFOSTPlayerActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        case 3:
                            Activity.StartActivity(typeof(GFPVListActivity));
                            Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                            break;
                        default:
                            //ETC.ShowSnackbar(snackbarLayout, Resource.String.AbnormalAccess, Snackbar.LengthShort, Android.Graphics.Color.DarkRed);
                            Toast.MakeText(Activity, Resource.String.AbnormalAccess, ToastLength.Short).Show();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, Activity);
                    //ETC.ShowSnackbar(snackbarLayout, Resource.String.MenuAccess_Fail, Snackbar.LengthLong, Android.Graphics.Color.DarkRed);
                    Toast.MakeText(Activity, Resource.String.MenuAccess_Fail, ToastLength.Short).Show();
                }
            }
        }

        class MainRecyclerViewHolder : RecyclerView.ViewHolder
        {
            public TextView Title { get; private set; }

            public MainRecyclerViewHolder(View view, Action<int> listener) : base(view)
            {
                Title = view.FindViewById<TextView>(Resource.Id.MainRecyclerView_List_Title);

                view.Click += (sender, e) => listener(LayoutPosition);
            }
        }

        class MainRecyclerListAdapter : RecyclerView.Adapter
        {
            string[] title;
            string[] iconId;
            Activity context;

            public event EventHandler<int> ItemClick;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="context"></param>
            /// <param name="type">0 : DB, 1 : GFDv1, 2 : GFUtil, 3 : Other</param>
            public MainRecyclerListAdapter(Activity context, int type = 0)
            {
                this.context = context;

                switch (type)
                {
                    case 0:
                        title = ETC.Resources.GetStringArray(Resource.Array.Main_DB_TitleList);
                        break;
                    case 1:
                        int id = 0;

                        if (ETC.locale.Language == "ko")
                        {
                            id = Resource.Array.Main_GFDv1_TitleList_ko;
                        }
                        else
                        {
                            id = Resource.Array.Main_GFDv1_TitleList;
                        }

                        title = ETC.Resources.GetStringArray(id);
                        break;
                    case 2:
                        title = ETC.Resources.GetStringArray(Resource.Array.Main_GFUtil_TitleList);
                        break;
                    case 3:
                        title = ETC.Resources.GetStringArray(Resource.Array.Main_Other_TitleList);
                        break;
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Main_RecyclerView_ListLayout, parent, false);

                var vh = new MainRecyclerViewHolder(view, OnClick);

                return vh;
            }

            public override int ItemCount
            {
                get { return title.Length; }
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
                var vh = holder as MainRecyclerViewHolder;

                try
                {
                    vh.Title.Text = title[position];
                }
                catch (Exception ex)
                {
                    ETC.LogError(ex, context);
                    Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
                }
            }
        }

    }
}