// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class IndexerBuilder : PropertyOrIndexerBuilder, IIndexerBuilder, IIndexerImpl
{
    public ParameterBuilderList Parameters { get; } = new();

    protected override IRef<IFieldOrPropertyOrIndexer> ToFieldOrPropertyOrIndexerRef() => this.Ref;

    public override Writeability Writeability
    {
        get
            => this switch
            {
                { SetMethod: null } => Writeability.None,
                { HasInitOnlySetter: true } => Writeability.InitOnly,
                _ => Writeability.All
            };

        set
        {
            switch (this, value)
            {
                case ({ SetMethod: not null }, Writeability.All):
                    this.HasInitOnlySetter = false;

                    break;

                case ({ SetMethod: not null }, Writeability.InitOnly):
                    this.HasInitOnlySetter = true;

                    break;

                default:
                    throw new InvalidOperationException(
                        $"Writeability can only be set for indexers with a setter to either {Writeability.InitOnly} or {Writeability.All}." );
            }
        }
    }

    IParameterList IHasParameters.Parameters => this.Parameters;

    IParameterBuilderList IHasParametersBuilder.Parameters => this.Parameters;

    public IIndexer? OverriddenIndexer { get; set; }

    IIndexer IIndexer.Definition => this;

    public IIndexerInvoker With( InvokerOptions options ) => new IndexerInvoker( this, options );

    public IIndexerInvoker With( object? target, InvokerOptions options = default ) => new IndexerInvoker( this, options, target );

    public object GetValue( params object?[] args ) => new IndexerInvoker( this ).GetValue( args );

    public object SetValue( object? value, params object?[] args ) => new IndexerInvoker( this ).SetValue( value, args );

    public override DeclarationKind DeclarationKind => DeclarationKind.Indexer;

    public IReadOnlyList<IIndexer> ExplicitInterfaceImplementations { get; private set; } = Array.Empty<IIndexer>();

    public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    public override IMember? OverriddenMember => this.OverriddenIndexer;

    public override IRef<IMember> ToMemberRef() => this.Ref;

    public IInjectMemberTransformation ToTransformation() => new IntroduceIndexerTransformation( this.ParentAdvice, this );

    public IndexerBuilder(
        Advice advice,
        INamedType targetType,
        bool hasGetter,
        bool hasSetter )
        : base( advice, targetType, "this[]", hasGetter, hasSetter, false, false )
    {
        Invariant.Assert( hasGetter || hasSetter );

        this.HasInitOnlySetter = false;
    }

    public IParameterBuilder AddParameter( string name, IType type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = default )
    {
        this.CheckNotFrozen();

        var parameter = new ParameterBuilder( this, this.Parameters.Count, name, type, refKind, this.ParentAdvice );
        parameter.DefaultValue = defaultValue;
        this.Parameters.Add( parameter );

        return parameter;
    }

    public IParameterBuilder AddParameter( string name, Type type, RefKind refKind = RefKind.None, TypedConstant? defaultValue = default )
    {
        this.CheckNotFrozen();

        var iType = this.Compilation.Factory.GetTypeByReflectionType( type );
        var typeConstant = defaultValue != null ? TypedConstant.Create( defaultValue, iType ) : default;

        return this.AddParameter( name, iType, refKind, typeConstant );
    }

    public void SetExplicitInterfaceImplementation( IIndexer interfaceIndexer ) => this.ExplicitInterfaceImplementations = [interfaceIndexer];

    [Memo]
    public BuilderRef<IIndexer> Ref => new( this );

    public override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public override IRef<IPropertyOrIndexer> ToPropertyOrIndexerRef() => this.Ref;

    public new IRef<IIndexer> ToRef() => this.Ref;

    public override IRef<IMemberOrNamedType> ToMemberOrNamedTypeRef() => this.Ref;
}