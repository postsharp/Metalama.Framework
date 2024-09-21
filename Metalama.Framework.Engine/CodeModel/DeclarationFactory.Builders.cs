﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.CodeModel;

public partial class DeclarationFactory
{
    private readonly ConcurrentDictionary<BuilderCacheKey, IDeclaration> _builderCache = new();

    internal IAttribute GetAttribute( AttributeBuilder attributeBuilder, ReferenceResolutionOptions options = default, IGenericContext? genericContext = null )
        => (IAttribute) this._builderCache.GetOrAdd(
            new BuilderCacheKey( attributeBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltAttribute( x.attributeBuilder, x.me._compilationModel, key.GenericContext ),
            (me: this, attributeBuilder) );

    private static Exception CreateBuilderNotExists( IDeclarationBuilder builder )
        => new InvalidOperationException( $"The declaration '{builder}' does not exist in the current compilation." );

    private IParameter? GetParameter(
        BaseParameterBuilder parameterBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
    {
        return (IParameter) this._builderCache.GetOrAdd(
            new BuilderCacheKey( parameterBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltParameter( x.parameterBuilder, x.me._compilationModel, key.GenericContext ),
            (me: this, parameterBuilder) );
    }

    internal ITypeParameter GetGenericParameter(
        TypeParameterBuilder typeParameterBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null )
        => (ITypeParameter) this._builderCache.GetOrAdd(
            new BuilderCacheKey( typeParameterBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltTypeParameter( x.typeParameterBuilder, x.me._compilationModel, key.GenericContext ),
            (me: this, typeParameterBuilder) );

    internal IMethod? GetMethod(
        MethodBuilder methodBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
    {
        return (IMethod) this._builderCache.GetOrAdd(
            new BuilderCacheKey( methodBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltMethod( x.methodBuilder, x.me._compilationModel, key.GenericContext ),
            (me: this, methodBuilder) );
    }

    internal IMethod GetAccessor( AccessorBuilder methodBuilder, ReferenceResolutionOptions options = default, IGenericContext? genericContext = null )
        => (IMethod) this._builderCache.GetOrAdd(
            new BuilderCacheKey( methodBuilder, genericContext.AsGenericContext() ),
            static ( key, x )
                => ((IHasAccessors) x.me.GetDeclaration( x.methodBuilder.ContainingMember, x.options, key.GenericContext ).AssertNotNull()).GetAccessor(
                    x.methodBuilder.MethodKind )!,
            (me: this, options, methodBuilder) );

    internal IConstructor? GetConstructor(
        ConstructorBuilder constructorBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
    {
        return (IConstructor) this._builderCache.GetOrAdd(
            new BuilderCacheKey( constructorBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltConstructor( x.constructorBuilder, x.me._compilationModel, key.GenericContext ),
            (me: this, constructorBuilder) );
    }

    internal IField? GetField(
        FieldBuilder fieldBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
    {
        return (IField) this._builderCache.GetOrAdd(
            new BuilderCacheKey( fieldBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltField( x.fieldBuilder, x.me._compilationModel, key.GenericContext ),
            (me: this, fieldBuilder) );
    }

    internal IProperty? GetProperty(
        PropertyBuilder propertyBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
    {
        return (IProperty) this._builderCache.GetOrAdd(
            new BuilderCacheKey( propertyBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltProperty( x.me._compilationModel, x.propertyBuilder, key.GenericContext ),
            (me: this, propertyBuilder) );
    }

    internal IIndexer? GetIndexer(
        IndexerBuilder indexerBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
    {
        return (IIndexer) this._builderCache.GetOrAdd(
            new BuilderCacheKey( indexerBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltIndexer( x.me._compilationModel, x.indexerBuilder, key.GenericContext ),
            (me: this, indexerBuilder) );
    }

    internal IEvent? GetEvent(
        EventBuilder eventBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
    {
        return (IEvent) this._builderCache.GetOrAdd(
            new BuilderCacheKey( eventBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltEvent( x.me._compilationModel, x.eventBuilder, key.GenericContext ),
            (me: this, eventBuilder) );
    }

    internal INamedType? GetNamedType(
        NamedTypeBuilder namedTypeBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
    {
        return (INamedType) this._builderCache.GetOrAdd(
            new BuilderCacheKey( namedTypeBuilder, genericContext.AsGenericContext() ),
            static ( key, x ) => new BuiltNamedType( x.me._compilationModel, x.namedTypeBuilder, key.GenericContext ),
            (me: this, namedTypeBuilder) );
    }

    internal INamespace? GetNamespace(
        NamespaceBuilder namespaceBuilder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
    {
        return (INamespace) this._builderCache.GetOrAdd(
            new BuilderCacheKey( namespaceBuilder, genericContext.AsGenericContext() ),
            static ( _, x ) => new BuiltNamespace( x.me._compilationModel, x.namespaceBuilder ),
            (me: this, namespaceBuilder) );
    }

    internal IDeclaration? GetDeclaration(
        IDeclarationBuilder builder,
        ReferenceResolutionOptions options = default,
        IGenericContext? genericContext = null,
        bool throwIfMissing = true )
        => builder switch
        {
            MethodBuilder methodBuilder => this.GetMethod( methodBuilder, options, genericContext, throwIfMissing ),
            FieldBuilder fieldBuilder => this.GetField( fieldBuilder, options, genericContext, throwIfMissing ),
            PropertyBuilder propertyBuilder => this.GetProperty( propertyBuilder, options, genericContext, throwIfMissing ),
            IndexerBuilder indexerBuilder => this.GetIndexer( indexerBuilder, options, genericContext, throwIfMissing ),
            EventBuilder eventBuilder => this.GetEvent( eventBuilder, options, genericContext, throwIfMissing ),
            BaseParameterBuilder parameterBuilder => this.GetParameter( parameterBuilder, options, genericContext, throwIfMissing ),
            AttributeBuilder attributeBuilder => this.GetAttribute( attributeBuilder, options, genericContext ),
            TypeParameterBuilder genericParameterBuilder => this.GetGenericParameter( genericParameterBuilder, options, genericContext ),
            AccessorBuilder accessorBuilder => this.GetAccessor( accessorBuilder, options, genericContext ),
            ConstructorBuilder constructorBuilder => this.GetConstructor( constructorBuilder, options, genericContext, throwIfMissing ),
            NamedTypeBuilder namedTypeBuilder => this.GetNamedType( namedTypeBuilder, options, genericContext, throwIfMissing ),
            NamespaceBuilder namespaceBuilder => this.GetNamespace( namespaceBuilder, options, genericContext, throwIfMissing ),

            // This is for linker tests (fake builders), which resolve to themselves.
            // ReSharper disable once SuspiciousTypeConversion.Global
            ISdkRef<IDeclaration> reference => reference.GetTarget( this._compilationModel ).AssertNotNull(),
            _ => throw new AssertionFailedException( $"Cannot get a declaration for a {builder.GetType()}" )
        };
}