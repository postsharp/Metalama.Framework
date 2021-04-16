// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class DesignTimeAspectPipelineContext : IAspectPipelineContext
    {
        private readonly Action<Diagnostic>? _reportDiagnostic;

        public DesignTimeAspectPipelineContext(
            CSharpCompilation compilation,
            IBuildOptions buildOptions,
            Action<Diagnostic>? reportDiagnostic,
            CancellationToken cancellationToken )
        {
            this._reportDiagnostic = reportDiagnostic;
            this.BuildOptions = buildOptions;
            this.Compilation = compilation;
            this.CancellationToken = cancellationToken;
        }

        public CSharpCompilation Compilation { get; }

        public ImmutableArray<object> Plugins => ImmutableArray<object>.Empty;

        public IList<ResourceDescription> ManifestResources => ImmutableArray<ResourceDescription>.Empty;

        public IBuildOptions BuildOptions { get; }

        public CancellationToken CancellationToken { get; }

        public void ReportDiagnostic( Diagnostic diagnostic ) => this._reportDiagnostic?.Invoke( diagnostic );

        public bool HandleExceptions => true;
    }
}