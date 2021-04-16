// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            annotatedSyntaxRoot = currentSyntaxRoot;

            // Annotate the syntax tree with info about build- and run-time nodes,
            var annotatorRewriter = new TemplateAnnotator( (CSharpCompilation) semanticModel.Compilation, symbolAnnotationMap );
            annotatedSyntaxRoot = annotatorRewriter.Visit( annotatedSyntaxRoot )!;
            diagnostics.AddRange( annotatorRewriter.Diagnostics );

            // Stop if we have any error.
            if ( annotatorRewriter.Diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                return false;
            }

            return true;
        }

        public bool TryAnnotate(
            SyntaxNode sourceSyntaxRoot,
            SemanticModel semanticModel,
            List<Diagnostic> diagnostics,
            [NotNullWhen( true )] out SyntaxNode? annotatedSyntaxRoot )
            => this.TryAnnotate( sourceSyntaxRoot, semanticModel, diagnostics, out _, out annotatedSyntaxRoot );

        public bool TryCompile(
            Compilation compileTimeCompilation,
            MethodDeclarationSyntax sourceSyntaxRoot,
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
            var templateCompilerRewriter = new TemplateCompilerRewriter( compileTimeCompilation, symbolAnnotationMap );
            transformedSyntaxRoot = templateCompilerRewriter.Visit( annotatedSyntaxRoot );
            diagnostics.AddRange( templateCompilerRewriter.Diagnostics );

            return transformedSyntaxRoot != null && !templateCompilerRewriter.Diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error );
        }
    }
}
