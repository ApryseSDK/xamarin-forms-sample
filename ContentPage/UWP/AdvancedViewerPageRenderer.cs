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
using pdftron.PDF.Tools.Controls.ViewModels.Viewer;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

[assembly: ExportRenderer(typeof(AdvancedViewerPage), typeof(AdvancedViewerPageRenderer))]
namespace CustomRenderer.UWP
{
    public class AdvancedViewerPageRenderer : PageRenderer
    {
        Page page;
        Application app;
        pdftron.PDF.Tools.Controls.Viewer.ViewerControl _viewerControl;

        protected override async void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            Xamarin.Forms.NavigationPage.SetHasBackButton(this.Element, false);

            try
            {
                app = Application.Current;

                await SetupUserInterfaceAsync();

                this.Children.Add(page);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"      ERROR: ", ex.Message);
            }
        }

        async Task SetupUserInterfaceAsync()
        {
            _viewerControl = new pdftron.PDF.Tools.Controls.Viewer.ViewerControl();
            string path = System.IO.Path.Combine(Package.Current.InstalledLocation.Path, "sample.pdf");
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(path);
            await _viewerControl.ActivateWithFileAsync(storageFile);

            page = new Page();
            page.Content = _viewerControl;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (page == null)
                return base.ArrangeOverride(finalSize);

            page.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            page.UpdateLayout();

            return finalSize;
        }
    }
}
