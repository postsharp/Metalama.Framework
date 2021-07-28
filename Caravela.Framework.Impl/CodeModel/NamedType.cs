// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class NamedType : MemberOrNamedType, ITypeInternal, ISdkNamedType
    {
        internal INamedTypeSymbol TypeSymbol { get; }

        ITypeSymbol? ISdkType.TypeSymbol => this.TypeSymbol;

        public override ISymbol Symbol => this.TypeSymbol;

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

        public Type ToType() => CompileTimeType.Create( this );

        public override MemberInfo ToMemberInfo() => this.ToType();

        public bool IsReadOnly => this.TypeSymbol.IsReadOnly;

        public bool HasDefaultConstructor
            => this.TypeSymbol.TypeKind == RoslynTypeKind.Struct ||
               (this.TypeSymbol.TypeKind == RoslynTypeKind.Class && !this.TypeSymbol.IsAbstract &&
                this.TypeSymbol.InstanceConstructors.Any( ctor => ctor.Parameters.Length == 0 ));

        public bool IsOpenGeneric
            => this.GenericArguments.Any( ga => ga is IGenericParameter ) || (this.ContainingDeclaration as INamedType)?.IsOpenGeneric == true;

        [Memo]
        public INamedTypeList NestedTypes => new NamedTypeList( this, this.TypeSymbol.GetTypeMembers().Select( t => new MemberRef<INamedType>( t ) ) );

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
                    .Select( m => new MemberRef<IConstructor>( m ) ) );

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

                return ((TypeDeclarationSyntax) syntaxReference.GetSyntax()).Modifiers.Any( m => m.Kind() == SyntaxKind.PartialKeyword );
            }
        }

        [Memo]
        public IGenericParameterList GenericParameters
            => new GenericParameterList(
                this,
                this.TypeSymbol.TypeParameters
                    .Select( DeclarationRef.FromSymbol<IGenericParameter> ) );

        [Memo]
        public INamespace Namespace => this.Compilation.Factory.GetNamespace( this.TypeSymbol.ContainingNamespace );

        [Memo]
        public string FullName => this.TypeSymbol.ToDisplayString();

        [Memo]
        public IReadOnlyList<IType> GenericArguments => this.TypeSymbol.TypeArguments.Select( a => this.Compilation.Factory.GetIType( a ) ).ToImmutableList();

        [Memo]
        public IAssembly DeclaringAssembly => this.Compilation.Factory.GetAssembly( this.TypeSymbol.ContainingAssembly );

        [Memo]
        public override IDeclaration? ContainingDeclaration
            => this.TypeSymbol.ContainingSymbol switch
            {
                INamespaceSymbol => this.Compilation.Factory.GetAssembly( this.TypeSymbol.ContainingAssembly ),
                INamedTypeSymbol containingType => this.Compilation.Factory.GetNamedType( containingType ),
                _ => throw new NotImplementedException()
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

        public INamedType WithGenericArguments( params IType[] genericArguments )
            => this.Compilation.Factory.GetNamedType( this.TypeSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() ) );

        public bool Equals( IType other ) => this.Compilation.InvariantComparer.Equals( this, other );

        public override string ToString() => this.TypeSymbol.ToString();

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
            INamedType currentType = this;

            while ( currentType != null )
            {
                var introducedInterface =
                    this.Compilation.GetObservableTransformationsOnElement( currentType )
                        .OfType<IntroducedInterface>()
                        .Where( i => this.Compilation.InvariantComparer.Equals( i.InterfaceType, interfaceMember.DeclaringType ) )
                        .SingleOrDefault();

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
                return symbolMembers.Select( x => new MemberRef<TMember>( x ) );
            }

            if ( !transformations.OfType<TBuilder>().Any( t => t is IReplaceMember ) )
            {
                // No replaced members.
                return
                    symbolMembers
                        .Select( x => new MemberRef<TMember>( x ) )
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
                        var resolved = replace.ReplacedMember.Resolve( this.Compilation );

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
                    members.Add( new MemberRef<TMember>( symbol ) );
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
    }
}