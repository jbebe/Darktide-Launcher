using System;
using System.Windows;
using System.Windows.Markup;
using ResourceDictionary;
using ResourceDictionary.Properties;

namespace Launcher;

public partial class QualityDialog : Window, IComponentConnector
{
	public QualityDialog(GameSettingsHolder settingsHolder, SysInfo.DetectQualityResult result)
	{
		InitializeComponent();
		base.Activated += QualityDialog_Activated;
		base.FontFamily = FontManager.CurrentFont;
		ButtonOK.Click += close_dialog;
		base.DataContext = this;
		string lOC_Quality_Settings_Profile_Not_Found = ResourceDictionary.Properties.Resources.LOC_Quality_Settings_Profile_Not_Found;
		Title_1.Text = ResourceDictionary.Properties.Resources.LOC_Quality_Settings_Hardware_Detected;
		bool flag = result.cpu_mark >= 0 && result.cpu_mark < SysInfo.MIN_REQ_CPU_MARK;
		Body_1_0_0.Text = ResourceDictionary.Properties.Resources.LOC_Quality_Settings_CPU + ":";
		Body_1_1_0.Text = ((result.cpu_quality != 0) ? result.CPU : lOC_Quality_Settings_Profile_Not_Found);
		Body_1_1_0.Tag = result.cpu_quality == GameSettingsHolder.QualityType.None || flag;
		bool flag2 = result.gpu_mark >= 0 && result.gpu_mark < SysInfo.MIN_REQ_GPU_MARK;
		Body_1_0_1.Text = ResourceDictionary.Properties.Resources.LOC_Quality_Settings_GPU + ":";
		Body_1_1_1.Text = ((result.gpu_quality != 0) ? result.GPU : lOC_Quality_Settings_Profile_Not_Found);
		Body_1_1_1.Tag = result.gpu_quality == GameSettingsHolder.QualityType.None || flag2;
		Title_2.Text = ResourceDictionary.Properties.Resources.LOC_Quality_Settings_Assigned_Settings;
		Body_2_0_0.Text = ResourceDictionary.Properties.Resources.LOC_Quality_Settings_Graphic_Preset + ":";
		string text = result.preferred_quality.ToString();
		string name = $"LOC_{char.ToUpper(text[0])}{text.Substring(1)}";
		Body_2_0_1.Text = ResourceDictionary.Properties.Resources.ResourceManager.GetString(name);
		if (result.DLSSMode == GameSettingsHolder.DLSSMode.on)
		{
			Body_2_1_0.Text = ResourceDictionary.Properties.Resources.loc_setting_nvidia_dlss + ":";
			string name2 = $"LOC_setting_dlss_quality_{settingsHolder.CurrentDLSSMode.ToString()}";
			Body_2_1_1.Text = ResourceDictionary.Properties.Resources.ResourceManager.GetString(name2);
			if (settingsHolder.CurrentOutput.Features.SupportsRaytracing)
			{
				Body_2_2_0.Text = ResourceDictionary.Properties.Resources.LOC_ray_tracing + ":";
				string text2 = settingsHolder.CurrentRayTracingMode.ToString();
				string name3 = $"LOC_{char.ToUpper(text2[0])}{text2.Substring(1)}";
				Body_2_2_1.Text = ResourceDictionary.Properties.Resources.ResourceManager.GetString(name3);
			}
			else
			{
				Body_2_2_0.Height = 0.0;
				Body_2_2_1.Height = 0.0;
			}
		}
		else if (result.FSRMode != 0)
		{
			Body_2_1_0.Text = ResourceDictionary.Properties.Resources.loc_setting_fsr + ":";
			string name4 = $"LOC_setting_fsr_quality_{settingsHolder.CurrentFSRMode.ToString()}";
			Body_2_1_1.Text = ResourceDictionary.Properties.Resources.ResourceManager.GetString(name4);
			Body_2_2_0.Text = ResourceDictionary.Properties.Resources.LOC_setting_anti_ailiasing + ":";
			string name5 = $"LOC_setting_anti_ailiasing_{settingsHolder.CurrentAntiAliasingMode}";
			Body_2_2_1.Text = ResourceDictionary.Properties.Resources.ResourceManager.GetString(name5);
		}
		else if (result.XeSSMode != 0)
		{
			Body_2_1_0.Text = ResourceDictionary.Properties.Resources.loc_setting_xess + ":";
			string name6 = $"LOC_setting_fsr_quality_{settingsHolder.CurrentXeSSMode.ToString()}";
			Body_2_1_1.Text = ResourceDictionary.Properties.Resources.ResourceManager.GetString(name6);
			Body_2_2_0.Height = 0.0;
			Body_2_2_1.Height = 0.0;
		}
		if (result.cpu_quality == GameSettingsHolder.QualityType.None || result.gpu_quality == GameSettingsHolder.QualityType.None)
		{
			Body_3.Text = ResourceDictionary.Properties.Resources.LOC_Quality_Settings_Hardware_Not_Found;
		}
		else if (result.cpu_quality < result.gpu_quality)
		{
			Body_3.Text = ResourceDictionary.Properties.Resources.LOC_Quality_Settings_Note_2;
		}
		else
		{
			Body_3.Height = 0.0;
		}
		Title_3.Text = ResourceDictionary.Properties.Resources.LOC_Quality_Settings_Note_1;
		if (flag || flag2)
		{
			BelowMinimumRequirementsStackPanel.Visibility = Visibility.Visible;
			BelowMinimumRequirementsTitle.Text = ResourceDictionary.Properties.Resources.LOC_below_minimum_requirements;
		}
		else
		{
			MinimumRequirementsFulfilledStackPanel.Visibility = Visibility.Visible;
			MinimumRequirementsFulfilledTitle.Text = ResourceDictionary.Properties.Resources.LOC_minimum_requirements_fulfilled;
		}
	}

	private void QualityDialog_Activated(object sender, EventArgs e)
	{
		if (!base.IsLoaded)
		{
			BeginAnimation(UIElement.OpacityProperty, Animations.Fade(0.0, 1.0, 500.0, autoReverse: false));
		}
	}

	protected void close_dialog(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
