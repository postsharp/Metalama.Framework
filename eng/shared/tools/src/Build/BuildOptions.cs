using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.Build
{
    public record BuildOptions( VersionSpec Version, BuildConfiguration Configuration = BuildConfiguration.Debug,
        Verbosity Verbosity = Verbosity.Minimal );
}