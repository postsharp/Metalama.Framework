// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

        // TODO: When called from a diagnostic suppressor, we don't have a way to report diagnostics.
        void ReportDiagnostic( Diagnostic diagnostic );

        bool HandleExceptions { get; }
    }
}
