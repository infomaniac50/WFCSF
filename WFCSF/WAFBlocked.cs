using System.Globalization;
using System.Net;

namespace IP_Processor
{
    struct WafBlocked
    {
        public IPAddress Ip { get; set; }
        public RegionInfo CountyCode { get; set; }
        public int BlockedCount { get; set; }
    }
}
