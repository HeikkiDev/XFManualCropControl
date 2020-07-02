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
        }
    }
}
