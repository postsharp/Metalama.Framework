using System;
using System.Linq;
using Caravela.Reactive;
using Caravela.Reactive.Collections;
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
            this.RoslynCompilation = roslynCompilation;

            this.SymbolMap = new(this);
        }

        [Memo]
        public IReactiveCollection<ITypeInfo> Types => this.RoslynCompilation.Assembly.GetTypes().Select( this.SymbolMap.GetTypeInfo).ToImmutableReactive();

        public INamedType? GetTypeByMetadataName(string metadataName)
        {
            var symbol = this.RoslynCompilation.GetTypeByMetadataName(metadataName);

            return symbol == null ? null : this.SymbolMap.GetNamedType(symbol);
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
