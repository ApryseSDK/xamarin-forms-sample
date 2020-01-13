using Android.Content;
using Android.Net;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

using Android.Support.V4.App;

using pdftron.PDF.Config;
using pdftron.PDF.Controls;

namespace CustomRenderer.Droid
{
    public class DocumentView : pdftron.PDF.Controls.DocumentView
    {
        private static string TAG = "DocumentView";

        public event System.EventHandler NavigationButtonPressed;

        private int[] mCustomToolbarRes;

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

        public void OpenDocument(Uri documentUri, string password, ViewerConfig config, FragmentManager manager, int[] customToolbarRes)
        {
            base.SetDocumentUri(documentUri);
            base.SetPassword(password);
            base.SetViewerConfig(config);
            base.SetSupportFragmentManager(manager);

            mCustomToolbarRes = customToolbarRes;
        }

        protected override void BuildViewer()
        {
            base.BuildViewer();
            if (mCustomToolbarRes != null)
            {
                MViewerBuilder.UsingCustomToolbar(mCustomToolbarRes);
            }
        }

        public override void OnNavButtonPressed()
        {
            base.OnNavButtonPressed();

            NavigationButtonPressed?.Invoke(this, new System.EventArgs());
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

        public override bool OnToolbarOptionsItemSelected(IMenuItem menuItem)
        {
            if (menuItem.ItemId == Resource.Id.action_show_toast)
            {
                Toast.MakeText(this.Context, "Show toast is clicked!", ToastLength.Short).Show();
                return true;
            }
            else if (menuItem.ItemId == Resource.Id.action_star)
            {
                Toast.MakeText(this.Context, "Star is clicked!", ToastLength.Short).Show();
                return true;
            }
            return base.OnToolbarOptionsItemSelected(menuItem);
        }
    }
}