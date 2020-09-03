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
using System.Collections.Generic;
using Android.Graphics.Drawables;

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

        // bookmarks
        private int mBookmarkDialogCurrentTab = 0;

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
            mSeekBar.MenuItemClicked += (sender, e) =>
            {
                if (e.MenuItemPosition == ThumbnailSlider.PositionLeft)
                {
                    openThumbnailsDialog();
                }
                else if (e.MenuItemPosition == ThumbnailSlider.PositionRight)
                {
                    openBookmarksDialog(mBookmarkDialogCurrentTab);
                }
            };
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

        private void openThumbnailsDialog()
        {
            var thumbDialog = ThumbnailsViewFragment.NewInstance().SetPdfViewCtrl(mPdfViewCtrl);
            thumbDialog.ExportThumbnails += (sender, e) =>
            {
                Console.WriteLine("ExportThumbnails");
            };
            thumbDialog.ThumbnailsViewDialogDismiss += (sender, e) =>
            {
                Console.WriteLine("ThumbnailsViewDialogDismiss");
            };
            thumbDialog.SetStyle((int)DialogFragmentStyle.NoTitle, Resource.Style.CustomAppTheme);
            FragmentActivity fragmentActivity = null;
            if (activity is FragmentActivity)
            {
                fragmentActivity = activity as FragmentActivity;
            }
            thumbDialog.Show(fragmentActivity.SupportFragmentManager, "thumbnails_dialog");
        }

        private void openBookmarksDialog(int which)
        {
            var bookmarksDialog = pdftron.PDF.Dialog.BookmarksDialogFragment.NewInstance();
            bookmarksDialog.SetPdfViewCtrl(mPdfViewCtrl);
            List<DialogFragmentTab> tabs = new List<DialogFragmentTab>();
            var annotationsTab = new DialogFragmentTab(
                Java.Lang.Class.FromType(typeof(AnnotationDialogFragment)), BookmarksTabLayout.TagTabAnnotation, GetDrawable(Resource.Drawable.ic_annotations_white_24dp), null, "Annotations", null, Resource.Menu.fragment_annotlist_sort);
            var outlineDialog = new DialogFragmentTab(
                Java.Lang.Class.FromType(typeof(OutlineDialogFragment)), BookmarksTabLayout.TagTabOutline, GetDrawable(Resource.Drawable.ic_outline_white_24dp), null, "Outline", null);
            var userBookmarksDialog = new DialogFragmentTab(
                Java.Lang.Class.FromType(typeof(UserBookmarkDialogFragment)), BookmarksTabLayout.TagTabBookmark, GetDrawable(Resource.Drawable.ic_bookmarks_white_24dp), null, "Bookmarks", null);
            tabs.Add(annotationsTab);
            tabs.Add(outlineDialog);
            tabs.Add(userBookmarksDialog);
            bookmarksDialog.SetDialogFragmentTabs(tabs, which);
            bookmarksDialog.SetStyle((int)DialogFragmentStyle.NoTitle, Resource.Style.CustomAppTheme);
            FragmentActivity fragmentActivity = null;
            if (activity is FragmentActivity)
            {
                fragmentActivity = activity as FragmentActivity;
            }
            bookmarksDialog.Show(fragmentActivity.SupportFragmentManager, "bookmarks_dialog");
            bookmarksDialog.AnnotationClicked += (sender, e) =>
            {
                bookmarksDialog.Dismiss();
            };
            bookmarksDialog.ExportAnnotations += (sender, e) =>
            {
                bookmarksDialog.Dismiss();
            };
            bookmarksDialog.OutlineClicked += (sender, e) =>
            {
                bookmarksDialog.Dismiss();
            };
            bookmarksDialog.UserBookmarkClick += (sender, e) =>
            {
                mPdfViewCtrl.SetCurrentPage(e.PageNum);
                bookmarksDialog.Dismiss();
            };
            bookmarksDialog.BookmarksDialogWillDismiss += (sender, e) =>
            {
                bookmarksDialog.Dismiss();
            };
            bookmarksDialog.BookmarksDialogDismissed += (sender, e) =>
            {
                mBookmarkDialogCurrentTab = e.TabIndex;
            };
        }

        private Drawable GetDrawable(int res)
        {
            return pdftron.PDF.Tools.Utils.Utils.GetDrawable(activity, res);
        }
    }
}

