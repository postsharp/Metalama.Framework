// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Caravela.Framework.Impl.Formatting
{
    public partial class FormattedCodeWriter
    {
        protected const string CSharpClassTagName = "csharp";
        protected const string DiagnosticTagName = "diagnostic";
        public const string DiagnosticAnnotationName = "caravela-diagnostic";

        public static T AddDiagnosticAnnotations<T>( T syntaxRoot, string? filePath, IEnumerable<Diagnostic>? diagnostics )
            where T : SyntaxNode
        {
            if ( diagnostics == null || filePath == null )
            {
                return syntaxRoot;
            }

            var fileName = Path.GetFileName( filePath );

            var outputSyntaxRoot = syntaxRoot;

            foreach ( var diagnostic in diagnostics )
            {
                var position = diagnostic.Location.GetLineSpan();

                if ( !position.IsValid || !position.Path.EndsWith( filePath, StringComparison.OrdinalIgnoreCase ) )
                {
                    // The diagnostic is not in the current document.
                    continue;
                }

                if ( Path.GetFileName( diagnostic.Location.SourceTree!.FilePath ) == fileName )
                {
                    var node = outputSyntaxRoot.FindNode( diagnostic.Location.SourceSpan );

                    outputSyntaxRoot = outputSyntaxRoot.ReplaceNode(
                        node,
                        node.WithAdditionalAnnotations(
                            new SyntaxAnnotation(
                                DiagnosticAnnotationName,
                                diagnostic.Severity + ": " + diagnostic.GetMessage() ) ) );
                }
            }

            return outputSyntaxRoot;
        }

        protected static (SourceText SourceText, ClassifiedTextSpanCollection TextSpans) GetClassifiedTextSpans(
            Document document,
            SyntaxNode? annotatedSyntaxRoot = null,
            IEnumerable<Diagnostic>? diagnostics = null,
            bool addTitles = false )
        {
            var sourceText = document.GetTextAsync().Result;
            var syntaxTree = document.GetSyntaxTreeAsync().Result!;

            if ( syntaxTree.GetText() != sourceText )
            {
                throw new AssertionFailedException();
            }
            
            var syntaxRoot = syntaxTree.GetRoot();

            // Process the annotations by the template compiler.
            ClassifiedTextSpanCollection classifiedTextSpans;

            if ( annotatedSyntaxRoot != null )
            {
                var classifier = new TextSpanClassifier( sourceText, detectRegion: true );
                classifier.Visit( annotatedSyntaxRoot );
                classifiedTextSpans = (ClassifiedTextSpanCollection) classifier.ClassifiedTextSpans;
            }
            else
            {
                classifiedTextSpans = new ClassifiedTextSpanCollection( sourceText );
            }

            // Process the annotations by the aspect linker (on the output document).
            GeneratedCodeVisitor generatedCodeVisitor = new( classifiedTextSpans );
            generatedCodeVisitor.Visit( syntaxRoot );

            // Add C# classifications
            var semanticModel = document.Project.GetCompilationAsync().Result!.GetSemanticModel( syntaxTree );

            foreach ( var csharpSpan in Classifier.GetClassifiedSpans(
                    semanticModel,
                    syntaxRoot.Span,
                    document.Project.Solution.Workspace )
                .OrderBy( c => c.TextSpan.Start )
                .ThenBy( c => c.ClassificationType ) )
            {
                var existingSpan = classifiedTextSpans.GetClassifiedSpans( csharpSpan.TextSpan ).SingleOrDefault();

                var combinedClassification = existingSpan.Tags != null! && existingSpan.Tags.TryGetValue( CSharpClassTagName, out var existingClassification )
                    ? existingClassification + ";" + csharpSpan.ClassificationType
                    : csharpSpan.ClassificationType;

                classifiedTextSpans.SetTag( csharpSpan.TextSpan, CSharpClassTagName, combinedClassification );
            }

            // Add XML doc based on the input compilation.
            if ( addTitles )
            {
                var visitor = new AddTitlesVisitor( classifiedTextSpans, semanticModel );
                visitor.Visit( syntaxRoot );
            }

            if ( diagnostics != null && document.FilePath != null )
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
                                diagnostic.Severity + ": " + diagnostic.GetMessage() );
                        }
                    }
                }
            }

            return (sourceText, classifiedTextSpans);
        }
    }
}