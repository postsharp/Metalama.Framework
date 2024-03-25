// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Spectre.Console;
using System.Globalization;
using System.Linq;

namespace Metalama.Tool.Licensing;

[UsedImplicitly]
internal class PrintTotalLicenseUsageCommand : LicenseUsageBaseCommand
{
    protected override void Execute( LicenseUsageCommandContext context )
    {
        var requiredAspectClassesCount = context.Files.Max( f => f.TotalAspectClasses );

        var table = new Table();
        table.AddColumns( "Maximum Used Aspect Classes Count", "Maximum Metalama Version", "Maximum Metalama Build Date" );

        table.AddRow(
            requiredAspectClassesCount.ToString( CultureInfo.InvariantCulture ),
            context.Files.Select( f => f.MetalamaVersion ).OrderByDescending( f => f, new PackageVersionComparer() ).First(),
            context.Files.Where( f => f.MetalamaBuildDate.HasValue ).Max( f => f.MetalamaBuildDate!.Value ).ToString( "d", CultureInfo.InvariantCulture ) );

        context.Console.Out.Write( table );
    }
}