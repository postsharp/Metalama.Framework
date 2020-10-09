using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PostSharp.Framework.Impl
{
    internal class Compilation : ICompilation
    {
        internal CSharpCompilation RoslynCompilation { get; }

        internal Cache Cache { get; }

        internal Compilation(CSharpCompilation roslynCompilation)
        {
            RoslynCompilation = roslynCompilation;

            Cache = new(this);
        }

        [LazyThreadSafeProperty]
        public IReadOnlyList<ITypeInfo> Types => GetTypes(RoslynCompilation.Assembly.GlobalNamespace).ToImmutableArray();

        private IEnumerable<ITypeInfo> GetTypes(INamespaceSymbol ns)
        {
            foreach (var type in ns.GetTypeMembers())
            {
                yield return Cache.GetTypeInfo(type);
            }

            foreach (var namespaceMember in ns.GetNamespaceMembers())
            {
                foreach (var type in GetTypes(namespaceMember))
                    yield return type;
            }
        }
    }

    // TODO: rename to TypeCache?
    // TODO: does this ownership work for accessor methods?
    /// <remarks>
    /// Cache owns <see cref="IType"/> objects in the compilation, all other objects are owned by their container.
    /// </remarks>
    internal class Cache
    {
        private readonly Compilation compilation;

        public Cache(Compilation compilation) => this.compilation = compilation;

        readonly ConcurrentDictionary<ITypeSymbol, IType> typeCache = new();

        internal TypeInfo GetTypeInfo(INamedTypeSymbol typeSymbol) => GetNamedType(typeSymbol).TypeInfo;

        internal IType GetIType(ITypeSymbol typeSymbol) => typeCache.GetOrAdd(typeSymbol, ts => Factory.CreateIType(ts, compilation));

        internal NamedType GetNamedType(INamedTypeSymbol typeSymbol) => (NamedType)typeCache.GetOrAdd(typeSymbol, ts => new NamedType((INamedTypeSymbol)ts, compilation));
    }

    internal static class Factory
    {
        internal static IType CreateIType(ITypeSymbol typeSymbol, Compilation compilation) =>
            typeSymbol switch
            {
                INamedTypeSymbol namedType => new NamedType(namedType, compilation),
                _ => throw new NotImplementedException()
            };
    }

    // for testing
    public static class CodeModel
    {
        public static ICompilation CreateCompilation(CSharpCompilation roslynCompilation) => new Compilation(roslynCompilation);
    }
}
