using Android.Content;
using Android.Net;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;

using pdftron.PDF.Config;
using pdftron.PDF.Controls;

namespace CustomRenderer.Droid
{
    public class DocumentView : pdftron.PDF.Controls.DocumentView
    {
        private static string TAG = "DocumentView";

        public event System.EventHandler NavigationButtonPressed;

        public DocumentView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public DocumentView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
        }

        public void OpenDocument(Uri documentUri, string password, ViewerConfig config, FragmentManager manager)
        {
            base.SetDocumentUri(documentUri);
            base.SetPassword(password);
            base.SetViewerConfig(config);
            base.SetSupportFragmentManager(manager);
        }

        public override void OnNavButtonPressed()
        {
            base.OnNavButtonPressed();

            NavigationButtonPressed?.Invoke(this, new System.EventArgs());
        }

        public override void OnTabDocumentLoaded(string tag)
        {
            base.OnTabDocumentLoaded(tag);
        }

        public override bool CanShowFileInFolder()
        {
            return false;
        }

        public override bool CanShowFileCloseSnackbar()
        {
            return false;
        }

        public override bool CanRecreateActivity()
        {
            return false;
        }

        public override void OnLastTabClosed()
        {
            base.OnLastTabClosed();

            NavigationButtonPressed?.Invoke(this, new System.EventArgs());
        }

        public override bool OnToolbarOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_share)
            {
                PrintPdfBookmarks();
            }
            return base.OnToolbarOptionsItemSelected(item);
        }


        public void PrintPdfBookmarks()
        {
            var pdfdoc = GetPDFViewCtrl().InnerDoc;
            var bookmark = pdftron.PDF.Tools.Utils.BookmarkManager.GetRootPdfBookmark(pdfdoc, false);
            var bookmarks = pdftron.PDF.Tools.Utils.BookmarkManager.GetPdfBookmarks(bookmark);
            foreach(var bm in bookmarks)
            {
                System.Console.WriteLine("title: " + bm.Title);
            }
        }

        private pdftronprivate.PDF.PDFViewCtrl GetPDFViewCtrl()
        {
            if (MPdfViewCtrlTabHostFragment != null && MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment != null)
            {
                return MPdfViewCtrlTabHostFragment.CurrentPdfViewCtrlFragment.PDFViewCtrl;
            }
            return null;
        }
    }
}