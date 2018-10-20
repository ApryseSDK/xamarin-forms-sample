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
    public class DocumentView : FrameLayout
    {
        private static string TAG = "DocumentView";
        private PdfViewCtrlTabHostFragment mPdfViewCtrlTabHostFragment;
        private FragmentManager mFragmentManager;

        private int mNavIconRes = Resource.Drawable.ic_arrow_back_white_24dp;
        private bool mShowNavIcon = true;
        private Uri mDocumentUri;
        private string mPassword = "";
        private ViewerConfig mViewerConfig;

        public event System.EventHandler OnNavButtonPressed;

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
            mDocumentUri = documentUri;
            mPassword = password;
            mViewerConfig = config;

            mFragmentManager = manager;
        }

        public void SetNavIconRes(int resId)
        {
            mNavIconRes = resId;
        }

        public void SetShowNavIcon(bool showNavIcon)
        {
            mShowNavIcon = showNavIcon;
        }

        public void PrepView()
        {
            Bundle args = PdfViewCtrlTabFragment.CreateBasicPdfViewCtrlTabBundle(this.Context, mDocumentUri, mPassword, mViewerConfig);
            args.PutParcelable(PdfViewCtrlTabHostFragment.BundleTabHostConfig, mViewerConfig);
            args.PutInt(PdfViewCtrlTabHostFragment.BundleTabHostNavIcon, mShowNavIcon ? mNavIconRes : 0);

            if (mPdfViewCtrlTabHostFragment != null)
            {
                mPdfViewCtrlTabHostFragment.OnOpenAddNewTab(args);
                return;
            }
            mPdfViewCtrlTabHostFragment = PdfViewCtrlTabHostFragment.NewInstance(args);

            if (mFragmentManager != null)
            {
                mFragmentManager.BeginTransaction()
                    .Add(mPdfViewCtrlTabHostFragment, TAG)
                    .CommitNow();

                View fragmentView = mPdfViewCtrlTabHostFragment.View;
                if (fragmentView != null)
                {
                    AddView(fragmentView, LayoutParams.MatchParent, LayoutParams.MatchParent);
                }
            }
        }

        public void Cleanup()
        {
            if (mFragmentManager != null)
            {
                PdfViewCtrlTabHostFragment fragment = (PdfViewCtrlTabHostFragment)mFragmentManager.FindFragmentByTag(TAG);
                if (fragment != null)
                {
                    mFragmentManager.BeginTransaction()
                        .Remove(fragment)
                        .CommitAllowingStateLoss();
                }
            }
            mPdfViewCtrlTabHostFragment = null;
            mFragmentManager = null;
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            PrepView();

            if (mPdfViewCtrlTabHostFragment != null)
            {
                mPdfViewCtrlTabHostFragment.CanShowFileInFolder += PdfViewCtrlTabHostFragment_CanShowFileInFolder;
                mPdfViewCtrlTabHostFragment.CanRecreateActivityEvent += PdfViewCtrlTabHostFragment_CanRecreateActivityEvent;
                mPdfViewCtrlTabHostFragment.NavButtonPressed += HandleNavButtonPressed;
                mPdfViewCtrlTabHostFragment.LastTabClosed += HandleNavButtonPressed;
            }
        }

        private void HandleNavButtonPressed(object sender, System.EventArgs e)
        {
            OnNavButtonPressed?.Invoke(sender, e);
        }

        private void PdfViewCtrlTabHostFragment_CanRecreateActivityEvent(object sender, PdfViewCtrlTabHostFragment.CanRecreateActivityEventArgs e)
        {
            e.Handled = false;
        }

        private void PdfViewCtrlTabHostFragment_CanShowFileInFolder(object sender, PdfViewCtrlTabHostFragment.CanShowFileInFolderEventArgs e)
        {
            e.Handled = false;
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();

            if (mPdfViewCtrlTabHostFragment != null)
            {
                mPdfViewCtrlTabHostFragment.CanShowFileInFolder -= PdfViewCtrlTabHostFragment_CanShowFileInFolder;
                mPdfViewCtrlTabHostFragment.CanRecreateActivityEvent -= PdfViewCtrlTabHostFragment_CanRecreateActivityEvent;
            }

            Cleanup();
        }
    }
}