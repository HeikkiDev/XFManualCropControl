using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace XFManualCropApp.Controls
{
    public class ManualCropView : AbsoluteLayout
    {
        private const double BallContainerSize = 50;
        private const double BallDiameter = 25;
        private const double BallRadius = 12.5;

        private HoleLayerView _rectangleHoleLayer;
        private Frame _boxViewTopLeft;
        private Frame _boxViewTopRight;
        private Frame _boxViewBottomRight;
        private Frame _boxViewBottomLeft;
        private double _absoluteLayoutWidth;
        private double _absoluteLayoutHeight;
        private double _originalTopLeftTranslationX;
        private double _originalTopLeftTranslationY;
        private double _holeTopLeftTranslationX;
        private double _holeTopLeftTranslationY;
        private double _originalTopRightTranslationX;
        private double _originalTopRightTranslationY;
        private double _holeTopRightTranslationX;
        private double _holeTopRightTranslationY;
        private double _originalBottomRightTranslationX;
        private double _originalBottomRightTranslationY;
        private double _holeBottomRightTranslationX;
        private double _holeBottomRightTranslationY;
        private double _originalBottomLeftTranslationX;
        private double _originalBottomLeftTranslationY;
        private double _holeBottomLeftTranslationX;
        private double _holeBottomLeftTranslationY;
        private readonly PanGestureRecognizer _panGestureTopLeft;
        private readonly PanGestureRecognizer _panGestureTopRight;
        private readonly PanGestureRecognizer _panGestureBottomRight;
        private readonly PanGestureRecognizer _panGestureBottomLeft;
        private readonly TaskCompletionSource<bool> _sizeChangedTask;

        public Point TopLeftCorner => _rectangleHoleLayer.TopLeftCorner;
        public Point TopRightCorner => _rectangleHoleLayer.TopRightCorner;
        public Point BottomLeftCorner => _rectangleHoleLayer.BottomLeftCorner;
        public Point BottomRightCorner => _rectangleHoleLayer.BottomRightCorner;

        public Button ApplyButton { get; set; }

        public ManualCropView()
        {
            _panGestureTopLeft = new PanGestureRecognizer();
            _panGestureTopRight = new PanGestureRecognizer();
            _panGestureBottomRight = new PanGestureRecognizer();
            _panGestureBottomLeft = new PanGestureRecognizer();
            _sizeChangedTask = new TaskCompletionSource<bool>();
            SizeChanged += OnSizeChanged;
        }

        public async void Initialize(Point? topLeft = null, Point? topRight = null, Point? bottomRight = null, Point? bottomLeft = null)
        {
            await _sizeChangedTask.Task;

            if (topLeft == null || topRight == null || bottomRight == null || bottomLeft == null)
            {
                // Default crop size 15% screen margin
                topLeft = new Point(_absoluteLayoutWidth * 0.15, _absoluteLayoutHeight * 0.15);
                topRight = new Point(_absoluteLayoutWidth * 0.85, _absoluteLayoutHeight * 0.15);
                bottomRight = new Point(_absoluteLayoutWidth * 0.85, _absoluteLayoutHeight * 0.85);
                bottomLeft = new Point(_absoluteLayoutWidth * 0.15, _absoluteLayoutHeight * 0.85);
            }

            _boxViewTopLeft = CreateCircularBoxView();
            _boxViewTopRight = CreateCircularBoxView();
            _boxViewBottomRight = CreateCircularBoxView();
            _boxViewBottomLeft = CreateCircularBoxView();

            _rectangleHoleLayer = new HoleLayerView { TopLeftCorner = topLeft.Value, TopRightCorner = topRight.Value, BottomRightCorner = bottomRight.Value, BottomLeftCorner = bottomLeft.Value };

            Children.Add(_rectangleHoleLayer);
            Children.Add(_boxViewTopLeft);
            Children.Add(_boxViewTopRight);
            Children.Add(_boxViewBottomRight);
            Children.Add(_boxViewBottomLeft);

            _rectangleHoleLayer.DrawRectangle();

            _boxViewTopLeft.TranslationX = topLeft.Value.X - BallDiameter;
            _boxViewTopLeft.TranslationY = topLeft.Value.Y - BallDiameter;

            _boxViewTopRight.TranslationX = topRight.Value.X - BallDiameter;
            _boxViewTopRight.TranslationY = topRight.Value.Y - BallDiameter;

            _boxViewBottomRight.TranslationX = bottomRight.Value.X - BallDiameter;
            _boxViewBottomRight.TranslationY = bottomRight.Value.Y - BallDiameter;

            _boxViewBottomLeft.TranslationX = bottomLeft.Value.X - BallDiameter;
            _boxViewBottomLeft.TranslationY = bottomLeft.Value.Y - BallDiameter;

            _panGestureTopLeft.PanUpdated += OnPanUpdatedTopLeft;
            _panGestureTopRight.PanUpdated += OnPanUpdatedTopRight;
            _panGestureBottomRight.PanUpdated += OnPanUpdatedBottomRight;
            _panGestureBottomLeft.PanUpdated += OnPanUpdatedBottomLeft;
            _boxViewTopLeft.GestureRecognizers.Add(_panGestureTopLeft);
            _boxViewTopRight.GestureRecognizers.Add(_panGestureTopRight);
            _boxViewBottomRight.GestureRecognizers.Add(_panGestureBottomRight);
            _boxViewBottomLeft.GestureRecognizers.Add(_panGestureBottomLeft);
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            if (!(sender is AbsoluteLayout absoluteLayout))
            {
                return;
            }

            if (absoluteLayout.Height <= 0 || absoluteLayout.Width <= 0)
            {
                return;
            }

            _absoluteLayoutWidth = absoluteLayout.Width;
            _absoluteLayoutHeight = absoluteLayout.Height;
            _sizeChangedTask.SetResult(true);
        }

        private void OnPanUpdatedTopLeft(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _originalTopLeftTranslationX = _boxViewTopLeft.TranslationX;
                    _originalTopLeftTranslationY = _boxViewTopLeft.TranslationY;
                    _holeTopLeftTranslationX = _rectangleHoleLayer.TopLeftCorner.X;
                    _holeTopLeftTranslationY = _rectangleHoleLayer.TopLeftCorner.Y;
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    break;
                case GestureStatus.Running:

                    double translationX = _boxViewTopLeft.TranslationX;
                    double translationY = _boxViewTopLeft.TranslationY;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        translationX = _originalTopLeftTranslationX;
                        translationY = _originalTopLeftTranslationY;
                    }

                    // Border screen limit
                    if (translationX + e.TotalX <= 0 - BallRadius
                        || translationX + e.TotalX >= _absoluteLayoutWidth - (BallRadius * 3)
                        || translationY + e.TotalY <= 0 - BallRadius
                        || translationY + e.TotalY >= _absoluteLayoutHeight - (BallRadius * 3))
                    {
                        return;
                    }

                    // Intersection with other corner balls limit
                    if (translationX + e.TotalX >= _boxViewTopRight.TranslationX - BallDiameter
                        || translationY + e.TotalY >= _boxViewBottomLeft.TranslationY - BallDiameter
                        || translationY + e.TotalY >= _boxViewBottomRight.TranslationY - BallDiameter)
                    {
                        return;
                    }

                    double pointTranslationX;
                    double pointTranslationY;
                    double holeTranslationX;
                    double holeTranslationY;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        pointTranslationX = _originalTopLeftTranslationX + e.TotalX;
                        pointTranslationY = _originalTopLeftTranslationY + e.TotalY;
                        holeTranslationX = _holeTopLeftTranslationX + e.TotalX;
                        holeTranslationY = _holeTopLeftTranslationY + e.TotalY;
                    }
                    else
                    {
                        pointTranslationX = _boxViewTopLeft.TranslationX + e.TotalX;
                        pointTranslationY = _boxViewTopLeft.TranslationY + e.TotalY;
                        holeTranslationX = _rectangleHoleLayer.TopLeftCorner.X + e.TotalX;
                        holeTranslationY = _rectangleHoleLayer.TopLeftCorner.Y + e.TotalY;
                    }

                    _boxViewTopLeft.TranslationX = pointTranslationX;
                    _boxViewTopLeft.TranslationY = pointTranslationY;

                    Point pointToUpdate = _rectangleHoleLayer.TopLeftCorner;
                    pointToUpdate.X = holeTranslationX;
                    pointToUpdate.Y = holeTranslationY;

                    _rectangleHoleLayer.TopLeftCorner = pointToUpdate;
                    _rectangleHoleLayer.DrawRectangle();
                    break;
            }
        }

        private void OnPanUpdatedTopRight(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _originalTopRightTranslationX = _boxViewTopRight.TranslationX;
                    _originalTopRightTranslationY = _boxViewTopRight.TranslationY;
                    _holeTopRightTranslationX = _rectangleHoleLayer.TopRightCorner.X;
                    _holeTopRightTranslationY = _rectangleHoleLayer.TopRightCorner.Y;
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    break;
                case GestureStatus.Running:

                    double translationX = _boxViewTopRight.TranslationX;
                    double translationY = _boxViewTopRight.TranslationY;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        translationX = _originalTopRightTranslationX;
                        translationY = _originalTopRightTranslationY;
                    }

                    // Border screen limit
                    if (translationX + e.TotalX <= 0 - BallRadius
                        || translationX + e.TotalX >= _absoluteLayoutWidth - (BallRadius * 3)
                        || translationY + e.TotalY <= 0 - BallRadius
                        || translationY + e.TotalY >= _absoluteLayoutHeight - (BallRadius * 3))
                    {
                        return;
                    }

                    // Intersection with other corner balls limit
                    if (translationX + e.TotalX <= _boxViewTopLeft.TranslationX + BallDiameter
                        || translationY + e.TotalY >= _boxViewBottomLeft.TranslationY - BallDiameter
                        || translationY + e.TotalY >= _boxViewBottomRight.TranslationY - BallDiameter)
                    {
                        return;
                    }

                    double pointTranslationX;
                    double pointTranslationY;
                    double holeTranslationX;
                    double holeTranslationY;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        pointTranslationX = _originalTopRightTranslationX + e.TotalX;
                        pointTranslationY = _originalTopRightTranslationY + e.TotalY;
                        holeTranslationX = _holeTopRightTranslationX + e.TotalX;
                        holeTranslationY = _holeTopRightTranslationY + e.TotalY;
                    }
                    else
                    {
                        pointTranslationX = _boxViewTopRight.TranslationX + e.TotalX;
                        pointTranslationY = _boxViewTopRight.TranslationY + e.TotalY;
                        holeTranslationX = _rectangleHoleLayer.TopRightCorner.X + e.TotalX;
                        holeTranslationY = _rectangleHoleLayer.TopRightCorner.Y + e.TotalY;
                    }

                    _boxViewTopRight.TranslationX = pointTranslationX;
                    _boxViewTopRight.TranslationY = pointTranslationY;

                    Point pointToUpdate = _rectangleHoleLayer.TopRightCorner;
                    pointToUpdate.X = holeTranslationX;
                    pointToUpdate.Y = holeTranslationY;

                    _rectangleHoleLayer.TopRightCorner = pointToUpdate;
                    _rectangleHoleLayer.DrawRectangle();
                    break;
            }
        }

        private void OnPanUpdatedBottomRight(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _originalBottomRightTranslationX = _boxViewBottomRight.TranslationX;
                    _originalBottomRightTranslationY = _boxViewBottomRight.TranslationY;
                    _holeBottomRightTranslationX = _rectangleHoleLayer.BottomRightCorner.X;
                    _holeBottomRightTranslationY = _rectangleHoleLayer.BottomRightCorner.Y;
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    break;
                case GestureStatus.Running:

                    double translationX = _boxViewBottomRight.TranslationX;
                    double translationY = _boxViewBottomRight.TranslationY;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        translationX = _originalBottomRightTranslationX;
                        translationY = _originalBottomRightTranslationY;
                    }

                    // Border screen limit
                    if (translationX + e.TotalX <= 0 - BallRadius
                        || translationX + e.TotalX >= _absoluteLayoutWidth - (BallRadius * 3)
                        || translationY + e.TotalY <= 0 - BallRadius
                        || translationY + e.TotalY >= _absoluteLayoutHeight - (BallRadius * 3))
                    {
                        return;
                    }

                    // Intersection with other corner balls limit
                    if (translationX + e.TotalX <= _boxViewBottomLeft.TranslationX + BallDiameter
                        || translationY + e.TotalY <= _boxViewTopLeft.TranslationY + BallDiameter
                        || translationY + e.TotalY <= _boxViewTopRight.TranslationY + BallDiameter)
                    {
                        return;
                    }

                    double pointTranslationX;
                    double pointTranslationY;
                    double holeTranslationX;
                    double holeTranslationY;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        pointTranslationX = _originalBottomRightTranslationX + e.TotalX;
                        pointTranslationY = _originalBottomRightTranslationY + e.TotalY;
                        holeTranslationX = _holeBottomRightTranslationX + e.TotalX;
                        holeTranslationY = _holeBottomRightTranslationY + e.TotalY;
                    }
                    else
                    {
                        pointTranslationX = _boxViewBottomRight.TranslationX + e.TotalX;
                        pointTranslationY = _boxViewBottomRight.TranslationY + e.TotalY;
                        holeTranslationX = _rectangleHoleLayer.BottomRightCorner.X + e.TotalX;
                        holeTranslationY = _rectangleHoleLayer.BottomRightCorner.Y + e.TotalY;
                    }

                    _boxViewBottomRight.TranslationX = pointTranslationX;
                    _boxViewBottomRight.TranslationY = pointTranslationY;

                    Point pointToUpdate = _rectangleHoleLayer.BottomRightCorner;
                    pointToUpdate.X = holeTranslationX;
                    pointToUpdate.Y = holeTranslationY;

                    _rectangleHoleLayer.BottomRightCorner = pointToUpdate;
                    _rectangleHoleLayer.DrawRectangle();
                    break;
            }
        }

        private void OnPanUpdatedBottomLeft(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _originalBottomLeftTranslationX = _boxViewBottomLeft.TranslationX;
                    _originalBottomLeftTranslationY = _boxViewBottomLeft.TranslationY;
                    _holeBottomLeftTranslationX = _rectangleHoleLayer.BottomLeftCorner.X;
                    _holeBottomLeftTranslationY = _rectangleHoleLayer.BottomLeftCorner.Y;
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    break;
                case GestureStatus.Running:

                    double translationX = _boxViewBottomLeft.TranslationX;
                    double translationY = _boxViewBottomLeft.TranslationY;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        translationX = _originalBottomLeftTranslationX;
                        translationY = _originalBottomLeftTranslationY;
                    }

                    // Border screen limit
                    if (translationX + e.TotalX <= 0 - BallRadius
                        || translationX + e.TotalX >= _absoluteLayoutWidth - (BallRadius * 3)
                        || translationY + e.TotalY <= 0 - BallRadius
                        || translationY + e.TotalY >= _absoluteLayoutHeight - (BallRadius * 3))
                    {
                        return;
                    }

                    // Intersection with other corner balls limit
                    if (translationX + e.TotalX >= _boxViewBottomRight.TranslationX - BallDiameter
                        || translationY + e.TotalY <= _boxViewTopLeft.TranslationY + BallDiameter
                        || translationY + e.TotalY <= _boxViewTopRight.TranslationY + BallDiameter)
                    {
                        return;
                    }

                    double pointTranslationX;
                    double pointTranslationY;
                    double holeTranslationX;
                    double holeTranslationY;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        pointTranslationX = _originalBottomLeftTranslationX + e.TotalX;
                        pointTranslationY = _originalBottomLeftTranslationY + e.TotalY;
                        holeTranslationX = _holeBottomLeftTranslationX + e.TotalX;
                        holeTranslationY = _holeBottomLeftTranslationY + e.TotalY;
                    }
                    else
                    {
                        pointTranslationX = _boxViewBottomLeft.TranslationX + e.TotalX;
                        pointTranslationY = _boxViewBottomLeft.TranslationY + e.TotalY;
                        holeTranslationX = _rectangleHoleLayer.BottomLeftCorner.X + e.TotalX;
                        holeTranslationY = _rectangleHoleLayer.BottomLeftCorner.Y + e.TotalY;
                    }

                    _boxViewBottomLeft.TranslationX = pointTranslationX;
                    _boxViewBottomLeft.TranslationY = pointTranslationY;

                    Point pointToUpdate = _rectangleHoleLayer.BottomLeftCorner;
                    pointToUpdate.X = holeTranslationX;
                    pointToUpdate.Y = holeTranslationY;

                    _rectangleHoleLayer.BottomLeftCorner = pointToUpdate;
                    _rectangleHoleLayer.DrawRectangle();
                    break;
            }
        }

        private static Frame CreateCircularBoxView()
        {
            return new Frame()
            {
                WidthRequest = BallContainerSize,
                HeightRequest = BallContainerSize,
                Padding = 0,
                Margin = 0,
                HasShadow = false,
                BackgroundColor = Color.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Content = new BoxView
                {
                    WidthRequest = BallDiameter,
                    HeightRequest = BallDiameter,
                    Margin = 0,
                    CornerRadius = (Device.RuntimePlatform == Device.Android) ? BallContainerSize : BallRadius,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    BackgroundColor = Color.White
                }
            };
        }
    }
}
