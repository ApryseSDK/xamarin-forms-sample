using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

using pdftron;
using System;

namespace CustomRenderer.Droid
{
    [Activity(Label = "CustomRenderer.Droid", 
        Icon = "@drawable/icon", MainLauncher = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden,
        WindowSoftInputMode = SoftInput.AdjustPan,
        Theme = "@style/MainTheme")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Instance = this;
            global::Xamarin.Forms.Forms.Init(this, bundle);

            try
            {
                pdftron.PDF.Tools.Utils.AppUtils.InitializePDFNetApplication(this);
                Console.WriteLine(PDFNet.GetVersion());
            }
            catch (pdftron.Common.PDFNetException e)
            {
                Console.WriteLine(e.GetMessage());
                return;
            }
            
            LoadApplication(new App());
        }
    }
}

