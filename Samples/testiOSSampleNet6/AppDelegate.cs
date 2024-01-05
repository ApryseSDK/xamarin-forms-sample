using pdftron;
using pdftron.PDF.Controls;

namespace testiOSSampleNet6;

[Register ("AppDelegate")]
public class AppDelegate : UIApplicationDelegate {
	public override UIWindow? Window {
		get;
		set;
	}

	public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
	{
		// create a new window instance based on the screen size
		Window = new UIWindow (UIScreen.MainScreen.Bounds);

		// create a UIViewController with a single UILabel
		var vc = new UIViewController ();

        var documentController = new PTDocumentController();
		var navigationController = new UINavigationController(documentController);

		Window.RootViewController = navigationController;
        documentController.ToolManager.UseSystemColorPicker = true;
        documentController.OpenDocumentWithURL(new Uri("https://pdftron.s3.amazonaws.com/downloads/pdfref.pdf"));


        // make the window visible
        Window.MakeKeyAndVisible ();

		return true;
	}
}

