using System.Net;

namespace Xamino.Base
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static bool IsIpAddress(this string value) => IPAddress.TryParse(value, out var ipAddress);

        public static bool IsPort(this string value)
        {
            if (int.TryParse(value, out var port))
                return 1 <= port && port <= 65535;
            return false;
        }
    }
}
