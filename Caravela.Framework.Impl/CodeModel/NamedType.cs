// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class NamedType : Member, ITypeInternal, ISdkNamedType
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

        public override bool IsReadOnly => this.TypeSymbol.IsReadOnly;

        public override bool IsAsync => false;

        public bool HasDefaultConstructor
            => this.TypeSymbol.TypeKind == RoslynTypeKind.Struct ||
               (this.TypeSymbol.TypeKind == RoslynTypeKind.Class && !this.TypeSymbol.IsAbstract &&
                this.TypeSymbol.InstanceConstructors.Any( ctor => ctor.Parameters.Length == 0 ));

        public bool IsOpenGeneric
            => this.GenericArguments.Any( ga => ga is IGenericParameter ) || (this.ContainingElement as INamedType)?.IsOpenGeneric == true;

        [Memo]
        public INamedTypeList NestedTypes => new NamedTypeList( this, this.TypeSymbol.GetTypeMembers().Select( t => new MemberLink<INamedType>( t ) ) );

        [Memo]
        public IPropertyList Properties
            => new PropertyList(
                this,
                this.TypeSymbol.GetMembers()
                    .Select(
                        m => m switch
                        {
                            IPropertySymbol p => new MemberLink<IProperty>( p ),
                            _ => default
                        } ) );

        public IFieldList Fields
            => new FieldList(
                this,
                this.TypeSymbol.GetMembers()
                    .Select(
                        m => m switch
                        {
                            IFieldSymbol p => new MemberLink<IField>( p ),
                            _ => default
                        } ) );

        [Memo]
        public IFieldOrPropertyList FieldsAndProperties => new FieldAndPropertiesList( this.Fields, this.Properties );

        [Memo]
        public IEventList Events
            => new EventList(
                this,
                this.TypeSymbol
                    .GetMembers()
                    .OfType<IEventSymbol>()
                    .Select( e => new MemberLink<IEvent>( e ) ) );

        [Memo]
        public IMethodList Methods
            => new MethodList(
                this,
                this.TypeSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where( m => m.MethodKind != MethodKind.Constructor && m.MethodKind != MethodKind.StaticConstructor )
                    .Select( m => new MemberLink<IMethod>( m ) )
                    .Concat(
                        this.Compilation.GetObservableTransformationsOnElement( this )
                            .OfType<MethodBuilder>()
                            .Select( m => new MemberLink<IMethod>( m ) ) ) );

        [Memo]
        public IConstructorList Constructors
            => new ConstructorList(
                this,
                this.TypeSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where( m => m.MethodKind == MethodKind.Constructor )
                    .Select( m => new MemberLink<IConstructor>( m ) ) );

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
                var syntaxReference = this.TypeSymbol.DeclaringSyntaxReferences.FirstOrDefault();

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
                    .Select( tp => CodeElementLink.FromSymbol<IGenericParameter>( tp ) ) );

        [Memo]
        public string? Namespace => this.TypeSymbol.ContainingNamespace?.ToDisplayString();

        [Memo]
        public string FullName => this.TypeSymbol.ToDisplayString();

        [Memo]
        public IReadOnlyList<IType> GenericArguments => this.TypeSymbol.TypeArguments.Select( a => this.Compilation.Factory.GetIType( a ) ).ToImmutableList();

        [Memo]
        public IAssembly DeclaringAssembly => this.Compilation.Factory.GetAssembly( this.TypeSymbol.ContainingAssembly );

        [Memo]
        public override ICodeElement? ContainingElement
            => this.TypeSymbol.ContainingSymbol switch
            {
                INamespaceSymbol => this.Compilation.Factory.GetAssembly( this.TypeSymbol.ContainingAssembly ),
                INamedTypeSymbol containingType => this.Compilation.Factory.GetNamedType( containingType ),
                _ => throw new NotImplementedException()
            };

        public override CodeElementKind ElementKind => CodeElementKind.Type;

        [Memo]
        public INamedType? BaseType => this.TypeSymbol.BaseType == null ? null : this.Compilation.Factory.GetNamedType( this.TypeSymbol.BaseType );

        [Memo]
        public IReadOnlyList<INamedType> ImplementedInterfaces
            => this.TypeSymbol.AllInterfaces.Select( this.Compilation.Factory.GetNamedType ).ToImmutableArray();

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public INamedType WithGenericArguments( params IType[] genericArguments )
            => this.Compilation.Factory.GetNamedType( this.TypeSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() ) );

        public bool Equals( IType other ) => this.Compilation.InvariantComparer.Equals( this, other );

        public override string ToString() => this.TypeSymbol.ToString();
    }
}