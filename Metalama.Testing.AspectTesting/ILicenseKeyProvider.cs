// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Testing.AspectTesting;

public interface ILicenseKeyProvider
{
    bool TryGetLicenseKey( string name, out string? licenseKey );
}