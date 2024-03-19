// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using System;
using System.IO;

namespace Metalama.Tool.Licensing;

[UsedImplicitly]
internal class ResetLicenseUsageCommand : LicenseUsageBaseCommand
{
    protected override void Execute( LicenseUsageCommandContext context, LicenseUsageCommandSettings settings )
    {
        var deleted = 0;

        foreach ( var file in context.Files )
        {
            try
            {
                if ( file.DataFilePath != null )
                {
                    File.Delete( file.DataFilePath );
                }

                deleted++;
            }
            catch ( Exception e )
            {
                context.Console.WriteWarning( $"Cannot delete '{file.DataFilePath}': {e.Message}" );
            }
        }

        context.Console.WriteSuccess( $"{deleted} files have been deleted." );
    }
}