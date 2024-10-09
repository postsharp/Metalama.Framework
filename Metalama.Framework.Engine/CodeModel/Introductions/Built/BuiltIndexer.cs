﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltIndexer : BuiltPropertyOrIndexer, IIndexerImpl
{
    private readonly IndexerBuilderData _indexerBuilder;

    public BuiltIndexer( IndexerBuilderData builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this._indexerBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this._indexerBuilder;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this._indexerBuilder;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder => this._indexerBuilder;

    protected override MemberBuilderData MemberBuilder => this._indexerBuilder;

    public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    protected override PropertyOrIndexerBuilderData PropertyOrIndexerBuilder => this._indexerBuilder;

    [Memo]
    public IParameterList Parameters
        => new ParameterList(
            this,
            this.Compilation.GetParameterCollection( this.Ref ) );

    [Memo]
    public IIndexer? OverriddenIndexer => this.MapDeclaration( this._indexerBuilder.OverriddenIndexer );

    [Memo]
    public IIndexer Definition => this.Compilation.Factory.GetIndexer( this._indexerBuilder ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    [Memo]
    private IFullRef<IIndexer> Ref => this.RefFactory.FromBuilt<IIndexer>( this );

    public IRef<IIndexer> ToRef() => this.Ref;

    public override IFullRef<IMember> ToMemberFullRef() => this.Ref;

    private protected override IFullRef<IDeclaration> ToFullDeclarationRef() => this.Ref;

    public IIndexerInvoker With( InvokerOptions options ) => new IndexerInvoker( this, options );

    public IIndexerInvoker With( object? target, InvokerOptions options = default ) => new IndexerInvoker( this, options, target );

    public object GetValue( params object?[] args ) => new IndexerInvoker( this ).GetValue( args );

    public object SetValue( object? value, params object?[] args ) => new IndexerInvoker( this ).SetValue( value, args );

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations
        => this._indexerBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( this.MapDeclaration );
}