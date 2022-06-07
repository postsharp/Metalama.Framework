// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.References;
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
    private ImmutableDictionary<Ref<IHasParameters>, ParameterUpdatableCollection> _parameters;

    private ImmutableDictionary<INamedTypeSymbol, IConstructorBuilder> _staticConstructors =
        ImmutableDictionary<INamedTypeSymbol, IConstructorBuilder>.Empty.WithComparers( SymbolEqualityComparer.Default );

    public bool IsMutable { get; private set; }

    private TCollection GetMemberCollection<TKey, TDeclaration, TCollection>(
        ref ImmutableDictionary<TKey, TCollection> dictionary,
        bool requestMutableCollection,
        TKey declaringTypeSymbol,
        Func<CompilationModel, TKey, TCollection> createCollection )
        where TDeclaration : class, IDeclaration
        where TCollection : UpdatableDeclarationCollection<TDeclaration>
        where TKey : notnull
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
        => this.GetMemberCollection<INamedTypeSymbol, IField, FieldUpdatableCollection>(
            ref this._fields,
            mutable,
            declaringType,
            ( c, t ) => new FieldUpdatableCollection( c, t ) );

    internal MethodUpdatableCollection GetMethodCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<INamedTypeSymbol, IMethod, MethodUpdatableCollection>(
            ref this._methods,
            mutable,
            declaringType,
            ( c, t ) => new MethodUpdatableCollection( c, t ) );

    internal ConstructorUpdatableCollection GetConstructorCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<INamedTypeSymbol, IConstructor, ConstructorUpdatableCollection>(
            ref this._constructors,
            mutable,
            declaringType,
            ( c, t ) => new ConstructorUpdatableCollection( c, t ) );

    internal PropertyUpdatableCollection GetPropertyCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<INamedTypeSymbol, IProperty, PropertyUpdatableCollection>(
            ref this._properties,
            mutable,
            declaringType,
            ( c, t ) => new PropertyUpdatableCollection( c, t ) );

    internal IndexerUpdatableCollection GetIndexerCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<INamedTypeSymbol, IIndexer, IndexerUpdatableCollection>(
            ref this._indexers,
            mutable,
            declaringType,
            ( c, t ) => new IndexerUpdatableCollection( c, t ) );

    internal EventUpdatableCollection GetEventCollection( INamedTypeSymbol declaringType, bool mutable )
        => this.GetMemberCollection<INamedTypeSymbol, IEvent, EventUpdatableCollection>(
            ref this._events,
            mutable,
            declaringType,
            ( c, t ) => new EventUpdatableCollection( c, t ) );

    internal InterfaceUpdatableCollection GetInterfaceImplementationCollection( INamedTypeSymbol declaringType, bool mutable )
    {
        return this.GetMemberCollection<INamedTypeSymbol, INamedType, InterfaceUpdatableCollection>(
            ref this._interfaceImplementations,
            mutable,
            declaringType,
            ( c, t ) => new InterfaceUpdatableCollection( c, t ) );
    }

    internal ParameterUpdatableCollection GetParameterCollection( Ref<IHasParameters> parent, bool mutable )
    {
        return this.GetMemberCollection<Ref<IHasParameters>, IParameter, ParameterUpdatableCollection>(
            ref this._parameters,
            mutable,
            parent,
            ( c, t ) => new ParameterUpdatableCollection( c, t ) );
    }

    internal IConstructorBuilder? GetStaticConstructor( INamedTypeSymbol declaringType )
    {
        this._staticConstructors.TryGetValue( declaringType, out var value );

        return value;
    }

    internal void AddTransformation( IObservableTransformation transformation )
    {
        if ( !this.IsMutable )
        {
            throw new InvalidOperationException( "Cannot add transformation to an immutable compilation." );
        }

        this.AddTransformation( this, transformation );
    }

    private void AddTransformation( CompilationModel originCompilation, IObservableTransformation transformation )
    {
        // "originCompilation" is intended for resolving references, i.e. it should be fully initialized.
        //  * For immutable compilation models, this should be the prototype.
        //  * For mutable compilation models, this should be the compilation itself.

        // Replaced declaration should be always removed before adding the replacement.
        if ( transformation is IReplaceMemberTransformation replaceMember )
        {
            this.AddReplaceMemberTransformation( originCompilation, replaceMember );
        }

        if ( transformation is IDeclarationBuilder builder )
        {
            this.AddDeclaration( builder );
        }

        if ( transformation is AppendParameterTransformation appendParameterTransformation )
        {
            this.AddDeclaration( appendParameterTransformation.Parameter );
        }

        if ( transformation is IIntroduceInterfaceTransformation introduceInterface )
        {
            this.AddIntroduceInterfaceTransformation( introduceInterface );
        }
    }

    private void AddReplaceMemberTransformation( CompilationModel originCompilation, IReplaceMemberTransformation transformation )
    {
        if ( transformation.ReplacedMember.IsDefault )
        {
            return;
        }

        var replaced = transformation.ReplacedMember;

        switch ( replaced.GetTarget( originCompilation ) )
        {
            case IConstructor { IsStatic: false } replacedConstructor:
                var constructors = this.GetConstructorCollection( replacedConstructor.DeclaringType.GetSymbol().AssertNotNull(), true );
                constructors.Remove( replaced.As<IConstructor>() );

                break;

            case IField replacedField:
                var fields = this.GetFieldCollection( replacedField.DeclaringType.GetSymbol().AssertNotNull(), true );
                fields.Remove( replaced.As<IField>() );

                break;

            default:
                throw new AssertionFailedException();
        }

        // Update the redirection cache.
        if ( transformation is { ReplacedMember: { } replacedMember } )
        {
            if ( transformation is IDeclarationBuilder builder )
            {
                this._redirectionCache = this._redirectionCache.Add( replacedMember.ToRef().As<IDeclaration>(), Ref.FromBuilder( builder ) );
            }
            else
            {
                throw new AssertionFailedException();
            }
        }
    }

    private void AddDeclaration( IDeclaration declaration )
    {
        switch ( declaration )
        {
            case IMethod method:
                var methods = this.GetMethodCollection( method.DeclaringType.GetSymbol().AssertNotNull(), true );
                methods.Add( method.ToMemberRef() );

                break;

            case IConstructor { IsStatic: false } constructor:
                var constructors = this.GetConstructorCollection( constructor.DeclaringType.GetSymbol().AssertNotNull(), true );
                constructors.Add( constructor.ToMemberRef() );

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

            case IField field:
                var fields = this.GetFieldCollection( field.DeclaringType.GetSymbol().AssertNotNull(), true );
                fields.Add( field.ToMemberRef() );

                break;

            case IProperty property:
                var properties = this.GetPropertyCollection( property.DeclaringType.GetSymbol().AssertNotNull(), true );
                properties.Add( property.ToMemberRef() );

                break;

            case IEvent @event:
                var events = this.GetEventCollection( @event.DeclaringType.GetSymbol().AssertNotNull(), true );
                events.Add( @event.ToMemberRef() );

                break;

            case IParameterBuilder parameter:
                var parameters = this.GetParameterCollection( ((IHasParameters) parameter.ContainingDeclaration!).ToTypedRef(), true );
                parameters.Add( parameter );

                break;

            default:
                throw new AssertionFailedException();
        }
    }

    private void AddIntroduceInterfaceTransformation( IIntroduceInterfaceTransformation transformation )
    {
        var introduceInterface = (IntroduceInterfaceTransformation) transformation;

        var interfaces =
            this.GetInterfaceImplementationCollection(
                (INamedTypeSymbol) introduceInterface.ContainingDeclaration.GetSymbol().AssertNotNull(),
                true );

        interfaces.Add( introduceInterface );
    }
}