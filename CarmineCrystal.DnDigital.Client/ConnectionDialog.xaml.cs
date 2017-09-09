using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CarmineCrystal.DnDigital.Client
{
	public sealed partial class ConnectionDialog : ContentDialog
	{
		public RemoteSystem SelectedSystem { get; set; }

		public ObservableCollection<RemoteSystem> Devices { get; } = new ObservableCollection<RemoteSystem>();
		private RemoteSystemWatcher Watcher;

		public ConnectionDialog()
		{
			InitializeComponent();
			DataContext = this;
			StartWatch();
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			if (SelectedSystem == null)
			{
				args.Cancel = true;
				return;
			}

			Watcher.Stop();
		}

		private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Watcher.Stop();
		}

		private async void StartWatch()
		{
			RemoteSystemAccessStatus Status = await RemoteSystem.RequestAccessAsync();
			Watcher = RemoteSystem.CreateWatcher();
			Watcher.RemoteSystemAdded += Watcher_RemoteSystemAdded;
			Watcher.RemoteSystemRemoved += Watcher_RemoteSystemRemoved;
			Watcher.Start();
		}

		private async void Watcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
		{
			await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Devices.Add(args.RemoteSystem));
		}

		private async void Watcher_RemoteSystemRemoved(RemoteSystemWatcher sender, RemoteSystemRemovedEventArgs args)
		{
			await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Devices.Remove(Devices.First(x => x.Id == args.RemoteSystemId)));
		}
	}
}
