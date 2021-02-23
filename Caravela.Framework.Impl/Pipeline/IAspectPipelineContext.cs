using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The context in which an <see cref="AspectPipeline"/> is executed. Gives information about the outside.
    /// </summary>
    public interface IAspectPipelineContext
    {
        CancellationToken CancellationToken { get; }

        CSharpCompilation Compilation { get; }

        ImmutableArray<object> Plugins { get; }

        IList<ResourceDescription> ManifestResources { get; }

        IBuildOptions BuildOptions { get; }

        void ReportDiagnostic( Diagnostic diagnostic );

        bool HandleExceptions { get; }
    }
}
