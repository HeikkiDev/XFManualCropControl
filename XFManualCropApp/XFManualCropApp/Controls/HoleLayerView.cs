using System;
using Xamarin.Forms;

namespace XFManualCropApp.Controls
{
    public class HoleLayerView : View
    {
        public event EventHandler DrawRectangleHole;

        public Point TopLeftCorner { get; set; }
        public Point TopRightCorner { get; set; }
        public Point BottomLeftCorner { get; set; }
        public Point BottomRightCorner { get; set; }

        public void DrawRectangle()
        {
            DrawRectangleHole?.Invoke(this, EventArgs.Empty);
        }
    }
}
