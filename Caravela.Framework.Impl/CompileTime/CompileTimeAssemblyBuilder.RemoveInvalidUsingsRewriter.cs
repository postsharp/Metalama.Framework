using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeAssemblyBuilder
    {
        // PERF: explicitly skip over all other nodes?
        internal class RemoveInvalidUsingsRewriter : CSharpSyntaxRewriter
        {
            private readonly Compilation _compilation;

            public RemoveInvalidUsingsRewriter( Compilation compilation )
            {
                this._compilation = compilation;
            }

            public override SyntaxNode? VisitUsingDirective( UsingDirectiveSyntax node )
            {
                var symbolInfo = this._compilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node.Name );

                if ( symbolInfo.Symbol == null )
                {
                    return null;
                }

                return node;
            }
        }
    }
}
