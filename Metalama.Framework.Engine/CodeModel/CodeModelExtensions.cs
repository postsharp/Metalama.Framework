// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel
{
    public static class CodeModelExtensions
    {
        public static Compilation GetRoslynCompilation( this ICompilation compilation ) => ((CompilationModel) compilation).RoslynCompilation;

        public static bool TryGetDeclaration( this ICompilation compilation, ISymbol symbol, out IDeclaration? declaration )
            => ((CompilationModel) compilation).Factory.TryGetDeclaration( symbol, out declaration );

        public static SyntaxNode? GetPrimaryDeclarationSyntax( this IDeclaration declaration )
        {
            return declaration.GetSymbol()?.GetPrimaryDeclaration();
        }

        public static SyntaxTree? GetPrimarySyntaxTree( this IDeclaration declaration )
            => declaration switch
            {
                IDeclarationImpl declarationImpl => declarationImpl.PrimarySyntaxTree,
                _ => throw new AssertionFailedException( $"The type {declaration.GetType()} does not implement IDeclarationImpl." )
            };
    }
}