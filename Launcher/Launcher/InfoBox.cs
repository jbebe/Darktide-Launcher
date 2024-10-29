using System;
using System.Drawing;
using System.Windows;
using System.Windows.Markup;

namespace Launcher;

public partial class InfoBox : Window, IComponentConnector
{
	public static readonly DependencyProperty WindowWidthValueProperty = DependencyProperty.Register("WindowWidthValue", typeof(double), typeof(InfoBox));

	public static readonly DependencyProperty WindowHeightValueProperty = DependencyProperty.Register("WindowHeightValue", typeof(double), typeof(InfoBox));

	public string BodyText { get; private set; }

	public string ButtonText { get; private set; }

	public string TitleText { get; private set; }

	public bool ShowTitle { get; private set; }

	public double BodyHeight { get; private set; }

	public string BackgroundImage { get; private set; }

	public double WindowWidthValue
	{
		get
		{
			return (double)GetValue(WindowWidthValueProperty);
		}
		set
		{
			SetValue(WindowWidthValueProperty, value);
		}
	}

	public double WindowHeightValue
	{
		get
		{
			return (double)GetValue(WindowHeightValueProperty);
		}
		set
		{
			SetValue(WindowHeightValueProperty, value);
		}
	}

	public InfoBox(string bodyText, string titleText = "", string buttonText = "Ok", double windowWidth = 400.0, double windowHeight = 360.0, double bodyHeight = 190.0, string backgroundImagePath = "/ResourceDictionary;component/assets/common/popup_background_400x360.png")
	{
		InfoBox infoBox = this;
		BodyText = bodyText;
		ButtonText = buttonText;
		TitleText = titleText;
		WindowWidthValue = windowWidth;
		WindowHeightValue = windowHeight;
		BodyHeight = bodyHeight;
		BackgroundImage = backgroundImagePath;
		ShowTitle = !string.IsNullOrEmpty(TitleText);
		InitializeComponent();
		base.Activated += InfoBox_Activated;
		base.DataContext = this;
		base.Loaded += delegate
		{
			if (infoBox.Owner != null)
			{
				ScreenHandler.DoCenterTop(infoBox, new Rectangle
				{
					Width = (int)windowWidth,
					Height = (int)windowHeight
				}, infoBox.Owner);
			}
		};
	}

	private void InfoBox_Activated(object sender, EventArgs e)
	{
		if (!base.IsLoaded)
		{
			BeginAnimation(UIElement.OpacityProperty, Animations.Fade(0.0, 1.0, 500.0, autoReverse: false));
		}
	}

	private void ConfirmationButton_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
