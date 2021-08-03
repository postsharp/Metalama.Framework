// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Formatting
{
    public partial class FormattedCodeWriter
    {
        protected const string CSharpClassTagName = "csharp";
        protected const string DiagnosticTagName = "diagnostic";
        public const string DiagnosticAnnotationName = "caravela-diagnostic";

        protected static (SourceText SourceText, ClassifiedTextSpanCollection TextSpans) GetClassifiedTextSpans(
            Document document,
            SyntaxNode? annotatedSyntaxRoot = null,
            IEnumerable<Diagnostic>? diagnostics = null,
            bool addTitles = false )
        {
            var sourceText = document.GetTextAsync().Result;
            var syntaxTree = document.GetSyntaxTreeAsync().Result!;

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
                classifiedTextSpans = new ClassifiedTextSpanCollection();
            }

            // Process the annotations by the aspect linker (on the output document).
            GeneratedCodeVisitor generatedCodeVisitor = new( classifiedTextSpans );
            generatedCodeVisitor.Visit( syntaxTree.GetRoot() );

            // Add C# classifications
            var semanticModel = document.Project.GetCompilationAsync().Result!.GetSemanticModel( syntaxTree );

            foreach ( var csharpSpan in Classifier.GetClassifiedSpans(
                    semanticModel,
                    syntaxTree.GetRoot().Span,
                    document.Project!.Solution.Workspace )
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
                visitor.Visit( syntaxTree.GetRoot() );
            }

            if ( diagnostics != null )
            {
                foreach ( var diagnostic in diagnostics )
                {
                    if ( diagnostic.Location.SourceTree?.FilePath != document.FilePath )
                    {
                        // The diagnostic is not in the current document.
                        continue;
                    }

                    foreach ( var span in classifiedTextSpans.GetClassifiedSpans( diagnostic.Location.SourceSpan ) )
                    {
                        if ( diagnostic.Location.SourceSpan.Contains( span.Span ) )
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