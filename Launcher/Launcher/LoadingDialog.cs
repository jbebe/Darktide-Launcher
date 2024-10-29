using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using ResourceDictionary;

namespace Launcher;

public partial class LoadingDialog : Window, IComponentConnector
{
	private string _message;

	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			_message = value;
		}
	}

	public LoadingDialog()
	{
		InitializeComponent();
		base.FontFamily = FontManager.CurrentFont;
		base.DataContext = this;
		base.Closing += LoadingDialog_Closing;
	}

	private void LoadingDialog_Closing(object sender, CancelEventArgs e)
	{
		BeginAnimation(UIElement.OpacityProperty, Animations.Fade(0.0, 1.0, 500.0, autoReverse: false));
	}
}
