using System;
using System.Linq;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl
{
    internal class Compilation : ICompilation
    {
        internal CSharpCompilation RoslynCompilation { get; }

        internal SymbolMap SymbolMap { get; }

        internal Compilation(CSharpCompilation roslynCompilation)
        {
            RoslynCompilation = roslynCompilation;

            SymbolMap = new(this);
        }

        [Memo]
        public IReactiveCollection<ITypeInfo> Types => RoslynCompilation.Assembly.GetTypes().Select(SymbolMap.GetTypeInfo).ToImmutableReactive();

        public INamedType? GetTypeByMetadataName(string metadataName)
        {
            var symbol = RoslynCompilation.GetTypeByMetadataName(metadataName);

            return symbol == null ? null : SymbolMap.GetNamedType(symbol);
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
