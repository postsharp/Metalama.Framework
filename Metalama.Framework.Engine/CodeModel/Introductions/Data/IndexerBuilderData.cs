// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.Introductions.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class IndexerBuilderData : PropertyOrIndexerBuilderData
{
    private readonly IRef<IIndexer> _ref;

    public ImmutableArray<ParameterBuilderData> Parameters { get; }

    public IRef<IIndexer>? OverriddenIndexer { get; }

    public IReadOnlyList<IRef<IIndexer>> ExplicitInterfaceImplementations { get; }

    public IndexerBuilderData( IndexerBuilder builder, IRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new DeclarationBuilderDataRef<IIndexer>( this );
        this.Parameters = builder.Parameters.ToImmutable( this._ref );
        this.OverriddenIndexer = builder.OverriddenIndexer?.ToRef();
        this.ExplicitInterfaceImplementations = builder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => i.ToRef() );

        if ( builder.GetMethod != null )
        {
            this.GetMethod = new MethodBuilderData( builder.GetMethod, this._ref );
        }

        if ( builder.SetMethod != null )
        {
            this.SetMethod = new MethodBuilderData( builder.SetMethod, this._ref );
        }
    }

    protected override IRef<IDeclaration> ToDeclarationRef() => this._ref;

    public new IRef<IIndexer> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Indexer;

    public override IRef<IMember>? OverriddenMember => throw new NotImplementedException();

    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => this.ExplicitInterfaceImplementations;

    public override MethodBuilderData? GetMethod { get; }

    public override MethodBuilderData? SetMethod { get; }

    public override IEnumerable<DeclarationBuilderData> GetOwnedDeclarations()
    {
        return base.GetOwnedDeclarations().Concat( this.Parameters );
    }
}