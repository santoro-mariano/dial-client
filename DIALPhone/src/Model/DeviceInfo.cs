using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DIALClient.Model
{
    internal sealed class DeviceInfo : IDeviceInfo
    {
        public DeviceInfo(Uri applicationUrlBase, XDocument deviceDescription)
        {
            this.ApplicationUrlBase = applicationUrlBase;
            if (deviceDescription.Root != null)
            {
                var rootNamespace = deviceDescription.Root.Name.Namespace;
                var urlBase = deviceDescription.Root.Element(rootNamespace + "URLBase");
                if (urlBase != null && !String.IsNullOrEmpty(urlBase.Value)) this.UrlBase = new Uri(urlBase.Value);
                var device = deviceDescription.Root.Element(rootNamespace + "device");
                if (device != null)
                {
                    var model = device.Element(rootNamespace + "modelName");
                    var manufacturer = device.Element(rootNamespace + "manufacturer");
                    var friendlyName = device.Element(rootNamespace + "friendlyName");
                    var udn = device.Element(rootNamespace + "UDN");
                    var deviceType = device.Element(rootNamespace + "deviceType");

                    if (model != null && !String.IsNullOrEmpty(model.Value)) this.Model = model.Value;
                    if (manufacturer != null && !String.IsNullOrEmpty(manufacturer.Value)) this.Manufacturer = manufacturer.Value;
                    if (friendlyName != null && !String.IsNullOrEmpty(friendlyName.Value)) this.FriendlyName = friendlyName.Value;
                    if (udn != null && !String.IsNullOrEmpty(udn.Value)) this.Udn = udn.Value;
                    if (deviceType != null && !String.IsNullOrEmpty(deviceType.Value)) this.DeviceType = deviceType.Value;
                }
            }
        }

        public Uri ApplicationUrlBase
        {
            get;
            private set;
        }
        public string FriendlyName
        {
            get;
            private set;
        }
        public Uri UrlBase
        {
            get;
            private set;
        }
        public string Manufacturer
        {
            get;
            private set;
        }
        public string Model
        {
            get;
            private set;
        }

        public string DeviceType
        {
            get;
            private set;
        }

        public string Udn
        {
            get;
            private set;
        }

        public async Task<IApplicationInfo> GetApplicationInfo(string applicationCode)
        {
            return await this.GetApplicationInfo<ApplicationInfo>(applicationCode);
        }

        public async Task<IApplicationInfo> GetApplicationInfo<T>(string applicationCode) where T : ApplicationInfo
        {
            return await DialRestService.GetApplicationInfo<T>(this, applicationCode);
        }
    }
}
