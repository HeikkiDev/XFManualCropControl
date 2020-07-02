using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XFManualCropApp.Controls;
using Color = Android.Graphics.Color;

[assembly: ExportRenderer(typeof(HoleLayerView), typeof(XFManualCropApp.Droid.Renderers.HoleLayerViewRenderer))]
namespace XFManualCropApp.Droid.Renderers
{
    internal class HoleLayerViewRenderer : ViewRenderer
    {
        private HoleLayerView _holeLayerView;
        private bool _drawRectangle;
        private int _screenPixelsWidth;
        private int _screenPixelsHeight;

        public HoleLayerViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                return;
            }

            SetLayerType(LayerType.Software, null);
            SetBackgroundColor(Color.Transparent);

            _holeLayerView = (HoleLayerView)e.NewElement;
            _holeLayerView.DrawRectangleHole += HoleLayerViewOnDrawRectangleHole;

            var displayMetrics = new DisplayMetrics();
            CrossCurrentActivity.Current.Activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
            _screenPixelsWidth = displayMetrics.WidthPixels;
            _screenPixelsHeight = displayMetrics.HeightPixels;
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (!_drawRectangle)
            {
                return;
            }

            double scale = _screenPixelsWidth / Element.Width;

            var points = new Path();
            points.MoveTo((float)(_holeLayerView.TopLeftCorner.X * scale), (float)(_holeLayerView.TopLeftCorner.Y * scale));
            points.LineTo((float)(_holeLayerView.TopRightCorner.X * scale), (float)(_holeLayerView.TopRightCorner.Y * scale));
            points.LineTo((float)(_holeLayerView.BottomRightCorner.X * scale), (float)(_holeLayerView.BottomRightCorner.Y * scale));
            points.LineTo((float)(_holeLayerView.BottomLeftCorner.X * scale), (float)(_holeLayerView.BottomLeftCorner.Y * scale));
            points.LineTo((float)(_holeLayerView.TopLeftCorner.X * scale), (float)(_holeLayerView.TopLeftCorner.Y * scale));
            points.Close();

            var transparentPaint = new Paint { Color = Color.Transparent };
            transparentPaint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Clear));

            var borderPaint = new Paint { Color = Color.Black, StrokeWidth = 4 };
            borderPaint.SetStyle(Paint.Style.Stroke);

            canvas.DrawColor(Color.Argb(150, 0, 0, 0));
            canvas.DrawPath(points, transparentPaint);
            canvas.DrawPath(points, borderPaint);
        }

        private void HoleLayerViewOnDrawRectangleHole(object sender, EventArgs e)
        {
            _drawRectangle = true;
            Invalidate(); //redraw
        }
    }
}