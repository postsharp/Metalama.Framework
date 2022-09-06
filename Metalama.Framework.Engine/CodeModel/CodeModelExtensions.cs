// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel
{
    public static class CodeModelExtensions
    {
        public static Compilation GetRoslynCompilation( this ICompilation compilation ) => ((CompilationModel) compilation).RoslynCompilation;

        public static bool TryGetDeclaration( this ICompilation compilation, ISymbol symbol, out IDeclaration? declaration )
            => ((CompilationModel) compilation).Factory.TryGetDeclaration( symbol, out declaration );
    }
}