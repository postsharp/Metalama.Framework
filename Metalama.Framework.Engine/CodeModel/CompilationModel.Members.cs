// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Attributes;
using Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Substituted;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel;

public sealed partial class CompilationModel
{
    private ImmutableDictionary<Ref<INamedType>, FieldUpdatableCollection> _fields;
    private ImmutableDictionary<Ref<INamedType>, ISourceMemberCollection<IMethod>> _methods;
    private ImmutableDictionary<Ref<INamedType>, ConstructorUpdatableCollection> _constructors;
    private ImmutableDictionary<Ref<INamedType>, EventUpdatableCollection> _events;
    private ImmutableDictionary<Ref<INamedType>, PropertyUpdatableCollection> _properties;
    private ImmutableDictionary<Ref<INamedType>, IndexerUpdatableCollection> _indexers;
    private ImmutableDictionary<Ref<INamedType>, InterfaceUpdatableCollection> _interfaceImplementations;
    private ImmutableDictionary<Ref<INamedType>, AllInterfaceUpdatableCollection> _allInterfaceImplementations;
    private ImmutableDictionary<Ref<IHasParameters>, ParameterUpdatableCollection> _parameters;
    private ImmutableDictionary<Ref<IDeclaration>, AttributeUpdatableCollection> _attributes;
    private ImmutableDictionary<Ref<INamedType>, IConstructorBuilder> _staticConstructors;
    private ImmutableDictionary<Ref<INamedType>, IMethodBuilder> _finalizers;
    private ImmutableDictionary<Ref<INamespaceOrNamedType>, TypeUpdatableCollection> _namedTypes;
    private ImmutableDictionary<Ref<INamespace>, NamespaceUpdatableCollection> _namespaces;

    internal ImmutableDictionaryOfArray<Ref<IDeclaration>, AnnotationInstance> Annotations { get; private set; }

    private bool IsMutable { get; }

    internal bool Contains( FieldBuilder fieldBuilder )
        => (this._fields.TryGetValue( fieldBuilder.DeclaringType.ToValueTypedRef(), out var fields )
            && fields.Contains( fieldBuilder.ToValueTypedRef<IField>() ))
           || this.TryGetRedirectedDeclaration( fieldBuilder.ToValueTypedRef(), out _ );

    internal bool Contains( MethodBuilder methodBuilder )
        => methodBuilder switch
        {
            { MethodKind: MethodKind.Finalizer } =>
                this._finalizers.TryGetValue( methodBuilder.DeclaringType.ToValueTypedRef(), out var finalizer )
                && finalizer == methodBuilder,
            _ =>
                this._methods.TryGetValue( methodBuilder.DeclaringType.ToValueTypedRef(), out var methods )
                && methods.Contains( methodBuilder.ToValueTypedRef<IMethod>() )
        };

    internal bool Contains( ConstructorBuilder constructorBuilder )
        => (this._constructors.TryGetValue( constructorBuilder.DeclaringType.ToValueTypedRef(), out var constructors )
            && constructors.Contains( constructorBuilder.ToValueTypedRef<IConstructor>() ))
           || (this._staticConstructors.TryGetValue( constructorBuilder.DeclaringType.ToValueTypedRef(), out var staticConstructors )
               && staticConstructors == constructorBuilder);

    internal bool Contains( EventBuilder eventBuilder )
        => this._events.TryGetValue( eventBuilder.DeclaringType.ToValueTypedRef(), out var events )
           && events.Contains( eventBuilder.ToValueTypedRef<IEvent>() );

    internal bool Contains( PropertyBuilder propertyBuilder )
        => this._properties.TryGetValue( propertyBuilder.DeclaringType.ToValueTypedRef(), out var properties )
           && properties.Contains( propertyBuilder.ToValueTypedRef<IProperty>() );

    internal bool Contains( IndexerBuilder indexerBuilder )
        => this._indexers.TryGetValue( indexerBuilder.DeclaringType.ToValueTypedRef(), out var indexers )
           && indexers.Contains( indexerBuilder.ToValueTypedRef<IIndexer>() );

