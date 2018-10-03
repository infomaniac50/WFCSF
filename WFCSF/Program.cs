using CommandLine;
using static infomaniac50.Waf;

namespace infomaniac50
{
    class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        static int Main(string[] args)
        {
            int ExitCode = Parser.Default.ParseArguments<WafOptions>(args)
                .MapResult(
                    (WafOptions opts) => RunWafAndReturnExitCode(opts),
                    errs => 1
                );

#if DEBUG
            System.Console.WriteLine("Press any key to finish.");
            System.Console.ReadKey();
#endif
            return ExitCode;
        }
    }
}
