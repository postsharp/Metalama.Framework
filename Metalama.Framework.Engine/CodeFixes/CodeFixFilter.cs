// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// A predicate that determines whether code fixes should be captured for a specific diagnostic and location.
    /// </summary>
    internal delegate bool CodeFixFilter( IDiagnosticDefinition diagnosticDefinition, Location location );
}