﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Accessibility = Metalama.Framework.Code.Accessibility;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Source;

internal sealed class NamedTypeImpl : MemberOrNamedType, INamedTypeImpl
{
    private readonly NamedType _facade;

    ITypeSymbol ISdkType.TypeSymbol => this.NamedTypeSymbol;

    public override ISymbol Symbol => this.NamedTypeSymbol;

    public override bool CanBeInherited => this.IsReferenceType.GetValueOrDefault() && !this.IsSealed;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
        => this.Compilation.GetDerivedTypes( this, options );

    internal NamedTypeImpl( NamedType facade, INamedTypeSymbol namedTypeSymbol, CompilationModel compilation ) : base( compilation )
    {
        this._facade = facade;
        this.NamedTypeSymbol = namedTypeSymbol.AssertBelongsToCompilationContext( compilation.CompilationContext );
    }

    TypeKind IType.TypeKind
        => this.NamedTypeSymbol.TypeKind switch
        {
            Microsoft.CodeAnalysis.TypeKind.Class when !this.NamedTypeSymbol.IsRecord => TypeKind.Class,
            Microsoft.CodeAnalysis.TypeKind.Class when this.NamedTypeSymbol.IsRecord => TypeKind.RecordClass,
            Microsoft.CodeAnalysis.TypeKind.Delegate => TypeKind.Delegate,
            Microsoft.CodeAnalysis.TypeKind.Enum => TypeKind.Enum,
            Microsoft.CodeAnalysis.TypeKind.Interface => TypeKind.Interface,
            Microsoft.CodeAnalysis.TypeKind.Struct when !this.NamedTypeSymbol.IsRecord => TypeKind.Struct,
            Microsoft.CodeAnalysis.TypeKind.Struct when this.NamedTypeSymbol.IsRecord => TypeKind.RecordStruct,
            Microsoft.CodeAnalysis.TypeKind.Error => TypeKind.Error,
            _ => throw new InvalidOperationException( $"Unexpected type kind for '{this.NamedTypeSymbol}': {this.NamedTypeSymbol.TypeKind}." )
        };

    [Memo]
    public SpecialType SpecialType => this.GetSpecialTypeCore();

    private SpecialType GetSpecialTypeCore()
    {
        var specialType = this.NamedTypeSymbol.SpecialType.ToOurSpecialType();

        if ( specialType != SpecialType.None )
        {
            return specialType;
        }
        else if ( this.IsGeneric )
        {
            switch ( this.NamedTypeSymbol.Name )
            {
                case "IAsyncEnumerable" when this.IsCanonicalGenericInstance
                                             && this.NamedTypeSymbol.ContainingNamespace.ToDisplayString() == "System.Collections.Generic":
                    return SpecialType.IAsyncEnumerable_T;

                case "IAsyncEnumerator" when this.IsCanonicalGenericInstance
                                             && this.NamedTypeSymbol.ContainingNamespace.ToDisplayString() == "System.Collections.Generic":
                    return SpecialType.IAsyncEnumerator_T;

                case nameof(ValueTask)
                    when this.IsCanonicalGenericInstance && this.NamedTypeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks":
                    return SpecialType.ValueTask_T;

                case nameof(Task)
                    when this.IsCanonicalGenericInstance && this.NamedTypeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks":
                    return SpecialType.Task_T;
            }

            return SpecialType.None;
        }
        else
        {
            return this.NamedTypeSymbol.Name switch
            {
                nameof(ValueTask) when this.NamedTypeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks"
                    => SpecialType.ValueTask,
                nameof(Task) when this.NamedTypeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks"
                    => SpecialType.Task,
                nameof(Type) when this.NamedTypeSymbol.ContainingNamespace.ToDisplayString() == "System"
                    => SpecialType.Type,
                _ => SpecialType.None
            };
        }
    }

    public Type ToType() => this.Compilation.Factory.GetReflectionType( this.NamedTypeSymbol );

    public bool? IsReferenceType => this.NamedTypeSymbol.IsReferenceType;

    public bool? IsNullable => this.NamedTypeSymbol.IsNullable();

    public bool Equals( SpecialType specialType ) => this.SpecialType == specialType;

