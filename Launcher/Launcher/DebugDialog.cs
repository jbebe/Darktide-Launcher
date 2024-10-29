using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Launcher;

public partial class DebugDialog : Window, IComponentConnector
{
	public static string DebugText;

	public DebugDialog(string content)
	{
		InitializeComponent();
		DebugOutput.Text = content;
		TextBox debugOutput = DebugOutput;
		debugOutput.Text = debugOutput.Text + "\n\n" + DebugText;
		DismissButton.Click += DismissButton_click;
	}

	public void Show(Window owner)
	{
		Show();
	}

	protected void DismissButton_click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
