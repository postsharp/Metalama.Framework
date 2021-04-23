// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    internal sealed partial class AspectPipelineTransformer
    {
        private class AspectPipelineContext : IAspectPipelineContext, IDiagnosticAdder
        {
            private readonly TransformerContext _transformerContext;

            public AspectPipelineContext( TransformerContext transformerContext )
            {
                this._transformerContext = transformerContext;
                this.BuildOptions = new BuildOptions( new AnalyzerBuildOptionsSource( this._transformerContext.GlobalOptions ) );
            }

            public ImmutableArray<object> Plugins => this._transformerContext.Plugins;

            public IBuildOptions BuildOptions { get; }

            public void ReportDiagnostic( Diagnostic diagnostic ) => this._transformerContext.ReportDiagnostic( diagnostic );

            public bool HandleExceptions => true;
        }
    }
}