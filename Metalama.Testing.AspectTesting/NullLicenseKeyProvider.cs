// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Testing.AspectTesting;

internal class NullLicenseKeyProvider : ILicenseKeyProvider
{
    public bool TryGetLicenseKey( string name, [NotNullWhen( true )] out string? licenseKey )
    {
        licenseKey = null;

        if ( name.Equals( "none", StringComparison.OrdinalIgnoreCase ) )
        {
            return true;
        }

        return false;
    }
}