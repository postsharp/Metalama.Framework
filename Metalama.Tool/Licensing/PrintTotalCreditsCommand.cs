// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Spectre.Console;
using System;
using System.Globalization;
using System.Linq;

namespace Metalama.Tool.Licensing;

[UsedImplicitly]
internal class PrintTotalCreditsCommand : CreditsBaseCommand
{
    protected override void Execute( CreditsCommandContext context, CreditsCommandSettings settings )
    {
        var requiredCredits = (int) Math.Ceiling( context.Files.Max( f => f.TotalCredits ) );

        var table = new Table();
        table.AddColumns( "Maximum Credit Requirements", "Maximum Metalama Version", "Maximum Metalama Build Date" );

        table.AddRow(
            requiredCredits.ToString( CultureInfo.InvariantCulture ),
            context.Files.Select( f => f.MetalamaVersion ).OrderByDescending( f => f, new PackageVersionComparer() ).First(),
            context.Files.Where( f => f.MetalamaBuildDate.HasValue ).Max( f => f.MetalamaBuildDate!.Value ).ToString( "d", CultureInfo.InvariantCulture ) );

        context.Console.Out.Write( table );
    }
}