// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Spectre.Console;
using System.Globalization;
using System.Linq;

namespace Metalama.Tool.Licensing;

[UsedImplicitly]
internal class PrintCreditDetailsCommand : CreditsBaseCommand
{
    protected override void Execute( CreditsCommandContext context, CreditsCommandSettings settings )
    {
        var table = new Table();
        table.AddColumn( "Item Name" );
        table.AddColumn( "Item Kind" );
        table.AddColumn( "Credit Cost" );
        table.AddColumn( "Projects" );

        var groups =
            context.Files
                .SelectMany( f => f.ConsumedCredits.Select( c => (File: f, Credit: c) ) )
                .GroupBy( c => c.Credit.ItemName )
                .OrderBy( g => g.First().Credit.Kind )
                .ThenBy( g => g.Key );

        foreach ( var group in groups )
        {
            table.AddRow(
                group.Key,
                group.First().Credit.Kind.ToString(),
                group.Max( i => i.Credit.ConsumedCredits ).ToString( CultureInfo.InvariantCulture ),
                group.Select( x => x.File.ProjectPath ).Distinct().Count().ToString( CultureInfo.InvariantCulture ) );
        }

        context.Console.Out.Write( table );
    }
}