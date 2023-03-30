// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Spectre.Console.Cli;
using System.ComponentModel;

#pragma warning disable CS8618

namespace Metalama.Tool.Divorce;

internal class DivorceCommandSettings : BaseCommandSettings
{
    [Description( "Force the divorce feature, even if the working directory is not known to be clean." )]
    [CommandOption( "--force" )]
    public bool Force { get; init; }
}