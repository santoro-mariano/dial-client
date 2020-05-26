using System.Xml.Linq;

namespace DIALClient.Model
{
    public sealed class ApplicationOptions
    {
        internal ApplicationOptions(XElement optionsDescription)
        {
            var allowStop = optionsDescription.Attribute("allowStop");
            if (allowStop != null && !string.IsNullOrEmpty(allowStop.Value)) this.AllowStop = bool.Parse(allowStop.Value);
        }

        public ApplicationOptions()
        { }

        /// <summary>
        /// Indicates wheter the application can be stopped.
        /// </summary>
        public bool AllowStop { get; private set; }
    }
}
