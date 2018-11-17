using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using CarmineCrystal.Networking;
using CarmineCrystal.DnDigital.Messages;
using System.Reflection;
using CarmineCrystal.DnDigital.Datamodels;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace CarmineCrystal.DnDigital.Client
{
	public sealed partial class MasterPage : Page, INotifyPropertyChanged
	{
		public bool InDrawingMode
		{
			get => PlayBoard.BoardMode == Board.Mode.Drawing;
			set
			{
				PlayBoard.BoardMode = value ? Board.Mode.Drawing : Board.Mode.Manipulation;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(DrawingModeVisibility));
				NotifyPropertyChanged(nameof(ManipulationModeVisibility));
			}
		}

		public bool ObjectLock
		{
			get => PlayBoard.LockObjects;
			set => PlayBoard.LockObjects = value;
		}

		public Visibility DrawingModeVisibility => InDrawingMode ? Visibility.Visible : Visibility.Collapsed;
		public Visibility ManipulationModeVisibility => !InDrawingMode ? Visibility.Visible : Visibility.Collapsed;

		public MasterPage()
		{
			this.DataContext = this;
			this.InitializeComponent();

			Message.Initialize(typeof(AddCharacterMessage).GetTypeInfo().Assembly);
			NetworkServer.Start(5000);
			NetworkServer.ClientAdded += OnNetworkServerClientAddedAsync;
		}

		private async void OnNetworkServerClientAddedAsync(NetworkClient target)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				try
				{
					MemoryStream Buffer = new MemoryStream();
					await PlayBoard.GetDrawCanvas().InkPresenter.StrokeContainer.SaveAsync(Buffer.AsOutputStream());

					target.Send(new BoardLinesMessage() { Data = Buffer.ToArray() });

					foreach (Character c in PlayBoard.GetCharacters())
					{
						target.Send(new AddCharacterMessage() { NewCharacter = c });
					}

					target.Send(new CameraMovedMessage() { HorizontalOffset = PlayBoard.ScrollerHorizontalOffset, VerticalOffset = PlayBoard.ScrollerVerticalOffset, ZoomFactor = PlayBoard.ScrollerZoomFactor });
				}
				catch (Exception e)
				{
					var _ = new MessageDialog(e.ToString(), "Error in client add.").ShowAsync();
				}
			});
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			PlayBoard.BoardMode = Board.Mode.Manipulation;
			InkTools.TargetInkCanvas = PlayBoard.GetDrawCanvas();

			PlayBoard.CameraMoving += OnPlayBoardCameraMoving;
			PlayBoard.CharacterMoved += OnPlayBoardCharacterMoved;
			PlayBoard.LinesAdded += OnPlayBoardLinesAdded;
			PlayBoard.LinesRemoved += OnPlayBoardLinesRemoved;

			PlayBoard.GetDrawCanvas().InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;
			PlayBoard.ScrollbarsVisible = true;
		}

		private async void OnPlayBoardLinesRemoved(object sender, InkStrokesErasedEventArgs e)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				MemoryStream Buffer = new MemoryStream();
				await PlayBoard.GetDrawCanvas().InkPresenter.StrokeContainer.SaveAsync(Buffer.AsOutputStream());

				NetworkServer.BroadCast(new BoardLinesMessage() { Data = Buffer.ToArray() });
			});
		}

		private async void OnPlayBoardLinesAdded(object sender, InkStrokesCollectedEventArgs e)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				MemoryStream Buffer = new MemoryStream();
				await PlayBoard.GetDrawCanvas().InkPresenter.StrokeContainer.SaveAsync(Buffer.AsOutputStream());

				NetworkServer.BroadCast(new BoardLinesMessage() { Data = Buffer.ToArray() });
			});
		}

		private void OnPlayBoardCharacterMoved(object sender, Character e)
		{
			NetworkServer.BroadCast(new MoveCharacterMessage() { TargetCharacter = e });
		}

		private void OnPlayBoardCameraMoving(object sender, ScrollViewerViewChangingEventArgs e)
		{
			NetworkServer.BroadCast(new CameraMovedMessage() { HorizontalOffset = e.FinalView.HorizontalOffset, VerticalOffset = e.FinalView.VerticalOffset, ZoomFactor = e.FinalView.ZoomFactor });
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName]string memberName = null)
		{
			if (memberName != null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
			}
		}

		private void RemoveLastLineButtonClick(object sender, RoutedEventArgs e)
		{
			InkStroke LastStroke = PlayBoard.GetDrawCanvas().InkPresenter.StrokeContainer.GetStrokes().LastOrDefault();

			if (LastStroke == null)
			{
				return;
			}

			LastStroke.Selected = true;
			PlayBoard.GetDrawCanvas().InkPresenter.StrokeContainer.DeleteSelected();

			OnPlayBoardLinesRemoved(this, null);
		}

		private async void OnAddCharacterButtonClick(object sender, RoutedEventArgs e)
		{
			ConfigureFlyout.Hide();

			AddCharacterDialog Dialog = new AddCharacterDialog() { FullSizeDesired = true };
			ContentDialogResult Result = await Dialog.ShowAsync();

			if (Result == ContentDialogResult.Primary)
			{
				Character NewCharacter = new Character() { Color = Dialog.SelectedColor, Name = Dialog.NameText, Position = PlayBoard.CameraPosition, Visuals = Dialog.UseCustomVisuals ? Dialog.VisualsPath : null, ID = Character.MaxID++, Size = new Datamodels.Rect(new Datamodels.Point(), Dialog.CharacterWidth - 0.2, Dialog.CharacterHeight - 0.2) };
				PlayBoard.AddCharacter(NewCharacter);
				NetworkServer.BroadCast(new AddCharacterMessage() { NewCharacter = NewCharacter });
			}
		}

		private async void OnRemoveCharacterButtonClick(object sender, RoutedEventArgs e)
		{
			ConfigureFlyout.Hide();

			RemoveCharacterDialog Dialog = new RemoveCharacterDialog(PlayBoard.GetCharacters().ToList());
			ContentDialogResult Result = await Dialog.ShowAsync();

			if (Result == ContentDialogResult.Primary)
			{
				PlayBoard.RemoveCharacter(Dialog.Selected);
				NetworkServer.BroadCast(new RemoveCharacterMessage() { OldCharacter = Dialog.Selected });
			}
		}

		private void OnFreezeSwitchToggled(object sender, RoutedEventArgs e)
		{
			NetworkServer.BroadCast(new FreezeMessage() { Freeze = ((ToggleSwitch)sender).IsOn });
		}
	}
}
