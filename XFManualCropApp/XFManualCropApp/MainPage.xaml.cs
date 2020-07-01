using System.ComponentModel;
using Xamarin.Forms;

namespace XFManualCropApp
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private bool _pageLoaded;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_pageLoaded)
            {
                return;
            }

            _pageLoaded = true;
            CropView.Initialize();

            //var topLeft = new Point(50, 100);
            //var topRight = new Point(300, 100);
            //var bottomRight = new Point(300, 600);
            //var bottomLeft = new Point(50, 600);
            //CropView.Initialize(topLeft, topRight, bottomRight, bottomLeft);
        }
    }
}
