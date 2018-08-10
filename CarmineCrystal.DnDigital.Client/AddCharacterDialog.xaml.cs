using CarmineCrystal.DnDigital.Datamodels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CarmineCrystal.DnDigital.Client
{
	public sealed partial class AddCharacterDialog : ContentDialog, INotifyPropertyChanged
	{
		public string NameText { get; set; }
		public string VisualsPath { get; set; }
		public bool UseCustomVisuals { get; set; }

		private float _Hue;
		public float Hue
		{
			get => _Hue;
			set
			{
				_Hue = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(SelectedColor));
				NotifyPropertyChanged(nameof(SelectedColorBrush));
			}
		}
		private float _Saturation = 1;
		public float Saturation
		{
			get => _Saturation;
			set
			{
				_Saturation = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(SelectedColor));
				NotifyPropertyChanged(nameof(SelectedColorBrush));
			}
		}
		private float _Value = 1;
		public float Value
		{
			get => _Value;
			set
			{
				_Value = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(SelectedColor));
				NotifyPropertyChanged(nameof(SelectedColorBrush));
			}
		}

		public Color SelectedColor => Color.FromHSV(Hue, Saturation, Value);

		public string WidthText { get; private set; } = "1";
		public double CharacterWidth
		{
			get
			{
				if (double.TryParse(WidthText, out double Result))
				{
					return Math.Max(Result, 0.5);
				}

				return 1;
			}
		}

		public string HeightText { get; private set; } = "1";
		public double CharacterHeight
		{
			get
			{
				if (double.TryParse(HeightText, out double Result))
				{
					return Math.Max(Result, 0.5);
				}

				return 1;
			}
		}

		public SolidColorBrush SelectedColorBrush => new SolidColorBrush(SelectedColor.ToWindowsColor());

		public AddCharacterDialog()
		{
			string[] VisualsPaths = Directory.GetFiles("Assets", "*", SearchOption.AllDirectories).Where(x => Path.GetDirectoryName(x) != "Assets").ToArray();
			this.DataContext = this;
			this.InitializeComponent();

			VisualsList.ItemsSource = VisualsPaths;
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{

		}

		private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{

		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName]string memberName = null)
		{
			if (memberName != null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
			}
		}
	}
}
