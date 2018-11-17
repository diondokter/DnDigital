using CarmineCrystal.DnDigital.Messages;
using CarmineCrystal.Networking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UIElementClone;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace CarmineCrystal.DnDigital.Client
{
    public sealed partial class ClientPage : Page
    {
		public NetworkClient Server;

        public ClientPage()
        {
			this.DataContext = this;
            this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			Message.Initialize(typeof(AddCharacterMessage).GetTypeInfo().Assembly);
			Server = new NetworkClient(((ClientPageParameters)e.Parameter).Hostname, 5000, null,

				new DelegateMessageProcessingModule<AddCharacterMessage>((message, server) => PlayBoard.AddCharacter(message.NewCharacter)),
				new DelegateMessageProcessingModule<MoveCharacterMessage>((message, server) => PlayBoard.MoveCharacter(message.TargetCharacter)),
				new DelegateMessageProcessingModule<RemoveCharacterMessage>((message, server) => PlayBoard.RemoveCharacter(message.OldCharacter)),
				new DelegateMessageProcessingModule<BoardLinesMessage>(async (message, server) =>
				{
					await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
					{
						MemoryStream Buffer = new MemoryStream(message.Data);
						await PlayBoard.GetDrawCanvas().InkPresenter.StrokeContainer.LoadAsync(Buffer.AsInputStream());
					});
				}),
				new DelegateMessageProcessingModule<CameraMovedMessage>((message, server) => PlayBoard.SetCamera(message.HorizontalOffset, message.VerticalOffset, message.ZoomFactor)),
				new DelegateMessageProcessingModule<FreezeMessage>(async (message, server) =>
				{
					await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
					{
						if (message.Freeze)
						{
							FreezeBoard();
						}
						else
						{
							UnFreezeBoard();
						}
					});
				})
			);

			PlayBoard.GetDrawCanvas().InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.None;
			PlayBoard.ScrollbarsVisible = false;
		}

		private async void FreezeBoard()
		{
			RenderTargetBitmap FreezeElement = new RenderTargetBitmap();
			await FreezeElement.RenderAsync(PlayBoard);
			PlayBoard.Visibility = Visibility.Collapsed;
			FreezeFrame.Visibility = Visibility.Visible;
			FreezeFrame.Source = FreezeElement;
		}

		private void UnFreezeBoard()
		{
			PlayBoard.Visibility = Visibility.Visible;
			FreezeFrame.Visibility = Visibility.Collapsed;
		}

		public class ClientPageParameters
		{
			public string Hostname;
		}
	}
}
