using RyderDisplay.Components.Network;
using RyderDisplay.Components.UI;
using RyderDisplay.Components.UI.Dynamic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RyderDisplay
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, RyderClient.Callback
    {
        private RyderClient ryderClient = new RyderClient();
        private List<Element> UIelements = new List<Element>();
        public MainPage()
        {
            this.InitializeComponent();
            ryderClient.addEndpoint("authentication", this);
            ryderClient.addEndpoint("status", this);

            /*RyderClient client = new RyderClient("192.168.1.218", 9519, "1234");
            client.connect();*/
            // GPU core clock
            TextView elem1 = new TextView(this, "GPU core clock", null, new float[] { 50, 50 }, 5);
            elem1.setFontSize(15);
            elem1.setMetricPath(new List<string> { "msi", "Core clock" });
            TextView elem2 = new TextView(this, "GPU core clock unit", elem1, new float[] { 53, 46 }, 1);
            elem2.setFontSize(10);
            elem2.setStringFormat("MHz");
            RoundProgressBar elem3 = new RoundProgressBar(this, "CPU usage", null, new float[] { 300, 300 }, 250, 5);
            elem3.setMetricPath(new List<string> { "msi", "CPU usage" });

            UIelements.Add(elem1);
            UIelements.Add(elem2);
            UIelements.Add(elem3);

            //UIelements.Add(new TextView(this, "CPU clock", new double[] { 50, 50 }, 4)); // new List<string>{ "msi", "CPU clock" }
            //UIelements.Add(new TextView(this, "GPU core clock", new double[] { 50, 25 }, 5)); // new List<string> { "msi", "Core clock" }
        }

        private async void onWindowLoaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000); // Workaround to bug (https://github.com/unoplatform/uno/issues/4550)
            showConnectionDialog();
        }

        public void OnReceive(string cmd, object json)
        {
            if (cmd.Equals("authentication"))
            {
                string result = (string)json;
                if (result.Equals("deny"))
                {
                    this.ryderClient.disconnect();
                    _ = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() => { this.showConnectionDialog(); }));
                }
            } else if (cmd.Equals("status"))
            {
                foreach (Element o in UIelements) o.OnReceive(cmd, json);
            }
        }

        private async void showConnectionDialog()
        {
            _ = await new ConnectionDialog(this.ryderClient).ShowAsync(ContentDialogPlacement.Popup);
        }
    }
}
