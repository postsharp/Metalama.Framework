// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable InconsistentNaming

#pragma warning disable SA1310

namespace Metalama.Framework.DesignTime.Contracts;

/// <summary>
/// Exposes the <see cref="ContractVersion_1_0"/> constant, which is used to differentiate versions of the API between pre-releases.
/// This class intentionally only exposes <i>constants</i> so they are copied in the caller code during compilation.
/// </summary>
public static class ContractsVersion
{
    /// <summary>
    /// Gets the current version of the 1.0 contracts.
    /// </summary>
    public const int ContractVersion_1_0 = 2;
}