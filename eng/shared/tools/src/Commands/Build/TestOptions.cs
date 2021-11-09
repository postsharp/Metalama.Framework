using Spectre.Console.Cli;
using System.ComponentModel;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class TestOptions : BuildOptions
    {
        [Description( "Verify test coverage after running the tests" )]
        [CommandOption( "--include-coverage" )]
        public bool GenerateCoverage { get; init; }
    }
}