using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.RecyclerView.Widget;

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace GFI_with_GFS_A
{
    [Activity(Label = "GFPVListActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GFPVListActivity : BaseAppCompatActivity
    {
        internal static string pvPath;

        private RecyclerView recyclerView;

        internal static string[] pvList =
        {
            "PV_Cube",
            "PV_HuntingRabbit",
            "PV_Arctic",
            "PV_DeepDive",
            "PV_Singularity",
            "PV_DJMAX",
            "PV_ContinuumTurbulence",
            "PV_Isomer",
            "PV_VA",
            "PV_ShatteredConnexion",
            "PV_PhantomSyndrome",
            "PV_PolarizedLight",
            "PV_PhotoGalleryPuzzle",
            "PV_BihaiSecret",
            "PV_DreamDrama"
        };
        internal static string[] titleList =
        {
            ETC.Resources.GetString(Resource.String.Game_Event_Cube),
            ETC.Resources.GetString(Resource.String.Game_Event_HuntingRabbit),
            ETC.Resources.GetString(Resource.String.Game_Event_Arctic),
            ETC.Resources.GetString(Resource.String.Game_Event_DeepDive),
            ETC.Resources.GetString(Resource.String.Game_Event_Singularity),
            ETC.Resources.GetString(Resource.String.Game_Event_DJMAX),
            ETC.Resources.GetString(Resource.String.Game_Event_ContinuumTurbulence),
            ETC.Resources.GetString(Resource.String.Game_Event_Isomer),
            ETC.Resources.GetString(Resource.String.Game_Event_VA),
            ETC.Resources.GetString(Resource.String.Game_Event_ShatteredConnexion),
            ETC.Resources.GetString(Resource.String.Game_Event_PhantomSyndrome),
            ETC.Resources.GetString(Resource.String.Game_Event_PolarizedLight),
            ETC.Resources.GetString(Resource.String.Game_Event_PhotoGalleryPuzzle),
            ETC.Resources.GetString(Resource.String.Game_Event_BihaiSecret),
            ETC.Resources.GetString(Resource.String.Game_Event_DreamDrama)
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.GFPVListLayout);

            SetSupportActionBar(FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.GFPVListMainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.Main_Other_GFPV);

            pvPath = Path.Combine(ETC.cachePath, "Video", "PV");

            recyclerView = FindViewById<RecyclerView>(Resource.Id.GFPVListRecyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));

            var adapter = new GFPVListAdapter();
            adapter.ItemClick += Adapter_ItemClick;

            recyclerView.SetAdapter(adapter);
        }

        private void Adapter_ItemClick(object sender, int e)
        {
            string pvFileName = pvList[e];

            try
            {
                if (!File.Exists(Path.Combine(pvPath, $"{pvFileName}.mp4")))
                {
                    var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this, ETC.dialogBG);
                    builder.SetTitle(Resource.String.GFPVActivity_DownloadRequireDialog_Title);
                    builder.SetMessage(Resource.String.GFPVActivity_DownloadRequireDialog_Message);
                    builder.SetCancelable(true);
                    builder.SetPositiveButton(Resource.String.AlertDialog_Download, async delegate { await PVDownload(pvFileName, e); });
                    builder.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

                    builder.Show();
                }
                else
                {
                    RunPV(Path.Combine(pvPath, $"{pvFileName}.mp4"));
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
        }

        private async Task PVDownload(string pvName, int position)
        {
            Dialog dialog = null;

            try
            {
                ProgressBar nowProgressBar;
                TextView nowProgress;

                View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

                v.FindViewById<TextView>(Resource.Id.ProgressStatusMessage).SetText(Resource.String.GFPVActivity_DownloadDialog_Message);
                v.FindViewById<TextView>(Resource.Id.ProgressNowFile).Text = pvName;
                v.FindViewById<LinearLayout>(Resource.Id.TotalProgressLayout).Visibility = ViewStates.Gone;

                nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
                nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

                var pd = new AndroidX.AppCompat.App.AlertDialog.Builder(this, ETC.dialogBGDownload);
                pd.SetTitle(Resource.String.GFPVActivity_DownloadDialog_Title);
                pd.SetCancelable(false);
                pd.SetView(v);

                dialog = pd.Create();
                dialog.Show();

                string localPath = Path.Combine(pvPath, $"{pvName}.mp4");
                string serverPath = Path.Combine(ETC.server, "Data", "Video", "PV", $"{pvName}.mp4");

                using (var wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (sender, e) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            nowProgressBar.Progress = e.ProgressPercentage;
                            nowProgress.Text = $"{e.ProgressPercentage}%";
                        });
                    };
                    wc.DownloadFileCompleted += (sender, e) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            nowProgressBar.Progress = 100;
                            nowProgress.Text = "100%";
                        });
                    };

                    await wc.DownloadFileTaskAsync(serverPath, localPath);
                }

                await Task.Delay(500);

                recyclerView.GetAdapter().NotifyItemChanged(position);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
            }
            finally
            {
                dialog?.Dismiss();
            }
        }

        private void RunPV(string filePath)
        {
            _ = Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
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

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
        }

        private class GFPVListViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Icon { get; private set; }
            public TextView Title { get; private set; }
            public ImageView DownloadIcon { get; private set; }

            public GFPVListViewHolder(View view, Action<int> listener) : base(view)
            {
                Icon = view?.FindViewById<ImageView>(Resource.Id.GFPVListIconImageView);
                Title = view?.FindViewById<TextView>(Resource.Id.GFPVListTitleText);
                DownloadIcon = view?.FindViewById<ImageView>(Resource.Id.GFPVListDownloadIconImageView);

                view.Click += (sender, e) => listener(LayoutPosition);
            }
        }

        private class GFPVListAdapter : RecyclerView.Adapter
        {
            public event EventHandler<int> ItemClick;

            public GFPVListAdapter()
            {

            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var vh = holder as GFPVListViewHolder;

                vh.Title.Text = titleList[position];

                int iconId = 0;

                switch (position)
                {
                    case 0:
                        iconId = Resource.Drawable.Album_Cube;
                        break;
                    case 1:
                        iconId = Resource.Drawable.Album_HuntingRabbit;
                        break;
                    case 2:
                        iconId = Resource.Drawable.Album_Arctic;
                        break;
                    case 3:
                        iconId = Resource.Drawable.Album_DeepDive;
                        break;
                    case 4:
                        iconId = Resource.Drawable.Album_Singularity;
                        break;
                    case 5:
                        iconId = Resource.Drawable.Album_DJMAX;
                        break;
                    case 6:
                        iconId = Resource.Drawable.Album_ContinuumTurbulence;
                        break;
                    case 7:
                        iconId = Resource.Drawable.Album_Isomer;
                        break;
                    case 8:
                        iconId = Resource.Drawable.Album_VA;
                        break;
                    case 9:
                        iconId = Resource.Drawable.Album_ShatteredConnexion;
                        break;
                    case 10:
                        iconId = Resource.Drawable.Album_PhantomSyndrome;
                        break;
                    case 11:
                        iconId = Resource.Drawable.Album_PolarizedLight;
                        break;
                    case 12:
                        iconId = Resource.Drawable.Album_PhotoGalleryPuzzle;
                        break;
                    case 13:
                        iconId = Resource.Drawable.Album_BihaiSecret;
                        break;
                    case 14:
                        iconId = Resource.Drawable.Album_DreamDrama;
                        break;
                }

                vh.Icon.SetImageResource(iconId);

                if (File.Exists(Path.Combine(pvPath, $"{pvList[position]}.mp4")))
                { 
                    if (ETC.useLightTheme)
                    {
                        vh.DownloadIcon.SetImageResource(Resource.Drawable.done_icon_whitetheme);
                    }
                    else
                    {
                        vh.DownloadIcon.SetImageResource(Resource.Drawable.done_icon);
                    }
                }
                else
                {
                    if (ETC.useLightTheme)
                    {
                        vh.DownloadIcon.SetImageResource(Resource.Drawable.download_icon_whitetheme);
                    }
                    else
                    {
                        vh.DownloadIcon.SetImageResource(Resource.Drawable.download_icon);
                    }
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.GFPVListListLayout, parent, false);
                var vh = new GFPVListViewHolder(view, OnClick);

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