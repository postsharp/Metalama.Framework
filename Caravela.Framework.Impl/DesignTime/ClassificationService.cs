// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
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
    /// <remarks>
    /// This class is public because it is used by Try.Caravela.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public class ClassificationService : IClassificationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ClassificationService( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
        }

        public bool ContainsCompileTimeCode( SyntaxNode syntaxRoot ) => CompileTimeCodeDetector.HasCompileTimeCode( syntaxRoot );

        IClassifiedTextSpans IClassificationService.GetClassifiedTextSpans(
            SemanticModel model,
            CancellationToken cancellationToken )
            => this.GetClassifiedTextSpans( model, cancellationToken );

        public ClassifiedTextSpanCollection GetClassifiedTextSpans( SemanticModel model, CancellationToken cancellationToken )
        {
            var syntaxRoot = model.SyntaxTree.GetRoot();
            var diagnostics = new DiagnosticList();

            var templateCompiler = new TemplateCompiler( this._serviceProvider, model.Compilation );

            _ = templateCompiler.TryAnnotate( syntaxRoot, model, diagnostics, cancellationToken, out var annotatedSyntaxRoot );

            var text = model.SyntaxTree.GetText();
            var classifier = new TextSpanClassifier( text );
            classifier.Visit( annotatedSyntaxRoot );

            return classifier.ClassifiedTextSpans;
        }

        public static ClassifiedTextSpanCollection GetClassifiedTextSpansOfAnnotatedSyntaxTree( SyntaxTree syntaxTree, CancellationToken cancellationToken )
        {
            var text = syntaxTree.GetText();
            var classifier = new TextSpanClassifier( text );
            classifier.Visit( syntaxTree.GetRoot() );

            return classifier.ClassifiedTextSpans;
        }
    }
}