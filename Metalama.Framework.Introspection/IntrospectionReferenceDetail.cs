using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Introspection;

public readonly struct IntrospectionReferenceDetail : IDiagnosticLocation
{
    public IIntrospectionReference Reference { get; }

    public ReferenceKinds Kinds { get; }

    public SourceReference Source { get; }

    internal IntrospectionReferenceDetail( IIntrospectionReference reference, ReferenceKinds kinds, SourceReference source )
    {
        this.Reference = reference;
        this.Kinds = kinds;
        this.Source = source;
    }
}