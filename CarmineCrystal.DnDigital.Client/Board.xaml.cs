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
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace CarmineCrystal.DnDigital.Client
{
	public sealed partial class Board : UserControl, INotifyPropertyChanged
	{
		private Mode _BoardMode = Mode.Manipulation;
		public Mode BoardMode
		{
			get => _BoardMode;
			set
			{
				_BoardMode = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(InDrawingMode));
				NotifyPropertyChanged(nameof(InManipulationMode));

				DrawCanvas.InkPresenter.IsInputEnabled = BoardMode == Mode.Drawing;
			}
		}

		public bool InDrawingMode
		{
			get => BoardMode == Mode.Drawing;
			set => BoardMode = value ? Mode.Drawing : Mode.Manipulation;
		}
		public bool InManipulationMode
		{
			get => BoardMode == Mode.Manipulation;
			set => BoardMode = value ? Mode.Manipulation : Mode.Drawing;
		}

		public Datamodels.Point CameraPosition => new Datamodels.Point(Scroller.HorizontalOffset / Scroller.ZoomFactor + Scroller.ActualWidth / 2 / Scroller.ZoomFactor, Scroller.VerticalOffset / Scroller.ZoomFactor + Scroller.ActualHeight / 2 / Scroller.ZoomFactor);
		public double ScrollerHorizontalOffset => Scroller.HorizontalOffset;
		public double ScrollerVerticalOffset => Scroller.VerticalOffset;
		public float ScrollerZoomFactor => Scroller.ZoomFactor;
		public bool ScrollbarsVisible
		{
			get => Scroller.HorizontalScrollBarVisibility != ScrollBarVisibility.Hidden;
			set
			{
				Scroller.HorizontalScrollBarVisibility = value ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
				Scroller.VerticalScrollBarVisibility = value ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
			}
		}

		private List<Character> Characters = new List<Character>();
		private Dictionary<Character, CharacterView> CharacterViews = new Dictionary<Character, CharacterView>();
		public IReadOnlyList<Character> GetCharacters() => Characters.AsReadOnly();

		public Board()
		{
			this.DataContext = this;
			this.InitializeComponent();
		}

		private bool _LockObjects = false;
		public bool LockObjects
		{
			get => _LockObjects;

			set
			{
				_LockObjects = value;

				foreach (CharacterView CV in CharacterViews.Values.Where(x => x.IsObject))
				{
					if (value)
					{
						CV.IsHitTestVisible = false;
						CV.ManipulationDelta -= OnCharacterManipulationDelta;
					}
					else
					{
						CV.IsHitTestVisible = true;
						CV.ManipulationDelta += OnCharacterManipulationDelta;
					}
				}
			}
		}

		public event EventHandler<ScrollViewerViewChangingEventArgs> CameraMoving
		{
			add => Scroller.ViewChanging += (sender, args) => value(this, args);
			remove => Scroller.ViewChanging -= (sender, args) => value(this, args);
		}

		public async void SetCamera(double? HorizontalOffset, double? VerticalOffset, float? ZoomFactor) => await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => Scroller.ChangeView(HorizontalOffset, VerticalOffset, ZoomFactor));

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

		public IEnumerable<string> CharacterNames => Characters.Select(x => x.Name);

		public async void AddCharacter(Character newCharacter)
		{
			Characters.Add(newCharacter);

			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				CharacterView NewView = new CharacterView(newCharacter);
				CharacterViews[newCharacter] = NewView;

				if (NewView.IsObject)
				{
					ArrangementCanvas.Children.Insert(0, NewView);

					if (LockObjects)
					{
						NewView.IsHitTestVisible = false;
					}
					else
					{
						NewView.IsHitTestVisible = true;
						NewView.ManipulationDelta += OnCharacterManipulationDelta;
					}
				}
				else
				{
					ArrangementCanvas.Children.Add(NewView);

					NewView.ManipulationDelta += OnCharacterManipulationDelta;
				}

				MoveCharacter(newCharacter);
			});
		}

		private void OnCharacterManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			int Index = Characters.FindIndex(y => y == CharacterViews.First(x => x.Value == sender).Key);
			Characters[Index].Position = new Datamodels.Point(Characters[Index].Position.X + e.Delta.Translation.X / Scroller.ZoomFactor, Characters[Index].Position.Y + e.Delta.Translation.Y / Scroller.ZoomFactor);
			Characters[Index].Rotation += e.Delta.Rotation;
			MoveCharacter(Characters[Index]);
			CharacterMoved?.Invoke(this, Characters[Index]);
		}

		public event EventHandler<Character> CharacterMoved;

		public async void MoveCharacter(Character character)
		{
			if (character == null)
			{
				return;
			}

			int Index = Characters.FindIndex(x => x?.ID == character?.ID);
			Characters[Index].Position = character.Position;

			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				((CompositeTransform)CharacterViews[Characters[Index]].RenderTransform).Rotation = character.Rotation;
				Canvas.SetLeft(CharacterViews[Characters[Index]], Characters[Index].Position.X);
				Canvas.SetTop(CharacterViews[Characters[Index]], Characters[Index].Position.Y);
			});
		}

		public async void RemoveCharacter(Character oldCharacter)
		{
			Character Char = Characters.First(x => x.ID == oldCharacter.ID);
			Characters.Remove(Char);

			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				CharacterViews[Char].ManipulationDelta -= OnCharacterManipulationDelta;

				ArrangementCanvas.Children.Remove(CharacterViews[Char]);
				CharacterViews.Remove(Char);
			});
		}


		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName]string memberName = null)
		{
			if (memberName != null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
			}
		}

		public InkCanvas GetDrawCanvas() => DrawCanvas;

		public enum Mode
		{
			Drawing,
			Manipulation
		}
	}
}
