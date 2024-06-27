using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Introspection;

public readonly struct Reference : IDiagnosticLocation
{
    public IDeclarationReference DeclarationReference { get; }

    public ReferenceKinds Kinds { get; }

    public SourceReference Source { get; }

    internal Reference( IDeclarationReference declarationReference, ReferenceKinds kinds, SourceReference source )
    {
        this.DeclarationReference = declarationReference;
        this.Kinds = kinds;
        this.Source = source;
    }
}