    internal bool Contains( BaseParameterBuilder parameterBuilder )
        => parameterBuilder.ContainingDeclaration switch
        {
            DeclarationBuilder declarationBuilder => this.Contains( declarationBuilder ),
            null => false,
            _ => this._parameters.TryGetValue( ((IHasParameters) parameterBuilder.ContainingDeclaration).ToValueTypedRef(), out var parameters )
                 && parameters.Contains( parameterBuilder.ToValueTypedRef<IParameter>() )
        };

    internal bool Contains( NamedTypeBuilder namedTypeBuilder )
        => this._namedTypes.TryGetValue(
               ((INamespaceOrNamedType?) namedTypeBuilder.DeclaringType ?? namedTypeBuilder.ContainingNamespace ?? throw new AssertionFailedException())
               .ToValueTypedRef(),
               out var namedTypes )
           && namedTypes.Contains( namedTypeBuilder.ToValueTypedRef<INamedType>() );

    internal bool Contains( NamespaceBuilder namespaceBuilder )
        => this._namespaces.TryGetValue(
               (namespaceBuilder.ContainingNamespace ?? namespaceBuilder.ContainingNamespace ?? throw new AssertionFailedException())
               .ToValueTypedRef(),
               out var namespaces )
           && namespaces.Contains( namespaceBuilder.ToValueTypedRef<INamespace>() );

    private bool Contains( DeclarationBuilder builder )
        => builder switch
        {
            FieldBuilder fieldBuilder => this.Contains( fieldBuilder ),
            MethodBuilder methodBuilder => this.Contains( methodBuilder ),
            ConstructorBuilder constructorBuilder => this.Contains( constructorBuilder ),
            EventBuilder eventBuilder => this.Contains( eventBuilder ),
            PropertyBuilder propertyBuilder => this.Contains( propertyBuilder ),
            IndexerBuilder indexerBuilder => this.Contains( indexerBuilder ),
            BaseParameterBuilder parameterBuilder => this.Contains( parameterBuilder ),
            _ => throw new AssertionFailedException( $"Unexpected declaration type {builder.GetType()}." )
        };

    // TODO: Check why the next method is never used.
    // Resharper disable UnusedMember.Global

    internal bool Contains( ParameterBuilder parameterBuilder )
    {
        if ( parameterBuilder.IsReturnParameter )
        {
            return this.Contains( (DeclarationBuilder) parameterBuilder.DeclaringMember );
        }
        else if ( parameterBuilder.DeclaringMember is DeclarationBuilder declarationBuilder )
        {
            return this.Contains( declarationBuilder ) && ((IHasParameters) declarationBuilder).Parameters.Contains( parameterBuilder );
        }

        // This can also be a parameter appended to an existing declaration.
        return this._parameters.TryGetValue( parameterBuilder.DeclaringMember.ToValueTypedRef(), out var events )
               && events.Contains( parameterBuilder.ToValueTypedRef<IParameter>() );
    }

    private TCollection GetMemberCollection<TOwner, TDeclaration, TCollection>(
        ref ImmutableDictionary<Ref<TOwner>, TCollection> dictionary,
        bool requestMutableCollection,
        Ref<TOwner> declaringTypeRef,
        Func<CompilationModel, Ref<TOwner>, TCollection> createCollection,
        Func<TCollection, Ref<TOwner>, TCollection>? createSubstitutedCollection = null )
        where TOwner : class, IDeclaration
        where TDeclaration : class, IDeclaration
        where TCollection : ISourceDeclarationCollection<TDeclaration>
        => this.GetMemberCollection<TOwner, TDeclaration, Ref<TDeclaration>, TCollection>(
            ref dictionary,
            requestMutableCollection,
            declaringTypeRef,
            createCollection,
            createSubstitutedCollection );

