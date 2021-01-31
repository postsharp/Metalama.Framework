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
            DesignTimeEntryPoints.RegisterEntryPoint( ProjectDesignTimeEntryPoint.Instance );
        }

        public static void Initialize()
        {
            // Make sure the type is initialized.
            Instance.GetType();
        }

        public bool HasTemplateHighlightingUpdatedClient => this.TemplateHighlightingUpdated != null;

        internal void SignalTemplateHighlightingUpdated( TemplateHighlightingInfo info )
        {
            this.TemplateHighlightingUpdated?.Invoke( info );
        }

        public event Action<TemplateHighlightingInfo> TemplateHighlightingUpdated;


        public bool HandlesProject( Microsoft.CodeAnalysis.Project project )
        {
            // TODO: test version of references.
            return project.MetadataReferences.Any( r => r switch
                {
                    CompilationReference cr => string.Equals( cr.Compilation.AssemblyName, "Caravela.Framework", StringComparison.OrdinalIgnoreCase ),
                    PortableExecutableReference per => string.Equals( Path.GetFileNameWithoutExtension( per.FilePath ), "Caravela.Framework", StringComparison.OrdinalIgnoreCase ),
                    _ => false
                }
                               ) ||
                       project.ProjectReferences.Any( r => string.Equals( project.Solution.GetProject( r.ProjectId )!.AssemblyName, "Caravela.Framework", StringComparison.OrdinalIgnoreCase ) );
        }

        public T? GetService<T>() where T : class => typeof(T) == typeof(IProjectDesignTimeEntryPoint) ? (T) (object) this : null;


        public event Action<IDesignTimeEntryPoint> Disposed;

        public bool TryProvideClassifiedSpans( SemanticModel semanticModel, SyntaxNode root, out ITextSpanClassifier classifier )
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