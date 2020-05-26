using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Web.Http;
using DIALClient.Model;

namespace DIALClient
{
    public static class DialServiceDiscovery
    {
        /// <summary>
        /// Search devices on the local network.
        /// </summary>
        /// <param name="timeoutMiliseconds">Timeout to search devices.</param>
        /// <param name="stopOnFirstDevice">Indicates wheter the Discovery Service must stop when find the first device.</param>
        /// <returns>All found devices.</returns>
        public static async Task<IEnumerable<IDeviceInfo>> GetAllDevices(bool stopOnFirstDevice = false, uint timeoutMiliseconds = 3000)
        {
            if (timeoutMiliseconds == 0)
            {
                throw new InvalidOperationException("Timeout must be greater than zero.");
            }
            var devices = new List<IDeviceInfo>();
            var timeoutCancellationTokenSource = new CancellationTokenSource();
            using (var socket = new DatagramSocket())
            {
                socket.MessageReceived += async (sender, args) =>
                {
                    var reader = args.GetDataReader();
                    var deviceLocationUri = GetDeviceLocation(reader.ReadString(reader.UnconsumedBufferLength));
                    var deviceInfo = await GetDeviceInfo(deviceLocationUri);
#if DEBUG
                    Debug.WriteLine(string.Format("DEVICE FOUND: Name = {0}, Manufacturer = {1}, Model = {2}, Address = {3}", deviceInfo.FriendlyName, deviceInfo.Manufacturer, deviceInfo.Model, deviceInfo.UrlBase));
#endif
                    devices.Add(deviceInfo);
                    if (stopOnFirstDevice)
                    {
                        timeoutCancellationTokenSource.Cancel();
                    }
                };
                await socket.BindEndpointAsync(null, "");
                socket.JoinMulticastGroup(DialConstants.MulticastHost);
                var writer = new DataWriter(await socket.GetOutputStreamAsync(DialConstants.MulticastHost, DialConstants.UdpPort));
                writer.WriteString(string.Format(DialConstants.MSearchMessage, DialConstants.MulticastHost, DialConstants.UdpPort));
                await writer.StoreAsync();
                try
                {
                    await Task.Delay((int)timeoutMiliseconds, timeoutCancellationTokenSource.Token);
                }
                catch
                {}
                
            }
            return devices;
        }

        /// <summary>
        /// Search one device on the local network with the specified name.
        /// </summary>
        /// <param name="deviceName">Friendly name of the device.</param>
        /// <param name="timeoutMiliseconds">Timeout to search the device.</param>
        /// <returns>Device with the specified name.</returns>
        public static async Task<IDeviceInfo> GetDevice(string deviceName, uint timeoutMiliseconds = 6000)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                throw new ArgumentNullException("deviceName");
            }
            if (timeoutMiliseconds == 0)
            {
                throw new InvalidOperationException("Timeout must be greater than zero.");
            }
            IDeviceInfo deviceInfo = null;
            var timeoutCancellationTokenSource = new CancellationTokenSource();
            using (var socket = new DatagramSocket())
            {
                socket.MessageReceived += async (sender, args) =>
                {
                    var reader = args.GetDataReader();
                    var deviceLocationUri = GetDeviceLocation(reader.ReadString(reader.UnconsumedBufferLength));
                    var dinfo = await GetDeviceInfo(deviceLocationUri);
#if DEBUG
                    Debug.WriteLine(string.Format("DEVICE FOUND: Name = {0}, Manufacturer = {1}, Model = {2}, Address = {3}", dinfo.FriendlyName, dinfo.Manufacturer, dinfo.Model, dinfo.UrlBase));
#endif
                    if (dinfo.FriendlyName.Equals(deviceName))
                    {
                        deviceInfo = dinfo;
                        timeoutCancellationTokenSource.Cancel();
                    }
                };
                await socket.BindEndpointAsync(null, "");
                socket.JoinMulticastGroup(DialConstants.MulticastHost);
                var writer = new DataWriter(await socket.GetOutputStreamAsync(DialConstants.MulticastHost, DialConstants.UdpPort));
                writer.WriteString(string.Format(DialConstants.MSearchMessage, DialConstants.MulticastHost, DialConstants.UdpPort));
                await writer.StoreAsync();
                try
                {
                    await Task.Delay((int)timeoutMiliseconds, timeoutCancellationTokenSource.Token);
                }
                catch
                {}
            }

            return deviceInfo;
        }

        private static Uri GetDeviceLocation(string ssdpResponse)
        {
            var strReader = new StringReader(ssdpResponse);
            string location;
            while (!string.IsNullOrEmpty(location = strReader.ReadLine()))
            {
                if (location.ToUpper().Contains("LOCATION"))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(location))
            {
                return null;
            }

            var locationParts = location.Split(':');
            var finalLocation = string.Empty;
            for (var i = 1; i < locationParts.Length; i++)
            {
                finalLocation += locationParts[i] + ":";
            }
            finalLocation = finalLocation.TrimEnd(':');
            return new Uri(finalLocation);
        }

        private static async Task<DeviceInfo> GetDeviceInfo(Uri deviceLocation)
        {
            using (var client = new HttpClient())
            {
                var descriptionResponse = await client.GetAsync(deviceLocation);
                var applicationUrl = new Uri(descriptionResponse.Headers.Single().Value);
                var responseContent = await descriptionResponse.Content.ReadAsStringAsync();
                return new DeviceInfo(applicationUrl, XDocument.Parse(responseContent));
            }
        }
    }
}
