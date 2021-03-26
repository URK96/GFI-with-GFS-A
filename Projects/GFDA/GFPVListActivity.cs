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

namespace GFDA
{
    [Activity(Label = "GFPVListActivity", Theme = "@style/GFS.Toolbar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GFPVListActivity : BaseAppCompatActivity
    {
        internal static string pvPath;

        private RecyclerView recyclerView;

        internal static readonly string[] pvList =
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
            "PV_DreamDrama",
            "PV_DualRandomness",
            "PV_BountyFeast",
            "PV_MirrorStage",
            "PV_MirrorStage_2"
        };
        internal static readonly string[] titleList =
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
            ETC.Resources.GetString(Resource.String.Game_Event_DreamDrama),
            ETC.Resources.GetString(Resource.String.Game_Event_DualRandomness),
            ETC.Resources.GetString(Resource.String.Game_Event_BountyFeast),
            ETC.Resources.GetString(Resource.String.Game_Event_MirrorStage),
            $"{ETC.Resources.GetString(Resource.String.Game_Event_MirrorStage)} - 2"
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ETC.useLightTheme)
            {
                SetTheme(Resource.Style.GFS_Toolbar_Light);
            }

            // Create your application here
            SetContentView(Resource.Layout.GFPVMainLayout);

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

            public GFPVListAdapter() { }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var vh = holder as GFPVListViewHolder;

                vh.Title.Text = titleList[position];

                int iconId = position switch
                {
                    0 => Resource.Drawable.Album_Cube,
                    1 => Resource.Drawable.Album_HuntingRabbit,
                    2 => Resource.Drawable.Album_Arctic,
                    3 => Resource.Drawable.Album_DeepDive,
                    4 => Resource.Drawable.Album_Singularity,
                    5 => Resource.Drawable.Album_DJMAX,
                    6 => Resource.Drawable.Album_ContinuumTurbulence,
                    7 => Resource.Drawable.Album_Isomer,
                    8 => Resource.Drawable.Album_VA,
                    9 => Resource.Drawable.Album_ShatteredConnexion,
                    10 => Resource.Drawable.Album_PhantomSyndrome,
                    11 => Resource.Drawable.Album_PolarizedLight,
                    12 => Resource.Drawable.Album_PhotoGalleryPuzzle,
                    13 => Resource.Drawable.Album_BihaiSecret,
                    14 => Resource.Drawable.Album_DreamDrama,
                    15 => Resource.Drawable.Album_DualRandomness,
                    16 => Resource.Drawable.Album_BountyFeast,
                    17 or 18 => Resource.Drawable.Album_MirrorStage,
                    _ => default
                };

                vh.Icon.SetImageResource(iconId);

                if (File.Exists(Path.Combine(pvPath, $"{pvList[position]}.mp4")))
                {
                    vh.DownloadIcon.SetImageResource(ETC.useLightTheme ? Resource.Drawable.done_icon_whitetheme : Resource.Drawable.done_icon);
                }
                else
                {
                    vh.DownloadIcon.SetImageResource(ETC.useLightTheme ? Resource.Drawable.download_icon_wt : Resource.Drawable.download_icon);
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var view = LayoutInflater.From(parent?.Context).Inflate(Resource.Layout.GFPVListLayout, parent, false);
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