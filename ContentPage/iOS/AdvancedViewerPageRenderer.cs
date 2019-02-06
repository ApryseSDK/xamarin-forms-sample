using AVFoundation;
using CoreGraphics;
using CustomRenderer;
using CustomRenderer.iOS;
using Foundation;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using pdftron.PDF;
using pdftron.PDF.Tools;
using pdftron.PDF.Controls;

[assembly: ExportRenderer(typeof(AdvancedViewerPage), typeof(AdvancedViewerPageRenderer))]
namespace CustomRenderer.iOS
{
    public class AdvancedViewerPageRenderer : PageRenderer
    {
        private PTTabbedDocumentViewController mTabViewController;

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            try
            {
                SetupUserInterface();
                SetupEventHandlers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"           ERROR: ", ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        void SetupUserInterface()
        {
            mTabViewController = new PTTabbedDocumentViewController();
            mTabViewController.TabsEnabled = false;
            UINavigationController navigationController = new UINavigationController(mTabViewController);

            AddChildViewController(navigationController);

            View.AddSubview(navigationController.View);

            navigationController.DidMoveToParentViewController(this);

            NSUrl fileURL = NSBundle.MainBundle.GetUrlForResource("sample", "pdf");
            NSError error = null;
            bool success = mTabViewController.OpenDocumentWithURL(fileURL, out error);
            if (!success)
            {
                Console.WriteLine("OpenDocumentWithURL failed...");
            }
        }

        void SetupEventHandlers()
        {
            mTabViewController.WillRemoveTabAtIndex += (sender, e) => {
                if (((PTTabbedDocumentViewController)sender).TabURLs.Length > 1)
                {
                    return;
                }
                OnNavButtonPressed();
            };

            mTabViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, e) => {
                OnNavButtonPressed();
            });
        }

        async void OnNavButtonPressed() 
        {
            await this.Element.Navigation.PopAsync();
        }

        public UIViewController FindParentViewController()
        {
            UIResponder parentResponder = this;
            while (parentResponder != null)
            {
                if (parentResponder is UIViewController)
                {
                    return (UIViewController)parentResponder;
                }
                parentResponder = parentResponder.NextResponder;
            }
            return null;
        }
    }
}