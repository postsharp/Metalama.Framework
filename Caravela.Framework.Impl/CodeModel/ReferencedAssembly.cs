﻿using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class ReferencedAssembly : CodeElement, IAssembly
    {
        public ReferencedAssembly( IAssemblySymbol assemblySymbol, CompilationModel compilation ) : base(compilation)
        {
            this.AssemblySymbol = assemblySymbol;
        }

        public IAssemblySymbol AssemblySymbol { get; }

        public override CodeElementKind ElementKind => CodeElementKind.Assembly;

        public override ISymbol Symbol => this.AssemblySymbol;
    }
}
