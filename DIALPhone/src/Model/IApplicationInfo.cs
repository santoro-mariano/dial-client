using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DIALClient.Model
{
    public interface IApplicationInfo
    {
        /// <summary>
        /// Device Information.
        /// </summary>
        IDeviceInfo DeviceInfo
        {
            get;
        }

        /// <summary>
        /// Application Friendly Name.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Device Application Name.
        /// </summary>
        string Code
        {
            get;
        }

        /// <summary>
        /// Application options.
        /// </summary>
        ApplicationOptions Options
        {
            get;
        }

        /// <summary>
        /// Current state of application in remote device.
        /// </summary>
        ApplicationStates State
        {
            get;
        }

        string RunningResourceName
        {
            get;
        }

        /// <summary>
        /// Url used to install application on remote device.
        /// </summary>
        Uri InstallUrl
        {
            get;
        }

        /// <summary>
        /// Additional data of application.
        /// </summary>
        XElement AdditionalData
        {
            get;
        }

        Uri InstanceUrl
        {
            get;
        }

        Task Install();

        Task Run(string parameters = null);

        Task Stop();
    }
}
