using System;
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
