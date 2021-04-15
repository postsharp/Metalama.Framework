// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    internal sealed partial class AspectPipelineTransformer
    {
        private class AspectPipelineContext : IAspectPipelineContext
        {
            private readonly TransformerContext _transformerContext;

            public AspectPipelineContext( TransformerContext transformerContext )
            {
                this._transformerContext = transformerContext;
                this.BuildOptions = new BuildOptions( new AnalyzerBuildOptionsSource( this._transformerContext.GlobalOptions ) );
            }

            public CSharpCompilation Compilation => (CSharpCompilation) this._transformerContext.Compilation;

            public ImmutableArray<object> Plugins => this._transformerContext.Plugins;

            public IList<ResourceDescription> ManifestResources => this._transformerContext.ManifestResources;

            public IBuildOptions BuildOptions { get; }

            public CancellationToken CancellationToken => CancellationToken.None;

            public void ReportDiagnostic( Diagnostic diagnostic ) => this._transformerContext.ReportDiagnostic( diagnostic );

            public bool HandleExceptions => true;
        }
    }
}