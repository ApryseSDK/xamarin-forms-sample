using CustomRenderer;
using CustomRenderer.UWP;
using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;
using Windows.Foundation;

[assembly: ExportRenderer(typeof(ViewerPage), typeof(ViewerPageRenderer))]
namespace CustomRenderer.UWP
{
    public class ViewerPageRenderer : PageRenderer
    {
        Page page;
        Application app;

        pdftron.PDF.PDFViewCtrl mPdfViewCtrl;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            try
            {
                app = Application.Current;

                SetupUserInterface();

                this.Children.Add(page);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"      ERROR: ", ex.Message);
            }
        }

        void SetupUserInterface()
        {
            mPdfViewCtrl = new pdftron.PDF.PDFViewCtrl();
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(mPdfViewCtrl);

            string path = System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "sample.pdf");
            pdftron.PDF.PDFDoc doc = new pdftron.PDF.PDFDoc(path);
            mPdfViewCtrl.SetDoc(doc);
            mPdfViewCtrl.SetPagePresentationMode(pdftron.PDF.PDFViewCtrlPagePresentationMode.e_single_page);

            page = new Page();
            page.Content = stackPanel;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            page.Arrange(new Windows.Foundation.Rect(0, 0, finalSize.Width, finalSize.Height));
            return finalSize;
        }
    }
}
