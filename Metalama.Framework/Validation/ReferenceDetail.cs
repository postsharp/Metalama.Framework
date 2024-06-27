// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Validation;

/// <summary>
/// Represents a single reference in a <see cref="ReferenceValidationContext"/>. This class is exposed by the <see cref="ReferenceValidationContext.Details"/> property.
/// </summary>
[CompileTime]
public readonly struct ReferenceDetail
{
    private readonly ReferenceValidationContext _context;

    internal object NodeOrToken { get; }

    internal object Symbol { get; }

    internal ReferenceDetail( ReferenceValidationContext context, object nodeOrToken, object symbol, ReferenceKinds referenceKind )
    {
        this._context = context;
        this.NodeOrToken = nodeOrToken;
        this.Symbol = symbol;
        this.ReferenceKind = referenceKind;
    }

    /// <summary>
    /// Gets the referencing declaration (i.e. the one containing the reference).
    /// </summary>
    public IDeclaration ReferencingDeclaration => this._context.ResolveDeclaration( this );

    /// <summary>
    /// Gets the location where diagnostics should be reported.
    /// </summary>
    [PublicAPI]
    public IDiagnosticLocation? DiagnosticLocation => this._context.ResolveLocation( this );

    /// <summary>
    /// Gets a reference of the source code of this reference.
    /// </summary>
    public SourceReference Source => new( this.NodeOrToken, this._context.SourceReferenceImpl );

    /// <summary>
    /// Gets the <see cref="ReferenceKinds"/>. This property returns a single flag of the enum.
    /// </summary>
    public ReferenceKinds ReferenceKind { get; }

    /// <summary>
    /// Gets an object allowing to report diagnostics on this reference instance.
    /// </summary>
    public ScopedDiagnosticSink Diagnostics
        => new( this._context.Diagnostics.Sink, this._context.DiagnosticSource, this.DiagnosticLocation, this.ReferencingDeclaration );
}