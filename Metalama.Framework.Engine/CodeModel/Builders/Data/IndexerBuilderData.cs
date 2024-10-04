using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders.Collections;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal class IndexerBuilderData : PropertyOrIndexerBuilderData
{
    public ImmutableArray<ParameterBuilderData> Parameters { get; }

    public IndexerBuilderData( IndexerBuilder builder, IRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration )
    {
        var me = this.ToDeclarationRef();
        this.Parameters = builder.Parameters.ToImmutable(me);
    }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Indexer;
}