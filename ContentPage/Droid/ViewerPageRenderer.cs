using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using CustomRenderer;
using CustomRenderer.Droid;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using pdftron.PDF.Tools;
using pdftron.PDF.Tools.Utils;
using pdftron.PDF.Config;
using Android.Content.Res;
using AndroidX.Fragment.App;
using pdftron.PDF.Widget.Toolbar.Component;
using pdftron.PDF.Widget.Preset.Component;
using pdftron.PDF.Widget.Toolbar;
using AndroidX.Lifecycle;
using pdftron.PDF.Widget.Preset.Component.View;
using pdftron.PDF.Widget.Toolbar.Component.View;
using pdftron.PDF.Widget.Toolbar.Builder;

[assembly: ExportRenderer(typeof(ViewerPage), typeof(ViewerPageRenderer))]
namespace CustomRenderer.Droid
{
    public class ViewerPageRenderer : PageRenderer
    {
        global::Android.Views.View view;

        private pdftron.PDF.PDFViewCtrl mPdfViewCtrl;
        private pdftron.PDF.PDFDoc mPdfDoc;
        private ToolManager mToolManager;
        private AnnotationToolbarComponent mAnnotationToolbarComponent;
        private PresetBarComponent mPresetBarComponent;
        private FrameLayout mToolbarContainer;
        private FrameLayout mPresetContainer;

        private FragmentActivity mFragmentActivity;

        public ViewerPageRenderer(Context context) : base(context)
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
            var activity = this.Context as Activity;
            view = activity.LayoutInflater.Inflate(Resource.Layout.ViewerLayout, this, false);

            // init UI
            mPdfViewCtrl = view.FindViewById<pdftron.PDF.PDFViewCtrl>(Resource.Id.pdfviewctrl);
            mToolbarContainer = view.FindViewById<FrameLayout>(Resource.Id.annotation_toolbar_container);
            mPresetContainer = view.FindViewById<FrameLayout>(Resource.Id.preset_container);

            // setup PDFViewCtrl and ToolManager
            AppUtils.SetupPDFViewCtrl(mPdfViewCtrl, PDFViewCtrlConfig.GetDefaultConfig(this.Context));

            if (activity is FragmentActivity)
            {
                mFragmentActivity = activity as FragmentActivity;
            }
            mToolManager = ToolManagerBuilder.From().Build(mFragmentActivity, mPdfViewCtrl);

            // setup toolbars
            SetupAnnotationToolbar();

            var file = Utils.CopyResourceToLocal(this.Context, Resource.Raw.sample, "sample", ".pdf");
            mPdfDoc = mPdfViewCtrl.OpenPDFUri(Android.Net.Uri.FromFile(file), "");

        }

        void SetupAnnotationToolbar()
        {
            var toolManagerViewModel = (ToolManagerViewModel)ViewModelProviders.Of(mFragmentActivity).Get(Java.Lang.Class.FromType(typeof(ToolManagerViewModel)));
            toolManagerViewModel.ToolManager = mToolManager;
            var presetViewModel = (PresetBarViewModel)ViewModelProviders.Of(mFragmentActivity).Get(Java.Lang.Class.FromType(typeof(PresetBarViewModel)));
            var annotationToolbarViewModel = (AnnotationToolbarViewModel)ViewModelProviders.Of(mFragmentActivity).Get(Java.Lang.Class.FromType(typeof(AnnotationToolbarViewModel)));

            mAnnotationToolbarComponent = new AnnotationToolbarComponent(
                mFragmentActivity,
                annotationToolbarViewModel,
                presetViewModel,
                toolManagerViewModel,
                new AnnotationToolbarView(mToolbarContainer)
            );
            mPresetBarComponent = new PresetBarComponent(
                mFragmentActivity,
                mFragmentActivity.SupportFragmentManager,
                presetViewModel,
                toolManagerViewModel,
                new PresetBarView(mPresetContainer)
            );

            // Create our custom toolbar and pass it to the annotation toolbar UI component
            mAnnotationToolbarComponent.InflateWithBuilder(
                    AnnotationToolbarBuilder.WithTag("Custom Toolbar")
                            .AddToolButton(ToolbarButtonType.Square, DefaultToolbars.ButtonId.Square.Value())
                            .AddToolButton(ToolbarButtonType.Ink, DefaultToolbars.ButtonId.Ink.Value())
                            .AddToolButton(ToolbarButtonType.FreeHighlight, DefaultToolbars.ButtonId.FreeHighlight.Value())
                            .AddToolButton(ToolbarButtonType.Eraser, DefaultToolbars.ButtonId.Eraser.Value())
                            .AddToolStickyButton(ToolbarButtonType.Undo, DefaultToolbars.ButtonId.Undo.Value())
                            .AddToolStickyButton(ToolbarButtonType.Redo, DefaultToolbars.ButtonId.Redo.Value())
            );
        }

        void SetupEventHandlers()
        {

        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            view.Measure(msw, msh);
            view.Layout(0, 0, r - l, b - t);
        }

        protected override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);


        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();

            mPdfViewCtrl?.Destroy();
            mPdfViewCtrl = null;
            mPdfDoc?.Close();
            mPdfDoc = null;
        }
    }
}

