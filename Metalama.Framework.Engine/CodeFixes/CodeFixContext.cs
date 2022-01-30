// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// Exposes objects required by <see cref="CodeFixBuilder"/>.
    /// </summary>
    internal class CodeFixContext
    {
        public PartialCompilation OriginalCompilation { get; }

        public ServiceProvider ServiceProvider => this.PipelineConfiguration.ServiceProvider;

        public AspectPipelineConfiguration PipelineConfiguration { get; }

        public CodeFixContext(
            PartialCompilation originalCompilation,
            AspectPipelineConfiguration pipelineConfiguration )
        {
            this.OriginalCompilation = originalCompilation;
            this.PipelineConfiguration = pipelineConfiguration;
        }
    }
}