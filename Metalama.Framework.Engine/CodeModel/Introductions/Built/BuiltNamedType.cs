// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
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

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltNamedType : BuiltMemberOrNamedType, INamedTypeImpl
{
    public NamedTypeBuilderData NamedTypeBuilderData { get; }

    public BuiltNamedType( NamedTypeBuilderData builderData, CompilationModel compilation, IGenericContext genericContext ) : base(
        compilation,
        genericContext )
    {
        this.NamedTypeBuilderData = builderData;
    }

    public override DeclarationBuilderData BuilderData => this.NamedTypeBuilderData;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder => this.NamedTypeBuilderData;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this.NamedTypeBuilderData;

    public bool HasDefaultConstructor => true; // TODO

    [Memo]
    public INamedType? BaseType => this.MapDeclaration( this.NamedTypeBuilderData.BaseType );

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
    public INamespace ContainingNamespace => this.GetContainingNamespace();

    private INamespace GetContainingNamespace()
    {
        var containingDeclaration = this.ContainingDeclaration;

        return containingDeclaration switch
        {
            INamespace ns => ns,
            INamedType type => type.ContainingNamespace,
            _ => throw new AssertionFailedException()
        };
    }

    [Memo]
    private IFullRef<INamedType> Ref => this.RefFactory.FromBuilt<INamedType>( this );

    public IRef<INamedType> ToRef() => this.Ref;

    IRef<IType> IType.ToRef() => this.Ref;

    IRef<INamespaceOrNamedType> INamespaceOrNamedType.ToRef() => this.Ref;

    private protected override IFullRef<IDeclaration> ToDeclarationRef() => this.Ref;

    INamedTypeCollection INamedType.NestedTypes => this.Types;

    [Memo]
    public string FullName => ((INamespaceOrNamedTypeImpl) this.ContainingDeclaration.AssertNotNull()).FullName + "." + this.Name;

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
            this.Compilation.GetPropertyCollection( this.Ref.Definition ) );

    [Memo]
    public IPropertyCollection AllProperties => new AllPropertiesCollection( this );

    [Memo]
    public IIndexerCollection Indexers
        => new IndexerCollection(
            this,
            this.Compilation.GetIndexerCollection( this.Ref.Definition ) );

    [Memo]
    public IIndexerCollection AllIndexers => new AllIndexersCollection( this );

    [Memo]
    public IFieldCollection Fields
        => new FieldCollection(
            this,
            this.Compilation.GetFieldCollection( this.Ref.Definition ) );

    [Memo]
    public IFieldCollection AllFields => new AllFieldsCollection( this );

    [Memo]
    public IFieldOrPropertyCollection FieldsAndProperties => new FieldAndPropertiesCollection( this.Fields, this.Properties );

    public IFieldOrPropertyCollection AllFieldsAndProperties => new AllFieldsAndPropertiesCollection( this );

    [Memo]
    public IEventCollection Events
        => new EventCollection(
            this,
            this.Compilation.GetEventCollection( this.Ref.Definition ) );

    [Memo]
    public IEventCollection AllEvents => new AllEventsCollection( this );

    [Memo]
    public IMethodCollection Methods
        => new MethodCollection(
            this,
            this.Compilation.GetMethodCollection( this.Ref.Definition ) );

    public IMethodCollection AllMethods => new AllMethodsCollection( this );

    IConstructor? INamedType.PrimaryConstructor => null;

    public IConstructorCollection Constructors
        => new ConstructorCollection(
            this,
            this.Compilation.GetConstructorCollection( this.Ref.Definition ) );

    IConstructor? INamedType.StaticConstructor => null;

    IMethod? INamedType.Finalizer => null;

    public INamedType TypeDefinition => this.Definition;

    [Memo]
    public INamedType Definition => this.Compilation.Factory.GetNamedType( this.NamedTypeBuilderData ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    public INamedType UnderlyingType => this.Definition;

    public TypeKind TypeKind => this.NamedTypeBuilderData.TypeKind;

    public SpecialType SpecialType => SpecialType.None;

    public bool? IsReferenceType => this.NamedTypeBuilderData.TypeKind is TypeKind.Class or TypeKind.RecordClass;

    public bool IsReadOnly => this.NamedTypeBuilderData.IsReadOnly;

    public bool IsRef => this.NamedTypeBuilderData.IsRef;

    public bool? IsNullable => false; // TODO: We don't have a mechanism to create nullable introduced types

    [Memo]
    public ITypeParameterList TypeParameters => new TypeParameterList( this, this.NamedTypeBuilderData.TypeParameters.SelectAsReadOnlyList( t => t.ToRef() ) );

    [Memo]
    public IReadOnlyList<IType> TypeArguments => this.NamedTypeBuilderData.TypeParameters.SelectAsImmutableArray( t => this.MapType( t.ToRef() ) );

    public bool IsGeneric => this.NamedTypeBuilderData.TypeParameters.Length > 0;

    public bool IsCanonicalGenericInstance => this.GenericContext.IsEmptyOrIdentity;

    public ExecutionScope ExecutionScope => ExecutionScope.RunTime;

    public ITypeSymbol? TypeSymbol => null;

    public ISymbol? Symbol => null;

    public bool Equals( SpecialType specialType ) => false;

    public bool Equals( IType? otherType, TypeComparison typeComparison )
        => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

    public bool Equals( IType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public bool Equals( INamedType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public override bool CanBeInherited => !this.IsSealed;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = DerivedTypesOptions.Default )
        => Array.Empty<IDeclaration>(); // TODO

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