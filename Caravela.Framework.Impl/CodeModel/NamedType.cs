using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Impl.Reactive;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal sealed class NamedType : Type, INamedType
    {
        internal INamedTypeSymbol NamedTypeSymbol { get; }
        internal override ITypeSymbol TypeSymbol => NamedTypeSymbol;

        internal NamedType(INamedTypeSymbol symbol, Compilation compilation)
            : base(compilation) => NamedTypeSymbol = symbol;

        public string Name => NamedTypeSymbol.Name;

        [Memo]
        // TODO: verify simple call to ToDisplayString gives the desired result in all cases
        public string FullName => NamedTypeSymbol.ToDisplayString();

        [Memo]
        public IReadOnlyList<IType> GenericArguments => NamedTypeSymbol.TypeArguments.Select(a => Compilation.SymbolMap.GetIType(a)).ToImmutableArray();

        public ITypeInfo GetTypeInfo(in ReactiveObserverToken observerToken)
        {
            // TODO: actually use observerToken
            return TypeInfo;
        }

        [Memo]
        internal TypeInfo TypeInfo => new TypeInfo(this, Compilation);

        [Memo]
        public ICodeElement? ContainingElement => TypeSymbol.ContainingSymbol switch
        {
            INamespaceSymbol => null,
            INamedTypeSymbol containingType => Compilation.SymbolMap.GetTypeInfo(containingType),
            _ => throw new NotImplementedException()
        };

        [Memo]
        public IReactiveCollection<IAttribute> Attributes => TypeSymbol.GetAttributes().Select(a => new Attribute(a, Compilation.SymbolMap)).ToImmutableReactive();

        public override string ToString() => NamedTypeSymbol.ToString();
    }

    internal abstract class Type : IType
    {
        internal abstract ITypeSymbol TypeSymbol { get; }
        internal Compilation Compilation { get; }

        protected Type(Compilation compilation) => Compilation = compilation;

        public bool Is(IType other) => Compilation.RoslynCompilation.HasImplicitConversion(TypeSymbol, ((Type)other).TypeSymbol);
    }
}
