using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Templating
{
    public class TemplateCompiler
    {
        public const string TemplateMethodSuffix = "_Template";

        private bool TryAnnotate(
            SyntaxNode sourceSyntaxRoot,
            SemanticModel semanticModel,
            List<Diagnostic> diagnostics,
            [NotNullWhen( true )] out SemanticAnnotationMap? symbolAnnotationMap,
            [NotNullWhen( true )] out SyntaxNode? annotatedSyntaxRoot )
        {
            SyntaxNode currentSyntaxRoot;

            void FixupTreeForDiagnostics()
            {
                // Put the annotated node back into the original tree, so that diagnostics have correct locations.
                var markerAnnotation = new SyntaxAnnotation();
                var annotatedTree = sourceSyntaxRoot.SyntaxTree.GetRoot().ReplaceNode(
                    sourceSyntaxRoot,
                    currentSyntaxRoot.WithAdditionalAnnotations( markerAnnotation ) );
                currentSyntaxRoot = annotatedTree.GetAnnotatedNodes( markerAnnotation ).Single();
            }

            // Annotate the syntax tree with symbols.
            symbolAnnotationMap = new SemanticAnnotationMap();
            currentSyntaxRoot = symbolAnnotationMap.AnnotateTree( sourceSyntaxRoot, semanticModel );

            FixupTreeForDiagnostics();

            // Find calls to Proceed.
            var proceedAnnotator = new ProceedCallAnnotator( symbolAnnotationMap );
            currentSyntaxRoot = proceedAnnotator.Visit( currentSyntaxRoot )!;
            diagnostics.AddRange( proceedAnnotator.Diagnostics );

            FixupTreeForDiagnostics();

            annotatedSyntaxRoot = currentSyntaxRoot;

            // Annotate the syntax tree with info about build- and run-time nodes,
            var annotatorRewriter = new TemplateAnnotator( (CSharpCompilation) semanticModel.Compilation, symbolAnnotationMap );

            // TODO: #28266 the algorihm should now work with a single iteration. However, just removing the code breaks it.
            var changeIdBefore = -1;
            var iterations = 0;

            while ( true )
            {
                iterations++;

                Invariant.Assert( iterations < 32, "too many iterations" );

                annotatedSyntaxRoot = annotatorRewriter.Visit( annotatedSyntaxRoot )!;

                diagnostics.AddRange( annotatorRewriter.Diagnostics );

                // Stop if we have any error.
                if ( annotatorRewriter.Diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
                {
                    return false;
                }

                // Stop if no change was detected.
                if ( changeIdBefore == annotatorRewriter.ChangeId )
                {
                    return true;
                }

                changeIdBefore = annotatorRewriter.ChangeId;
            }
        }

        public bool TryAnnotate(
            SyntaxNode sourceSyntaxRoot,
            SemanticModel semanticModel,
            List<Diagnostic> diagnostics,
            [NotNullWhen( true )] out SyntaxNode? annotatedSyntaxRoot )
         => this.TryAnnotate( sourceSyntaxRoot, semanticModel, diagnostics, out _, out annotatedSyntaxRoot );

        public bool TryCompile(
            SyntaxNode sourceSyntaxRoot,
            SemanticModel semanticModel,
            List<Diagnostic> diagnostics,
            [NotNullWhen( true )] out SyntaxNode? annotatedSyntaxRoot,
            [NotNullWhen( true )] out SyntaxNode? transformedSyntaxRoot )
        {

            if ( !this.TryAnnotate( sourceSyntaxRoot, semanticModel, diagnostics, out var symbolAnnotationMap, out annotatedSyntaxRoot ) )
            {
                transformedSyntaxRoot = null;
                return false;
            }

            // Compile the syntax tree.
            var templateCompilerRewriter = new TemplateCompilerRewriter( symbolAnnotationMap );
            transformedSyntaxRoot = templateCompilerRewriter.Visit( annotatedSyntaxRoot );

            // TODO: add diagnostics.
            if ( transformedSyntaxRoot == null )
            {
                return false;
            }

            return true;
        }
    }
}
