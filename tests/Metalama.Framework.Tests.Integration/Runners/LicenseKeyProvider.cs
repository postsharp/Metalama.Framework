// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Runners;

public class LicenseKeyProvider : ILicenseKeyProvider
{
    private static readonly TestLicenseKeyProvider _provider = new( typeof(LicenseKeyProvider).Assembly );

    public bool TryGetLicenseKey( string name, out string? licenseKey )
    {
        licenseKey = _provider.GetLicenseKey( name );

        return true;
    }
}