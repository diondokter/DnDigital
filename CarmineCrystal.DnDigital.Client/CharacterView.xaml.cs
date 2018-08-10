using CarmineCrystal.DnDigital.Datamodels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CarmineCrystal.DnDigital.Client
{
	public sealed partial class CharacterView : UserControl
	{
		private Character Model { get; set; }

		public SolidColorBrush CharacterColor => new SolidColorBrush(Model.Color.ToWindowsColor());
		public string CharacterName => Model.Name;
		public bool HasName => CharacterName != null;
		public string CharacterInitial => HasName ? CharacterName[0].ToString() : null;
		public string CharacterVisuals => Model.Visuals;
		public Visibility CustomVisuals => CharacterVisuals != null ? Visibility.Visible : Visibility.Collapsed;
		public Visibility NameBoxVisuals => CharacterVisuals != null && HasName ? Visibility.Visible : Visibility.Collapsed;
		public Visibility DefaultVisuals => CharacterVisuals == null ? Visibility.Visible : Visibility.Collapsed;
		public bool IsObject => !string.IsNullOrEmpty(CharacterVisuals) && CharacterVisuals.Contains("Objects");

		public CharacterView(Character target)
		{
			Model = target;
			this.DataContext = this;
			this.InitializeComponent();

			CompositeTransform transform = new CompositeTransform();
			RenderTransform = transform;

			transform.CenterX = 50;
			transform.CenterY = 50;

			transform.TranslateX = -50;
			transform.TranslateY = -50;

			SetScale(target, transform);

			ManipulationMode = ManipulationModes.TranslateY | ManipulationModes.TranslateX | ManipulationModes.Rotate;
		}

		public async void SetScale(Character target, CompositeTransform transform)
		{
			if (IsObject)
			{
				StorageFile SF = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(CharacterVisuals));
				ImageProperties ImProp = await SF.Properties.GetImagePropertiesAsync();

				double Size = Math.Max(ImProp.Width / 20.0, ImProp.Height / 20.0);

				string FileName = Path.GetFileNameWithoutExtension(CharacterVisuals);
				string SizeModifier = FileName.Split('-').Last();
				if (int.TryParse(SizeModifier, out int Modifier))
				{
					Size = Math.Max(ImProp.Width / (double)Modifier, ImProp.Height / (double)Modifier);
				}

				target.Size = new Datamodels.Rect(new Datamodels.Point(), Size, Size);
			}

			transform.ScaleX = target.Size.Width / 10;
			transform.ScaleY = target.Size.Height / 10;
		}
	}
}
