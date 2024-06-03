// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Accessibility = Metalama.Framework.Code.Accessibility;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class NamedTypeBuilder : MemberOrNamedTypeBuilder, INamedTypeBuilder, INamedTypeImpl
{
    private bool _isPartial;
    private INamedType? _baseType;

    public GenericParameterBuilderList TypeParameters { get; } = new();

    public NamedTypeBuilder( Advice advice, INamespaceOrNamedType declaringNamespaceOrType, string name ) : base(
        advice,
        declaringNamespaceOrType as INamedType,
        name )
    {
        this.ContainingNamespace = declaringNamespaceOrType switch
        {
            INamespace @namespace => @namespace,
            INamedType namedType => namedType.ContainingNamespace,
            _ => throw new AssertionFailedException( $"Unsupported: {declaringNamespaceOrType}" )
        };

        this.BaseType = ((CompilationModel) this.ContainingNamespace.Compilation).Factory.GetSpecialType( SpecialType.Object );
    }

    public override void Freeze()
    {
        base.Freeze();

        foreach ( var typeParameter in this.TypeParameters )
        {
            typeParameter.Freeze();
        }
    }

    public ITypeParameterBuilder AddTypeParameter( string name )
    {
        this.CheckNotFrozen();

        var builder = new TypeParameterBuilder( this, this.TypeParameters.Count, name );
        this.TypeParameters.Add( builder );

        return builder;
    }

    public override IDeclaration ContainingDeclaration => (IDeclaration?) this.DeclaringType ?? this.ContainingNamespace;

    public bool IsPartial
    {
        get => this._isPartial;
        set
        {
            this.CheckNotFrozen();

            this._isPartial = value;
        }
    }

    public bool HasDefaultConstructor => true;

    public INamedType? BaseType
    {
        get => this._baseType;
        set
        {
            this.CheckNotFrozen();

            this._baseType = value;
        }
    }

    [Memo]
    public IImplementedInterfaceCollection AllImplementedInterfaces => new EmptyImplementedInterfaceCollection();

    [Memo]
    public IImplementedInterfaceCollection ImplementedInterfaces => new EmptyImplementedInterfaceCollection();

    INamespace INamedType.Namespace => this.ContainingNamespace;

    public INamespace ContainingNamespace { get; }

    INamedTypeCollection INamedType.NestedTypes => this.Types;

    INamespace INamedType.ContainingNamespace => this.ContainingNamespace;

    public string FullName
        => this.DeclaringType != null
            ? $"{this.DeclaringType.FullName}.{this.Name}"
            : $"{this.ContainingNamespace.FullName}.{this.Name}";

    [Memo]
    public INamedTypeCollection Types => new EmptyNamedTypeCollection();

    [Memo]
    public IPropertyCollection Properties => new EmptyPropertyCollection( this );

    [Memo]
    public IPropertyCollection AllProperties => new EmptyPropertyCollection( this );

    [Memo]
    public IIndexerCollection Indexers => new EmptyIndexerCollection( this );

    [Memo]
    public IIndexerCollection AllIndexers => new EmptyIndexerCollection( this );

    [Memo]
    public IFieldCollection Fields => new EmptyFieldCollection( this );

    [Memo]
    public IFieldCollection AllFields => new EmptyFieldCollection( this );

    [Memo]
    public IFieldOrPropertyCollection FieldsAndProperties => new EmptyFieldOrPropertyCollection( this );

    [Memo]
    public IFieldOrPropertyCollection AllFieldsAndProperties => new EmptyFieldOrPropertyCollection( this );

    [Memo]
    public IEventCollection Events => new EmptyEventCollection( this );

    [Memo]
    public IEventCollection AllEvents => new EmptyEventCollection( this );

    [Memo]
    public IMethodCollection Methods => new EmptyMethodCollection( this );

    [Memo]
    public IMethodCollection AllMethods => new EmptyMethodCollection( this );

    public IConstructor? PrimaryConstructor => null;

    [Memo]
    public IConstructorCollection Constructors => new EmptyConstructorCollection( this );

    public IConstructor? StaticConstructor => null;

    public IMethod? Finalizer => null;

    public bool IsReadOnly => false;

    public bool IsRef => false;

    public INamedType TypeDefinition => this;

    public INamedType Definition => this;

    public INamedType UnderlyingType => this;

    public TypeKind TypeKind => TypeKind.Class;

    public SpecialType SpecialType => SpecialType.None;

    public bool? IsReferenceType => true;

    public bool? IsNullable => false;

    IGenericParameterList IGeneric.TypeParameters => this.TypeParameters;

    [Memo]
    public IReadOnlyList<IType> TypeArguments => Array.Empty<IType>();

    public bool IsGeneric => false;

    public bool IsCanonicalGenericInstance => false;

    Accessibility IMemberOrNamedType.Accessibility => this.Accessibility;

    [Memo]
    public IAttributeCollection Attributes => new AttributeBuilderCollection();

    public int Depth => this.ContainingNamespace.Depth + 1;

    public bool BelongsToCurrentProject => true;

    public ImmutableArray<SourceReference> Sources => ImmutableArray<SourceReference>.Empty;

    IMemberOrNamedType IMemberOrNamedType.Definition => this;

    public override DeclarationKind DeclarationKind => DeclarationKind.NamedType;

    public override bool CanBeInherited => false;

    public ISymbol? Symbol => this.TypeSymbol;

    public ITypeSymbol? TypeSymbol => null;

    public bool Equals( SpecialType specialType ) => false;

    public bool Equals( IType? otherType, TypeComparison typeComparison )
        => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

    public bool Equals( IType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public bool Equals( INamedType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public bool IsSubclassOf( INamedType type ) => false;

    public Type ToType() => throw new NotImplementedException();

    public bool TryFindImplementationForInterfaceMember( IMember interfaceMember, [NotNullWhen( true )] out IMember? implementationMember )
    {
        throw new NotSupportedException( "This method is not supported on the builder." );
    }

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.FullName;

    public IntroduceNamedTypeTransformation ToTransformation() => new( this.ParentAdvice, this );

    IReadOnlyList<IMember> INamedTypeImpl.GetOverridingMembers( IMember member )
    {
        throw new NotSupportedException( "This method is not supported on the builder." );
    }

    bool INamedTypeImpl.IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember )
    {
        throw new NotSupportedException( "This method is not supported on the builder." );
    }

    ITypeImpl ITypeImpl.Accept( TypeRewriter visitor )
    {
        return visitor.Visit( this );
    }

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments )
    {
        throw new NotImplementedException();
    }

    public override SyntaxTree PrimarySyntaxTree
        =>

            // TODO: This is a bit of circular dependency.
            this.ToInsertPosition().SyntaxTree;
}