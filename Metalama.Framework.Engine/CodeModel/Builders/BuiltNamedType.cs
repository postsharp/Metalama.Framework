// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltNamedType : BuiltMemberOrNamedType, INamedTypeImpl, ISdkType, ISdkDeclaration
{
    public NamedTypeBuilder TypeBuilder { get; set; }

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.TypeBuilder;

    public override DeclarationBuilder Builder => this.TypeBuilder;

    public BuiltNamedType( NamedTypeBuilder builder, CompilationModel compilation) : base(compilation, builder)
    {
        this.TypeBuilder = builder;
    }

    public bool IsPartial => this.TypeBuilder.IsPartial;

    public bool HasDefaultConstructor => this.TypeBuilder.HasDefaultConstructor;

    public INamedType? BaseType => this.TypeBuilder.BaseType;

    public IImplementedInterfaceCollection AllImplementedInterfaces
        => new AllImplementedInterfacesCollection(
            this,
            this.Compilation.GetAllInterfaceImplementationCollection( this.TypeSymbol.ToTypedRef<INamedType>( this.Compilation.CompilationContext ), false ) );

    public IImplementedInterfaceCollection ImplementedInterfaces
        => new ImplementedInterfacesCollection(
            this,
            this.Compilation.GetInterfaceImplementationCollection( this.TypeBuilder.ToRef().As<INamedType>(), false ) );

    public INamespace Namespace => this.TypeBuilder.Namespace;

    public string FullName => this.TypeBuilder.FullName;

    [Memo]
    public INamedTypeCollection NestedTypes
        => new NamedTypeCollection(
            this,
            this.Compilation.GetNamedTypeCollection( this.TypeBuilder.ToRef().As<INamespaceOrNamedType>() ) );

    [Memo]
    public IPropertyCollection Properties
        => new PropertyCollection(
            this,
            this.Compilation.GetPropertyCollection( this.TypeBuilder.ToRef().As<INamedType>() ) );

    [Memo]
    public IPropertyCollection AllProperties => new AllPropertiesCollection( this );

    [Memo]
    public IIndexerCollection Indexers
        => new IndexerCollection(
            this,
            this.Compilation.GetIndexerCollection( this.Builder.ToRef().As<INamedType>() ) );

    [Memo]
    public IIndexerCollection AllIndexers => new AllIndexersCollection( this );

    [Memo]
    public IFieldCollection Fields
        => new FieldCollection(
            this,
            this.Compilation.GetFieldCollection( this.Builder.ToRef().As<INamedType>() ) );

    [Memo]
    public IFieldCollection AllFields => new AllFieldsCollection( this );

    [Memo]
    public IFieldOrPropertyCollection FieldsAndProperties => new FieldAndPropertiesCollection( this.Fields, this.Properties );

    public IFieldOrPropertyCollection AllFieldsAndProperties => new AllFieldsAndPropertiesCollection( this );

    [Memo]
    public IEventCollection Events
        => new EventCollection(
            this,
            this.Compilation.GetEventCollection( this.Builder.ToRef().As<INamedType>() ) );

    [Memo]
    public IEventCollection AllEvents => new AllEventsCollection( this );

    public IMethodCollection Methods
        => new MethodCollection(
            this,
            this.Compilation.GetMethodCollection( this.Builder.ToRef().As<INamedType>() ) );

    public IMethodCollection AllMethods => new AllMethodsCollection( this );

    public IConstructor? PrimaryConstructor => this.TypeBuilder.PrimaryConstructor;

    public IConstructorCollection Constructors
        => new ConstructorCollection(
            this,
            this.Compilation.GetConstructorCollection( this.Builder.ToRef().As<INamedType>() ) );

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

    public Microsoft.CodeAnalysis.ITypeSymbol TypeSymbol => this.TypeBuilder.TypeSymbol;

    public Microsoft.CodeAnalysis.ISymbol Symbol => this.TypeSymbol;

    public bool Equals( SpecialType specialType ) => false;

    public bool Equals( IType? otherType, TypeComparison typeComparison ) => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

    public bool Equals( IType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public bool Equals( INamedType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = DerivedTypesOptions.Default ) => Array.Empty<IDeclaration>();

    public bool IsSubclassOf( INamedType type ) => type.SpecialType == Code.SpecialType.Object;

    public Type ToType()
    {
        throw new NotImplementedException();
    }

    protected override Microsoft.CodeAnalysis.ISymbol? GetSymbol() => this.TypeSymbol;

    public bool TryFindImplementationForInterfaceMember( IMember interfaceMember, out IMember? implementationMember )
    {
        implementationMember = null;
        return false;
    }

    IReadOnlyList<IMember> INamedTypeImpl.GetOverridingMembers( IMember member )
    {
        throw new NotImplementedException();
    }

    bool INamedTypeImpl.IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember )
    {
        throw new NotImplementedException();
    }

    ITypeImpl ITypeImpl.Accept( TypeRewriter visitor )
    {
        throw new NotImplementedException();
    }

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments )
    {
        return new NamedType( ( (Microsoft.CodeAnalysis.INamedTypeSymbol) this.Builder.GetSymbol()).Construct( typeArguments.Select( x => x.GetSymbol() ).ToArray() ), this.Compilation );
    }
}