// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// Represents a <see cref="CodeActionModel"/> and the set of diagnostics to which it is attached. 
    /// </summary>
    public record CodeFixModel( CodeActionBaseModel CodeAction, ImmutableArray<Diagnostic> Diagnostic );
}