using System.Collections.Concurrent;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <remarks>
    /// Symbol map owns <see cref="IType"/> and <see cref="IMethod"/> objects in the compilation, other objects are owned by their container.
    /// </remarks>
    internal class SymbolMap
    {
        private readonly SourceCompilation compilation;

        public SymbolMap(SourceCompilation compilation) => this.compilation = compilation;

        readonly ConcurrentDictionary<ITypeSymbol, IType> typeCache = new();
        readonly ConcurrentDictionary<IMethodSymbol, IMethod> methodCache = new();

        internal IType GetIType(ITypeSymbol typeSymbol) => this.typeCache.GetOrAdd(typeSymbol, ts => Factory.CreateIType(ts, this.compilation ));

        internal NamedType GetNamedType(INamedTypeSymbol typeSymbol) => (NamedType) this.typeCache.GetOrAdd(typeSymbol, ts => new NamedType((INamedTypeSymbol)ts, this.compilation ));

        internal IMethod GetMethod(IMethodSymbol methodSymbol) => this.methodCache.GetOrAdd(methodSymbol, ms => new Method(ms, this.compilation ));
    }
}
