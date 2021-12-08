// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.DesignTime.Pipeline;
using Metalama.Framework.Impl.Options;
using Metalama.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Impl.DesignTime.CodeFixes
{
    /// <summary>
    /// Exposes objects required by <see cref="CodeFixBuilder"/>.
    /// </summary>
    internal class CodeFixContext
    {
        public Document OriginalDocument { get; }

        public IServiceProvider ServiceProvider => this.PipelineConfiguration.ServiceProvider;

        public DesignTimeAspectPipelineFactory PipelineFactory { get; }

        public IProjectOptions ProjectOptions { get; }

        public AspectPipelineConfiguration PipelineConfiguration { get; }

        public CodeFixContext(
            Document originalDocument,
            DesignTimeAspectPipelineFactory pipelineFactory,
            IProjectOptions projectOptions,
            AspectPipelineConfiguration pipelineConfiguration )
        {
            this.OriginalDocument = originalDocument;
            this.PipelineFactory = pipelineFactory;
            this.ProjectOptions = projectOptions;
            this.PipelineConfiguration = pipelineConfiguration;
        }
    }
}