using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl
{
    internal class Compilation : ICompilation
    {
        internal CSharpCompilation RoslynCompilation { get; }

        internal SymbolMap Cache { get; }

        internal Compilation(CSharpCompilation roslynCompilation)
        {
            RoslynCompilation = roslynCompilation;

            Cache = new(this);
        }

        [Memo]
        public IReadOnlyList<ITypeInfo> Types => RoslynCompilation.Assembly.GetTypes().Select(Cache.GetTypeInfo).ToImmutableArray();

        public INamedType? GetTypeByMetadataName(string metadataName)
        {
            var symbol = RoslynCompilation.GetTypeByMetadataName(metadataName);

            return symbol == null ? null : Cache.GetNamedType(symbol);
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
