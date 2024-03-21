// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Spectre.Console;
using System.Globalization;
using System.Linq;

namespace Metalama.Tool.Licensing;

[UsedImplicitly]
internal class PrintLicenseUsageDetailsCommand : LicenseUsageBaseCommand
{
    protected override void Execute( LicenseUsageCommandContext context )
    {
        var table = new Table();
        table.AddColumn( "Aspect Class Name" );
        table.AddColumn( "Projects" );

        var groups =
            context.Files
                .SelectMany( f => f.ConsumedAspectClasses.Select( c => (File: f, AspectClassName: c) ) )
                .GroupBy( c => c.AspectClassName )
                .OrderBy( g => g.Key );

        foreach ( var group in groups )
        {
            table.AddRow(
                group.Key,
                group.Select( x => x.File.ProjectPath ).Distinct().Count().ToString( CultureInfo.InvariantCulture ) );
        }

        context.Console.Out.Write( table );
    }
}