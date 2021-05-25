// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// The implementation of <see cref="IClassificationService"/>.
    /// </summary>
    internal class ClassificationService : IClassificationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ClassificationService( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
        }

        public bool TryGetClassifiedTextSpans(
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out IReadOnlyClassifiedTextSpanCollection? classifiedTextSpans )
        {
            // TODO: if the root is not "our", return false.

            var diagnostics = new DiagnosticList();

            var templateCompiler = new TemplateCompiler( this._serviceProvider );

            _ = templateCompiler.TryAnnotate( semanticModel.SyntaxTree.GetRoot(), semanticModel, diagnostics, cancellationToken, out var annotatedSyntaxRoot );

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