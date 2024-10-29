using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;

namespace Launcher;

public partial class ConfirmationBox : Window, IComponentConnector
{
	public static readonly DependencyProperty WindowWidthValueProperty = DependencyProperty.Register("WindowWidthValue", typeof(double), typeof(ConfirmationBox));

	public static readonly DependencyProperty WindowHeightValueProperty = DependencyProperty.Register("WindowHeightValue", typeof(double), typeof(ConfirmationBox));

	public string BodyText { get; private set; }

	public string TitleText { get; private set; }

	public string LeftButtonText { get; private set; }

	public string RightButtonText { get; private set; }

	public DialogResult LeftButtonResult { get; private set; }

	public DialogResult RightButtonResult { get; private set; }

	public bool ShowTitle { get; private set; }

	public DialogResult Result { get; private set; }

	public string BackgroundImage { get; private set; }

	public double BodyHeight { get; private set; }

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

	public ConfirmationBox(string bodyText, string titleText = "", string leftButtonText = "Yes", string rightButtonText = "No", DialogResult leftButtonResult = System.Windows.Forms.DialogResult.Yes, DialogResult rightButtonResult = System.Windows.Forms.DialogResult.No, double windowWidth = 480.0, double windowHeight = 450.0, double bodyHeight = 250.0, string backgroundImagePath = "/ResourceDictionary;component/assets/common/popup_background_480x450.png")
	{
		ConfirmationBox confirmationBox = this;
		BodyText = bodyText;
		LeftButtonText = leftButtonText;
		RightButtonText = rightButtonText;
		LeftButtonResult = leftButtonResult;
		RightButtonResult = rightButtonResult;
		TitleText = titleText;
		ShowTitle = !string.IsNullOrEmpty(TitleText);
		WindowWidthValue = windowWidth;
		WindowHeightValue = windowHeight;
		BodyHeight = bodyHeight;
		BackgroundImage = backgroundImagePath;
		InitializeComponent();
		base.Activated += ConfirmationBox_Activated;
		base.DataContext = this;
		base.Loaded += delegate
		{
			if (confirmationBox.Owner != null)
			{
				ScreenHandler.DoCenterTop(confirmationBox, new Rectangle
				{
					Width = (int)windowWidth,
					Height = (int)windowHeight
				}, confirmationBox.Owner);
			}
		};
	}

	private void ConfirmationBox_Activated(object sender, EventArgs e)
	{
		if (!base.IsLoaded)
		{
			BeginAnimation(UIElement.OpacityProperty, Animations.Fade(0.0, 1.0, 500.0, autoReverse: false));
		}
	}

	private void LeftButton_Click(object sender, RoutedEventArgs e)
	{
		Result = LeftButtonResult;
		Close();
	}

	private void RightButton_Click(object sender, RoutedEventArgs e)
	{
		Result = RightButtonResult;
		Close();
	}
}
