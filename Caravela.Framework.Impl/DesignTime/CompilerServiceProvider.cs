using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class CompilerServiceProvider : ICompilerServiceProvider, IClassificationService
    {
        public static readonly CompilerServiceProvider Instance = new CompilerServiceProvider();

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
            Instance.GetType();
        }

      
        public Version Version { get; }
    
        public T? GetCompilerService<T>() where T : class, ICompilerService => typeof(T) == typeof(IClassificationService) ? (T) (object) this : null;

        public event Action<ICompilerServiceProvider> Unloaded;

        public bool TryGetClassifiedTextSpans( SemanticModel semanticModel, SyntaxNode root, out IReadOnlyClassifiedTextSpanCollection classifiedTextSpans )
        {
            // TODO: if the root is not "our", return false.
            
            var diagnostics = new List<Diagnostic>();
            var templateCompiler = new TemplateCompiler();


            templateCompiler.TryAnnotate( semanticModel.SyntaxTree.GetRoot( ), semanticModel, diagnostics, out var annotatedSyntaxRoot );

            if ( annotatedSyntaxRoot != null )
            {
              
                var text = semanticModel.SyntaxTree.GetText( );
                TextSpanClassifier classifier = new TextSpanClassifier( text );
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