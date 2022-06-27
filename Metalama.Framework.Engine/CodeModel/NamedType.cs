﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
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
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed partial class NamedType : MemberOrNamedType, INamedTypeInternal
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
                RoslynTypeKind.Class when !this.TypeSymbol.IsRecord => TypeKind.Class,
                RoslynTypeKind.Class when this.TypeSymbol.IsRecord => TypeKind.RecordClass,
                RoslynTypeKind.Delegate => TypeKind.Delegate,
                RoslynTypeKind.Enum => TypeKind.Enum,
                RoslynTypeKind.Interface => TypeKind.Interface,
                RoslynTypeKind.Struct when !this.TypeSymbol.IsRecord => TypeKind.Struct,
                RoslynTypeKind.Struct when this.TypeSymbol.IsRecord => TypeKind.RecordStruct,
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
        public INamedTypeCollection NestedTypes
            => new NamedTypeCollection(
                this,
                new TypeUpdatableCollection( this.Compilation, this.TypeSymbol ) );

        [Memo]
        public IPropertyCollection Properties
            => new PropertyCollection(
                this,
                this.Compilation.GetPropertyCollection( this.TypeSymbol ) );

        [Memo]
        public IPropertyCollection AllProperties => new AllPropertiesCollection( this );

        [Memo]
        public IIndexerCollection Indexers
            => new IndexerCollection(
                this,
                this.Compilation.GetIndexerCollection( this.TypeSymbol ) );

        [Memo]
        public IIndexerCollection AllIndexers => new AllIndexersCollection( this );

        [Memo]
        public IFieldCollection Fields
            => new FieldCollection(
                this,
                this.Compilation.GetFieldCollection( this.TypeSymbol ) );

        [Memo]
        public IFieldCollection AllFields => new AllFieldsCollection( this );

        [Memo]
        public IFieldOrPropertyCollection FieldsAndProperties => new FieldAndPropertiesCollection( this.Fields, this.Properties );

        public IFieldOrPropertyCollection AllFieldsAndProperties => throw new NotImplementedException();

        [Memo]
        public IEventCollection Events
            => new EventCollection(
                this,
                this.Compilation.GetEventCollection( this.TypeSymbol ) );

        [Memo]
        public IEventCollection AllEvents => new AllEventsCollection( this );

        [Memo]
        public IMethodCollection Methods
            => new MethodCollection(
                this,
                this.Compilation.GetMethodCollection( this.TypeSymbol ) );

        [Memo]
        public IMethodCollection AllMethods => new AllMethodsCollection( this );

        [Memo]
        public IMethodCollection Operators
            => new OperatorCollection(
                this,
                this.Compilation.GetOperatorCollection( this.TypeSymbol ) );

        [Memo]
        public IMethodCollection AllOperators => new AllOperatorsCollection( this );

        [Memo]
        public IConstructorCollection Constructors
            => new ConstructorCollection(
                this,
                this.Compilation.GetConstructorCollection( this.TypeSymbol ) );

        public IConstructor? StaticConstructor => this.GetStaticConstructorImpl();

        public IMethod? Finalizer => this.GetFinalizerImpl();

        private IConstructor? GetStaticConstructorImpl()
        {
            var builder = this.Compilation.GetStaticConstructor( this.TypeSymbol );

            if ( builder != null )
            {
                return this.Compilation.Factory.GetConstructor( builder );
            }

            var symbol = this.TypeSymbol.StaticConstructors.SingleOrDefault();

            if ( symbol != null )
            {
                return this.Compilation.Factory.GetConstructor( symbol );
            }

            return null;
        }

        private IMethod? GetFinalizerImpl()
        {
            var builder = this.Compilation.GetFinalizer( this.TypeSymbol );

            if ( builder != null )
            {
                return this.Compilation.Factory.GetDeclaration<IMethod>( builder );
            }

            var symbol = this.TypeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault( m => m.Name == "Finalize" && m.TypeParameters.Length == 0 && m.Parameters.Length == 0 );

            if ( symbol != null )
            {
                return this.Compilation.Factory.GetFinalizer( symbol );
            }

            return null;
        }

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

                return modifiers.Any( m => m.IsKind( SyntaxKind.PartialKeyword ) );
            }
        }

        [Memo]
        public IGenericParameterList TypeParameters
            => new TypeParameterList(
                this,
                this.TypeSymbol.TypeParameters
                    .Select( x => Ref.FromSymbol<ITypeParameter>( x, this.Compilation.RoslynCompilation ) )
                    .ToList() );

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
        public IImplementedInterfaceCollection AllImplementedInterfaces
            => new AllImplementedInterfacesCollection( this, this.Compilation.GetAllInterfaceImplementationCollection( this.TypeSymbol, false ) );

        [Memo]
        public IImplementedInterfaceCollection ImplementedInterfaces
            => new ImplementedInterfaceCollection( this, this.Compilation.GetInterfaceImplementationCollection( this.TypeSymbol, false ) );

        ICompilation ICompilationElement.Compilation => this.Compilation;

        IGeneric IGenericInternal.ConstructGenericInstance( params IType[] typeArguments )
        {
            if ( this.DeclaringType is { IsOpenGeneric: true } )
            {
                throw new InvalidOperationException(
                    UserMessageFormatter.Format(
                        $"Cannot construct a generic instance of this nested type because the declaring type '{this.DeclaringType}' has unbound type parameters." ) );
            }

            var typeArgumentSymbols = typeArguments.Select( a => a.GetSymbol() ).ToArray();

            var typeSymbol = this.TypeSymbol;
            var constructedTypeSymbol = typeSymbol.Construct( typeArgumentSymbols );

            return this.Compilation.Factory.GetNamedType( constructedTypeSymbol );
        }

        public IEnumerable<IMember> GetOverridingMembers( IMember member )
        {
            var isInterfaceMember = member.DeclaringType.TypeKind == TypeKind.Interface;

            if ( member.IsStatic || (!isInterfaceMember && (!member.IsVirtual || member.IsSealed)) )
            {
                return Enumerable.Empty<IMember>();
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

                    if ( MemberComparer<IMember>.Instance.Equals( candidateMember, typeMember ) )
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
            if ( type.TypeKind is TypeKind.Class or TypeKind.RecordClass )
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
                        .GetInterfaceImplementationCollection( this.TypeSymbol, false )
                        .Introductions
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

        public ITypeInternal Accept( TypeRewriter visitor ) => visitor.Visit( this );

        internal ITypeInternal WithTypeArguments( ImmutableArray<IType> types )
        {
            var hasDifference = false;

            for ( var i = 0; i < types.Length; i++ )
            {
                if ( types[i] != this.TypeArguments[i] )
                {
                    hasDifference = true;

                    break;
                }
            }

            if ( !hasDifference )
            {
                return this;
            }

            var typeArgumentSymbols = new ITypeSymbol[types.Length];

            for ( var i = 0; i < types.Length; i++ )
            {
                typeArgumentSymbols[i] = types[i].GetSymbol();
            }

            var symbol = this.TypeSymbol.OriginalDefinition.Construct( typeArgumentSymbols );

            return (ITypeInternal) this.GetCompilationModel().Factory.GetIType( symbol );
        }
    }
}