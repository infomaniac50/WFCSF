using System.Globalization;
using System.Net;

namespace infomaniac50
{
    struct WafBlocked
    {
        public IPAddress Ip { get; set; }
        public RegionInfo CountyCode { get; set; }
        public int BlockedCount { get; set; }
    }
}
