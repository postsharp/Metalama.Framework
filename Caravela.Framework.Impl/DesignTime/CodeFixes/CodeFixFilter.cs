// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    /// <summary>
    /// A predicate that determines whether code fixes should be captured for a specific diagnostic and location.
    /// </summary>
    internal delegate bool CodeFixFilter( IDiagnosticDefinition diagnosticDefinition, Location location );
}