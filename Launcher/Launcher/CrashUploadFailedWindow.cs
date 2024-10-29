using System.Windows;
using System.Windows.Markup;
using ResourceDictionary;

namespace Launcher;

public partial class CrashUploadFailedWindow : Window, IComponentConnector
{
	public CrashUploadFailedWindow()
	{
		InitializeComponent();
		base.FontFamily = FontManager.CurrentFont;
		base.DataContext = this;
	}

	private void OkButtonClick(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
