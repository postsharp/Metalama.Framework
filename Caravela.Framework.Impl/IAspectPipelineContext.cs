using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    public interface IAspectPipelineContext
    {
        Compilation Compilation { get; }

        ImmutableArray<object> Plugins { get; }

        IList<ResourceDescription> ManifestResources { get; }

        bool GetOptionsFlag( string flagName );

        void ReportDiagnostic( Diagnostic diagnostic );
    }
}
