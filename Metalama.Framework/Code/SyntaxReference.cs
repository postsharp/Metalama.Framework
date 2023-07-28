// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Code;

/// <summary>
/// Represents a syntax node or a syntax token. 
/// </summary>
[CompileTime]
public readonly struct SyntaxReference
{
    private readonly ISyntaxReferenceImpl _syntaxReferenceImpl;

    /// <summary>
    /// Gets the Roslyn <c>SyntaxNode</c>, <c>SyntaxToken</c>.
    /// </summary>
    public object NodeOrToken { get; }

    /// <summary>
    /// Gets the <c>SyntaxKind</c> of the node or token.
    /// </summary>
    public string Kind => this._syntaxReferenceImpl.GetKind( this );

    internal SyntaxReference( object nodeOrToken, ISyntaxReferenceImpl syntaxReferenceImpl )
    {
        this.NodeOrToken = nodeOrToken;
        this._syntaxReferenceImpl = syntaxReferenceImpl;
    }

    /// <summary>
    /// Gets the location of the node.
    /// </summary>
    public IDiagnosticLocation DiagnosticLocation => this._syntaxReferenceImpl.GetDiagnosticLocation( this );

    /// <summary>
    /// Gets the content of the node or token (without trivia).
    /// </summary>
    public override string ToString() => this.NodeOrToken?.ToString() ?? "null";
}