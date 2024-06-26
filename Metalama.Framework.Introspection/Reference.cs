// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Introspection;

public readonly struct Reference
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