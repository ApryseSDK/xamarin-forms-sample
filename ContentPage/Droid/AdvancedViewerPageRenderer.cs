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
using pdftron.PDF.Annots;

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
            mDocumentView.SetShowNavIcon(false);
            mDocumentView.TabDocumentLoaded += DocumentView_TabDocumentLoaded;

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
            mDocumentView.NavigationButtonPressed += DocumentView_OnNavButtonPressed;
        }

        private void DocumentView_TabDocumentLoaded(object sender, PdfViewCtrlTabHostFragment.TabDocumentLoadedEventArgs e)
        {
            var toolManager = mDocumentView.MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.ToolManager;
            toolManager.AnnotationsAdded += ToolManager_AnnotationsAdded;
            toolManager.AnnotationsModified += ToolManager_AnnotationsModified;
        }

        private void handleSignatureWidget(pdftron.PDF.Annot annot)
        {
            var widget = new Widget(annot);
            var field = widget.GetField();
            if (field.GetType() == pdftron.PDF.Field.Type.e_signature)
            {
                var sigWidget = new SignatureWidget(annot);
                var digiSigField = sigWidget.GetDigitalSignatureField();
                if (digiSigField.HasVisibleAppearance())
                {
                    Console.WriteLine("signature has appearance");
                }
                else
                {
                    Console.WriteLine("signature does not have appearance");
                }
            }
        }

        private void ToolManager_AnnotationsAdded(object sender, ToolManager.AnnotationsAddedEventArgs e)
        {
            foreach (var annotPair in e.Annots)
            {
                var annot = pdftron.PDF.TypeConvertHelper.ConvAnnotToManaged(annotPair.Key);
                if (annot.GetType() == pdftron.PDF.Annot.Type.e_Widget)
                {
                    handleSignatureWidget(annot);
                }
            }
        }
        private void ToolManager_AnnotationsModified(object sender, ToolManager.AnnotationsModifiedEventArgs e)
        {
            foreach (var annotPair in e.Annots)
            {
                var annot = pdftron.PDF.TypeConvertHelper.ConvAnnotToManaged(annotPair.Key);
                if (annot.GetType() == pdftron.PDF.Annot.Type.e_Widget)
                {
                    handleSignatureWidget(annot);
                }
            }
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
                .HideToolbars(new String[]{
                        pdftron.PDF.Widget.Toolbar.Component.DefaultToolbars.TagPrepareFormToolbar
                })
                .MultiTabEnabled(false)
                .ShowReflowOption(false)
                .ShowCloseTabOption(false)
                .FullscreenModeEnabled(false)
                .UseSupportActionBar(false)
                .ToolManagerBuilder(toolmanagerBuilder)
                .SaveCopyExportPath(this.Context.FilesDir.AbsolutePath)
                .Build();
            return config;
        }
    }
}

