using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace XFManualCropApp
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly SKBitmap _originalBitmap;
        private bool _pageLoaded;
        private SKPath _pathToClip;
        private byte[] _bytearray;

        public MainPage()
        {
            InitializeComponent();
            
            _originalBitmap = LoadBitmapResource(typeof(MainPage), "XFManualCropApp.wallpaper.png");
            CropImageCanvas.PaintSurface += OnCanvasViewPaintSurface;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_pageLoaded)
            {
                return;
            }

            _pageLoaded = true;
            ManualCropView.Initialize();
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (_pathToClip != null)
            {
                // Crop canvas before drawing image
                canvas.ClipPath(_pathToClip);
            }

            canvas.DrawBitmap(_originalBitmap, info.Rect);

            if (_pathToClip != null)
            {
                // Get cropped image byte array
                var snap = surface.Snapshot();
                var data = snap.Encode();
                _bytearray = data.ToArray();
                
                ManualCropView.IsVisible = false;
                CropButton.IsVisible = false;
                DisplayCroppedButton.IsVisible = true;
            }
        }
        
        private void CropButton_OnClicked(object sender, EventArgs e)
        {
            _pathToClip = new SKPath();
            SKPoint[] arr = new SKPoint[]
            {
                new SKPoint((float) (ManualCropView.TopLeftCorner.X * DeviceDisplay.MainDisplayInfo.Density), (float) (ManualCropView.TopLeftCorner.Y * DeviceDisplay.MainDisplayInfo.Density)),
                new SKPoint((float) (ManualCropView.TopRightCorner.X * DeviceDisplay.MainDisplayInfo.Density), (float) (ManualCropView.TopRightCorner.Y * DeviceDisplay.MainDisplayInfo.Density)),
                new SKPoint((float) (ManualCropView.BottomRightCorner.X * DeviceDisplay.MainDisplayInfo.Density), (float) (ManualCropView.BottomRightCorner.Y * DeviceDisplay.MainDisplayInfo.Density)),
                new SKPoint((float) (ManualCropView.BottomLeftCorner.X * DeviceDisplay.MainDisplayInfo.Density), (float) (ManualCropView.BottomLeftCorner.Y * DeviceDisplay.MainDisplayInfo.Density))
            };
            _pathToClip.AddPoly(arr);
            
            CropImageCanvas.InvalidateSurface(); // redraw
        }

        private void DisplayCroppedButton_OnClicked(object sender, EventArgs e)
        {
            DisplayCroppedButton.IsVisible = false;
            CropImageCanvas.IsVisible = false;
            CroppedImageView.IsVisible = true;

            Stream stream = new MemoryStream(_bytearray);
            CroppedImageView.Source = ImageSource.FromStream(() => stream);
        }
        
        private static SKBitmap LoadBitmapResource(Type type, string resourceID)
        {
            Assembly assembly = type.GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            {
                return SKBitmap.Decode(stream);
            }
        }
    }
}
