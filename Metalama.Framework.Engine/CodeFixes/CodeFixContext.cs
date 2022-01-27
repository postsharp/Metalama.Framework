// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// Exposes objects required by <see cref="CodeFixBuilder"/>.
    /// </summary>
    internal class CodeFixContext
    {
        public Document OriginalDocument { get; }

        public ServiceProvider ServiceProvider => this.PipelineConfiguration.ServiceProvider;

        public IProjectOptions ProjectOptions { get; }

        public AspectPipelineConfiguration PipelineConfiguration { get; }

        public CodeFixContext(
            Document originalDocument,
            IProjectOptions projectOptions,
            AspectPipelineConfiguration pipelineConfiguration )
        {
            this.OriginalDocument = originalDocument;
            this.ProjectOptions = projectOptions;
            this.PipelineConfiguration = pipelineConfiguration;
        }
    }
}