﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
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
    internal sealed class NamedType : MemberOrNamedType, INamedTypeImpl
    {
        public NamedTypeImpl Implementation { get; }

        internal NamedType( INamedTypeSymbol typeSymbol, CompilationModel compilation ) : base( compilation )
        {
            this.Implementation = new NamedTypeImpl( this, typeSymbol, compilation );
        }

        protected override void OnUsingDeclaration() => UserCodeExecutionContext.CurrentOrNull?.AddDependencyFrom( this );

        public override bool CanBeInherited
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.CanBeInherited;
            }
        }

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
        {
            this.OnUsingDeclaration();

            return this.Implementation.GetDerivedDeclarations( options );
        }

        public override DeclarationKind DeclarationKind
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.DeclarationKind;
            }
        }

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

        public bool Equals( IType? otherType, TypeComparison typeComparison ) => this.Implementation.Equals( otherType, typeComparison );

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

        public bool IsGeneric
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsGeneric;
            }
        }

        public bool IsCanonicalGenericInstance
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsCanonicalGenericInstance;
            }
        }

        public IGeneric ConstructGenericInstance( IReadOnlyList<IType> typeArguments )
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

        INamespace INamedType.Namespace => this.ContainingNamespace;

        public INamespace ContainingNamespace
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.ContainingNamespace;
            }
        }

        [Memo]
        private IRef<INamedType> Ref => this.RefFactory.FromSymbolBasedDeclaration<INamedType>( this );

        IRef<INamedType> INamedType.ToRef() => this.Ref;

        IRef<IType> IType.ToRef() => this.Ref;

        IRef<INamespaceOrNamedType> INamespaceOrNamedType.ToRef() => this.Ref;

        INamedTypeCollection INamedType.NestedTypes => this.Types;

        public string FullName
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.FullName;
            }
        }

        public INamedTypeCollection Types
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.Types;
            }
        }

        public INamedTypeCollection AllTypes
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.AllTypes;
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

        public IConstructor? PrimaryConstructor
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.PrimaryConstructor;
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

        public bool IsRef
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.IsRef;
            }
        }

        ICompilation ICompilationElement.Compilation => this.Compilation;

        private protected override IRef<IDeclaration> ToDeclarationRef() => ((IDeclaration) this.Implementation).ToRef();

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

        [Memo]
        public INamedType Definition
            => this.TypeSymbol.Equals( this.TypeSymbol.OriginalDefinition )
                ? this
                : this.Compilation.Factory.GetNamedType( ((INamedTypeSymbol) this.TypeSymbol).OriginalDefinition );

        protected override IRef<IMemberOrNamedType> ToMemberOrNamedTypeRef() => this.UnderlyingType.ToRef();

        protected override IMemberOrNamedType GetDefinition() => this.Definition;

        INamedType INamedType.TypeDefinition => this.Definition;

        public INamedType UnderlyingType => this.Implementation.UnderlyingType;

        public IType Accept( TypeRewriter visitor ) => visitor.Visit( this );

        public IReadOnlyList<IMember> GetOverridingMembers( IMember member )
        {
            this.OnUsingDeclaration();

            return this.Implementation.GetOverridingMembers( member );
        }

        public bool IsImplementationOfInterfaceMember( IMember typeMember, IMember interfaceMember )
        {
            this.OnUsingDeclaration();

            return this.Implementation.IsImplementationOfInterfaceMember( typeMember, interfaceMember );
        }

        internal ITypeImpl WithTypeArguments( IReadOnlyList<IType> types )
        {
            var hasDifference = false;

            for ( var i = 0; i < types.Count; i++ )
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

            var typeArgumentSymbols = new ITypeSymbol[types.Count];

            for ( var index = 0; index < types.Count; index++ )
            {
                var t = types[index];
                typeArgumentSymbols[index] = t.GetSymbol().AssertSymbolNotNull();
            }

            var symbol = ((INamedTypeSymbol) this.TypeSymbol).ConstructedFrom.Construct( typeArgumentSymbols );

            return (ITypeImpl) this.Compilation.Factory.GetIType( symbol );
        }

        public override IDeclaration ContainingDeclaration => this.Implementation.ContainingDeclaration;

        public ITypeSymbol TypeSymbol
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Implementation.NamedTypeSymbol;
            }
        }

        public bool Equals( IType? other )
        {
            this.OnUsingDeclaration();

            return this.Implementation.Equals( other );
        }

        public bool Equals( INamedType? other )
        {
            this.OnUsingDeclaration();

            return this.Implementation.Equals( other );
        }

        public override int GetHashCode()
        {
            this.OnUsingDeclaration();

            return this.Implementation.GetHashCode();
        }
    }
}