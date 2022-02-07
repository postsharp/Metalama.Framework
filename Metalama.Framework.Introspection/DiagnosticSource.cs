// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Introspection;

/// <summary>
/// Enumerates the possible sources (or originators) of an <see cref="IIntrospectionDiagnostic"/>.
/// </summary>
public enum DiagnosticSource
{
    /// <summary>
    /// The diagnostic is produced by Metalama.
    /// </summary>
    Metalama,

    /// <summary>
    /// The diagnostic is produced by the C# compiler.
    /// </summary>
    CSharp
}