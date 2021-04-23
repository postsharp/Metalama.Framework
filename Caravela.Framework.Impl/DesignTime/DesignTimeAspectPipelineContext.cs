// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class DesignTimeAspectPipelineContext : IAspectPipelineContext
    {
        public DesignTimeAspectPipelineContext(
            IBuildOptions buildOptions )
        {
            this.BuildOptions = buildOptions;
        }

        public ImmutableArray<object> Plugins => ImmutableArray<object>.Empty;

        public IBuildOptions BuildOptions { get; }

        public bool HandleExceptions => true;
    }
}