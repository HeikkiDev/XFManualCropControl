using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XFManualCropApp.Controls;
using XFManualCropApp.iOS.Extensions;

[assembly: ExportRenderer(typeof(HoleLayerView), typeof(XFManualCropApp.iOS.Renderers.HoleLayerViewRenderer))]
namespace XFManualCropApp.iOS.Renderers
{
    internal class HoleLayerViewRenderer : ViewRenderer<HoleLayerView, UIView>
    {
        private UIView _nativeView;
        private HoleLayerView _holeLayerView;
        private CAShapeLayer _holeShapeLayer;
        private CAShapeLayer _borderShapeLayer;
        private nfloat _layerWidth;
        private nfloat _layerHeight;

        protected override void OnElementChanged(ElementChangedEventArgs<HoleLayerView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                return;
            }

            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            UIView superView = window.RootViewController.GetViewControllerOnTopOfStack().View;
            _layerWidth = superView.Bounds.Width;
            _layerHeight = superView.Bounds.Height;
            _nativeView = new UIView(new CGRect(x: 0, y: 0, width: _layerWidth, height: _layerHeight));
            _nativeView.BackgroundColor = UIColor.Clear;
            SetNativeControl(_nativeView);

            _holeShapeLayer = new CAShapeLayer();
            _nativeView.Layer.AddSublayer(_holeShapeLayer);
            _borderShapeLayer = new CAShapeLayer();
            _nativeView.Layer.AddSublayer(_borderShapeLayer);

            _holeLayerView = (HoleLayerView)e.NewElement;
            _holeLayerView.DrawRectangleHole += HoleLayerViewOnDrawRectangleHole;

            var topLeft = _holeLayerView.TopLeftCorner;
            var topRight = _holeLayerView.TopRightCorner;
            var bottomRight = _holeLayerView.BottomRightCorner;
            var bottomLeft = _holeLayerView.BottomLeftCorner;
            _holeLayerView.TopLeftCorner = topLeft;
            _holeLayerView.TopRightCorner = topRight;
            _holeLayerView.BottomRightCorner = bottomRight;
            _holeLayerView.BottomLeftCorner = bottomLeft;
        }

        private void HoleLayerViewOnDrawRectangleHole(object sender, EventArgs e)
        {
            Point firstCorner = _holeLayerView.TopLeftCorner;
            Point secondCorner = _holeLayerView.TopRightCorner;
            Point thirstCorner = _holeLayerView.BottomRightCorner;
            Point fourthCorner = _holeLayerView.BottomLeftCorner;

            var overlayPath = UIBezierPath.FromRect(_nativeView.Bounds);

            var transparentPath = new UIBezierPath();
            transparentPath.MoveTo(new CGPoint(firstCorner.X, firstCorner.Y));
            transparentPath.AddLineTo(new CGPoint(secondCorner.X, secondCorner.Y));
            transparentPath.AddLineTo(new CGPoint(thirstCorner.X, thirstCorner.Y));
            transparentPath.AddLineTo(new CGPoint(fourthCorner.X, fourthCorner.Y));
            transparentPath.ClosePath();

            overlayPath.AppendPath(transparentPath);
            overlayPath.UsesEvenOddFillRule = true;

            // Border path
            _borderShapeLayer.Path = transparentPath.CGPath;
            _borderShapeLayer.StrokeColor = UIColor.Black.CGColor;
            _borderShapeLayer.FillColor = UIColor.Clear.CGColor;

            // Rectangle hole path
            _holeShapeLayer.FillRule = CAShapeLayer.FillRuleEvenOdd;
            _holeShapeLayer.FillColor = UIColor.Black.ColorWithAlpha((nfloat)0.5).CGColor;
            _holeShapeLayer.Path = overlayPath.CGPath;
        }
    }
}