using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Caravela.Framework.Impl.Templating
{
    public class TemplateCompiler
    {
        private bool TryGetAnnotatedSyntax( SyntaxNode sourceSyntaxRoot,
            SemanticModel semanticModel,
            List<Diagnostic> diagnostics,
            [NotNullWhen( true )] out SemanticAnnotationMap? symbolAnnotationMap,
            [NotNullWhen( true )] out SyntaxNode? annotatedSyntaxRoot
            )
        {
            // Annotate the syntax tree with symbols.
            symbolAnnotationMap = new SemanticAnnotationMap();
            annotatedSyntaxRoot = symbolAnnotationMap.AnnotateTree( sourceSyntaxRoot, semanticModel );


            // Annotate the syntax tree with info about build- and run-time nodes,
            var annotatorRewriter = new TemplateRewriterFilter<TemplateAnnotator>( symbolAnnotationMap,
                new TemplateAnnotator( (CSharpCompilation) semanticModel.Compilation, symbolAnnotationMap ) );

            var changeIdBefore = -1;

            int iterations = 0;

            while ( true )
            {
                iterations++;

                Debug.Assert( iterations < 32 );

                annotatedSyntaxRoot = annotatorRewriter.Visit( annotatedSyntaxRoot );

                diagnostics.AddRange( annotatorRewriter.Inner.Diagnostics );

                // Stop if we have any error.
                if ( annotatorRewriter.Inner.Diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
                {
                    return false;
                }

                // Stop if no change was detected.
                if ( changeIdBefore == annotatorRewriter.Inner.ChangeId )
                {
                    return true;
                }

                changeIdBefore = annotatorRewriter.Inner.ChangeId;
            }

            // Unreachable.
            throw new AssertionFailedException();

        }


        public bool TryCompile( SyntaxNode sourceSyntaxRoot,
            SemanticModel semanticModel,
            List<Diagnostic> diagnostics,
            [NotNullWhen( true )] out SyntaxNode? annotatedSyntaxRoot,
            [NotNullWhen( true )] out SyntaxNode? transformedSyntaxRoot )
        {

            if ( !this.TryGetAnnotatedSyntax( sourceSyntaxRoot, semanticModel, diagnostics, out var symbolAnnotationMap, out annotatedSyntaxRoot ) )
            {
                transformedSyntaxRoot = null;
                return false;
            }


            // Compile the syntax tree.
            var templateCompilerRewriter = new TemplateRewriterFilter<TemplateCompilerRewriter>( symbolAnnotationMap, 
                new TemplateCompilerRewriter( symbolAnnotationMap ) );
            transformedSyntaxRoot = templateCompilerRewriter.Visit( annotatedSyntaxRoot );

            // TODO: add diagnostics.

            return true;

        }
    }
}
