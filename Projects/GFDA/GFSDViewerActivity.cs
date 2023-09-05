using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.CoordinatorLayout.Widget;

using Google.Android.Material.Snackbar;

using Org.Apache.Http.Client.Params;

using SkiaSharp.Views.Android;

using static Google.Android.Material.Snackbar.Snackbar;
using SkiaSpineSharp;
using Spine;
using System.Timers;
using System.Threading;
using Javax.Security.Auth;
using static Android.Views.Choreographer;
using SkiaSharp;
using Xamarin.Essentials;

namespace GFDA
{
    [Activity(Label = "@string/Activity_SDViewerActivity",
              Theme = "@style/GFS.Toolbar",
              ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GFSDViewerActivity : BaseAppCompatActivity
    {
        private const int DefaultFrameRate = 60;

        private float updateDelta = 0;
        private int frameRate;
        private readonly string dataBasePath = Path.Combine(ETC.cachePath, "SDAnimationData");
        private readonly string serverDataBasePath = Path.Combine(ETC.server, "Data", "SDData");
        private string atlasDataPath = string.Empty;
        private string skelDataPath = string.Empty;
        private string textureDataPath = string.Empty;
        private bool isRendering = false;
        private bool isDataLoading = false;
        private float centerScreenX = 0;
        private float centerScreenY = 0;
        private System.Timers.Timer renderTimer;
        private List<Animation> sdAnimations = new();
        private SkiaSpineRenderer renderer = null;
        private Skeleton skeleton = null;
        private AnimationState animationState = null;
        private AndroidX.AppCompat.Widget.Toolbar toolbar;
        private Spinner characterSelector;
        private Spinner animationSelector;
        private SKCanvasView canvasView;
        private CoordinatorLayout snackbarLayout;

        private int FrameRate
        {
            get => frameRate;
            set
            {
                frameRate = value;
                updateDelta = 0.015f * (DefaultFrameRate / frameRate);
            }
        }

        private float SDScale { get; set; } = 3;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                if (ETC.useLightTheme)
                {
                    SetTheme(Resource.Style.GFS_Toolbar_Light);
                }

                SetContentView(Resource.Layout.GFSDViewerLayout);

                toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.GFSDViewerToolbar);
                characterSelector = FindViewById<Spinner>(Resource.Id.GFSDViewerCharacterSelector);
                animationSelector = FindViewById<Spinner>(Resource.Id.GFSDViewerAnimationSelector);
                canvasView = FindViewById<SKCanvasView>(Resource.Id.GFSDViewerCanvasView);
                snackbarLayout = FindViewById<CoordinatorLayout>(Resource.Id.DBSnackbarLayout);

                SetSupportActionBar(toolbar);
                SupportActionBar.SetTitle(Resource.String.GFSDViewerActivity_Title);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                InitializeSelectors();
                CalcScreenCenter();

                FrameRate = 60;

                renderTimer = new()
                {
                    Interval = TimeSpan.FromMilliseconds(1000 / FrameRate).TotalMilliseconds
                };

                renderTimer.Elapsed += RenderTimer_Elapsed;

                canvasView.PaintSurface += CanvasView_PaintSurface;
            }
            catch (Exception ex)
            {
                ETC.LogError(ex, this);
                ETC.ShowSnackbar(snackbarLayout, Resource.String.Activity_OnCreateError, Snackbar.LengthShort, Android.Graphics.Color.DeepPink);
            }
        }

        private void CalcScreenCenter()
        {
            centerScreenX = (float)DeviceDisplay.MainDisplayInfo.Width / 2;
            centerScreenY = (float)DeviceDisplay.MainDisplayInfo.Height / 2;
        }

        private void RenderTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SpinWait.SpinUntil(() => !isRendering);

            if (isDataLoading)
            {
                renderTimer?.Stop();

                return;
            }

