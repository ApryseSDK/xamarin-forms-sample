using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using CustomRenderer;
using CustomRenderer.Droid;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Views;
using Android.Graphics;
using Android.Widget;

using pdftron.PDF.Tools;
using pdftron.PDF.Controls;
using pdftron.PDF.Tools.Utils;
using pdftron.PDF.Config;
using Android.Content.Res;

using FragmentActivity = AndroidX.Fragment.App.FragmentActivity;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;

[assembly: ExportRenderer(typeof(AdvancedViewerPage), typeof(AdvancedViewerPageRenderer))]
namespace CustomRenderer.Droid
{
    public class AdvancedViewerPageRenderer : PageRenderer
    {
        global::Android.Views.View view;

        private DocumentView mDocumentView;

        Activity activity;

        public AdvancedViewerPageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
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
                AddView(view);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"			ERROR: ", ex.Message);
            }
        }

        void SetupUserInterface()
        {
            activity = this.Context as Activity;
            view = activity.LayoutInflater.Inflate(Resource.Layout.AdvancedViewerLayout, this, false);

            mDocumentView = view.FindViewById<DocumentView>(Resource.Id.document_view);

            var context = this.Context;
            FragmentManager childManager = null;
            if (context is FragmentActivity)
            {
                var activity = context as FragmentActivity;
                var manager = activity.SupportFragmentManager;

                var fragments = manager.Fragments;
                if (fragments.Count > 0)
                {
                    childManager = fragments[0].ChildFragmentManager;
                }
                if (childManager != null)
                {
                    mDocumentView.OpenDocument(GetFile(), "", GetConfig(), childManager);
                }
            }
        }

        void SetupEventHandlers()
        {
            mDocumentView.TabDocumentLoaded += MDocumentView_TabDocumentLoaded;
            mDocumentView.NavigationButtonPressed += DocumentView_OnNavButtonPressed;
        }

        private void MDocumentView_TabDocumentLoaded(object sender, PdfViewCtrlTabHostFragment.TabDocumentLoadedEventArgs e)
        {
            var pdfviewctrl = mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.PDFViewCtrl;
            mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.UpdateViewMode(pdftronprivate.PDF.PDFViewCtrl.PagePresentationModes.Facing);


            AddCanvas(1);
            AddCanvas(2);
        }

        async void DocumentView_OnNavButtonPressed(object sender, EventArgs e)
        {
            await this.Element.Navigation.PopAsync();
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            view.Measure(msw, msh);
            view.Layout(0, 0, r - l, b - t);
        }

        private Android.Net.Uri GetFile()
        {
            var file = Utils.CopyResourceToLocal(this.Context, Resource.Raw.sixth, "sample", ".pdf");
            return Android.Net.Uri.FromFile(file);
        }

        private ViewerConfig GetConfig()
        {
            var toolmanagerBuilder = ToolManagerBuilder.From()
                .SetAutoSelect(true);
            var builder = new ViewerConfig.Builder();
            var config = builder
                .ShowTopToolbar(false)
                .ShowBottomNavBar(false)
                .MultiTabEnabled(true)
                .FullscreenModeEnabled(false)
                .UseSupportActionBar(false)
                .ToolManagerBuilder(toolmanagerBuilder)
                .SaveCopyExportPath(this.Context.FilesDir.AbsolutePath)
                .Build();
            return config;
        }

        private void AddCanvas(int pageNumber)
        {
            var pdfviewctrl = mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.PDFViewCtrl;
            var pdfdoc = mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.PdfDoc;

            var firstPage = pdfdoc.GetPage(pageNumber);
            var cropBox = firstPage.CropBox;

            var canvasHeight = (int)(cropBox.Height);
            var canvasWidth = (int)(cropBox.Width);

            var layout = new NativeCustomRelativeLayout(pdfviewctrl.Context);
            var color = new Android.Graphics.Color(0, 255, 00, 20);
            layout.SetBackgroundColor(color);
            layout.SetRect(pdfviewctrl, new pdftronprivate.PDF.Rect(0, 0, canvasWidth, canvasHeight), pageNumber);
            pdfviewctrl.AddView(layout);

            var myButton = new Android.Widget.Button(pdfviewctrl.Context);
            myButton.Text = "BUTTON";
            myButton.SetTextColor(Android.Graphics.Color.Pink);


            layout.AddView(myButton);
        }
    }
}

