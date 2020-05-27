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

using pdftron.FDF;
using pdftron.PDF;
using pdftron.PDF.Tools;
using pdftron.PDF.Controls;
using pdftron.PDF.Tools.Utils;
using pdftron.PDF.Config;
using Android.Content.Res;

using FragmentActivity = AndroidX.Fragment.App.FragmentActivity;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using System.Collections;

[assembly: ExportRenderer(typeof(AdvancedViewerPage), typeof(AdvancedViewerPageRenderer))]
namespace CustomRenderer.Droid
{
    public class AdvancedViewerPageRenderer : PageRenderer
    {
        global::Android.Views.View view;

        private DocumentView mDocumentView;

        Activity activity;

        private PDFDoc mPdfDoc;

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
            mDocumentView.MPdfViewCtrlTabHostFragment.TabDocumentLoaded += MPdfViewCtrlTabHostFragment_TabDocumentLoaded;

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

        private void MPdfViewCtrlTabHostFragment_TabDocumentLoaded(object sender, PdfViewCtrlTabHostFragment.TabDocumentLoadedEventArgs e)
        {
            mPdfDoc = TypeConvertHelper.ConvPdfDocToManaged(mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.PdfDoc);

            var toolManager = mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.ToolManager;
            toolManager.AnnotationsAdded += ToolManager_AnnotationsAdded;
            toolManager.AnnotationsModified += ToolManager_AnnotationsModified;
            toolManager.AnnotationsPreRemove += ToolManager_AnnotationsPreRemove;
        }

        private void ToolManager_AnnotationsPreRemove(object sender, ToolManager.AnnotationsPreRemoveEventArgs e)
        {
            foreach (var annotPair in e.Annots)
            {
                FDFDoc fdfDoc;
                var annots = new ArrayList();
                annots.Add(TypeConvertHelper.ConvAnnotToManaged(annotPair.Key));
                fdfDoc = this.mPdfDoc.FDFExtract(annots);

                String xfdf = fdfDoc.SaveAsXFDF();
                Console.WriteLine("pre remove xfdf", xfdf);
            }
        }

        private void ToolManager_AnnotationsModified(object sender, ToolManager.AnnotationsModifiedEventArgs e)
        {
            foreach (var annotPair in e.Annots)
            {
                FDFDoc fdfDoc;
                var annots = new ArrayList();
                annots.Add(TypeConvertHelper.ConvAnnotToManaged(annotPair.Key));
                fdfDoc = this.mPdfDoc.FDFExtract(annots);

                String xfdf = fdfDoc.SaveAsXFDF();
                Console.WriteLine("modify xfdf", xfdf);
            }
        }

        private void ToolManager_AnnotationsAdded(object sender, ToolManager.AnnotationsAddedEventArgs e)
        {
            foreach (var annotPair in e.Annots)
            {
                FDFDoc fdfDoc;
                var annots = new ArrayList();
                annots.Add(TypeConvertHelper.ConvAnnotToManaged(annotPair.Key));
                fdfDoc = this.mPdfDoc.FDFExtract(annots);

                String xfdf = fdfDoc.SaveAsXFDF();
                Console.WriteLine("add xfdf", xfdf);
            }
        }

        void SetupEventHandlers()
        {
            mDocumentView.NavigationButtonPressed += DocumentView_OnNavButtonPressed;
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
            var file = Utils.CopyResourceToLocal(this.Context, Resource.Raw.sample, "sample", ".pdf");
            return Android.Net.Uri.FromFile(file);
        }

        private ViewerConfig GetConfig()
        {
            var toolmanagerBuilder = ToolManagerBuilder.From()
                .SetAutoSelect(true);
            var builder = new ViewerConfig.Builder();
            var config = builder
                .MultiTabEnabled(true)
                .FullscreenModeEnabled(false)
                .UseSupportActionBar(false)
                .ToolManagerBuilder(toolmanagerBuilder)
                .SaveCopyExportPath(this.Context.FilesDir.AbsolutePath)
                .Build();
            return config;
        }

        private void onRemoteChange(String xfdf)
        {
            this.mPdfDoc.Lock();

            FDFDoc fdfDoc = this.mPdfDoc.FDFExtract(PDFDoc.ExtractFlag.e_annots_only);
            fdfDoc.MergeAnnots(xfdf);

            this.mPdfDoc.FDFUpdate(fdfDoc);

            this.mPdfDoc.Unlock();
        }
    }
}

