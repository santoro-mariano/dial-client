using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage.Streams;
using Windows.Web.Http;
using DIALClient.Model;

namespace DIALClient
{
    /// <summary>
    /// DIAL Rest Service class used to interact with specified discovered device.
    /// </summary>
    public static class DialRestService
    {
        public static async Task<IApplicationInfo> GetApplicationInfo(IDeviceInfo deviceInfo, string applicationCode)
        {
            return await GetApplicationInfo<ApplicationInfo>(deviceInfo, applicationCode);
        }

        /// <summary>
        /// Returns application information of specified application name.
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="applicationCode">Application name.</param>
        /// <returns>Application information.</returns>
        public static async Task<IApplicationInfo> GetApplicationInfo<T>(IDeviceInfo deviceInfo, string applicationCode) where T : ApplicationInfo
        {
            if (string.IsNullOrEmpty(applicationCode))
            {
                throw new ArgumentNullException(applicationCode);
            }

            if (deviceInfo.ApplicationUrlBase == null)
            {
                throw new InvalidOperationException("Current device does not have application url base.");
            }

            using (var client = new HttpClient())
            {
                var applicationResourceUrl = new Uri(deviceInfo.ApplicationUrlBase + applicationCode);
#if DEBUG
                Debug.WriteLine(string.Format("GET REQUEST: Url = {0}", applicationResourceUrl));
#endif
                var response = await client.GetAsync(applicationResourceUrl);
#if DEBUG
                Debug.WriteLine(string.Format("GET RESPONSE: IsSuccessStatusCode = {0}", response.IsSuccessStatusCode));
#endif
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                T appInfo;

                try
                {
                    appInfo = (T)Activator.CreateInstance(typeof (T), XDocument.Parse(await response.Content.ReadAsStringAsync()));
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Invalid ApplicationInfo class. Constructor class must have just one argument of type XDocument.");
                }
                appInfo.DeviceInfo = deviceInfo;
                appInfo.Code = applicationCode;
                return appInfo;
            }
        }

        /// <summary>
        /// Install specified application on DIAL Server device.
        /// </summary>
        /// <param name="installUrl">Install Url.</param>
        public static async Task InstallApplication(Uri installUrl)
        {
            if (installUrl == null)
            {
                throw new ArgumentNullException("installUrl");
            }

            using (var client = new HttpClient())
            {
#if DEBUG
                Debug.WriteLine(string.Format("GET REQUEST: Url = {0}", installUrl));
#endif
                await client.GetAsync(installUrl);
            }
        }

        /// <summary>
        /// Executes application in DIAL Server device.
        /// </summary>
        /// <param name="applicationUrl">Application Url.</param>
        /// <param name="parameters">Application constructor parameters.</param>
        /// <returns>Application Instance.</returns>
        public static async Task<Uri> RunApplication(Uri applicationUrl, string parameters = null)
        {
            if (applicationUrl == null)
            {
                throw new ArgumentNullException("applicationUrl");
            }

            using (var client = new HttpClient())
            {
                HttpStringContent content = null;
                if (!string.IsNullOrEmpty(parameters))
                {
                    content = new HttpStringContent(parameters,UnicodeEncoding.Utf8,"text/plain");
                }
                else
                {
                    client.DefaultRequestHeaders.TryAppendWithoutValidation("CONTENT-LENGTH","0");
                }
#if DEBUG
                Debug.WriteLine(string.Format("POST REQUEST: Url = {0}, Content = {1}", applicationUrl, parameters ?? "<null>"));
#endif
                var response = await client.PostAsync(applicationUrl, content);
#if DEBUG
                Debug.WriteLine(string.Format("POST RESPONSE: IsSuccessStatusCode = {0}", response.IsSuccessStatusCode));
#endif
                if (response.IsSuccessStatusCode)
                {
                    return new Uri(response.Headers.Location.AbsoluteUri);
                }

                return null;
            }
        }

        /// <summary>
        /// Stop running application on DIAL Server device.
        /// </summary>
        public static async Task StopApplication(Uri instanceUrl)
        {
            using (var client = new HttpClient())
            {
#if DEBUG
                Debug.WriteLine(string.Format("DELETE REQUEST: Url = {0}", instanceUrl));
#endif
                await client.DeleteAsync(instanceUrl);
            }
        }
    }
}
