using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using System.Net;
using System.Globalization;

namespace IP_Processor
{
    struct WAFBlocked
    {
        public IPAddress Ip { get; set; }
        public RegionInfo CountyCode { get; set; }
        public int BlockedCount { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string directory = Directory.GetCurrentDirectory();
            string filter = "*.txt";
            
            IEnumerable<string> files = Directory.EnumerateFiles(directory, filter);

            //ConcurrentBag<WAFBlocked> firewallEntries = new ConcurrentBag<WAFBlocked>();

            var firewallEntries = files.AsParallel().SelectMany<string, WAFBlocked?>(path => GetFirewallTable(path)).Where(entry => entry.HasValue).Select(entry => entry.Value);
            //foreach (var path in files)
            //{
            //    foreach (WAFBlocked? entry in GetFirewallTable(path))
            //    {
            //        if (entry.HasValue)
            //        {
            //            firewallEntries.Add(entry.Value);
            //        }
            //    }
            //}

            foreach (var entry in firewallEntries)
            {
                if (entry.BlockedCount > 10)
                {
                    Console.WriteLine("{0} # from {1} was blocked {2} times in Wordfence", entry.Ip.ToString(), entry.CountyCode.EnglishName, entry.BlockedCount);
                }
            }
        }

        static IEnumerable<WAFBlocked?> GetFirewallTable(string path)
        {
            StreamReader streamReader = new StreamReader(path);

            // Skip lines until Top 10 IPs Blocked is reached.
            // TODO: See if there is a "SkipUntil" kind of function we can use.
            while (streamReader.Peek() >= 0)
            {
                string currentLine = streamReader.ReadLine();

                if (currentLine == "Top 10 IPs Blocked")
                {
                    break;
                }
            }

            var lines = new List<string>();
            // Copy lines until "Update Blocked IPs" is reached.
            // TODO: See if there is a "ReadUntil" kind of function we can use.
            while (streamReader.Peek() >= 0)
            {
                string currentLine = streamReader.ReadLine();

                if (currentLine == "Update Blocked IPs")
                {
                    break;
                }

                lines.Add(currentLine);
            }

            streamReader.Close();

            var firewallTable = lines.Skip(1).Where(line => line.Trim(' ', '\t') != String.Empty).Select<string, WAFBlocked?>(line =>
            {
                char[] separator = { ' ', '\t' };
                var columns = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                IPAddress blockedAddress;
                CultureInfo addressCulture = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(cultureInfo =>
                {
                    RegionInfo region = new RegionInfo(cultureInfo.LCID);

                    return region.TwoLetterISORegionName == columns[1];
                }).DefaultIfEmpty(CultureInfo.CurrentCulture).First();
                int blockedCount;

                if (IPAddress.TryParse(columns[0], out blockedAddress) && int.TryParse(columns[2], out blockedCount))
                {
                    return new WAFBlocked
                    {
                        Ip = blockedAddress,
                        BlockedCount = blockedCount,
                        CountyCode = new RegionInfo(addressCulture.LCID)
                    };
                }

                return null;
            });

            return firewallTable;
        }
    }
}
