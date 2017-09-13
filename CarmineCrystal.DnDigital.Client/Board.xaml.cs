using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CarmineCrystal.DnDigital.Client
{
	public sealed partial class Board : UserControl
	{
		public Board()
		{
			this.InitializeComponent();
		}

		public event EventHandler<ScrollViewerViewChangingEventArgs> CameraMoving
		{
			add => Scroller.ViewChanging += (sender, args) => value(this, args);
			remove => Scroller.ViewChanging -= (sender, args) => value(this, args);
		}

		public void SetCamera(double? HorizontalOffset, double? VerticalOffset, float? ZoomFactor) => Scroller.ChangeView(HorizontalOffset, VerticalOffset, ZoomFactor);

		public event EventHandler<InkStrokesCollectedEventArgs> LinesAdded
		{
			add => DrawCanvas.InkPresenter.StrokesCollected += (sender, args) => value(this, args);
			remove => DrawCanvas.InkPresenter.StrokesCollected -= (sender, args) => value(this, args);
		}

		public event EventHandler<InkStrokesErasedEventArgs> LinesRemoved
		{
			add => DrawCanvas.InkPresenter.StrokesErased += (sender, args) => value(this, args);
			remove => DrawCanvas.InkPresenter.StrokesErased -= (sender, args) => value(this, args);
		}

		public void AddLine(InkStroke line) => DrawCanvas.InkPresenter.StrokeContainer.AddStroke(line);
		public void AddLines(IEnumerable<InkStroke> lines) => DrawCanvas.InkPresenter.StrokeContainer.AddStrokes(lines);

		public void RemoveLine(uint id)
		{
			DrawCanvas.InkPresenter.StrokeContainer.GetStrokeById(id).Selected = true;
			DrawCanvas.InkPresenter.StrokeContainer.DeleteSelected();
		}

		public void RemoveLines(IEnumerable<uint> ids)
		{
			foreach (uint id in ids)
			{
				DrawCanvas.InkPresenter.StrokeContainer.GetStrokeById(id).Selected = true;
			}

			DrawCanvas.InkPresenter.StrokeContainer.DeleteSelected();
		}
	}
}
