// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Attributes;
using Metalama.Framework.Engine.AdviceImpl.InterfaceImplementation;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
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
    private ImmutableDictionary<IRef<INamedType>, FieldUpdatableCollection> _fields;
    private ImmutableDictionary<IRef<INamedType>, MethodUpdatableCollection> _methods;
    private ImmutableDictionary<IRef<INamedType>, ConstructorUpdatableCollection> _constructors;
    private ImmutableDictionary<IRef<INamedType>, EventUpdatableCollection> _events;
    private ImmutableDictionary<IRef<INamedType>, PropertyUpdatableCollection> _properties;
    private ImmutableDictionary<IRef<INamedType>, IndexerUpdatableCollection> _indexers;
    private ImmutableDictionary<IRef<INamedType>, InterfaceUpdatableCollection> _interfaceImplementations;
    private ImmutableDictionary<IRef<INamedType>, AllInterfaceUpdatableCollection> _allInterfaceImplementations;
    private ImmutableDictionary<IRef<IHasParameters>, ParameterUpdatableCollection> _parameters;
    private ImmutableDictionary<IRef<IDeclaration>, AttributeUpdatableCollection> _attributes;
    private ImmutableDictionary<IRef<INamedType>, IConstructorBuilder> _staticConstructors;
    private ImmutableDictionary<IRef<INamedType>, IMethodBuilder> _finalizers;
    private ImmutableDictionary<IRef<INamespaceOrNamedType>, TypeUpdatableCollection> _namedTypesByParent;
    private ImmutableDictionary<IRef<INamespace>, NamespaceUpdatableCollection> _namespaces;
    private TypeUpdatableCollection? _topLevelNamedTypes;

    internal ImmutableDictionaryOfArray<IRef<IDeclaration>, AnnotationInstance> Annotations { get; private set; }

    private bool IsMutable { get; }

    internal bool Contains( FieldBuilder fieldBuilder )
        => (this._fields.TryGetValue( fieldBuilder.DeclaringType.ToRef(), out var fields )
            && fields.Contains( fieldBuilder.ToRef() ))
           || this.IsRedirected( fieldBuilder.ToRef() );

    internal bool Contains( MethodBuilder methodBuilder )
        => methodBuilder switch
        {
            { MethodKind: MethodKind.Finalizer } =>
                this._finalizers.TryGetValue( methodBuilder.DeclaringType.ToRef(), out var finalizer )
                && finalizer == methodBuilder,
            _ =>
                this._methods.TryGetValue( methodBuilder.DeclaringType.ToRef(), out var methods )
                && methods.Contains( methodBuilder.ToRef() )
        };

    internal bool Contains( ConstructorBuilder constructorBuilder )
        => (this._constructors.TryGetValue( constructorBuilder.DeclaringType.ToRef(), out var constructors )
            && constructors.Contains( constructorBuilder.ToRef() ))
           || (this._staticConstructors.TryGetValue( constructorBuilder.DeclaringType.ToRef(), out var staticConstructors )
               && staticConstructors == constructorBuilder);

    internal bool Contains( EventBuilder eventBuilder )
        => this._events.TryGetValue( eventBuilder.DeclaringType.ToRef(), out var events )
           && events.Contains( eventBuilder.ToRef() );

    internal bool Contains( PropertyBuilder propertyBuilder )
        => this._properties.TryGetValue( propertyBuilder.DeclaringType.ToRef(), out var properties )
           && properties.Contains( propertyBuilder.ToRef() );

    internal bool Contains( IndexerBuilder indexerBuilder )
        => this._indexers.TryGetValue( indexerBuilder.DeclaringType.ToRef(), out var indexers )
           && indexers.Contains( indexerBuilder.ToRef() );

    internal bool Contains( BaseParameterBuilder parameterBuilder )
        => parameterBuilder.ContainingDeclaration switch
        {
            DeclarationBuilder declarationBuilder => this.Contains( declarationBuilder ),
            null => false,
            _ => this._parameters.TryGetValue( parameterBuilder.ContainingDeclaration.ToRef().As<IHasParameters>(), out var parameters )
                 && parameters.Contains( parameterBuilder.ToRef() )
        };

    internal bool Contains( NamedTypeBuilder namedTypeBuilder )
    {
        var namespaceOrNamedType = (INamespaceOrNamedType?) namedTypeBuilder.DeclaringType
                                   ?? namedTypeBuilder.ContainingNamespace ?? throw new AssertionFailedException();

        return this._namedTypesByParent.TryGetValue(
                   namespaceOrNamedType.ToRef(),
                   out var namedTypes )
               && namedTypes.Contains( namedTypeBuilder.ToRef() );
    }

    internal bool Contains( NamespaceBuilder namespaceBuilder )
    {
        var containingNamespace = namespaceBuilder.ContainingNamespace ?? namespaceBuilder.ContainingNamespace ?? throw new AssertionFailedException();

        return this._namespaces.TryGetValue(
                   containingNamespace.ToRef(),
                   out var namespaces )
               && namespaces.Contains( namespaceBuilder.ToRef() );
    }

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
        return this._parameters.TryGetValue( parameterBuilder.DeclaringMember.ToRef().As<IHasParameters>(), out var events )
               && events.Contains( parameterBuilder.ToRef() );
    }

    private TCollection GetMemberCollection<TOwner, TDeclaration, TCollection>(
        ref ImmutableDictionary<IRef<TOwner>, TCollection> dictionary,
        bool requestMutableCollection,
        IRef<TOwner> declaration,
        Func<CompilationModel, IRef<TOwner>, TCollection> createCollection )
        where TOwner : class, IDeclaration
        where TDeclaration : class, IDeclaration
        where TCollection : IUpdatableCollection<TDeclaration>
    {
        Invariant.Assert( !(requestMutableCollection && !this.IsMutable) );
        Invariant.Assert( declaration.IsDefinition() );

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
        }
        else
        {
            collection = createCollection( this.Compilation, declaration );
            dictionary = dictionary.SetItem( declaration, collection );
        }

        return collection;
    }

    internal FieldUpdatableCollection GetFieldCollection( IRef<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IField, FieldUpdatableCollection>(
            ref this._fields,
            mutable,
            declaringType,
            static ( c, t ) => new FieldUpdatableCollection( c, t ) );

    internal MethodUpdatableCollection GetMethodCollection( IRef<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IMethod, MethodUpdatableCollection>(
            ref this._methods,
            mutable,
            declaringType,
            static ( c, t ) => new MethodUpdatableCollection( c, t ) );

    internal ConstructorUpdatableCollection GetConstructorCollection( IRef<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IConstructor, ConstructorUpdatableCollection>(
            ref this._constructors,
            mutable,
            declaringType,
            static ( c, t ) => new ConstructorUpdatableCollection( c, t ) );

    internal PropertyUpdatableCollection GetPropertyCollection( IRef<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IProperty, PropertyUpdatableCollection>(
            ref this._properties,
            mutable,
            declaringType,
            static ( c, t ) => new PropertyUpdatableCollection( c, t ) );

    internal IndexerUpdatableCollection GetIndexerCollection( IRef<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IIndexer, IndexerUpdatableCollection>(
            ref this._indexers,
            mutable,
            declaringType,
            static ( c, t ) => new IndexerUpdatableCollection( c, t ) );

    internal EventUpdatableCollection GetEventCollection( IRef<INamedType> declaringType, bool mutable = false )
        => this.GetMemberCollection<INamedType, IEvent, EventUpdatableCollection>(
            ref this._events,
            mutable,
            declaringType,
            static ( c, t ) => new EventUpdatableCollection( c, t ) );

    internal InterfaceUpdatableCollection GetInterfaceImplementationCollection( IRef<INamedType> declaringType, bool mutable )
        => this.GetMemberCollection<INamedType, INamedType, InterfaceUpdatableCollection>(
            ref this._interfaceImplementations,
            mutable,
            declaringType,
            ( c, t ) => new InterfaceUpdatableCollection( c, t ) );

    internal AllInterfaceUpdatableCollection GetAllInterfaceImplementationCollection( IRef<INamedType> declaringType, bool mutable )
        => this.GetMemberCollection<INamedType, INamedType, AllInterfaceUpdatableCollection>(
            ref this._allInterfaceImplementations,
            mutable,
            declaringType,
            static ( c, t ) => new AllInterfaceUpdatableCollection( c, t ) );

    internal ParameterUpdatableCollection GetParameterCollection( IRef<IHasParameters> parent, bool mutable = false )
        => this.GetMemberCollection<IHasParameters, IParameter, ParameterUpdatableCollection>(
            ref this._parameters,
            mutable,
            parent,
            static ( c, t ) => new ParameterUpdatableCollection( c, t ) );

    internal TypeUpdatableCollection GetNamedTypeCollectionByParent( IRef<INamespaceOrNamedType> parent, bool mutable = false )
        => this.GetMemberCollection<INamespaceOrNamedType, INamedType, TypeUpdatableCollection>(
            ref this._namedTypesByParent,
            mutable,
            parent,
            static ( c, t ) => new TypeUpdatableCollection( c, t ) );

    internal TypeUpdatableCollection GetTopLevelNamedTypeCollection( bool mutable = false )
    {
        if ( this._topLevelNamedTypes != null )
        {
            if ( !ReferenceEquals( this._topLevelNamedTypes.Compilation, this ) && mutable )
            {
                // The UpdateArray was created in another compilation snapshot, so it is not mutable in the current compilation.
                // We need to take a copy of it.
                this._topLevelNamedTypes = (TypeUpdatableCollection) this._topLevelNamedTypes.Clone( this.Compilation );
            }
        }
        else
        {
            this._topLevelNamedTypes = new TypeUpdatableCollection( this.Compilation );
        }

        return this._topLevelNamedTypes;
    }

    internal NamespaceUpdatableCollection GetNamespaceCollection( IRef<INamespace> declaringNamespace, bool mutable = false )
        => this.GetMemberCollection<INamespace, INamespace, NamespaceUpdatableCollection>(
            ref this._namespaces,
            mutable,
            declaringNamespace,
            static ( c, t ) => new NamespaceUpdatableCollection( c, t ) );

    internal AttributeUpdatableCollection GetAttributeCollection( IRef<IDeclaration> parent, bool mutable = false )
        => this.GetMemberCollection<IDeclaration, IAttribute, AttributeUpdatableCollection>(
            ref this._attributes,
            mutable,
            parent,
            static ( c, t ) => new AttributeUpdatableCollection( c, t ) );

    internal IConstructorBuilder? GetStaticConstructor( INamedTypeSymbol declaringType )
    {
        this._staticConstructors.TryGetValue( declaringType.ToRef( this.CompilationContext ), out var value );

        return value;
    }

    internal IMethodBuilder? GetFinalizer( INamedTypeSymbol declaringType )
    {
        this._finalizers.TryGetValue( declaringType.ToRef( this.CompilationContext ), out var value );

        return value;
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
            this.Annotations.Add(
                addAnnotationTransformation.TargetDeclaration.ToRef(),
                addAnnotationTransformation.AnnotationInstance );

    private void RemoveAttributes( RemoveAttributesTransformation removeAttributes )
    {
        var attributes = this.GetAttributeCollection( removeAttributes.ContainingDeclaration.ToRef(), true );
        attributes.Remove( removeAttributes.AttributeType );
    }

    private void AddReplaceMemberTransformation( IReplaceMemberTransformation transformation )
    {
        if ( transformation.ReplacedMember == null )
        {
            return;
        }

        var replaced = transformation.ReplacedMember;
        this.Factory.Invalidate( replaced );

        switch ( replaced )
        {
            case IConstructor { IsStatic: false } replacedConstructor:
                var constructors = this.GetConstructorCollection( replacedConstructor.DeclaringType.ToRef(), true );
                constructors.Remove( replacedConstructor.ToRef() );

                break;

            case IConstructor { IsStatic: true }:
                // Nothing to do, static constructor is replaced in the collection earlier.
                break;

            case IField replacedField:
                var fields = this.GetFieldCollection( replacedField.DeclaringType.ToRef(), true );
                fields.Remove( replacedField.ToRef() );

                break;

            default:
                throw new AssertionFailedException( $"Unexpected declaration: '{replaced}'." );
        }

        // Update the redirection cache.
        if ( transformation is { ReplacedMember: { } replacedMember } )
        {
            if ( transformation is IIntroduceDeclarationTransformation introduceDeclarationTransformation )
            {
                var newBuilder = introduceDeclarationTransformation.DeclarationBuilder;

                Invariant.Assert( !(replacedMember is IBuilderRef replacedBuilderRef && newBuilder.Equals( replacedBuilderRef.BuilderData )) );

                this._redirections = this._redirections.Add( replacedMember.ToRef(), newBuilder );
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
                var finalizerDeclaringType = finalizer.DeclaringType.ToRef();

                if ( this._finalizers.ContainsKey( finalizerDeclaringType ) )
                {
                    // Duplicate.
                    throw new AssertionFailedException( $"The type '{finalizer.DeclaringType}' already contains a finalizer." );
                }

                this._finalizers = this._finalizers.SetItem( finalizerDeclaringType, finalizer );

                break;

            case IMethod method:
                var methods = this.GetMethodCollection( method.DeclaringType.ToRef(), true ).AssertCast<MethodUpdatableCollection>();
                methods.Add( method.ToRef() );

                break;

            case IConstructor { IsStatic: false } constructor:
                var constructors = this.GetConstructorCollection( constructor.DeclaringType.ToRef(), true );
                constructors.Add( constructor.ToRef() );

                break;

            case IConstructorBuilder { IsStatic: true } staticConstructorBuilder:
                var staticCtorDeclaringType = staticConstructorBuilder.DeclaringType.ToRef();

                if ( this._staticConstructors.ContainsKey( staticCtorDeclaringType ) )
                {
                    // Duplicate.
                    throw new AssertionFailedException( $"The type '{staticConstructorBuilder.DeclaringType}' already contains a static constructor." );
                }

                this._staticConstructors = this._staticConstructors.SetItem( staticCtorDeclaringType, staticConstructorBuilder );

                break;

            case IField field:
                var fields = this.GetFieldCollection( field.DeclaringType.ToRef(), true );
                fields.Add( field.ToRef() );

                break;

            case IProperty property:
                var properties = this.GetPropertyCollection( property.DeclaringType.ToRef(), true );
                properties.Add( property.ToRef() );

                break;

            case IIndexer indexer:
                var indexers = this.GetIndexerCollection( indexer.DeclaringType.ToRef(), true );
                indexers.Add( indexer.ToRef() );

                break;

            case IEvent @event:
                var events = this.GetEventCollection( @event.DeclaringType.ToRef(), true );
                events.Add( @event.ToRef() );

                break;

            case IParameterBuilder parameter:
                var parameters = this.GetParameterCollection( parameter.DeclaringMember.ToRef().As<IHasParameters>(), true );
                parameters.Add( parameter );

                break;

            case AttributeBuilder attribute:
                var attributes = this.GetAttributeCollection( attribute.ContainingDeclaration.ToRef(), true );
                attributes.Add( attribute );

                break;

            case INamedType namedType:
                var types = this.GetNamedTypeCollectionByParent(
                    namedType.ContainingDeclaration.AssertNotNull().ToRef().As<INamespaceOrNamedType>(),
                    true );

                types.Add( namedType.ToRef() );

                if ( namedType.DeclaringType == null )
                {
                    var topLevelTypes = this.GetTopLevelNamedTypeCollection( true );
                    topLevelTypes.Add( namedType.ToRef() );
                }

                break;

            case INamespace @namespace:
                var namespaces = this.GetNamespaceCollection(
                    @namespace.ContainingNamespace.AssertNotNull().ToRef(),
                    true );

                namespaces.Add( @namespace.ToRef() );

                break;

            default:
                throw new AssertionFailedException( $"Unexpected declaration type: {declaration.GetType()}." );
        }
    }

    private void AddIntroduceInterfaceTransformation( IIntroduceInterfaceTransformation transformation )
    {
        var introduceInterface = (IntroduceInterfaceTransformation) transformation;

        var targetType = (INamedType) introduceInterface.ContainingDeclaration;

        var interfaces = this.GetInterfaceImplementationCollection( targetType.ToRef(), true );

        interfaces.Add( introduceInterface );

        foreach ( var type in new[] { targetType }.Concat( this.GetDerivedTypes( targetType ) ) )
        {
            var allInterfaces = this.GetAllInterfaceImplementationCollection( type.ToRef(), true );

            allInterfaces.Add( introduceInterface );
        }
    }
}