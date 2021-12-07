// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Impl.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
        /// <summary>
        /// Removes invalid <c>using</c> statements from a compile-time syntax tree. Such using statements
        /// are typically run-time-only.
        /// </summary>
        internal class RemoveInvalidUsingRewriter : CSharpSyntaxRewriter
        {
            private readonly Compilation _compileTimeCompilation;

            public RemoveInvalidUsingRewriter( Compilation compileTimeCompilation )
            {
                this._compileTimeCompilation = compileTimeCompilation;
            }

            public override SyntaxNode? VisitUsingDirective( UsingDirectiveSyntax node )
            {
                var symbolInfo = this._compileTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node.Name );

                if ( symbolInfo.Symbol == null )
                {
                    return null;
                }

                return node;
            }
        }
    }
}