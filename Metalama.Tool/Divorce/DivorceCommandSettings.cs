// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Spectre.Console.Cli;
using System.ComponentModel;

#pragma warning disable CS8618

namespace Metalama.Tool.Divorce;

internal class DivorceCommandSettings : BaseCommandSettings
{
    [Description( "Path to the project file on which the divorce feature will be run." )]
    [CommandArgument( 0, "<projectPath>" )]
    public string ProjectPath { get; init; }

    [Description( "Force the divorce feature, even if the working directory is not known to be clean." )]
    [CommandOption( "--force" )]
    public bool Force { get; init; }

    [Description( "The build configuration, usually either Debug or Release." )]
    [CommandOption( "--configuration | -c" )]
    public string? Configuration { get; init; }

    [Description( "The target framework, such as net7.0, netstandard2.0 or net471." )]
    [CommandOption( "--framework | -f" )]
    public string? TargetFramework { get; init; }
}