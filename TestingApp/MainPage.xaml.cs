using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641
using DIALClient;
using DIALClient.Model;

namespace TestingApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.LaunchTest();
            

        }

        private async void LaunchTest()
        {
            lblData.Text = "Loading devices, please wait...";
            this.NavigationCacheMode = NavigationCacheMode.Required;
            var test = await DialServiceDiscovery.GetAllDevices(true, 10000);
            var device = test.FirstOrDefault();
            if (device == null)
            {
                lblData.Text = "Not device found.";
            }
            else
            {
                lblData.Text = string.Format("{0}({1})", device.FriendlyName, device.Manufacturer);
                var appInfo = await device.GetApplicationInfo<ApplicationInfo>("YouTube");
                if (appInfo != null)
                {
                    if (appInfo.State == ApplicationStates.Running)
                    {
                        await appInfo.Stop();
                    }
                    lblApplication.Text = "Starting Youtube...";
                    await appInfo.Run();
                    lblInstanceUrl.Text = "Instance Url: " + appInfo.InstanceUrl;
                    await Task.Delay(5000);
                    await device.GetApplicationInfo("YouTube");
                }
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
    }
}
