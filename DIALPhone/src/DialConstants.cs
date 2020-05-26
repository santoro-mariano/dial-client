using Windows.Networking;

namespace DIALClient
{
    internal static class DialConstants
    {
        public static string SearchTarget = "urn:dial-multiscreen-org:service:dial:1";

        public static HostName MulticastHost = new HostName("239.255.255.250");

        public static string UdpPort = "1900";

        public static string MSearchMessage = "M-SEARCH * HTTP/1.1\r\nHOST: {0}:{1}\r\nST: urn:dial-multiscreen-org:service:dial:1\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\n\r\n";
    }
}
