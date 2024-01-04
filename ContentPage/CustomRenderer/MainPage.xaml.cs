using System;
using Xamarin.Forms;

namespace CustomRenderer
{
	public partial class MainPage : ContentPage
	{
		public MainPage ()
		{
			InitializeComponent ();
		}

		async void OnOpenViewerButtonClicked(object sender, EventArgs e)
		{
            await Navigation.PushAsync(new ViewerPage());
        }

        async void OnOpenAdvancedViewerButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AdvancedViewerPage());
        }
    }
}

