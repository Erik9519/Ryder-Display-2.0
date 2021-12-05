using RyderDisplay.Components.Network;
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

namespace RyderDisplay
{
	public sealed partial class ConnectionDialog : ContentDialog
	{
		// Internal Variables
		RyderClient ryderClient;
		public ConnectionDialog(RyderClient ryderClient)
		{
			this.InitializeComponent();
			this.ryderClient = ryderClient;
		}

		private void ConnectButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			this.ryderClient.setup(this.ip.Text, int.Parse(this.port.Text), this.pswd.Text);
			this.ryderClient.connect();
		}
	}
}
