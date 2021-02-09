using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// A <see cref="CSharpSyntaxRewriter"/> that flattens blocks from the output of <see cref="TemplateCompiler"/>.
    /// Some blocks must be flattened for semantic reasons, other just for aesthetic ones.
    /// </summary>
    internal class FlattenBlocksRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitBlock( BlockSyntax node )
        {
            // This flattens the block structure when possible (i.e. there is no local variable)
            // and when it is requested through an annotation.

            var statements = new List<StatementSyntax>();

            void Flatten( BlockSyntax block )
            {
                foreach ( var statement in block.Statements )
                {
                    var processedStatement = (StatementSyntax) this.Visit( statement );

                    switch ( processedStatement )
                    {
                        case EmptyStatementSyntax _:
                            continue;

                        case BlockSyntax subBlock:
                            {
                                var mustFlatten = subBlock.HasFlattenBlockAnnotation();

                                if ( mustFlatten ||
                                    !subBlock.Statements.Any( s => s is LocalDeclarationStatementSyntax ) )
                                {
                                    Flatten( subBlock );
                                }
                                else
                                {
                                    statements.Add( processedStatement );
                                }

                                break;
                            }

                        default:
                            statements.Add( processedStatement );
                            break;
                    }
                }
            }

            Flatten( node );

            return node.CopyAnnotationsTo( Block( statements.ToArray() ) );
        }
    }
}