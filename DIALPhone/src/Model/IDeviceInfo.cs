using System;
using System.Threading.Tasks;

namespace DIALClient.Model
{
    public interface IDeviceInfo
    {
        string FriendlyName
        {
            get;
        }

        Uri UrlBase
        {
            get;
        }

        Uri ApplicationUrlBase
        {
            get;
        }

        string Manufacturer
        {
            get;
        }

        string Model
        {
            get;
        }

        string DeviceType
        {
            get;
        }

        string Udn
        {
            get;
        }

        Task<IApplicationInfo> GetApplicationInfo(string applicationCode);

        Task<IApplicationInfo> GetApplicationInfo<T>(string applicationCode) where T : ApplicationInfo;
    }
}