            animationState?.Update(updateDelta);
            animationState?.Apply(skeleton);
            skeleton?.UpdateWorldTransform();
            canvasView?.Invalidate();
        }

        private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            renderer ??= new()
            {
                QuadTriangles = new int[] { 0, 1, 2, 2, 3, 0 },
                TextureFilterQuality = SKFilterQuality.Medium
            };

            if (skeleton is not null)
            {
                isRendering = true;

                SKCanvas canvas = e.Surface.Canvas;

                canvas.Clear();
                canvas.Translate(centerScreenX, centerScreenY);
                canvas.Scale(1, -1);
                canvas.Scale(SDScale);

                try
                {
                    renderer?.Draw(e.Surface.Canvas, skeleton);
                }
                catch { }

                isRendering = false;
            }
        }

        private void InitializeSelectors()
        {
            string[] characterItems = new[]
            {
                "K2",
                "K3",
                "K5",
                //"K7",
                "K11"
            };
            ArrayAdapter adapter = new(this, Resource.Layout.SpinnerListLayout, characterItems);

            adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

            characterSelector.Adapter = adapter;

            characterSelector.ItemSelected += async (sender, e) =>
            {
                string characterName = characterItems[e.Position];

                isDataLoading = true;

                SpinWait.SpinUntil(() => !isRendering);

                await LoadSDData(characterName);

                isDataLoading = false;
            };

            animationSelector.ItemSelected += (sender, e) =>
            {
                isDataLoading = true;

                SpinWait.SpinUntil(() => !isRendering);

                animationState?.SetAnimation(0, sdAnimations[e.Position], true);

                isDataLoading = false;

                renderTimer?.Start();
            };
        }

        private async Task LoadSDData(string characterName)
        {
            await CheckSDData(characterName);

            SkiaSpineTextureLoader textureLoader = new();
            Atlas atlas = new(atlasDataPath, textureLoader);
            AtlasAttachmentLoader attachmentLoader = new(atlas);
            SkeletonBinary skeletonBinary = new(attachmentLoader);
            SkeletonData skeletonData = skeletonBinary.ReadSkeletonData(skelDataPath);

            skeleton = new(skeletonData);

            AnimationStateData animationStateData = new(skeletonData);

            animationState = new(animationStateData);

            sdAnimations.Clear();
            sdAnimations.AddRange(skeletonData.Animations);
            UpdateAnimationSelectorItems();

            // Local Functions

            void UpdateAnimationSelectorItems()
            {
                ArrayAdapter adapter = new(this, Resource.Layout.SpinnerListLayout, sdAnimations.Select(x => x.Name).ToArray());

                adapter.SetDropDownViewResource(Resource.Layout.SpinnerListLayout);

                animationSelector.Adapter = adapter;
            }
        }

        private async Task CheckSDData(string characterName)
        {
            string characterDataBasePath = Path.Combine(dataBasePath, characterName);

            if (!Directory.Exists(characterDataBasePath))
            {
                Directory.CreateDirectory(characterDataBasePath);
            }

            atlasDataPath = Path.Combine(characterDataBasePath, $"{characterName}.atlas");
            skelDataPath = Path.Combine(characterDataBasePath, $"{characterName}.skel");
            textureDataPath = Path.Combine(characterDataBasePath, $"{characterName}.png");

            string serverDataPath = Path.Combine(serverDataBasePath, characterName);

            if (!File.Exists(atlasDataPath))
            {
                string atlasServerDataPath = Path.Combine(serverDataPath, $"{characterName}.atlas");

                await DownloadData(atlasServerDataPath, atlasDataPath);
            }

            if (!File.Exists(skelDataPath))
            {
                string skelServerDataPath = Path.Combine(serverDataPath, $"{characterName}.skel");

                await DownloadData(skelServerDataPath, skelDataPath);
            }

            if (!File.Exists(textureDataPath))
            {
                string textureServerDataPath = Path.Combine(serverDataPath, $"{characterName}.png");

                await DownloadData(textureServerDataPath, textureDataPath);
            }

            FileInfo info = new(textureDataPath);


            // Local Functions

            async Task DownloadData(string url, string savePath)
            {
                using WebClient webClient = new();

                await webClient.DownloadFileTaskAsync(url, savePath);
            }
        }
    }
}