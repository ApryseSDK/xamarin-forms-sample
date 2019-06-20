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

using FragmentActivity = Android.Support.V4.App.FragmentActivity;
using FragmentManager = Android.Support.V4.App.FragmentManager;

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
            // var file = Utils.CopyResourceToLocal(this.Context, Resource.Raw.sample, "sample", ".pdf");
            return Android.Net.Uri.Parse("https://s3.amazonaws.com/pdftron/files/pdf/linearization/5mb-linearized.pdf");
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
                // Use local cache for open URL. If using external storage, 
                // permissions will have to be handled manually
                .OpenUrlCachePath(this.Context.CacheDir.AbsolutePath)
                // Additionally, you can restrict download usage to only 
                // download parts of a linearized PDF via URL
                // .RestrictDownloadUsage(true)
                .Build();
            return config;
        }
    }
}

