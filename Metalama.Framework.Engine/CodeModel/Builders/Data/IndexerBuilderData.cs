using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal class IndexerBuilderData : PropertyOrIndexerBuilderData
{
    public ImmutableArray<ParameterBuilderData> Parameters { get; }
    public IRef<IIndexer>? OverriddenIndexer { get; }
    public IReadOnlyList<IRef<IIndexer>> ExplicitInterfaceImplementation { get; }


    public IndexerBuilderData( IndexerBuilder builder, IRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration )
    {
        var me = this.ToDeclarationRef();
        this.Parameters = builder.Parameters.ToImmutable(me);
        this.OverriddenIndexer = builder.OverriddenIndexer?.ToRef();
        this.ExplicitInterfaceImplementation = builder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => i.ToRef() );

    }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Indexer;

    public override IRef<IMember>? OverriddenMember => throw new NotImplementedException();


    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => this.ExplicitInterfaceImplementation;
}