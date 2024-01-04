using AVFoundation;
using CoreGraphics;
using CustomRenderer;
using CustomRenderer.iOS;
using Foundation;
using System;
using UIKit;

using pdftron.PDF;
using pdftron.PDF.Tools;
using pdftron.PDF.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

// TODO Xamarin.Forms.ExportRendererAttribute is not longer supported. For more details see https://github.com/dotnet/maui/wiki/Using-Custom-Renderers-in-.NET-MAUI
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
                OpenDocument();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"           ERROR: ", ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            PTTabbedDocumentViewController tabbedDocumentViewController = mTabViewController;
            mTabViewController = null;

            nuint index = tabbedDocumentViewController.SelectedIndex;
            PTDocumentBaseViewController documentViewController = tabbedDocumentViewController.SelectedViewController;
            PDFDoc pdfDoc = TypeConvertHelper.ConvPdfDocToManaged(documentViewController.PdfViewCtrl.GetDoc());

            documentViewController.CloseDocumentWithCompletionHandler((bool success) => {
                pdfDoc.Close();
                pdfDoc = null;
            });

            tabbedDocumentViewController.RemoveTabAtIndex(index);
            tabbedDocumentViewController = null;

            GC.Collect();
        }

        void SetupUserInterface()
        {
            mTabViewController = new PTTabbedDocumentViewController();
            mTabViewController.ViewControllerClass = new ObjCRuntime.Class(typeof(PTDocumentController));

            mTabViewController.TabsEnabled = false;
            UINavigationController navigationController = new UINavigationController(mTabViewController);

            AddChildViewController(navigationController);

            View.AddSubview(navigationController.View);

            navigationController.DidMoveToParentViewController(this);
        }

        void SetupEventHandlers()
        {
            mTabViewController.CreateViewController += (sender, e) =>
            {
                return new PTDocumentController();
            };
            mTabViewController.WillRemoveTabAtIndex += (sender, e) =>
            {
                if (mTabViewController == null)
                {
                    return;
                }
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

        void OpenDocument()
        {
            NSUrl fileURL = NSBundle.MainBundle.GetUrlForResource("sample", "pdf");
            mTabViewController.OpenDocumentWithURL(fileURL);
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