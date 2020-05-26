using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DIALClient.Model
{
    /// <summary>
    /// ApplicationInfo class.
    /// </summary>
    public class ApplicationInfo : IApplicationInfo
    {
        public ApplicationInfo(XDocument applicationDescription)
        {
            if (applicationDescription.Root != null)
            {
                var rootNamespace = applicationDescription.Root.Name.Namespace;
                var service = applicationDescription.Root;
                var name = service.Element(rootNamespace + "name");
                var options = service.Element(rootNamespace + "options");
                var state = service.Element(rootNamespace + "state");
                var link = service.Element(rootNamespace + "link");
                this.AdditionalData = service.Element(rootNamespace + "additionalData");

                if (name != null && !string.IsNullOrEmpty(name.Value)) this.Name = name.Value;
                if (options != null) this.Options = new ApplicationOptions(options);
                if (state != null && !string.IsNullOrEmpty(state.Value))
                {
                    var stateValue = state.Value.ToUpper();
                    if (stateValue.Equals("RUNNING"))
                    {
                        this.State = ApplicationStates.Running;
                    }
                    else if (stateValue.Equals("STOPPED"))
                    {
                        this.State = ApplicationStates.Stopped;
                    }
                    else if (stateValue.StartsWith("INSTALLABLE="))
                    {
                        this.State = ApplicationStates.Installable;
                        this.InstallUrl = new Uri(stateValue.Replace("INSTALLABLE=", ""));
                    }
                }
                if (link != null && link.HasAttributes)
                {
                    var href = link.Attribute("href");
                    if (href != null && !string.IsNullOrEmpty(href.Value)) this.RunningResourceName = href.Value;
                }
            }
        }

        public IDeviceInfo DeviceInfo
        {
            get;
            internal set;
        }

        public string Name
        {
            get;
            internal set;
        }
        public string Code
        {
            get;
            internal set;
        }
        public ApplicationOptions Options
        {
            get;
            internal set;
        }
        public ApplicationStates State
        {
            get;
            internal set;
        }

        public string RunningResourceName
        {
            get;
            internal set;
        }

        public Uri InstallUrl
        {
            get;
            internal set;
        }

        public XElement AdditionalData
        {
            get;
            internal set;
        }

        public Uri InstanceUrl
        {
            get
            {
                if (this.State != ApplicationStates.Running || this.DeviceInfo == null || this.DeviceInfo.ApplicationUrlBase == null || string.IsNullOrEmpty(this.Code) || string.IsNullOrEmpty(this.RunningResourceName))
                {
                    return null;
                }

                return new Uri(this.DeviceInfo.ApplicationUrlBase + this.Code + "/" + this.RunningResourceName);
            }
        }

        public async Task Install()
        {
            if (this.State != ApplicationStates.Installable)
            {
                throw new InvalidOperationException("Invalid Application State");
            }
            await DialRestService.InstallApplication(this.InstallUrl);
        }

        public async Task Run(string parameters = null)
        {
            if (this.State != ApplicationStates.Stopped)
            {
                throw new InvalidOperationException("Invalid Application State");
            }
            var instanceUrl = await DialRestService.RunApplication(new Uri(this.DeviceInfo.ApplicationUrlBase + this.Code), parameters);
            this.State = ApplicationStates.Running;
            this.RunningResourceName = instanceUrl.AbsolutePath.Split('/').Last();
        }

        public async Task Stop()
        {
            if (this.State != ApplicationStates.Running)
            {
                throw new InvalidOperationException("Invalid Application State");
            }
            await DialRestService.StopApplication(this.InstanceUrl);
            this.State = ApplicationStates.Stopped;
            this.RunningResourceName = null;
        }
    }
}