    private TCollection GetMemberCollection<TOwner, TDeclaration, TRef, TCollection>(
        ref ImmutableDictionary<Ref<TOwner>, TCollection> dictionary,
        bool requestMutableCollection,
        Ref<TOwner> declaration,
        Func<CompilationModel, Ref<TOwner>, TCollection> createCollection,
        Func<TCollection, Ref<TOwner>, TCollection>? createSubstitutedCollection )
        where TOwner : class, IDeclaration
        where TDeclaration : class, IDeclaration
        where TCollection : ISourceDeclarationCollection<TDeclaration, TRef>
        where TRef : IRefImpl<TDeclaration>, IEquatable<TRef>
    {
        if ( requestMutableCollection && !this.IsMutable )
        {
            // Cannot get a mutable collection when the model is immutable.
            throw new InvalidOperationException();
        }

        // If the model is mutable, we need to return a mutable collection because it may be mutated after the
        // front-end collection is returned.
        var returnMutableCollection = requestMutableCollection || this.IsMutable;

        if ( dictionary.TryGetValue( declaration, out var collection ) )
        {
            if ( !ReferenceEquals( collection.Compilation, this ) && returnMutableCollection )
            {
                // The UpdateArray was created in another compilation snapshot, so it is not mutable in the current compilation.
                // We need to take a copy of it.
                collection = (TCollection) collection.Clone( this.Compilation );
                dictionary = dictionary.SetItem( declaration, collection );
            }

            return collection;
        }
        else
        {
            if ( createSubstitutedCollection != null &&
                 declaration.Target is INamedTypeSymbol { IsGenericType: true } substitutedType &&
                 substitutedType.OriginalDefinition != substitutedType )
            {
                var sourceCollection = this.GetMemberCollection<TOwner, TDeclaration, TRef, TCollection>(
                    ref dictionary,
                    requestMutableCollection,
                    substitutedType.OriginalDefinition.ToValueTypedRef<TOwner>( this.CompilationContext ),
                    createCollection,
                    createSubstitutedCollection );

                collection = createSubstitutedCollection( sourceCollection, declaration );
            }
            else
            {
                collection = createCollection( this.Compilation, declaration );
            }

            dictionary = dictionary.SetItem( declaration, collection );
        }

        return collection;
    }

