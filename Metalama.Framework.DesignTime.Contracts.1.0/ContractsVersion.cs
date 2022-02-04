// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    public const int ContractVersion_1_0 = 1;
}