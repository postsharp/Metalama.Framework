using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Introspection;

public readonly struct IntrospectionReferenceDetail : IDiagnosticLocation
{
    public IIntrospectionDeclarationReference DeclarationReference { get; }

    public ReferenceKinds Kinds { get; }

    public SourceReference Source { get; }

    internal IntrospectionReferenceDetail( IIntrospectionDeclarationReference declarationReference, ReferenceKinds kinds, SourceReference source )
    {
        this.DeclarationReference = declarationReference;
        this.Kinds = kinds;
        this.Source = source;
    }
}