using Microsoft.Maui;

namespace samples
{
	public class MainWindow : IWindow
	{
		public MainWindow()
		{
			Page = new MainPage();
		}

		public IPage Page { get; set; }

		public IMauiContext MauiContext { get; set; }
	}
}