using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PostSharp.Framework.Impl
{
    internal sealed class NamedType : Type, INamedType
    {
        internal INamedTypeSymbol NamedTypeSymbol { get; }
        internal override ITypeSymbol TypeSymbol => NamedTypeSymbol;

        internal NamedType(INamedTypeSymbol symbol, Compilation compilation)
            : base(compilation) => NamedTypeSymbol = symbol;

        public string Name => NamedTypeSymbol.Name;

        [LazyThreadSafeProperty]
        // TODO: verify simple call to ToDisplayString gives the desired result in all cases
        public string FullName => NamedTypeSymbol.ToDisplayString();

        [LazyThreadSafeProperty]
        public IReadOnlyList<IType> GenericArguments => NamedTypeSymbol.TypeArguments.Select(a => Compilation.Cache.GetIType(a)).ToImmutableArray();

        public ITypeInfo GetTypeInfo(ITypeResolutionToken typeResolutionToken)
        {
            // TODO: actually use typeResolutionToken
            return TypeInfo;
        }

        [LazyThreadSafeProperty]
        internal TypeInfo TypeInfo => new TypeInfo(this, Compilation);

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
