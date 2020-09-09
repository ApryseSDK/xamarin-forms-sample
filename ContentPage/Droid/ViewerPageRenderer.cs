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
using AndroidX.Fragment.App;
using Color = Android.Graphics.Color;
using pdftron.PDF;
using pdftron.PDF.Annots;
using Page = Xamarin.Forms.Page;

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

            mPdfViewCtrl.DocumentLoad += (sender, e) =>
            {
                SetAnnotationCanvas(sender, e);
            };

            var file = Utils.CopyResourceToLocal(this.Context, Resource.Raw.sixth, "sample", ".pdf");
            mPdfDoc = mPdfViewCtrl.OpenPDFUri(Android.Net.Uri.FromFile(file), "");

            FragmentActivity fragmentActivity = null;
            if (activity is FragmentActivity)
            {
                fragmentActivity = activity as FragmentActivity;
            }
            mToolManager = ToolManagerBuilder.From().Build(fragmentActivity, mPdfViewCtrl);
            mToolManager.SetCanOpenEditToolbarFromPan(true);
            mToolManager.OpenEditToolbar += (sender, e) =>
            {
                mAnnotationToolbar.Show(AnnotationToolbar.StartModeEditToolbar, null, 0, e.Mode, !mAnnotationToolbar.IsShowing);
            };

            mAnnotationToolbar = view.FindViewById<AnnotationToolbar>(Resource.Id.annotationtoolbar);
            mAnnotationToolbar.Setup(mToolManager);
            mAnnotationToolbar.SetButtonStayDown(true);
            mAnnotationToolbar.HideButton(AnnotationToolbarButtonId.Close);
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
                // workaround Xamarin.Forms issue on rotation
                PostDelayed(() => {
                    mAnnotationToolbar.OnConfigurationChanged(newConfig);
                }, 0);
                
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

        private void SetAnnotationCanvas(object sender, System.EventArgs e)
        {
            AddCanvas(1);
            AddCanvas(2);
        }

        private void AddCanvas(int pageNumber)
        {
            var zoom = mPdfViewCtrl.GetZoomForViewMode(mPdfViewCtrl.PageViewMode);
            var firstPage = mPdfDoc.GetPage(pageNumber);
            var cropBox = firstPage.GetCropBox();

            var canvasHeight = (int)(cropBox.Height());
            var canvasWidth = (int)(cropBox.Width());

            var layout = new NativeCustomRelativeLayout(mPdfViewCtrl.Context);
            var color = new Android.Graphics.Color(0, 255, 00, 20);
            layout.SetBackgroundColor(color);
            layout.SetRect(mPdfViewCtrl, new pdftronprivate.PDF.Rect(0, 0, canvasWidth, canvasHeight), pageNumber);
            mPdfViewCtrl.AddView(layout);

            var myButton = new Android.Widget.Button(mPdfViewCtrl.Context);
            myButton.Text = "BUTTON";
            myButton.SetTextColor(Android.Graphics.Color.Pink);
            myButton.Click += CreateTextAnnotation;

            layout.AddView(myButton);
        }

        private void CreateTextAnnotation(object sender, System.EventArgs e)
        {
            mPdfViewCtrl.DocLock(true);

            var editText = new EditText(mPdfViewCtrl.Context);
            editText.Text = "this is a test this is a test this is a test this is a test ";
            editText.TextSize = 12;
            editText.SetSingleLine(false);
            editText.ImeOptions = Android.Views.InputMethods.ImeAction.None;
            editText.LayoutParameters = new ViewGroup.LayoutParams(500, 200);
            editText.SetTextColor(Color.Red);

            // layout view
            editText.Measure(500, 200);
            editText.Layout(0, 0, 500, 200);

            var page = 1;

            var color = Color.BlueViolet;
            var rectBox = new pdftronprivate.PDF.Rect(0, 0, 500, 200);

            var nativeDoc = TypeConvertHelper.ConvPDFDocToNative(mPdfDoc);

            var textAnnot = pdftronprivate.PDF.Annots.FreeText.Create(nativeDoc, rectBox);

            textAnnot.FontSize = 12;
            textAnnot.SetColor(Utils.Color2ColorPt(color), 3);
            textAnnot.Contents = editText.Text;

            nativeDoc.GetPage(page).AnnotPushBack(textAnnot);

            textAnnot.RefreshAppearance();

            AnnotUtils.CreateCustomFreeTextAppearance(editText, mPdfViewCtrl,
                    textAnnot, page,
                    rectBox);

            mPdfViewCtrl.Update(textAnnot, page);

            mPdfViewCtrl.DocUnlock();
        }

        public static ColorPt GetColor(Color color)
        {
            var r = color.R / 256.0;
            var g = color.G / 256.0;
            var b = color.B / 256.0;

            var colorPt = new ColorPt(r, g, b);
            return colorPt;
        }
    }
}

