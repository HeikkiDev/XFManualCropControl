using UIKit;

namespace XFManualCropApp.iOS.Extensions
{
    public static class UIViewControllerExtensions
    {
        public static UIViewController GetViewControllerOnTopOfStack(this UIViewController rootViewController)
        {
            while (rootViewController.PresentedViewController != null)
            {
                rootViewController = rootViewController.PresentedViewController;
            }

            return rootViewController;
        }
    }
}