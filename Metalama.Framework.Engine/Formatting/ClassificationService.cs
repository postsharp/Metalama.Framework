// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.Formatting
{
    [ExcludeFromCodeCoverage]
    public class ClassificationService
    {
        private readonly ProjectServiceProvider _serviceProvider;

        public ClassificationService( ProjectServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
        }

        public static bool ContainsCompileTimeCode( SyntaxNode syntaxRoot ) => CompileTimeCodeFastDetector.HasCompileTimeCode( syntaxRoot );

        public ClassifiedTextSpanCollection GetClassifiedTextSpans( SemanticModel model, CancellationToken cancellationToken )
        {
            var syntaxRoot = model.SyntaxTree.GetRoot();
            var diagnostics = new DiagnosticBag();

            var templateCompiler = new TemplateCompiler( this._serviceProvider, model.Compilation );

            _ = templateCompiler.TryAnnotate( syntaxRoot, model, diagnostics, cancellationToken, out var annotatedSyntaxRoot, out _ );

            var text = model.SyntaxTree.GetText();
            var classifier = new TextSpanClassifier( text );
            classifier.Visit( annotatedSyntaxRoot );
            classifier.ClassifiedTextSpans.Polish();

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