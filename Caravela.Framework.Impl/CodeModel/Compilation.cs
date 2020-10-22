﻿using System;
using System.Linq;
using Caravela.Reactive;
using Caravela.Reactive.Sources;
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
        public IReactiveCollection<ITypeInfo> DeclaredTypes => this.RoslynCompilation.Assembly.GetTypes().Select( this.SymbolMap.GetTypeInfo ).ToImmutableReactive();

        [Memo]
        public IReactiveCollection<INamedType> DeclaredAndReferencedTypes => this.RoslynCompilation.GetTypes().Select( this.SymbolMap.GetNamedType ).ToImmutableReactive();

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
    public static class CompilationFactory
    {
        public static ICompilation CreateCompilation(CSharpCompilation roslynCompilation) => new Compilation(roslynCompilation);
    }
}
