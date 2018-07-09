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

[assembly: ExportRenderer(typeof(ViewerPage), typeof(ViewerPageRenderer))]
namespace CustomRenderer.Droid
{
    public class ViewerPageRenderer : PageRenderer
    {
        global::Android.Views.View view;

        private pdftron.PDF.PDFViewCtrl mPdfViewCtrl;
        private pdftron.PDF.PDFDoc mPdfDoc;
        private ToolManager mToolManager;
        private AnnotationToolbar mAnnotationToolbar;
        private ThumbnailSlider mSeekBar;

        Activity activity;

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
            activity = this.Context as Activity;
            view = activity.LayoutInflater.Inflate(Resource.Layout.ViewerLayout, this, false);

            mPdfViewCtrl = view.FindViewById<pdftron.PDF.PDFViewCtrl>(Resource.Id.pdfviewctrl);
            AppUtils.SetupPDFViewCtrl(mPdfViewCtrl, PDFViewCtrlConfig.GetDefaultConfig(this.Context));

            var file = Utils.CopyResourceToLocal(this.Context, Resource.Raw.sample, "sample", ".pdf");
            mPdfDoc = mPdfViewCtrl.OpenPDFUri(Android.Net.Uri.FromFile(file), "");

            Android.Support.V4.App.FragmentActivity fragmentActivity = null;
            if (activity is Android.Support.V4.App.FragmentActivity)
            {
                fragmentActivity = activity as Android.Support.V4.App.FragmentActivity;
            }
            mToolManager = ToolManagerBuilder.From().Build(fragmentActivity, mPdfViewCtrl);

            mAnnotationToolbar = view.FindViewById<AnnotationToolbar>(Resource.Id.annotationtoolbar);
            mAnnotationToolbar.FindViewById(Resource.Id.controls_annotation_toolbar_btn_close).Visibility = ViewStates.Gone;
            mAnnotationToolbar.Setup(mToolManager);
            mAnnotationToolbar.SetButtonStayDown(true);
            mAnnotationToolbar.Show();

            mSeekBar = view.FindViewById<ThumbnailSlider>(Resource.Id.thumbseekbar);
        }

        void SetupEventHandlers()
        {
            mPdfViewCtrl.PageNumberChanged += (sender, e) =>
            {
                mSeekBar?.SetProgress(e.CurPage);
            };
            mAnnotationToolbar.UndoRedo += (sender, e) =>
            {
                mSeekBar?.RefreshPageCount();
            };
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

            if (mAnnotationToolbar != null && mAnnotationToolbar.IsShowing)
            {
                mAnnotationToolbar.OnConfigurationChanged(newConfig);
            }
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();

            mSeekBar?.ClearResources();
            mSeekBar = null;

            mPdfViewCtrl?.Destroy();
            mPdfViewCtrl = null;
            mPdfDoc?.Close();
            mPdfDoc = null;
        }
    }
}

