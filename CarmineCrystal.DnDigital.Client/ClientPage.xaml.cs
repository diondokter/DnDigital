using CarmineCrystal.Networking;
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
    public sealed partial class ClientPage : Page
    {
		public NetworkClient Server;

        public ClientPage()
        {
            this.InitializeComponent();
        }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			Server = new NetworkClient(((ClientPageParameters)e.Parameter).Hostname, 5000);
		}

		public class ClientPageParameters
		{
			public string Hostname;
		}
	}
}
