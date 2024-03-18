// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes
{
    /// <summary>
    /// Represents a code action and the set of diagnostics to which it is attached. 
    /// </summary>
    public sealed record CodeFixModel( ICodeActionModel CodeAction, ImmutableArray<Diagnostic> Diagnostic );

    public interface ICodeActionModel;
}