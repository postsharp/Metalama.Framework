// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class CompilerServiceProvider : ICompilerServiceProvider, IClassificationService
    {
        public static readonly CompilerServiceProvider Instance = new();

        static CompilerServiceProvider()
        {
            DesignTimeEntryPointManager.Instance.RegisterServiceProvider( Instance );
        }

        public CompilerServiceProvider()
        {
            this.Version = this.GetType().Assembly.GetName().Version;
        }

        public static void Initialize()
        {
            // Make sure the type is initialized.
            _ = Instance.GetType();
        }

        public Version Version { get; }

        public T? GetCompilerService<T>()
            where T : class, ICompilerService
            => typeof( T ) == typeof( IClassificationService ) ? (T) (object) this : null;

        event Action<ICompilerServiceProvider>? ICompilerServiceProvider.Unloaded
        {
            add { }
            remove { }
        }

        public bool TryGetClassifiedTextSpans( SemanticModel semanticModel, SyntaxNode root, [NotNullWhen( true )] out IReadOnlyClassifiedTextSpanCollection? classifiedTextSpans )
        {
            // TODO: if the root is not "our", return false.

            var diagnostics = new List<Diagnostic>();
            var templateCompiler = new TemplateCompiler();

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