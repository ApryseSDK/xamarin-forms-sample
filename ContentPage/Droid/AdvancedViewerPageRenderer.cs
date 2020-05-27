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

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Page> e)
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
            mDocumentView.TabDocumentLoaded += MDocumentView_TabDocumentLoaded;

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

        private void MDocumentView_TabDocumentLoaded(object sender, PdfViewCtrlTabHostFragment.TabDocumentLoadedEventArgs e)
        {
            mPdfDoc = TypeConvertHelper.ConvPdfDocToManaged(mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.PdfDoc);

            mockAnnotChange();

            var toolManager = mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.ToolManager;
            toolManager.AnnotationsAdded += ToolManager_AnnotationsAdded;
            toolManager.AnnotationsModified += ToolManager_AnnotationsModified;
            toolManager.AnnotationsPreRemove += ToolManager_AnnotationsPreRemove;
        }

        private void ToolManager_AnnotationsPreRemove(object sender, ToolManager.AnnotationsPreRemoveEventArgs e)
        {
            foreach (var annotPair in e.Annots)
            {
                // for delete, we just want the id

                var annot = TypeConvertHelper.ConvAnnotToManaged(annotPair.Key);
                var id = annot?.GetUniqueID()?.GetAsPDFText();
                var xfdf = "<delete><id>" + id + "</id></delete>";

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

                xfdf = xfdf.Replace("<annots>", "<modify>");
                xfdf = xfdf.Replace("</annots>", "</modify>");

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

                xfdf = xfdf.Replace("<annots>", "<add>");
                xfdf = xfdf.Replace("</annots>", "</add>");

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

        private void mockAnnotChange()
        {
            var xfdf = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<xfdf xmlns=\"http://ns.adobe.com/xfdf/\" xml:space=\"preserve\">\n\t<add>\n\t\t<square style=\"solid\" width=\"5\" color=\"#E44234\" opacity=\"1\" creationdate=\"D:20200527165902Z\" flags=\"print\" date=\"D:20200527165902Z\" name=\"7dd20d5e-3568-418a-ade6-1d1e50e36881\" page=\"1\" rect=\"97.6445,426.754,202.999,510.573\" title=\"\" />\n\t</add>\n\t<pdf-info import-version=\"3\" version=\"2\" xmlns=\"http://www.pdftron.com/pdfinfo\" />\n</xfdf>";
            onRemoteChange(xfdf);
        }

        private void onRemoteChange(String xfdf)
        {
            this.mPdfDoc.Lock();

            FDFDoc fdfDoc = this.mPdfDoc.FDFExtract(PDFDoc.ExtractFlag.e_annots_only);
            fdfDoc.MergeAnnots(xfdf);

            this.mPdfDoc.FDFUpdate(fdfDoc);

            mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.PDFViewCtrl.Update(true);

            this.mPdfDoc.Unlock();
        }
    }
}

