﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Impl.CodeModel.Collections;
using Metalama.Framework.Impl.CodeModel.References;
using Metalama.Framework.Impl.Collections;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Transformations;
using Metalama.Framework.Impl.Utilities;
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
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Impl.CodeModel
{
    internal sealed class NamedType : MemberOrNamedType, INamedTypeInternal
    {
        private SpecialType? _specialType;

        internal INamedTypeSymbol TypeSymbol { get; }

        ITypeSymbol? ISdkType.TypeSymbol => this.TypeSymbol;

        public override ISymbol Symbol => this.TypeSymbol;

        public override bool CanBeInherited => this.IsReferenceType.GetValueOrDefault() && !this.IsSealed;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => this.Compilation.GetDerivedTypes( this, deep );

        internal NamedType( INamedTypeSymbol typeSymbol, CompilationModel compilation ) : base( compilation )
        {
            this.TypeSymbol = typeSymbol;
        }

        TypeKind IType.TypeKind
            => this.TypeSymbol.TypeKind switch
            {
                RoslynTypeKind.Class => TypeKind.Class,
                RoslynTypeKind.Delegate => TypeKind.Delegate,
                RoslynTypeKind.Enum => TypeKind.Enum,
                RoslynTypeKind.Interface => TypeKind.Interface,
                RoslynTypeKind.Struct => TypeKind.Struct,
                _ => throw new InvalidOperationException( $"Unexpected type kind {this.TypeSymbol.TypeKind}." )
            };

        public SpecialType SpecialType => this._specialType ??= this.GetSpecialTypeCore();

        private SpecialType GetSpecialTypeCore()
        {
            var specialType = this.TypeSymbol.SpecialType.ToOurSpecialType();

            if ( specialType != SpecialType.None )
            {
                return specialType;
            }
            else if ( this.IsGeneric )
            {
                if ( this.IsOpenGeneric )
                {
                    return this.TypeSymbol.Name switch
                    {
                        "IAsyncEnumerable" when this.TypeSymbol.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
                            => SpecialType.IAsyncEnumerable_T,
                        "IAsyncEnumerator" when this.TypeSymbol.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
                            => SpecialType.IAsyncEnumerator_T,
                        nameof(ValueTask) when this.TypeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks"
                            => SpecialType.ValueTask_T,
                        nameof(Task) when this.TypeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks"
                            => SpecialType.Task_T,
                        _ => SpecialType.None
                    };
                }
                else
                {
                    return SpecialType.None;
                }
            }
            else
            {
                return this.TypeSymbol.Name switch
                {
                    nameof(ValueTask) when this.TypeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks"
                        => SpecialType.ValueTask,
                    nameof(Task) when this.TypeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks"
                        => SpecialType.Task,
                    _ => SpecialType.None
                };
            }
        }

        public Type ToType() => this.GetCompilationModel().Factory.GetReflectionType( this.TypeSymbol );

        public bool? IsReferenceType => this.TypeSymbol.IsReferenceType;

        public bool? IsNullable
        {
            get
            {
                if ( this.TypeSymbol.IsReferenceType )
                {
                    return this.TypeSymbol.NullableAnnotation switch
                    {
                        NullableAnnotation.Annotated => true,
                        NullableAnnotation.NotAnnotated => false,
                        _ => null
                    };
                }
                else
                {
                    return false;
                }
            }
        }

        public override MemberInfo ToMemberInfo() => this.ToType();

        public bool IsReadOnly => this.TypeSymbol.IsReadOnly;

        public bool IsExternal => !SymbolEqualityComparer.Default.Equals( this.TypeSymbol.ContainingAssembly, this.Compilation.RoslynCompilation.Assembly );

        public bool HasDefaultConstructor
            => this.TypeSymbol.TypeKind == RoslynTypeKind.Struct ||
               (this.TypeSymbol.TypeKind == RoslynTypeKind.Class && !this.TypeSymbol.IsAbstract &&
                this.TypeSymbol.InstanceConstructors.Any( ctor => ctor.Parameters.Length == 0 ));

        public bool IsOpenGeneric => this.TypeSymbol.TypeArguments.Any( ga => ga is ITypeParameterSymbol ) || this.DeclaringType is { IsOpenGeneric: true };

        public bool IsGeneric => this.TypeSymbol.IsGenericType;

        [Memo]
        public INamedTypeList NestedTypes
            => new NamedTypeList(
                this,
                this.TypeSymbol.GetTypeMembers()
                    .Where( t => this.Compilation.SymbolClassifier.GetTemplatingScope( t ) != TemplatingScope.CompileTimeOnly )
                    .Select( t => new MemberRef<INamedType>( t, this.Compilation.RoslynCompilation ) ) );

        [Memo]
        public IPropertyList Properties
            => new PropertyList(
                this,
                this.TransformMembers<IProperty, IPropertyBuilder, IPropertySymbol>(
                    this.TypeSymbol
                        .GetMembers()
                        .OfType<IPropertySymbol>()
                        .ToReadOnlyList() ) );

        [Memo]
        public IFieldList Fields
            => new FieldList(
                this,
                this.TransformMembers<IField, IFieldBuilder, IFieldSymbol>(
                    this.TypeSymbol
                        .GetMembers()
                        .OfType<IFieldSymbol>()
                        .Where( s => s is { CanBeReferencedByName: true } )
                        .ToReadOnlyList() ) );

        [Memo]
        public IFieldOrPropertyList FieldsAndProperties => new FieldAndPropertiesList( this.Fields, this.Properties );

        [Memo]
        public IEventList Events
            => new EventList(
                this,
                this.TransformMembers<IEvent, IEventBuilder, IEventSymbol>(
                    this.TypeSymbol
                        .GetMembers()
                        .OfType<IEventSymbol>()
                        .ToReadOnlyList() ) );

        [Memo]
        public IMethodList Methods
            => new MethodList(
                this,
                this.TransformMembers<IMethod, IMethodBuilder, IMethodSymbol>(
                    this.TypeSymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(
                            m =>
                                m.MethodKind != MethodKind.Constructor
                                && m.MethodKind != MethodKind.StaticConstructor
                                && m.MethodKind != MethodKind.PropertyGet
                                && m.MethodKind != MethodKind.PropertySet
                                && m.MethodKind != MethodKind.EventAdd
                                && m.MethodKind != MethodKind.EventRemove
                                && m.MethodKind != MethodKind.EventRaise )
                        .ToReadOnlyList() ) );

        [Memo]
        public IConstructorList Constructors
            => new ConstructorList(
                this,
                this.TypeSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where( m => m.MethodKind == MethodKind.Constructor )
                    .Select( m => new MemberRef<IConstructor>( m, this.Compilation.RoslynCompilation ) ) );

        [Memo]
        public IConstructor? StaticConstructor
            => this.TypeSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where( m => m.MethodKind == MethodKind.StaticConstructor )
                .Select( m => this.Compilation.Factory.GetConstructor( m ) )
                .SingleOrDefault();

        public bool IsPartial
        {
            get
            {
                var syntaxReference = this.TypeSymbol.GetPrimarySyntaxReference();

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

                return modifiers.Any( m => m.Kind() == SyntaxKind.PartialKeyword );
            }
        }

        [Memo]
        public IGenericParameterList TypeParameters
            => new GenericParameterList(
                this,
                this.TypeSymbol.TypeParameters
                    .Select( x => Ref.FromSymbol<ITypeParameter>( x, this.Compilation.RoslynCompilation ) ) );

        [Memo]
        public INamespace Namespace => this.Compilation.Factory.GetNamespace( this.TypeSymbol.ContainingNamespace );

        [Memo]
        public string FullName => this.TypeSymbol.ToDisplayString();

        [Memo]
        public IReadOnlyList<IType> TypeArguments => this.TypeSymbol.TypeArguments.Select( a => this.Compilation.Factory.GetIType( a ) ).ToImmutableList();

        [Memo]
        public override IDeclaration? ContainingDeclaration
            => this.TypeSymbol.ContainingSymbol switch
            {
                INamespaceSymbol => this.Compilation.Factory.GetAssembly( this.TypeSymbol.ContainingAssembly ),
                INamedTypeSymbol containingType => this.Compilation.Factory.GetNamedType( containingType ),
                _ => throw new AssertionFailedException()
            };

        public override DeclarationKind DeclarationKind => DeclarationKind.NamedType;

        [Memo]
        public INamedType? BaseType => this.TypeSymbol.BaseType == null ? null : this.Compilation.Factory.GetNamedType( this.TypeSymbol.BaseType );

        [Memo]
        public IImplementedInterfaceList AllImplementedInterfaces
            => // TODO: Correct order after concat and distinct?            
                (this.BaseType?.AllImplementedInterfaces ?? Enumerable.Empty<INamedType>())
                .Concat(
                    this.TypeSymbol.Interfaces.Select( this.Compilation.Factory.GetNamedType )
                        .Concat(
                            this.Compilation.GetObservableTransformationsOnElement( this )
                                .OfType<IntroducedInterface>()
                                .Select( i => i.InterfaceType ) ) )
                .Distinct() // Remove duplicates (re-implementations of earlier interface by aspect).
                .ToImmutableArray()
                .ToImplementedInterfaceList();

        [Memo]
        public IImplementedInterfaceList ImplementedInterfaces
            => // TODO: Correct order after concat and distinct?            
                this.TypeSymbol.Interfaces.Select( this.Compilation.Factory.GetNamedType )
                    .Concat(
                        this.Compilation.GetObservableTransformationsOnElement( this )
                            .OfType<IntroducedInterface>()
                            .Select( i => i.InterfaceType ) )
                    .Distinct() // Remove duplicates (re-implementations of earlier interface by aspect).
                    .ToImmutableArray()
                    .ToImplementedInterfaceList();

        ICompilation ICompilationElement.Compilation => this.Compilation;

        IGeneric IGenericInternal.ConstructGenericInstance( params IType[] typeArguments )
        {
            if ( this.DeclaringType is { IsOpenGeneric: true } )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format(
                        $"Cannot construct a generic instance of this nested type because the declaring type '{this.DeclaringType}' has unbound type parameters." ) );
            }

            return this.Compilation.Factory.GetNamedType( this.TypeSymbol.Construct( typeArguments.Select( a => a.GetSymbol() ).ToArray() ) );
        }

        public IEnumerable<IMember> GetOverridingMembers( IMember member )
        {
            var isInterfaceMember = member.DeclaringType.TypeKind == TypeKind.Interface;

            if ( member.IsStatic || (!isInterfaceMember && (!member.IsVirtual || member.IsSealed)) )
            {
                return Enumerable.Empty<IMember>();
            }

            IMemberList<IMember> members;

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

                default:
                    return Enumerable.Empty<IMember>();
            }

            var candidates = members.OfName( member.Name );

            var overridingMembers = new List<IMember>();

            foreach ( var candidate in candidates )
            {
                if ( isInterfaceMember )
                {
                    if ( ((INamedTypeInternal) candidate.DeclaringType).IsImplementationOfInterfaceMember( candidate, member ) )
                    {
                        overridingMembers.Add( candidate );
                    }
                }
                else
                {
                    // Override. Look for overrides.
                    for ( var c = (IMemberImpl) candidate; c != null; c = (IMemberImpl?) c.OverriddenMember )
                    {
                        if ( c.OverriddenMember?.GetOriginalDefinition() == member )
                        {
                            overridingMembers.Add( candidate );
                        }
                    }
                }
            }

            return overridingMembers.ToList();
        }

        public bool IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember )
        {
            // Some trivial checks first.
            if ( typeMember.Name != interfaceMember.Name
                 || typeMember.DeclarationKind != interfaceMember.DeclarationKind
                 || !(typeMember.Accessibility == Accessibility.Public || typeMember.IsExplicitInterfaceImplementation) )
            {
                return false;
            }

            var interfaceType = interfaceMember.DeclaringType.GetSymbol();
            var relevantInterfaces = this.GetAllInterfaces().Where( t => t.ConstructedFrom.Equals( interfaceType ) );

            foreach ( var implementedInterface in relevantInterfaces )
            {
                foreach ( var candidateSymbol in implementedInterface.GetMembers( typeMember.Name ) )
                {
                    var candidateMember = (IMember) this.Compilation.Factory.GetDeclaration( candidateSymbol );

                    if ( SignatureComparer.Instance.Equals( candidateMember, typeMember ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Equals( IType other ) => this.Compilation.InvariantComparer.Equals( this, other );

        public bool IsSubclassOf( INamedType type )
        {
            // TODO: enum.IsSubclassOf(int) == true etc.
            if ( type.TypeKind == TypeKind.Class )
            {
                INamedType? currentType = this;

                while ( currentType != null )
                {
                    if ( this.Compilation.InvariantComparer.Equals( currentType, type ) )
                    {
                        return true;
                    }

                    currentType = currentType.BaseType;
                }

                return false;
            }
            else if ( type.TypeKind == TypeKind.Interface )
            {
                return this.ImplementedInterfaces.SingleOrDefault( i => this.Compilation.InvariantComparer.Equals( i, type ) ) != null;
            }
            else
            {
                return this.Compilation.InvariantComparer.Equals( this, type );
            }
        }

        public bool TryFindImplementationForInterfaceMember( IMember interfaceMember, [NotNullWhen( true )] out IMember? implementationMember )
        {
            // TODO: Type introductions.
            var symbolInterfaceMemberImplementationSymbol = this.TypeSymbol.FindImplementationForInterfaceMember( interfaceMember.GetSymbol().AssertNotNull() );

            var symbolInterfaceMemberImplementation =
                symbolInterfaceMemberImplementationSymbol != null
                    ? (IMember) this.Compilation.Factory.GetDeclaration( symbolInterfaceMemberImplementationSymbol )
                    : null;

            // Introduced implementation can be implementing the interface member in a subtype.
            INamedType? currentType = this;

            while ( currentType != null )
            {
                var introducedInterface =
                    this.Compilation
                        .GetObservableTransformationsOnElement( currentType )
                        .OfType<IntroducedInterface>()
                        .SingleOrDefault( i => this.Compilation.InvariantComparer.Equals( i.InterfaceType, interfaceMember.DeclaringType ) );

                if ( introducedInterface != null )
                {
                    // TODO: Generics.
                    if ( !introducedInterface.MemberMap.TryGetValue( interfaceMember, out var interfaceMemberImplementation ) )
                    {
                        throw new AssertionFailedException();
                    }

                    // Which is later in inheritance?
                    if ( symbolInterfaceMemberImplementation == null || currentType.IsSubclassOf( symbolInterfaceMemberImplementation.DeclaringType ) )
                    {
                        implementationMember = interfaceMemberImplementation;

                        return true;
                    }
                    else
                    {
                        implementationMember = symbolInterfaceMemberImplementation;

                        return true;
                    }
                }

                currentType = currentType.BaseType?.GetOriginalDefinition();
            }

            if ( symbolInterfaceMemberImplementation != null )
            {
                implementationMember = symbolInterfaceMemberImplementation;

                return true;
            }
            else
            {
                implementationMember = null;

                return false;
            }
        }

        private IEnumerable<MemberRef<TMember>> TransformMembers<TMember, TBuilder, TSymbol>( IReadOnlyList<TSymbol> symbolMembers )
            where TMember : class, IMember
            where TBuilder : IMemberBuilder, TMember
            where TSymbol : class, ISymbol
        {
            var transformations = this.Compilation.GetObservableTransformationsOnElement( this );

            if ( transformations.Length == 0 )
            {
                // No transformations.
                return symbolMembers.Select( x => new MemberRef<TMember>( x, this.Compilation.RoslynCompilation ) );
            }

            if ( !transformations.OfType<TBuilder>().Any( t => t is IReplaceMember ) )
            {
                // No replaced members.
                return
                    symbolMembers
                        .Select( x => new MemberRef<TMember>( x, this.Compilation.RoslynCompilation ) )
                        .Concat( transformations.OfType<TBuilder>().Select( x => x.ToMemberRef<TMember>() ) );
            }

            var allSymbols = new HashSet<TSymbol>( symbolMembers, SymbolEqualityComparer.Default );
            var replacedSymbols = new HashSet<TSymbol>( SymbolEqualityComparer.Default );
            var replacedBuilders = new HashSet<TBuilder>();
            var builders = new List<TBuilder>();

            // Go through transformations, noting replaced symbols and builders.
            foreach ( var builder in transformations )
            {
                if ( builder is IReplaceMember replace )
                {
                    if ( replace.ReplacedMember.Target is TSymbol symbol && allSymbols.Contains( replace.ReplacedMember.Target ) )
                    {
                        // If the MemberRef points to a symbol just remove from symbol list.
                        // This prevents needless allocation.
                        replacedSymbols.Add( symbol );
                    }
                    else
                    {
                        // Otherwise resolve the MemberRef.
                        var resolved = replace.ReplacedMember.GetTarget( this.Compilation );

                        if ( resolved is TMember )
                        {
                            var resolvedSymbol = (TSymbol?) resolved.GetSymbol();

                            if ( resolvedSymbol != null )
                            {
                                replacedSymbols.Add( resolvedSymbol );
                            }
                            else if ( resolved is TBuilder replacedBuilder )
                            {
                                replacedBuilders.Add( replacedBuilder );
                            }
                            else
                            {
                                throw new AssertionFailedException();
                            }
                        }
                    }
                }

                if ( builder is TBuilder typedBuilder )
                {
                    builders.Add( typedBuilder );
                }
            }

            var members = new List<MemberRef<TMember>>();

            foreach ( var symbol in symbolMembers )
            {
                if ( !replacedSymbols.Contains( symbol ) )
                {
                    members.Add( new MemberRef<TMember>( symbol, this.Compilation.RoslynCompilation ) );
                }
            }

            foreach ( var builder in builders )
            {
                if ( !replacedBuilders.Contains( builder ) )
                {
                    members.Add( builder.ToMemberRef<TMember>() );
                }
            }

            return members;
        }

        private void PopulateAllInterfaces( ImmutableHashSet<INamedTypeSymbol>.Builder builder, GenericMap genericMap )
        {
            // Process the Roslyn type system.
            foreach ( var type in this.TypeSymbol.Interfaces )
            {
                builder.Add( (INamedTypeSymbol) genericMap.Map( type ) );
            }

            if ( this.TypeSymbol.BaseType != null )
            {
                var newGenericMap = genericMap.CreateBaseMap( this.TypeSymbol.BaseType.TypeArguments );
                ((NamedType) this.BaseType!).PopulateAllInterfaces( builder, newGenericMap );
            }

            // TODO: process introductions.
        }

        [Memo]
        public ImmutableHashSet<INamedTypeSymbol> AllInterfaces => this.GetAllInterfaces();

        private ImmutableHashSet<INamedTypeSymbol> GetAllInterfaces()
        {
            var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>( SymbolEqualityComparer.Default );
            this.PopulateAllInterfaces( builder, this.Compilation.EmptyGenericMap );

            return builder.ToImmutable();
        }
    }
}