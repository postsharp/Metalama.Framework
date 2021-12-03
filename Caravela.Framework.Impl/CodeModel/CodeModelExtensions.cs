// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    public static class CodeModelExtensions
    {
        public static Compilation GetRoslynCompilation( this ICompilation compilation ) => ((CompilationModel) compilation).RoslynCompilation;

        public static bool TryGetDeclaration( this ICompilation compilation, ISymbol symbol, out IDeclaration? declaration )
            => ((CompilationModel) compilation).Factory.TryGetDeclaration( symbol, out declaration );
    }
}