using CommandLine;
using static IP_Processor.Waf;

namespace IP_Processor
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<WafOptions>(args)
                .MapResult(
                    (WafOptions opts) => RunWafAndReturnExitCode(opts),
                    errs => 1
                );
        }
    }
}
