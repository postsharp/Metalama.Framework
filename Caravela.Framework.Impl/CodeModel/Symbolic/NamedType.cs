using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class NamedType : CodeElement, INamedType, ITypeInternal
    {
        internal INamedTypeSymbol TypeSymbol { get; }

        ITypeSymbol ITypeInternal.TypeSymbol => this.TypeSymbol;

        protected internal override ISymbol Symbol => this.TypeSymbol;

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

        public bool IsAbstract => this.TypeSymbol.IsAbstract;

        public bool IsSealed => this.TypeSymbol.IsSealed;

        public bool IsStatic => this.TypeSymbol.IsStatic;

        public bool HasDefaultConstructor =>
            this.TypeSymbol.TypeKind == RoslynTypeKind.Struct ||
            (this.TypeSymbol.TypeKind == RoslynTypeKind.Class && !this.TypeSymbol.IsAbstract &&
             this.TypeSymbol.InstanceConstructors.Any( ctor => ctor.Parameters.Length == 0 ));

        [Memo] public IReadOnlyList<INamedType> NestedTypes => this.TypeSymbol.GetTypeMembers().Select( this.Compilation.GetNamedType ).ToImmutableArray();

        [Memo]
        public IReadOnlyList<IProperty> Properties =>
            this.TypeSymbol.GetMembers().Select(
                    m => m switch
                    {
                        IPropertySymbol p => new Property( p, this ),
                        IFieldSymbol {IsImplicitlyDeclared: false} f => new Field( f, this ),
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
                .Select( m => this.Compilation.GetMethod( m ) )
                .Concat( this.Compilation.ObservableTransformations.GetByKey( this ).OfType<MethodTransformationBuilder>() )
                .ToImmutableArray();

        public INamedType MakeGenericType( params IType[] genericArguments ) => throw new NotImplementedException();

        [Memo]
        public IReadOnlyList<IGenericParameter> GenericParameters =>
            this.TypeSymbol.TypeParameters.Select( tp => this.Compilation.GetGenericParameter( tp ) ).ToImmutableList();

        public string Name => this.TypeSymbol.Name;

        [Memo] public string? Namespace => this.TypeSymbol.ContainingNamespace?.ToDisplayString();

        [Memo] public string FullName => this.TypeSymbol.ToDisplayString();

        [Memo] public IReadOnlyList<IType> GenericArguments => this.TypeSymbol.TypeArguments.Select( a => this.Compilation.GetIType( a ) ).ToImmutableList();

        [Memo]
        public override ICodeElement? ContainingElement => this.TypeSymbol.ContainingSymbol switch
        {
            INamespaceSymbol => null,
            INamedTypeSymbol containingType => this.Compilation.GetNamedType( containingType ),
            _ => throw new NotImplementedException()
        };


        public override CodeElementKind ElementKind => CodeElementKind.Type;

        [Memo] public INamedType? BaseType => this.TypeSymbol.BaseType == null ? null : this.Compilation.GetNamedType( this.TypeSymbol.BaseType );

        [Memo]
        public IReadOnlyList<INamedType> ImplementedInterfaces => this.TypeSymbol.AllInterfaces.Select( this.Compilation.GetNamedType ).ToImmutableArray();

        ITypeFactory IType.TypeFactory => this.Compilation;

    
        public INamedType WithGenericArguments( params IType[] genericArguments ) =>
            this.Compilation.GetNamedType( this.TypeSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() ) );

        public override string ToString() => this.TypeSymbol.ToString();
    }
}