﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal sealed class NamedType : Member, INamedType, ITypeInternal
    {
        internal INamedTypeSymbol TypeSymbol { get; }

        ITypeSymbol? ISdkType.TypeSymbol => this.TypeSymbol;

        public override ISymbol Symbol => this.TypeSymbol;

        internal NamedType( INamedTypeSymbol typeSymbol, CompilationModel compilation ) : base( compilation )
        {
            this.TypeSymbol = typeSymbol;
        }

        TypeKind IType.TypeKind => this.TypeSymbol.TypeKind switch
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

        public bool HasDefaultConstructor =>
            this.TypeSymbol.TypeKind == RoslynTypeKind.Struct ||
            (this.TypeSymbol.TypeKind == RoslynTypeKind.Class && !this.TypeSymbol.IsAbstract &&
             this.TypeSymbol.InstanceConstructors.Any( ctor => ctor.Parameters.Length == 0 ));

        public bool IsOpenGeneric => this.GenericArguments.Any( ga => ga is IGenericParameter ) || (this.ContainingElement as INamedType)?.IsOpenGeneric == true;

        [Memo]
        public IReadOnlyList<INamedType> NestedTypes => this.TypeSymbol.GetTypeMembers().Select( this.Compilation.Factory.GetNamedType ).ToImmutableArray();

        [Memo]
        public IReadOnlyList<IProperty> Properties =>
            this.TypeSymbol.GetMembers().Select(
                    m => m switch
                    {
                        IPropertySymbol p => new Property( p, this ),
                        IFieldSymbol { IsImplicitlyDeclared: false } f => new Field( f, this ),
                        _ => (IProperty) null!
                    } )
                .Where( p => p != null )
                .ToImmutableArray();

        [Memo]
        public IReadOnlyList<IEvent> Events
            => this.TypeSymbol
                .GetMembers()
                .OfType<IEventSymbol>()
                .Select( e => new Event( e, this ) )
                .ToImmutableArray();

        [Memo]
        public IReadOnlyList<IMethod> Methods
            => this.TypeSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where( m => m.MethodKind != MethodKind.Constructor && m.MethodKind != MethodKind.StaticConstructor )
                .Select( m => this.Compilation.Factory.GetMethod( m ) )
                .Concat( this.Compilation.ObservableTransformations.GetByKey( this ).OfType<MethodBuilder>() )
                .ToImmutableArray();

        [Memo]
        public IReadOnlyList<IConstructor> Constructors
            => this.TypeSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where( m => m.MethodKind == MethodKind.Constructor )
                .Select( m => this.Compilation.Factory.GetConstructor( m ) )
                .ToImmutableArray();

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
        public IReadOnlyList<IGenericParameter> GenericParameters =>
            this.TypeSymbol.TypeParameters.Select( tp => this.Compilation.Factory.GetGenericParameter( tp ) ).ToImmutableList();

        [Memo]
        public string? Namespace => this.TypeSymbol.ContainingNamespace?.ToDisplayString();

        [Memo]
        public string FullName => this.TypeSymbol.ToDisplayString();

        [Memo]
        public IReadOnlyList<IType> GenericArguments => this.TypeSymbol.TypeArguments.Select( a => this.Compilation.Factory.GetIType( a ) ).ToImmutableList();

        [Memo]
        public override ICodeElement? ContainingElement => this.TypeSymbol.ContainingSymbol switch
        {
            INamespaceSymbol => null,
            INamedTypeSymbol containingType => this.Compilation.Factory.GetNamedType( containingType ),
            _ => throw new NotImplementedException()
        };

        public override CodeElementKind ElementKind => CodeElementKind.Type;

        [Memo]
        public INamedType? BaseType => this.TypeSymbol.BaseType == null ? null : this.Compilation.Factory.GetNamedType( this.TypeSymbol.BaseType );

        [Memo]
        public IReadOnlyList<INamedType> ImplementedInterfaces => this.TypeSymbol.AllInterfaces.Select( this.Compilation.Factory.GetNamedType ).ToImmutableArray();

        ITypeFactory IType.TypeFactory => this.Compilation.Factory;

        public INamedType WithGenericArguments( params IType[] genericArguments ) =>
            this.Compilation.Factory.GetNamedType( this.TypeSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() ) );

        public override string ToString() => this.TypeSymbol.ToString();
    }
}