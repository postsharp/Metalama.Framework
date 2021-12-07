// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.DesignTime.CodeFixes
{
    /// <summary>
    /// Represents a <see cref="Metalama.Framework.Impl.DesignTime.CodeFixes.CodeActionModel"/> and the set of diagnostics to which it is attached. 
    /// </summary>
    internal record CodeFixModel( CodeActionBaseModel CodeAction, ImmutableArray<Diagnostic> Diagnostic );
}