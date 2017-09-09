using CarmineCrystal.DnDigital.Datamodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace CarmineCrystal.DnDigital.Client
{
	/// <summary>
	/// AlignmentGrid is used to display a grid to help aligning controls
	/// Based on https://github.com/Microsoft/UWPCommunityToolkit/blob/master/Microsoft.Toolkit.Uwp.DeveloperTools/AlignmentGrid/AlignmentGrid.cs
	/// </summary>
	public class AlignmentGrid : ContentControl
	{
		/// <summary>
		/// Identifies the <see cref="LineBrush"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(AlignmentGrid), new PropertyMetadata(null, OnPropertyChanged));

		/// <summary>
		/// Identifies the <see cref="StepSize"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty StepSizeProperty = DependencyProperty.Register(nameof(StepSize), typeof(double), typeof(AlignmentGrid), new PropertyMetadata(20.0, OnPropertyChanged));

		/// <summary>
		/// Identifies the <see cref="HorizontalStep"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(nameof(Offset), typeof(double), typeof(AlignmentGrid), new PropertyMetadata(new Point(), OnPropertyChanged));

		private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			AlignmentGrid alignmentGrid = dependencyObject as AlignmentGrid;

			alignmentGrid?.Rebuild();
		}

		private readonly Canvas ContainerCanvas = new Canvas();

		/// <summary>
		/// Gets or sets the step to use horizontally.
		/// </summary>
		public Brush LineBrush
		{
			get => (Brush)GetValue(LineBrushProperty);
			set => SetValue(LineBrushProperty, value);
		}

		/// <summary>
		/// Gets or sets the step to use horizontally.
		/// </summary>
		public double StepSize
		{
			get => (double)GetValue(StepSizeProperty);
			set => SetValue(StepSizeProperty, value);
		}

		/// <summary>
		/// Gets or sets the step to use horizontally.
		/// </summary>
		public Point Offset
		{
			get => (Point)GetValue(OffsetProperty);
			set => SetValue(OffsetProperty, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AlignmentGrid"/> class.
		/// </summary>
		public AlignmentGrid()
		{
			SizeChanged += AlignmentGrid_SizeChanged;

			IsHitTestVisible = false;

			Opacity = 0.5;

			HorizontalContentAlignment = HorizontalAlignment.Stretch;
			VerticalContentAlignment = VerticalAlignment.Stretch;
			Content = ContainerCanvas;
		}

		private void Rebuild()
		{
			ContainerCanvas.Children.Clear();
			double horizontalStep = StepSize;
			double verticalStep = StepSize;
			Brush brush = LineBrush ?? (Brush)Application.Current.Resources["ApplicationForegroundThemeBrush"];

			for (double x = Offset.X; x < ActualWidth; x += horizontalStep)
			{
				Rectangle line = new Rectangle
				{
					Width = 1,
					Height = ActualHeight,
					Fill = brush
				};
				Canvas.SetLeft(line, x);

				ContainerCanvas.Children.Add(line);
			}

			for (double y = Offset.Y; y < ActualHeight; y += verticalStep)
			{
				Rectangle line = new Rectangle
				{
					Width = ActualWidth,
					Height = 1,
					Fill = brush
				};
				Canvas.SetTop(line, y);

				ContainerCanvas.Children.Add(line);
			}
		}

		private void AlignmentGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Rebuild();
		}
	}
}