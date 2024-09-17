// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltNamedType : BuiltMemberOrNamedType, INamedTypeImpl
{
    public NamedTypeBuilder TypeBuilder { get; }

    public BuiltNamedType( CompilationModel compilation, NamedTypeBuilder builder ) : base( compilation )
    {
        this.TypeBuilder = builder;
    }

    public override DeclarationBuilder Builder => this.TypeBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.TypeBuilder;

    protected override NamedDeclarationBuilder NamedDeclarationBuilder => this.TypeBuilder;

    public bool IsPartial => this.TypeBuilder.IsPartial;

    public bool HasDefaultConstructor => this.TypeBuilder.HasDefaultConstructor;

    public INamedType? BaseType => this.Compilation.Factory.GetDeclaration( this.TypeBuilder.BaseType, ReferenceResolutionOptions.CanBeMissing );

    public IImplementedInterfaceCollection AllImplementedInterfaces
        => new AllImplementedInterfacesCollection(
            this,
            this.Compilation.GetAllInterfaceImplementationCollection( this.TypeBuilder.ToValueTypedRef<INamedType>(), false ) );

    public IImplementedInterfaceCollection ImplementedInterfaces
        => new ImplementedInterfacesCollection(
            this,
            this.Compilation.GetInterfaceImplementationCollection( this.TypeBuilder.ToValueTypedRef().As<INamedType>(), false ) );

    INamespace INamedType.Namespace => this.ContainingNamespace;

    public INamespace ContainingNamespace => this.TypeBuilder.ContainingNamespace;

    IRef<INamedType> INamedType.ToRef() => this.TypeBuilder.BoxedRef;

    IRef<IType> IType.ToRef() => this.TypeBuilder.BoxedRef;

    IRef<INamespaceOrNamedType> INamespaceOrNamedType.ToRef() => this.TypeBuilder.BoxedRef;

    INamedTypeCollection INamedType.NestedTypes => this.Types;

    public string FullName => this.TypeBuilder.FullName;

    [Memo]
    public INamedTypeCollection Types
        => new NamedTypeCollection(
            this,
            this.Compilation.GetNamedTypeCollection( this.TypeBuilder.ToValueTypedRef().As<INamespaceOrNamedType>() ) );

    [Memo]
    public INamedTypeCollection AllTypes => new AllTypesCollection( this );

    [Memo]
    public IPropertyCollection Properties
        => new PropertyCollection(
            this,
            this.Compilation.GetPropertyCollection( this.TypeBuilder.ToValueTypedRef().As<INamedType>() ) );

    [Memo]
    public IPropertyCollection AllProperties => new AllPropertiesCollection( this );

    [Memo]
    public IIndexerCollection Indexers
        => new IndexerCollection(
            this,
            this.Compilation.GetIndexerCollection( this.Builder.ToValueTypedRef().As<INamedType>() ) );

    [Memo]
    public IIndexerCollection AllIndexers => new AllIndexersCollection( this );

    [Memo]
    public IFieldCollection Fields
        => new FieldCollection(
            this,
            this.Compilation.GetFieldCollection( this.Builder.ToValueTypedRef().As<INamedType>() ) );

    [Memo]
    public IFieldCollection AllFields => new AllFieldsCollection( this );

    [Memo]
    public IFieldOrPropertyCollection FieldsAndProperties => new FieldAndPropertiesCollection( this.Fields, this.Properties );

    public IFieldOrPropertyCollection AllFieldsAndProperties => new AllFieldsAndPropertiesCollection( this );

    [Memo]
    public IEventCollection Events
        => new EventCollection(
            this,
            this.Compilation.GetEventCollection( this.Builder.ToValueTypedRef().As<INamedType>() ) );

    [Memo]
    public IEventCollection AllEvents => new AllEventsCollection( this );

    public IMethodCollection Methods
        => new MethodCollection(
            this,
            this.Compilation.GetMethodCollection( this.Builder.ToValueTypedRef().As<INamedType>() ) );

    public IMethodCollection AllMethods => new AllMethodsCollection( this );

    public IConstructor? PrimaryConstructor => this.TypeBuilder.PrimaryConstructor;

    public IConstructorCollection Constructors
        => new ConstructorCollection(
            this,
            this.Compilation.GetConstructorCollection( this.Builder.ToValueTypedRef().As<INamedType>() ) );

    public IConstructor? StaticConstructor => this.TypeBuilder.StaticConstructor;

    public IMethod? Finalizer => this.TypeBuilder.Finalizer;

    public bool IsReadOnly => this.TypeBuilder.IsReadOnly;

    public bool IsRef => this.TypeBuilder.IsRef;

    public INamedType TypeDefinition => this.TypeBuilder.TypeDefinition;

    public INamedType Definition => this.TypeBuilder.Definition;

    public INamedType UnderlyingType => this.TypeBuilder.UnderlyingType;

    public TypeKind TypeKind => this.TypeBuilder.TypeKind;

    public SpecialType SpecialType => this.TypeBuilder.SpecialType;

    public bool? IsReferenceType => this.TypeBuilder.IsReferenceType;

    public bool? IsNullable => this.TypeBuilder.IsNullable;

    public IGenericParameterList TypeParameters => this.TypeBuilder.TypeParameters;

    public IReadOnlyList<IType> TypeArguments => this.TypeBuilder.TypeArguments;

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