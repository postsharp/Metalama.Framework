// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class NamedTypeBuilder : MemberOrNamedTypeBuilder, INamedTypeBuilder, INamedTypeImpl, ISdkType, ISdkDeclaration
{
    public GenericParameterBuilderList TypeParameters { get; } = new();

    public NamedTypeBuilder( Advice advice, INamespaceOrNamedType declaringNamespaceOrType, string name ) : base( advice, declaringNamespaceOrType as INamedType, name )
    {
        this.Namespace = declaringNamespaceOrType switch
        {
            INamespace @namespace => @namespace,
            INamedType namedType => namedType.Namespace,
            _ => throw new AssertionFailedException($"Unsupported: {declaringNamespaceOrType}"),
        };

        this.BaseType = ((CompilationModel) this.Namespace.Compilation).Factory.GetSpecialType( Code.SpecialType.Object );
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

    public override IDeclaration ContainingDeclaration => (IDeclaration?) this.DeclaringType ?? this.Namespace;

    public bool IsPartial => false;

    public bool HasDefaultConstructor => true;

    public INamedType? BaseType { get; set; }

    INamedType? INamedType.BaseType => this.BaseType;

    [Memo]
    public IImplementedInterfaceCollection AllImplementedInterfaces => new EmptyImplementedInterfaceCollection();

    [Memo]
    public IImplementedInterfaceCollection ImplementedInterfaces => new EmptyImplementedInterfaceCollection();

    public INamespace Namespace { get; }

    INamespace INamedType.Namespace => this.Namespace;

    public string FullName
        => this.DeclaringType != null
            ? $"{this.DeclaringType.FullName}.{this.Name}"
            : $"{this.Namespace.FullName}.{this.Name}";

    [Memo]
    public INamedTypeCollection NestedTypes => new EmptyNamedTypeCollection();

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

    public Code.TypeKind TypeKind => Code.TypeKind.Class;

    public Code.SpecialType SpecialType => Code.SpecialType.None;

    public bool? IsReferenceType => true;

    public bool? IsNullable => false;

    IGenericParameterList IGeneric.TypeParameters => this.TypeParameters;

    [Memo]
    public IReadOnlyList<IType> TypeArguments => Array.Empty<IType>();

    public bool IsGeneric => false;

    public bool IsCanonicalGenericInstance => false;

    Code.Accessibility IMemberOrNamedType.Accessibility => this.Accessibility;

    [Memo]
    public IAttributeCollection Attributes => new AttributeBuilderCollection();

    public int Depth => this.Namespace.Depth + 1;

    public bool BelongsToCurrentProject => true;

    public ImmutableArray<SourceReference> Sources => ImmutableArray<SourceReference>.Empty;

    IMemberOrNamedType IMemberOrNamedType.Definition => this;

    public override DeclarationKind DeclarationKind => DeclarationKind.NamedType;

    public override bool CanBeInherited => false;

    public ISymbol Symbol => this.TypeSymbol;

    [Memo]
    public ITypeSymbol TypeSymbol => new FakeNamedTypeSymbol( this );

    public bool Equals( Code.SpecialType specialType ) => false;

    public bool Equals( IType? otherType, TypeComparison typeComparison )
        => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

    public bool Equals( IType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public bool Equals( INamedType? other ) => this.Compilation.Comparers.Default.Equals( this, other );

    public bool IsSubclassOf( INamedType type ) => false;

    public Type ToType() => throw new NotImplementedException();

    public bool TryFindImplementationForInterfaceMember( IMember interfaceMember, [NotNullWhen( true )] out IMember? implementationMember )
    {
        implementationMember = null;

        return false;
    }

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.FullName;

    public IntroduceNamedTypeTransformation ToTransformation() => new( this.ParentAdvice, this );

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
        throw new NotImplementedException();
    }

#pragma warning disable RS1009 // Only internal implementations of this interface are allowed
    private class FakeNamedTypeSymbol : INamedTypeSymbol
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed
    {
        private readonly NamedTypeBuilder? _builder;
        private readonly FakeNamedTypeSymbol? _constructedFromType;
        private readonly ImmutableArray<ITypeParameterSymbol> _typeParameters;
        private readonly ImmutableArray<ITypeSymbol> _typeArguments;

        public FakeNamedTypeSymbol( NamedTypeBuilder builder )
        {
            this._builder = builder;
            this._constructedFromType = null;
            this._typeParameters = ((IEnumerable<TypeParameterBuilder>) builder.TypeParameters).Select( tp => (ITypeParameterSymbol) new FakeTypeParameterSymbol( tp ) ).ToImmutableArray();
            this._typeArguments = this._typeParameters.ToImmutableArray<ITypeSymbol>();
        }

        public FakeNamedTypeSymbol( FakeNamedTypeSymbol constructedFromType, ImmutableArray<ITypeSymbol> typeArguments )
        {
            this._builder = constructedFromType._builder;
            this._constructedFromType = constructedFromType;
            this._typeParameters = constructedFromType._typeParameters;
            this._typeArguments = typeArguments;
        }

        public int Arity => this._builder.TypeParameters.Count;

        public bool IsGenericType => this._builder.IsGeneric;

        public bool IsUnboundGenericType => this._builder.IsCanonicalGenericInstance;

        public bool IsScriptClass => false;

        public bool IsImplicitClass => false;

        public bool IsComImport => false;

        public bool IsFileLocal => false;

        public IEnumerable<string> MemberNames => Array.Empty<string>();

        public ImmutableArray<ITypeParameterSymbol> TypeParameters => this._typeParameters;

        public ImmutableArray<ITypeSymbol> TypeArguments => this._typeArguments;

        public ImmutableArray<NullableAnnotation> TypeArgumentNullableAnnotations => ImmutableArray<NullableAnnotation>.Empty;

        public INamedTypeSymbol OriginalDefinition => this;

        public IMethodSymbol? DelegateInvokeMethod => null;

        public INamedTypeSymbol? EnumUnderlyingType => null;

        public INamedTypeSymbol ConstructedFrom => this;

        public ImmutableArray<IMethodSymbol> InstanceConstructors => ImmutableArray<IMethodSymbol>.Empty;

        public ImmutableArray<IMethodSymbol> StaticConstructors => ImmutableArray<IMethodSymbol>.Empty;

        public ImmutableArray<IMethodSymbol> Constructors => ImmutableArray<IMethodSymbol>.Empty;

        public ISymbol? AssociatedSymbol => null;

        public bool MightContainExtensionMethods => false;

        public INamedTypeSymbol? TupleUnderlyingType => null;

        public ImmutableArray<IFieldSymbol> TupleElements => ImmutableArray<IFieldSymbol>.Empty;

        public bool IsSerializable => false;

        public INamedTypeSymbol? NativeIntegerUnderlyingType => null;

        public Microsoft.CodeAnalysis.TypeKind TypeKind => Microsoft.CodeAnalysis.TypeKind.Class;

        public INamedTypeSymbol? BaseType => this._builder.Compilation.Factory.GetSpecialType( Code.SpecialType.Object ).GetSymbol();

        public ImmutableArray<INamedTypeSymbol> Interfaces => ImmutableArray<INamedTypeSymbol>.Empty;

        public ImmutableArray<INamedTypeSymbol> AllInterfaces => ImmutableArray<INamedTypeSymbol>.Empty;

        public bool IsReferenceType => true;

        public bool IsValueType => false;

        public bool IsAnonymousType => false;

        public bool IsTupleType => false;

        public bool IsNativeIntegerType => false;

        public Microsoft.CodeAnalysis.SpecialType SpecialType => Microsoft.CodeAnalysis.SpecialType.None;

        public bool IsRefLikeType => false;

        public bool IsUnmanagedType => false;

        public bool IsReadOnly => false;

        public bool IsRecord => false;

        public NullableAnnotation NullableAnnotation => NullableAnnotation.None;

        public bool IsNamespace => false;

        public bool IsType => true;

        public SymbolKind Kind => SymbolKind.NamedType;

        public string Language => "C#";

        public string Name => this._builder.Name;

        public string MetadataName
            => this._builder.DeclaringType != null
                ? $"{this._builder.DeclaringType.GetSymbol().AssertNotNull().MetadataName}+{this._builder.Name}"
                : this._builder.Name;

        public int MetadataToken => 0;

        public ISymbol ContainingSymbol => this._builder.ContainingDeclaration.GetSymbol().AssertNotNull();

        public IAssemblySymbol ContainingAssembly => this._builder.ContainingDeclaration.GetSymbol().AssertNotNull().ContainingAssembly;

        public IModuleSymbol ContainingModule => this._builder.ContainingDeclaration.GetSymbol().AssertNotNull().ContainingModule;

        public INamedTypeSymbol ContainingType => (INamedTypeSymbol) this._builder.ContainingDeclaration.GetSymbol().AssertNotNull();

        public INamespaceSymbol ContainingNamespace => this._builder.ContainingDeclaration.GetSymbol().AssertNotNull().ContainingNamespace;

        public bool IsDefinition => true;

        public bool IsStatic => false;

        public bool IsVirtual => false;

        public bool IsOverride => false;

        public bool IsAbstract => false;

        public bool IsSealed => false;

        public bool IsExtern => false;

        public bool IsImplicitlyDeclared => false;

        public bool CanBeReferencedByName => false;

        public ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        public Microsoft.CodeAnalysis.Accessibility DeclaredAccessibility => Microsoft.CodeAnalysis.Accessibility.Public;

        public bool HasUnsupportedMetadata => false;

        ITypeSymbol ITypeSymbol.OriginalDefinition => this;

        ISymbol ISymbol.OriginalDefinition => this;

        public void Accept( SymbolVisitor visitor ) => visitor.VisitNamedType( this );

        public TResult? Accept<TResult>( SymbolVisitor<TResult> visitor ) => visitor.VisitNamedType( this );

        public TResult Accept<TArgument, TResult>( SymbolVisitor<TArgument, TResult> visitor, TArgument argument ) => visitor.VisitNamedType( this, argument );

        public INamedTypeSymbol Construct( params ITypeSymbol[] typeArguments )
        {
            return new FakeNamedTypeSymbol( this, typeArguments.ToImmutableArray() );
        }

        public INamedTypeSymbol Construct( ImmutableArray<ITypeSymbol> typeArguments, ImmutableArray<NullableAnnotation> typeArgumentNullableAnnotations )
            => throw new NotImplementedException();

        public INamedTypeSymbol ConstructUnboundGenericType() => throw new NotImplementedException();

        public bool Equals( [NotNullWhen( true )] ISymbol? other, SymbolEqualityComparer equalityComparer ) => this == other.OriginalDefinition;

        public bool Equals( ISymbol? other ) => this == other;

        public ISymbol? FindImplementationForInterfaceMember( ISymbol interfaceMember ) => null;

        public ImmutableArray<AttributeData> GetAttributes() => ImmutableArray<AttributeData>.Empty;

        public string? GetDocumentationCommentId() => throw new NotImplementedException();

        public string? GetDocumentationCommentXml(
            CultureInfo? preferredCulture = null,
            bool expandIncludes = false,
            CancellationToken cancellationToken = default )
            => throw new NotImplementedException();

        public ImmutableArray<ISymbol> GetMembers() => ImmutableArray<ISymbol>.Empty;

        public ImmutableArray<ISymbol> GetMembers( string name ) => ImmutableArray<ISymbol>.Empty;

        public ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers( int ordinal ) => ImmutableArray<CustomModifier>.Empty;

        public ImmutableArray<INamedTypeSymbol> GetTypeMembers() => ImmutableArray<INamedTypeSymbol>.Empty;

        public ImmutableArray<INamedTypeSymbol> GetTypeMembers( string name ) => ImmutableArray<INamedTypeSymbol>.Empty;

        public ImmutableArray<INamedTypeSymbol> GetTypeMembers( string name, int arity ) => ImmutableArray<INamedTypeSymbol>.Empty;

        public ImmutableArray<SymbolDisplayPart> ToDisplayParts( NullableFlowState topLevelNullability, SymbolDisplayFormat? format = null )
            => throw new NotImplementedException();

        public ImmutableArray<SymbolDisplayPart> ToDisplayParts( SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public string ToDisplayString( NullableFlowState topLevelNullability, SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public string ToDisplayString( SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(
            SemanticModel semanticModel,
            NullableFlowState topLevelNullability,
            int position,
            SymbolDisplayFormat? format = null )
            => throw new NotImplementedException();

        public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts( SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null )
            => throw new NotImplementedException();

        public string ToMinimalDisplayString(
            SemanticModel semanticModel,
            NullableFlowState topLevelNullability,
            int position,
            SymbolDisplayFormat? format = null )
            => throw new NotImplementedException();

        public string ToMinimalDisplayString( SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null )
            => throw new NotImplementedException();

        public ITypeSymbol WithNullableAnnotation( NullableAnnotation nullableAnnotation ) => throw new NotImplementedException();
    }

#pragma warning disable RS1009 // Only internal implementations of this interface are allowed
    private class FakeTypeParameterSymbol : ITypeParameterSymbol
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed
    {
        private readonly TypeParameterBuilder _builder;

        public FakeTypeParameterSymbol(TypeParameterBuilder builder)
        {
            this._builder = builder;
        }

        public int Ordinal => this._builder.Index;

        public Microsoft.CodeAnalysis.VarianceKind Variance => Microsoft.CodeAnalysis.VarianceKind.None;

        public TypeParameterKind TypeParameterKind => TypeParameterKind.Type;

        public IMethodSymbol? DeclaringMethod => null;

        public INamedTypeSymbol? DeclaringType => (INamedTypeSymbol) this._builder.ContainingDeclaration.GetSymbol().AssertNotNull();

        public bool HasReferenceTypeConstraint => false;

        public NullableAnnotation ReferenceTypeConstraintNullableAnnotation => NullableAnnotation.NotAnnotated;

        public bool HasValueTypeConstraint => false;

        public bool HasUnmanagedTypeConstraint => false;

        public bool HasNotNullConstraint => false;

        public bool HasConstructorConstraint => false;

        public ImmutableArray<ITypeSymbol> ConstraintTypes => ImmutableArray<ITypeSymbol>.Empty;

        public ImmutableArray<NullableAnnotation> ConstraintNullableAnnotations => ImmutableArray<NullableAnnotation>.Empty;

        public ITypeParameterSymbol OriginalDefinition => this;

        public ITypeParameterSymbol? ReducedFrom => null;

        public Microsoft.CodeAnalysis.TypeKind TypeKind => Microsoft.CodeAnalysis.TypeKind.TypeParameter;

        public INamedTypeSymbol? BaseType => null;

        public ImmutableArray<INamedTypeSymbol> Interfaces => ImmutableArray<INamedTypeSymbol>.Empty;

        public ImmutableArray<INamedTypeSymbol> AllInterfaces => ImmutableArray<INamedTypeSymbol>.Empty;

        public bool IsReferenceType => false;

        public bool IsValueType => false;

        public bool IsAnonymousType => false;

        public bool IsTupleType => false;

        public bool IsNativeIntegerType => false;

        public Microsoft.CodeAnalysis.SpecialType SpecialType => Microsoft.CodeAnalysis.SpecialType.None;

        public bool IsRefLikeType => false;

        public bool IsUnmanagedType => false;

        public bool IsReadOnly => false;

        public bool IsRecord => false;

        public NullableAnnotation NullableAnnotation => NullableAnnotation.NotAnnotated;

        public bool IsNamespace => false;

        public bool IsType => true;

        public SymbolKind Kind => SymbolKind.TypeParameter;

        public string Language => "C#";

        public string Name => this._builder.Name;

        public string MetadataName => $"``{this.Ordinal}";

        public int MetadataToken => 0;

        public ISymbol ContainingSymbol => this.DeclaringType;

        public IAssemblySymbol ContainingAssembly => this.DeclaringType.ContainingAssembly;

        public IModuleSymbol ContainingModule => this.DeclaringType.ContainingModule;

        public INamedTypeSymbol ContainingType => this.DeclaringType;

        public INamespaceSymbol ContainingNamespace => this.DeclaringType.ContainingNamespace;

        public bool IsDefinition => true;

        public bool IsStatic => false;

        public bool IsVirtual => false;

        public bool IsOverride => false;

        public bool IsAbstract => false;

        public bool IsSealed => false;

        public bool IsExtern => false;

        public bool IsImplicitlyDeclared => false;

        public bool CanBeReferencedByName => true;

        public ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        public Microsoft.CodeAnalysis.Accessibility DeclaredAccessibility => Microsoft.CodeAnalysis.Accessibility.NotApplicable;

        public bool HasUnsupportedMetadata => false;

        ITypeSymbol ITypeSymbol.OriginalDefinition => this;

        ISymbol ISymbol.OriginalDefinition => this;

        public void Accept( SymbolVisitor visitor ) => visitor.VisitTypeParameter( this );

        public TResult? Accept<TResult>( SymbolVisitor<TResult> visitor ) => visitor.VisitTypeParameter( this );

        public TResult Accept<TArgument, TResult>( SymbolVisitor<TArgument, TResult> visitor, TArgument argument ) => visitor.VisitTypeParameter( this, argument );

        public bool Equals( [NotNullWhen( true )] ISymbol? other, SymbolEqualityComparer equalityComparer ) => this == other.OriginalDefinition;

        public bool Equals( ISymbol? other ) => this == other;

        public ISymbol? FindImplementationForInterfaceMember( ISymbol interfaceMember ) => null;

        public ImmutableArray<AttributeData> GetAttributes() => ImmutableArray<AttributeData>.Empty;

        public string? GetDocumentationCommentId() => throw new NotImplementedException();

        public string? GetDocumentationCommentXml( CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default ) => throw new NotImplementedException();

        public ImmutableArray<ISymbol> GetMembers() => ImmutableArray<ISymbol>.Empty;

        public ImmutableArray<ISymbol> GetMembers( string name ) => ImmutableArray<ISymbol>.Empty;

        public ImmutableArray<INamedTypeSymbol> GetTypeMembers() => ImmutableArray<INamedTypeSymbol>.Empty;

        public ImmutableArray<INamedTypeSymbol> GetTypeMembers( string name ) => ImmutableArray<INamedTypeSymbol>.Empty;

        public ImmutableArray<INamedTypeSymbol> GetTypeMembers( string name, int arity ) => ImmutableArray<INamedTypeSymbol>.Empty;

        public ImmutableArray<SymbolDisplayPart> ToDisplayParts( NullableFlowState topLevelNullability, SymbolDisplayFormat? format = null )
        {
            throw new NotImplementedException();
        }

        public ImmutableArray<SymbolDisplayPart> ToDisplayParts( SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public string ToDisplayString( NullableFlowState topLevelNullability, SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public string ToDisplayString( SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts( SemanticModel semanticModel, NullableFlowState topLevelNullability, int position, SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts( SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public string ToMinimalDisplayString( SemanticModel semanticModel, NullableFlowState topLevelNullability, int position, SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public string ToMinimalDisplayString( SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null ) => throw new NotImplementedException();

        public ITypeSymbol WithNullableAnnotation( NullableAnnotation nullableAnnotation ) => throw new NotImplementedException();
    }
}