using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

[assembly:XamlCompilation(XamlCompilationOptions.Compile)]
namespace CustomRenderer
{
	public class App : Application
	{
		public App ()
		{
			MainPage = new NavigationPage (new CustomRenderer.MainPage ());
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

