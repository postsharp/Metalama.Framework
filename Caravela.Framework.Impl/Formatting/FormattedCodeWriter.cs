// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.Formatting
{
    public partial class FormattedCodeWriter
    {
        protected const string CSharpClassTagName = "csharp";
        protected const string DiagnosticTagName = "diagnostic";
        private const string _diagnosticAnnotationName = "caravela-diagnostic";

        public FormattedCodeWriter( IServiceProvider serviceProvider )
        {
            this.ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; set; }

        public static T AddDiagnosticAnnotations<T>( T syntaxRoot, string? filePath, IEnumerable<Diagnostic>? diagnostics )
            where T : SyntaxNode
        {
            if ( diagnostics == null || filePath == null || syntaxRoot.Span.IsEmpty )
            {
                // Coverage: ignore.
                return syntaxRoot;
            }

            var fileName = Path.GetFileName( filePath );

            var outputSyntaxRoot = syntaxRoot;

            foreach ( var diagnostic in diagnostics )
            {
                var position = diagnostic.Location.GetLineSpan();

                if ( !position.IsValid || !Path.GetFileName( position.Path ).Equals( fileName, StringComparison.OrdinalIgnoreCase ) )
                {
                    // The diagnostic is not in the current document.
                    continue;
                }

                var serializedDiagnostic = new DiagnosticAnnotation( diagnostic );

                if ( outputSyntaxRoot.Span.Contains( diagnostic.Location.SourceSpan ) )
                {
                    var node = outputSyntaxRoot.FindNode( diagnostic.Location.SourceSpan );

                    outputSyntaxRoot = outputSyntaxRoot.ReplaceNode(
                        node,
                        node.WithAdditionalAnnotations(
                            new SyntaxAnnotation(
                                _diagnosticAnnotationName,
                                serializedDiagnostic.ToJson() ) ) );
                }
            }

            return outputSyntaxRoot;
        }

        protected async Task<ClassifiedTextSpanCollection> GetClassifiedTextSpansAsync(
            Document document,
            bool areNodesAnnotated = false,
            IEnumerable<Diagnostic>? diagnostics = null,
            bool addTitles = false )
        {
            var sourceText = await document.GetTextAsync();
            var syntaxTree = (await document.GetSyntaxTreeAsync())!;

            if ( await syntaxTree.GetTextAsync() != sourceText )
            {
                throw new AssertionFailedException();
            }

            var syntaxRoot = await syntaxTree.GetRootAsync();

            var compilation = await document.Project.GetCompilationAsync();
            var semanticModel = compilation!.GetSemanticModel( syntaxTree );
            var classificationService = new ClassificationService( this.ServiceProvider );

            ClassifiedTextSpanCollection classifiedTextSpans;

            if ( areNodesAnnotated )
            {
                // Aspect Workbench uses this branch.
                classifiedTextSpans = classificationService.GetClassifiedTextSpansOfAnnotatedSyntaxTree( syntaxTree, CancellationToken.None );
            }
            else
            {
                // Annotate the whole syntax tree with the classification service.
                // Note that we don't take into account the output of the template compiler executed from the pipeline,
                // because the template compiler, when executed from the pipeline, only adds annotations to templates, not to the whole syntax tree.

                
                classifiedTextSpans = classificationService.GetClassifiedTextSpans( semanticModel, CancellationToken.None );
            }

        // Process the annotations by the aspect linker (on the output document).
            FormattingVisitor formattingVisitor = new( classifiedTextSpans );
            formattingVisitor.Visit( syntaxRoot );

            foreach ( var csharpSpan in Classifier.GetClassifiedSpans(
                    semanticModel,
                    syntaxRoot.Span,
                    document.Project.Solution.Workspace )
                .OrderBy( c => c.TextSpan.Start )
                .ThenBy( c => c.ClassificationType ) )
            {
                foreach ( var existingSpan in classifiedTextSpans.GetClassifiedSpans( csharpSpan.TextSpan ) )
                {
                    var combinedClassification =
                        existingSpan.Tags != null! && existingSpan.Tags.TryGetValue( CSharpClassTagName, out var existingClassification )
                            ? existingClassification + ";" + csharpSpan.ClassificationType
                            : csharpSpan.ClassificationType;

                    var intersection = csharpSpan.TextSpan.Intersection( csharpSpan.TextSpan ).AssertNotNull();

                    classifiedTextSpans.SetTag( intersection, CSharpClassTagName, combinedClassification );
                }
            }

            // Add XML doc based on the input compilation.
            if ( addTitles )
            {
                var visitor = new AddTitlesVisitor( classifiedTextSpans, semanticModel );
                visitor.Visit( syntaxRoot );
            }

            if ( diagnostics != null && document.FilePath != null )
            {
                // Coverage: ignore (used by Aspect Workbench).
                AddDiagnostics( document, diagnostics, classifiedTextSpans );
            }

            return classifiedTextSpans;
        }

        // Coverage: ignore (used by Aspect Workbench).
        private static void AddDiagnostics( Document document, IEnumerable<Diagnostic> diagnostics, ClassifiedTextSpanCollection classifiedTextSpans )
        {
            foreach ( var diagnostic in diagnostics )
            {
                var position = diagnostic.Location.GetLineSpan();

                if ( !position.IsValid || !position.Path.EndsWith( document.FilePath, StringComparison.OrdinalIgnoreCase ) )
                {
                    // The diagnostic is not in the current document.
                    continue;
                }

                foreach ( var span in classifiedTextSpans.GetClassifiedSpans( diagnostic.Location.SourceSpan ) )
                {
                    if ( diagnostic.Location.SourceSpan.IntersectsWith( span.Span ) )
                    {
                        classifiedTextSpans.SetTag(
                            diagnostic.Location.SourceSpan,
                            DiagnosticTagName,
                            new DiagnosticAnnotation( diagnostic ).ToJson() );
                    }
                }
            }
        }
    }
}