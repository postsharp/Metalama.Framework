// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Metalama.Tool.Licensing;

internal sealed class LicenseUsageCommandSettings : BaseCommandSettings
{
    [CommandOption( "-d|--days" )]
    [Description( "Includes only projects built in the specified number of days." )]
    public double? LastDays { get; init; }

    [CommandOption( "-w|--weeks" )]
    [Description( "Includes only projects built in the specified number of weeks. The default is 1 week." )]
    public double? LastWeeks { get; init; }

    [CommandOption( "-h|--hours" )]
    [Description( "Includes only projects built in the specified number of hours." )]
    public double? LastHours { get; init; }

    [CommandOption( "-p|--project" )]
    [Description(
        "Includes only the specified projects, specified by file name without extension. You can use `*` to match any substring. May be a comma-separated list." )]
    public string? Projects { get; init; }

    [CommandOption( "-c|--configuration" )]
    [Description( "Includes only the specified build configurations (typically Debug or Release).  May be a comma-separated list." )]
    public string? Configurations { get; init; }

    [CommandOption( "-f|--framework" )]
    [Description( "Includes only the specified target frameworks. May be a comma-separated list. To match an empty string, use 'empty'." )]
    public string? TargetFrameworks { get; init; }

    internal static IReadOnlyList<string> SplitCommaSeparatedList( string? s )
        => s switch
        {
            null => ArraySegment<string>.Empty,
            _ => s.Split( new[] { ',', ';' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries )
        };

    public IReadOnlyList<Regex> GetProjectRegexes()
    {
        var list = new List<Regex>();

        if ( this.Projects == null )
        {
            return list;
        }

        foreach ( var project in SplitCommaSeparatedList( this.Projects ) )
        {
            if ( !string.IsNullOrWhiteSpace( project ) )
            {
                list.Add(
                    new Regex(
                        "^" + project.Replace( ".", "\\.", StringComparison.Ordinal ).Replace( "*", ".*", StringComparison.Ordinal ) + "$",
                        RegexOptions.IgnoreCase ) );
            }
        }

        return list;
    }

    public DateTime GetHorizon()
    {
        if ( this.LastDays != null )
        {
            return DateTime.Today.AddDays( -1 * (this.LastDays.Value - 1) );
        }
        else if ( this.LastWeeks != null )
        {
            return DateTime.Today.AddDays( -7 * (this.LastWeeks.Value - 1) );
        }
        else if ( this.LastHours != null )
        {
            return DateTime.Now.AddHours( -1 * this.LastHours.Value );
        }
        else
        {
            // Default.
            return DateTime.Today.AddDays( -7 );
        }
    }
}