    public bool Equals( IType? otherType, TypeComparison typeComparison )
        => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this._facade, otherType );

    public override MemberInfo ToMemberInfo() => this.ToType();

    public bool IsReadOnly => this.NamedTypeSymbol.IsReadOnly;

    public bool IsRef => this.NamedTypeSymbol.IsRefLikeType;

    public bool HasDefaultConstructor
        => this.NamedTypeSymbol.TypeKind == Microsoft.CodeAnalysis.TypeKind.Struct ||
           (this.NamedTypeSymbol is { TypeKind: Microsoft.CodeAnalysis.TypeKind.Class, IsAbstract: false } &&
            this.NamedTypeSymbol.InstanceConstructors.Any( ctor => ctor.Parameters.Length == 0 ));

    public bool IsGeneric => this.NamedTypeSymbol.IsGenericType;

    public bool IsCanonicalGenericInstance => this.NamedTypeSymbol.OriginalDefinition == this.NamedTypeSymbol;

    [Memo]
    public INamedTypeCollection Types
        => new NamedTypeCollection(
            this._facade,
            this.Compilation.GetNamedTypeCollectionByParent( this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ) ) );

    INamedTypeCollection INamedType.NestedTypes => this.Types;

    [Memo]
    public INamedTypeCollection AllTypes => new AllTypesCollection( this._facade );

    [Memo]
    public IPropertyCollection Properties
        => new PropertyCollection(
            this._facade,
            this.Compilation.GetPropertyCollection( this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ) ) );

    [Memo]
    public IPropertyCollection AllProperties => new AllPropertiesCollection( this._facade );

    [Memo]
    public IIndexerCollection Indexers
        => new IndexerCollection(
            this._facade,
            this.Compilation.GetIndexerCollection( this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ) ) );

    [Memo]
    public IIndexerCollection AllIndexers => new AllIndexersCollection( this._facade );

    [Memo]
    public IFieldCollection Fields
        => new FieldCollection(
            this._facade,
            this.Compilation.GetFieldCollection( this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ) ) );

    [Memo]
    public IFieldCollection AllFields => new AllFieldsCollection( this._facade );

    [Memo]
    public IFieldOrPropertyCollection FieldsAndProperties => new FieldAndPropertiesCollection( this.Fields, this.Properties );

    [Memo]
    public IFieldOrPropertyCollection AllFieldsAndProperties => new AllFieldsAndPropertiesCollection( this._facade );

    [Memo]
    public IEventCollection Events
        => new EventCollection(
            this._facade,
            this.Compilation.GetEventCollection( this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ) ) );

    [Memo]
    public IEventCollection AllEvents => new AllEventsCollection( this._facade );

    [Memo]
    public IMethodCollection Methods
        => new MethodCollection(
            this._facade,
            this.Compilation.GetMethodCollection( this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ) ) );

    [Memo]
    public IMethodCollection AllMethods => new AllMethodsCollection( this._facade );

    [Memo]
    public IConstructorCollection Constructors
        => new ConstructorCollection(
            this._facade,
            this.Compilation.GetConstructorCollection( this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ) ) );

    [Memo]
    public IConstructor? PrimaryConstructor => this.GetPrimaryConstructorImpl();

    [Memo]
    public IConstructor? StaticConstructor => this.GetStaticConstructorImpl();

    public IMethod? Finalizer => this.GetFinalizerImpl();

    private IConstructor? GetPrimaryConstructorImpl()
    {
        var constructors = this.Compilation.GetConstructorCollection( this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ) );

        foreach ( var constructor in constructors )
        {
            if ( constructor is ISymbolRef { Symbol: IMethodSymbol methodSymbol } && methodSymbol.IsPrimaryConstructor() )
            {
                return this.Compilation.Factory.GetConstructor( methodSymbol );
            }

            // TODO: Builders? (In case we e.g. add a parameter)
        }

        return null;
    }

    private IConstructor? GetStaticConstructorImpl()
    {
        var builder = this.Compilation.GetStaticConstructor( this.NamedTypeSymbol );

        if ( builder != null )
        {
            return this.Compilation.Factory.GetConstructor( builder );
        }

        var symbol = this.NamedTypeSymbol.StaticConstructors.SingleOrDefault();

        if ( symbol != null )
        {
            return this.Compilation.Factory.GetConstructor( symbol );
        }

        return null;
    }

    private IMethod? GetFinalizerImpl()
    {
        var builder = this.Compilation.GetFinalizer( this.NamedTypeSymbol );

        if ( builder != null )
        {
            return this.Compilation.Factory.GetMethod( builder );
        }

        var symbol = this.NamedTypeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .SingleOrDefault( m => m is { Name: "Finalize", TypeParameters.Length: 0, Parameters.Length: 0 } );

        if ( symbol != null )
        {
            return this.Compilation.Factory.GetMethod( symbol );
        }

        return null;
    }

    public override bool IsPartial
    {
        get
        {
            var syntaxReference = this.NamedTypeSymbol.GetPrimarySyntaxReference();

            if ( syntaxReference == null )
            {
                return false;
            }

            var syntax = syntaxReference.GetSyntax();

            var modifiers = syntax switch
            {
                TypeDeclarationSyntax type => type.Modifiers,
                EnumDeclarationSyntax e => e.Modifiers,
                DelegateDeclarationSyntax d => d.Modifiers,
                _ => default
            };

            return modifiers.Any( m => m.IsKind( SyntaxKind.PartialKeyword ) );
        }
    }

    [Memo]
    public ITypeParameterList TypeParameters
        => new TypeParameterList(
            this._facade.Definition,
            this.NamedTypeSymbol.TypeParameters.Select( x => this.RefFactory.FromSymbol<ITypeParameter>( x ) )
                .ToReadOnlyList() );

    INamespace INamedType.Namespace => this.ContainingNamespace;

    [Memo]
    public INamespace ContainingNamespace
        =>

            // Empty error type symbols (like unspecified type parameter) have null namespace.
            // Other error types usually have assembly-specific global namespace.
            this.NamedTypeSymbol.ContainingNamespace != null
                ? this.Compilation.Factory.GetNamespace( this.NamedTypeSymbol.ContainingNamespace )
                : this.Compilation.GlobalNamespace;

    IRef<INamespaceOrNamedType> INamespaceOrNamedType.ToRef() => this.Ref;

    [Memo]
    public string FullName => this.NamedTypeSymbol.GetFullName().AssertNotNull();

    [Memo]
    public IReadOnlyList<IType> TypeArguments => this.NamedTypeSymbol.TypeArguments.Select( a => this.Compilation.Factory.GetIType( a ) ).ToImmutableList();

    [Memo]
    public override IDeclaration ContainingDeclaration
        => this.NamedTypeSymbol.ContainingSymbol switch
        {
            INamespaceSymbol => this.Compilation.Factory.GetAssembly( this.NamedTypeSymbol.ContainingAssembly ),
            INamedTypeSymbol containingType => this.Compilation.Factory.GetNamedType( containingType ),
            null => this.Compilation, // Empty error type symbol goes here. Other error types return a namespace, which we handle above.
            _ => throw new AssertionFailedException( $"Unexpected containing symbol kind: {this.NamedTypeSymbol.ContainingSymbol.Kind}." )
        };

    public override DeclarationKind DeclarationKind => DeclarationKind.NamedType;

    [Memo]
    public INamedType? BaseType => this.NamedTypeSymbol.BaseType == null ? null : this.Compilation.Factory.GetNamedType( this.NamedTypeSymbol.BaseType );

    [Memo]
    public IImplementedInterfaceCollection AllImplementedInterfaces
        => new AllImplementedInterfacesCollection(
            this._facade,
            this.Compilation.GetAllInterfaceImplementationCollection(
                this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ),
                false ) );

    [Memo]
    public IImplementedInterfaceCollection ImplementedInterfaces
        => new ImplementedInterfacesCollection(
            this._facade,
            this.Compilation.GetInterfaceImplementationCollection(
                this.NamedTypeSymbol.OriginalDefinition.ToRef( this.GetCompilationContext() ),
                false ) );

    ICompilation ICompilationElement.Compilation => this.Compilation;

    IGeneric IGenericInternal.ConstructGenericInstance( IReadOnlyList<IType> typeArguments )
    {
        var typeArgumentSymbols =
            typeArguments.SelectAsArray( a => a.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ) );

        var typeSymbol = this.NamedTypeSymbol;
        var constructedTypeSymbol = typeSymbol.ConstructedFrom.Construct( typeArgumentSymbols );

        return this.Compilation.Factory.GetNamedType( constructedTypeSymbol );
    }

    public IReadOnlyList<IMember> GetOverridingMembers( IMember member )
    {
        var isInterfaceMember = member.DeclaringType.TypeKind == TypeKind.Interface;

        if ( !((IDeclarationImpl) member).CanBeInherited )
        {
            return Array.Empty<IMember>();
        }

        IMemberCollection<IMember> members;

        switch ( member.DeclarationKind )
        {
            case DeclarationKind.Method:
                members = this.Methods;

                break;

            case DeclarationKind.Property:
                members = this.Properties;

                break;

            case DeclarationKind.Event:
                members = this.Events;

                break;

            case DeclarationKind.Indexer:
                members = this.Indexers;

                break;

            default:
                return Array.Empty<IMember>();
        }

        var candidates = members.OfName( member.Name );

        var overridingMembers = new List<IMember>();

        foreach ( var candidate in candidates )
        {
            if ( isInterfaceMember )
            {
                if ( ((INamedTypeImpl) candidate.DeclaringType).IsImplementationOfInterfaceMember( candidate, member ) )
                {
                    overridingMembers.Add( candidate );
                }
            }
            else
            {
                // Override. Look for overrides.
                for ( var c = (IMemberImpl) candidate; c != null; c = (IMemberImpl?) c.OverriddenMember )
                {
                    if ( c.OverriddenMember?.Definition == member )
                    {
                        overridingMembers.Add( candidate );
                    }
                }
            }
        }

        return overridingMembers;
    }

    public bool IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember )
    {
        // Some trivial checks first.
        if ( !typeMember.Name.EndsWith( interfaceMember.Name, StringComparison.Ordinal )
             || typeMember.DeclarationKind != interfaceMember.DeclarationKind
             || !(typeMember.Accessibility == Accessibility.Public || typeMember.IsExplicitInterfaceImplementation) )
        {
            return false;
        }

        var interfaceType = interfaceMember.DeclaringType.GetSymbol();
        var relevantInterfaces = this.GetAllInterfaces().Where( t => t.Equals( interfaceType ) || t.ConstructedFrom.Equals( interfaceType ) );

        foreach ( var implementedInterface in relevantInterfaces )
        {
            foreach ( var candidateSymbol in implementedInterface.GetMembers( interfaceMember.Name ) )
            {
                var candidateMember = (IMember) this.Compilation.Factory.GetDeclaration( candidateSymbol );

                if ( (candidateMember.SignatureEquals( interfaceMember )
                      || candidateMember.Definition.SignatureEquals( interfaceMember ))
                     && candidateMember.SignatureEquals( typeMember ) )
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsSubclassOf( INamedType type )
    {
        // TODO: enum.IsSubclassOf(int) == true etc.
        if ( type.TypeKind is TypeKind.Class or TypeKind.RecordClass )
        {
            INamedType? currentType = this;

            while ( currentType != null )
            {
                if ( this.Compilation.Comparers.Default.Equals( currentType, type ) )
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }
        else if ( type.TypeKind == TypeKind.Interface )
        {
            return this.ImplementedInterfaces.SingleOrDefault( i => this.Compilation.Comparers.Default.Equals( i, type ) ) != null;
        }
        else
        {
            return this.Compilation.Comparers.Default.Equals( this, type );
        }
    }

    private static IMember? FindMemberOfSignature( INamedType type, IMember member )
    {
        IMember? candidate = member switch
        {
            IMethod method => type.Methods.OfExactSignature( method ),
            IConstructor constructor => type.Constructors.OfExactSignature( constructor ),
            IProperty property => type.Properties.OfName( property.Name ).SingleOrDefault(),
            IIndexer indexer => type.Indexers.OfExactSignature( indexer ),
            IEvent @event => type.Events.OfName( @event.Name ).SingleOrDefault(),
            _ => throw new AssertionFailedException( $"Unexpected member kind: {member.DeclarationKind}." )
        };

        if ( StructuralDeclarationComparer.ContainingDeclarationOblivious.Equals( candidate, member ) )
        {
            return candidate;
        }

        return null;
    }

    public bool TryFindImplementationForInterfaceMember( IMember interfaceMember, [NotNullWhen( true )] out IMember? implementationMember )
    {
        // Fastest track: check if we are the required interface itself.
        if ( interfaceMember.DeclaringType.Equals( this ) )
        {
            implementationMember = interfaceMember;

            return true;
        }

        // Try to find using symbols.
        var symbolInterfaceMemberSymbol = interfaceMember.GetSymbol();

        if ( symbolInterfaceMemberSymbol != null )
        {
            var symbolInterfaceMemberImplementationSymbol =
                this.NamedTypeSymbol.FindImplementationForInterfaceMember( symbolInterfaceMemberSymbol );

            if ( symbolInterfaceMemberImplementationSymbol != null )
            {
                implementationMember = (IMember) this.Compilation.Factory.GetDeclaration( symbolInterfaceMemberImplementationSymbol );

                return true;
            }
        }

        // Find the member in introduced interfaces, including in subtypes.
        var currentTypeDefinition = this.Definition;
        var currentTypeSymbol = this.NamedTypeSymbol;

        while ( true )
        {
            var currentGenericContext = GenericContext.Get( currentTypeSymbol, this.GetCompilationContext() );

            var introducedInterface =
                this.Compilation
                    .GetInterfaceImplementationCollection( currentTypeDefinition.ToFullRef(), false )
                    .Introductions
                    .SingleOrDefault( i => currentGenericContext.Map( i.InterfaceType.GetTarget( this.Compilation ) ).Equals( interfaceMember.DeclaringType ) );

            if ( introducedInterface != null )
            {
                // TODO: Use the generic map to match (compare) the member in the following call to MemberMap.TryGetValue or FindMemberOfSignature.

                if ( introducedInterface.MemberMap.TryGetValue( interfaceMember, out var interfaceMemberImplementation ) )
                {
                    // We found it as an explicit member.
                    implementationMember = interfaceMemberImplementation.ForCompilation( this.Compilation );

                    return true;
                }
                else
                {
                    // Match in all members.
                    var candidate = FindMemberOfSignature( currentTypeDefinition, interfaceMember );

                    if ( candidate?.Accessibility == Accessibility.Public )
                    {
                        implementationMember = candidate.ForCompilation( this.Compilation );

                        return true;
                    }
                    else
                    {
                        implementationMember = null;

                        return false;
                    }
                }
            }

            if ( currentTypeSymbol.BaseType == null )
            {
                break;
            }
            else
            {
                currentTypeSymbol = currentTypeSymbol.BaseType;
                currentTypeDefinition = currentTypeDefinition.BaseType.AssertNotNull().Definition;
            }
        }

        implementationMember = null;

        return false;
    }

    public INamedTypeSymbol NamedTypeSymbol { get; }

    [Memo]
    public INamedType Definition
        => this.NamedTypeSymbol.Equals( this.NamedTypeSymbol.OriginalDefinition )
            ? this
            : this.Compilation.Factory.GetNamedType( this.NamedTypeSymbol.OriginalDefinition );

    INamedType INamedType.TypeDefinition => throw new NotSupportedException();

    protected override IMemberOrNamedType GetDefinitionMemberOrNamedType() => throw new NotSupportedException();

    [Memo]
    public INamedType UnderlyingType => this.GetUnderlyingTypeCore();

    private INamedType GetUnderlyingTypeCore()
    {
        var enumUnderlyingType = this.NamedTypeSymbol.EnumUnderlyingType;

        if ( enumUnderlyingType != null )
        {
            return this.Compilation.Factory.GetNamedType( enumUnderlyingType );
        }

        var isNullable = this.IsNullable;

        if ( isNullable != null )
        {
            if ( this.IsReferenceType == true )
            {
                // We have an annotated reference type, return the non-annotated type.
                return this.Compilation.Factory.GetNamedType( (INamedTypeSymbol) this.NamedTypeSymbol.WithNullableAnnotation( NullableAnnotation.None ) );
            }
        }

        // Fall back to self.
        return this._facade;
    }

    private void PopulateAllInterfaces( ImmutableHashSet<INamedTypeSymbol>.Builder builder, in SymbolBasedGenericMap genericMap )
    {
        var compilation = this.Compilation.RoslynCompilation;

        // Process the Roslyn type system.
        foreach ( var type in this.NamedTypeSymbol.Interfaces )
        {
            builder.Add( genericMap.SubstituteSymbol( type, compilation ) );
        }

        if ( this.NamedTypeSymbol.BaseType != null )
        {
            var newGenericMap = genericMap.SubstituteSymbols( this.NamedTypeSymbol.BaseType.TypeArguments, compilation );
            ((NamedType) this.BaseType!).Implementation.PopulateAllInterfaces( builder, newGenericMap );
        }

        // TODO: process introductions.
    }

    private ImmutableHashSet<INamedTypeSymbol> GetAllInterfaces()
    {
        var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>( this.GetCompilationContext().SymbolComparer );
        this.PopulateAllInterfaces( builder, SymbolBasedGenericMap.Empty );

        return builder.ToImmutable();
    }

    IType ITypeImpl.Accept( TypeRewriter visitor ) => throw new NotSupportedException();

    public bool Equals( IType? other ) => this.Equals( other, TypeComparison.Default );

    public bool Equals( INamedType? other ) => this.Equals( other, TypeComparison.Default );

    public override int GetHashCode() => this.GetCompilationContext().SymbolComparer.GetHashCode( this.NamedTypeSymbol );

    [Memo]
    public IFullRef<INamedType> Ref => this.RefFactory.FromSymbolBasedDeclaration<INamedType>( this );

    private protected override IFullRef<IDeclaration> ToFullDeclarationRef() => this.Ref;

    IRef<INamedType> INamedType.ToRef() => this.Ref;

    IRef<IType> IType.ToRef() => this.Ref;

    protected override IRef<IMemberOrNamedType> ToMemberOrNamedTypeRef() => this.Ref;
}