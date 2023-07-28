// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Code;

/// <summary>
/// Represents a syntax node in source code. 
/// </summary>
[CompileTime]
public readonly struct SourceReference
{
    private readonly ISourceReferenceImpl _sourceReferenceImpl;

    internal SourceReference( object nodeOrToken, ISourceReferenceImpl sourceReferenceImpl )
    {
        this.NodeOrToken = nodeOrToken;
        this._sourceReferenceImpl = sourceReferenceImpl;
    }

    /// <summary>
    /// Gets the Roslyn <c>SyntaxNode</c>, <c>SyntaxToken</c>.
    /// This property can be used by SDK-based plugins. 
    /// </summary>
    public object NodeOrToken { get; }

    /// <summary>
    /// Gets the <c>SyntaxKind</c> of the node or token.
    /// </summary>
    public string Kind => this._sourceReferenceImpl.GetKind( this );

    /// <summary>
    /// Gets a value indicating whether the current syntax node is contains the implementation of the declaration.
    /// This property evaluates to <c>false</c> only for partial methods without implementation.
    /// </summary>
    public bool IsImplementationPart => this._sourceReferenceImpl.IsImplementationPart( this );

    /// <summary>
    /// Gets the location where the diagnostic should be reported for the current syntax. Typically this is the identifier of the declaration,
    /// but the location can be a keyword for nodes that do not have an identifier.
    /// </summary>
    public IDiagnosticLocation DiagnosticLocation => this._sourceReferenceImpl.GetDiagnosticLocation( this );

    /// <summary>
    /// Gets source file, line and column for the node.
    /// </summary>
    public SourceSpan Span => this._sourceReferenceImpl.GetSourceSpan( this );

    /// <summary>
    /// Gets the text representation (i.e. the source code) of the current syntax node or token.
    /// </summary>
    /// <param name="normalized"><c>true</c> if whitespace should be normalized, <c>false</c> if the original formatting should be preserved.</param>
    /// <returns>The source code of the current syntax node.</returns>
    public string GetText( bool normalized = false ) => this._sourceReferenceImpl.GetText( this, normalized );

    /// <summary>
    /// Gets the content of the node or token (without trivia).
    /// </summary>
    public override string ToString() => this.NodeOrToken?.ToString() ?? "null";
}