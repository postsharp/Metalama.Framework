using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
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

        public bool IsDefaultConstructible =>
            this.TypeSymbol.TypeKind == TypeKind.Struct ||
            (this.TypeSymbol.TypeKind == TypeKind.Class && !this.TypeSymbol.IsAbstract && this.TypeSymbol.InstanceConstructors.Any( ctor => ctor.Parameters.Length == 0 ));

        /// <summary>
        /// Filters members to only those that were declared in this type.
        /// </summary>
        private IReactiveCollection<T> OnlyDeclared<T>( IReactiveCollection<T> source )
            where T : IMember =>
            source.Where( m => ((CodeElement) (object) m).Symbol.DeclaringSyntaxReferences.Any(
                memberReference => this.Symbol.DeclaringSyntaxReferences.Any( typeReference =>
                    typeReference.GetSyntax().Contains( memberReference.GetSyntax() ) ) ) );

        [Memo]
        public IReactiveCollection<INamedType> NestedTypes => this.TypeSymbol.GetTypeMembers().Select( this.SymbolMap.GetNamedType ).ToImmutableReactive();

        [Memo]
        public IReactiveCollection<IProperty> AllProperties => this.TypeSymbol.GetMembers().OfType<IPropertySymbol>().Select(p => new Property(p, this)).ToImmutableReactive();

        [Memo]
        public IReactiveCollection<IProperty> Properties => this.OnlyDeclared( this.AllProperties );

        public IReactiveCollection<IEvent> AllEvents => throw new NotImplementedException();

        [Memo]
        public IReactiveCollection<IEvent> Events => this.OnlyDeclared( this.AllEvents );

        [Memo]
        public IReactiveCollection<IMethod> AllMethods => this.TypeSymbol.GetMembers().OfType<IMethodSymbol>().Select(m => this.SymbolMap.GetMethod(m)).ToImmutableReactive();

        [Memo]
        public IReactiveCollection<IMethod> Methods => this.OnlyDeclared( this.AllMethods );

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public string Name => this.TypeSymbol.Name;

        [Memo]
        public string? Namespace => this.TypeSymbol.ContainingNamespace?.ToDisplayString();

        [Memo]
        // TODO: verify simple call to ToDisplayString gives the desired result in all cases
        public string FullName => this.TypeSymbol.ToDisplayString();

        [Memo]
        public IReadOnlyList<IType> GenericArguments => this.TypeSymbol.TypeArguments.Select( a => this.Compilation.SymbolMap.GetIType( a ) ).ToImmutableArray();

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

        [Memo]
        public INamedType? BaseType => this.TypeSymbol.BaseType == null ? null : this.Compilation.SymbolMap.GetNamedType( this.TypeSymbol.BaseType );

        [Memo]
        public IReactiveCollection<INamedType> ImplementedInterfaces => this.TypeSymbol.AllInterfaces.Select( this.Compilation.SymbolMap.GetNamedType ).ToImmutableReactive();

        public bool Is( IType other ) => this.Compilation.RoslynCompilation.HasImplicitConversion( this.TypeSymbol, other.GetSymbol() );

        public override string ToString() => this.TypeSymbol.ToString();
    }
}
