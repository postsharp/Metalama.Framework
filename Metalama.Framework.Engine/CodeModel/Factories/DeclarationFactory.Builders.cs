// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Factories;

public partial class DeclarationFactory
{
    private readonly Cache<IDeclarationBuilder, IDeclaration> _builderCache;

    private readonly record struct CreateFromBuilderArgs<TBuilder>( TBuilder Builder, GenericContext GenericContext, DeclarationFactory Factory )
    {
        public CompilationModel Compilation => this.Factory._compilationModel;
    }

    private delegate TDeclaration CreateFromBuilderDelegate<out TDeclaration, TBuilder>( in CreateFromBuilderArgs<TBuilder> args );

    private TDeclaration GetDeclarationFromBuilder<TDeclaration, TBuilder>(
        TBuilder builder,
        IGenericContext? genericContext,
        CreateFromBuilderDelegate<TDeclaration, TBuilder> createBuiltDeclaration,
        bool supportsRedirection = false )
        where TDeclaration : class, IDeclaration
        where TBuilder : IDeclarationBuilder, TDeclaration
    {
        using ( StackOverflowHelper.Detect() )
        {
            return (TDeclaration) this._builderCache.GetOrAdd(
                builder,
                genericContext.AsGenericContext(),
                typeof(TDeclaration),
                static ( _, gc, x ) =>
                {
                    if ( x.supportsRedirection && x.me._compilationModel.TryGetRedirectedDeclaration(
                            x.builder.ToRef(),
                            out var redirected ) )
                    {
                        // It's normal that redirections redirect to builders that have the same reference!
                        // Beware not to enable redirections for this call, as this would cause an infinite recursion.
                        return x.me.GetDeclarationFromBuilder( (TBuilder) redirected, gc, x.createBuiltDeclaration );
                    }

                    return x.createBuiltDeclaration( new CreateFromBuilderArgs<TBuilder>( x.builder, gc, x.me ) );
                },
                (me: this, builder, createBuiltDeclaration, supportsRedirection) );
        }
    }

    internal IAttribute GetAttribute( AttributeBuilder attributeBuilder, IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IAttribute, AttributeBuilder>(
            attributeBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<AttributeBuilder> args ) => new BuiltAttribute( args.Builder, args.Compilation, args.GenericContext ) );

    private IParameter GetParameter(
        BaseParameterBuilder parameterBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IParameter, BaseParameterBuilder>(
            parameterBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<BaseParameterBuilder> args ) => new BuiltParameter( args.Builder, args.Compilation, args.GenericContext ) );

    internal ITypeParameter GetTypeParameter(
        TypeParameterBuilder typeParameterBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<ITypeParameter, TypeParameterBuilder>(
            typeParameterBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<TypeParameterBuilder> args ) => new BuiltTypeParameter( args.Builder, args.Compilation, args.GenericContext ) );

    internal IMethod GetMethod(
        MethodBuilder methodBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IMethod, MethodBuilder>(
            methodBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<MethodBuilder> args ) => new BuiltMethod( args.Builder, args.Compilation, args.GenericContext ) );

    internal IMethod GetAccessor( AccessorBuilder methodBuilder, IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IMethod, AccessorBuilder>(
            methodBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<AccessorBuilder> args ) =>
                ((IHasAccessors) args.Factory.GetDeclaration( args.Builder.ContainingMember, args.GenericContext )).AssertNotNull()
                .GetAccessor( args.Builder.MethodKind )
                .AssertNotNull() );

    internal IConstructor GetConstructor(
        ConstructorBuilder constructorBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IConstructor, ConstructorBuilder>(
            constructorBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<ConstructorBuilder> args ) => new BuiltConstructor( args.Builder, args.Compilation, args.GenericContext ),
            true );

    internal IField GetField(
        IFieldBuilder fieldBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IField, IFieldBuilder>(
            fieldBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<IFieldBuilder> args ) => new BuiltField( args.Builder, args.Compilation, args.GenericContext ),
            true );