    internal FieldUpdatableCollection GetFieldCollection( in Ref<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IField, FieldUpdatableCollection>(
            ref this._fields,
            mutable,
            declaringType,
            ( c, t ) => new FieldUpdatableCollection( c, t ) );

    internal ISourceMemberCollection<IMethod> GetMethodCollection( in Ref<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IMethod, ISourceMemberCollection<IMethod>>(
            ref this._methods,
            mutable,
            declaringType,
            ( c, t ) => new MethodUpdatableCollection( c, t ),
            ( s, t ) => new MemberSubstitutedCollection<IMethod>( s, t ) );

    internal ConstructorUpdatableCollection GetConstructorCollection( in Ref<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IConstructor, ConstructorUpdatableCollection>(
            ref this._constructors,
            mutable,
            declaringType,
            ( c, t ) => new ConstructorUpdatableCollection( c, t ) );

    internal PropertyUpdatableCollection GetPropertyCollection( in Ref<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IProperty, PropertyUpdatableCollection>(
            ref this._properties,
            mutable,
            declaringType,
            ( c, t ) => new PropertyUpdatableCollection( c, t ) );

    internal IndexerUpdatableCollection GetIndexerCollection( in Ref<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IIndexer, IndexerUpdatableCollection>(
            ref this._indexers,
            mutable,
            declaringType,
            ( c, t ) => new IndexerUpdatableCollection( c, t ) );

    internal EventUpdatableCollection GetEventCollection( in Ref<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IEvent, EventUpdatableCollection>(
            ref this._events,
            mutable,
            declaringType,
            ( c, t ) => new EventUpdatableCollection( c, t ) );

    internal InterfaceUpdatableCollection GetInterfaceImplementationCollection( in Ref<INamedType> declaringType, bool mutable )
        => this.GetMemberCollection<INamedType, INamedType, InterfaceUpdatableCollection>(
            ref this._interfaceImplementations,
            mutable,
            declaringType,
            ( c, t ) => new InterfaceUpdatableCollection( c, t ) );

    internal AllInterfaceUpdatableCollection GetAllInterfaceImplementationCollection( in Ref<INamedType> declaringType, bool mutable )
        => this.GetMemberCollection<INamedType, INamedType, AllInterfaceUpdatableCollection>(
            ref this._allInterfaceImplementations,
            mutable,
            declaringType,
            ( c, t ) => new AllInterfaceUpdatableCollection( c, t ) );

    internal ParameterUpdatableCollection GetParameterCollection( in Ref<IHasParameters> parent, bool mutable = false )
        => this.GetMemberCollection<IHasParameters, IParameter, ParameterUpdatableCollection>(
            ref this._parameters,
            mutable,
            parent,
            ( c, t ) => new ParameterUpdatableCollection( c, t ) );

    internal AttributeUpdatableCollection GetAttributeCollection( in Ref<IDeclaration> parent, bool mutable = false )
    {
        var moduleSymbol = parent.Target is ISourceAssemblySymbol ? this.RoslynCompilation.SourceModule : null;

        return this.GetMemberCollection<IDeclaration, IAttribute, AttributeRef, AttributeUpdatableCollection>(
            ref this._attributes,
            mutable,
            parent,
            ( c, t ) => new AttributeUpdatableCollection( c, t, moduleSymbol ),
            null );
    }

    internal IConstructorBuilder? GetStaticConstructor( INamedTypeSymbol declaringType )
    {
        this._staticConstructors.TryGetValue( declaringType.ToValueTypedRef<INamedType>( this.CompilationContext ), out var value );

        return value;
    }

    internal IMethodBuilder? GetFinalizer( INamedTypeSymbol declaringType )
    {
        this._finalizers.TryGetValue( declaringType.ToValueTypedRef<INamedType>( this.CompilationContext ), out var value );

        return value;
    }

    internal TypeUpdatableCollection GetNamedTypeCollection( in Ref<INamespaceOrNamedType> declaringNamespaceOrType, bool mutable = false )
    {
        if ( mutable && !this.IsMutable )
        {
            // Cannot get a mutable collection when the model is immutable.
            throw new InvalidOperationException();
        }

        // If the model is mutable, we need to return a mutable collection because it may be mutated after the
        // front-end collection is returned.
        var returnMutableCollection = mutable || this.IsMutable;

        if ( this._namedTypes.TryGetValue( declaringNamespaceOrType, out var collection ) )
        {
            if ( !ReferenceEquals( collection.Compilation, this ) && returnMutableCollection )
            {
                // The UpdateArray was created in another compilation snapshot, so it is not mutable in the current compilation.
                // We need to take a copy of it.
                collection = (TypeUpdatableCollection) collection.Clone( this.Compilation );
                this._namedTypes = this._namedTypes.SetItem( declaringNamespaceOrType, collection );
            }

            return collection;
        }
        else
        {
            collection = new TypeUpdatableCollection( this, declaringNamespaceOrType );
            this._namedTypes = this._namedTypes.SetItem( declaringNamespaceOrType, collection );
        }

        return collection;
    }

    internal NamespaceUpdatableCollection GetNamespaceCollection( in Ref<INamespace> declaringNamespace, bool mutable = false )
    {
        if ( mutable && !this.IsMutable )
        {
            // Cannot get a mutable collection when the model is immutable.
            throw new InvalidOperationException();
        }

        // If the model is mutable, we need to return a mutable collection because it may be mutated after the
        // front-end collection is returned.
        var returnMutableCollection = mutable || this.IsMutable;

        if ( this._namespaces.TryGetValue( declaringNamespace, out var collection ) )
        {
            if ( !ReferenceEquals( collection.Compilation, this ) && returnMutableCollection )
            {
                // The UpdateArray was created in another compilation snapshot, so it is not mutable in the current compilation.
                // We need to take a copy of it.
                collection = (NamespaceUpdatableCollection) collection.Clone( this.Compilation );
                this._namespaces = this._namespaces.SetItem( declaringNamespace, collection );
            }

            return collection;
        }
        else
        {
            collection = new NamespaceUpdatableCollection( this, declaringNamespace );
            this._namespaces = this._namespaces.SetItem( declaringNamespace, collection );
        }

        return collection;
    }

    internal void AddTransformation( ITransformation transformation )
    {
        if ( !this.IsMutable )
        {
            throw new InvalidOperationException( "Cannot add transformation to an immutable compilation." );
        }

        if ( transformation.Observability == TransformationObservability.None )
        {
            return;
        }

        // Replaced declaration should be always removed before adding the replacement.
        if ( transformation is IReplaceMemberTransformation replaceMember )
        {
            this.AddReplaceMemberTransformation( replaceMember );
        }

        if ( transformation is RemoveAttributesTransformation removeAttributes )
        {
            this.RemoveAttributes( removeAttributes );
        }

        // IMPORTANT: Keep the builder interface in this condition for linker tests, which use fake builders.
        if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation )
        {
            var builder = introduceDeclarationTransformation.DeclarationBuilder;
            builder.Freeze();

            this.AddDeclaration( builder );
        }

        if ( transformation is IntroduceParameterTransformation appendParameterTransformation )
        {
            this.AddDeclaration( appendParameterTransformation.Parameter );
        }

        if ( transformation is IIntroduceInterfaceTransformation introduceInterface )
        {
            this.AddIntroduceInterfaceTransformation( introduceInterface );
        }

        if ( transformation is AddAnnotationTransformation addAnnotationTransformation )
        {
            this.AddAnnotation( addAnnotationTransformation );
        }
    }

    private void AddAnnotation( AddAnnotationTransformation addAnnotationTransformation )
        => this.Annotations =
            this.Annotations.Add( addAnnotationTransformation.TargetDeclaration.ToValueTypedRef(), addAnnotationTransformation.AnnotationInstance );

    private void RemoveAttributes( RemoveAttributesTransformation removeAttributes )
    {
        var attributes = this.GetAttributeCollection( removeAttributes.ContainingDeclaration.ToValueTypedRef(), true );
        attributes.Remove( removeAttributes.AttributeType );
    }

    private void AddReplaceMemberTransformation( IReplaceMemberTransformation transformation )
    {
        if ( transformation.ReplacedMember.IsDefault )
        {
            return;
        }

        var replaced = transformation.ReplacedMember;

        switch ( replaced.GetTarget( this ) )
        {
            case IConstructor { IsStatic: false } replacedConstructor:
                var constructors = this.GetConstructorCollection( replacedConstructor.DeclaringType.ToValueTypedRef(), true );
                constructors.Remove( replaced.As<IConstructor>() );

                break;

            case IConstructor { IsStatic: true }:
                // Nothing to do, static constructor is replaced in the collection earlier.
                break;

            case IField replacedField:
                var fields = this.GetFieldCollection( replacedField.DeclaringType.ToValueTypedRef(), true );
                fields.Remove( replaced.As<IField>() );

                break;

            default:
                throw new AssertionFailedException( $"Unexpected declaration: '{replaced.GetTarget( this )}'." );
        }

        // Update the redirection cache.
        if ( transformation is { ReplacedMember: { } replacedMember } )
        {
            if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation )
            {
                this._redirections = this._redirections.Add(
                    replacedMember.ToRef().As<IDeclaration>(),
                    Ref.FromBuilder( introduceDeclarationTransformation.DeclarationBuilder ) );
            }
            else
            {
                throw new AssertionFailedException( $"Unexpected transformation type: {transformation.GetType()}." );
            }
        }
    }

    private void AddDeclaration( IDeclaration declaration )
    {
        switch ( declaration )
        {
            case IMethodBuilder { MethodKind: MethodKind.Finalizer } finalizer:
                var finalizerDeclaringType = finalizer.DeclaringType.ToValueTypedRef();

                if ( this._finalizers.ContainsKey( finalizerDeclaringType ) )
                {
                    // Duplicate.
                    throw new AssertionFailedException( $"The type '{finalizer.DeclaringType}' already contains a finalizer." );
                }

                this._finalizers = this._finalizers.SetItem( finalizerDeclaringType, finalizer );

                break;

            case IMethod method:
                var methods = this.GetMethodCollection( method.DeclaringType.ToValueTypedRef(), true ).AssertCast<MethodUpdatableCollection>();
                methods.Add( method.ToMemberRef() );

                break;

            case IConstructor { IsStatic: false } constructor:
                var constructors = this.GetConstructorCollection( constructor.DeclaringType.ToValueTypedRef(), true );
                constructors.Add( constructor.ToMemberRef() );

                break;

            case IConstructorBuilder { IsStatic: true } staticConstructorBuilder:
                var staticCtorDeclaringType = staticConstructorBuilder.DeclaringType.ToValueTypedRef();

                if ( this._staticConstructors.ContainsKey( staticCtorDeclaringType ) )
                {
                    // Duplicate.
                    throw new AssertionFailedException( $"The type '{staticConstructorBuilder.DeclaringType}' already contains a static constructor." );
                }

                this._staticConstructors = this._staticConstructors.SetItem( staticCtorDeclaringType, staticConstructorBuilder );

                break;

            case IField field:
                var fields = this.GetFieldCollection( field.DeclaringType.ToValueTypedRef(), true );
                fields.Add( field.ToMemberRef() );

                break;

            case IProperty property:
                var properties = this.GetPropertyCollection( property.DeclaringType.ToValueTypedRef(), true );
                properties.Add( property.ToMemberRef() );

                break;

            case IIndexer indexer:
                var indexers = this.GetIndexerCollection( indexer.DeclaringType.ToValueTypedRef(), true );
                indexers.Add( indexer.ToMemberRef() );

                break;

            case IEvent @event:
                var events = this.GetEventCollection( @event.DeclaringType.ToValueTypedRef(), true );
                events.Add( @event.ToMemberRef() );

                break;

            case IParameterBuilder parameter:
                var parameters = this.GetParameterCollection( parameter.DeclaringMember.ToValueTypedRef(), true );
                parameters.Add( parameter );

                break;

            case AttributeBuilder attribute:
                var attributes = this.GetAttributeCollection( attribute.ContainingDeclaration.ToValueTypedRef(), true );
                attributes.Add( attribute );

                break;

            case INamedType namedType:
                var types = this.GetNamedTypeCollection(
                    namedType.ContainingDeclaration.AssertNotNull().ToValueTypedRef().As<INamespaceOrNamedType>(),
                    true );

                types.Add( namedType.ToMemberRef() );

                break;

            case INamespace @namespace:
                var namespaces = this.GetNamespaceCollection(
                    @namespace.ContainingNamespace.AssertNotNull().ToValueTypedRef().As<INamespace>(),
                    true );

                namespaces.Add( @namespace.ToMemberRef() );

                break;

            default:
                throw new AssertionFailedException( $"Unexpected declaration type: {declaration.GetType()}." );
        }
    }

    private void AddIntroduceInterfaceTransformation( IIntroduceInterfaceTransformation transformation )
    {
        var introduceInterface = (IntroduceInterfaceTransformation) transformation;

        var targetType = (INamedType) introduceInterface.ContainingDeclaration;

        var interfaces = this.GetInterfaceImplementationCollection( targetType.ToValueTypedRef(), true );

        interfaces.Add( introduceInterface );

        foreach ( var type in new[] { targetType }.Concat( this.GetDerivedTypes( targetType ) ) )
        {
            var allInterfaces = this.GetAllInterfaceImplementationCollection( type.ToValueTypedRef(), true );

            allInterfaces.Add( introduceInterface );
        }
    }
}