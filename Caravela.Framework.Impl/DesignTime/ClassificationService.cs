// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class ClassificationService : IClassificationService
    {
        public bool TryGetClassifiedTextSpans(
            SemanticModel semanticModel,
            SyntaxNode root,
            [NotNullWhen( true )] out IReadOnlyClassifiedTextSpanCollection? classifiedTextSpans )
        {
            // TODO: if the root is not "our", return false.

            var diagnostics = new DiagnosticList();

            var templateCompiler = new TemplateCompiler( ServiceProvider.Empty );

            _ = templateCompiler.TryAnnotate( semanticModel.SyntaxTree.GetRoot(), semanticModel, diagnostics, out var annotatedSyntaxRoot );

            if ( annotatedSyntaxRoot != null )
            {
                var text = semanticModel.SyntaxTree.GetText();
                var classifier = new TextSpanClassifier( text );
                classifier.Visit( annotatedSyntaxRoot );
                classifiedTextSpans = classifier.ClassifiedTextSpans;
            }
            else
            {
                classifiedTextSpans = null;

                return false;
            }

            return true;
        }
    }
}