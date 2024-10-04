// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Builders.Data;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Builders.Built;

internal sealed class BuiltNamedType : BuiltMemberOrNamedType, INamedTypeImpl
{
    public NamedTypeBuilderData TypeBuilder { get; }

    public BuiltNamedType( NamedTypeBuilderData builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this.TypeBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this.TypeBuilder;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder => this.TypeBuilder;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this.TypeBuilder;

    public bool IsPartial => this.TypeBuilder.IsPartial;

    public bool HasDefaultConstructor => this.TypeBuilder.HasDefaultConstructor;

    [Memo]
    public INamedType? BaseType => this.MapDeclaration( this.TypeBuilder.BaseType );

    public IImplementedInterfaceCollection AllImplementedInterfaces
        => new AllImplementedInterfacesCollection(
            this,
            this.Compilation.GetAllInterfaceImplementationCollection( this.Ref, false ) );

    public IImplementedInterfaceCollection ImplementedInterfaces
        => new ImplementedInterfacesCollection(
            this,
            this.Compilation.GetInterfaceImplementationCollection( this.Ref, false ) );

    INamespace INamedType.Namespace => this.ContainingNamespace;

    [Memo]
    public INamespace ContainingNamespace => this.MapDeclaration( this.TypeBuilder.ContainingNamespace );

    [Memo]
    private IRef<INamedType> Ref => this.RefFactory.FromBuilt<INamedType>( this );

    public IRef<INamedType> ToRef() => this.Ref;

    IRef<IType> IType.ToRef() => this.Ref;

    IRef<INamespaceOrNamedType> INamespaceOrNamedType.ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    INamedTypeCollection INamedType.NestedTypes => this.Types;

    public string FullName => this.TypeBuilder.FullName;

    [Memo]
    public INamedTypeCollection Types
        => new NamedTypeCollection(
            this,
            this.Compilation.GetNamedTypeCollectionByParent( this.Ref ) );

    [Memo]
    public INamedTypeCollection AllTypes => new AllTypesCollection( this );

    [Memo]
    public IPropertyCollection Properties
        => new PropertyCollection(
            this,
            this.Compilation.GetPropertyCollection( this.Ref.GetDefinition() ) );

    [Memo]
    public IPropertyCollection AllProperties => new AllPropertiesCollection( this );

    [Memo]
    public IIndexerCollection Indexers
        => new IndexerCollection(
            this,
            this.Compilation.GetIndexerCollection( this.Ref.GetDefinition() ) );

    [Memo]
    public IIndexerCollection AllIndexers => new AllIndexersCollection( this );

    [Memo]
    public IFieldCollection Fields
        => new FieldCollection(
            this,
            this.Compilation.GetFieldCollection( this.Ref.GetDefinition() ) );

    [Memo]
    public IFieldCollection AllFields => new AllFieldsCollection( this );

    [Memo]
    public IFieldOrPropertyCollection FieldsAndProperties => new FieldAndPropertiesCollection( this.Fields, this.Properties );

    public IFieldOrPropertyCollection AllFieldsAndProperties => new AllFieldsAndPropertiesCollection( this );

    [Memo]
    public IEventCollection Events
        => new EventCollection(
            this,
            this.Compilation.GetEventCollection( this.Ref.GetDefinition() ) );

    [Memo]
    public IEventCollection AllEvents => new AllEventsCollection( this );

    [Memo]
    public IMethodCollection Methods
        => new MethodCollection(
            this,
            this.Compilation.GetMethodCollection( this.Ref.GetDefinition() ) );

    public IMethodCollection AllMethods => new AllMethodsCollection( this );

    public IConstructor? PrimaryConstructor => this.TypeBuilder.PrimaryConstructor;

    public IConstructorCollection Constructors
        => new ConstructorCollection(
            this,
            this.Compilation.GetConstructorCollection( this.Ref.GetDefinition() ) );

    public IConstructor? StaticConstructor => this.TypeBuilder.StaticConstructor;

    public IMethod? Finalizer => this.TypeBuilder.Finalizer;

    public bool IsReadOnly => this.TypeBuilder.IsReadOnly;

    public bool IsRef => this.TypeBuilder.IsRef;

    public INamedType TypeDefinition => this.Definition;

    [Memo]
    public INamedType Definition => this.Compilation.Factory.GetNamedType( this.TypeBuilder ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    public INamedType UnderlyingType => this.Definition;

    public TypeKind TypeKind => this.TypeBuilder.TypeKind;

    public SpecialType SpecialType => this.TypeBuilder.SpecialType;

    public bool? IsReferenceType => this.TypeBuilder.IsReferenceType;

    public bool? IsNullable => this.TypeBuilder.IsNullable;

    public ITypeParameterList TypeParameters => this.TypeBuilder.TypeParameters;

    [Memo]
    public IReadOnlyList<IType> TypeArguments => this.TypeParameters.SelectAsImmutableArray( this.MapType );

    public bool IsGeneric => this.TypeBuilder.IsGeneric;

    public bool IsCanonicalGenericInstance => this.TypeBuilder.IsCanonicalGenericInstance;

    public ExecutionScope ExecutionScope => this.TypeBuilder.ExecutionScope;

    public ITypeSymbol? TypeSymbol => null;

    public ISymbol? Symbol => null;

    public bool Equals( SpecialType specialType ) => false;

    public bool Equals( IType? otherType, TypeComparison typeComparison )
        => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

    public bool Equals( IType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public bool Equals( INamedType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = DerivedTypesOptions.Default )
        => Array.Empty<IDeclaration>();

    public bool IsSubclassOf( INamedType type ) => type.SpecialType == SpecialType.Object;

    public Type ToType()
    {
        throw new NotSupportedException( "Reflection types on introduced types are not yet supported." );
    }

    protected override ISymbol? GetSymbol() => this.TypeSymbol;

    public bool TryFindImplementationForInterfaceMember( IMember interfaceMember, [NotNullWhen( true )] out IMember? implementationMember )
    {
        throw new NotSupportedException( "TryFindImplementationForInterfaceMember on introduced types is not yet supported." );
    }

    IReadOnlyList<IMember> INamedTypeImpl.GetOverridingMembers( IMember member )
    {
        throw new NotSupportedException( "GetOverridingMembers on introduced types is not yet supported." );
    }

    bool INamedTypeImpl.IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember )
    {
        throw new NotSupportedException( "IsImplementationOfInterfaceMember on introduced types is not yet supported." );
    }

    IType ITypeImpl.Accept( TypeRewriter visitor )
    {
        return visitor.Visit( this );
    }

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments )
    {
        throw new NotSupportedException( "ConstructGenericInstance on introduced types is not yet supported." );
    }
}