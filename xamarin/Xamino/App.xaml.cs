using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamino.Pages;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Xamino
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();

			MainPage = new NavigationPage(new CommandPage())
			{
                BarBackgroundColor = (Color) Application.Current.Resources["Teal"],
                BarTextColor = Color.White
			};
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
