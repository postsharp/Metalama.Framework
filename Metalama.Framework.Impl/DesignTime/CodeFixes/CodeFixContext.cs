// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
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