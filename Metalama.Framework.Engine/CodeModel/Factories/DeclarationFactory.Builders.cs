// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.Introductions.Built;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Factories;

public partial class DeclarationFactory
{
    private readonly Cache<DeclarationBuilderData, IDeclaration> _builderCache;

    private readonly record struct CreateFromBuilderArgs<TBuilderData>( TBuilderData Builder, GenericContext GenericContext, DeclarationFactory Factory )
    {
        public CompilationModel Compilation => this.Factory._compilationModel;
    }

    private delegate TDeclaration CreateFromBuilderDelegate<out TDeclaration, TBuilder>( in CreateFromBuilderArgs<TBuilder> args );

    private TDeclaration GetDeclarationFromBuilder<TDeclaration, TBuilderData>(
        TBuilderData builder,
        IGenericContext? genericContext,
        CreateFromBuilderDelegate<TDeclaration, TBuilderData> createBuiltDeclaration,
        bool supportsRedirection = false )
        where TDeclaration : class, IDeclaration
        where TBuilderData : DeclarationBuilderData
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
                        return x.me.GetDeclarationFromBuilder( (TBuilderData) redirected, gc, x.createBuiltDeclaration );
                    }

                    return x.createBuiltDeclaration( new CreateFromBuilderArgs<TBuilderData>( x.builder, gc, x.me ) );
                },
                (me: this, builder, createBuiltDeclaration, supportsRedirection) );
        }
    }

    internal IAttribute GetAttribute( AttributeBuilderData attributeBuilder, IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IAttribute, AttributeBuilderData>(
            attributeBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<AttributeBuilderData> args ) => new BuiltAttribute( args.Builder, args.Compilation, args.GenericContext ) );

    private IParameter GetParameter(
        ParameterBuilderData parameterBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IParameter, ParameterBuilderData>(
            parameterBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<ParameterBuilderData> args ) => new BuiltParameter( args.Builder, args.Compilation, args.GenericContext ) );

    internal ITypeParameter GetTypeParameter(
        TypeParameterBuilderData typeParameterBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<ITypeParameter, TypeParameterBuilderData>(
            typeParameterBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<TypeParameterBuilderData> args ) => new BuiltTypeParameter( args.Builder, args.Compilation, args.GenericContext ) );

    internal IMethod GetMethod(
        MethodBuilderData methodBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IMethod, MethodBuilderData>(
            methodBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<MethodBuilderData> args ) => new BuiltMethod( args.Builder, args.Compilation, args.GenericContext ) );

    internal IMethod GetAccessor( MethodBuilderData methodBuilder, IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IMethod, MethodBuilderData>(
            methodBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<MethodBuilderData> args ) =>
                ((IHasAccessors) args.Builder.ContainingDeclaration.GetTarget( args.Compilation, args.GenericContext ))
                .GetAccessor( args.Builder.MethodKind )
                .AssertNotNull() );

    internal IConstructor GetConstructor(
        ConstructorBuilderData constructorBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IConstructor, ConstructorBuilderData>(
            constructorBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<ConstructorBuilderData> args ) => new BuiltConstructor( args.Builder, args.Compilation, args.GenericContext ),
            true );

    internal IField GetField(
        FieldBuilderData fieldBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IField, FieldBuilderData>(
            fieldBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<FieldBuilderData> args ) => new BuiltField( args.Builder, args.Compilation, args.GenericContext ),
            true );

    internal IProperty GetProperty(
        PropertyBuilderData propertyBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IProperty, PropertyBuilderData>(
            propertyBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<PropertyBuilderData> args ) => new BuiltProperty( args.Builder, args.Compilation, args.GenericContext ) );

    internal IIndexer GetIndexer(
        IndexerBuilderData indexerBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IIndexer, IndexerBuilderData>(
            indexerBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<IndexerBuilderData> args ) => new BuiltIndexer( args.Builder, args.Compilation, args.GenericContext ) );

    internal IEvent GetEvent(
        EventBuilderData eventBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<IEvent, EventBuilderData>(
            eventBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<EventBuilderData> args ) => new BuiltEvent( args.Builder, args.Compilation, args.GenericContext ) );

    internal INamedType GetNamedType(
        NamedTypeBuilderData namedTypeBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<INamedType, NamedTypeBuilderData>(
            namedTypeBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<NamedTypeBuilderData> args ) => new BuiltNamedType( args.Builder, args.Compilation, args.GenericContext ) );

    internal INamespace GetNamespace(
        NamespaceBuilderData namespaceBuilder,
        IGenericContext? genericContext = null )
        => this.GetDeclarationFromBuilder<INamespace, NamespaceBuilderData>(
            namespaceBuilder,
            genericContext,
            static ( in CreateFromBuilderArgs<NamespaceBuilderData> args ) => new BuiltNamespace( args.Builder, args.Compilation ) );

    internal IDeclaration GetDeclaration(
        DeclarationBuilderData builder,
        IGenericContext? genericContext = null,
        Type? interfaceType = null )
    {
        // Note that interfaceType may be a non-final interface, e.g. IFieldOrProperty.
        Invariant.Assert( interfaceType == null || builder.DeclarationKind.GetPossibleDeclarationInterfaceTypes().Any( interfaceType.IsAssignableFrom ) );

        return builder switch
        {
            // TODO PERF: switch based on DeclarationKind or use an array of delegates.

            MethodBuilderData { ContainingDeclaration.DeclarationKind: DeclarationKind.NamedType } methodBuilder => this.GetMethod( methodBuilder, genericContext ),
            MethodBuilderData { ContainingDeclaration.DeclarationKind: DeclarationKind.Property or DeclarationKind.Event or DeclarationKind.Field } accessorBuilder =>this.GetAccessor( accessorBuilder, genericContext ),
            FieldBuilderData fieldBuilder when interfaceType == null || interfaceType != typeof(IProperty) => this.GetField( fieldBuilder, genericContext ),
            FieldBuilderData fieldBuilder when interfaceType == typeof(IProperty) => fieldBuilder.OverridingProperty.AssertNotNull().GetTarget(this._compilationModel,genericContext),
            PropertyBuilderData propertyBuilder when interfaceType == null || interfaceType != typeof(IField) =>
                this.GetProperty( propertyBuilder, genericContext ),
            PropertyBuilderData propertyBuilder when interfaceType == typeof(IField) => propertyBuilder.GetOriginalField( this._compilationModel, (GenericContext?) genericContext ?? GenericContext.Empty),
            IndexerBuilderData indexerBuilder => this.GetIndexer( indexerBuilder, genericContext ),
            EventBuilderData eventBuilder => this.GetEvent( eventBuilder, genericContext ),
            ParameterBuilderData parameterBuilder => this.GetParameter( parameterBuilder, genericContext ),
            AttributeBuilderData attributeBuilder => this.GetAttribute( attributeBuilder, genericContext ),
            TypeParameterBuilderData genericParameterBuilder => this.GetTypeParameter( genericParameterBuilder, genericContext ),
            ConstructorBuilderData constructorBuilder => this.GetConstructor( constructorBuilder, genericContext ),
            NamedTypeBuilderData namedTypeBuilder => this.GetNamedType( namedTypeBuilder, genericContext ),
            NamespaceBuilderData namespaceBuilder => this.GetNamespace( namespaceBuilder, genericContext ),

            // This is for linker tests (fake builders), which resolve to themselves.
            // ReSharper disable once SuspiciousTypeConversion.Global
            ISdkRef reference => (IDeclaration) reference.GetTarget( this._compilationModel ).AssertNotNull(),
            _ => throw new AssertionFailedException( $"Cannot get a declaration for a {builder.GetType()}" )
        };
    }
}