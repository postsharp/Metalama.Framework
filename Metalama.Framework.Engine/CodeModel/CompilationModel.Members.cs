// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel;

public partial class CompilationModel
{
    private ImmutableDictionary<INamedTypeSymbol, FieldUpdatableCollection> _fields;
    private ImmutableDictionary<INamedTypeSymbol, MethodUpdatableCollection> _methods;
    private ImmutableDictionary<INamedTypeSymbol, ConstructorUpdatableCollection> _constructors;
    private ImmutableDictionary<INamedTypeSymbol, EventUpdatableCollection> _events;
    private ImmutableDictionary<INamedTypeSymbol, PropertyUpdatableCollection> _properties;
    private ImmutableDictionary<INamedTypeSymbol, IndexerUpdatableCollection> _indexers;
    private ImmutableDictionary<INamedTypeSymbol, InterfaceUpdatableCollection> _interfaceImplementations;

    private ImmutableDictionary<INamedTypeSymbol, IConstructorBuilder> _staticConstructors =
        ImmutableDictionary<INamedTypeSymbol, IConstructorBuilder>.Empty.WithComparers( SymbolEqualityComparer.Default );

    public bool IsMutable { get; private set; }

    private TCollection GetMemberCollection<TDeclaration, TCollection>(
        ref ImmutableDictionary<INamedTypeSymbol, TCollection> dictionary,
        bool requestMutableCollection,
        INamedTypeSymbol declaringTypeSymbol,
        Func<CompilationModel, INamedTypeSymbol, TCollection> createCollection )
        where TDeclaration : class, IMemberOrNamedType
        where TCollection : UpdatableDeclarationCollection<TDeclaration>
    {
        if ( requestMutableCollection && !this.IsMutable )
        {
            // Cannot get a mutable collection when the model is immutable.
            throw new InvalidOperationException();
        }

        // If the model is mutable, we need to return a mutable collection because it may be mutated after the
        // front-end collection is returned.
        var returnMutableCollection = requestMutableCollection || this.IsMutable;

        if ( dictionary.TryGetValue( declaringTypeSymbol, out var collection ) )
        {
            if ( collection.Compilation != this && returnMutableCollection )
            {
                // The UpdateArray was created in another compilation snapshot, so it is not mutable in the current compilation.
                // We need to take a copy of it.
                collection = (TCollection) collection.Clone( this.Compilation );
                dictionary = dictionary.SetItem( declaringTypeSymbol, collection );
            }

            return collection;
        }
        else
        {
            collection = createCollection( this.Compilation, declaringTypeSymbol );
            dictionary = dictionary.SetItem( declaringTypeSymbol, collection );
        }

        return collection;
    }

    internal FieldUpdatableCollection GetFieldCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<IField, FieldUpdatableCollection>(
            ref this._fields,
            mutable,
            declaringType,
            ( c, t ) => new FieldUpdatableCollection( c, t ) );

    internal MethodUpdatableCollection GetMethodCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<IMethod, MethodUpdatableCollection>(
            ref this._methods,
            mutable,
            declaringType,
            ( c, t ) => new MethodUpdatableCollection( c, t ) );

    internal ConstructorUpdatableCollection GetConstructorCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<IConstructor, ConstructorUpdatableCollection>(
            ref this._constructors,
            mutable,
            declaringType,
            ( c, t ) => new ConstructorUpdatableCollection( c, t ) );

    internal PropertyUpdatableCollection GetPropertyCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<IProperty, PropertyUpdatableCollection>(
            ref this._properties,
            mutable,
            declaringType,
            ( c, t ) => new PropertyUpdatableCollection( c, t ) );

    internal IndexerUpdatableCollection GetIndexerCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<IIndexer, IndexerUpdatableCollection>(
            ref this._indexers,
            mutable,
            declaringType,
            ( c, t ) => new IndexerUpdatableCollection( c, t ) );

    internal EventUpdatableCollection GetEventCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<IEvent, EventUpdatableCollection>(
            ref this._events,
            mutable,
            declaringType,
            ( c, t ) => new EventUpdatableCollection( c, t ) );

    internal InterfaceUpdatableCollection GetInterfaceImplementationCollection( INamedTypeSymbol declaringType, bool mutable )
    {
        return this.GetMemberCollection<INamedType, InterfaceUpdatableCollection>(
            ref this._interfaceImplementations,
            mutable,
            declaringType,
            ( c, t ) => new InterfaceUpdatableCollection( c, t ) );
    }

    internal IConstructorBuilder? GetStaticConstructor( INamedTypeSymbol declaringType )
    {
        this._staticConstructors.TryGetValue( declaringType, out var value );

        return value;
    }

    internal void AddTransformation( IObservableTransformation transformation )
    {
        void ApplyRemoval<T>( UpdatableMemberCollection<T> collection )
            where T : class, IMemberOrNamedType
        {
            if ( transformation is IReplaceMemberTransformation replaceMemberTransformation )
            {
                collection.Remove( replaceMemberTransformation.ReplacedMember!.Value.As<T>() );
            }
        }

        switch ( transformation )
        {
            case IMethodBuilder methodBuilder:
                var methods = this.GetMethodCollection( methodBuilder.DeclaringType.GetSymbol().AssertNotNull(), true );
                methods.Add( methodBuilder.ToMemberRef<IMethod>() );
                ApplyRemoval( methods );

                break;

            case IConstructorBuilder { IsStatic: false } constructorBuilder:
                var constructors = this.GetConstructorCollection( constructorBuilder.DeclaringType.GetSymbol().AssertNotNull(), true );
                constructors.Add( constructorBuilder.ToMemberRef<IConstructor>() );
                ApplyRemoval( constructors );

                break;

            case IConstructorBuilder { IsStatic: true } staticConstructorBuilder:
                var declaringType = staticConstructorBuilder.DeclaringType.GetSymbol().AssertNotNull();

                if ( this._staticConstructors.ContainsKey( declaringType ) )
                {
                    // Duplicate.
                    throw new AssertionFailedException();
                }

                this._staticConstructors = this._staticConstructors.SetItem( declaringType, staticConstructorBuilder );

                break;

            case IFieldBuilder fieldBuilder:
                var fields = this.GetFieldCollection( fieldBuilder.DeclaringType.GetSymbol().AssertNotNull(), true );
                fields.Add( fieldBuilder.ToMemberRef<IField>() );
                ApplyRemoval( fields );

                break;

            case IPropertyBuilder propertyBuilder:
                var properties = this.GetPropertyCollection( propertyBuilder.DeclaringType.GetSymbol().AssertNotNull(), true );
                properties.Add( propertyBuilder.ToMemberRef<IProperty>() );
                ApplyRemoval( properties );

                break;

            case IntroduceInterfaceTransformation introduceInterface:
                var interfaces = this.GetInterfaceImplementationCollection(
                    (INamedTypeSymbol) introduceInterface.ContainingDeclaration.GetSymbol().AssertNotNull(),
                    true );

                interfaces.Add( introduceInterface );

                break;

            default:
                throw new AssertionFailedException();
        }
    }
}