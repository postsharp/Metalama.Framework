using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Pipeline
{
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
