using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Caravela.Framework.DesignTime
{
    internal class ProjectDesignTimeEntryPoint : IDesignTimeEntryPoint, IProjectDesignTimeEntryPoint
    {
        public static readonly ProjectDesignTimeEntryPoint Instance = new ProjectDesignTimeEntryPoint();

        static ProjectDesignTimeEntryPoint()
        {
            DesignTimeEntryPointManager.RegisterEntryPoint( Instance );
        }

        public ProjectDesignTimeEntryPoint()
        {
            this.Version = this.GetType().Assembly.GetName().Version;
        }

        public static void Initialize()
        {
            // Make sure the type is initialized.
            Instance.GetType();
        }

      
        public Version Version { get; }
    
        public T? GetCompilerService<T>() where T : class => typeof(T) == typeof(IProjectDesignTimeEntryPoint) ? (T) (object) this : null;

        public event Action<IDesignTimeEntryPoint> Unloaded;

        public bool TryGetTextSpanClassifier( SemanticModel semanticModel, SyntaxNode root, out ITextSpanClassifier classifier )
        {
            // TODO: if the root is not "our", return false.
            
            var diagnostics = new List<Diagnostic>();
            var templateCompiler = new TemplateCompiler();


            templateCompiler.TryAnnotate( semanticModel.SyntaxTree.GetRoot( ), semanticModel, diagnostics, out var annotatedSyntaxRoot );

            if ( annotatedSyntaxRoot != null )
            {
              
                var text = semanticModel.SyntaxTree.GetText( );
                CompileTimeTextSpanMarker marker = new CompileTimeTextSpanMarker( text );
                marker.Visit( annotatedSyntaxRoot );
                classifier = marker.Classifier;
            }
            else
            {
                classifier = null;
                return false;
            }

            return true;
        }
        
    }
}