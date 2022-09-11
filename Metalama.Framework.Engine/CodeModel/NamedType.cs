// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// The public object that represents an <see cref="INamedType"/>. The implementation is in <see cref="NamedTypeImpl"/>.
    /// This class exists because it needs to add a dependency context check before each member access, which makes
    /// it hard to use [Memo].
    /// </summary>
    internal sealed class NamedType : MemberOrNamedType, INamedTypeInternal
    {
        public NamedTypeImpl Implementation { get; }

        internal NamedType( INamedTypeSymbol typeSymbol, CompilationModel compilation ) : base( compilation, typeSymbol )
        {
            this.Implementation = new NamedTypeImpl( this, typeSymbol, compilation );
        }

        protected override void OnUsingDeclaration()
        {
            UserCodeExecutionContext.CurrentInternal?.AddDependency( this );
        }

        public override bool CanBeInherited
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.CanBeInherited;
            }
        }

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true )
        {
            this.OnUsingDeclaration();

            return this.Implementation.GetDerivedDeclarations( deep );
        }

        public override DeclarationKind DeclarationKind
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.DeclarationKind;
            }
        }

        // Calling OnUsingDeclaration creates an infinite recursion.
        public override ISymbol Symbol => this.Implementation.Symbol;

        public override MemberInfo ToMemberInfo()
        {
            this.OnUsingDeclaration();

            return this.Implementation.ToMemberInfo();
        }

        public TypeKind TypeKind
        {
            get
            {
                this.OnUsingDeclaration();

                return ((IType) this.Implementation).TypeKind;
            }
        }

        public SpecialType SpecialType
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.SpecialType;
            }
        }

        public Type ToType()
        {
            this.OnUsingDeclaration();

            return this.Implementation.ToType();
        }

        public bool? IsReferenceType
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsReferenceType;
            }
        }

        public bool? IsNullable
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsNullable;
            }
        }

        public bool Equals( SpecialType specialType )
        {
            this.OnUsingDeclaration();

            return this.Implementation.Equals( specialType );
        }

        public IGenericParameterList TypeParameters
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.TypeParameters;
            }
        }

        public IReadOnlyList<IType> TypeArguments
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.TypeArguments;
            }
        }

        public bool IsOpenGeneric
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsOpenGeneric;
            }
        }

        public bool IsGeneric
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsGeneric;
            }
        }

        public IGeneric ConstructGenericInstance( params IType[] typeArguments )
        {
            this.OnUsingDeclaration();

            return ((IGenericInternal) this.Implementation).ConstructGenericInstance( typeArguments );
        }

        public bool IsPartial
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsPartial;
            }
        }

        public bool IsExternal
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsExternal;
            }
        }

        public bool HasDefaultConstructor
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.HasDefaultConstructor;
            }
        }

        public INamedType? BaseType
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.BaseType;
            }
        }

        public IImplementedInterfaceCollection AllImplementedInterfaces
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.AllImplementedInterfaces;
            }
        }

        public IImplementedInterfaceCollection ImplementedInterfaces
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.ImplementedInterfaces;
            }
        }

        public INamespace Namespace
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.Namespace;
            }
        }

        public string FullName
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.FullName;
            }
        }

        public INamedTypeCollection NestedTypes
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.NestedTypes;
            }
        }

        public IPropertyCollection Properties
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.Properties;
            }
        }

        public IPropertyCollection AllProperties
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.AllProperties;
            }
        }

        public IIndexerCollection Indexers
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.Indexers;
            }
        }

        public IIndexerCollection AllIndexers
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.AllIndexers;
            }
        }

        public IFieldCollection Fields
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.Fields;
            }
        }

        public IFieldCollection AllFields
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.AllFields;
            }
        }

        public IFieldOrPropertyCollection FieldsAndProperties
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.FieldsAndProperties;
            }
        }

        public IFieldOrPropertyCollection AllFieldsAndProperties
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.AllFieldsAndProperties;
            }
        }

        public IEventCollection Events
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.Events;
            }
        }

        public IEventCollection AllEvents
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.AllEvents;
            }
        }

        public IMethodCollection Methods
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.Methods;
            }
        }

        public IMethodCollection AllMethods
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.AllMethods;
            }
        }

        public IConstructorCollection Constructors
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.Constructors;
            }
        }

        public IConstructor? StaticConstructor
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.StaticConstructor;
            }
        }

        public IMethod? Finalizer
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.Finalizer;
            }
        }
        
        public bool IsReadOnly
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsReadOnly;
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
        public string FullName => this.TypeSymbol.GetFullName().AssertNotNull();

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
            => new ImplementedInterfacesCollection( this, this.Compilation.GetInterfaceImplementationCollection( this.TypeSymbol, false ) );

        ICompilation ICompilationElement.Compilation => this.Compilation;

        IGeneric IGenericInternal.ConstructGenericInstance( params IType[] typeArguments )
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsReadOnly;
            }
        }

        public bool IsSubclassOf( INamedType type )
        {
            this.OnUsingDeclaration();

            return this.Implementation.IsSubclassOf( type );
        }

        public bool TryFindImplementationForInterfaceMember( IMember interfaceMember, [NotNullWhen( true )] out IMember? implementationMember )
        {
            this.OnUsingDeclaration();

            return this.Implementation.TryFindImplementationForInterfaceMember( interfaceMember, out implementationMember );
        }

        public INamedType TypeDefinition
            => this.TypeSymbol == this.TypeSymbol.OriginalDefinition ? this : this.Compilation.Factory.GetNamedType( this.TypeSymbol.OriginalDefinition );

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

        public IEnumerable<IMember> GetOverridingMembers( IMember member )
        {
            this.OnUsingDeclaration();

            return this.Implementation.GetOverridingMembers( member );
        }

        public bool IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember )
        {
            this.OnUsingDeclaration();

            return this.Implementation.IsImplementationOfInterfaceMember( typeMember, interfaceMember );
        }

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

            var symbol = ((INamedTypeSymbol) this.TypeSymbol.OriginalDefinition).Construct( typeArgumentSymbols );

            return (ITypeInternal) this.GetCompilationModel().Factory.GetIType( symbol );
        }

        public override IDeclaration? ContainingDeclaration => this.Implementation.ContainingDeclaration;
    }
}