using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

		private void OnPlayMasterButtonClick(object sender, RoutedEventArgs e)
		{

		}

		private async void OnPlayClientButtonClick(object sender, RoutedEventArgs e)
		{
			ConnectionDialog Dialog = new ConnectionDialog();
			ContentDialogResult Result = await Dialog.ShowAsync();

			if (Result == ContentDialogResult.Primary)
			{
				this.Frame.Navigate(typeof(ClientPage), new ClientPage.ClientPageParameters() { Hostname = Dialog.SelectedSystem.DisplayName });
			}
		}
	}
}
