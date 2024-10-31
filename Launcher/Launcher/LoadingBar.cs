using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using ResourceDictionary;

namespace Launcher;

public partial class LoadingBar : Window, IComponentConnector
{
	private bool _isShowing;

	public double Value
	{
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				BarScale.ScaleX = value;
				SliderProgressBar.Value = value;
			});
		}
	}

	public string ProgressText
	{
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				TextProgress.Text = value;
			});
		}
	}

	public string Label
	{
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				TextTitle.Text = value;
			});
		}
	}

	public new virtual void Show()
	{
		if (!_isShowing)
		{
			_isShowing = true;
			base.Dispatcher.Invoke(delegate
			{
				base.Show();
			});
		}
	}

	public LoadingBar()
	{
		InitializeComponent();
		base.FontFamily = FontManager.CurrentFont;
		base.DataContext = this;
	}

	private void LoadingBar_OnLoaded(object sender, RoutedEventArgs e)
	{
		if (!(TextTitle.ActualWidth < canvas.ActualWidth))
		{
			DoubleAnimation doubleAnimation = new DoubleAnimation();
			Point point = TextTitle.TransformToAncestor(canvas).Transform(new Point(0.0, 0.0));
			doubleAnimation.From = point.X;
			doubleAnimation.To = canvas.ActualWidth - point.X - TextTitle.ActualWidth;
			doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
			doubleAnimation.AutoReverse = true;
			doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(TextTitle.ActualWidth / 50.0));
			TextTitle.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
		}
	}

    private void ButtonClose_Click(object sender, RoutedEventArgs e) => Close();
}
