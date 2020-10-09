using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PostSharp.Framework.Impl
{
    internal class NamedType : INamedType
    {
        internal INamedTypeSymbol Symbol { get; }
        internal Compilation Compilation { get; }

        internal NamedType(INamedTypeSymbol symbol, Compilation compilation)
        {
            Symbol = symbol;
            Compilation = compilation;
        }

        public string Name => Symbol.Name;

        [LazyThreadSafeProperty]
        // TODO: verify simple call to ToDisplayString gives the desired result in all cases
        public string FullName => Symbol.ToDisplayString();

        [LazyThreadSafeProperty]
        public IReadOnlyList<IType> GenericArguments => Symbol.TypeArguments.Select(a => Compilation.Cache.GetIType(a)).ToImmutableArray();

        public ITypeInfo GetTypeInfo(ITypeResolutionToken typeResolutionToken)
        {
            // TODO: actually use typeResolutionToken
            return TypeInfo;
        }

        [LazyThreadSafeProperty]
        internal TypeInfo TypeInfo => new TypeInfo(this, Compilation);
    }
}
