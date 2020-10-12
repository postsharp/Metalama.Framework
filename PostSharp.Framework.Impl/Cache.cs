using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace PostSharp.Framework.Impl
{
    /// <remarks>
    /// Cache owns <see cref="IType"/> and <see cref="IMethod"/> objects in the compilation, other objects are owned by their container.
    /// </remarks>
    internal class Cache
    {
        private readonly Compilation compilation;

        public Cache(Compilation compilation) => this.compilation = compilation;

        readonly ConcurrentDictionary<ITypeSymbol, IType> typeCache = new();
        readonly ConcurrentDictionary<IMethodSymbol, IMethod> methodCache = new();

        internal TypeInfo GetTypeInfo(INamedTypeSymbol typeSymbol) => GetNamedType(typeSymbol).TypeInfo;

        internal IType GetIType(ITypeSymbol typeSymbol) => typeCache.GetOrAdd(typeSymbol, ts => Factory.CreateIType(ts, compilation));

        internal NamedType GetNamedType(INamedTypeSymbol typeSymbol) => (NamedType)typeCache.GetOrAdd(typeSymbol, ts => new NamedType((INamedTypeSymbol)ts, compilation));

        internal IMethod GetMethod(IMethodSymbol methodSymbol) => methodCache.GetOrAdd(methodSymbol, ms => new Method(ms, compilation));
    }
}
