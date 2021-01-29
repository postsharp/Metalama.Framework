using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using TypeKind = Caravela.Framework.Code.TypeKind;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class NamedType : CodeElement, INamedType, ITypeInternal
    {
        internal INamedTypeSymbol TypeSymbol { get; }
        ITypeSymbol ITypeInternal.TypeSymbol => this.TypeSymbol;
        protected internal override ISymbol Symbol => this.TypeSymbol;

        internal override SourceCompilation Compilation { get; }

        internal NamedType(INamedTypeSymbol typeSymbol, SourceCompilation compilation)
        {
            this.TypeSymbol = typeSymbol;
            this.Compilation = compilation;
        }

        TypeKind IType.Kind => this.TypeSymbol.TypeKind switch
        {
            RoslynTypeKind.Class => TypeKind.Class,
            RoslynTypeKind.Delegate => TypeKind.Delegate,
            RoslynTypeKind.Enum => TypeKind.Enum,
            RoslynTypeKind.Interface => TypeKind.Interface,
            RoslynTypeKind.Struct => TypeKind.Struct,
            _ => throw new InvalidOperationException($"Unexpected type kind {this.TypeSymbol.TypeKind}.")
        };

        public bool HasDefaultConstructor =>
            this.TypeSymbol.TypeKind == RoslynTypeKind.Struct ||
            (this.TypeSymbol.TypeKind == RoslynTypeKind.Class && !this.TypeSymbol.IsAbstract && this.TypeSymbol.InstanceConstructors.Any( ctor => ctor.Parameters.Length == 0 ));

        [Memo]
        public IReactiveCollection<INamedType> NestedTypes => this.TypeSymbol.GetTypeMembers().Select( this.SymbolMap.GetNamedType ).ToImmutableReactive();

        [Memo]
        public IReactiveCollection<IProperty> Properties =>
            this.TypeSymbol.GetMembers().Select( m => m switch
            {
                IPropertySymbol p => new Property( p, this ),
                IFieldSymbol { IsImplicitlyDeclared: false } f => new Field( f, this ),
                _ => (IProperty) null!
            } )
            .Where( p => p != null )
            .ToImmutableReactive();

        [Memo]
        public IReactiveCollection<IEvent> Events => this.TypeSymbol.GetMembers().OfType<IEventSymbol>().Select( e => new Event( e, this ) ).ToImmutableReactive();

        [Memo]
        public IReactiveCollection<IMethod> Methods => this.TypeSymbol.GetMembers().OfType<IMethodSymbol>().Select(m => this.SymbolMap.GetMethod(m)).ToImmutableReactive();

        [Memo]
        public IImmutableList<IGenericParameter> GenericParameters =>
            this.TypeSymbol.TypeParameters.Select( tp => this.SymbolMap.GetGenericParameter( tp ) ).ToImmutableList();

        public string Name => this.TypeSymbol.Name;

        [Memo]
        public string? Namespace => this.TypeSymbol.ContainingNamespace?.ToDisplayString();

        [Memo]
        public string FullName => this.TypeSymbol.ToDisplayString();

        [Memo]
        public IImmutableList<IType> GenericArguments => this.TypeSymbol.TypeArguments.Select( a => this.Compilation.SymbolMap.GetIType( a ) ).ToImmutableList();

        [Memo]
        public override ICodeElement? ContainingElement => this.TypeSymbol.ContainingSymbol switch
        {
            INamespaceSymbol => null,
            INamedTypeSymbol containingType => this.Compilation.SymbolMap.GetNamedType( containingType ),
            _ => throw new NotImplementedException()
        };

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes =>
            this.TypeSymbol.GetAttributes().Select( a => new Attribute( a, this.Compilation.SymbolMap ) ).ToImmutableReactive();

        public override CodeElementKind Kind => CodeElementKind.Type;

        [Memo]
        public INamedType? BaseType => this.TypeSymbol.BaseType == null ? null : this.Compilation.SymbolMap.GetNamedType( this.TypeSymbol.BaseType );

        [Memo]
        public IReactiveCollection<INamedType> ImplementedInterfaces => this.TypeSymbol.AllInterfaces.Select( this.Compilation.SymbolMap.GetNamedType ).ToImmutableReactive();

        public bool Is( IType other ) => this.Compilation.RoslynCompilation.HasImplicitConversion( this.TypeSymbol, other.GetSymbol() );

        public bool Is( Type other ) =>
            this.Is( this.Compilation.GetTypeByReflectionType( other ) ?? throw new ArgumentException( $"Could not resolve type {other}.", nameof( other ) ) );

        public IArrayType MakeArrayType( int rank = 1 ) =>
            (IArrayType) this.SymbolMap.GetIType( this.Compilation.RoslynCompilation.CreateArrayTypeSymbol( this.TypeSymbol, rank ) );

        public IPointerType MakePointerType() =>
            (IPointerType) this.SymbolMap.GetIType( this.Compilation.RoslynCompilation.CreatePointerTypeSymbol( this.TypeSymbol ) );

        public INamedType WithGenericArguments( params IType[] genericArguments ) =>
            this.SymbolMap.GetNamedType( this.TypeSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() ) );

        public override string ToString() => this.TypeSymbol.ToString();
    }
}
