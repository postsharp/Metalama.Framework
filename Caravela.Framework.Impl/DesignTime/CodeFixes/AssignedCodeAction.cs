// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    /// <summary>
    /// Represents a <see cref="CodeAction"/> and the set of diagnostics to which it is attached. 
    /// </summary>
    internal record AssignedCodeAction( CodeAction CodeAction, ImmutableArray<Diagnostic> Diagnostic );
}