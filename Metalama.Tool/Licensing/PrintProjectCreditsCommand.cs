// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Spectre.Console;
using System.Globalization;
using System.Linq;

namespace Metalama.Tool.Licensing;

[UsedImplicitly]
internal class PrintProjectCreditsCommand : CreditsBaseCommand
{
    protected override void Execute( CreditsCommandContext context, CreditsCommandSettings settings )
    {
        var table = new Table();

        table.AddColumn( "Project File" );
        table.AddColumn( "Configuration" );
        table.AddColumn( "Target Framework" );
        table.AddColumn( "Required Credits" );
        table.AddColumn( "Metalama Version" );
        table.AddColumn( "Metalama Build Date" );

        foreach ( var file in context.Files.OrderBy( p => p.ProjectPath ).ThenBy( p => p.Configuration ).ThenBy( p => p.TargetFramework ) )
        {
            table.AddRow(
                file.ProjectPath,
                file.Configuration,
                file.TargetFramework,
                file.TotalCredits.ToString( CultureInfo.InvariantCulture ),
                file.MetalamaVersion,
                file.MetalamaBuildDate?.ToString( "d", CultureInfo.InvariantCulture ) ?? " " );
        }

        table.AddRow(
            "MAXIMUM",
            "",
            "",
            context.Files.Max( f => f.TotalCredits ).ToString( CultureInfo.InvariantCulture ),
            context.Files.Select( f => f.MetalamaVersion ).OrderByDescending( f => f, new PackageVersionComparer() ).First(),
            context.Files.Where( f => f.MetalamaBuildDate.HasValue ).Max( f => f.MetalamaBuildDate!.Value ).ToString( "d", CultureInfo.InvariantCulture ) );

        context.Console.Out.Write( table );
    }
}