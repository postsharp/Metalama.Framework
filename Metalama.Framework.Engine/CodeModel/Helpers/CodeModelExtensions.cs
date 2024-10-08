// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Helpers
{
    public static class CodeModelExtensions
    {
        internal static SyntaxNode? GetPrimaryDeclarationSyntax( this IDeclaration declaration )
        {
            return declaration.GetSymbol()?.GetPrimaryDeclarationSyntax();
        }

        internal static SyntaxNode? GetPrimaryDeclarationSyntax( this IFullRef declaration )
        {
            return declaration.GetClosestContainingSymbol().GetPrimaryDeclarationSyntax();
        }

        public static SyntaxTree? GetPrimarySyntaxTree( this IDeclaration declaration )
            => declaration switch
            {
                IDeclarationImpl declarationImpl => declarationImpl.PrimarySyntaxTree,
                _ => throw new AssertionFailedException( $"The type {declaration.GetType()} does not implement IDeclarationImpl." )
            };
    }
}