using CommandLine;
using static infomaniac50.Waf;

namespace infomaniac50
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
