using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

namespace CustomRenderer.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : Microsoft.Maui.MauiUIApplicationDelegate
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();

            pdftron.PDFNet.Initialize("");
            Console.WriteLine(pdftron.PDFNet.GetVersion());

            LoadApplication (new App ());

			return base.FinishedLaunching (app, options);
		}
	}
}

