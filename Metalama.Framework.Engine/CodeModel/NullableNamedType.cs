// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using SpecialType = Metalama.Framework.Code.SpecialType;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// A wrapper over <see cref="NamedType"/> for the nullable version of a type, so the <see cref="NamedType"/> instance stays unique.
/// </summary>
internal class NullableNamedType : INamedTypeInternal
{
    private readonly NamedType _underlying;

    public ITypeSymbol TypeSymbol { get; }

    public NullableNamedType( NamedType underlying, INamedTypeSymbol typeSymbol )
    {
        this._underlying = underlying;
        this.TypeSymbol = typeSymbol;
    }

    ICompilation ICompilationElement.Compilation => ((ICompilationElement) this._underlying).Compilation;

    public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        => this.TypeSymbol.ToDisplayString( format.ToRoslyn() );

    TypeKind IType.TypeKind => ((IType) this._underlying).TypeKind;

    SpecialType IType.SpecialType => this._underlying.SpecialType;

    Type IType.ToType() => this._underlying.ToType();

    bool? IType.IsReferenceType => this._underlying.IsReferenceType;

    bool? IType.IsNullable => true;

    public bool Equals( SpecialType specialType ) => this._underlying.Equals( specialType );

    IRef<IDeclaration> IDeclaration.ToRef() => this.ToRef();

    ImmutableArray<SyntaxReference> IDeclarationImpl.DeclaringSyntaxReferences => this._underlying.DeclaringSyntaxReferences;

    bool IDeclarationImpl.CanBeInherited => this._underlying.CanBeInherited;

    SyntaxTree? IDeclarationImpl.PrimarySyntaxTree => this._underlying.PrimarySyntaxTree;

    IEnumerable<IDeclaration> IDeclarationImpl.GetDerivedDeclarations( bool deep ) => this._underlying.GetDerivedDeclarations( deep );

    public Ref<IDeclaration> ToRef() => new( this.TypeSymbol, this.GetCompilationModel().RoslynCompilation );

    IAssembly IDeclaration.DeclaringAssembly => this._underlying.DeclaringAssembly;

    DeclarationOrigin IDeclaration.Origin => this._underlying.Origin;

    IDeclaration? IDeclaration.ContainingDeclaration => this._underlying.ContainingDeclaration;

    IAttributeCollection IDeclaration.Attributes => this._underlying.Attributes;

    DeclarationKind IDeclaration.DeclarationKind => this._underlying.DeclarationKind;

    public bool IsImplicitlyDeclared => this._underlying.IsImplicitlyDeclared;

    string INamedDeclaration.Name => this._underlying.Name;

    Accessibility IMemberOrNamedType.Accessibility => this._underlying.Accessibility;

    bool IMemberOrNamedType.IsAbstract => this._underlying.IsAbstract;

    bool IMemberOrNamedType.IsStatic => this._underlying.IsStatic;

    bool IMemberOrNamedType.IsSealed => this._underlying.IsSealed;

    bool IMemberOrNamedType.IsNew => this._underlying.IsNew;

    INamedType? IMemberOrNamedType.DeclaringType => ((IMemberOrNamedType) this._underlying).DeclaringType;

    MemberInfo IMemberOrNamedType.ToMemberInfo() => this._underlying.ToMemberInfo();

    IGenericParameterList IGeneric.TypeParameters => this._underlying.TypeParameters;

    IReadOnlyList<IType> IGeneric.TypeArguments => this._underlying.TypeArguments;

    bool IGeneric.IsOpenGeneric => this._underlying.IsOpenGeneric;

    bool IGeneric.IsGeneric => this._underlying.IsGeneric;

    IGeneric IGenericInternal.ConstructGenericInstance( params IType[] typeArguments )
        => ((IGenericInternal) this._underlying).ConstructGenericInstance( typeArguments );

    bool INamedType.IsPartial => this._underlying.IsPartial;

    bool INamedType.IsExternal => this._underlying.IsExternal;

    bool INamedType.HasDefaultConstructor => this._underlying.HasDefaultConstructor;

    INamedType? INamedType.BaseType => this._underlying.BaseType;

    IImplementedInterfaceCollection INamedType.AllImplementedInterfaces => this._underlying.AllImplementedInterfaces;

    IImplementedInterfaceCollection INamedType.ImplementedInterfaces => this._underlying.ImplementedInterfaces;

    INamespace INamedType.Namespace => this._underlying.Namespace;

    string INamedType.FullName => this._underlying.FullName;

    INamedTypeCollection INamedType.NestedTypes => this._underlying.NestedTypes;

    IPropertyCollection INamedType.Properties => this._underlying.Properties;

    IPropertyCollection INamedType.AllProperties => this._underlying.AllProperties;

    IIndexerCollection INamedType.Indexers => this._underlying.Indexers;

    IIndexerCollection INamedType.AllIndexers => this._underlying.AllIndexers;

    IFieldCollection INamedType.Fields => this._underlying.Fields;

    IFieldCollection INamedType.AllFields => this._underlying.AllFields;

    IFieldOrPropertyCollection INamedType.FieldsAndProperties => this._underlying.FieldsAndProperties;

    IFieldOrPropertyCollection INamedType.AllFieldsAndProperties => this._underlying.AllFieldsAndProperties;

    IEventCollection INamedType.Events => this._underlying.Events;

    IEventCollection INamedType.AllEvents => this._underlying.AllEvents;

    IMethodCollection INamedType.Methods => this._underlying.Methods;

    IMethodCollection INamedType.AllMethods => this._underlying.AllMethods;

    IConstructorCollection INamedType.Constructors => this._underlying.Constructors;

    IConstructor? INamedType.StaticConstructor => this._underlying.StaticConstructor;

    IMethod? INamedType.Finalizer => this._underlying.Finalizer;

    bool INamedType.IsReadOnly => this._underlying.IsReadOnly;

    bool INamedType.IsSubclassOf( INamedType type ) => this._underlying.IsSubclassOf( type );

    bool INamedType.TryFindImplementationForInterfaceMember( IMember interfaceMember, [NotNullWhen( true )] out IMember? implementationMember )
        => this._underlying.TryFindImplementationForInterfaceMember( interfaceMember, out implementationMember );

    public INamedType TypeDefinition => this._underlying.TypeDefinition;

    ITypeInternal ITypeInternal.Accept( TypeRewriter visitor ) => this._underlying.Accept( visitor );

    T IMeasurableInternal.GetMetric<T>() => this._underlying.GetMetric<T>();

    IDeclaration IDeclarationInternal.OriginalDefinition => this._underlying.OriginalDefinition;

    IEnumerable<IMember> INamedTypeInternal.GetOverridingMembers( IMember member ) => this._underlying.GetOverridingMembers( member );

    bool INamedTypeInternal.IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember )
        => this._underlying.IsImplementationOfInterfaceMember( typeMember, interfaceMember );

    ISymbol? ISdkDeclaration.Symbol => this.TypeSymbol;

    Location? IDiagnosticLocationImpl.DiagnosticLocation => this._underlying.DiagnosticLocation;

    public override string ToString() => this.ToDisplayString();
}