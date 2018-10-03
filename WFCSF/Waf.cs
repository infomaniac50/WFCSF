using CommandLine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace infomaniac50
{
    class Waf
    {
        [Verb("waf", HelpText = "Process Wordfence Emails.")]
        public class WafOptions
        {

        }

        public static int RunWafAndReturnExitCode(WafOptions opts)
        {
            string directory = Directory.GetCurrentDirectory();
            string filter = "*.txt";

            IEnumerable<string> files = Directory.EnumerateFiles(directory, filter);

            var firewallEntries = files.AsParallel().SelectMany(path => GetFirewallTable(path)).Where(entry => entry.HasValue).Select(entry => entry.Value);

            foreach (var entry in firewallEntries)
            {
                if (entry.BlockedCount > 10)
                {
                    Console.WriteLine(Properties.Resources.CsfRuleTemplate, entry.Ip.ToString(), entry.CountyCode.EnglishName, entry.BlockedCount);
                }
            }

            return 0;
        }

        private static IEnumerable<WafBlocked?> GetFirewallTable(string path)
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
            

            var firewallTable = lines.Skip(1).Where(line => !String.IsNullOrWhiteSpace(line)).Select<string, WafBlocked?>(line =>
            {
                char[] separator = { ' ', '\t' };
                var columns = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                CultureInfo addressCulture = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(cultureInfo =>
                {
                    RegionInfo region = new RegionInfo(cultureInfo.LCID);

                    return region.TwoLetterISORegionName == columns[1];
                }).DefaultIfEmpty(CultureInfo.CurrentCulture).First();

                if (IPAddress.TryParse(columns[0], out IPAddress blockedAddress) && int.TryParse(columns[2], out int blockedCount))
                {
                    return new WafBlocked
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
