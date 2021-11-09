using PostSharp.Engineering.BuildTools.Build;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class CommonOptions : CommandSettings
    {
        [Description( "Sets the build configuration (Debug or Release)" )]
        [CommandOption( ("-c|--configuration") )]
        public BuildConfiguration Configuration { get; }

        [Description( "Creates a numbered build (typically for an internal CI build)" )]
        [CommandOption( "--build-number" )]
        public int BuildNumber { get; init; }

        [Description( "Creates a public build (typically to publish to nuget.org)" )]
        [CommandOption( "--public-build" )]
        public bool PublicBuild { get; init; }

        [Description( "Sets the verbosity" )]
        [CommandOption( "-v|--verbosity" )]
        [DefaultValue( Verbosity.Minimal )]
        public Verbosity Verbosity { get; init; }
        
        [Description("Executes only the current command, but not the previous command")]
        [CommandOption("--skip-dependencies")]
        public bool SkipDependencies { get; init; }
        
        public VersionSpec VersionSpec => this.BuildNumber > 0
            ? new VersionSpec( VersionKind.Numbered, this.BuildNumber )
            : this.PublicBuild
                ? new VersionSpec( VersionKind.Public )
                : new VersionSpec( VersionKind.Local );
    }
}