    internal IProperty GetProperty(
        PropertyBuilder propertyBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IProperty, PropertyBuilder>(
            propertyBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<PropertyBuilder> args ) => new BuiltProperty( args.Builder, args.Compilation, args.GenericContext ) );

    internal IIndexer GetIndexer(
        IndexerBuilder indexerBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IIndexer, IndexerBuilder>(
            indexerBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<IndexerBuilder> args ) => new BuiltIndexer( args.Builder, args.Compilation, args.GenericContext ) );

    internal IEvent GetEvent(
        EventBuilder eventBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IEvent, EventBuilder>(
            eventBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<EventBuilder> args ) => new BuiltEvent( args.Builder, args.Compilation, args.GenericContext ) );

    internal INamedType GetNamedType(
        NamedTypeBuilder namedTypeBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<INamedType, NamedTypeBuilder>(
            namedTypeBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<NamedTypeBuilder> args ) => new BuiltNamedType( args.Builder, args.Compilation, args.GenericContext ) );

    internal INamespace GetNamespace(
        NamespaceBuilder namespaceBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<INamespace, NamespaceBuilder>(
            namespaceBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<NamespaceBuilder> args ) => new BuiltNamespace( args.Builder, args.Compilation ) );

    internal IDeclaration GetDeclaration(
        IDeclarationBuilder builder,
        IGenericContext? genericContext = null,
        Type? interfaceType = null )
    {
        // Note that interfaceType may be a non-final interface, e.g. IFieldOrProperty.
        Invariant.Assert( interfaceType == null || builder.DeclarationKind.GetPossibleDeclarationInterfaceTypes().Any( interfaceType.IsAssignableFrom ) );

        return builder switch
        {
            // TODO PERF: switch based on DeclarationKind or use an array of delegates.

            MethodBuilder methodBuilder => this.GetMethod( methodBuilder, genericContext ),
            IFieldBuilder fieldBuilder when interfaceType == null || interfaceType != typeof(IProperty) => this.GetField( fieldBuilder, genericContext ),
            IFieldBuilder fieldBuilder when interfaceType == typeof(IProperty) => this.GetProperty( (PropertyBuilder) fieldBuilder, genericContext ),
            PropertyBuilder propertyBuilder when interfaceType == null || interfaceType != typeof(IField) =>
                this.GetProperty( propertyBuilder, genericContext ),
            PropertyBuilder propertyBuilder when interfaceType == typeof(IField) => this.GetField( (IFieldBuilder) propertyBuilder, genericContext ),
            IndexerBuilder indexerBuilder => this.GetIndexer( indexerBuilder, genericContext ),
            EventBuilder eventBuilder => this.GetEvent( eventBuilder, genericContext ),
            BaseParameterBuilder parameterBuilder => this.GetParameter( parameterBuilder, genericContext ),
            AttributeBuilder attributeBuilder => this.GetAttribute( attributeBuilder, genericContext ),
            TypeParameterBuilder genericParameterBuilder => this.GetTypeParameter( genericParameterBuilder, genericContext ),
            AccessorBuilder accessorBuilder => this.GetAccessor( accessorBuilder, genericContext ),
            ConstructorBuilder constructorBuilder => this.GetConstructor( constructorBuilder, genericContext ),
            NamedTypeBuilder namedTypeBuilder => this.GetNamedType( namedTypeBuilder, genericContext ),
            NamespaceBuilder namespaceBuilder => this.GetNamespace( namespaceBuilder, genericContext ),

            // This is for linker tests (fake builders), which resolve to themselves.
            // ReSharper disable once SuspiciousTypeConversion.Global
            ISdkRef<IDeclaration> reference => reference.GetTarget( this._compilationModel ).AssertNotNull(),
            _ => throw new AssertionFailedException( $"Cannot get a declaration for a {builder.GetType()}" )
        };
    }
}