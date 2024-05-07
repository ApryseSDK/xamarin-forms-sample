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
            mDocumentView.NavigationButtonPressed += DocumentView_OnNavButtonPressed;
        }

        async void DocumentView_OnNavButtonPressed(object sender, EventArgs e)
        {
            //await this.Element.Navigation.PopAsync();
            DocumentActivity.OpenDocument(activity, GetFile(), GetConfig());
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
                .DisableToolModes(new ToolManager.ToolMode[]
                {
                    ToolManager.ToolMode.AreaMeasureCreate,
                    ToolManager.ToolMode.ArrowCreate,
                    ToolManager.ToolMode.CalloutCreate,
                    ToolManager.ToolMode.CloudCreate,
                    ToolManager.ToolMode.CountMeasurement,
                    ToolManager.ToolMode.FileAttachmentCreate,
                    ToolManager.ToolMode.FormCheckboxCreate,
                    ToolManager.ToolMode.FormComboBoxCreate,
                    ToolManager.ToolMode.FormListBoxCreate,
                    ToolManager.ToolMode.FormRadioGroupCreate,
                    ToolManager.ToolMode.FormSignatureCreate,
                    ToolManager.ToolMode.FormTextFieldCreate,
                    ToolManager.ToolMode.FreeTextDateCreate,
                    ToolManager.ToolMode.FreeTextSpacingCreate,
                    ToolManager.ToolMode.DigitalSignature,
                    ToolManager.ToolMode.LineCreate,
                    ToolManager.ToolMode.OvalCreate,
                    ToolManager.ToolMode.PerimeterMeasureCreate,
                    ToolManager.ToolMode.PolygonCreate,
                    ToolManager.ToolMode.PolylineCreate,
                    ToolManager.ToolMode.RectAreaMeasureCreate,
                    ToolManager.ToolMode.RectCreate,
                    ToolManager.ToolMode.RectLink,
                    ToolManager.ToolMode.RectRedaction,
                    ToolManager.ToolMode.RubberStamper,
                    ToolManager.ToolMode.RulerCreate,
                    ToolManager.ToolMode.SoundCreate,
                    ToolManager.ToolMode.Stamper,
                    ToolManager.ToolMode.TextLinkCreate,
                    ToolManager.ToolMode.TextRedaction,
                })
                .SetAutoSelect(true);
            var builder = new ViewerConfig.Builder();
            var config = builder
                .MultiTabEnabled(false)
                .FullscreenModeEnabled(false)
                .UseSupportActionBar(false)
                .ToolManagerBuilder(toolmanagerBuilder)
                .HideToolbars(new string[]
                {
                    pdftron.PDF.Widget.Toolbar.Component.DefaultToolbars.TagDrawToolbar,
                    pdftron.PDF.Widget.Toolbar.Component.DefaultToolbars.TagFavoriteToolbar,
                    pdftron.PDF.Widget.Toolbar.Component.DefaultToolbars.TagInsertToolbar,
                    pdftron.PDF.Widget.Toolbar.Component.DefaultToolbars.TagMeasureToolbar,
                    pdftron.PDF.Widget.Toolbar.Component.DefaultToolbars.TagPensToolbar,
                    pdftron.PDF.Widget.Toolbar.Component.DefaultToolbars.TagPrepareFormToolbar,
                    pdftron.PDF.Widget.Toolbar.Component.DefaultToolbars.TagRedactionToolbar,
                })
                .SaveCopyExportPath(this.Context.FilesDir.AbsolutePath)
                .ShowAnnotationsList(false)
                .ShowBookmarksView(false)
                .ShowBottomToolbar(false)
                .ShowEditPagesOption(false)
                .ShowSaveCopyOption(false)
                .ShowShareOption(false)
                .ShowDocumentSettingsOption(false)
                .ShowDigitalSignaturesOption(false)
                .ShowPrintOption(false)
                .ShowCloseTabOption(false)
                .Build();
            return config;
        }
    }
}

