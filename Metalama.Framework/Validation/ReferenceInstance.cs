// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Validation;

[CompileTime]
public readonly struct ReferenceInstance
{
    private readonly ReferenceValidationContext _context;

    internal object NodeOrToken { get; }

    internal object Symbol { get; }

    internal ReferenceInstance( ReferenceValidationContext context, object nodeOrToken, object symbol, ReferenceKinds referenceKinds )
    {
        this._context = context;
        this.NodeOrToken = nodeOrToken;
        this.Symbol = symbol;
        this.ReferenceKinds = referenceKinds;
    }

    public IDeclaration ReferencingDeclaration => this._context.ResolveDeclaration( this );

    public IDiagnosticLocation? DiagnosticLocation => this._context.ResolveLocation( this );

    public SourceReference Source => new( this.NodeOrToken, this._context.SourceReferenceImpl );

    public ReferenceKinds ReferenceKinds { get; }

    public ScopedDiagnosticSink Diagnostics
        => new( this._context.Diagnostics.Sink, this._context.DiagnosticSource, this.DiagnosticLocation, this.ReferencingDeclaration );
}