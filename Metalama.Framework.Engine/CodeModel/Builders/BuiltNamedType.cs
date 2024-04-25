// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    public IImplementedInterfaceCollection AllImplementedInterfaces => this.TypeBuilder.AllImplementedInterfaces;

    public IImplementedInterfaceCollection ImplementedInterfaces => this.TypeBuilder.ImplementedInterfaces;

    public INamespace Namespace => this.TypeBuilder.Namespace;

    public string FullName => this.TypeBuilder.FullName;

    public INamedTypeCollection NestedTypes => this.TypeBuilder.NestedTypes;

    public IPropertyCollection Properties => this.TypeBuilder.Properties;

    public IPropertyCollection AllProperties => this.TypeBuilder.AllProperties;

    public IIndexerCollection Indexers => this.TypeBuilder.Indexers;

    public IIndexerCollection AllIndexers => this.TypeBuilder.AllIndexers;

    public IFieldCollection Fields => this.TypeBuilder.Fields;

    public IFieldCollection AllFields => this.TypeBuilder.AllFields;

    public IFieldOrPropertyCollection FieldsAndProperties => this.TypeBuilder.FieldsAndProperties;

    public IFieldOrPropertyCollection AllFieldsAndProperties => this.TypeBuilder.AllFieldsAndProperties;

    public IEventCollection Events => this.TypeBuilder.Events;

    public IEventCollection AllEvents => this.TypeBuilder.AllEvents;

    public IMethodCollection Methods => this.TypeBuilder.Methods;

    public IMethodCollection AllMethods => this.TypeBuilder.AllMethods;

    public IConstructor? PrimaryConstructor => this.TypeBuilder.PrimaryConstructor;

    public IConstructorCollection Constructors => this.TypeBuilder.Constructors;

    public IConstructor? StaticConstructor => this.TypeBuilder.StaticConstructor;

    public IMethod? Finalizer => this.TypeBuilder.Finalizer;

    public bool IsReadOnly => this.TypeBuilder.IsReadOnly;

    public bool IsRef => this.TypeBuilder.IsRef;

    public INamedType TypeDefinition => this.TypeBuilder.TypeDefinition;

    public INamedType Definition => this.TypeBuilder.Definition;

    public INamedType UnderlyingType => this.TypeBuilder.UnderlyingType;

    public Code.TypeKind TypeKind => this.TypeBuilder.TypeKind;

    public Code.SpecialType SpecialType => this.TypeBuilder.SpecialType;

    public bool? IsReferenceType => this.TypeBuilder.IsReferenceType;

    public bool? IsNullable => this.TypeBuilder.IsNullable;

    public IGenericParameterList TypeParameters => this.TypeBuilder.TypeParameters;

    public IReadOnlyList<IType> TypeArguments => this.TypeBuilder.TypeArguments;

    public bool IsGeneric => this.TypeBuilder.IsGeneric;

    public bool IsCanonicalGenericInstance => this.TypeBuilder.IsCanonicalGenericInstance;

    public ExecutionScope ExecutionScope => this.TypeBuilder.ExecutionScope;

    public Microsoft.CodeAnalysis.ITypeSymbol TypeSymbol => this.TypeBuilder.TypeSymbol;

    public Microsoft.CodeAnalysis.ISymbol Symbol => this.TypeSymbol;

    public bool Equals( Code.SpecialType specialType ) => false;

    public bool Equals( IType? otherType, TypeComparison typeComparison ) => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

    public bool Equals( IType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public bool Equals( INamedType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = DerivedTypesOptions.Default ) => Array.Empty<IDeclaration>();

    public bool IsSubclassOf( INamedType type ) => type.SpecialType == Code.SpecialType.Object;

    public Type ToType()
    {
        throw new NotSupportedException();
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
        return new NamedType( ( (INamedTypeSymbol) this.Builder.GetSymbol()).Construct( typeArguments.Select( x => x.GetSymbol() ).ToArray() ), this.Compilation );
    }
}