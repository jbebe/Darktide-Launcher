using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using ResourceDictionary;

namespace Launcher;

public partial class DisableRaytracingMessageWindow : Window, IComponentConnector
{
	public DialogResult Result { get; set; }

	public DisableRaytracingMessageWindow()
	{
		InitializeComponent();
		base.Activated += DisableRaytracingMessageWindow_Activated;
		base.FontFamily = FontManager.CurrentFont;
		base.DataContext = this;
	}

	private void DisableRaytracingMessageWindow_Activated(object sender, EventArgs e)
	{
		if (!base.IsLoaded)
		{
			BeginAnimation(UIElement.OpacityProperty, Animations.Fade(0.0, 1.0, 500.0, autoReverse: false));
		}
	}

	private void LeaveOnButtonClick(object sender, RoutedEventArgs e)
	{
		Result = System.Windows.Forms.DialogResult.No;
		Close();
	}

	private void TurnOffButtonClick(object sender, RoutedEventArgs e)
	{
		Result = System.Windows.Forms.DialogResult.Yes;
		Close();
	}
}
