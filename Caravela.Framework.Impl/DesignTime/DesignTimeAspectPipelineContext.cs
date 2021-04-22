// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class DesignTimeAspectPipelineContext : IAspectPipelineContext
    {
        public DesignTimeAspectPipelineContext(
            CSharpCompilation compilation,
            IBuildOptions buildOptions,
            CancellationToken cancellationToken )
        {
            this.BuildOptions = buildOptions;
            this.Compilation = compilation;
            this.CancellationToken = cancellationToken;
        }

        public CSharpCompilation Compilation { get; }

        public ImmutableArray<object> Plugins => ImmutableArray<object>.Empty;

        public IList<ResourceDescription> ManifestResources => ImmutableArray<ResourceDescription>.Empty;

        public IBuildOptions BuildOptions { get; }

        public CancellationToken CancellationToken { get; }

        public bool HandleExceptions => true;
    }
}