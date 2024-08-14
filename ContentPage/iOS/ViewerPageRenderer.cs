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

[assembly: ExportRenderer(typeof(ViewerPage), typeof(ViewerPageRenderer))]
namespace CustomRenderer.iOS
{
    public class ViewerPageRenderer : PageRenderer
    {
        private PDFViewCtrl mPdfViewCtrl;
        private PDFDoc mPdfDoc;
        private PTToolManager mToolManager;
        private PTThumbnailSliderViewController mThumbnailSliderViewController;

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
                System.Diagnostics.Debug.WriteLine(@"			ERROR: ", ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            mPdfViewCtrl?.CloseDoc();
            mPdfViewCtrl = null;
            mPdfDoc?.Close();
            mPdfDoc = null;
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);
        }

        void SetupUserInterface()
        {
            CGRect viewRect = new CGRect(0, 0, View.Frame.Size.Width, View.Frame.Size.Height);
            mPdfViewCtrl = new pdftron.PDF.PDFViewCtrl(viewRect);

            mPdfViewCtrl.SetupThumbnails(false, true, true, 0, 500 * 1024 * 1024, 0.5);
            View.AddSubview(mPdfViewCtrl);

            mPdfViewCtrl.TranslatesAutoresizingMaskIntoConstraints = false;
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                mPdfViewCtrl.LeadingAnchor.ConstraintEqualTo(this.View.LeadingAnchor),
                mPdfViewCtrl.WidthAnchor.ConstraintEqualTo(this.View.WidthAnchor),
                mPdfViewCtrl.TopAnchor.ConstraintEqualTo(this.View.LayoutMarginsGuide.TopAnchor),
                mPdfViewCtrl.BottomAnchor.ConstraintEqualTo(this.View.BottomAnchor)
            });

            var docPath = "sample.pdf";
            mPdfDoc = new PDFDoc(docPath);
            mPdfViewCtrl.Doc = TypeConvertHelper.ConvPDFDocToNative(mPdfDoc);
            mPdfViewCtrl.PagePresentationMode = PagePresentationModes.e_single_page;
            mPdfViewCtrl.SetHighlightFields(true);

            mToolManager = new PTToolManager(mPdfViewCtrl);
            mPdfViewCtrl.ToolManager = mToolManager;

            mThumbnailSliderViewController = new PTThumbnailSliderViewController(mPdfViewCtrl);
            mThumbnailSliderViewController.View.TranslatesAutoresizingMaskIntoConstraints = false;

            AddChildViewController(mThumbnailSliderViewController);
            View.AddSubview(mThumbnailSliderViewController.View);

            bool isIOS11 = UIDevice.CurrentDevice.CheckSystemVersion(11, 0);
            var bottomAnchor = this.View.BottomAnchor;
            if (isIOS11)
            {
                bottomAnchor = this.View.SafeAreaLayoutGuide.BottomAnchor;
            }

            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                mThumbnailSliderViewController.View.LeadingAnchor.ConstraintEqualTo(this.View.LeadingAnchor),
                mThumbnailSliderViewController.View.WidthAnchor.ConstraintEqualTo(this.View.WidthAnchor),
                mThumbnailSliderViewController.View.BottomAnchor.ConstraintEqualTo(bottomAnchor)
            });
            mThumbnailSliderViewController.DidMoveToParentViewController(this);
        }

        void SetupEventHandlers()
        {
            mPdfViewCtrl.GotThumbAsync += (sender, e) =>
            {
                if (e.Image == null)
                {
                    return;
                }
                //this.mThumbnailSliderViewController?.SetThumbnail(e.Image, e.Page_num);
            };
            mPdfViewCtrl.PageNumberChangedFrom += (sender, e) =>
            {
                //mThumbnailSliderViewController?.SetSliderValue(e.NewPageNumber);
            };

            mToolManager.ToolManagerToolChanged += (sender, e) =>
            {
                //mAnnotationToolbar?.SetButtonForTool(mToolManager.Tool);
            };
        }
    }
}